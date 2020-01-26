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
    public class GLComboBox : GLBaseControl
    {

        public Color ForeColor { get { return foreColor; } set { foreColor = value; Invalidate(); } }       // of text
        public string Text { get { return dropdownbox.Text; } set { dropdownbox.Text = value; Invalidate(); } }

        // list box

        public List<string> Items { get { return dropdownbox.Items; } set { dropdownbox.Items = value; } }
        public List<Image> ImageItems { get { return dropdownbox.ImageItems; } set { dropdownbox.ImageItems = value; } }
        public int[] ItemSeperators { get { return dropdownbox.ItemSeperators; } set { dropdownbox.ItemSeperators = value;  } }

        public int SelectedIndex { get { return dropdownbox.SelectedIndex; } set { dropdownbox.SelectedIndex = value; Invalidate();} }

        public int DropDownHeightMaximum { get { return dropdownbox.DropDownHeightMaximum; } set { dropdownbox.DropDownHeightMaximum = value; } }

        public Color MouseOverBackColor { get { return dropdownbox.MouseOverBackColor; } set { dropdownbox.MouseOverBackColor = value; } }
        public Color DropDownBackgroundColor { get { return dropdownbox.BackColor; } set { dropdownbox.BackColor = value; } }
        public Color ItemSeperatorColor { get { return dropdownbox.ItemSeperatorColor; } set { dropdownbox.ItemSeperatorColor = value; } }

        // scroll bar
        public Color ArrowColor { get { return dropdownbox.ArrowColor; } set { dropdownbox.ArrowColor = value; } }       // of text
        public Color SliderColor { get { return dropdownbox.SliderColor; } set { dropdownbox.SliderColor = value; } }
        public Color ArrowButtonColor { get { return dropdownbox.ArrowButtonColor; } set { dropdownbox.ArrowButtonColor = value; } }
        public Color ArrowBorderColor { get { return dropdownbox.ArrowBorderColor; } set { dropdownbox.ArrowBorderColor = value; } }
        public float ArrowUpDrawAngle { get { return dropdownbox.ArrowUpDrawAngle; } set { dropdownbox.ArrowUpDrawAngle = value; } }
        public float ArrowDownDrawAngle { get { return dropdownbox.ArrowDownDrawAngle; } set { dropdownbox.ArrowDownDrawAngle = value; } }
        public float ArrowColorScaling { get { return dropdownbox.ArrowColorScaling; } set { dropdownbox.ArrowColorScaling = value; } }
        public Color MouseOverButtonColor { get { return dropdownbox.MouseOverButtonColor; } set { dropdownbox.MouseOverButtonColor = value; } }
        public Color MousePressedButtonColor { get { return dropdownbox.MousePressedButtonColor; } set { dropdownbox.MousePressedButtonColor = value; } }
        public Color ThumbButtonColor { get { return dropdownbox.ThumbButtonColor; } set { dropdownbox.ThumbButtonColor = value; } }
        public Color ThumbBorderColor { get { return dropdownbox.ThumbBorderColor; } set { dropdownbox.ThumbBorderColor = value; } }
        public float ThumbColorScaling { get { return dropdownbox.ThumbColorScaling; } set { dropdownbox.ThumbColorScaling = value; } }
        public float ThumbDrawAngle { get { return dropdownbox.ThumbDrawAngle; } set { dropdownbox.ThumbDrawAngle = value; } }

        public float DisabledScaling
        {
            get { return disabledScaling; }
            set
            {
                if (float.IsNaN(value) || float.IsInfinity(value))
                    return;
                else if (disabledScaling != value)
                {
                    disabledScaling = value;
                    Invalidate();
                }
            }
        }

        public GLComboBox()
        {
            InvalidateOnEnterLeave = true;
            Focusable = true;
            dropdownbox.Visible = false;
            dropdownbox.Name = "CB-Dropdown";
            dropdownbox.SelectedIndexChanged += SelectedIndexChanged;
            dropdownbox.OtherKeyPressed += OtherKeyPressed;
        }

        public GLComboBox(string name, Rectangle location, List<string> itms, Color backcolour) : this()
        {
            Name = name;
            Items = itms;
            Bounds = location;
            if (location.Width == 0 || location.Height == 0)
            {
                location.Width = location.Height = 10;
                AutoSize = true;
            }
            BackColor = backcolour;
        }

        private GLListBox dropdownbox = new GLListBox();
        private Color foreColor { get; set; } = Color.Black;
        private float disabledScaling = 0.5F;

        public override void Paint(Rectangle area, Graphics gr)
        {
            string text = Text;

            if (text != null)
            {
                bool enabled = Enabled && Items.Count > 0;

                if (enabled && Hover)
                    DrawBack(area, gr, MouseOverBackColor, MouseOverBackColor.Multiply(0.5f), BackColorGradient);
              
                int arrowwidth = Font.ScalePixels(20);

                int texthorzspacing = 1;
                Rectangle textbox = new Rectangle(area.X, area.Y, area.Width - arrowwidth - 2 * texthorzspacing, area.Height);
                Rectangle arrowbox = new Rectangle(area.Right - arrowwidth, area.Y, arrowwidth, area.Height);

                using (var fmt = new StringFormat())
                {
                    fmt.Alignment = StringAlignment.Near;
                    using (Brush textb = new SolidBrush(enabled ? this.ForeColor : this.ForeColor.Multiply(DisabledScaling)))
                    {
                        gr.DrawString(text, Font, textb, textbox, fmt);
                    }
                }

                if (enabled)
                {
                    int hoffset = arrowbox.Width / 12 + 2;
                    int voffset = arrowbox.Height / 4;
                    Point arrowpt1 = new Point(arrowbox.Left + hoffset, arrowbox.Y + voffset);
                    Point arrowpt2 = new Point(arrowbox.XCenter(), arrowbox.Bottom - voffset);
                    Point arrowpt3 = new Point(arrowbox.Right - hoffset, arrowpt1.Y);

                    Point arrowpt1c = new Point(arrowpt1.X, arrowpt2.Y);
                    Point arrowpt2c = new Point(arrowpt2.X, arrowpt1.Y);
                    Point arrowpt3c = new Point(arrowpt3.X, arrowpt2.Y);

                    using (Pen p2 = new Pen(ForeColor))
                    {
                        if (dropdownbox.Visible)
                        {
                            gr.DrawLine(p2, arrowpt1c, arrowpt2c);            // the arrow!
                            gr.DrawLine(p2, arrowpt2c, arrowpt3c);
                        }
                        else
                        {
                            gr.DrawLine(p2, arrowpt1, arrowpt2);            // the arrow!
                            gr.DrawLine(p2, arrowpt2, arrowpt3);
                        }
                    }
                }
            }
        }

        public override void OnMouseClick(GLMouseEventArgs e)
        {
            base.OnMouseClick(e);
            if ( e.Button == GLMouseEventArgs.MouseButtons.Left )
            {
                Activate();
            }
        }

        public override void OnKeyDown(GLKeyEventArgs e)
        {
            base.OnKeyDown(e);

            if ( !e.Handled && Items.Count>0)
            { 
                if (SelectedIndex < 0)
                    SelectedIndex = 0;

                if (e.Alt && (e.KeyCode == System.Windows.Forms.Keys.Up || e.KeyCode == System.Windows.Forms.Keys.Down))
                {
                    Activate();
                }
                else if (e.KeyCode == System.Windows.Forms.Keys.Up || e.KeyCode == System.Windows.Forms.Keys.Left)
                {
                    if (SelectedIndex > 0)
                    {
                        SelectedIndex = SelectedIndex - 1;
                    }
                }
                else if (e.KeyCode == System.Windows.Forms.Keys.Down || e.KeyCode == System.Windows.Forms.Keys.Right)
                {
                    if (SelectedIndex < Items.Count - 1)
                    {
                        SelectedIndex = SelectedIndex + 1;
                    }
                }
            }
        }


        private void Activate()
        {
            bool activatable = Enabled && Items.Count > 0 && Parent != null;

            if (!activatable)
                return;

            dropdownbox.Bounds = new Rectangle(Left, Bottom + 1, Width, Height);
            dropdownbox.BackColor = BackColor;
            dropdownbox.AutoSize = true;
            dropdownbox.Font = Font;
            dropdownbox.DropDownHeightMaximum = 100;
            dropdownbox.Visible = true;
            Parent.Add(dropdownbox);
            dropdownbox.SetFocus();
        }

        public virtual void SelectedIndexChanged(GLBaseControl c, int v)
        {
            dropdownbox.Visible = false;
            Parent.Remove(dropdownbox);
            SetFocus();
            Invalidate();
        }

        public virtual void OtherKeyPressed(GLBaseControl c, GLKeyEventArgs e)
        {
            if ( e.KeyCode == System.Windows.Forms.Keys.Escape)
            {
                dropdownbox.Visible = false;
                Parent.Remove(dropdownbox);
                SetFocus();
                Invalidate();
            }

        }



    }
}
