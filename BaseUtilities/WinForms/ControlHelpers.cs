/*
 * Copyright © 2016 - 2022 EDDiscovery development team
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
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

public static partial class ControlHelpersStaticFunc
{
    #region Control, Control Collections

    static public void DisposeTree(this Control c, int lvl)     // pass lvl = 0 to dispose of this object itself..
    {
        //System.Diagnostics.Debug.WriteLine(lvl + " at " + c.GetType().Name + " " + c.Name);
        List<Control> todispose = new List<Control>();

        foreach (Control s in c.Controls)
        {
            //System.Diagnostics.Debug.WriteLine(".. " + s.GetType().Name + " " + s.Name);
            if (s.Controls.Count > 0)
            {
                //System.Diagnostics.Debug.WriteLine(lvl + " Go into " + s.GetType().Name + " " + s.Name);
                s.DisposeTree(lvl + 1);
            }

            if (!(s is SplitterPanel))        // owned by their SCs..
                todispose.Add(s);
        }

        foreach (Control s in todispose)
        {
            //System.Diagnostics.Debug.WriteLine(lvl + " Dispose " + s.GetType().Name + " " + s.Name);
            s.Dispose();
        }

        if ( lvl ==0 && !( c is SplitterPanel))
        {
            //System.Diagnostics.Debug.WriteLine(lvl + " Dispose " + c.GetType().Name + " " + c.Name);
            c.Dispose();
        }
    }

    static public void DumpTree(this Control c, int lvl)
    {
        System.Diagnostics.Debug.WriteLine("                                             ".Substring(0,lvl*2) + "Control " + c.GetType().Name + ":" + c.Name + c.Location + c.Size);

        foreach (Control s in c.Controls)
        {
            s.DumpTree(lvl + 1);
        }
    }

    static public int RunActionOnTree(this Control c, Predicate<Control> cond, Action<Control> action )       // Given a condition, run action on it, count instances
    {
        //System.Diagnostics.Debug.WriteLine("Raot: " + c.Parent.GetType().Name + "->" + c.GetType().Name + ":" + c.Name);
        bool istype = cond(c);

        if (istype)
        {
            //System.Diagnostics.Debug.WriteLine("Action on " + c.GetType().Name + " " + c.Name + " ==? " + istype);
            action(c);
        }

        int total = istype ? 1 : 0;

        foreach (Control s in c.Controls)                   // all sub controls get a chance to play!
            total += RunActionOnTree(s, cond, action);

        return total;
    }

    // DO a refresh after this. presumes you have sorted the order of controls added in the designer file
    // from C, offset either up/down dependent on on.  Remember in tag of c direction you shifted.  Don't shift if in same direction
    // useful for autolayout forms
    static public void ShiftControls(this Control.ControlCollection coll, Control c, int offset, bool on)
    {
        bool enabled = false;
        bool prevon = false;
        foreach (Control ctrl in coll)
        {
            if (ctrl == c)
            {
                prevon = (ctrl.Tag == null) ? true : (bool)ctrl.Tag;
                ctrl.Tag = on;
                enabled = prevon != on;
                //System.Diagnostics.Debug.WriteLine("Decided for enable " + enabled + " to " + on);
            }

            if (enabled)
            {
                ctrl.Location = new Point(ctrl.Left, ctrl.Top + ((on) ? offset : -offset));
                //System.Diagnostics.Debug.WriteLine("Control " + ctrl.Name + " to " + ctrl.Location + " offset " + offset + " on " + on);
            }
        }
    }

    static public void InsertRangeBefore(this Control.ControlCollection coll, Control startpoint, IEnumerable<Control> clist)
    {
        List<Control> cur = new List<Control>();
        foreach (Control c in coll)
        {
            if (c == startpoint)
                cur.AddRange(clist);

            cur.Add(c);
        }
        coll.Clear();
        coll.AddRange(cur.ToArray());
    }

    public static Size FindMaxSubControlArea(this Control parent, int hpad, int vpad, Type[] excludedtypes = null, bool debugout = false)
    {
        Size s = new Size(0, 0);
        foreach (Control c in parent.Controls)
        {
            if (excludedtypes == null || !excludedtypes.Contains(c.GetType()))
            {
                if (debugout)
                    System.Diagnostics.Debug.WriteLine("Control " + c.GetType().Name + " " + c.Name + " " + c.Location + " " + c.Size + " R " + c.Right + " B" + c.Bottom);
                s.Width = Math.Max(s.Width, c.Right);
                s.Height = Math.Max(s.Height, c.Bottom);
            }
        }

        if (debugout)
            System.Diagnostics.Debug.WriteLine("Control Measured " + s);
        s.Width += hpad;
        s.Height += vpad;
        return s;
    }

    static public void ApplyAnchor(this Control c, AnchorStyles ac, Point initialpos, Size initialsize, int widthdelta, int heightdelta)
    {
        if (ac == AnchorStyles.None)
            return;

        //System.Diagnostics.Debug.WriteLine("Control {0} is at {1}, initialpos {2} ", c.Name, c.Location, initialpos);
        int left = initialpos.X;
        int width = initialsize.Width;
        if ((ac & AnchorStyles.Right) != 0)
        {
            if ((ac & AnchorStyles.Left) != 0)
            {
                width = Math.Max(initialsize.Width, initialsize.Width + widthdelta);
            }
            else
                left = Math.Max(initialpos.X, initialpos.X + widthdelta);
        }

        int top = initialpos.Y;
        int height = initialsize.Height;
        if ((ac & AnchorStyles.Bottom) != 0)
        {
            if ((ac & AnchorStyles.Top) != 0)
            {
                height = Math.Max(initialsize.Height, initialsize.Height + heightdelta);
            }
            else
                top = Math.Max(initialpos.Y, initialpos.Y + heightdelta);
        }

        //System.Diagnostics.Debug.WriteLine("Move to {0} to {1} {2}", c.Name, new Point(left, top), new Size(width, height));
        c.Location = new Point(left, top);
        c.Size = new Size(width, height);
        //System.Diagnostics.Debug.WriteLine(".. results in {0} {1}", c.Location, c.Size);
    }

    static public Rectangle RectangleScreenCoords(this Control c)
    {
        Point p = c.PointToScreen(new Point(0, 0));
        return new Rectangle(p.X, p.Y, c.Width, c.Height);
    }

    static public void DebugSizePosition(this Control p, ToolTip t)     // assign info to tooltip
    {
        t.SetToolTip(p, p.Name + " " + p.Location + p.Size + "F:" + p.ForeColor + "B:" + p.BackColor);
        foreach (Control c in p.Controls)
            c.DebugSizePosition(t);
    }

    static public Control FirstY(this Control.ControlCollection cc, Type[] t)
    {
        int miny = int.MaxValue;
        int minx = int.MaxValue;
        Control highest = null;
        foreach (Control c in cc)
        {
            if (t.Contains(c.GetType()))
            {
                if (c.Top < miny || (c.Top == miny && c.Left < minx))
                {
                    miny = c.Top;
                    minx = c.Left;
                    highest = c;
                }
            }
        }

        return highest;
    }

    static public string GetHeirarchy(this Control c, bool name = false)
    {
        string str = c.GetType().Name + (name && c.Name.HasChars() ? (" '" + c.Name + "'") : "");
        while (c.Parent != null)
        {
            c = c.Parent;
            str = c.GetType().Name + (name && c.Name.HasChars() ? (" '" + c.Name + "'") : "") + ":" + str;
        }
        return str;
    }

    public static int XCenter(this Control c)
    {
        return (c.Right + c.Left) / 2;
    }

    public static int YCenter(this Control c)
    {
        return (c.Top + c.Bottom) / 2;
    }


    #endregion

    #region Content Align

    static public StringFormat StringFormatFromContentAlignment(ContentAlignment c)
    {
        StringFormat f = new StringFormat();
        if (c == ContentAlignment.BottomCenter || c == ContentAlignment.MiddleCenter || c == ContentAlignment.TopCenter)
            f.Alignment = StringAlignment.Center;
        else if (c == ContentAlignment.BottomLeft || c == ContentAlignment.MiddleLeft || c == ContentAlignment.TopLeft)
            f.Alignment = StringAlignment.Near;
        else
            f.Alignment = StringAlignment.Far;

        if (c == ContentAlignment.BottomCenter || c == ContentAlignment.BottomLeft || c == ContentAlignment.BottomRight)
            f.LineAlignment = StringAlignment.Far;
        else if (c == ContentAlignment.MiddleLeft || c == ContentAlignment.MiddleCenter || c == ContentAlignment.MiddleRight)
            f.LineAlignment = StringAlignment.Center;
        else
            f.LineAlignment = StringAlignment.Near;

        return f;
    }

    static public Rectangle ImagePositionFromContentAlignment(this ContentAlignment c, Rectangle client, Size image, bool cliptorectangle = false)
    {
        int left = client.Left;

        if (c == ContentAlignment.BottomCenter || c == ContentAlignment.MiddleCenter || c == ContentAlignment.TopCenter)
            left += Math.Max((client.Width - image.Width) / 2, 0);
        else if (c == ContentAlignment.BottomLeft || c == ContentAlignment.MiddleLeft || c == ContentAlignment.TopLeft)
            left += 0;
        else
            left += Math.Max(client.Width - image.Width, 0);

        int top = client.Top;

        if (c == ContentAlignment.BottomCenter || c == ContentAlignment.BottomLeft || c == ContentAlignment.BottomRight)
            top += Math.Max(client.Height - image.Height, 0);
        else if (c == ContentAlignment.MiddleLeft || c == ContentAlignment.MiddleCenter || c == ContentAlignment.MiddleRight)
            top += Math.Max((client.Height - image.Height) / 2, 0);
        else
            top += 0;

        if (cliptorectangle)        // ensure we start in rectangle..
        {
            left = Math.Max(0, left);
            top = Math.Max(0, top);
        }

        return new Rectangle(left, top, image.Width, image.Height);
    }

    #endregion

    #region Rectangles

    public static int XCenter(this Rectangle r)
    {
        return (r.Right + r.Left) / 2;
    }

    public static int YCenter(this Rectangle r)
    {
        return (r.Top + r.Bottom) / 2;
    }

    static public GraphicsPath RectCutCorners(int x, int y, int width, int height, int roundnessleft, int roundnessright)
    {
        GraphicsPath gr = new GraphicsPath();

        gr.AddLine(x + roundnessleft, y, x + width - 1 - roundnessright, y);
        gr.AddLine(x + width - 1, y + roundnessright, x + width - 1, y + height - 1 - roundnessright);
        gr.AddLine(x + width - 1 - roundnessright, y + height - 1, x + roundnessleft, y + height - 1);
        gr.AddLine(x, y + height - 1 - roundnessleft, x, y + roundnessleft);
        gr.AddLine(x, y + roundnessleft, x + roundnessleft, y);         // close figure manually, closing it with a break does not seem to work
        return gr;
    }

    // produce a rounded rectangle with a cut out at the top..

    static public GraphicsPath RectCutCorners(int x, int y, int width, int height, int roundnessleft, int roundnessright, int topcutpos, int topcutlength)
    {
        GraphicsPath gr = new GraphicsPath();

        if (topcutlength > 0)
        {
            gr.AddLine(x + roundnessleft, y, x + topcutpos, y);
            gr.StartFigure();
            gr.AddLine(x + topcutpos + topcutlength, y, x + width - 1 - roundnessright, y);
        }
        else
            gr.AddLine(x + roundnessleft, y, x + width - 1 - roundnessright, y);

        gr.AddLine(x + width - 1, y + roundnessright, x + width - 1, y + height - 1 - roundnessright);
        gr.AddLine(x + width - 1 - roundnessright, y + height - 1, x + roundnessleft, y + height - 1);
        gr.AddLine(x, y + height - 1 - roundnessleft, x, y + roundnessleft);
        gr.AddLine(x, y + roundnessleft, x + roundnessleft, y);         // close figure manually, closing it with a break does not seem to work
        return gr;
    }

    #endregion


    #region Screen Alignment

    static public Size SizeWithinScreen(this Control p, Size size, int wmargin = 128, int hmargin = 128)
    {
        Screen scr = Screen.FromPoint(p.Location);
        Rectangle scrb = scr.Bounds;
        //System.Diagnostics.Debug.WriteLine("Screen is " + scrb);
        return new Size(Math.Min(size.Width, scrb.Width - wmargin), Math.Min(size.Height, scrb.Height - hmargin));
    }

    public enum VerticalAlignment { Top, Middle, Bottom };

    // the Location has been set to the initial pos, then rework to make sure it shows on screen. Locky means try to keep to Y position unless its too small
    static public void PositionSizeWithinScreen(this Control p, int wantedwidth, int wantedheight, bool lockY, 
                                                    Size margin, HorizontalAlignment? halign = null, VerticalAlignment? vertalign = null, int scrollbarallowwidth = 0)
    {
        Screen scr = Screen.FromPoint(p.Location);
        Rectangle scrb = scr.Bounds;

        int left = p.Left;
        int width = Math.Min(wantedwidth, scrb.Width - margin.Width * 2);         // ensure within screen limits taking off margins

        if (halign == HorizontalAlignment.Right)
        {
            left = scr.Bounds.Left + Math.Max(scrb.Width-margin.Width-width, margin.Width);               
        }
        else if (halign == HorizontalAlignment.Center)
        {
            left = scr.Bounds.Left + scrb.Width / 2 - width / 2;
        }
        else if (halign == HorizontalAlignment.Left)
        {
            left = scr.Bounds.Left + margin.Width;
        }

        int top = p.Top;
        int height = Math.Min(wantedheight, scrb.Height - margin.Height * 2);        // ensure within screen

        if (vertalign == VerticalAlignment.Bottom )
        {
            top = scr.Bounds.Top + Math.Max(scrb.Height - margin.Height - height, margin.Height);
        }
        else if (vertalign == VerticalAlignment.Middle)
        {
            top = scr.Bounds.Top + scrb.Height / 2 - height / 2;
        }
        else if (vertalign == VerticalAlignment.Top)
        {
            top = scr.Bounds.Top + margin.Height;
        }

        int botscreen = scr.Bounds.Bottom;

        int availableh = botscreen - top - margin.Height;                        // available height from top to bottom less margin

        if (height > availableh)                                            // if not enough height available
        {
            if (lockY && availableh >= scrb.Height / 4)                     // if locky and available is reasonable
            {
                height = availableh;                                        // lock height to it, keep y
            }
            else
            {
                top = scr.Bounds.Top + Math.Max(margin.Height, scrb.Height - margin.Height - height);      // at least margin, or at least height-margin-wantedheight
                height = Math.Min(scrb.Height - margin.Height * 2, height);        // and limit to margin*2
            }

            width += scrollbarallowwidth;                                   // need a scroll bar
        }

        if (left + width >= scr.Bounds.Right - margin.Width)                      // too far right
        {
            left = scr.Bounds.Right - margin.Width - width;
        }

        if (left < scr.Bounds.Left + margin.Width)                                // too far left
        {
            left = scr.Bounds.Left + margin.Width;
        }


        //  System.Diagnostics.Debug.WriteLine("Pos " + new Point(left, top) + " size " + new Size(width,height));
        p.Location = new Point(left, top);
        p.Size = new Size(width, height);
    }

    static public Point PositionWithinRectangle(this Point p, Size ps, Rectangle other)      // clamp to within client rectangle of another
    {
        return new Point(Math.Min(p.X, other.Width - ps.Width),                   // respecting size, ensure we are within the rectangle of another
                                Math.Min(p.Y, other.Height - ps.Height));
    }

    #endregion



    #region Context Menu Strips

    public static string GetToolStripState( this ContextMenuStrip cms )     // semi colon list of checked items
    {
        string s = "";
        foreach( ToolStripItem c in cms.Items)
        {
            var t = c as ToolStripMenuItem;
            if (t != null)
            {
                if (t.CheckState == CheckState.Checked)
                    s += t.Name + ";";

                foreach (ToolStripItem d in t.DropDownItems)
                {
                    var dt = d as ToolStripMenuItem;
                    if (dt != null)
                    {
                        if (dt.CheckState == CheckState.Checked)
                            s += dt.Name + ";";
                    }
                }
            }
        }

        return s;
    }

    public static void SetToolStripState(this ContextMenuStrip cms, string ss)  // semi colon list of items to check
    {
        string[] s = ss.Split(';');

        foreach (ToolStripItem c in cms.Items)
        {
            var t = c as ToolStripMenuItem;
            if (t != null)
            {
                t.CheckState = s.Contains(t.Name) ? CheckState.Checked : CheckState.Unchecked;

                foreach (ToolStripItem d in t.DropDownItems)
                {
                    var dt = d as ToolStripMenuItem;
                    if (dt != null)
                    {
                        CheckState cs = s.Contains(dt.Name) ? CheckState.Checked : CheckState.Unchecked;
                        dt.CheckState = cs;
                    }
                }
            }
        }
    }

    #endregion

    #region Misc

    // this scales the font down only to fit into textarea given a graphic and text.  Used in Paint
    // fnt itself is not deallocated.
    public static Font GetFontToFit(this Graphics g, string text, Font fnt, Size textarea, StringFormat fmt)
    {
        if (!text.HasChars())       // can't tell
            return fnt;

        bool ownfont = false;
        while (true)
        {
            SizeF drawnsize = g.MeasureString(text, fnt, new Point(0, 0), fmt);

            if (fnt.Size < 2 || ((int)(drawnsize.Width + 0.99f) <= textarea.Width && (int)(drawnsize.Height + 0.99f) <= textarea.Height))
                return fnt;

            if (ownfont)
                fnt.Dispose();

            fnt = BaseUtils.FontLoader.GetFont(fnt.FontFamily.Name, fnt.Size - 0.5f, fnt.Style);
            ownfont = true;
        }
    }

    // this scales the font up or down to fit width and height.  Text is not allowed to wrap, its unformatted
    // fnt itself is not deallocated.
    public static Font GetFontToFit(string text, Font fnt, Size areasize)
    {
        if (!text.HasChars())       // can't tell
            return fnt;

        SizeF drawnsize = BaseUtils.BitMapHelpers.MeasureStringUnformattedLengthInBitmap(text, fnt);

        bool smallerthanbox = Math.Ceiling(drawnsize.Width) <= areasize.Width && Math.Ceiling(drawnsize.Height) < areasize.Height;
        float dir = smallerthanbox ? 0.5f : -0.5f;
        float fontsize = fnt.Size;
        System.Diagnostics.Debug.WriteLine($"Autofont {fnt.Name} {fnt.Size} fit {areasize} = {drawnsize} {smallerthanbox} dir {dir}");

        bool ownfont = false;

        while (true)
        {
            fontsize += dir;

            Font fnt2 = BaseUtils.FontLoader.GetFont(fnt.FontFamily.Name, fontsize, fnt.Style);

            drawnsize = BaseUtils.BitMapHelpers.MeasureStringUnformattedLengthInBitmap(text, fnt2);
            smallerthanbox = Math.Ceiling(drawnsize.Width) <= areasize.Width && Math.Ceiling(drawnsize.Height) < areasize.Height;

            System.Diagnostics.Debug.WriteLine($"Autofontnext  {fnt2.Name} {fnt2.Size} fit {areasize} = {drawnsize} {smallerthanbox} dir {dir}");

            // conditions to stop, betting too big, betting small enough, too small font
            if ((dir > 0 && !smallerthanbox) || (dir < 0 && smallerthanbox) || (dir < 0 && fnt.Size < 2))
            {
                fnt2.Dispose();
                return fnt;
            }
            else
            {
                if (ownfont)
                    fnt.Dispose();
                fnt = fnt2;
                ownfont = true;
            }
        }
    }




    public static Size MeasureItems(this Graphics g, Font fnt , string[] array, StringFormat fmt)
    {
        Size max = new Size(0, 0);
        foreach (string s in array)
        {
            SizeF f = g.MeasureString(s, fnt, new Point(0, 0), fmt);
            max = new Size(Math.Max(max.Width, (int)(f.Width + 0.99)), Math.Max(max.Height, (int)(f.Height + 0.99)));
        }

        return max;
    }

    public static int ScalePixels(this Font f, int nominalat12)      //given a font, and size at normal 12 point, what size should i make it now
    {
        return (int)(f.GetHeight() / 18 * nominalat12);
    }

    public static float ScaleSize(this Font f, float nominalat12)      //given a font, and size at normal 12 point, what size should i make it now
    {
        return f.GetHeight() / 18 * nominalat12;
    }


    static public SizeF CurrentAutoScaleFactor(this Form f)
    {
        if (f.AutoScaleMode == AutoScaleMode.None)      // if in autoscale none, CurrentAutoScaleDimensions returns 0,0 but we want a 1,1 return
            return new SizeF(1, 1);
        else
            return new SizeF(f.CurrentAutoScaleDimensions.Width / 6, f.CurrentAutoScaleDimensions.Height / 13);
    }

    static public SizeF InvCurrentAutoScaleFactor(this Form f)
    {
        if (f.AutoScaleMode == AutoScaleMode.None)      // if in autoscale none, CurrentAutoScaleDimensions returns 0,0 but we want a 1,1 return
            return new SizeF(1, 1);
        else
            return new SizeF(6 / f.CurrentAutoScaleDimensions.Width, 13 / f.CurrentAutoScaleDimensions.Height);
    }

    static public void CopyToolTips(this System.ComponentModel.IContainer c, Control outerctrl, Control[] ctrlitems)
    {
        if (c != null)
        {
            var clisttt = c.Components.OfType<ToolTip>().ToList(); // find all tooltips

            foreach (ToolTip t in clisttt)
            {
                string s = t.GetToolTip(outerctrl);
                if (s != null && s.Length > 0)
                {
                    foreach (Control inner in ctrlitems)
                        t.SetToolTip(inner, s);
                }
            }
        }
    }

    static public System.ComponentModel.IContainer GetParentContainerComponents(this Control p)
    {
        IContainerControl c = p.GetContainerControl();  // get container control (UserControl or Form)

        if (c != null)  // paranoia in case control is not connected
        {
            // find all fields, incl private of them
            var memcc = c.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

            var icontainers = (from f in memcc      // pick out a list of IContainers (should be only 1)
                               where f.FieldType.FullName == "System.ComponentModel.IContainer"
                               select f);
            return icontainers?.FirstOrDefault()?.GetValue(c) as System.ComponentModel.IContainer;  // But IT may be null if no containers are on the form
        }

        return null;
    }

    // used to compute ImageAttributes, given a disabled scaling, a remap table, and a optional color matrix
    static public void ComputeDrawnPanel(out ImageAttributes Enabled,
                    out ImageAttributes Disabled,
                    float disabledscaling, System.Drawing.Imaging.ColorMap[] remap, float[][] colormatrix = null)
    {
        Enabled = new ImageAttributes();
        Enabled.SetRemapTable(remap, ColorAdjustType.Bitmap);
        if (colormatrix != null)
            Enabled.SetColorMatrix(new ColorMatrix(colormatrix), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

        Disabled = new ImageAttributes();
        Disabled.SetRemapTable(remap, ColorAdjustType.Bitmap);

        if (colormatrix != null)
        {
            colormatrix[0][0] *= disabledscaling;     // the identity positions are scaled by BDS 
            colormatrix[1][1] *= disabledscaling;
            colormatrix[2][2] *= disabledscaling;
            Disabled.SetColorMatrix(new ColorMatrix(colormatrix), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        }
        else
        {
            float[][] disabledMatrix = {
                        new float[] {disabledscaling,  0,  0,  0, 0},        // red scaling factor of BDS
                        new float[] {0,  disabledscaling,  0,  0, 0},        // green scaling factor of BDS
                        new float[] {0,  0,  disabledscaling,  0, 0},        // blue scaling factor of BDS
                        new float[] {0,  0,  0,  1, 0},        // alpha scaling factor of 1
                        new float[] {0,0,0, 0, 1}};    // three translations of 0

            Disabled.SetColorMatrix(new ColorMatrix(disabledMatrix), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        }
    }

    #endregion

}
