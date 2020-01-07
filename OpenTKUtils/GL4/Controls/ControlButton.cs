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
        public GLButtonBase()
        {
            InvalidateOnEnterLeave = true;
            InvalidateOnMouseDownUp = true;
        }

        public Action<GLBaseControl, MouseEventArgs> Click { get; set; } = null;         

        public Color ButtonBackColor { get { return buttonBackColor; } set { buttonBackColor = value; Invalidate(); } }
        public Color MouseOverBackColor { get { return mouseOverBackColor; } set { mouseOverBackColor = value; Invalidate(); } }
        public Color MouseDownBackColor { get { return mouseDownBackColor; } set { mouseDownBackColor = value; Invalidate(); } }

        public string Text { get { return text; } set { text = value; Invalidate(); } }
        public ContentAlignment TextAlign { get { return textAlign; } set { textAlign = value; Invalidate(); } }
        public Color ForeColor { get { return foreColor; } set { foreColor = value; Invalidate(); } }       // of text

        private string text;
        private Color buttonBackColor { get; set; } = Color.Gray;
        private Color mouseOverBackColor { get; set; } = Color.Green;
        private Color mouseDownBackColor { get; set; } = Color.YellowGreen;
        private Color foreColor { get; set; } = Color.White;
        private ContentAlignment textAlign { get; set; } = ContentAlignment.MiddleCenter;

        public override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button == MouseEventArgs.MouseButtons.Left)
                OnClick(e);
        }

        public virtual void OnClick(MouseEventArgs e)
        {
            Click?.Invoke(this, e);
        }
    }

    public class GLButton : GLButtonBase
    {
        public Color ButtonBorderColor { get { return buttonBorderColor; } set { buttonBorderColor = value; Invalidate(); } }

        public float BorderColorScaling
        {
            get { return borderColorScaling; }
            set
            {
                if (float.IsNaN(value) || float.IsInfinity(value))
                    return;
                else if (borderColorScaling != value)
                {
                    borderColorScaling = value;
                    Invalidate();
                }
            }
        }

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

        private Color buttonBorderColor { get; set; } = Color.Brown;
        private float borderColorScaling = 1.25F;
        private float buttonColorScaling = 0.5F;

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

        public override void Paint(Bitmap bmp, Rectangle area, Graphics gr)
        {
            Color colBack = Color.Empty;
            Color colBorder = Color.Empty;

            if ( Enabled == false )
            {
                colBack = ButtonBackColor.Multiply(DisabledScaling);
                colBorder = ButtonBorderColor.Multiply(DisabledScaling);
            }
            else if (MouseButtonsDown == MouseEventArgs.MouseButtons.Left )
            {
                colBack = MouseDownBackColor;
                colBorder = ButtonBorderColor.Multiply(borderColorScaling);
            }
            else if (Hover)
            {
                colBack = MouseOverBackColor;
                colBorder = ButtonBorderColor.Multiply(borderColorScaling);
            }
            else
            {
                colBack = ButtonBackColor;
                colBorder = ButtonBorderColor;
            }

            Rectangle border = area;
            border.Width--; border.Height--;

            Rectangle buttonarea = area;
            buttonarea.Inflate(-1, -1);

            if (buttonarea.Width >= 1 && buttonarea.Height >= 1)  // ensure size
            {
                using (var b = new LinearGradientBrush(buttonarea, colBack, colBack.Multiply(buttonColorScaling), 90))
                    gr.FillRectangle(b, buttonarea);       // linear grad brushes do not respect smoothing mode, btw
            }

            gr.SmoothingMode = SmoothingMode.None;

            if (border.Width >= 1 && border.Height >= 1)        // ensure it does not except
            {
                using (var p = new Pen(colBorder))
                    gr.DrawRectangle(p, border);
            }

            if (buttonarea.Width < 1 || buttonarea.Height < 1)  // and no point drawing any more in the button area if its too small, it will except
                return;

            if (Image != null)
            {
                base.DrawImage(Image, buttonarea, gr);
            }

            gr.SmoothingMode = SmoothingMode.AntiAlias;

            if (!string.IsNullOrEmpty(Text))
            {
                using (var fmt = ControlHelpersStaticFunc.StringFormatFromContentAlignment(TextAlign))
                {
                    using (Brush textb = new SolidBrush((Enabled) ? this.ForeColor : this.ForeColor.Multiply(DisabledScaling)))
                    {
                        gr.DrawString(this.Text, this.Font, textb, buttonarea, fmt);
                    }
                }
            }

            gr.SmoothingMode = SmoothingMode.None;

        }

    }
}
