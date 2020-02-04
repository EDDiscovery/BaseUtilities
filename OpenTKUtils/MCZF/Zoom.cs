/*
 * Copyright 2015 - 2019 EDDiscovery development team
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
using BaseUtils;
using OpenTK;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace OpenTKUtils.Common
{
    public class Zoom
    {
        public float Zoom1Distance { get; set; } = 1000F;                     // distance that Current=1 will be from the Position, in the direction of the camera.

        public float Current { get; set; } = 1.0f;
        public void Set(Vector3 eye, Vector3 lookat) { Current = Zoom1Distance / (eye - lookat).Length; }  // set using two points

        public float EyeDistance { get { return Zoom1Distance / Current; } }    // distance of eye from target position
        
        public bool InSlew { get { return (zoomtimer.IsRunning); } }
        public float Default { get { return defaultZoom; } set { defaultZoom = Math.Min(Math.Max(value, ZoomMin),ZoomMax); } }

        public float ZoomMax = 300F;            // Default out Current
        public float ZoomMin = 0.01F;           // Iain special ;-) - this depends on znear (the clip distance - smaller you can Current in more) and Zoomdistance.
        public float ZoomFact = 1.258925F;      // scaling

        private float defaultZoom = 1F;

        private int zoomtimeperstep = 0;
        private float zoommultiplier = 0;
        private float zoomtarget = 0;
        private Stopwatch zoomtimer = new Stopwatch();
        long zoomnextsteptime = 0;

        public void Multiply(float other)
        {
            KillSlew();
            Current *= other;
            Current = Math.Max(Math.Min(Current, ZoomMax), ZoomMin);
        }

        public void Scale(bool direction)        // direction true is scale up Current
        {
            KillSlew();
            if (direction)
            {
                Current *= (float)ZoomFact;
                if (Current > ZoomMax)
                    Current = (float)ZoomMax;
            }
            else
            {
                Current /= (float)ZoomFact;
                if (Current < ZoomMin)
                    Current = (float)ZoomMin;
            }
        }


        public void GoTo( float z , float timetozoom = 0)        // <0 means auto estimate
        {
            if (timetozoom == 0)
            {
                Current = z;
            }
            else if ( z != Current )
            {
                zoomtarget = z;

                if ( timetozoom < 0 )       // auto estimate on log distance between them
                {
                    timetozoom = (float)(Math.Abs(Math.Log10(zoomtarget / Current)) * 1.5);
                }

                zoomtimeperstep = 50;          // go for 20hz tick
                int wantedsteps = (int)((timetozoom * 1000.0F) / zoomtimeperstep);
                zoommultiplier = (float)Math.Pow(10.0, Math.Log10(zoomtarget / Current) / wantedsteps );      // I.S^n = F I = initial, F = final, S = scaling, N = no of steps

                zoomtimer.Stop();
                zoomtimer.Reset();
                zoomtimer.Start();
                zoomnextsteptime = 0;

                //Console.WriteLine("Current {0} to {1} in {2} steps {3} steptime {4} mult {5}", _zoom, _zoomtarget, timetozoom*1000, wantedsteps, _zoomtimeperstep , _zoommultiplier );
            }
        }

        public void KillSlew()
        {
            zoomtimer.Stop();
        }

        public bool DoSlew()                           // do dynamic Current adjustments..  true if a readjust Current needed
        {
            if (zoomtimer.IsRunning && zoomtimer.ElapsedMilliseconds >= zoomnextsteptime)
            {
                float newzoom = (float)(Current * zoommultiplier);
                bool stop = (zoomtarget > Current) ? (newzoom >= zoomtarget) : (newzoom <= zoomtarget);

                //Console.WriteLine("{0} Current {1} -> {2} m {3} t {4} stop {5}", _zoomtimer.ElapsedMilliseconds, _zoom , newzoom, _zoommultiplier, _zoomtarget, stop);

                if (stop)
                {
                    Current = zoomtarget;
                    zoomtimer.Stop();
                }
                else
                {
                    Current = newzoom;
                    zoomnextsteptime += zoomtimeperstep;
                }

                return true;
            }
            else
                return false;
        }

        public bool Keyboard(KeyboardState kbd, float adjustment)
        {
            bool changed = false;

            if (kbd.IsAnyPressed(KeyboardState.ShiftState.None,Keys.Add, Keys.M))
            {
                Multiply(adjustment);
                changed = true;
            }

            if (kbd.IsAnyPressed(KeyboardState.ShiftState.None, Keys.Subtract, Keys.N) )
            {
                Multiply(1.0f / adjustment);
                changed = true;
            }

            float newzoom = 0;

            if (kbd.IsPressedRemove(Keys.D1))
                newzoom = ZoomMax;
            if (kbd.IsPressedRemove(Keys.D2))
                newzoom = 100;                                                      // Factor 3 scale
            if (kbd.IsPressedRemove(Keys.D3))
                newzoom = 33;
            if (kbd.IsPressedRemove(Keys.D4))
                newzoom = 11F;
            if (kbd.IsPressedRemove(Keys.D5))
                newzoom = 3.7F;
            if (kbd.IsPressedRemove(Keys.D6))
                newzoom = 1.23F;
            if (kbd.IsPressedRemove(Keys.D7))
                newzoom = 0.4F;
            if (kbd.IsPressedRemove(Keys.D8))
                newzoom = 0.133F;
            if (kbd.IsPressedRemove(Keys.D9))
                newzoom = ZoomMin;

            if (newzoom != 0)
            {
                GoTo(newzoom, -1);
                changed = true;
            }

            return changed;
        }


    }
}
