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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public class GLGroupBox : GLForeDisplayTextBase
    {
        public const int GBMargins = 2;
        public const int GBPadding = 2;
        public const int GBBorderWidth = 1;
        public const int GBXoffset = 8;
        public const int GBXpad = 2;

        public GLGroupBox(string name, string title, Rectangle location) : base(name, location)
        {
            PaddingNI = new Padding(GBPadding);
            MarginNI = new Margin(GBMargins, GroupBoxHeight, GBMargins, GBMargins);
            BorderWidthNI = GBBorderWidth;
            BorderColorNI = DefaultBorderColor;
            text = title;
        }

        public GLGroupBox(string name, string title, DockingType type, float dockpercent) : this(name, title, DefaultWindowRectangle)
        {
            Dock = type;
            DockPercent = dockpercent;
        }

        public GLGroupBox(string name, string title, Size sizep, DockingType type, float dockpercentage) : this(name, title, new Rectangle(new Point(0,0),sizep))
        {
            Dock = type;
            DockPercent = dockpercentage;
        }

        public GLGroupBox() : this("GB?", "", DefaultWindowRectangle)
        {
        }

        public int GroupBoxHeight { get { return (Font?.ScalePixels(20) ?? 20) + GBMargins * 2; } }

        public override void OnFontChanged()
        {
            base.OnFontChanged();
            MarginNI = new Margin(GBMargins, GroupBoxHeight, GBMargins, GBMargins);
        }

        protected override void DrawBorder(Rectangle bounds, Graphics gr, Color bc, float bw)      // normal override, you can overdraw border if required.
        {
            int topoffset = this.Text.HasChars() ? (Margin.Top * 3 / 8 ) : GBMargins;
            Rectangle rectarea = new Rectangle(bounds.Left + Margin.Left,
                                bounds.Top + topoffset,
                                bounds.Width - Margin.TotalWidth - 1,
                                bounds.Height - Margin.Bottom- topoffset - 1);

            System.Diagnostics.Debug.WriteLine("Bounds {0} rectarea {1}", bounds, rectarea);

            using (var fmt = ControlHelpersStaticFunc.StringFormatFromContentAlignment(TextAlign))
            {
                var size = this.Text.HasChars() ? gr.MeasureString(this.Text, this.Font, 10000, fmt) : new SizeF(0, 0);
                int twidth = (int)(size.Width + 0.99f);

                using (var p = new Pen(bc, bw))
                {
                    if (this.Text.HasChars())
                    {
                        gr.DrawLine(p, rectarea.Left + GBXoffset - GBXpad, rectarea.Top, rectarea.Left, rectarea.Top);
                        gr.DrawLine(p, rectarea.Right, rectarea.Top, rectarea.Left + GBXoffset  + twidth + GBXpad, rectarea.Top);
                    }
                    else
                    {
                        gr.DrawLine(p, rectarea.Left, rectarea.Top, rectarea.Right, rectarea.Top);
                    }

                    gr.DrawLine(p, rectarea.Left, rectarea.Top, rectarea.Left, rectarea.Bottom - 1);
                    gr.DrawLine(p, rectarea.Left, rectarea.Bottom - 1, rectarea.Right, rectarea.Bottom - 1);
                    gr.DrawLine(p, rectarea.Right, rectarea.Bottom - 1, rectarea.Right, rectarea.Top);
                    gr.DrawLine(p, rectarea.Right, rectarea.Bottom - 1, rectarea.Right, rectarea.Top);
                }

                if (this.Text.HasChars())
                { 
                    gr.SmoothingMode = SmoothingMode.AntiAlias;
                    using (Brush textb = new SolidBrush((Enabled) ? this.ForeColor : this.ForeColor.Multiply(DisabledScaling)))
                    {
                        Rectangle titlearea = new Rectangle(bounds.Left+GBXoffset, bounds.Top, twidth, GroupBoxHeight );
                        gr.DrawString(this.Text, this.Font, textb, titlearea, fmt);
                    }

                    gr.SmoothingMode = SmoothingMode.None;
                    }
            }
        }
    }
}


