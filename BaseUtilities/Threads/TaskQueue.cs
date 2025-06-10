/*
 * Copyright © 2020 EDDiscovery development team
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
 *
 */

using System;
using System.Collections.Generic;
using System.Threading;

namespace BaseUtils.Threads
{
    // with thanks to https://michaelscodingspot.com/c-job-queues/
    // Uses the thread pool to make a thread to consume actions in order, thread terminates when all tasks done

    public class TaskQueue
    {
        public bool Active { get { lock (jobs) { return jobs.Count > 0; } } }

        private Queue<Action> jobs = new Queue<Action>();
        private bool delegateQueuedOrRunning = false;

        public void Enqueue(Action job)
        {
            lock (jobs)
            {
                jobs.Enqueue(job);
                if (!delegateQueuedOrRunning)
                {
                    delegateQueuedOrRunning = true;
                    ThreadPool.QueueUserWorkItem(ProcessQueuedItems, null);
                }
            }
        }

        private void ProcessQueuedItems(object ignored)
        {
            while (true)
            {
                Action job;
                lock (jobs)
                {
                    if (jobs.Count == 0)        // its locked, so we can't get race conditions between adding/removing
                    {
                        delegateQueuedOrRunning = false;
                        break;
                    }

                    job = jobs.Dequeue();
                }

                job.Invoke();
            }
        }
    }
}









//        public bool Active { get { return active != 0; } }

//        private ConcurrentQueue<Action> actionqueue = new ConcurrentQueue<Action>();
//        private int active = 0;
//        private Thread thread = null;

//        public TaskQueue()
//        {
//        }

//        public void Enqueue(Action a)
//        {
//            actionqueue.Enqueue(a);

//            // no thread, or not alive, or indicating not active.
//            if ( thread==null || !thread.IsAlive || Interlocked.CompareExchange(ref active, 1, 0)==0)



//            if (Interlocked.CompareExchange(ref active, 1, 0) == 0)
//            { 
//              //  System.Diagnostics.Debug.WriteLine("Make Thread");
//                var thread = new Thread(new ThreadStart(Run));
//                thread.IsBackground = true;
//                thread.Start();
//            }
//        }

//        private void Run()
//        {
//            //System.Diagnostics.Debug.WriteLine("Start Thread");
//            while (actionqueue.TryDequeue(out Action nextaction))
//            {
//              //  System.Diagnostics.Debug.WriteLine("Run task");
//                nextaction.Invoke();
//            }
//            //active=1, alive
//            Interlocked.CompareExchange(ref active, 0, 1);
//            //active=0, alive
//            //System.Diagnostics.Debug.WriteLine("Finish");
//        }
//    }
//}

