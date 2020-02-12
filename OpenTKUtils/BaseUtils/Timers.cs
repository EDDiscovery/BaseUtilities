/*
 * Copyright © 2016 EDDiscovery development team
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace OpenTKUtils.Timers
{
    public class Timer : IDisposable
    {
        public bool Running { get; private set; } = false;
        public bool Recurring { get; set; } = false;
        public Action<Timer, long> Tick { get; set; } = null;
        public Object Tag { get; set; } = null;

        public Timer()
        {
        }

        public Timer(int msdelta, Action<Timer, long> tickaction, bool recurring = false)
        {
            Tick = tickaction;
            Start(msdelta, recurring);
        }

        public Timer(int msdelta, bool recurring = false)
        {
            Start(msdelta, recurring);
        }

        public void Start(int msdelta, bool recurring = false)
        {
            if (!mastertimer.IsRunning)
                mastertimer.Start();

            this.Recurring = recurring;
            this.Running = true;

            this.tickdelta = Stopwatch.Frequency / 1000 * msdelta;

            long timeout = mastertimer.ElapsedTicks + this.tickdelta;
            timerlist.Add(timeout, this);
            System.Diagnostics.Debug.WriteLine("Start timer");
        }

        public void Stop()
        {
            int i = timerlist.IndexOfValue(this);
            if (i >= 0)
            {
                timerlist.RemoveAt(i);
                Running = false;
                System.Diagnostics.Debug.WriteLine("Stop timer");
            }
        }

        public static void ProcessTimers()      // Someone needs to call this..
        {
            long timenow = mastertimer.ElapsedTicks;

            while (timerlist.Count > 0 && timerlist.Keys[0] < timenow)     // for all timers which have ticked out
            {
                long tickout = timerlist.Keys[0];   // remember its tick

                Timer t = timerlist.Values[0];

                t.Tick?.Invoke(t, mastertimer.ElapsedMilliseconds);   // fire event

                timerlist.RemoveAt(timerlist.IndexOfValue(t));  // remove from list

                if ( t.Recurring )  // add back if recurring
                {
                    timerlist.Add(tickout + t.tickdelta, t);     // add back to list
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }

        long tickdelta;

        static SortedList<long,Timer> timerlist = new SortedList<long,Timer>();
        static Stopwatch mastertimer = new Stopwatch();

    }
}
