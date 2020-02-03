/*
 * Copyright © 2019 EDDiscovery development team
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
using System.Collections.Generic;
using System.Linq;
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
        private Thread SqlThread;
        private ManualResetEvent StopRequestedEvent = new ManualResetEvent(false);
        private bool StopRequested = false;
        private bool SwitchToReadOnlyRequested = false;
        private AutoResetEvent JobQueuedEvent = new AutoResetEvent(false);
        private ManualResetEvent StopCompleted = new ManualResetEvent(true);
        private ReaderWriterLock ReadOnlyLock = new ReaderWriterLock();
        protected bool ReadOnly { get; private set; } = false;
        private string SqlThreadName;
        private ConcurrentBag<Thread> ReadOnlyThreads = new ConcurrentBag<Thread>();
        private int ThreadsAvailable = 0;
        private int RunningThreads = 0;

        public long? SqlThreadId => SqlThread?.ManagedThreadId;

        protected ThreadLocal<ConnectionType> Connection = new ThreadLocal<ConnectionType>(true);

        protected abstract ConnectionType CreateConnection();

        private void SqlThreadProc()    // SQL process thread
        {
            int recursiondepth = 0;
            int threadnum = ReadOnlyThreads.Count;

            if (StopRequested)
            {
                return;
            }

            try
            {
                Interlocked.Increment(ref ThreadsAvailable);
                Interlocked.Increment(ref RunningThreads);
                StopCompleted.Reset();

                using (Connection.Value = CreateConnection())   // hold connection over whole period.
                {
                    while (!StopRequested && !SwitchToReadOnlyRequested)
                    {
                        switch (WaitHandle.WaitAny(new WaitHandle[] { StopRequestedEvent, JobQueuedEvent }, threadnum != 0 ? 10000 : Timeout.Infinite))
                        {
                            case 1:
                                bool ro = ReadOnly;
                                try
                                {
                                    Interlocked.Decrement(ref ThreadsAvailable);

                                    if (ro)
                                    {
                                        ReadOnlyLock.AcquireReaderLock(Timeout.Infinite);
                                    }
                                    else
                                    {
                                        ReadOnlyLock.AcquireWriterLock(Timeout.Infinite);
                                    }

                                    while (JobQueue.TryDequeue(out Job job))
                                    {
                                        System.Diagnostics.Debug.Assert(recursiondepth++ == 0); // we must not have a call to Job.Exec() calling back. Should never happen but check
                                                                                                //System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Execute Job");
                                        job.Exec();
                                        recursiondepth--;
                                    }
                                }
                                finally
                                {
                                    Interlocked.Increment(ref ThreadsAvailable);

                                    if (ro)
                                    {
                                        ReadOnlyLock.ReleaseReaderLock();
                                    }
                                    else
                                    {
                                        ReadOnlyLock.ReleaseWriterLock();
                                    }
                                }
                                break;
                            case 0:
                                return;
                            case WaitHandle.WaitTimeout:
                                if (threadnum != 0)
                                {
                                    return;
                                }
                                break;
                        }
                    }
                }
            }
            finally
            {
                Interlocked.Decrement(ref ThreadsAvailable);
                if (Interlocked.Decrement(ref RunningThreads) == 0)
                {
                    StopCompleted.Set();
                }
            }
        }

        protected T Execute<T>(Func<T> func, int skipframes = 1, int warnthreshold = 500)  // in caller thread, queue to job queue, wait for complete
        {
            if (StopRequested && StopCompleted.WaitOne(0))
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            using (var job = new Job<T>(func, skipframes, warnthreshold))       // make a new job and queue it
            {
                if (Thread.CurrentThread.ManagedThreadId == SqlThread?.ManagedThreadId)     // if current thread is the SQL Job thread, uh-oh
                {
                    System.Diagnostics.Trace.WriteLine($"{typeof(ConnectionType).Name} Database Re-entrancy\n{new System.Diagnostics.StackTrace(skipframes, true).ToString()}");
                    job.Exec();
                    return job.Wait();
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Queue Job " + Thread.CurrentThread.Name);
                    JobQueue.Enqueue(job);
                    JobQueuedEvent.Set();           // kick the thread to execute it.

                    if (ThreadsAvailable == 0 && ReadOnly && !SwitchToReadOnlyRequested)
                    {
                        StartReadonlyThread();
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

        public void Start(string threadname)
        {
            StopRequested = false;
            StopRequestedEvent.Reset();
            StopCompleted.Reset();

            if (SqlThread == null && ReadOnly == false)
            {
                SqlThreadName = threadname;
                SqlThread = new Thread(SqlThreadProc);
                SqlThread.Name = threadname;
                SqlThread.IsBackground = true;
                System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + $"Start {typeof(ConnectionType).Name} SQL Thread " + threadname);
                SqlThread.Start();
            }
        }

        private void StartReadonlyThread()
        {
            var thread = new Thread(SqlThreadProc);
            thread.Name = $"{SqlThreadName} RO {ReadOnlyThreads.Count}";
            thread.IsBackground = true;
            System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + $"Start {typeof(ConnectionType).Name} SQL Thread {SqlThreadName} RO {ReadOnlyThreads.Count}");
            thread.Start();
            ReadOnlyThreads.Add(thread);
        }

        public void SetReadOnly()
        {
            if (!ReadOnly && !StopRequested)
            {
                ReadOnlyLock.AcquireWriterLock(Timeout.Infinite);
                SwitchToReadOnlyRequested = true;

                if (SqlThread != null)
                {
                    System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + $"Stop {typeof(ConnectionType).Name} SQL Thread " + SqlThreadName);
                    StopRequestedEvent.Set();
                    StopCompleted.WaitOne();
                    System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + $"Stopped {typeof(ConnectionType).Name} SQL " + SqlThreadName);
                    SqlThread = null;
                }

                ReadOnly = true;
                SwitchToReadOnlyRequested = false;
                StopRequestedEvent.Reset();
                ReadOnlyLock.ReleaseWriterLock();
                StartReadonlyThread();
            }
        }

        public void SetReadWrite()
        {
            if (ReadOnly)
            {
                ReadOnlyLock.AcquireWriterLock(Timeout.Infinite);
                SwitchToReadOnlyRequested = true;
                StopRequestedEvent.Set();
                StopCompleted.WaitOne();
                ReadOnly = false;
                ReadOnlyLock.ReleaseWriterLock();
                StopRequestedEvent.Reset();

                while (ReadOnlyThreads.TryTake(out var thread));

                SqlThread = new Thread(SqlThreadProc);
                SqlThread.Name = SqlThreadName;
                SqlThread.IsBackground = true;
                System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + $"Start {typeof(ConnectionType).Name} SQL Thread " + SqlThreadName);
                SqlThread.Start();
            }
        }

        public T WithReadWrite<T>(Func<T> action)
        {
            if (ReadOnly)
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
            else
            {
                return action();
            }
        }

        public void WithReadWrite(Action action)
        {
            WithReadWrite<object>(() => { action(); return null; });
        }

        public void Stop()
        {
            ReadOnlyLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                if (SqlThread != null || ReadOnlyThreads.Count != 0)
                {
                    System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + $"Stop {typeof(ConnectionType).Name} SQL Thread " + SqlThreadName);
                    StopRequested = true;
                    StopRequestedEvent.Set();
                    StopCompleted.WaitOne();
                    System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + $"Stopped {typeof(ConnectionType).Name} SQL " + SqlThreadName);
                    SqlThread = null;
                }
            }
            finally
            {
                ReadOnlyLock.ReleaseWriterLock();
            }
        }
    }

}
