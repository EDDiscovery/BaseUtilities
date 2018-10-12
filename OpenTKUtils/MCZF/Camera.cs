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
    public class Camera
    {
        public Vector3 Current { get { return cameraDir; } }    // in degrees

        public bool InSlew { get { return (cameraDirSlewProgress < 1.0F); } }

        private Vector3 cameraDir = Vector3.Zero;               // X = up/down, Y = rotate, Z = yaw, in degrees. 
        private float cameraDirSlewProgress = 1.0f;             // 0 -> 1 slew progress
        private float cameraDirSlewTime;                        // how long to take to do the slew
        private Vector3 cameraDirSlewPosition;                  // where to slew to.

        public void Set(Vector3 pos)
        {
            KillSlew();
            cameraDir = pos;
        }

        public void Rotate( Vector3 offset )
        {
            KillSlew();

            cameraDir.X = BoundedAngle(cameraDir.X + offset.X);
            cameraDir.Y = BoundedAngle(cameraDir.Y + offset.Y);
            cameraDir.Z = BoundedAngle(cameraDir.Z + offset.Z);        // rotate camera by asked value

            // Limit camera pitch
            if (cameraDir.X < 0 && cameraDir.X > -90)
                cameraDir.X = 0;
            if (cameraDir.X > 180 || cameraDir.X <= -90)
                cameraDir.X = 180;
        }

        public void RotateTBD(Vector3 rot)
        {
            cameraDir += rot;

            if (cameraDir.X < 0 && cameraDir.X > -90)
                cameraDir.X = 0;

            if (cameraDir.X > 180 || cameraDir.X <= -90)
                cameraDir.X = 180;
        }

        public void Pan(Vector3 pos, float timeslewsec = 0)       // may pass a Nan Position - no action
        {
            if (!float.IsNaN(pos.X))
            {
                if (timeslewsec == 0)
                {
                    cameraDir = pos;
                }
                else
                {
                    cameraDirSlewPosition = pos;
                    cameraDirSlewProgress = 0.0f;
                    cameraDirSlewTime = (timeslewsec == 0) ? (1.0F) : timeslewsec;
                }
            }
        }

        public void LookAt(Vector3 curpos, Vector3 target, float zoom, float time = 0)            // real world 
        {
            Vector3 targetinv = new Vector3(target.X, -target.Y, target.Z);
            Vector3 eye = curpos;
            Vector3 camera = AzEl(eye, targetinv);
            camera.Y = 180 - camera.Y;      // adjust to this system
            Pan(camera, time);
        }

        public void KillSlew()
        {
            cameraDirSlewProgress = 1.0f;
        }

        public void DoSlew(int msticks)
        {
            if (cameraDirSlewProgress < 1.0f)
            {
                var newprogress = cameraDirSlewProgress + msticks / (cameraDirSlewTime * 1000);

                if (newprogress >= 1.0f)
                {
                    cameraDir = cameraDirSlewPosition;
                }
                else
                {
                    var slewstart = Math.Sin((cameraDirSlewProgress - 0.5) * Math.PI);
                    var slewend = Math.Sin((newprogress - 0.5) * Math.PI);
                    Debug.Assert((1 - 0 - slewstart) != 0);
                    var slewfact = (slewend - slewstart) / (1.0 - slewstart);

                    var totvector = new Vector3((float)(cameraDirSlewPosition.X - cameraDir.X), (float)(cameraDirSlewPosition.Y - cameraDir.Y), (float)(cameraDirSlewPosition.Z - cameraDir.Z));
                    cameraDir += Vector3.Multiply(totvector, (float)slewfact);
                }

                cameraDirSlewProgress = (float)newprogress;
            }
        }

        private static Vector3 AzEl(Vector3 curpos, Vector3 target)     // az and elevation between curpos and target
        {
            Vector3 delta = Vector3.Subtract(target, curpos);
            //Console.WriteLine("{0}->{1} d {2}", curpos, target, delta);

            float radius = delta.Length;

            if (radius < 0.1)
                return new Vector3(180, 0, 0);     // point forward, level

            float inclination = (float)Math.Acos(delta.Y / radius);
            float azimuth = (float)Math.Atan(delta.Z / delta.X);

            inclination *= (float)(180 / Math.PI);
            azimuth *= (float)(180 / Math.PI);

            if (delta.X < 0)      // atan wraps -90 (south)->+90 (north), then -90 to +90 around the y axis, going anticlockwise
                azimuth += 180;
            azimuth += 90;        // adjust to 0 at bottom, 180 north, to 360

            return new Vector3(inclination, azimuth, 0);
        }

        private float BoundedAngle(float angle)
        {
            return ((angle + 360 + 180) % 360) - 180;
        }
    }
}

  