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
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System;
using System.Collections.Generic;

namespace BaseUtils
{
    public static class AppTicks
    {
        static System.Diagnostics.Stopwatch stopwatch;
        static Dictionary<string, long> laptimes;       // holds lap counters, defined by string IDs

        private static void CreateStopWatch()
        {
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            laptimes = new Dictionary<string, long>();
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

        public static Tuple<string,int> TickCountLapDelta(string id, bool reset = false)        // lap time to last recorded tick of this id
        {
            long tc = TickCount;
            string idtext = id.StartsWith("@") ? "" : (" " + id);

            string res;
            int delta = 0;

            if (reset || !laptimes.ContainsKey(id))     // if reset, or not present
            {
                res = string.Format("{0}{1}", tc, idtext);
            }
            else
            {
                delta = (int)(tc - laptimes[id]);
                res = string.Format("{0}{1}+{2}", tc, idtext, delta);
            }

            laptimes[id] = tc;
            return new Tuple<string,int>(res,delta);
        }

        public static long TickCountFrom(string id)        // lap time to last recorded tick of this id
        {
            long tc = TickCount;
            if (laptimes.ContainsKey(id))     // if reset, or not present
                return tc - laptimes[id];
            else
                return 0;
        }

        public static string TickCountLap(string id, bool reset = false)        // lap time to last recorded tick of this id
        {
            return TickCountLapDelta(id, reset).Item1;
        }

        public static string TickCountLap(Object id, bool reset = false)        // lap time to last recorded tick of this object used as an identifier
        {
            return TickCountLap(id.GetType().Name, reset);
        }

        public static string TickCountLap(Type id, bool reset = false)        // lap time to last recorded tick of this type used as an identifier
        {
            return TickCountLap(id.Name, reset);
        }

        public static string TickCountLap()        // default Program lap
        {
            return TickCountLap("@");
        }

        public static string TickCountLapDelta()        // default Program lap
        {
            return TickCountLap("@");
        }
    }
}
