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

using OpenTK;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace OpenTKUtils.Common
{
    public class PositionCamera       // holds lookat and eyepositions and camera
    {
        #region Positions

        public Vector3 Lookat { get { return lookat; } set { KillSlew(); float d = EyeDistance; lookat = value; SetEyePositionFromLookat(cameradir, d);} }
        public Vector3 EyePosition { get { return eyeposition; } set { KillSlew(); float d = EyeDistance; eyeposition = value; SetLookatPositionFromEye(cameradir, d); } }

        public float EyeDistance { get { return (lookat - EyePosition).Length; } }

        public void Translate(Vector3 pos)
        {
            KillSlew();
            lookat += pos;
            eyeposition += pos;
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

        #endregion

        #region Camera

        // camera.x rotates around X, counterclockwise, = 0 (up), 90 = (forward), 180 (down)
        // camera.y rotates around Y, counterclockwise, = 0 (forward), 90 = (to left), 180 (back), -90 (to right)
        public Vector2 CameraDirection { get { return cameradir; } set { KillSlew(); cameradir = value; SetLookatPositionFromEye(value, EyeDistance); } }
        
        public float CameraRotation { get { return camerarot; } set { KillSlew(); camerarot = value; } }       // rotation around Z

        public void RotateCamera(Vector2 addazel, float addzrot, bool movepos)
        {
            KillSlew();
            System.Diagnostics.Debug.Assert(!float.IsNaN(addazel.X) && !float.IsNaN(addazel.Y));
            Vector2 cameraDir = CameraDirection;

            cameraDir.X = cameraDir.X.AddBoundedAngle(addazel.X);
            cameraDir.Y = cameraDir.Y.AddBoundedAngle(addazel.Y);

            if (cameraDir.X < 0 && cameraDir.X > -90)                   // Limit camera pitch
                cameraDir.X = 0;
            if (cameraDir.X > 180 || cameraDir.X <= -90)
                cameraDir.X = 180;

            camerarot = camerarot.AddBoundedAngle(addzrot);

            if (movepos)
                SetLookatPositionFromEye(cameraDir, EyeDistance);
            else
                SetEyePositionFromLookat(cameraDir, EyeDistance);
        }

        public void Pan(Vector2 newcamerapos, float timeslewsec = 0) 
        {
            if (timeslewsec == 0)
            {
                SetLookatPositionFromEye(newcamerapos, EyeDistance);
            }
            else
            {
                cameraDirSlewPosition = newcamerapos;
                cameraDirSlewProgress = 0.0f;
                cameraDirSlewTime = (timeslewsec == 0) ? (1.0F) : timeslewsec;
            }
        }

        public void PanTo(Vector3 target, float time = 0)            
        {
            Vector2 camera = EyePosition.AzEl(target, true);
            Pan(camera, time);
        }

        public void PanZoomTo(Vector3 target, float zoom, float time = 0) 
        {
            Vector2 camera = EyePosition.AzEl(target, true);
            Pan(camera, time);
            GoToZoom(zoom, time);
        }

        #endregion

        #region Zoom

        public float ZoomFactor { get { return Zoom1Distance / EyeDistance; } set { KillSlew(); Zoom(value); } }
        public float Zoom1Distance { get; set; } = 1000F;                     // distance that Current=1 will be from the Position, in the direction of the camera.
        public float ZoomMax = 300F;            // Default out Current
        public float ZoomMin = 0.01F;           // Iain special ;-) - this depends on znear (the clip distance - smaller you can Current in more) and Zoomdistance.
        public float ZoomScaling = 1.258925F;      // scaling

        public void ZoomScale(bool direction)
        {
            KillSlew();
            float newzoomfactor = ZoomFactor;
            if (direction)
            {
                newzoomfactor *= (float)ZoomScaling;
                if (newzoomfactor > ZoomMax)
                    newzoomfactor = (float)ZoomMax;
            }
            else
            {
                newzoomfactor /= (float)ZoomScaling;
                if (newzoomfactor < ZoomMin)
                    newzoomfactor = (float)ZoomMin;
            }

            Zoom(newzoomfactor);
        }

        public void GoToZoom(float z, float timetozoom = 0)        // <0 means auto estimate
        {
            z = Math.Max(Math.Min(z, ZoomMax), ZoomMin);

            if (timetozoom == 0)
            {
                Zoom(z);
            }
            else if (Math.Abs(z - ZoomFactor) > 0.001)
            {
                zoomSlewTarget = z;
                zoomSlewStart = ZoomFactor;

                if (timetozoom < 0)       // auto estimate on log distance between them
                {
                    timetozoom = (float)(Math.Abs(Math.Log10(zoomSlewTarget / ZoomFactor)) * 1.5);
                }

                zoomSlewTimeToZoom = timetozoom;
            }
        }

        private void Zoom(float newzoomfactor)
        {
            newzoomfactor = Math.Max(Math.Min(newzoomfactor, ZoomMax), ZoomMin);
            SetEyePositionFromLookat(CameraDirection, Zoom1Distance / newzoomfactor);
        }

        #endregion

        #region Position functions

        private Vector3 cameravector = new Vector3(0, 1, 0);        // camera vector, at CameraDir(0,0)

        public void SetPosition(Vector3 lookp, Vector3 eyeposp, float camerarotp = 0)     // set lookat/eyepos, rotation
        {
            lookat = lookp;
            eyeposition = eyeposp;
            camerarot = camerarotp;
            cameradir = eyeposition.AzEl(lookat, true);
        }

        public void SetPositionDistance(Vector3 lookp, Vector2 cameradirp, float distance, float camerarotp = 0)     // set lookat, cameradir, zoom from, rotation
        {
            lookat = lookp;
            cameradir = cameradirp;
            camerarot = camerarotp;
            SetEyePositionFromLookat(cameradir, distance);
        }

        public void SetPositionZoom(Vector3 lookp, Vector2 cameradirp, float zoom, float camerarotp = 0)     // set lookat, cameradir, zoom from, rotation
        {
            lookat = lookp;
            cameradir = cameradirp;
            camerarot = camerarotp;
            SetEyePositionFromLookat(cameradir, Zoom1Distance / zoom);
        }

        public void SetEyePositionFromLookat(Vector2 cameradir, float distance)              // from current lookat, set eyeposition, given a camera angle and a distance
        {
            Matrix3 transform = Matrix3.Identity;                   // identity nominal matrix, dir is in degrees

            transform *= Matrix3.CreateRotationX((float)(cameradir.X * Math.PI / 180.0f));      // we rotate the camera vector around X and Y to get a vector which points from eyepos to lookat pos
            transform *= Matrix3.CreateRotationY((float)(cameradir.Y * Math.PI / 180.0f));

            Vector3 eyerel = Vector3.Transform(cameravector, transform);       // the 0,1,0 sets the axis of the camera dir

            eyeposition = lookat - eyerel * distance;
            this.cameradir = cameradir;
        }

        public void SetLookatPositionFromEye(Vector2 cameradir, float distance)              // from current eye position, set lookat, given a camera angle and a distance
        {
            Matrix3 transform = Matrix3.Identity;                   // identity nominal matrix, dir is in degrees

            transform *= Matrix3.CreateRotationX((float)(cameradir.X * Math.PI / 180.0f));      // we rotate the camera vector around X and Y to get a vector which points from eyepos to lookat pos
            transform *= Matrix3.CreateRotationY((float)(cameradir.Y * Math.PI / 180.0f));

            Vector3 eyerel = Vector3.Transform(cameravector, transform);       // the 0,1,0 sets the axis of the camera dir

            lookat = eyeposition + eyerel * distance;      
            this.cameradir = cameradir;
        }

        #endregion

        #region Slew

        public bool InSlew { get { return (targetposSlewProgress < 1.0f || zoomSlewTarget > 0 || cameraDirSlewProgress < 1.0f); } }

        public void KillSlew()
        {
            targetposSlewProgress = 1.0f;
            zoomSlewTarget = 0;
            cameraDirSlewProgress = 1.0f;
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

            if ( zoomSlewTarget > 0 )
            {
                int wantedsteps = (int)((zoomSlewTimeToZoom * 1000.0F) / msticks);
                float zoommultiplier = (float)Math.Pow(10.0, Math.Log10(zoomSlewTarget / zoomSlewStart) / wantedsteps);      // I.S^n = F I = initial, F = final, S = scaling, N = no of steps

                float newzoom = (float)(ZoomFactor * zoommultiplier);
                System.Diagnostics.Debug.WriteLine("Zoom {0} -> {1} {2}", ZoomFactor, newzoom, zoommultiplier);
                bool stop = (zoomSlewTarget > ZoomFactor) ? (newzoom >= zoomSlewTarget) : (newzoom <= zoomSlewTarget);

                if (stop)
                {
                    newzoom = zoomSlewTarget;
                    zoomSlewTarget = 0;
                }

                SetEyePositionFromLookat(CameraDirection, Zoom1Distance / newzoom);
            }

            if (cameraDirSlewProgress < 1.0f)
            {
                var newprogress = cameraDirSlewProgress + msticks / (cameraDirSlewTime * 1000);

                if (newprogress >= 1.0f)
                {
                    SetLookatPositionFromEye(cameraDirSlewPosition, EyeDistance);
                }
                else
                {
                    var slewstart = Math.Sin((cameraDirSlewProgress - 0.5) * Math.PI);
                    var slewend = Math.Sin((newprogress - 0.5) * Math.PI);
                    Debug.Assert((1 - 0 - slewstart) != 0);
                    var slewfact = (slewend - slewstart) / (1.0 - slewstart);

                    var totvector = new Vector2((float)(cameraDirSlewPosition.X - CameraDirection.X), (float)(cameraDirSlewPosition.Y - CameraDirection.Y));
                    cameradir += Vector2.Multiply(totvector, (float)slewfact);
                    SetLookatPositionFromEye(cameradir, EyeDistance);
                }

                cameraDirSlewProgress = (float)newprogress;
            }
        }

        #endregion

        #region Different tracker

        private Vector3 lastlookat;
        private Vector3 lasteyepos;
        private float lastcamerarotation;

        public void ResetDifferenceTracker()
        {
            lasteyepos = EyePosition;
            lastlookat = Vector3.Zero;
            lastcamerarotation = 0;
        }

        public bool IsMoved(float minmovement = 0.1f, float cameramove = 1.0f)
        {
            bool moved = Vector3.Subtract(lastlookat, Lookat).Length >= minmovement;
            if (moved)
                lastlookat = Lookat;
            bool eyemoved = Vector3.Subtract(lasteyepos, EyePosition).Length >= minmovement;
            if (eyemoved)
                lasteyepos = EyePosition;
            bool rotated = Math.Abs(CameraRotation - lastcamerarotation) >= cameramove;
            if (rotated)
                lastcamerarotation = CameraRotation;
            return moved | eyemoved | rotated;
        }

        #endregion

        #region UI

        public bool PositionKeyboard(KeyboardMonitor kbd, bool inperspectivemode, float movedistance, bool elitemovement)
        {
            Vector3 positionMovement = Vector3.Zero;

            if (kbd.Shift)
                movedistance *= 2.0F;

            if (kbd.IsCurrentlyPressed(Keys.Left, Keys.A) != null)                // x axis
            {
                positionMovement.X = -movedistance;
            }
            else if (kbd.IsCurrentlyPressed(Keys.Right, Keys.D) != null)
            {
                positionMovement.X = movedistance;
            }

            if (kbd.IsCurrentlyPressed(Keys.PageUp, Keys.R) != null)              // y axis
            {
                positionMovement.Y = movedistance;
            }
            else if (kbd.IsCurrentlyPressed(Keys.PageDown, Keys.F) != null)
            {
                positionMovement.Y = -movedistance;
            }

            if (kbd.IsCurrentlyPressed(Keys.Up, Keys.W) != null)                  // z axis
            {
                positionMovement.Z = movedistance;
            }
            else if (kbd.IsCurrentlyPressed(Keys.Down, Keys.S) != null)
            {
                positionMovement.Z = -movedistance;
            }

            if (positionMovement.LengthSquared > 0)
            {
                if (inperspectivemode)
                {
                    Vector2 cameraDir = CameraDirection;

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

                        
                        
                        // TBD

                        var cameramove = Matrix4.CreateTranslation(new Vector3(positionMovement.X,positionMovement.Z,-positionMovement.Y));
                        cameramove *= Matrix4.CreateRotationZ(CameraRotation.Radians());   // rotate the translation by the camera look angle
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

        public bool CameraKeyboard(KeyboardMonitor kbd, float angle)
        {
            Vector3 cameraActionRotation = Vector3.Zero;

            if (kbd.Shift)
                angle *= 2.0F;

            if (kbd.IsCurrentlyPressed(Keys.NumPad4) != null)
            {
                cameraActionRotation.Z = angle;
            }
            if (kbd.IsCurrentlyPressed(Keys.NumPad6) != null)
            {
                cameraActionRotation.Z = -angle;
            }

            if (kbd.IsCurrentlyPressed(Keys.NumPad5, Keys.NumPad2, Keys.Z) != null)
            {
                cameraActionRotation.X = -angle;
            }
            if (kbd.IsCurrentlyPressed(Keys.NumPad8, Keys.X) != null)
            {
                cameraActionRotation.X = angle;
            }

            bool movepos = false;

            if (kbd.IsCurrentlyPressed(KeyboardMonitor.ShiftState.Ctrl, Keys.Q))
            {
                cameraActionRotation.Y = angle;
            }
            else if (kbd.IsCurrentlyPressed(Keys.NumPad7, Keys.Q) != null)
            {
                cameraActionRotation.Y = -angle;
                movepos = true;
            }

            if (kbd.IsCurrentlyPressed(KeyboardMonitor.ShiftState.Ctrl, Keys.E))
            {
                cameraActionRotation.Y = -angle;
            }
            else if (kbd.IsCurrentlyPressed(Keys.NumPad9, Keys.E) != null)
            {
                cameraActionRotation.Y = angle;
                movepos = true;
            }

            if (cameraActionRotation.LengthSquared > 0)
            {
                RotateCamera(new Vector2(cameraActionRotation.X, cameraActionRotation.Y), cameraActionRotation.Z, movepos);
                return true;
            }
            else
                return false;
        }


        public bool ZoomKeyboard(KeyboardMonitor kbd, float adjustment)
        {
            bool changed = false;

            if (kbd.IsCurrentlyPressed(KeyboardMonitor.ShiftState.None, Keys.Add, Keys.M))
            {
                Zoom(adjustment);
                changed = true;
            }

            if (kbd.IsCurrentlyPressed(KeyboardMonitor.ShiftState.None, Keys.Subtract, Keys.N))
            {
                Zoom(1.0f / adjustment);
                changed = true;
            }

            float newzoom = 0;

            if (kbd.HasBeenPressed(Keys.D1))
                newzoom = ZoomMax;
            if (kbd.HasBeenPressed(Keys.D2))
                newzoom = 100;                                                      // Factor 3 scale
            if (kbd.HasBeenPressed(Keys.D3))
                newzoom = 33;
            if (kbd.HasBeenPressed(Keys.D4))
                newzoom = 11F;
            if (kbd.HasBeenPressed(Keys.D5))
                newzoom = 3.7F;
            if (kbd.HasBeenPressed(Keys.D6))
                newzoom = 1.23F;
            if (kbd.HasBeenPressed(Keys.D7))
                newzoom = 0.4F;
            if (kbd.HasBeenPressed(Keys.D8))
                newzoom = 0.133F;
            if (kbd.HasBeenPressed(Keys.D9))
                newzoom = ZoomMin;

            if (newzoom != 0)
            {
                GoToZoom(newzoom, -1);
                changed = true;
            }

            return changed;
        }

        #endregion

        #region Privates

        private Vector3 lookat = Vector3.Zero;                // point where we are viewing. 
        private Vector3 eyeposition = new Vector3(10, 10, 10);  // and the eye position
        private Vector2 cameradir = Vector2.Zero;               // camera dir, kept in track
        private float camerarot = 0;                            // and rotation

        private float targetposSlewProgress = 1.0f;             // 0 -> 1 slew progress
        private float targetposSlewTime;                        // how long to take to do the slew
        private Vector3 targetposSlewPosition;                  // where to slew to.
        private Vector3 targetposEyePosition;                   // where to slew to.

        private float zoomSlewStart = 0;
        private float zoomSlewTarget = 0;
        private float zoomSlewTimeToZoom = 0;

        private float cameraDirSlewProgress = 1.0f;             // 0 -> 1 slew progress
        private float cameraDirSlewTime;                        // how long to take to do the slew
        private Vector2 cameraDirSlewPosition;                  // where to slew to.

        #endregion
    }
}

