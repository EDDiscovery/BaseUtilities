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

    public class Controller3D
    {
        public OpenTK.GLControl glControl { get; private set; }      // use to draw to

        public float zNear { get; private set; }                     // model znear

        public Func<int, float> KeyboardTravelSpeed;                            // optional set to scale travel key commands given this time interval
        public Func<int, float> KeyboardRotateSpeed;                            // optional set to scale camera key rotation commands given this time interval
        public Func<int, float> KeyboardZoomSpeed;                              // optional set to scale zoom speed commands given this time interval
        public float MouseRotateAmountPerPixel { get; set; } = 0.25f;           // mouse speeds, degrees/pixel
        public float MouseUpDownAmountAtZoom1PerPixel { get; set; } = 0.5f;     // per pixel movement at zoom 1 (zoom scaled)
        public float MouseTranslateAmountAtZoom1PerPixel { get; set; } = 2.0f;  // per pixel movement at zoom 1
        public bool EliteMovement { get; set; } = true;

        public Color BackColour { get; set; } = (Color)System.Drawing.ColorTranslator.FromHtml("#0D0D10");

        public Action<MatrixCalc, long> PaintObjects;       // madatory if you actually want to see anything

        public Action<MouseEventArgs> MouseDown;            // optional - set to handle more mouse actions if required
        public Action<MouseEventArgs> MouseUp;
        public Action<MouseEventArgs> MouseMove;
        public Action<MouseEventArgs> MouseWheel;

        public int LastHandleInterval;                      // set after handlekeyboard, how long since previous one was handled in ms

        public MatrixCalc MatrixCalc { get; private set; } = new MatrixCalc();
        public Zoom Zoom { get; private set; } = new Zoom();
        public Position Pos { get; private set; } = new Position();
        public Camera Camera { get; private set; } = new Camera();

        public CameraDirectionMovementTracker MovementTracker { get; set; } = new CameraDirectionMovementTracker();        // these track movements and zoom

        public void CreateGLControl()
        {
            this.glControl = new GLControl();
            this.glControl.Dock = DockStyle.Fill;
            this.glControl.BackColor = System.Drawing.Color.Black;
            this.glControl.Name = "glControl";
            this.glControl.TabIndex = 0;
            this.glControl.VSync = true;
            this.glControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseDown);
            this.glControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseMove);
            this.glControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseUp);
            this.glControl.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.glControl_OnMouseWheel);
            this.glControl.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl_Paint);
            this.glControl.KeyDown += new KeyEventHandler(keyboard.KeyDown);
            this.glControl.KeyUp += new KeyEventHandler(keyboard.KeyUp);
            this.glControl.Resize += GlControl_Resize;
        }

        public void Start(Vector3 lookat, Vector3 cameradir, float zoomn)
        {
            Pos.Lookat = lookat;
            Camera.Set(cameradir);
            Zoom.Default = zoomn;
            Zoom.SetDefault();

            MovementTracker.Update(Camera.Current, Pos.Lookat, this.Zoom.Current); // set up here so ready for action.. below uses it.
            SetModelProjectionMatrix();

            GL.ClearColor(BackColour);

            GL.Enable(EnableCap.DepthTest);         // standard - depth
            GL.FrontFace(FrontFaceDirection.Ccw);
            GLStatics.CullFace(true);               // cull faces, ccw.
            GLStatics.PointSize(1);                 // default is controlled by external not shaders

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

        // Redraw scene, something has changed

        public void Redraw() { glControl.Invalidate(); }            // invalidations causes a glControl_Paint

        public long Redraw(int times)                               // for testing, redraw the scene N times and give ms 
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < times; i++)
                glControl_Paint(null, null);
            long time = sw.ElapsedMilliseconds;
            sw.Stop();
            return time;
        }

        // Owner should call this at regular intervals.
        // handle keyboard, indicate if activated, handle other keys if required, return movement calculated in case you need to use it

        public CameraDirectionMovementTracker HandleKeyboard(bool activated, Action<BaseUtils.KeyboardState> handleotherkeys = null)
        {
            long elapsed = sysinterval.ElapsedMilliseconds;         // stopwatch provides precision timing on last paint time.
            LastHandleInterval = (int)(elapsed - lastintervalcount);
            lastintervalcount = elapsed;

            if (activated && glControl.Focused)                      // if we can accept keys
            {
                if (Camera.Keyboard(keyboard, KeyboardRotateSpeed?.Invoke(LastHandleInterval) ?? (0.07f*LastHandleInterval)))      // moving the camera around kills the pos slew (as well as its own slew)
                    Pos.KillSlew();

                if (Pos.Keyboard(keyboard, MatrixCalc.InPerspectiveMode, Camera.Current, KeyboardTravelSpeed?.Invoke(LastHandleInterval) ?? (0.1f*LastHandleInterval), EliteMovement))
                    Camera.KillSlew();              // moving the pos around kills the camera slew (as well as its own slew)

                Zoom.Keyboard(keyboard, KeyboardZoomSpeed?.Invoke(LastHandleInterval) ?? (1.0f + ((float)LastHandleInterval * 0.002f)));      // zoom slew is not affected by the above

                if (keyboard.IsPressedRemove(Keys.M, BaseUtils.KeyboardState.ShiftState.Ctrl))
                    EliteMovement = !EliteMovement;

                handleotherkeys?.Invoke(keyboard);
            }
            else
            {
                keyboard.Reset();
            }

            Pos.DoSlew(LastHandleInterval);
            Camera.DoSlew(LastHandleInterval);
            Zoom.DoSlew();

            MovementTracker.Update(Camera.Current, Pos.Lookat, Zoom.Current);       // Gross limit allows you not to repaint due to a small movement. I've set it to all the time for now, prefer the smoothness to the frame rate.

            if (MovementTracker.AnythingChanged)
            {
                MatrixCalc.CalculateModelMatrix(Pos.Lookat, Camera.Current, Zoom.Current);
                //System.Diagnostics.Debug.WriteLine("Moved " + pos.Current + " " + camera.Current);
                glControl.Invalidate();
            }

            return MovementTracker;
        }

        #region Implementation

        private void GlControl_Resize(object sender, EventArgs e)           // there was a gate in the original around OnShown.. not sure why.
        {
            SetModelProjectionMatrix();
            glControl.Invalidate();
        }

        private void SetModelProjectionMatrix()
        {
            MatrixCalc.CalculateProjectionMatrix(fov.Current, glControl.Width, glControl.Height, out float zn);
            zNear = zn;
            MatrixCalc.CalculateModelMatrix(Pos.Lookat, Camera.Current, Zoom.Current);
            GL.Viewport(0, 0, glControl.Width, glControl.Height);                        // Use all of the glControl painting area
        }

        // Paint the scene, call the installed PaintObjects after setting up the buffer and standard settings.

        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.PointSmooth);                                               // standard render options
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            PaintObjects?.Invoke(MatrixCalc, sysinterval.ElapsedMilliseconds);

            glControl.SwapBuffers();
        }

        private void glControl_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            KillSlews();

            mouseDownPos.X = e.X;
            mouseDownPos.Y = e.Y;

            if (e.Button.HasFlag(System.Windows.Forms.MouseButtons.Left))
            {
                mouseStartRotate.X = e.X;
                mouseStartRotate.Y = e.Y;
            }

            if (e.Button.HasFlag(System.Windows.Forms.MouseButtons.Right))
            {
                mouseStartTranslateXY.X = e.X;
                mouseStartTranslateXY.Y = e.Y;
                mouseStartTranslateXZ.X = e.X;
                mouseStartTranslateXZ.Y = e.Y;
            }

            MouseDown?.Invoke(e);
        }

        private void glControl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            bool notmovedmouse = Math.Abs(e.X - mouseDownPos.X) + Math.Abs(e.Y - mouseDownPos.Y) < 4;

            if (!notmovedmouse)     // if we moved it, its not a stationary click, ignore
                return;


            if (e.Button == System.Windows.Forms.MouseButtons.Right)                    // right clicks are about bookmarks.
            {
                mouseStartTranslateXY = new Point(int.MinValue, int.MinValue);         // indicate rotation is finished.
                mouseStartTranslateXZ = new Point(int.MinValue, int.MinValue);
            }

            MouseUp?.Invoke(e);
        }

        private void glControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (mouseStartRotate.X != int.MinValue) // on resize double click resize, we get a stray mousemove with left, so we need to make sure we actually had a down event
                {
                    KillSlews();
                    int dx = e.X - mouseStartRotate.X;
                    int dy = e.Y - mouseStartRotate.Y;

                    mouseStartRotate.X = mouseStartTranslateXZ.X = e.X;
                    mouseStartRotate.Y = mouseStartTranslateXZ.Y = e.Y;

                    Camera.Rotate(new Vector3((float)(dy * MouseRotateAmountPerPixel), (float)(dx * MouseRotateAmountPerPixel ), 0));
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
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
            else if (e.Button == (System.Windows.Forms.MouseButtons.Left | System.Windows.Forms.MouseButtons.Right))
            {
                if (mouseStartTranslateXZ.X != int.MinValue)
                {
                    KillSlews();

                    int dx = e.X - mouseStartTranslateXZ.X;
                    int dy = e.Y - mouseStartTranslateXZ.Y;

                    mouseStartTranslateXZ.X = mouseStartRotate.X = mouseStartTranslateXY.X = e.X;
                    mouseStartTranslateXZ.Y = mouseStartRotate.Y = mouseStartTranslateXY.Y = e.Y;
                    //System.Diagnostics.Trace.WriteLine("dx" + dx.ToString() + " dy " + dy.ToString() + " Button " + e.Button.ToString());

                    Matrix3 transform = Matrix3.CreateRotationZ((float)(-Camera.Current.Y * Math.PI / 180.0f));
                    Vector3 translation = new Vector3(dx * (1.0f / Zoom.Current) * MouseTranslateAmountAtZoom1PerPixel, -dy * (1.0f / Zoom.Current) * MouseTranslateAmountAtZoom1PerPixel, 0.0f);
                    translation = Vector3.Transform(translation, transform);

                    Pos.Translate(new Vector3(translation.X, 0, translation.Y));
                }
            }

            MouseMove?.Invoke(e);
        }

        private void glControl_OnMouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                if (keyboard.Ctrl)
                {
                    if (fov.Scale(e.Delta < 0))
                    {
                        SetModelProjectionMatrix();
                        glControl.Invalidate();
                    }
                }
                else
                {
                    Zoom.Scale(e.Delta > 0);
                }
            }

            MouseWheel?.Invoke(e);
        }

        private Fov fov = new Fov();
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
