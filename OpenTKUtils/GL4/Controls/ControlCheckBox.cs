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
    public class GLCheckBox : GLButtonBase
    {
        public Action<GLBaseControl, GLMouseEventArgs> CheckChanged { get; set; } = null;     // not fired by programatically changing CheckState

        public GL4.Controls.CheckState CheckState { get { return checkstate; } set { checkstate = value; Invalidate(); } }
        public bool Checked { get { return checkstate == CheckState.Checked; } set { checkstate = value ? CheckState.Checked : CheckState.Unchecked; Invalidate(); } }
        public bool AutoCheck { get; set; } = false;

        public Appearance Appearance { get { return appearance; } set { appearance = value; Invalidate(); } }
        public ContentAlignment CheckAlign { get { return checkalign; } set { checkalign = value; Invalidate(); } }
        public float TickBoxReductionRatio { get; set; } = 0.75f;       // Normal - size reduction

        public Image ImageUnchecked { get { return imageUnchecked; } set { imageUnchecked = value; Invalidate(); } }
        public Image ImageIndeterminate { get { return imageIndeterminate; } set { imageIndeterminate = value; Invalidate(); } }

        public GLCheckBox(string name, Rectangle location, string text, Color backcolour) : base(name,location,backcolour)
        {
            TextNI = text;
        }

        public GLCheckBox() : this("CB?", DefaultWindowRectangle, "", DefaultBackColor)
        {
        }

        protected override void SizeControl()
        {
            base.SizeControl();
            if ( AutoSize )
            {
                SizeF size = BaseUtils.BitMapHelpers.MeasureStringInBitmap(Text, Font, ControlHelpersStaticFunc.StringFormatFromContentAlignment(TextAlign));
                
                Size s = new Size((int)(size.Width + 0.999) + Margin.TotalWidth + Padding.TotalWidth + BorderWidth + 4,
                                 (int)(size.Height + 0.999) + Margin.TotalHeight + Padding.TotalHeight + BorderWidth + 4);

                SetLocationSizeNI(size:s);
            }
        }

        protected override void Paint(Rectangle area, Graphics gr)
        {
            bool hasimages = Image != null;

            if (Appearance == Appearance.Button)
            {
                if (Enabled)
                {
                    Rectangle marea = area;
                    marea.Inflate(-2, -2);

                    if (Hover)
                    {
                        using (Brush mover = new SolidBrush(MouseOverBackColor))
                           gr.FillRectangle(mover, marea);
                    }
                    else if (CheckState == CheckState.Checked)
                    {
                        using (Brush mover = new SolidBrush(ButtonBackColor))
                            gr.FillRectangle(mover, marea);
                    }
                }

                if (hasimages)
                    DrawImage(area, gr);

                using (var fmt = ControlHelpersStaticFunc.StringFormatFromContentAlignment(TextAlign))
                {
                    gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    DrawText(area, gr, fmt);
                    gr.SmoothingMode = SmoothingMode.Default;
                }
            }
            else
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

                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            }

        }

        private void DrawImage(Rectangle box, Graphics g)
        {
            Image image = CheckState == CheckState.Checked ? Image : ((CheckState == CheckState.Indeterminate && ImageIndeterminate != null) ? ImageIndeterminate : (ImageUnchecked != null ? ImageUnchecked : Image));
            base.DrawImage(image, box, g);
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
                Checked = !Checked;
                OnCheckChanged(e);
            }
        }

        public virtual void OnCheckChanged(GLMouseEventArgs e)
        {
            CheckChanged?.Invoke(this, e);
        }

        private GL4.Controls.CheckState checkstate { get; set; } = CheckState.Unchecked;
        private GL4.Controls.Appearance appearance { get; set; } = Appearance.Normal;
        private ContentAlignment checkalign { get; set; } = ContentAlignment.MiddleCenter;
        private Image imageUnchecked { get; set; } = null;               // Both - set image when unchecked.  Also set Image
        private Image imageIndeterminate { get; set; } = null;           // Both - optional - can set this, if required, if using indeterminate value
        private Color CheckBoxInnerColor { get; set; } = Color.White;    // Normal only inner colour
        private Color CheckColor { get; set; } = Color.DarkBlue;         // Button - back colour when checked, Normal - check colour


    }
}
