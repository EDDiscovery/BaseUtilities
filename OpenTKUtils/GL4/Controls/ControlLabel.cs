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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{

    public class GLLabel : GLForeDisplayBase
    {
        public string Text { get { return text; } set { text = value; Invalidate(); } }

        public ContentAlignment TextAlign { get { return textAlign; } set { textAlign = value; Invalidate(); } }

        public GLLabel(string name, Rectangle location, string text, Color backcolour) : base(name,location, backcolour)
        {
            this.text = text;
        }

        public GLLabel() : this("LB?", DefaultWindowRectangle, "", DefaultBackColor)
        {
        }

        protected override void SizeControl()
        {
            base.SizeControl();
            if (AutoSize)
            {
                SizeF size = new Size(0, 0);
                if (Text.HasChars())
                    size = BaseUtils.BitMapHelpers.MeasureStringInBitmap(Text, Font, ControlHelpersStaticFunc.StringFormatFromContentAlignment(TextAlign));

                Size s = new Size((int)(size.Width + 0.999) + Margin.TotalWidth + Padding.TotalWidth + BorderWidth + 4,
                                 (int)(size.Height + 0.999) + Margin.TotalHeight + Padding.TotalHeight + BorderWidth + 4);

                SetLocationSizeNI(size: s);
            }
        }

        protected override void Paint(Rectangle area, Graphics gr)
        {
            if (Text.HasChars())
            {
                gr.SmoothingMode = SmoothingMode.AntiAlias;

                using (var fmt = ControlHelpersStaticFunc.StringFormatFromContentAlignment(TextAlign))
                {
                    using (Brush textb = new SolidBrush((Enabled) ? this.ForeColor : this.ForeColor.Multiply(DisabledScaling)))
                    {
                        gr.DrawString(this.Text, this.Font, textb, area, fmt);
                    }
                }

                gr.SmoothingMode = SmoothingMode.None;
            }
        }

        private ContentAlignment textAlign { get; set; } = ContentAlignment.MiddleLeft;
        private string text = "";
    }
}
