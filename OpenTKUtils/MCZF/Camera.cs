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

        public Vector3 RotateToLookAtMe { get { return new Vector3(-(180f - Current.X), Current.Y, 0); } }

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

            cameraDir.X = cameraDir.X.BoundedAngle(offset.X);
            cameraDir.Y = cameraDir.Y.BoundedAngle(offset.Y);
            cameraDir.Z = cameraDir.Z.BoundedAngle(offset.Z);        // rotate camera by asked value

            // Limit camera pitch
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

        public void LookAt(Vector3 eyepos, Vector3 target, float zoom, float time = 0)            // real world
        {
            Vector3 camera = eyepos.AzEl(target,true);
            System.Diagnostics.Debug.WriteLine("...Eyepos " + eyepos + " target " + target + " Camera dir cur " + cameraDir + " to " + camera);
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

    }
}

