/*
 * Copyright © 2021 robbyxp1 @ github.com & EDDiscovery Team
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
    public abstract class SQLAdvProcessingThread<ConnectionType> where ConnectionType : IDisposable
    {
        #region Public control 
        public int Threads { get { return runningThreads; } }
        public int MaxThreads { get; set; } = 10;                      // maximum to create when MultiThreaded = true, 1 or more
        public int MinThreads { get; set; } = 3;                       // maximum to create when MultiThreaded = true, 1 or more

        public string Name { get; set; } = "SQLAdvProcessingThread";                            // thread name

        public bool MultiThreaded { get { return multithreaded; } set { SetMultithreaded(value); } }    // default is not

        protected abstract ConnectionType CreateConnection();   // override in derived class to make the connection

        // Execute SQL with the database in a thread.  Must indicate direction by name

        public T DBRead<T>(Func<ConnectionType, T> func, int warnthreshold = 500, string jobname = "")
        {
            return Execute(() => func.Invoke(connection.Value), false, 1, warnthreshold, jobname);
        }

        public void DBRead(Action<ConnectionType> action, int warnthreshold = 500, string jobname = "")
        {
            Execute<object>(() => { action.Invoke(connection.Value); return null; }, false, 1, warnthreshold, jobname);
        }

        public void DBWrite(Action<ConnectionType> action, int warnthreshold = 500, string jobname = "")
        {
            Execute<object>(() => { action.Invoke(connection.Value); return null; }, true, 1, warnthreshold, jobname);
        }

        public T DBWrite<T>(Func<ConnectionType, T> func, int warnthreshold = 500, string jobname = "")
        {
            return Execute(() => func.Invoke(connection.Value), true, 1, warnthreshold, jobname);
        }

        public void Stop()
        {
            StopAllThreads();
        }

        #endregion

        #region Privates

        protected ThreadLocal<ConnectionType> connection = new ThreadLocal<ConnectionType>(true);       // connection per thread

        private ConcurrentQueue<Job> jobQueue = new ConcurrentQueue<Job>();
        private AutoResetEvent jobQueuedEvent = new AutoResetEvent(false);      // first thread waiting resets it back

        private int createdThreads = 0;                 // used to track how many threads we asked for - will be different to RunningThread due to creation delay
        private int runningThreadsAvailable = 0;        // incremented when thread created runs, decremented when closed and during job execution
        private int runningThreads = 0;                 // incremented in thread when its runs, decremented when it exits

        private ManualResetEvent stopRequestedEvent = new ManualResetEvent(false);      // manual reset, multiple threads can be waiting on this one
        private ManualResetEvent stoppedAllThreads = new ManualResetEvent(true);        // Set to true as there are no running ones, cleared on thread start

        private ReaderWriterLock rwLock = new ReaderWriterLock();       // used to prevent writes when readers are running in MT scenarios

        private bool multithreaded = false;             // if MT
        private bool stopCreatingNewThreads = false;    // halt thread creation during stop

        private object locker = new object();  // used to lock the MT change

        private int checkRWLock = 0;        // used to double check reader/writer lock for now

        #endregion

        #region Processing Thread
        private void SqlThreadProc()    // SQL process thread
        {
            int recursiondepth = 0;

            Interlocked.Increment(ref runningThreadsAvailable);
            Interlocked.Increment(ref runningThreads);
            stoppedAllThreads.Reset();

            try
            {
                System.Diagnostics.Debug.WriteLine($"Start SQL thread {Thread.CurrentThread.Name}");

                using (connection.Value = CreateConnection())   // hold connection over whole period.
                {
                    while (true)
                    {
                        // multiple threads can be waiting on this.. 

                        switch (WaitHandle.WaitAny(new WaitHandle[] { stopRequestedEvent, jobQueuedEvent }))    // wait for event
                        {
                            case 1:     // JobQueuedEvent
                                try
                                {
                                    Interlocked.Decrement(ref runningThreadsAvailable);        // one less thread ready for use
                                    System.Diagnostics.Debug.WriteLine($"Thread state ta {runningThreadsAvailable} rt {runningThreads} mt {MultiThreaded} stop {stopCreatingNewThreads}");

                                    while (jobQueue.Count != 0 )
                                    {
                                        if ( stopRequestedEvent.WaitOne(0))             // if signalled a stop, break the loop   
                                            break;

                                        while (jobQueue.TryDequeue(out Job job))                    // and get the job
                                        {
                                            System.Diagnostics.Debug.Assert(recursiondepth++ == 0); // we must not have a call to Job.Exec() calling back. Should never happen but check
                                            
                                            if ( !MultiThreaded )       // if not multithreaded mode, we can just execute
                                            {
                                                System.Diagnostics.Debug.WriteLine($"On thread {Thread.CurrentThread.Name} execute job from {job.jobname} write {job.write}");
                                                job.Exec();
                                            }
                                            else if (job.write)
                                            {
                                                try
                                                {

                                                    rwLock.AcquireWriterLock(1000000);
                                                    int active = Interlocked.Increment(ref checkRWLock);
                                                    System.Diagnostics.Debug.Assert(active == 1);
                                                    System.Diagnostics.Debug.WriteLine($"On thread {Thread.CurrentThread.Name} execute write job from {job.jobname} active {active}");
                                                    job.Exec();
                                                    active = Interlocked.Decrement(ref checkRWLock);
                                                    System.Diagnostics.Debug.WriteLine($"On thread {Thread.CurrentThread.Name} finish write job from {job.jobname} active {active}");
                                                    rwLock.ReleaseWriterLock();
                                                }
                                                catch
                                                {
                                                    System.Diagnostics.Debug.WriteLine($"On thread {Thread.CurrentThread.Name} failed to get writer lock");
                                                    job.Exec();
                                                }
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    rwLock.AcquireReaderLock(1000000);
                                                    int active = Interlocked.Increment(ref checkRWLock);
                                                    System.Diagnostics.Debug.WriteLine($"On thread {Thread.CurrentThread.Name} execute mt job from {job.jobname} active {active}");
                                                    job.Exec();
                                                    active = Interlocked.Decrement(ref checkRWLock);
                                                    System.Diagnostics.Debug.WriteLine($"On thread {Thread.CurrentThread.Name} finish mt job from {job.jobname} active {active}");
                                                    rwLock.ReleaseReaderLock();
                                                }
                                                catch
                                                {
                                                    System.Diagnostics.Debug.WriteLine($"On thread {Thread.CurrentThread.Name} failed to get readers lock");
                                                    job.Exec();
                                                }

                                            }
                                            //System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Execute Job");
                                            recursiondepth--;
                                        }
                                    }
                                }
                                finally
                                {
                                    Interlocked.Increment(ref runningThreadsAvailable);
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
                System.Diagnostics.Debug.WriteLine($"Stop SQL thread {Thread.CurrentThread.Name}");

                Interlocked.Decrement(ref runningThreadsAvailable);            // stopping threads.. decr count, if 0, say all stopped

                if (Interlocked.Decrement(ref runningThreads) == 0)
                {
                    stoppedAllThreads.Set();
                    System.Diagnostics.Debug.WriteLine($"All threads stopped");
                }
            }
        }

        #endregion

        #region Execute 

        protected T Execute<T>(Func<T> func, bool write, int skipframes, int warnthreshold, string jobname)  // in caller thread, queue to job queue, wait for complete
        {
            using (var job = new Job<T>(func, write, skipframes, warnthreshold, Thread.CurrentThread.Name + jobname))       // make a new job
            {
                if (Thread.CurrentThread.Name != null && Thread.CurrentThread.Name.StartsWith(Name))            // we should not be calling this from a thread made by us
                { 
                    System.Diagnostics.Trace.WriteLine($"{typeof(ConnectionType).Name} Database Re-entrancy\n{new System.Diagnostics.StackTrace(skipframes, true).ToString()}");
                    job.Exec();
                    return job.Wait();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Submitting job write {write}, ta {runningThreadsAvailable} rt {runningThreads} mt {MultiThreaded} ct {createdThreads} stop {stopCreatingNewThreads}");

                    if (!stopCreatingNewThreads)   // if we can create new threads..
                    {
                        // if no threads, or MT and read and none available (no point creating new threads for write since they will be sequenced by the RWLock)
                        if (runningThreads == 0 || (MultiThreaded && !write && runningThreadsAvailable == 0)) 
                        {
                            TryStartThread();      // try and start another thread to service the request
                        }
                    }

                    jobQueue.Enqueue(job);
                    jobQueuedEvent.Set();  // kick one of the threads and execute it.
                    return job.Wait();     // must be infinite - can't release the caller thread until the job finished. try it with 10ms for instance, route finder just fails.
                }
            }
        }

        protected void Execute(Action action, bool write, int skipframes, int warnthreshold, string jobname )
        {
            Execute<object>(() => { action(); return null; }, write, skipframes + 1, warnthreshold, jobname);
        }

        private void SetMultithreaded(bool mt)
        {
            lock (locker)
            {
                StopAllThreads();       // stop everything
                multithreaded = mt;     // set state
                for (int i = 0; i < (multithreaded ? MinThreads : 1); i++)        // 1 thread for non MT, else MinThreads
                    TryStartThread();          // set up N threads
            }
        }

        private void TryStartThread()                   // add another thread to the pool, as long as we don't exceed the limit
        {
            int tno = Interlocked.Increment(ref createdThreads);
            if (tno <= MaxThreads)                      // if not exceeded, we can make one
            {
                var thread = new Thread(SqlThreadProc);
                thread.Name = $"{Name}-" + tno;
                thread.IsBackground = true;
                System.Diagnostics.Debug.WriteLine($"**************** Create Thread {thread.Name} ta {runningThreadsAvailable} rt {runningThreads} mt {MultiThreaded}");
                thread.Start();
            }
            else
            {
                Interlocked.Decrement(ref createdThreads);      // need to decrease it back
                System.Diagnostics.Debug.WriteLine($"**************** Reject Thread Creation ta {runningThreadsAvailable} rt {runningThreads} tno was {tno}");
            }
        }

        private void StopAllThreads()
        {
            stopCreatingNewThreads = true;                // just stop the threads creating new readers

            stopRequestedEvent.Set();                       // stop the threads - all of them
            Interlocked.MemoryBarrier();                    // ??
            stoppedAllThreads.WaitOne();                    // until the last one indicates its finished

            System.Diagnostics.Debug.Assert(runningThreadsAvailable == 0 && runningThreads == 0);  // ensure the counters are right
            System.Diagnostics.Debug.WriteLine("All threads indicated stopped");

            createdThreads = 0;
            stoppedAllThreads = new ManualResetEvent(true);   // all threads are stopped
            stopRequestedEvent = new ManualResetEvent(false);

            stopCreatingNewThreads = false;
        }

        #endregion
    }

    internal interface Job
    {
        void Exec();
        string jobname { get; set; }
        bool write { get; set; }
    }
    internal class Job<T> : Job, IDisposable
    {
        private Func<T> Func;           // this is the code to call to execute the job
        private T result;               // passed back result of the job
        public string jobname { get; set; }
        public bool write { get; set; }

        private ManualResetEventSlim waithandle;    // set when job finished
        private string stackTrace;
        private int warnThreshold;
        private Exception exception;
        private System.Diagnostics.Stopwatch stopwatch;

        public Job(Func<T> func, bool write, int skipframes, int warnthreshold, string jobname)       // in calller thread, set the job up
        {
            this.Func = func;
            this.write = write;
            this.stackTrace = new System.Diagnostics.StackTrace(skipframes + 1, true).ToString();       // stack trace of caller thread
            this.warnThreshold = warnthreshold;
            this.jobname = jobname;
            this.waithandle = new ManualResetEventSlim(false);
            this.stopwatch = System.Diagnostics.Stopwatch.StartNew();
        }

        public void Exec()     // in SQL thread, do the job
        {
            if (stopwatch.ElapsedMilliseconds > warnThreshold)
            {
                System.Diagnostics.Trace.WriteLine($"Job delayed for {stopwatch.ElapsedMilliseconds}ms");
            }
            stopwatch.Reset();

            try
            {
                result = Func.Invoke();
            }
            catch (Exception ex)
            {
                this.exception = ex;
            }
            finally
            {
                if (stopwatch.ElapsedMilliseconds > warnThreshold)
                {
                    System.Diagnostics.Trace.WriteLine($"Database connection held for {stopwatch.ElapsedMilliseconds}ms\n{stackTrace}");
                }

                waithandle.Set();
            }
        }

        public T Wait()     // in caller thread, wait for the job to complete.
        {
            waithandle.Wait();

            if (exception != null)
            {
                throw new SQLProcessingThreadException(exception);
            }
            else
            {
                return result;
            }
        }

        public void Dispose()
        {
            this.waithandle?.Dispose();
        }
    }


}
