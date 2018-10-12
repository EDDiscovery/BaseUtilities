/*
 * Copyright © 2015 - 2016 EDDiscovery development team
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
using System.Diagnostics;

namespace OpenTKUtils.Common
{
    public class Zoom
    {
        public float Current { get { return zoom; } }
        public bool InSlew { get { return (zoomtimer.IsRunning); } }
        public float Default { get { return defaultZoom; } set { defaultZoom = Math.Min(Math.Max(value, ZoomMin),ZoomMax); } }

        public float ZoomMax = 300F;            // Default out zoom 
        public float ZoomMin = 0.01F;           // Iain special ;-) - this depends on znear (the clip distance - smaller you can zoom in more) and Zoomdistance.   
        public float ZoomFact = 1.258925F;      // scaling

        private float defaultZoom = 1F;
        private float zoom = 1.0f;

        private int zoomtimeperstep = 0;
        private float zoommultiplier = 0;
        private float zoomtarget = 0;
        private Stopwatch zoomtimer = new Stopwatch();
        long zoomnextsteptime = 0;

        public void SetDefault()
        {
            KillSlew();
            zoom = defaultZoom;
        }

        public void Multiply(float other)
        {
            KillSlew();
            zoom *= other;
            zoom = Math.Max(Math.Min(zoom, ZoomMax), ZoomMin);
        }

        public void Scale(bool direction)        // direction true is scale up zoom
        {
            KillSlew();
            if (direction)
            {
                zoom *= (float)ZoomFact;
                if (zoom > ZoomMax)
                    zoom = (float)ZoomMax;
            }
            else
            {
                zoom /= (float)ZoomFact;
                if (zoom < ZoomMin)
                    zoom = (float)ZoomMin;
            }
        }


        public void GoTo( float z , float timetozoom = 0)        // <0 means auto estimate
        {
            if (timetozoom == 0)
            {
                zoom = z;
            }
            else if ( z != zoom )
            {
                zoomtarget = z;

                if ( timetozoom < 0 )       // auto estimate on log distance between them
                {
                    timetozoom = (float)(Math.Abs(Math.Log10(zoomtarget / zoom)) * 1.5);
                }

                zoomtimeperstep = 50;          // go for 20hz tick
                int wantedsteps = (int)((timetozoom * 1000.0F) / zoomtimeperstep);
                zoommultiplier = (float)Math.Pow(10.0, Math.Log10(zoomtarget / zoom) / wantedsteps );      // I.S^n = F I = initial, F = final, S = scaling, N = no of steps

                zoomtimer.Stop();
                zoomtimer.Reset();
                zoomtimer.Start();
                zoomnextsteptime = 0;

                //Console.WriteLine("Zoom {0} to {1} in {2} steps {3} steptime {4} mult {5}", _zoom, _zoomtarget, timetozoom*1000, wantedsteps, _zoomtimeperstep , _zoommultiplier );
            }
        }

        public void KillSlew()
        {
            zoomtimer.Stop();
        }

        public void DoSlew()                           // do dynamic zoom adjustments..  true if a readjust zoom needed
        {
            if ( zoomtimer.IsRunning && zoomtimer.ElapsedMilliseconds >= zoomnextsteptime )
            {
                float newzoom = (float)(zoom * zoommultiplier);
                bool stop = (zoomtarget > zoom) ? (newzoom >= zoomtarget) : (newzoom <= zoomtarget);

                //Console.WriteLine("{0} Zoom {1} -> {2} m {3} t {4} stop {5}", _zoomtimer.ElapsedMilliseconds, _zoom , newzoom, _zoommultiplier, _zoomtarget, stop);

                if (stop)
                {
                    zoom = zoomtarget;
                    zoomtimer.Stop();
                }
                else
                {
                    zoom = newzoom;
                    zoomnextsteptime += zoomtimeperstep;
                }
            }
        }



    }
}
