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
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace BaseUtils.Threads
{
    // Makes a thread to operate actions in order, thread stops when all actions are done.

    public class TaskQueue
    {
        private ConcurrentQueue<Action> actionqueue = new ConcurrentQueue<Action>();
        private int active = 0;

        public TaskQueue()
        {
        }

        public void Enqueue(Action a)
        {
            actionqueue.Enqueue(a);
            if (Interlocked.CompareExchange(ref active, 1, 0) == 0)
            { 
              //  System.Diagnostics.Debug.WriteLine("Make Thread");
                var thread = new Thread(new ThreadStart(Run));
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private void Run()
        {
            //System.Diagnostics.Debug.WriteLine("Start Thread");
            while (actionqueue.TryDequeue(out Action nextaction))
            {
              //  System.Diagnostics.Debug.WriteLine("Run task");
                nextaction.Invoke();
            }
            active = 0;
            //System.Diagnostics.Debug.WriteLine("Finish");
        }
    }
}

