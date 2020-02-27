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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    // a button type control

    public class GLButton : GLButtonTextBase
    {
        public Action<GLBaseControl, GLMouseEventArgs> Click { get; set; } = null;
        public Action<GLBaseControl> Return { get; set; } = null;

        public GLButton(string name, Rectangle location) : base(name, location)
        {
            PaddingNI = new Padding(2);       // standard format, a border with a pad of 1
            BorderWidthNI = 1;
            BorderColorNI = DefaultButtonBorderColor;
            BackColorNI = DefaultButtonBorderBackColor;
            Focusable = true;
            InvalidateOnFocusChange = true;
        }

        public GLButton(string name, Rectangle location, string text) : this(name, location)
        {
            TextNI = text;
        }

        public GLButton(string name, Rectangle location, Image img, bool stretch) : this(name, location)
        {
            TextNI = "";
            Image = img;
            ImageStretch = stretch;
        }

        public GLButton() : this("But?", DefaultWindowRectangle, "")
        {
        }

        protected override void SizeControl(Size parentsize)
        {
            base.SizeControl(parentsize);
            if ( AutoSize )
            {
                SizeF size = new Size(0, 0);
                if ( Text.HasChars() )
                    size = BitMapHelpers.MeasureStringInBitmap(Text, Font, ControlHelpersStaticFunc.StringFormatFromContentAlignment(TextAlign));
                if (Image != null)
                    size = new SizeF(size.Width+Image.Width, Math.Max(Image.Height,(int)(size.Height+0.999)));

                Size s = new Size((int)(size.Width + 0.999) + Margin.TotalWidth + Padding.TotalWidth + BorderWidth + 4,
                                 (int)(size.Height + 0.999) + Margin.TotalHeight + Padding.TotalHeight + BorderWidth + 4);

                SetLocationSizeNI(size: s);
            }
        }

        protected override void Paint(Rectangle area, Graphics gr)
        {
            Color colBack = Color.Empty;

            if ( Enabled == false )
            {
                colBack = ButtonBackColor.Multiply(DisabledScaling);
            }
            else if (MouseButtonsDown == GLMouseEventArgs.MouseButtons.Left )
            {
                colBack = MouseDownBackColor;
            }
            else if (Hover)
            {
                colBack = MouseOverBackColor;
            }
            else
            {
                colBack = ButtonBackColor;
            }

            if (area.Width < 1 || area.Height < 1)  // and no point drawing any more in the button area if its too small, it will except
                return;

            gr.SmoothingMode = SmoothingMode.None;

            if ( Focused )
            {
                using (var p = new Pen(MouseDownBackColor))
                {
                    gr.DrawRectangle(p, new Rectangle(area.Left,area.Top,area.Width-1,area.Height-1));
                    area.Inflate(new Size(-1, -1));
                }
            }

            using (var b = new LinearGradientBrush(new Rectangle(area.Left,area.Top-1,area.Width,area.Height+1), colBack, colBack.Multiply(BackColorScaling), 90))
                gr.FillRectangle(b, area);       // linear grad brushes do not respect smoothing mode, btw

            if (Image != null)
            {
                base.DrawImage(Image, area, gr, (Enabled) ? drawnImageAttributesEnabled : drawnImageAttributesDisabled);
            }

            gr.SmoothingMode = SmoothingMode.AntiAlias;

            if (!string.IsNullOrEmpty(Text))
            {
                using (var fmt = ControlHelpersStaticFunc.StringFormatFromContentAlignment(TextAlign))
                {
                    using (Brush textb = new SolidBrush((Enabled) ? this.ForeColor : this.ForeColor.Multiply(DisabledScaling)))
                    {
                        gr.DrawString(this.Text, this.Font, textb, area, fmt);
                    }
                }
            }

            gr.SmoothingMode = SmoothingMode.None;

        }

        
        public override void OnMouseClick(GLMouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button == GLMouseEventArgs.MouseButtons.Left)
                OnClick(e);
        }

        public virtual void OnClick(GLMouseEventArgs e)
        {
            Click?.Invoke(this, e);
        }

        public override void OnKeyPress(GLKeyEventArgs e)
        {
            if ( e.KeyChar == 13 )
            {
                OnReturn();
            }
        }

        public virtual void OnReturn()
        {
            Return?.Invoke(this);
        }

    }
}
