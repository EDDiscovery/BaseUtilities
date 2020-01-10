/*
 * Copyright © 2019 Robbyxp1 @ github.com
 * Part of the EDDiscovery Project
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
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;


namespace OpenTKUtils.WinForm
{
    // a win form control version of GLWindowControl

    public class GLWinFormControl : GLWindowControl
    {
        public Action<Object, MouseEventArgs> MouseDown { get; set; } = null;
        public Action<Object, MouseEventArgs> MouseUp { get; set; } = null;
        public Action<Object, MouseEventArgs> MouseMove { get; set; } = null;
        public Action<Object, MouseEventArgs> MouseEnter { get; set; } = null;
        public Action<Object, MouseEventArgs> MouseLeave { get; set; } = null;
        public Action<Object, MouseEventArgs> MouseClick { get; set; } = null;
        public Action<Object, MouseEventArgs> MouseWheel { get; set; } = null;
        public Action<Object, KeyEventArgs> KeyDown { get; set; } = null;
        public Action<Object, KeyEventArgs> KeyUp { get; set; } = null;
        public Action<Object, KeyEventArgs> KeyPress { get; set; } = null;
        public Action<Object> Resize { get; set; } = null;
        public Action<Object> Paint { get; set; } = null;

        public GLWinFormControl(Control attachcontrol)
        {
            glControl = CreateGLClass();

            attachcontrol.Controls.Add(glControl);

            glControl.MouseDown += Gc_MouseDown;
            glControl.MouseUp += Gc_MouseUp;
            glControl.MouseMove += Gc_MouseMove;
            glControl.MouseEnter += Gc_MouseEnter;
            glControl.MouseLeave += Gc_MouseLeave;
            glControl.MouseClick += Gc_MouseClick;
            glControl.MouseWheel += Gc_MouseWheel;
            glControl.KeyDown += Gc_KeyDown;
            glControl.KeyUp += Gc_KeyUp;
            glControl.KeyPress += Gc_KeyPress;
            glControl.Resize += Gc_Resize;
            glControl.Paint += GlControl_Paint;
        }

        private OpenTK.GLControl CreateGLClass()
        {
            OpenTK.GLControl gl;
            gl = new OpenTK.GLControl();
            gl.Dock = DockStyle.Fill;
            gl.BackColor = System.Drawing.Color.Black;
            gl.Name = "glControl";
            gl.TabIndex = 0;
            gl.VSync = true;
            gl.PreviewKeyDown += Gl_PreviewKeyDown;
            
            return gl;
        }

        private void Gl_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)    // all keys are for us
        {
            if ( e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down )
                e.IsInputKey = true;
        }

        public void Invalidate()
        {
            glControl.Invalidate();
        }

        public Color BackColour { get { return backcolor; } set { backcolor = value; GL.ClearColor(backcolor); } } 

        public OpenTK.GLControl glControl { get; private set; }      // use only in extreams for back compat

        public int Width { get { return glControl.Width; } }
        public int Height { get { return glControl.Height; } }
        public bool Focused { get { return glControl.Focused; } }

        private Color backcolor { get; set; } = (Color)System.Drawing.ColorTranslator.FromHtml("#0D0D10");

        private Point FindCursorFormCoords()
        {
            BaseUtils.Win32.UnsafeNativeMethods.GetCursorPos(out BaseUtils.Win32.UnsafeNativeMethods.POINT p);
            Point gcsp = glControl.PointToScreen(new Point(0, 0));
            return new Point(p.X - gcsp.X, p.Y - gcsp.Y);
        }

        private void Gc_MouseEnter(object sender, EventArgs e)
        {
            Point relcurpos = FindCursorFormCoords();
            var ev = new MouseEventArgs(relcurpos);
            MouseEnter?.Invoke(this, ev);
        }

        private void Gc_MouseLeave(object sender, EventArgs e)
        {
            Point relcurpos = FindCursorFormCoords();
            var ev = new MouseEventArgs(relcurpos);
            MouseLeave?.Invoke(this, ev);
        }

        private void Gc_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            MouseEventArgs.MouseButtons b = (((e.Button & System.Windows.Forms.MouseButtons.Left) != 0) ? MouseEventArgs.MouseButtons.Left : 0) |
                (((e.Button & System.Windows.Forms.MouseButtons.Middle) != 0) ? MouseEventArgs.MouseButtons.Middle : 0) |
                (((e.Button & System.Windows.Forms.MouseButtons.Right) != 0) ? MouseEventArgs.MouseButtons.Right : 0);

            var ev = new MouseEventArgs(b, e.Location, e.Clicks);
            MouseUp?.Invoke(this, ev);
        }

        private void Gc_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            MouseEventArgs.MouseButtons b = (((e.Button & System.Windows.Forms.MouseButtons.Left) != 0) ? MouseEventArgs.MouseButtons.Left : 0) |
                (((e.Button & System.Windows.Forms.MouseButtons.Middle) != 0) ? MouseEventArgs.MouseButtons.Middle : 0) |
                (((e.Button & System.Windows.Forms.MouseButtons.Right) != 0) ? MouseEventArgs.MouseButtons.Right : 0);

            var ev = new MouseEventArgs(b, e.Location, e.Clicks);
            MouseDown?.Invoke(this, ev);
        }

        private void Gc_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            MouseEventArgs.MouseButtons b = (((e.Button & System.Windows.Forms.MouseButtons.Left) != 0) ? MouseEventArgs.MouseButtons.Left : 0) |
                (((e.Button & System.Windows.Forms.MouseButtons.Middle) != 0) ? MouseEventArgs.MouseButtons.Middle : 0) |
                (((e.Button & System.Windows.Forms.MouseButtons.Right) != 0) ? MouseEventArgs.MouseButtons.Right : 0);
            var ev = new MouseEventArgs(b, e.Location, e.Clicks);
            MouseClick?.Invoke(this, ev);
        }

        private void Gc_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            MouseEventArgs.MouseButtons b = (((e.Button & System.Windows.Forms.MouseButtons.Left) != 0) ? MouseEventArgs.MouseButtons.Left : 0) |
                (((e.Button & System.Windows.Forms.MouseButtons.Middle) != 0) ? MouseEventArgs.MouseButtons.Middle : 0) |
                (((e.Button & System.Windows.Forms.MouseButtons.Right) != 0) ? MouseEventArgs.MouseButtons.Right : 0);
            var ev = new MouseEventArgs(b, e.Location, e.Clicks);
            MouseMove?.Invoke(this, ev);
        }

        private void Gc_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            MouseEventArgs.MouseButtons b = (((e.Button & System.Windows.Forms.MouseButtons.Left) != 0) ? MouseEventArgs.MouseButtons.Left : 0) |
                (((e.Button & System.Windows.Forms.MouseButtons.Middle) != 0) ? MouseEventArgs.MouseButtons.Middle : 0) |
                (((e.Button & System.Windows.Forms.MouseButtons.Right) != 0) ? MouseEventArgs.MouseButtons.Right : 0);
            var ev = new MouseEventArgs(b,e.Location, e.Clicks, e.Delta);
            MouseWheel?.Invoke(this, ev);
        }

        public void Gc_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)      
        {
            KeyEventArgs ka = new KeyEventArgs(e.Alt, e.Control, e.Shift, e.KeyCode, e.KeyValue, e.Modifiers);
            KeyDown?.Invoke(this, ka);
        }

        public void Gc_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            KeyEventArgs ka = new KeyEventArgs(e.Alt, e.Control, e.Shift, e.KeyCode, e.KeyValue, e.Modifiers);
            KeyUp?.Invoke(this, ka);
        }

        public void Gc_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            KeyEventArgs ka = new KeyEventArgs(e.KeyChar);     
            KeyPress?.Invoke(this, ka);
        }

        private void Gc_Resize(object sender, EventArgs e)
        {
            Resize?.Invoke(this);
        }

        // called by window after invalidate. Set up and call painter of objects

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.FrontFace(FrontFaceDirection.Ccw);
            GLStatics.DefaultDepthTest();
            GLStatics.DefaultCullFace();
            GLStatics.DefaultPointSize();                               // default is controlled by external not shaders
            GLStatics.DefaultBlend();
            GLStatics.DefaultPointSize();
            GLStatics.DefaultPrimitiveRestart();

            Paint?.Invoke(glControl);

            glControl.SwapBuffers();
        }
    }
}
