/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
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
using System.Drawing.Drawing2D;

namespace OpenTKUtils.GL4.Controls
{
    // Forms are usually placed below DisplayControl, but can act as movable controls inside other controls

    public enum DialogResult
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Abort = 3,
        Retry = 4,
        Ignore = 5,
        Yes = 6,
        No = 7
    }

    public class GLForm : GLForeDisplayTextBase
    {
        public const int FormMargins = 2;
        public const int FormPadding = 2;
        public const int FormBorderWidth = 1;

        public bool FormShown { get; set; } = false;        // only applies to top level forms
        public bool TabChangesFocus { get; set; } = true;
        public bool ShowClose { get; set; } = true;     // show close symbol

        public Action<GLForm> Shown;
        public Action<GLForm,GLHandledArgs> FormClosing;
        public Action<GLForm> FormClosed;

        public DialogResult DialogResult { get { return dialogResult; } set { SetDialogResult(value); }  }
        public Action<GLForm, DialogResult> DialogCallback { get; set; } // if a form sets a dialog result, this callback gets called

        public GLForm(string name, string title, Rectangle location) : base(name, location)
        {
            ForeColor = DefaultFormTextColor;
            BackColor = DefaultFormBackColor;
            PaddingNI = new Padding(FormPadding);
            MarginNI = new Margin(FormMargins);
            BorderWidthNI = FormBorderWidth;
            BorderColorNI = DefaultBorderColor;
            text = title;
        }

        public GLForm() : this("F?", "", DefaultWindowRectangle)
        {
        }

        public int TitleBarHeight { get { return (Font?.ScalePixels(20) ?? 20) + FormMargins * 2; } }

        public void Close()
        {
            GLHandledArgs e = new GLHandledArgs();
            OnClose(e);
            if ( !e.Handled )
            {
                OnClosed();
                Parent?.Remove(this);
            }
        }

        #region For inheritors

        public virtual void OnShown()   // only called if top level form
        {
            GLBaseControl lowest = FindNextTabChild(int.MaxValue, false);      // try the tab order, 0 first
            if (lowest != null)
                lowest.SetFocus();
            else
            {
                GLBaseControl firsty = FirstChildYOfType(null, (x) => x.Enabled && x.Focusable);     // focus on highest child
                if (firsty != null)
                    firsty.SetFocus();
            }

            Shown?.Invoke(this);
        }

        public virtual void OnClose(GLHandledArgs e)   
        {
            FormClosing?.Invoke(this, e);
        }

        public virtual void OnClosed()   
        {
            FormClosed?.Invoke(this);
        }

        #endregion

        #region Implementation

        private void SetDialogResult(DialogResult v)
        {
            dialogResult = v;
            DialogCallback?.Invoke(this, dialogResult);
        }

        public override void PerformRecursiveLayout()
        {
            if (text.HasChars())
                MarginNI = new Margin(Margin.Left, TitleBarHeight + FormMargins * 2, Margin.Right, Margin.Bottom);
            else
                MarginNI = new Margin(Margin.Left, FormMargins, Margin.Right, Margin.Bottom);

            base.PerformRecursiveLayout();
        }

        protected override void DrawBorder(Rectangle bounds, Graphics gr, Color bc, float bw)     
        {
            base.DrawBorder(bounds, gr, bc, bw);    // draw basic border

            Color c = (Enabled) ? this.ForeColor : this.ForeColor.Multiply(DisabledScaling);

            gr.SmoothingMode = SmoothingMode.AntiAlias;

            if (Text.HasChars())
            {

                using (var fmt = ControlHelpersStaticFunc.StringFormatFromContentAlignment(TextAlign))
                {
                    using (Brush textb = new SolidBrush(c))
                    {
                        Rectangle titlearea = new Rectangle(bounds.Left, bounds.Top, bounds.Width, TitleBarHeight);
                        gr.DrawString(this.Text, this.Font, textb, titlearea, fmt);
                    }
                }

            }

            if ( ShowClose )
            {
                Rectangle closearea = new Rectangle(bounds.Right- TitleBarHeight, bounds.Top, TitleBarHeight, TitleBarHeight);
                closearea.Inflate(new Size(-5,-5));

                using (Pen p = new Pen(c))
                {
                    gr.DrawLine(p, new Point(closearea.Left, closearea.Top), new Point(closearea.Right, closearea.Bottom));
                    gr.DrawLine(p, new Point(closearea.Left, closearea.Bottom), new Point(closearea.Right, closearea.Top));
                }
            }

            gr.SmoothingMode = SmoothingMode.None;
        }

        public override void OnMouseDown(GLMouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!e.Handled && e.Area != GLMouseEventArgs.AreaType.Client )
            {
                if (!OverClose(e))
                {
                    capturelocation = new Point(e.ControlLocation.X + e.Location.X, e.ControlLocation.Y + e.Location.Y);    // absolute screen location of capture
                    originalwindow = Bounds;
                    captured = e.Area;
                }
            }
        }

        public override void OnMouseMove(GLMouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Handled == false)
            {
                System.Diagnostics.Debug.WriteLine("Form drag " + e.Location +" " +  e.Area);
                if (captured != GLMouseEventArgs.AreaType.Client)
                {
                    Point curscrlocation = new Point(e.ControlLocation.X + e.Location.X, e.ControlLocation.Y + e.Location.Y);
                    Point capturedelta = new Point(curscrlocation.X - capturelocation.X, curscrlocation.Y - capturelocation.Y);
                    //System.Diagnostics.Debug.WriteLine("***************************");
                    //System.Diagnostics.Debug.WriteLine("Form " + captured + " " + e.Location + " " + capturelocation + " " + capturedelta);

                    if (captured == GLMouseEventArgs.AreaType.Left)
                    {
                        int left = originalwindow.Left + capturedelta.X;
                        int width = originalwindow.Right - left;
                        if (width > MinimumResizeWidth)
                            Bounds = new Rectangle(left, originalwindow.Top, width, originalwindow.Height);
                    }
                    else if (captured == GLMouseEventArgs.AreaType.Right)
                    {
                        int right = originalwindow.Right + capturedelta.X;
                        int width = right - originalwindow.Left;
                        if (width > MinimumResizeWidth)
                            Bounds = new Rectangle(originalwindow.Left, originalwindow.Top, width, originalwindow.Height);
                    }
                    else if (captured == GLMouseEventArgs.AreaType.Top)
                    {
                        Location = new Point(originalwindow.Left + capturedelta.X, originalwindow.Top + capturedelta.Y);
                    }
                    else if (captured == GLMouseEventArgs.AreaType.Bottom)
                    {
                        int bottom = originalwindow.Bottom + capturedelta.Y;
                        int height = bottom - originalwindow.Top;
                        if (height > MinimumResizeHeight)
                            Bounds = new Rectangle(originalwindow.Left, originalwindow.Top, originalwindow.Width, height);
                    }
                    else if (captured == GLMouseEventArgs.AreaType.NWSE)
                    {
                        int right = originalwindow.Right + capturedelta.X;
                        int bottom = originalwindow.Bottom + capturedelta.Y;
                        int width = right - originalwindow.Left;
                        int height = bottom - originalwindow.Top;
                        if (height > MinimumResizeHeight && width >= MinimumResizeWidth)
                            Bounds = new Rectangle(originalwindow.Left, originalwindow.Top, width, height);
                    }
                }
                else
                {
                    if (e.Area == GLMouseEventArgs.AreaType.Left || e.Area == GLMouseEventArgs.AreaType.Right)
                    {
                        FindDisplay()?.SetCursor(GLCursorType.EW);
                        cursorindicatingmovement = true;
                    }
                    else if (e.Area == GLMouseEventArgs.AreaType.Top && !OverClose(e))
                    {
                        FindDisplay()?.SetCursor(GLCursorType.Move);
                        cursorindicatingmovement = true;
                    }
                    else if (e.Area == GLMouseEventArgs.AreaType.Bottom)
                    {
                        FindDisplay()?.SetCursor(GLCursorType.NS);
                        cursorindicatingmovement = true;
                    }
                    else if (e.Area == GLMouseEventArgs.AreaType.NWSE)
                    {
                        FindDisplay()?.SetCursor(GLCursorType.NWSE);
                        cursorindicatingmovement = true;
                    }
                    else if ( cursorindicatingmovement )
                    {
                        FindDisplay()?.SetCursor(GLCursorType.Normal);
                        cursorindicatingmovement = false;
                    }
                }
            }
        }


        public override void OnMouseUp(GLMouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (captured != GLMouseEventArgs.AreaType.Client)
            {
                FindDisplay()?.SetCursor(GLCursorType.Normal);
                captured = GLMouseEventArgs.AreaType.Client;
                FindDisplay()?.SetCursor(GLCursorType.Normal);
            }
        }

        public override void OnMouseLeave(GLMouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (cursorindicatingmovement)
            {
                FindDisplay()?.SetCursor(GLCursorType.Normal);
                cursorindicatingmovement = false;
            }
        }

        public override void OnMouseClick(GLMouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (!e.Handled)
            {
                if (OverClose(e))
                {
                    System.Diagnostics.Debug.WriteLine("Click Close!");
                    Close();
                }
            }
        }

        public override void OnKeyDown(GLKeyEventArgs e)       // forms gets first dibs at keys of children
        {
            base.OnKeyDown(e);
            if (!e.Handled && TabChangesFocus && e.KeyCode == System.Windows.Forms.Keys.Tab)
            {
                GLBaseControl f = FindChildWithFocus();
                if ( f != null && f.TabOrder != -1)
                {
                    GLBaseControl next = FindNextTabChild(f.TabOrder, true);
                    if (next == null)
                        next = FindNextTabChild(int.MaxValue, false);
                    if (next != null)
                        next.SetFocus();
                }
                e.Handled = true;
            }
        }

        private bool OverClose(GLMouseEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Over close {0} {1} {2} {3}", e.Area == GLMouseEventArgs.AreaType.Top && e.X >= Width - TitleBarHeight, e.Area, e.X , Width - TitleBarHeight);
            return ShowClose && e.Area == GLMouseEventArgs.AreaType.Top && e.X >= Width - TitleBarHeight;
        }

        private GLMouseEventArgs.AreaType captured = GLMouseEventArgs.AreaType.Client;  // meaning none
        private Point capturelocation;
        private Rectangle originalwindow;
        private bool cursorindicatingmovement = false;
        private DialogResult dialogResult = DialogResult.None;

        #endregion
    }
}


