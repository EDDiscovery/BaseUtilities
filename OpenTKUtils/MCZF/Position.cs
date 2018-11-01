/*
 * Copyright © 2015 - 2018 EDDiscovery development team
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
using OpenTK;
using System;
using System.Diagnostics;

namespace OpenTKUtils.Common
{
    public class Position
    {
        public Vector3 Current { get { Vector3 norm = new Vector3(position); norm.Y = -norm.Y; return norm; } } // REAL world position of eye.  With no internal invert of Y.  Same as star positions
        public bool InSlew { get { return (targetposSlewProgress < 1.0f); } }

        private Vector3 position = Vector3.Zero;                // point where we are viewing. Eye is offset from this by _cameraDir * 1000/_zoom. (prev _cameraPos)
        private float targetposSlewProgress = 1.0f;             // 0 -> 1 slew progress
        private float targetposSlewTime;                        // how long to take to do the slew
        private Vector3 targetposSlewPosition;                  // where to slew to.

        public void Set(Vector3 pos)
        {
            KillSlew();
            position = pos;
        }

        public void Translate(Vector3 pos)
        {
            KillSlew();
            position += pos;
        }

        public void X(float adj) { KillSlew(); position.X += adj; }     // adjust axis
        public void Y(float adj) { KillSlew(); position.Y += adj; }
        public void Z(float adj) { KillSlew(); position.Z += adj; }

        // time <0 estimate, 0 instance >0 time
        public void GoTo(Vector3 normpos, float timeslewsec = 0, float unitspersecond = 10000F)       // may pass a Nan Position - no action. Y is normal sense
        {
            if (!float.IsNaN(normpos.X))
            {
                System.Diagnostics.Debug.WriteLine("Goto " + normpos + " in " + timeslewsec + " at " + unitspersecond);

                Vector3 pos = new Vector3(normpos);
                pos.Y = -pos.Y;     // invert to internal Y

                double dist = Math.Sqrt((position.X - pos.X) * (position.X - pos.X) + (position.Y - pos.Y) * (position.Y - pos.Y) + (position.Z - pos.Z) * (position.Z - pos.Z));
                Debug.Assert(!double.IsNaN(dist));      // had a bug due to incorrect signs!

                if (dist >= 1)
                {
                    if (timeslewsec == 0)
                    {
                        position = pos;
                    }
                    else
                    {
                        targetposSlewPosition = pos;
                        targetposSlewProgress = 0.0f;
                        targetposSlewTime = (timeslewsec < 0) ? ((float)Math.Max(1.0, dist / unitspersecond)) : timeslewsec;            //10000 ly/sec, with a minimum slew
                        System.Diagnostics.Debug.WriteLine("{0} Slew start to {1} in {2}",  Environment.TickCount % 10000 , targetposSlewPosition , targetposSlewTime);
                    }
                }
            }
        }

        public void KillSlew()
        {
            targetposSlewProgress = 1.0f;
        }

        public void DoSlew(int msticks)
        {
            if (targetposSlewProgress < 1.0f)
            {
                Debug.Assert(targetposSlewTime > 0);
                var newprogress = targetposSlewProgress + msticks / (targetposSlewTime * 1000);

                if (newprogress >= 1.0f)
                {
                    position = new Vector3(targetposSlewPosition.X, targetposSlewPosition.Y, targetposSlewPosition.Z);
                    System.Diagnostics.Debug.WriteLine("{0} Slew complete at {1}", Environment.TickCount % 10000, position);
                }
                else
                {
                    var slewstart = Math.Sin((targetposSlewProgress - 0.5) * Math.PI);
                    var slewend = Math.Sin((newprogress - 0.5) * Math.PI);
                    Debug.Assert((1 - 0 - slewstart) != 0);
                    var slewfact = (slewend - slewstart) / (1.0 - slewstart);

                    var totvector = new Vector3((float)(targetposSlewPosition.X - position.X), (float)(targetposSlewPosition.Y - position.Y), (float)(targetposSlewPosition.Z - position.Z));
                    position += Vector3.Multiply(totvector, (float)slewfact);
                    System.Diagnostics.Debug.WriteLine("{0} Slew to {1} prog {2}", Environment.TickCount % 10000, position, newprogress);
                }

                targetposSlewProgress = (float)newprogress;
            }
        }
   }
}

