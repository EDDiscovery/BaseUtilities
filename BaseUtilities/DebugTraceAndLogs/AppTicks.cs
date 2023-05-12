/*
 * Copyright © 2016 - 2017 EDDiscovery development team
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
 */

using System;
using System.Collections.Generic;

namespace BaseUtils
{
    public static class AppTicks
    {
        static System.Diagnostics.Stopwatch stopwatch;
        static Dictionary<string, long> laptimes;       // holds lap counters, defined by string IDs
        static Dictionary<string, long> startlaptimes;       // start time of lap counter

        private static void CreateStopWatch()
        {
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            laptimes = new Dictionary<string, long>();
            startlaptimes = new Dictionary<string, long>();
        }

        public static long TickCount        // current tick, with stopwatch creation
        {
            get
            {
                if (stopwatch == null)
                    CreateStopWatch();

                return stopwatch.ElapsedMilliseconds;
            }
        }

        // ID starting with @ means don't print it

        // return lap string, delta time to last lap, delta time to start 
        public static Tuple<string,int,int> TickCountLapDelta(string id, bool reset = false)        
        {
            long tc = TickCount;
            string idtext = id.StartsWith("@") ? "" : (" " + id);

            string res;
            int delta = 0;
            int totaldelta = 0;

            if (reset || !laptimes.ContainsKey(id))     // if reset, or not present
            {
                res = string.Format("{0}{1}", tc, idtext);
                startlaptimes[id] = tc;
            }
            else
            {
                totaldelta = (int)(tc - startlaptimes[id]);
                delta = (int)(tc - laptimes[id]);
                res = string.Format("{0} {1} +l {2} +t {3}", tc, idtext, delta, totaldelta);
            }

            laptimes[id] = tc;
            return new Tuple<string,int,int>(res,delta,totaldelta);
        }

        public static long TickCountFromLastLap(string id)        // lap time to last lap of this id
        {
            long tc = TickCount;
            if (laptimes.ContainsKey(id))     // if reset, or not present
                return tc - laptimes[id];
            else
                return 0;
        }

        public static long TickCountFromStart(string id)        // total time from start
        {
            long tc = TickCount;
            if (startlaptimes.ContainsKey(id))     // if reset, or not present
                return tc - startlaptimes[id];
            else
                return 0;
        }

        public static string TickCountLap(string id, bool reset = false)        // lap time to last recorded tick of this id
        {
            return TickCountLapDelta(id, reset).Item1;
        }

        public static string TickCountLap()        // default Program lap
        {
            return TickCountLap("@");
        }

        public static uint MS(uint mod = 10000)         // ms from enviroment, uint and modulo
        {
            return ((uint)Environment.TickCount) % mod;
        }

        public static uint MSd { get { return ((uint)Environment.TickCount) % 10000; } }
    }
}
