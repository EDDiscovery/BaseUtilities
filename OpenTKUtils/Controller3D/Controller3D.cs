/*
 * Copyright © 2015 - 2019 EDDiscovery development team
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
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace OpenTKUtils.Common
{
    // class brings together keyboard, mouse, posdir, zoom to provide a means to move thru the playfield and zoom.
    // handles keyboard actions and mouse actions to provide a nice method of controlling the 3d playfield
    // Attaches to a GLWindowControl and hooks its events to provide control

    public class Controller3D
    {
        public float ZoomDistance { get { return Zoom.Zoom1Distance; } set { Zoom.Zoom1Distance = value; } }

        private GLWindowControl glwin;
      
        public float ProjectionZNear { get; private set; }

        public Func<int, float, float> KeyboardTravelSpeed;                     // optional set to scale travel key commands given this time interval and camera distance
        public Func<int, float> KeyboardRotateSpeed;                            // optional set to scale camera key rotation commands given this time interval
        public Func<int, float> KeyboardZoomSpeed;                              // optional set to scale zoom speed commands given this time interval
        public float MouseRotateAmountPerPixel { get; set; } = 0.25f;           // mouse speeds, degrees/pixel
        public float MouseUpDownAmountAtZoom1PerPixel { get; set; } = 0.5f;     // per pixel movement at zoom 1 (zoom scaled)
        public float MouseTranslateAmountAtZoom1PerPixel { get; set; } = 2.0f;  // per pixel movement at zoom 1
        public bool EliteMovement { get; set; } = true;

        public Action<GLMatrixCalc, long> PaintObjects;       // madatory if you actually want to see anything

        public int LastHandleInterval;                      // set after handlekeyboard, how long since previous one was handled in ms

        public GLMatrixCalc MatrixCalc { get; private set; } = new GLMatrixCalc();
        public Zoom Zoom { get; private set; } = new Zoom();
        public Position Pos { get; private set; } = new Position();
        public Camera Camera { get; private set; } = new Camera();
        public Fov Fov { get; private set; } = new Fov();

        public CameraDirectionMovementTracker MovementTracker { get; set; } = new CameraDirectionMovementTracker();        // these track movements and zoom

        public void Start(GLWindowControl win, Vector3 lookat, Vector3 cameradir, float zoomn)
        {
            glwin = win;

            win.Resize += GlControl_Resize;
            win.Paint += glControl_Paint;
            win.MouseDown += glControl_MouseDown;
            win.MouseUp += glControl_MouseUp;
            win.MouseMove += glControl_MouseMove;
            win.MouseWheel += glControl_MouseWheel;
            win.KeyDown += glControl_KeyDown;
            win.KeyUp += glControl_KeyUp;

            Pos.Lookat = lookat;
            Camera.Set(cameradir);
            Zoom.Current = zoomn;
            Pos.SetEyePositionFromLookat(Camera.Current, Zoom.EyeDistance);
            MovementTracker.Update(Camera.Current, Pos.Lookat, Zoom.Current); // set up here so ready for action.. below uses it.

            MatrixCalc.CalculateModelMatrix(Pos.Lookat, Pos.EyePosition, Camera.Normal, glwin.Width, glwin.Height);
            SetModelProjectionMatrixViewPort();

            sysinterval.Start();
        }

        // Pos Direction interface
        // don't want direct class access, via this wrapper
        public void SetPosition(Vector3 posx) { Pos.Lookat = posx; }
        public void TranslatePosition(Vector3 posx) { Pos.Translate(posx); }
        public void SlewToPosition(Vector3 normpos, float timeslewsec = 0, float unitspersecond = 10000F) { Pos.GoTo(normpos, timeslewsec, unitspersecond); }

        public void SetCameraDir(Vector3 pos) { Camera.Set(pos); }
        public void RotateCameraDir(Vector3 rot) { Camera.Rotate(rot); }
        public void StartCameraPan(Vector3 pos, float timeslewsec = 0) { Camera.Pan(pos, timeslewsec); }
        public void CameraLookAt(Vector3 normtarget, float zoom, float time = 0, float unitspersecond = 1000F)
        { Pos.GoTo(normtarget, time, unitspersecond); Camera.LookAt(MatrixCalc.EyePosition, normtarget, zoom, time); }

        public void KillSlews() { Pos.KillSlew(); Camera.KillSlew(); Zoom.KillSlew(); }

        // Zoom
        public void StartZoom(float z, float timetozoom = 0) { Zoom.GoTo(z, timetozoom); }

        // perspective.. use this don't just change the matrixcalc.
        public void ChangePerspectiveMode(bool on)
        {
            MatrixCalc.InPerspectiveMode = on;
            MatrixCalc.CalculateModelMatrix(Pos.Lookat, Pos.EyePosition, Camera.Normal, glwin.Width, glwin.Height);
            SetModelProjectionMatrixViewPort();
            glwin.Invalidate();
        }

        // Redraw scene, something has changed

        public void Redraw() { glwin.Invalidate(); }            // invalidations causes a glControl_Paint

        public long Redraw(int times)                               // for testing, redraw the scene N times and give ms 
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < times; i++)
                glControl_Paint(null);
            long time = sw.ElapsedMilliseconds;
            sw.Stop();
            return time;
        }
        
        // Owner should call this at regular intervals.
        // handle keyboard, indicate if activated, handle other keys if required, return movement calculated in case you need to use it

        public CameraDirectionMovementTracker HandleKeyboardSlews(bool activated, Action<BaseUtils.KeyboardState> handleotherkeys = null)
        {
            long elapsed = sysinterval.ElapsedMilliseconds;         // stopwatch provides precision timing on last paint time.
            LastHandleInterval = (int)(elapsed - lastintervalcount);
            lastintervalcount = elapsed;

            if (activated && glwin.Focused)                      // if we can accept keys
            {
                if (MatrixCalc.InPerspectiveMode)       // camera rotations are only in perspective mode
                {
                    var ca = Camera.Keyboard(keyboard, KeyboardRotateSpeed?.Invoke(LastHandleInterval) ?? (0.02f * LastHandleInterval));

                    if (ca != Camera.KeyboardAction.None)      // moving the camera around kills the pos slew (as well as its own slew)
                    {
                        Pos.KillSlew();
                        if (ca == Camera.KeyboardAction.MovePosition)
                            Pos.SetLookatPositionFromEye(Camera.Current, Zoom.EyeDistance);
                        else
                            Pos.SetEyePositionFromLookat(Camera.Current, Zoom.EyeDistance);

                    }
                }

                if (Pos.Keyboard(keyboard, MatrixCalc.InPerspectiveMode, Camera.Current, KeyboardTravelSpeed?.Invoke(LastHandleInterval, MatrixCalc.EyeDistance) ?? (0.1f*LastHandleInterval), EliteMovement))
                    Camera.KillSlew();              // moving the pos around kills the camera slew (as well as its own slew)

                if ( Zoom.Keyboard(keyboard, KeyboardZoomSpeed?.Invoke(LastHandleInterval) ?? (1.0f + ((float)LastHandleInterval * 0.002f))))      // zoom slew is not affected by the above
                {
                    Pos.SetEyePositionFromLookat(Camera.Current, Zoom.EyeDistance);
                }

                if (keyboard.IsPressedRemove(Keys.M, BaseUtils.KeyboardState.ShiftState.Ctrl))
                    EliteMovement = !EliteMovement;

                if (keyboard.IsPressedRemove(Keys.P, BaseUtils.KeyboardState.ShiftState.Ctrl))
                    ChangePerspectiveMode(!MatrixCalc.InPerspectiveMode);

                handleotherkeys?.Invoke(keyboard);
            }
            else
            {
                keyboard.Reset();
            }

            Pos.DoSlew(LastHandleInterval);     // changes here will be picked up by AnythingChanged

            bool recalceye = Camera.DoSlew(LastHandleInterval);

            recalceye |= Zoom.DoSlew();

            if ( recalceye )
                Pos.SetEyePositionFromLookat(Camera.Current, Zoom.EyeDistance);

            MovementTracker.Update(Camera.Current, Pos.Lookat, Zoom.Current);       // Gross limit allows you not to repaint due to a small movement. I've set it to all the time for now, prefer the smoothness to the frame rate.

            if (MovementTracker.AnythingChanged)
            {
                //System.Diagnostics.Debug.WriteLine("Changed");
                MatrixCalc.CalculateModelMatrix(Pos.Lookat, Pos.EyePosition, Camera.Normal, glwin.Width, glwin.Height);
                glwin.Invalidate();
            }

            return MovementTracker;
        }

        #region Implementation

        private void GlControl_Resize(object sender)           // there was a gate in the original around OnShown.. not sure why.
        {
            SetModelProjectionMatrixViewPort();
            glwin.Invalidate();
        }

        private void SetModelProjectionMatrixViewPort()
        {
            MatrixCalc.CalculateProjectionMatrix(Fov.Current, glwin.Width, glwin.Height, out float zn);
            ProjectionZNear = zn;
        }

        // Paint the scene - just pass the call down to the installed PaintObjects

        private void glControl_Paint(Object obj)
        {
            PaintObjects?.Invoke(MatrixCalc, sysinterval.ElapsedMilliseconds);
        }

        private void glControl_MouseDown(object sender, GLMouseEventArgs e)
        {
            KillSlews();

            mouseDownPos.X = e.X;
            mouseDownPos.Y = e.Y;

            if (e.Button.HasFlag(GLMouseEventArgs.MouseButtons.Left))
            {
                mouseStartRotate.X = e.X;
                mouseStartRotate.Y = e.Y;
            }

            if (e.Button.HasFlag(GLMouseEventArgs.MouseButtons.Right))
            {
                mouseStartTranslateXY.X = e.X;
                mouseStartTranslateXY.Y = e.Y;
                mouseStartTranslateXZ.X = e.X;
                mouseStartTranslateXZ.Y = e.Y;
            }
        }

        private void glControl_MouseUp(object sender, GLMouseEventArgs e)
        {
            bool notmovedmouse = Math.Abs(e.X - mouseDownPos.X) + Math.Abs(e.Y - mouseDownPos.Y) < 4;

            if (!notmovedmouse)     // if we moved it, its not a stationary click, ignore
                return;

            if (e.Button == GLMouseEventArgs.MouseButtons.Right)                    // right clicks are about bookmarks.
            {
                mouseStartTranslateXY = new Point(int.MinValue, int.MinValue);         // indicate rotation is finished.
                mouseStartTranslateXZ = new Point(int.MinValue, int.MinValue);
            }
        }

        private void glControl_MouseMove(object sender, GLMouseEventArgs e)
        {
            if (e.Button == GLMouseEventArgs.MouseButtons.Left)
            {
                if (MatrixCalc.InPerspectiveMode && mouseStartRotate.X != int.MinValue) // on resize double click resize, we get a stray mousemove with left, so we need to make sure we actually had a down event
                {
                    KillSlews();
                    int dx = e.X - mouseStartRotate.X;
                    int dy = e.Y - mouseStartRotate.Y;

                    mouseStartRotate.X = mouseStartTranslateXZ.X = e.X;
                    mouseStartRotate.Y = mouseStartTranslateXZ.Y = e.Y;

                    Camera.Rotate(new Vector3((float)(dy * MouseRotateAmountPerPixel), (float)(dx * MouseRotateAmountPerPixel ), 0));
                    Pos.SetEyePositionFromLookat(Camera.Current, Zoom.EyeDistance);
                }
            }
            else if (e.Button == GLMouseEventArgs.MouseButtons.Right)
            {
                if (mouseStartTranslateXY.X != int.MinValue)
                {
                    KillSlews();

                    int dx = e.X - mouseStartTranslateXY.X;
                    int dy = e.Y - mouseStartTranslateXY.Y;

                    mouseStartTranslateXY.X = mouseStartTranslateXZ.X = e.X;
                    mouseStartTranslateXY.Y = mouseStartTranslateXZ.Y = e.Y;
                    //System.Diagnostics.Trace.WriteLine("dx" + dx.ToString() + " dy " + dy.ToString() + " Button " + e.Button.ToString());

                    Pos.Translate(new Vector3(0, -dy * (1.0f / Zoom.Current) * MouseUpDownAmountAtZoom1PerPixel, 0));
                }
            }
            else if (e.Button == (GLMouseEventArgs.MouseButtons.Left | GLMouseEventArgs.MouseButtons.Right))
            {
                if (mouseStartTranslateXZ.X != int.MinValue)
                {
                    KillSlews();

                    int dx = e.X - mouseStartTranslateXZ.X;
                    int dy = e.Y - mouseStartTranslateXZ.Y;

                    mouseStartTranslateXZ.X = mouseStartRotate.X = mouseStartTranslateXY.X = e.X;
                    mouseStartTranslateXZ.Y = mouseStartRotate.Y = mouseStartTranslateXY.Y = e.Y;
                    Vector3 translation = new Vector3(dx * (1.0f / Zoom.Current) * MouseTranslateAmountAtZoom1PerPixel, -dy * (1.0f / Zoom.Current) * MouseTranslateAmountAtZoom1PerPixel, 0.0f);

                    if (MatrixCalc.InPerspectiveMode)
                    {
                        //System.Diagnostics.Trace.WriteLine("dx" + dx.ToString() + " dy " + dy.ToString() + " Button " + e.Button.ToString());

                        Matrix3 transform = Matrix3.CreateRotationZ((float)(-Camera.Current.Y * Math.PI / 180.0f));
                        translation = Vector3.Transform(translation, transform);

                        Pos.Translate(new Vector3(translation.X, 0, translation.Y));
                    }
                    else
                        Pos.Translate(new Vector3(translation.X, 0, translation.Y));
                }
            }

        }

        private void glControl_MouseWheel(object sender, GLMouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                if (keyboard.Ctrl)
                {
                    if (Fov.Scale(e.Delta < 0))
                    {
                        SetModelProjectionMatrixViewPort();
                        glwin.Invalidate();
                    }
                }
                else
                {
                    Zoom.Scale(e.Delta > 0);
                    Pos.SetEyePositionFromLookat(Camera.Current, Zoom.EyeDistance);
                }
            }
        }

        private void glControl_KeyDown(object sender, GLKeyEventArgs e)
        {
            keyboard.KeyDown(e.Control, e.Shift, e.Alt, e.KeyCode);
        }

        private void glControl_KeyUp(object sender, GLKeyEventArgs e)
        {
            keyboard.KeyUp(e.Control, e.Shift, e.Alt, e.KeyCode);
        }


        private BaseUtils.KeyboardState keyboard = new BaseUtils.KeyboardState();        // needed to be held because it remembers key downs

        private Stopwatch sysinterval = new Stopwatch();    // to accurately measure interval between system ticks
        private long lastintervalcount = 0;                   // last update tick at previous update

        private Point mouseDownPos;
        private Point mouseStartRotate = new Point(int.MinValue, int.MinValue);        // used to indicate not started for these using mousemove
        private Point mouseStartTranslateXZ = new Point(int.MinValue, int.MinValue);
        private Point mouseStartTranslateXY = new Point(int.MinValue, int.MinValue);

        #endregion
    }
}
