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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SQLLiteExtensions
{
    public class SQLProcessingThread<ConnectionType> where ConnectionType: IDisposable, new()
    {
        private static ConcurrentBag<Tuple<Task, string>> TasksRun = new ConcurrentBag<Tuple<Task, string>>();

        private abstract class Job
        {
            public abstract Task Exec();
        }

        private class Job<T> : Job
        {
            private readonly ManualResetEventSlim JobCompletedEvent = new ManualResetEventSlim(false);
            private Task<T> JobTask;
            private readonly Func<Task<T>> Action;
            private readonly string CallerStackTrace;

            public Job(Func<Task<T>> action)
            {
                CallerStackTrace = new StackTrace().ToString();
                Action = action;
            }

            public override async Task Exec()
            {
                try
                {
                    var task = Action.Invoke();
                    TasksRun.Add(new Tuple<Task, string>(task, new StackTrace().ToString()));
                    var res = await task;
                    JobTask = Task.FromResult(res);
                }
                catch (Exception ex)
                {
                    JobTask = Task.FromException<T>(ex);
                }

                TasksRun.Add(new Tuple<Task, string>(JobTask, new StackTrace().ToString()));

                JobCompletedEvent.Set();
            }

            public T Wait()
            {
                JobCompletedEvent.Wait();
                return JobTask.Result;
            }

            public async Task<T> WaitAsync()
            {
                var waitjobbompleted = Task.Factory.StartNew(() => JobCompletedEvent.Wait());
                TasksRun.Add(new Tuple<Task, string>(waitjobbompleted, new StackTrace().ToString()));
                await waitjobbompleted;
                return await JobTask;
            }
        }

        private ConcurrentQueue<Job> JobQueue = new ConcurrentQueue<Job>();
        private Thread SqlThread;
        private ManualResetEvent StopRequestedEvent = new ManualResetEvent(false);
        private bool StopRequested = false;
        private ManualResetEvent JobQueuedEvent = new ManualResetEvent(false);
        private ManualResetEvent StopCompleted = new ManualResetEvent(true);

        public long? SqlThreadId => SqlThread?.ManagedThreadId;

        protected ConnectionType Connection;

        private void SqlThreadProc()    // SQL process thread
        {
            try
            {
                using (Connection = new ConnectionType())   // hold connection over whole period.
                {
                    var runningjobs = new Dictionary<Task, Job>();
                    var jobscompleted = new List<KeyValuePair<Task, Job>>();
                    var stoprequestedtask = Task.Run(() => StopRequestedEvent.WaitOne());
                    var jobqueuedtask = Task.Run(() => JobQueuedEvent.WaitOne());

                    while (!StopRequested)
                    {
                        if (runningjobs.Count == 0)
                        {
                            WaitHandle.WaitAny(new WaitHandle[] { StopRequestedEvent, JobQueuedEvent });
                        }

                        JobQueuedEvent.Reset();

                        while (!StopRequested && JobQueue.TryDequeue(out Job job))
                        {
                            runningjobs[job.Exec()] = job;
                        }

                        // Using .GetAwaiter().GetResult() ensures async tasks are processed on this thread
                        var tasktask = Task.WhenAny(runningjobs.Keys.Concat(new[] { stoprequestedtask, jobqueuedtask }));

                        try
                        {
                            var task = tasktask.GetAwaiter().GetResult();
                            task.Wait();

                            if (runningjobs.TryGetValue(task, out Job job))
                            {
                                runningjobs.Remove(task);
                                TasksRun.Add(new Tuple<Task, string>(task, new StackTrace().ToString()));
                                jobscompleted.Add(new KeyValuePair<Task, Job>(task, job));
                            }
                            else if (object.ReferenceEquals(task, jobqueuedtask))
                            {
                                jobqueuedtask = Task.Factory.StartNew(() => JobQueuedEvent.WaitOne());
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine($"Error executing task: {ex.ToString()}");
                        }
                    }
                }
            }
            finally
            {
                StopCompleted.Set();
            }
        }

        protected T Execute<T>(Func<Task<T>> action, int skipframes = 1, int warnthreshold = 500)  // in caller thread, queue to job queue, wait for complete
        {
            if (StopCompleted.WaitOne(0))
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();

            var ret = default(T);

            if (Thread.CurrentThread.ManagedThreadId == SqlThread?.ManagedThreadId)     // if current thread is the SQL Job thread, uh-oh
            {
                System.Diagnostics.Trace.WriteLine($"Database Re-entrancy\n{new System.Diagnostics.StackTrace(skipframes, true).ToString()}");
                var task = action();
                TasksRun.Add(new Tuple<Task, string>(task, new StackTrace().ToString()));
                ret = task.Result;
            }
            else
            {
                var job = new Job<T>(action);       // make a new job and queue it
                                                    //System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Queue Job " + Thread.CurrentThread.Name);
                JobQueue.Enqueue(job);
                JobQueuedEvent.Set();           // kick the thread to execute it.
                ret = job.Wait();
            }

            if (sw.ElapsedMilliseconds > warnthreshold)
            {
                var trace = new System.Diagnostics.StackTrace(skipframes, true);
                System.Diagnostics.Trace.WriteLine($"Database connection held for {sw.ElapsedMilliseconds}ms\n{trace.ToString()}");
            }

            return ret;
        }

        protected void Execute(Func<Task> func, int skipframes = 1, int warnthreshold = 500)
        {
            Execute<object>(async () => { await func(); return null; }, skipframes + 1, warnthreshold);
        }

        public void ExecuteWithDatabase(Func<ConnectionType, Task> action, int warnthreshold = 500)
        {
            Execute(() => { action.Invoke(Connection); return Task.CompletedTask; }, warnthreshold: warnthreshold);
        }

        public void ExecuteWithDatabase(Action<ConnectionType> action, int warnthreshold = 500)
        {
            Execute(() => { action.Invoke(Connection); return Task.CompletedTask; }, warnthreshold: warnthreshold);
        }

        public T ExecuteWithDatabase<T>(Func<ConnectionType, Task<T>> func, int warnthreshold = 500)
        {
            return Execute(() => func.Invoke(Connection), warnthreshold: warnthreshold);
        }

        public T ExecuteWithDatabase<T>(Func<ConnectionType, T> func, int warnthreshold = 500)
        {
            return Execute(() => { var ret = func.Invoke(Connection); return Task.FromResult(ret); }, warnthreshold: warnthreshold);
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
                System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Start SQL Thread " + threadname);
                SqlThread.Start();
            }
        }

        public void Stop()
        {
            if (SqlThread != null)
            {
                System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Stop SQL Thread " + SqlThread.Name);
                StopRequested = true;
                StopRequestedEvent.Set();
                StopCompleted.WaitOne();
                System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Stopped SQL " + SqlThread.Name);
                SqlThread = null;
            }
        }
    }

}
