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
    public class Position       // holds lookat and eyepositions.
    {
        public Vector3 Lookat { get { return lookat; } set { KillSlew(); lookat = value; } }
        public Vector3 EyePosition { get { return eyeposition; } set { KillSlew(); eyeposition = value; } }

        public bool InSlew { get { return (targetposSlewProgress < 1.0f); } }

        private Vector3 lookat = Vector3.Zero;                // point where we are viewing. 
        private Vector3 eyeposition = new Vector3(10,10,10);  // and the eye position

        private float targetposSlewProgress = 1.0f;             // 0 -> 1 slew progress
        private float targetposSlewTime;                        // how long to take to do the slew
        private Vector3 targetposSlewPosition;                  // where to slew to.
        private Vector3 targetposEyePosition;                   // where to slew to.

        public void Translate(Vector3 pos)
        {
            KillSlew();
            lookat += pos;
            eyeposition += pos;
        }

        public void X(float adj) { KillSlew(); lookat.X += adj; eyeposition.X += adj; }     // adjust axis
        public void Y(float adj) { KillSlew(); lookat.Y += adj; eyeposition.Y += adj; }
        public void Z(float adj) { KillSlew(); lookat.Z += adj; eyeposition.Z += adj; }

        public void SetEyePositionFromLookat(Vector3 cameradir, float distance)              // from current lookat, set eyeposition, given a camera angle and a distance
        {
            Matrix3 transform = Matrix3.Identity;                   // identity nominal matrix, dir is in degrees

            // we rotate the identity matrix by the camera direction
            // .x and .y values are set by our axis orientations..
            // .x translates around the x axis, x = 0 = to +Y, x = 90 on the x/z plane, x = 180 = to -Y
            // .y translates around the y axis. y= 0 = to +Z (forward), y = 90 = to +x (look from left), y = -90 = to -x (look from right), y = 180 = look back
            // .z rotates the camera.

            transform *= Matrix3.CreateRotationX((float)(cameradir.X * Math.PI / 180.0f));
            transform *= Matrix3.CreateRotationY((float)(cameradir.Y * Math.PI / 180.0f));
            transform *= Matrix3.CreateRotationZ((float)(cameradir.Z * Math.PI / 180.0f));

            Vector3 eyerel = Vector3.Transform(new Vector3(0, -distance, 0), transform);       // the 0,-E,0 sets the axis of the system..
            eyeposition = lookat + eyerel;
        }

        public void SetLookatPositionFromEye(Vector3 cameradir, float distance)              // from current eye position, set lookat, given a camera angle and a distance
        {
            Matrix3 transform = Matrix3.Identity;                   // identity nominal matrix, dir is in degrees

            // we rotate the identity matrix by the camera direction
            // .x and .y values are set by our axis orientations..
            // .x translates around the x axis, x = 0 = to +Y, x = 90 on the x/z plane, x = 180 = to -Y
            // .y translates around the y axis. y= 0 = to +Z (forward), y = 90 = to +x (look from left), y = -90 = to -x (look from right), y = 180 = look back
            // .z rotates the camera.

            transform *= Matrix3.CreateRotationX((float)(cameradir.X * Math.PI / 180.0f));
            transform *= Matrix3.CreateRotationY((float)(cameradir.Y * Math.PI / 180.0f));
            transform *= Matrix3.CreateRotationZ((float)(cameradir.Z * Math.PI / 180.0f));

            Vector3 eyerel = Vector3.Transform(new Vector3(0, -distance, 0), transform);       // the 0,-E,0 sets the axis of the system..
            lookat = eyeposition - eyerel;      // note we go backwards, as the eyerel and camera is defined as the vector from eye to lookat.
        }


        // time <0 estimate, 0 instance >0 time
        public void GoTo(Vector3 gotopos, float timeslewsec = 0, float unitspersecond = 10000F)       // may pass a Nan Position - no action. Y is normal sense
        {
            if (!float.IsNaN(gotopos.X))
            {
                //System.Diagnostics.Debug.WriteLine("Goto " + normpos + " in " + timeslewsec + " at " + unitspersecond);

                double dist = Math.Sqrt((lookat.X - gotopos.X) * (lookat.X - gotopos.X) + (lookat.Y - gotopos.Y) * (lookat.Y - gotopos.Y) + (lookat.Z - gotopos.Z) * (lookat.Z - gotopos.Z));
                Debug.Assert(!double.IsNaN(dist));      // had a bug due to incorrect signs!

                if (dist >= 1)
                {
                    Vector3 eyeoffset = eyeposition - lookat;

                    if (timeslewsec == 0)
                    {
                        lookat = gotopos;
                        eyeposition = gotopos + eyeoffset;
                    }
                    else
                    {
                        targetposSlewPosition = gotopos;
                        targetposEyePosition = gotopos + eyeoffset;
                        targetposSlewProgress = 0.0f;
                        targetposSlewTime = (timeslewsec < 0) ? ((float)Math.Max(1.0, dist / unitspersecond)) : timeslewsec;            //10000 ly/sec, with a minimum slew
                        //System.Diagnostics.Debug.WriteLine("{0} Slew start to {1} in {2}",  Environment.TickCount % 10000 , targetposSlewPosition , targetposSlewTime);
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
                    lookat = targetposSlewPosition;
                    eyeposition = targetposEyePosition;
                    //System.Diagnostics.Debug.WriteLine("{0} Slew complete at {1}", Environment.TickCount % 10000, position);
                }
                else
                {
                    var slewstart = Math.Sin((targetposSlewProgress - 0.5) * Math.PI);
                    var slewend = Math.Sin((newprogress - 0.5) * Math.PI);
                    Debug.Assert((1 - 0 - slewstart) != 0);
                    var slewfact = (slewend - slewstart) / (1.0 - slewstart);

                    var totvector = new Vector3((float)(targetposSlewPosition.X - lookat.X), (float)(targetposSlewPosition.Y - lookat.Y), (float)(targetposSlewPosition.Z - lookat.Z));
                    lookat += Vector3.Multiply(totvector, (float)slewfact);
                    eyeposition += Vector3.Multiply(totvector, (float)slewfact);
                    //System.Diagnostics.Debug.WriteLine("{0} Slew to {1} prog {2}", Environment.TickCount % 10000, position, newprogress);
                }

                targetposSlewProgress = (float)newprogress;
            }
        }

        public bool Keyboard(KeyboardState kbd, bool inperspectivemode, Vector3 cameraDir, float distance, bool elitemovement)
        {
            Vector3 positionMovement = Vector3.Zero;

            if (kbd.Shift)
                distance *= 2.0F;

            if (kbd.IsAnyPressed(Keys.Left, Keys.A) != null)                // x axis
            {
                positionMovement.X = -distance;
            }
            else if (kbd.IsAnyPressed(Keys.Right, Keys.D) != null)
            {
                positionMovement.X = distance;
            }

            if (kbd.IsAnyPressed(Keys.PageUp, Keys.R) != null)              // y axis
            {
                positionMovement.Y = distance;
            }
            else if (kbd.IsAnyPressed(Keys.PageDown, Keys.F) != null)
            {
                positionMovement.Y = -distance;
            }

            if (kbd.IsAnyPressed(Keys.Up, Keys.W) != null)                  // z axis
            {
                positionMovement.Z = distance;
            }
            else if (kbd.IsAnyPressed(Keys.Down, Keys.S) != null)
            {
                positionMovement.Z = -distance;
            }

            if (positionMovement.LengthSquared > 0)
            {
                if (inperspectivemode)
                {
                    if (elitemovement)  // elite movement means only the camera rotation around the Y axis is taken into account. 
                    {
                        var cameramove = Matrix4.CreateTranslation(positionMovement);
                        var rotY = Matrix4.CreateRotationY(cameraDir.Y.Radians());      // rotate by Y, which is rotation around the Y axis, which is where your looking at horizontally
                        cameramove *= rotY;
                        Translate(cameramove.ExtractTranslation());
                    }
                    else
                    {
                        // we need to rotate components to get the X/Y/Z into the same meaning as the camera angles.
                        var cameramove = Matrix4.CreateTranslation(new Vector3(positionMovement.X,positionMovement.Z,-positionMovement.Y));
                        cameramove *= Matrix4.CreateRotationZ(cameraDir.Z.Radians());   // rotate the translation by the camera look angle
                        cameramove *= Matrix4.CreateRotationX(cameraDir.X.Radians());
                        cameramove *= Matrix4.CreateRotationY(cameraDir.Y.Radians());
                        Translate(cameramove.ExtractTranslation());
                    }
                }
                else
                {
                    positionMovement.Y = 0;
                    Translate(positionMovement);
                }

                return true;
            }
            else
                return false;
        }


    }
}

