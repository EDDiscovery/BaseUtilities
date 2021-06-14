/*
 * Copyright © 2019-2021 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SQLLiteExtensions
{
    public class SQLProcessingThreadException : Exception
    {
        public SQLProcessingThreadException(Exception innerexception) : base(innerexception.Message, innerexception)
        {
        }
    }

    public abstract class SQLProcessingThread<ConnectionType> where ConnectionType: IDisposable
    {
        private abstract class Job
        {
            public abstract void Exec();
        }

        private class Job<T> : Job, IDisposable
        {
            private ManualResetEventSlim WaitHandle;
            private Func<T> Func;
            private string StackTrace;
            private int WarnThreshold;
            private Exception Exception;
            private T Result;
            private System.Diagnostics.Stopwatch Stopwatch;

            public Job(Func<T> func, int skipframes, int warnthreshold)
            {
                this.Func = func;
                this.WaitHandle = new ManualResetEventSlim(false);
                this.StackTrace = new System.Diagnostics.StackTrace(skipframes + 1, true).ToString();
                this.WarnThreshold = warnthreshold;
                this.Stopwatch = System.Diagnostics.Stopwatch.StartNew();
            }

            public override void Exec()
            {
                if (Stopwatch.ElapsedMilliseconds > WarnThreshold)
                {
                    System.Diagnostics.Trace.WriteLine($"{typeof(ConnectionType).Name} Job delayed for {Stopwatch.ElapsedMilliseconds}ms");
                }
                Stopwatch.Reset();

                try
                {
                    Result = Func.Invoke();
                }
                catch (Exception ex)
                {
                    this.Exception = ex;
                }
                finally
                {
                    if (Stopwatch.ElapsedMilliseconds > WarnThreshold)
                    {
                        System.Diagnostics.Trace.WriteLine($"{typeof(ConnectionType).Name} Database connection held for {Stopwatch.ElapsedMilliseconds}ms\n{StackTrace}");
                    }

                    WaitHandle.Set();
                }
            }

            public T Wait()
            {
                WaitHandle.Wait();

                if (Exception != null)
                {
                    throw new SQLProcessingThreadException(Exception);
                }
                else
                {
                    return Result;
                }
            }

            public void Dispose()
            {
                this.WaitHandle?.Dispose();
            }
        }

        private ConcurrentQueue<Job> JobQueue = new ConcurrentQueue<Job>();

        protected bool ReadOnly { get; private set; } = false;      // master control to indicate read or read/write mode

        private Thread SqlReadWriteThread;              // used when in read/write mode
        private string SqlReadWriteThreadName;

        private ConcurrentBag<Thread> ReadOnlyThreads = new ConcurrentBag<Thread>();        // the readonly threads bag

        private bool StopRequested = false;             // used by SQLThreadProc
        private bool SwitchToReadOnlyRequested = false;
        private int ThreadsAvailable = 0;
        private int RunningThreads = 0;
        private ManualResetEvent StopRequestedEvent = new ManualResetEvent(false);      // manual reset, multiple threads can be waiting on this one
        private ManualResetEvent StoppedAllThreads = new ManualResetEvent(true);        // set when all threads (indicated by threadsavailable) have stopped
        private AutoResetEvent JobQueuedEvent = new AutoResetEvent(false);      // first thread waiting resets it back

        private ReaderWriterLock RWLock = new ReaderWriterLock();

        private int WriterCount = 0;

        public long? SqlThreadId => SqlReadWriteThread?.ManagedThreadId;

        protected ThreadLocal<ConnectionType> Connection = new ThreadLocal<ConnectionType>(true);

        protected abstract ConnectionType CreateConnection();

        private void SqlThreadProc()    // SQL process thread
        {
            int recursiondepth = 0;

            if (StopRequested)
            {
                return;
            }

            try
            {
                Interlocked.Increment(ref ThreadsAvailable);
                Interlocked.Increment(ref RunningThreads);
                StoppedAllThreads.Reset();

                using (Connection.Value = CreateConnection())   // hold connection over whole period.
                {
                    while (!StopRequested && !SwitchToReadOnlyRequested)        // while can continue
                    {
                        // multiple threads can be waiting on this.. 

                        switch (WaitHandle.WaitAny(new WaitHandle[] { StopRequestedEvent, JobQueuedEvent }))    // wait for event
                        {
                            case 1:     // JobQueuedEvent
                                bool ro = ReadOnly;     // ?cache Readonly - why?
                                bool gotlock = false;       // have we grabbed the lock

                                try
                                {
                                    Interlocked.Decrement(ref ThreadsAvailable);        // one less thread ready for use

                                    while (JobQueue.Count != 0 && !StopRequested && !SwitchToReadOnlyRequested) // if queued, not stopped, and not switching, take the job
                                    {
                                        try
                                        {                                                           // get the lock
                                            if (ro)
                                            {
                                                RWLock.AcquireReaderLock(1000);                 
                                            }
                                            else
                                            {
                                                RWLock.AcquireWriterLock(1000);
                                            }

                                            gotlock = true;
                                        }
                                        catch (ApplicationException)
                                        {
                                            continue;
                                        }

                                        while (JobQueue.TryDequeue(out Job job))                    // and get the job
                                        {
                                            System.Diagnostics.Debug.Assert(recursiondepth++ == 0); // we must not have a call to Job.Exec() calling back. Should never happen but check
                                                                                                    //System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Execute Job");
                                            job.Exec();
                                            recursiondepth--;
                                        }
                                    }
                                }
                                finally
                                {
                                    Interlocked.Increment(ref ThreadsAvailable);                

                                    if (gotlock)
                                    {
                                        if (ro)
                                        {
                                            RWLock.ReleaseReaderLock();
                                        }
                                        else
                                        {
                                            RWLock.ReleaseWriterLock();
                                        }
                                    }
                                }
                                break;

                            case 0:     // stoprequested event.. go to finally
                                return;
                        }
                    }
                }
            }
            finally
            {
                Interlocked.Decrement(ref ThreadsAvailable);            // stopping threads.. decr count, if 0, say all stopped
                if (Interlocked.Decrement(ref RunningThreads) == 0)
                {
                    StoppedAllThreads.Set();
                }
            }
        }

        protected T Execute<T>(Func<T> func, int skipframes = 1, int warnthreshold = 500)  // in caller thread, queue to job queue, wait for complete
        {
            if (StopRequested && StoppedAllThreads.WaitOne(0))
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            using (var job = new Job<T>(func, skipframes, warnthreshold))       // make a new job
            {
                if (Thread.CurrentThread.ManagedThreadId == SqlReadWriteThread?.ManagedThreadId)     // if current thread is the SQL Job thread, uh-oh
                {
                    System.Diagnostics.Trace.WriteLine($"{typeof(ConnectionType).Name} Database Re-entrancy\n{new System.Diagnostics.StackTrace(skipframes, true).ToString()}");
                    job.Exec();
                    return job.Wait();
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Queue Job " + Thread.CurrentThread.Name);
                    JobQueue.Enqueue(job);
                    JobQueuedEvent.Set();           // kick one of the threads and execute it.

                    if (ThreadsAvailable == 0 && ReadOnly && !SwitchToReadOnlyRequested)        // if no threads are available, and we are in readonly, and not switching
                    {
                        StartReadonlyThread();      // start another thread to service the request
                    }

                    return job.Wait();     // must be infinite - can't release the caller thread until the job finished. try it with 10ms for instance, route finder just fails.
                    //System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Job finished " + sw.ElapsedMilliseconds + " " + Thread.CurrentThread.Name);
                }
            }
        }

        protected void Execute(Action action, int skipframes = 1, int warnthreshold = 500)
        {
            Execute<object>(() => { action(); return null; }, skipframes + 1, warnthreshold);
        }

        public void ExecuteWithDatabase(Action<ConnectionType> action, int warnthreshold = 500)
        {
            Execute<object>(() => { action.Invoke(Connection.Value); return null; }, warnthreshold: warnthreshold);
        }

        public T ExecuteWithDatabase<T>(Func<ConnectionType, T> func, int warnthreshold = 500)
        {
            return Execute(() => func.Invoke(Connection.Value), warnthreshold: warnthreshold);
        }

        // start up the system
        public void Start(string threadname)
        {
            StopRequested = false;
            StopRequestedEvent.Reset();
            StoppedAllThreads.Reset();

            SqlReadWriteThreadName = threadname;

            if (SqlReadWriteThread == null && ReadOnly == false)        // if we are in readwrite mode, start the single thread this mode supports
            {                                                           // if we are in read mode, a thread gets spawned on the first request
                StartReadWriteThread();
            }
        }

        private void StartReadWriteThread()
        {
            SqlReadWriteThread = new Thread(SqlThreadProc);
            SqlReadWriteThread.Name = SqlReadWriteThreadName;
            SqlReadWriteThread.IsBackground = true;
            System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + $"Start {typeof(ConnectionType).Name} SQL Thread " + SqlReadWriteThreadName);
            SqlReadWriteThread.Start();
        }

        private void SetReadWriteInternal()
        {
            SwitchToReadOnlyRequested = true;
            StopRequestedEvent.Set();                       // stop the threads - all of them
            Interlocked.MemoryBarrier();
            RWLock.AcquireWriterLock(Timeout.Infinite);
            StoppedAllThreads.WaitOne();                    // until the last one indicates its finished

            ReadOnly = false;
            RWLock.ReleaseWriterLock();

            StoppedAllThreads = new ManualResetEvent(true);     // reset them
            StopRequestedEvent = new ManualResetEvent(false);

            while (ReadOnlyThreads.TryTake(out var thread)) ;   // clean out the read bag

            SwitchToReadOnlyRequested = false;

            StartReadWriteThread();                         // start the single read write thread
        }

        private void StartReadonlyThread()                  // add another read only thread to the pool
        {
            var thread = new Thread(SqlThreadProc);
            thread.Name = $"{SqlReadWriteThreadName} RO {ReadOnlyThreads.Count}";
            thread.IsBackground = true;
            System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + $"Start {typeof(ConnectionType).Name} SQL Thread {SqlReadWriteThreadName} RO {ReadOnlyThreads.Count}");
            thread.Start();
            ReadOnlyThreads.Add(thread);
        }

        private void SetReadOnlyInternal()
        {
            if (!StopRequested)
            {
                SwitchToReadOnlyRequested = true;               // indicate switch
                Interlocked.MemoryBarrier();
                RWLock.AcquireWriterLock(Timeout.Infinite);

                if (SqlReadWriteThread != null)                 // if sqlreadwrite thread running
                {
                    System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + $"Stop {typeof(ConnectionType).Name} SQL Thread " + SqlReadWriteThreadName);
                    StopRequestedEvent.Set();
                    StoppedAllThreads.WaitOne();
                    System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + $"Stopped {typeof(ConnectionType).Name} SQL " + SqlReadWriteThreadName);
                    SqlReadWriteThread = null;
                }

                ReadOnly = true;
                SwitchToReadOnlyRequested = false;

                StoppedAllThreads = new ManualResetEvent(true);     // reset them
                StopRequestedEvent = new ManualResetEvent(false);  
                StopRequestedEvent.Reset();                     // ???

                RWLock.ReleaseWriterLock();

                StartReadonlyThread();
            }
        }

        public void SetReadWrite()
        {
            var stopcompleted = StoppedAllThreads;

            if (Interlocked.Increment(ref WriterCount) == 1)
            {
                SetReadWriteInternal();
            }
            else if (ReadOnly)
            {
                stopcompleted.WaitOne();
            }
        }

        public void SetReadOnly()
        {
            int count = Interlocked.Decrement(ref WriterCount);

            if (count < 0)
            {
                count = Interlocked.Increment(ref WriterCount);
            }

            if (count == 0 && !ReadOnly)
            {
                SetReadOnlyInternal();
            }
        }

        public T WithReadWrite<T>(Func<T> action)
        {
            SetReadWrite();

            try
            {
                return action();
            }
            finally
            {
                SetReadOnly();
            }
        }

        public void WithReadWrite(Action action)
        {
            WithReadWrite<object>(() => { action(); return null; });
        }

        public void Stop()
        {
            RWLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                if (SqlReadWriteThread != null || ReadOnlyThreads.Count != 0)
                {
                    System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + $"Stop {typeof(ConnectionType).Name} SQL Thread " + SqlReadWriteThreadName);
                    StopRequested = true;
                    StopRequestedEvent.Set();
                    StoppedAllThreads.WaitOne();
                    System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + $"Stopped {typeof(ConnectionType).Name} SQL " + SqlReadWriteThreadName);
                    SqlReadWriteThread = null;
                }
            }
            finally
            {
                RWLock.ReleaseWriterLock();
            }
        }
    }

}
