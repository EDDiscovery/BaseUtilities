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
    public class Camera
    {
        // X component = rotation around the x (horizontal to you) axis
        // Y component = rotation around the Y (vertical to you) axis
        // Z component = rotation around the Z (facing you) axis - therefore rotates the camera

        public Vector3 Current { get { return cameraDir; } }    // in degrees
        public Vector3 Normal { get { return cameraNormal; } }  // normal to the camera.. calculated if its moved.

        public Vector3 RotateToLookAtMe { get { return new Vector3(-(180f - Current.X), Current.Y, 0); } }

        public bool InSlew { get { return (cameraDirSlewProgress < 1.0F); } }

        private Vector3 cameraDir = Vector3.Zero;               // X = up/down, Y = rotate, Z = yaw, in degrees.
        private Vector3 cameraNormal = Vector3.Zero;        
        private float cameraDirSlewProgress = 1.0f;             // 0 -> 1 slew progress
        private float cameraDirSlewTime;                        // how long to take to do the slew
        private Vector3 cameraDirSlewPosition;                  // where to slew to.

        public void Set(Vector3 pos)
        {
            KillSlew();
            cameraDir = pos;
            CalcNormal();
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

            CalcNormal();
        }

        public void Pan(Vector3 pos, float timeslewsec = 0)       // may pass a Nan Position - no action
        {
            if (!float.IsNaN(pos.X))
            {
                if (timeslewsec == 0)
                {
                    cameraDir = pos;
                    CalcNormal();
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

        public bool DoSlew(int msticks)
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

                CalcNormal();
                cameraDirSlewProgress = (float)newprogress;

                return true;
            }
            else
                return false;
        }

        public void CalcNormal()                // we need the normal for the perspective mode
        {
            Matrix3 transform = Matrix3.Identity;                   // identity nominal matrix, dir is in degrees

            // we rotate the identity matrix by the camera direction
            // .x and .y values are set by our axis orientations..
            // .x translates around the x axis, x = 0 = to +Y, x = 90 on the x/z plane, x = 180 = to -Y
            // .y translates around the y axis. y= 0 = to +Z (forward), y = 90 = to +x (look from left), y = -90 = to -x (look from right), y = 180 = look back
            // .z rotates the camera.

            transform *= Matrix3.CreateRotationX((float)(cameraDir.X * Math.PI / 180.0f));
            transform *= Matrix3.CreateRotationY((float)(cameraDir.Y * Math.PI / 180.0f));
            transform *= Matrix3.CreateRotationZ((float)(cameraDir.Z * Math.PI / 180.0f));

            cameraNormal = Vector3.Transform(new Vector3(0.0f, 0.0f, 1.0f), transform);       // 0,0,1 also sets the axis - this whats make .x/.y address the x/y rotations
        }

        public enum KeyboardAction { None, MoveEye, MovePosition };

        public KeyboardAction Keyboard(KeyboardState kbd, float angle)
        {
            Vector3 cameraActionRotation = Vector3.Zero;
            KeyboardAction act = KeyboardAction.MoveEye;

            if (kbd.Shift)
                angle *= 2.0F;

            if (kbd.IsPressed(Keys.NumPad4) != null)
            {
                cameraActionRotation.Z = -angle;
            }
            if (kbd.IsPressed(Keys.NumPad6) != null)
            {
                cameraActionRotation.Z = angle;
            }

            if (kbd.IsAnyPressed(Keys.NumPad5, Keys.NumPad2,Keys.Z) != null)
            {
                cameraActionRotation.X = -angle;
            }
            if (kbd.IsAnyPressed(Keys.NumPad8,Keys.X) != null)
            {
                cameraActionRotation.X = angle;
            }

            if (kbd.IsPressed(KeyboardState.ShiftState.Ctrl, Keys.Q))
            {
                cameraActionRotation.Y = angle;
            }
            else if (kbd.IsAnyPressed(Keys.NumPad7, Keys.Q) != null)
            {
                cameraActionRotation.Y = -angle;
                act = KeyboardAction.MovePosition;
            }

            if (kbd.IsPressed(KeyboardState.ShiftState.Ctrl, Keys.E))
            {
                cameraActionRotation.Y = -angle;
            }
            else if (kbd.IsAnyPressed(Keys.NumPad9, Keys.E) != null)
            {
                cameraActionRotation.Y = angle;
                act = KeyboardAction.MovePosition;
            }

            if (cameraActionRotation.LengthSquared > 0)
            {
                Rotate(cameraActionRotation);
                return act;
            }
            else
                return KeyboardAction.None;
        }



    }
}

