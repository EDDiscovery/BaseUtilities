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
    public enum CheckBoxAppearance
    {
        Normal = 0,
        Button = 1,
        Radio = 2,
    }

    public enum CheckState { Unchecked, Checked, Indeterminate };

    public class GLCheckBox : GLButtonTextBase
    {
        public Action<GLBaseControl> CheckChanged { get; set; } = null;     // not fired by programatically changing CheckState

        public CheckState CheckState { get { return checkstate; } set { SetCheckState(value); } }
        public bool Checked { get { return checkstate == CheckState.Checked; } set { SetCheckState(value ? CheckState.Checked : CheckState.Unchecked); } }
        public bool AutoCheck { get; set; } = false;            // if true, autocheck on click

        public CheckBoxAppearance Appearance { get { return appearance; } set { appearance = value; Invalidate(); } }

        // Fore (text), ButtonBack, MouseOverBackColor, MouseDownBackColor from inherited class
        private Color CheckBoxInnerColor { get { return checkBoxInnerColor; } set { checkBoxInnerColor = value; Invalidate(); } } 
        private Color CheckColor { get { return checkColor; } set { checkColor = value; Invalidate(); } }

        public bool GroupRadioButton { get; set; } = false;     // if true, on check, turn off all other CheckBox of parents

        public ContentAlignment CheckAlign { get { return checkalign; } set { checkalign = value; Invalidate(); } }     // appearance Normal only
        public float TickBoxReductionRatio { get; set; } = 0.75f;       // Normal - size reduction

        public Image ImageUnchecked { get { return imageUnchecked; } set { imageUnchecked = value; Invalidate(); } }        // apperance normal/button only.  
        public Image ImageIndeterminate { get { return imageIndeterminate; } set { imageIndeterminate = value; Invalidate(); } }

        public void SetDrawnBitmapUnchecked(System.Drawing.Imaging.ColorMap[] remap, float[][] colormatrix = null)
        {
            drawnImageAttributesUnchecked?.Dispose();
            ControlHelpersStaticFunc.ComputeDrawnPanel(out drawnImageAttributesUnchecked, out System.Drawing.Imaging.ImageAttributes drawnImageAttributesDisabled, 1.0f, remap, colormatrix);
            Invalidate();
        }

        public GLCheckBox(string name, Rectangle location, string text) : base(name, location)
        {
            BackColorNI = Color.Transparent;
            TextNI = text;
        }

        public GLCheckBox(string name, Rectangle location, Image chk, Image unchk) : base(name, location)
        {
            BackColorNI = Color.Transparent;
            TextNI = "";
            Image = chk;
            ImageUnchecked = unchk;
            Appearance = CheckBoxAppearance.Button;
        }

        public GLCheckBox() : this("CB?", DefaultWindowRectangle, "")
        {
        }

        protected override void SizeControl(Size parentsize)
        {
            base.SizeControl(parentsize);

            if ( AutoSize )
            {
                SizeF size = BitMapHelpers.MeasureStringInBitmap(Text, Font, ControlHelpersStaticFunc.StringFormatFromContentAlignment(TextAlign));
                
                Size s = new Size((int)(size.Width + 0.999) + Margin.TotalWidth + Padding.TotalWidth + BorderWidth + 4,
                                 (int)(size.Height + 0.999) + Margin.TotalHeight + Padding.TotalHeight + BorderWidth + 4);

                SetLocationSizeNI(size:s);
            }
        }

        protected override void Paint(Rectangle area, Graphics gr)
        {
            bool hasimages = Image != null;

            if (Appearance == CheckBoxAppearance.Button)
            {
                if (Enabled)
                {
                    Rectangle marea = area;
                    marea.Inflate(-2, -2);

                    if (Hover)
                    {
                        using (var b = new LinearGradientBrush(marea, MouseOverBackColor, MouseOverBackColor.Multiply(BackColorScaling), 90))
                            gr.FillRectangle(b, marea);
                    }
                    else if (CheckState == CheckState.Checked)
                    {
                        using (var b = new LinearGradientBrush(marea, ButtonBackColor, ButtonBackColor.Multiply(BackColorScaling), 90))
                            gr.FillRectangle(b, marea);
                    }
                }

                if (hasimages)
                    DrawImage(area, gr);

                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (var fmt = ControlHelpersStaticFunc.StringFormatFromContentAlignment(TextAlign))
                    DrawText(area, gr, fmt);
            }
            else if ( Appearance == CheckBoxAppearance.Normal )
            {
                Rectangle tickarea = area;
                Rectangle textarea = area;

                int reduce = (int)(tickarea.Height * TickBoxReductionRatio);
                tickarea.Y += (tickarea.Height - reduce) / 2;
                tickarea.Height = reduce;
                tickarea.Width = tickarea.Height;

                if (CheckAlign == ContentAlignment.MiddleRight)
                {
                    tickarea.X = area.Width - tickarea.Width;
                    textarea.Width -= tickarea.Width;
                }
                else
                {
                    textarea.X += tickarea.Width;
                    textarea.Width -= tickarea.Width;
                }

                float discaling = Enabled ? 1.0f : DisabledScaling;

                Color checkboxbasecolour = (Enabled && Hover) ? MouseOverBackColor : ButtonBackColor.Multiply(discaling);

                if (!hasimages)      // draw the over box of the checkbox if no images
                {
                    using (Pen outer = new Pen(checkboxbasecolour))
                        gr.DrawRectangle(outer, tickarea);
                }

                tickarea.Inflate(-1, -1);

                Rectangle checkarea = tickarea;
                checkarea.Width++; checkarea.Height++;          // convert back to area

                //                System.Diagnostics.Debug.WriteLine("Owner draw " + Name + checkarea + rect);

                if (hasimages)
                {
                    if (Enabled && Hover)                // if mouse over, draw a nice box around it
                    {
                        using (Brush mover = new SolidBrush(MouseOverBackColor))
                        {
                            gr.FillRectangle(mover, checkarea);
                        }
                    }
                }
                else
                {                                   // in no image, we draw a set of boxes
                    using (Pen second = new Pen(CheckBoxInnerColor.Multiply(discaling), 1F))
                        gr.DrawRectangle(second, tickarea);

                    tickarea.Inflate(-1, -1);

                    using (Brush inner = new LinearGradientBrush(tickarea, CheckBoxInnerColor.Multiply(discaling), checkboxbasecolour, 225))
                        gr.FillRectangle(inner, tickarea);      // fill slightly over size to make sure all pixels are painted

                    using (Pen third = new Pen(checkboxbasecolour.Multiply(discaling), 1F))
                        gr.DrawRectangle(third, tickarea);
                }

                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (StringFormat fmt = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.FitBlackBox })
                    DrawText(textarea, gr, fmt);

                if (hasimages)
                {
                    DrawImage(checkarea, gr);
                }
                else
                {
                    Color c1 = Color.FromArgb(200, CheckColor.Multiply(discaling));
                    if (CheckState == CheckState.Checked)
                    {
                        Point pt1 = new Point(checkarea.X + 2, checkarea.Y + checkarea.Height / 2 - 1);
                        Point pt2 = new Point(checkarea.X + checkarea.Width / 2 - 1, checkarea.Bottom - 2);
                        Point pt3 = new Point(checkarea.X + checkarea.Width - 2, checkarea.Y);

                        using (Pen pcheck = new Pen(c1, 2.0F))
                        {
                            gr.DrawLine(pcheck, pt1, pt2);
                            gr.DrawLine(pcheck, pt2, pt3);
                        }
                    }
                    else if (CheckState == CheckState.Indeterminate)
                    {
                        Size cb = new Size(checkarea.Width - 5, checkarea.Height - 5);
                        if (cb.Width > 0 && cb.Height > 0)
                        {
                            using (Brush br = new SolidBrush(c1))
                            {
                                gr.FillRectangle(br, new Rectangle(new Point(checkarea.X + 2, checkarea.Y + 2), cb));
                            }
                        }
                    }
                }
            }
            else
            {
                Rectangle rect = area;

                rect.Height -= 6;
                rect.Y += 2;
                rect.Width = rect.Height;

                Rectangle textarea = area;
                textarea.X += rect.Width;
                textarea.Width -= rect.Width;

                Color basecolor = Hover ? MouseOverBackColor : ButtonBackColor;

                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (Brush outer = new SolidBrush(basecolor))
                    gr.FillEllipse(outer, rect);

                rect.Inflate(-1, -1);

                if (Enabled)
                {
                    using (Brush second = new SolidBrush(CheckBoxInnerColor))
                        gr.FillEllipse(second, rect);

                    rect.Inflate(-1, -1);

                    using (Brush inner = new LinearGradientBrush(rect, CheckBoxInnerColor, basecolor, 225))
                        gr.FillEllipse(inner, rect);      // fill slightly over size to make sure all pixels are painted
                }
                else
                {
                    using (Brush disabled = new SolidBrush(CheckBoxInnerColor))
                    {
                        gr.FillEllipse(disabled, rect);
                    }
                }

                rect.Inflate(-1, -1);

                if (Checked)
                {
                    Color c1 = Color.FromArgb(255, CheckColor);

                    using (Brush inner = new LinearGradientBrush(rect, CheckBoxInnerColor, c1, 45))
                        gr.FillEllipse(inner, rect);      // fill slightly over size to make sure all pixels are painted

                    using (Pen ring = new Pen(CheckColor))
                        gr.DrawEllipse(ring, rect);
                }

                using (StringFormat fmt = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
                    DrawText(textarea, gr, fmt);
            }

            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
        }

        private void DrawImage(Rectangle box, Graphics g)
        {
            if (ImageUnchecked != null)     // if we have an alt image for unchecked
            {
                Image image = CheckState == CheckState.Checked ? Image : ((CheckState == CheckState.Indeterminate && ImageIndeterminate != null) ? ImageIndeterminate : (ImageUnchecked != null ? ImageUnchecked : Image));
                base.DrawImage(image, box, g, (Enabled) ? drawnImageAttributesEnabled : drawnImageAttributesDisabled);
            }
            else
            {
                base.DrawImage(Image, box, g, (Enabled) ? ((Checked) ? drawnImageAttributesEnabled: drawnImageAttributesUnchecked) :drawnImageAttributesDisabled);
            }
        }

        private Font FontToUse;

        private void DrawText(Rectangle box, Graphics g, StringFormat fmt)
        {
            if (this.Text.HasChars())
            {
                using (Brush textb = new SolidBrush(Enabled ? this.ForeColor : this.ForeColor.Multiply(DisabledScaling)))
                {
                    if (FontToUse == null || FontToUse.FontFamily != Font.FontFamily || FontToUse.Style != Font.Style)
                        FontToUse = g.GetFontToFitRectangle(this.Text, Font, box, fmt);

                    g.DrawString(this.Text, FontToUse, textb, box, fmt);
                }
            }
        }

        public override void OnMouseClick(GLMouseEventArgs e)
        {
            base.OnMouseClick(e);
            if ( e.Button == GLMouseEventArgs.MouseButtons.Left && AutoCheck )
            {
                SetCheckState(checkstate == CheckState.Unchecked ? CheckState.Checked : CheckState.Unchecked);
            }
        }

        private void SetCheckState(CheckState value)
        {
            if (checkstate != value)
            {
                checkstate = value;

                if (GroupRadioButton && Parent != null && checkstate == CheckState.Checked)
                {
                    foreach (GLCheckBox c in Parent.ControlsZ.OfType<GLCheckBox>())
                    {
                        if (c != this && c.GroupRadioButton == true && c.checkstate != CheckState.Unchecked)    // if not us, in a group, and not unchecked
                        {
                            c.checkstate = CheckState.Unchecked;        // set directly
                            c.OnCheckChanged();                         // fire change
                            c.Invalidate();
                        }
                    }
                }

                OnCheckChanged();   // fire change on us
                Invalidate();
            }
        }

        protected virtual void OnCheckChanged()
        {
            CheckChanged?.Invoke(this);
        }

        private GL4.Controls.CheckState checkstate { get; set; } = CheckState.Unchecked;
        private GL4.Controls.CheckBoxAppearance appearance { get; set; } = CheckBoxAppearance.Normal;
        private ContentAlignment checkalign { get; set; } = ContentAlignment.MiddleCenter;

        private Image imageUnchecked { get; set; } = null;               // set if using different images for unchecked
        private Image imageIndeterminate { get; set; } = null;           // optional for intermediate
        private System.Drawing.Imaging.ImageAttributes drawnImageAttributesUnchecked = null;         // if unchecked image does not exist, use this for image scaling

        private Color checkBoxInnerColor { get; set; } = DefaultCheckBoxInnerColor;    // Normal only inner colour
        private Color checkColor { get; set; } = DefaultCheckColor;         // Button - back colour when checked, Normal - check colour

    }
}
