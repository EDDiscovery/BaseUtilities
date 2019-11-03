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
using System.Threading;

namespace SQLLiteExtensions
{
    public class SQLProcessingThread<ConnectionType> where ConnectionType: IDisposable, new()
    {
        private class Job : IDisposable
        {
            private ManualResetEventSlim WaitHandle;
            private Action Action;

            public Job(Action action)
            {
                this.Action = action;
                this.WaitHandle = new ManualResetEventSlim(false);
            }

            public void Exec()
            {
                Action.Invoke();
                WaitHandle.Set();
            }

            public void Wait(int timeout = 5000)
            {
                WaitHandle.Wait(timeout);
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
        private AutoResetEvent JobQueuedEvent = new AutoResetEvent(false);
        private ManualResetEvent StopCompleted = new ManualResetEvent(true);

        public long? SqlThreadId => SqlThread?.ManagedThreadId;
        private int recursiondepth = 0;

        protected ConnectionType Connection;

        private void SqlThreadProc()    // SQL process thread
        {
            using (Connection = new ConnectionType())   // hold connection over whole period.
            {
                while (!StopRequested)
                {
                    switch (WaitHandle.WaitAny(new WaitHandle[] { StopRequestedEvent, JobQueuedEvent }))
                    {
                        case 1:
                            while (JobQueue.TryDequeue(out Job job))
                            {
                                System.Diagnostics.Debug.Assert(recursiondepth++ == 0); // we must not have a call to Job.Exec() calling back. Should never happen but check
                                //System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Execute Job");
                                job.Exec();
                                recursiondepth--;
                            }
                            break;
                    }
                }
            }

            StopCompleted.Set();
        }

        protected void Execute(Action action, int skipframes = 1, int warnthreshold = 500)  // in caller thread, queue to job queue, wait for complete
        {
            if (StopCompleted.WaitOne(0))
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();

            if (Thread.CurrentThread.ManagedThreadId == SqlThread?.ManagedThreadId)     // if current thread is the SQL Job thread, uh-oh
            {
                System.Diagnostics.Trace.WriteLine($"{typeof(ConnectionType).Name} Database Re-entrancy\n{new System.Diagnostics.StackTrace(skipframes, true).ToString()}");
                action();
            }
            else
            {
                using (var job = new Job(action))       // make a new job and queue it
                {
                    //System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Queue Job " + Thread.CurrentThread.Name);
                    JobQueue.Enqueue(job);
                    JobQueuedEvent.Set();           // kick the thread to execute it.
                    job.Wait(Timeout.Infinite);     // must be infinite - can't release the caller thread until the job finished. try it with 10ms for instance, route finder just fails.
                    //System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Job finished " + sw.ElapsedMilliseconds + " " + Thread.CurrentThread.Name);
                }
            }

            if (sw.ElapsedMilliseconds > warnthreshold)
            {
                var trace = new System.Diagnostics.StackTrace(skipframes, true);
                System.Diagnostics.Trace.WriteLine($"{typeof(ConnectionType).Name} Database connection held for {sw.ElapsedMilliseconds}ms\n{trace.ToString()}");
            }
        }

        protected T Execute<T>(Func<T> func, int skipframes = 1, int warnthreshold = 500)
        {
            T ret = default(T);
            Execute(() => { ret = func(); }, skipframes + 1, warnthreshold);
            return ret;
        }

        private void ExecuteWithDatabaseInternal(Action<ConnectionType> action)
        {
            action.Invoke(Connection);
        }

        public void ExecuteWithDatabase(Action<ConnectionType> action, int warnthreshold = 500)
        {
            Execute(() => action.Invoke(Connection), warnthreshold: warnthreshold);
        }

        public T ExecuteWithDatabase<T>(Func<ConnectionType, T> func, int warnthreshold = 500)
        {
            return Execute(() =>
            {
                T ret = default(T);
                ExecuteWithDatabaseInternal(db => ret = func(db));
                return ret;
            }, warnthreshold: warnthreshold);
        }

        public void Start(string threadname)
        {
            StopRequested = false;
            StopRequestedEvent.Reset();
            StopCompleted.Reset();

            if (SqlThread == null)
            {
                SqlThread = new Thread(SqlThreadProc);
                SqlThread.Name = threadname;
                SqlThread.IsBackground = true;
                System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + $"Start {typeof(ConnectionType).Name}  SQL Thread " + threadname);
                SqlThread.Start();
            }
        }

        public void Stop()
        {
            if (SqlThread != null)
            {
                System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + $"Stop {typeof(ConnectionType).Name}  SQL Thread " + SqlThread.Name);
                StopRequested = true;
                StopRequestedEvent.Set();
                StopCompleted.WaitOne();
                System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + $"Stopped {typeof(ConnectionType).Name} SQL " + SqlThread.Name);
                SqlThread = null;
            }
        }
    }

}
