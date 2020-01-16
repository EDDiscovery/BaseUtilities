using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    // its a vertical scrollable list. Adding elements to it adds it to the 
    public class GLListBox : GLForeDisplayBase
    {
        public Action<GLBaseControl, int> SelectedIndexChanged { get; set; } = null;     // not fired by programatically 
        public Action<GLBaseControl, KeyEventArgs> OtherKeyPressed { get; set; } = null;     // not fired by programatically

        public List<string> Items { get { return items; } set { items = value; Invalidate(); PerformLayout(); } }
        public List<Image> ImageItems { get { return images; } set { images = value; Invalidate(); PerformLayout(); } }
        public int[] ItemSeperators { get { return itemSeperators; } set { itemSeperators = value; Invalidate(); } }

        public int SelectedIndex { get { return selectedIndex; } set { SetSelectedIndex(value); } }
        public string Text { get { return (items != null && selectedIndex>=0)?items[selectedIndex]:null; } set { SetSelectedIndex(value); } }

        public bool FitToItemsHeight { get { return fitToItemsHeight; } set { fitToItemsHeight = value; Invalidate(); } }
        public bool FitImagesToItemHeight { get { return fitImagesToItemHeight; } set { fitImagesToItemHeight = value; Invalidate(); } }

        public int DropDownHeightMaximum { get { return dropDownHeightMaximum; } set { dropDownHeightMaximum = value; InvalidateLayoutParent(); } }

        public Color MouseOverBackColor { get { return mouseOverBackColor; } set { mouseOverBackColor = value; Invalidate(); } }
        public Color ItemSeperatorColor { get { return itemSeperatorColor; } set { itemSeperatorColor = value; Invalidate(); } }

        // scroll bar
        public Color ArrowColor { get { return scrollbar.ArrowColor; } set { scrollbar.ArrowColor = value; } }       // of text
        public Color SliderColor { get { return scrollbar.SliderColor; } set { scrollbar.SliderColor = value; } }
        public Color ArrowButtonColor { get { return scrollbar.ArrowButtonColor; } set { scrollbar.ArrowButtonColor = value; } }
        public Color ArrowBorderColor { get { return scrollbar.ArrowBorderColor; } set { scrollbar.ArrowBorderColor = value; } }
        public float ArrowUpDrawAngle { get { return scrollbar.ArrowUpDrawAngle; } set { scrollbar.ArrowUpDrawAngle = value; } }
        public float ArrowDownDrawAngle { get { return scrollbar.ArrowDownDrawAngle; } set { scrollbar.ArrowDownDrawAngle = value; } }
        public float ArrowColorScaling { get { return scrollbar.ArrowColorScaling; } set { scrollbar.ArrowColorScaling = value; } }
        public Color MouseOverButtonColor { get { return scrollbar.MouseOverButtonColor; } set { scrollbar.MouseOverButtonColor = value; } }
        public Color MousePressedButtonColor { get { return scrollbar.MousePressedButtonColor; } set { scrollbar.MousePressedButtonColor = value; } }
        public Color ThumbButtonColor { get { return scrollbar.ThumbButtonColor; } set { scrollbar.ThumbButtonColor = value; } }
        public Color ThumbBorderColor { get { return scrollbar.ThumbBorderColor; } set { scrollbar.ThumbBorderColor = value; } }
        public float ThumbColorScaling { get { return scrollbar.ThumbColorScaling; } set { scrollbar.ThumbColorScaling = value; } }
        public float ThumbDrawAngle { get { return scrollbar.ThumbDrawAngle; } set { scrollbar.ThumbDrawAngle = value; } }

        public int ScrollBarWidth { get { return Font?.ScalePixels(20) ?? 20; } }

        public float GradientColorScaling
        {
            get { return gradientColorScaling; }
            set
            {
                if (float.IsNaN(value) || float.IsInfinity(value))
                    return;
                else if (gradientColorScaling != value)
                {
                    gradientColorScaling = value;
                    Invalidate();
                }
            }
        }

        public GLListBox()
        {
            Focusable = true;
            scrollbar = new GLScrollBar();
            scrollbar.Name = "GLLSB";
            scrollbar.Dock = DockingType.Right;
            scrollbar.SmallChange = 1;
            scrollbar.LargeChange = 1;
            scrollbar.Width = 20;
            scrollbar.Visible = false;
            scrollbar.Scroll += (s,e) => { if (firstindex != e.NewValue) { firstindex = e.NewValue; Invalidate(); } };
            Add(scrollbar);
        }

        public GLListBox(string n, Rectangle pos, List<string> texts, Color backcolor) : this()
        {
            items = texts;
            Focusable = true;
            Name = n;
            Bounds = pos;
            BackColor = backcolor;
        }

        public override void OnFontChanged()
        {
            PerformLayout();
        }

        public override void PerformSize()
        {
            base.PerformSize();
            if (AutoSize)       // measure text size and number of items to get idea of space required. Allow for scroll bar
            {
                int items = (Items != null) ? Items.Count() : 0;        
                SizeF max = new SizeF(ScrollBarWidth*2,0);
                if ( items>0)
                {
                    using (StringFormat f = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap })
                    {
                        foreach (var s in Items)
                        {
                            SizeF cur = BaseUtils.BitMapHelpers.MeasureStringInBitmap(s, Font, f);
                            if (cur.Width > max.Width)
                                max.Width = cur.Width;
                        }
                    }
                }
                int fh = (int)Font.GetHeight() + 2;
                Size = new Size((int)max.Width+ScrollBarWidth+8,Math.Min(items*fh+4,DropDownHeightMaximum));
            }
        }


        public override void PerformLayout()
        {
            if (scrollbar != null)  
                scrollbar.Width = ScrollBarWidth;       // set width 

            base.PerformLayout();                       // layout, scroll bar autodocks right

            if (Font != null)
            {
                int items = (Items != null) ? Items.Count() : 0;

                itemheight = (int)Font.GetHeight() + 2;

                displayableitems = ClientRectangle.Height / itemheight;            // number of items to display

                if (items > 0 && displayableitems > items)
                    displayableitems = items;

                //System.Diagnostics.Debug.WriteLine("List box" + mainarea + " " + items + "  " + displayableitems);

                if (items > displayableitems)
                {
                    scrollbar.Maximum = Items.Count - displayableitems;
                    scrollbar.Visible = true;
                }
                else
                    scrollbar.Visible = false;
            }
        }

        public override void Paint(Rectangle area, Graphics gr)
        {
           // System.Diagnostics.Debug.WriteLine("Paint List box");
            if (items != null && items.Count > 0)
            {
                Rectangle ca = ClientRectangle;
                Rectangle itemarea = new Rectangle(area.Left, area.Top, ca.Width - (scrollbar.Visible ? scrollbar.Width : 0), ca.Height);     // total width area
                itemarea.Height = itemheight;

                Rectangle textarea = itemarea;      // where we draw text
                Rectangle imagearea = itemarea;     // where we draw the images

                if (images != null)           // if we have images, allocate space between the 
                {
                    if (FitImagesToItemHeight)
                    {
                        imagearea = new Rectangle(imagearea.X, imagearea.Y, itemheight - 1, itemheight - 1);
                        textarea.X += imagearea.Width + 1;
                    }
                    else
                    {
                        int maxwidth = images.Max(x => x.Width);
                        textarea.X += maxwidth;
                        imagearea.Width = maxwidth;
                    }
                }

                if ( selectedindexset )     // we set the selected index, move to this and set focus to it, make sure its displayed
                {
                    focusindex = SelectedIndex;
                    if (focusindex > firstindex)         
                        firstindex = focusindex;
                    else if (focusindex >= firstindex + displayableitems)
                        firstindex = focusindex - displayableitems - 1;

                    selectedindexset = false;
                }

                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (StringFormat f = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap })
                using (Brush textb = new SolidBrush(this.ForeColor))
                using (Brush highlight = new SolidBrush(MouseOverBackColor))
                {
                    int offset = 0;

                    foreach (string s in items)
                    {   // if not fitting to items height, 
                        if (offset >= firstindex && offset < firstindex + displayableitems + (FitToItemsHeight ? 0 : 1))
                        {
                            if (offset == focusindex)
                            {
                                gr.FillRectangle(highlight, itemarea);
                            }

                            if (images != null && offset < images.Count)
                            {
                                gr.DrawImage(images[offset], imagearea);
                                //System.Diagnostics.Debug.WriteLine(offset + " Image is " + imagearea);
                            }

                            gr.DrawString(s, this.Font, textb, textarea, f);

                            if (itemSeperators != null && Array.IndexOf(itemSeperators, offset) >= 0)
                            {
                                using (Pen p = new Pen(ItemSeperatorColor))
                                {
                                    gr.DrawLine(p, new Point(textarea.Left, textarea.Top), new Point(textarea.Right, textarea.Top));
                                }
                            }

                            itemarea.Y += itemheight;
                            textarea.Y = imagearea.Y = itemarea.Y;
                        }

                        offset++;
                    }
                }

                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            }
        }

        private void SetSelectedIndex(int i)
        {
            if (items != null)
            {
                if (i >= 0 && i < items.Count)
                {
                    selectedIndex = i;
                    selectedindexset = true;
                    Invalidate();
                }
            }
        }

        private void SetSelectedIndex(string s, StringComparison c = StringComparison.InvariantCultureIgnoreCase)
        {
            if (items != null)
            {
                int i = items.FindIndex((x) => x.Equals(s, c));
                if (i >= 0)
                    SetSelectedIndex(i);
            }
        }

        protected void ScrollUpOne()
        {
            if (firstindex > 0)
            {
                firstindex--;
                scrollbar.Value = firstindex;
                Invalidate();
            }
        }
        protected void ScrollDownOne()
        {
            if (Items != null && firstindex < Items.Count() - displayableitems)
            {
                firstindex++;
                scrollbar.Value = firstindex;
                Invalidate();
            }
        }
        protected void FocusUpOne()
        {
            if (focusindex > 0)
            {
                focusindex--;
                Invalidate();
                if (focusindex < firstindex)
                    ScrollUpOne();
            }
        }
        protected void FocusDownOne()
        {
            if (Items != null && focusindex < Items.Count() - 1)
            {
                focusindex++;
                Invalidate();
                if (focusindex >= firstindex + displayableitems)
                    ScrollDownOne();
            }
        }

        public override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if ( !e.Handled)
            {
                if (items != null && itemheight > 0)       // if any items and we have done a calc layout.. just to check
                {
                    int index = firstindex + e.Location.Y / itemheight;

                    if (index >= 0 && index < items.Count)
                    {
                        selectedIndex = index;
                        SelectedIndexChanged?.Invoke(this, SelectedIndex);
                    }
                }

            }
        }

        public override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!e.Handled)
            {
                if (e.Delta > 0)
                    ScrollUpOne();
                else
                    ScrollDownOne();
            }
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!e.Handled)
            {
                if (items != null && itemheight > 0)  // may not have been set yet
                {
                    int y = e.Location.Y;
                    int index = (y / itemheight) + firstindex;
                    if (index < items.Count)
                    {
                        focusindex = index;
                        Invalidate();
                    }
                }
            }
        }


        public override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled)
            {
                //System.Diagnostics.Debug.WriteLine("KDown " + Name + " " + e.KeyCode);

                if (e.KeyCode == System.Windows.Forms.Keys.Up)
                {
                    FocusUpOne();
                }
                else if (e.KeyCode == System.Windows.Forms.Keys.Down)
                {
                    FocusDownOne();
                }

                if ((e.KeyCode == System.Windows.Forms.Keys.Enter || e.KeyCode == System.Windows.Forms.Keys.Return) || (e.Alt && (e.KeyCode == System.Windows.Forms.Keys.Up || e.KeyCode == System.Windows.Forms.Keys.Down)))
                {
                    selectedIndex = focusindex;
                    SelectedIndexChanged?.Invoke(this, SelectedIndex);
                }

                if (e.KeyCode == System.Windows.Forms.Keys.Delete || e.KeyCode == System.Windows.Forms.Keys.Escape || e.KeyCode == System.Windows.Forms.Keys.Back)
                {
                    OtherKeyPressed?.Invoke(this, e);
                }
            }
        }

        private bool fitToItemsHeight { get; set; } = true;              // if set, move the border to integer of item height.
        private bool fitImagesToItemHeight { get; set; } = false;        // if set images scaled to fit within item height
        private Color mouseOverBackColor { get; set; } = Color.Green;
        private float gradientColorScaling = 0.5F;
        private Color itemSeperatorColor { get; set; } = Color.Red;
        private GLScrollBar scrollbar;
        private List<string> items;
        private List<Image> images;
        private int[] itemSeperators { get; set; } = null;     // set to array giving index of each separator
        private int selectedIndex { get; set; } = -1;
        private int itemheight;
        private int displayableitems;
        private int firstindex = 0;
        private int focusindex = -1;
        private bool selectedindexset = false;
        private int dropDownHeightMaximum = 400;

    }
}
