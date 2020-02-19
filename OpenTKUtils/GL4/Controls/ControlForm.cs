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

using System.Drawing;
using System.Drawing.Drawing2D;

namespace OpenTKUtils.GL4.Controls
{
    public class GLForm : GLForeDisplayTextBase, IForm
    {
        public const int FormMargins = 2;
        public const int FormPadding = 2;
        public const int FormBorderWidth = 1;

        public GLForm(string name, string title, Rectangle location) : base(name, location)
        {
            BackColor = DefaultFormBackColor;
            PaddingNI = new Padding(FormPadding);
            MarginNI = new Margin(FormMargins);
            BorderWidthNI = FormBorderWidth;
            BorderColorNI = DefaultBorderColor;
            text = title;
            Themer?.Invoke(this);
        }

        public GLForm() : this("F?", "", DefaultWindowRectangle)
        {
        }

        public int TitleBarHeight { get { return (Font?.ScalePixels(20) ?? 20) + FormMargins * 2; } }


        public void Close()
        {
            OnClose();
            Parent?.Remove(this);
        }

        public void OnShown()
        {
        }

        public void OnClose()
        {
        }

        private GLMouseEventArgs.AreaType captured = GLMouseEventArgs.AreaType.Client;  // meaning none
        private Point capturelocation;
        private Rectangle originalwindow;
        private bool cursorindicatingmovement = false;

        public override void PerformRecursiveLayout()
        {
            if (text.HasChars())
                MarginNI = new Margin(Margin.Left, TitleBarHeight + FormMargins * 2, Margin.Right, Margin.Bottom);
            else
                MarginNI = new Margin(Margin.Left, FormMargins, Margin.Right, Margin.Bottom);

            base.PerformRecursiveLayout();
        }

        // move this to border paint
        protected override void DrawBorder(Rectangle bounds, Graphics gr, Color bc, float bw)      // normal override, you can overdraw border if required.
        {
            base.DrawBorder(bounds, gr, bc, bw);

            if (Text.HasChars())
            {
                gr.SmoothingMode = SmoothingMode.AntiAlias;

                using (var fmt = ControlHelpersStaticFunc.StringFormatFromContentAlignment(TextAlign))
                {
                    using (Brush textb = new SolidBrush((Enabled) ? this.ForeColor : this.ForeColor.Multiply(DisabledScaling)))
                    {
                        Rectangle titlearea = new Rectangle(bounds.Left, bounds.Top, bounds.Width, TitleBarHeight );
                        gr.DrawString(this.Text, this.Font, textb, titlearea, fmt);
                    }
                }
                gr.SmoothingMode = SmoothingMode.None;
            }
        }

        public override void OnMouseMove(GLMouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Handled == false)
            {
                if (captured != GLMouseEventArgs.AreaType.Client)
                {
                    Point capturedelta = new Point(e.Location.X - capturelocation.X, e.Location.Y - capturelocation.Y);
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
                    else if (e.Area == GLMouseEventArgs.AreaType.Top)
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

        public override void OnMouseDown(GLMouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!e.Handled && e.Area != GLMouseEventArgs.AreaType.Client)
            {
                capturelocation = e.Location;
                originalwindow = Bounds;
                captured = e.Area;
            }
        }

        public override void OnMouseUp(GLMouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (captured != GLMouseEventArgs.AreaType.Client)
            {
                captured = GLMouseEventArgs.AreaType.Client;
                FindDisplay()?.SetCursor(GLCursorType.Normal);
            }
        }

        public override void OnMouseLeave(GLMouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if ( cursorindicatingmovement )
            {
                FindDisplay()?.SetCursor(GLCursorType.Normal);
                cursorindicatingmovement = false;
            }
        }


    }
}


