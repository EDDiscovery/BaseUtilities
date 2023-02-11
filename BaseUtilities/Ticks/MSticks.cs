/*
 * Copyright © 2023-2023 EDDiscovery development team
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

namespace BaseUtils
{
    public class MSTicks
    {
        static uint TickCount { get { return (uint)Environment.TickCount; } }
        static bool IsTimedOut(uint tickcount) { return ((((uint)Environment.TickCount) - tickcount) & 0x80000000) == 0; }    // if the tick > tickcount, it will be positive, and therefore and will be zero
        static bool IsNotTimedOut(uint tickcount) { return ((((uint)Environment.TickCount) - tickcount) & 0x80000000) != 0; }    // if the tick > tickcount, it will be positive, and therefore and will be zero

        public uint StartTick { get; private set; }
        public uint EndTick { get; private set; }
        public bool IsRunning { get; private set; }
        public uint TimeRunning { get { return IsRunning ? (TickCount - StartTick) : 0; } }
        public double Percentage { get { return IsRunning ? ((double)(TickCount - StartTick) / (EndTick - StartTick)) : 0; } }

        public MSTicks()
        {
        }

        public MSTicks(uint delayms)
        {
            TimeoutAt(delayms);
        }
        public MSTicks(int delayms)
        {
            TimeoutAt(delayms);
        }
        public void Run()                   // run means just mark the start, end is as far away as possible. TimeRunning will tell you how long its been operating
        {
            int t = Environment.TickCount;
            StartTick = (uint)t;
            EndTick = StartTick + (uint.MaxValue/2-1);
            IsRunning = true;
        }
        public void TimeoutAt(int delayms)  // EndTime is set by the delay from now
        {
            int t = Environment.TickCount;
            StartTick = (uint)t;
            EndTick = (uint)(t + delayms);
            IsRunning = true;
        }
        public void TimeoutAt(uint delayms)
        {
            int t = Environment.TickCount;
            StartTick = (uint)t;
            EndTick = (uint)t + delayms;
            IsRunning = true;
        }
        public void Stop()
        {
            IsRunning = false;
        }

        public bool TimedOut { get { return IsTimedOut(EndTick); } }
        public bool TimedOutStop { get { if (IsRunning && IsTimedOut(EndTick)) { Stop(); return true; } else return false; } }
        public bool NotTimedOut { get { return IsNotTimedOut(EndTick); } }

        public void AddMS(uint delayms)
        {
            EndTick += delayms;
        }
        public void AddMS(int delayms)
        {
            EndTick += (uint)delayms;
        }
    }
}

