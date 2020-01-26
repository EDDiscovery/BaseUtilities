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

    public abstract class GLButtonBase : GLImageBase
    {
        public Action<GLBaseControl, GLMouseEventArgs> Click { get; set; } = null;         

        public Color ButtonBackColor { get { return buttonBackColor; } set { buttonBackColor = value; Invalidate(); } }
        public Color MouseOverBackColor { get { return mouseOverBackColor; } set { mouseOverBackColor = value; Invalidate(); } }
        public Color MouseDownBackColor { get { return mouseDownBackColor; } set { mouseDownBackColor = value; Invalidate(); } }

        public string Text { get { return text; } set { text = value; Invalidate(); } }
        public ContentAlignment TextAlign { get { return textAlign; } set { textAlign = value; Invalidate(); } }
        public Color ForeColor { get { return foreColor; } set { foreColor = value; Invalidate(); } }       // of text

        public GLButtonBase()
        {
            InvalidateOnEnterLeave = true;
            InvalidateOnMouseDownUp = true;
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

        private string text;
        private Color buttonBackColor { get; set; } = Color.Gray;
        private Color mouseOverBackColor { get; set; } = Color.Green;
        private Color mouseDownBackColor { get; set; } = Color.YellowGreen;
        private Color foreColor { get; set; } = Color.Black;
        private ContentAlignment textAlign { get; set; } = ContentAlignment.MiddleCenter;

    }

    public class GLButton : GLButtonBase
    {
        public float ButtonColorScaling
        {
            get { return buttonColorScaling; }
            set
            {
                if (float.IsNaN(value) || float.IsInfinity(value))
                    return;
                else if (buttonColorScaling != value)
                {
                    buttonColorScaling = value;
                    Invalidate();
                }
            }
        }

        public GLButton()
        {
            Padding = new Padding(1);       // standard format, a border with a pad of 1
            BorderWidth = 1;
            BorderColor = Color.Yellow;
        }

        public GLButton(string name, Rectangle location, string text, Color backcolour) : this()
        {
            Name = name;
            Text = text;
            if (location.Width == 0 || location.Height == 0)
            {
                location.Width = location.Height = 10;  // nominal
                AutoSize = true;
            }
            Bounds = location;
           
            BackColor = backcolour;
        }

        public override void PerformSize()
        {
            base.PerformSize();
            if ( AutoSize )
            {
                SizeF size = new Size(0, 0);
                if ( Text.HasChars() )
                    size = BaseUtils.BitMapHelpers.MeasureStringInBitmap(Text, Font, ControlHelpersStaticFunc.StringFormatFromContentAlignment(TextAlign));
                if (Image != null)
                    size = new SizeF(size.Width+Image.Width, Math.Max(Image.Height,(int)(size.Height+0.999)));

                Size = new Size((int)(size.Width + 0.999) + Margin.TotalWidth + Padding.TotalWidth + BorderWidth + 4,
                                 (int)(size.Height + 0.999) + Margin.TotalHeight + Padding.TotalHeight + BorderWidth + 4);
            }
        }

        public override void Paint(Rectangle area, Graphics gr)
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

            //tbd not filling top line
            using (var b = new LinearGradientBrush(area, colBack, colBack.Multiply(buttonColorScaling), 90))
                gr.FillRectangle(b, area);       // linear grad brushes do not respect smoothing mode, btw

            if (Image != null)
            {
                base.DrawImage(Image, area, gr);
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

        private Color buttonBorderColor { get; set; } = Color.Brown;
        private float buttonColorScaling = 0.5F;

    }
}
