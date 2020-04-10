
/*
 * Copyright © 2016 - 2019 EDDiscovery development team
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
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.Form;

public static class ControlHelpersStaticFunc
{
    #region Control

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

    #endregion

    #region Misc

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

    static public void CopyToolTips( this System.ComponentModel.IContainer c, Control outerctrl, Control[] ctrlitems)
    {
        if ( c != null )
        {
            var clisttt = c.Components.OfType<ToolTip>().ToList(); // find all tooltips

            foreach (ToolTip t in clisttt)
            {
                string s = t.GetToolTip(outerctrl);
                if (s != null && s.Length>0)
                {
                    foreach (Control inner in ctrlitems)
                        t.SetToolTip(inner, s);
                }
            }
        }
    }

    // used to compute ImageAttributes, given a disabled scaling, a remap table, and a optional color matrix
    static public void ComputeDrawnPanel( out ImageAttributes Enabled, 
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

    static public Size SizeWithinScreen(this Control p, Size size, int wmargin = 128, int hmargin = 128)
    {
        Screen scr = Screen.FromControl(p);
        Rectangle scrb = scr.Bounds;
        //System.Diagnostics.Debug.WriteLine("Screen is " + scrb);
        return new Size(Math.Min(size.Width, scrb.Width - wmargin), Math.Min(size.Height, scrb.Height - hmargin));
    }

    public enum VerticalAlignment { Top, Middle, Bottom };

    // the Location has been set to the initial pos, then rework to make sure it shows on screen. Locky means try to keep to Y position unless its too small
    static public void PositionSizeWithinScreen(this Control p, int wantedwidth, int wantedheight, bool lockY, 
                                                    int margin = 16, HorizontalAlignment? halign = null, VerticalAlignment? vertalign = null, int scrollbarallowwidth = 0)
    {
        Screen scr = Screen.FromControl(p);
        Rectangle scrb = scr.Bounds;

        int left = p.Left;
        int width = Math.Min(wantedwidth, scrb.Width - margin * 2);         // ensure within screen limits taking off margins

        if (halign == HorizontalAlignment.Right)
        {
            left = Math.Max(scrb.Width-margin-width, margin);               
        }
        else if (halign == HorizontalAlignment.Center)
        {
            left = scrb.Width / 2 - width / 2;
        }
        else if (halign == HorizontalAlignment.Left)
        {
            left = margin;
        }

        int top = p.Top;
        int height = Math.Min(wantedheight, scrb.Height - margin * 2);        // ensure within screen

        if (vertalign == VerticalAlignment.Bottom )
        {
            top = Math.Max(scrb.Height - margin - height, margin);
        }
        else if (vertalign == VerticalAlignment.Middle)
        {
            top = scrb.Height / 2 - height / 2;
        }
        else if (vertalign == VerticalAlignment.Top)
        {
            top = margin;
        }

        int availableh = scrb.Height - top - margin;                        // available height from top to bottom less margin

        if (height > availableh)                                            // if not enough height available
        {
            if (lockY && availableh >= scrb.Height / 4)                     // if locky and available is reasonable
                height = availableh;                                        // lock height to it, keep y
            else
            {
                top = Math.Max(margin, scrb.Height - margin - height);      // at least margin, or at least height-margin-wantedheight
                height = Math.Min(scrb.Height - margin * 2, height);        // and limit to margin*2
            }

            width += scrollbarallowwidth;                                   // need a scroll bar

            if (left + width >= scrb.Width)                                 // allow for width                            
                left -= scrollbarallowwidth;    
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

    #region Splitter

    static public void SplitterDistance(this SplitContainer sp, double value)           // set the splitter distance from a double value.. safe from exceptions.
    {
        if (!double.IsNaN(value) && !double.IsInfinity(value))
        {
            int a = (sp.Orientation == Orientation.Vertical) ? sp.Width : sp.Height;
            int curDist = sp.SplitterDistance;
            //System.Diagnostics.Debug.WriteLine("Size is " + a);
            if (a == 0)     // Sometimes the size is {0,0} if minimized. Calc dimension from the inner panels. See issue #1508.
                a = (sp.Orientation == Orientation.Vertical ? sp.Panel1.Width + sp.Panel2.Width : sp.Panel1.Height + sp.Panel2.Height) + sp.SplitterWidth;
            //System.Diagnostics.Debug.WriteLine("Now Size is " + a + " " + sp.Panel1MinSize + " " + (sp.Height - sp.Panel2MinSize));

            try
            {       // protect it against excepting because even with the careful protection above and below, it can still mess up if the window is completely small
                sp.SplitterDistance = Math.Min(Math.Max((int)Math.Round(a * value), sp.Panel1MinSize), a - sp.Panel2MinSize);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Splitter failed to set in " + sp.GetType().Name);
            }
            //System.Diagnostics.Debug.WriteLine($"SplitContainer {sp.Name} {sp.DisplayRectangle} {sp.Panel1MinSize}-{sp.Panel2MinSize} Set SplitterDistance to {value:N2} (was {curDist}, now {sp.SplitterDistance})");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"SplitContainer {sp.Name} {sp.DisplayRectangle} {sp.Panel1MinSize}-{sp.Panel2MinSize} Set SplitterDistance attempted with unsupported value ({value})");
        }
    }

    static public double GetSplitterDistance(this SplitContainer sp)                    // get the splitter distance as a fractional double
    {
        int a = (sp.Orientation == Orientation.Vertical) ? sp.Width : sp.Height;
        if (a == 0)     // Sometimes the size is {0,0} if minimized. Calc dimension from the inner panels. See issue #1508.
            a = (sp.Orientation == Orientation.Vertical ? sp.Panel1.Width + sp.Panel2.Width : sp.Panel1.Height + sp.Panel2.Height) + sp.SplitterWidth;
        double v = (double)sp.SplitterDistance / (double)a;
        //System.Diagnostics.Debug.WriteLine($"SplitContainer {sp.Name} {sp.DisplayRectangle} {sp.SplitterDistance} Get SplitterDistance {a} -> {v:N2}");
        return v;
    }

    static public double GetPanelsSizeSum(this SplitContainer sp)                    // get the splitter panels size sum
    {
        int a = (sp.Orientation == Orientation.Vertical) ? sp.Width : sp.Height;
        if (a == 0)     // Sometimes the size is {0,0} if minimized. Calc dimension from the inner panels. See issue #1508.
            a = (sp.Orientation == Orientation.Vertical ? sp.Panel1.Width + sp.Panel2.Width : sp.Panel1.Height + sp.Panel2.Height) + sp.SplitterWidth;
        double s = (double)a;
        //System.Diagnostics.Debug.WriteLine($"SplitContainer {sp.Name} {sp.DisplayRectangle} {sp.SplitterDistance} Get PanelSizeSum {a} -> {s:N2}");
        return s;
    }

    // Make a tree of splitters, controlled by the string in sp

    static public SplitContainer SplitterTreeMakeFromCtrlString(BaseUtils.StringParser sp, 
                                                            Func<Orientation, int, SplitContainer> MakeSC, 
                                                            Func<string, Control> MakeNode , int lvl)
    {
        char tomake;
        if (sp.SkipUntil(new char[] { 'H', 'V', 'U' }) && (tomake = sp.GetChar()) != 'U')
        {
            sp.IsCharMoveOn('(');   // ignore (

            SplitContainer sc = MakeSC(tomake == 'H' ? Orientation.Horizontal : Orientation.Vertical, lvl);

            double percent = sp.NextDouble(",") ?? 0.5;
            sc.SplitterDistance(percent);
            
            SplitContainer one = SplitterTreeMakeFromCtrlString(sp, MakeSC, MakeNode, lvl+1);

            if (one == null)
            {
                string para = sp.PeekChar() == '\'' ? sp.NextQuotedWord() : "";
                sc.Panel1.Controls.Add(MakeNode(para));
            }
            else
                sc.Panel1.Controls.Add(one);

            SplitContainer two = SplitterTreeMakeFromCtrlString(sp, MakeSC, MakeNode, lvl+1);

            if (two == null)
            {
                string para = sp.PeekChar() == '\'' ? sp.NextQuotedWord() : "";
                sc.Panel2.Controls.Add(MakeNode(para));
            }
            else
                sc.Panel2.Controls.Add(two);

            return sc;
        }
        else
            return null;
    }

    // Report control state of a tree of splitters

    static public string SplitterTreeState(this SplitContainer sc, string cur, Func<Control, string> getpara)
    {
        string state = sc.Orientation == Orientation.Horizontal ? "H( " : "V( ";
        state += sc.GetSplitterDistance().ToStringInvariant("0.##") + ", ";

        SplitContainer one = sc.Panel1.Controls[0] as SplitContainer;

        if (one != null)
        {
            string substate = SplitterTreeState(one, "", getpara);
            state = state + substate;
        }
        else
            state += "U'" + getpara(sc.Panel1.Controls[0]) + "'";

        state += ", ";
        SplitContainer two = sc.Panel2.Controls[0] as SplitContainer;

        if (two != null)
        {
            string substate = SplitterTreeState(two, "", getpara);
            state = state + substate;
        }
        else
            state += "U'" + getpara(sc.Panel2.Controls[0]) + "'";

        state += ") ";

        return state;
    }

    // Run actions at each Splitter Panel node

    static public void RunActionOnSplitterTree(this SplitContainer sc, Action<SplitterPanel, Control> action)       
    {
        SplitContainer one = sc.Panel1.Controls[0] as SplitContainer;

        if (one != null)
            RunActionOnSplitterTree(one, action);
        else
            action(sc.Panel1, sc.Panel1.Controls[0]);

        SplitContainer two = sc.Panel2.Controls[0] as SplitContainer;

        if (two != null)
            RunActionOnSplitterTree(two, action);
        else
            action(sc.Panel2, sc.Panel2.Controls[0]);
    }


    static public void Merge(this SplitContainer topsplitter , int panel )      // currentsplitter has a splitter underneath it in panel (0/1)
    {
        SplitContainer insidesplitter = (SplitContainer)topsplitter.Controls[panel].Controls[0];  // get that split container, error if not. 

        Control keep = insidesplitter.Panel1.Controls[0];      // we keep this control - the left/top one

        insidesplitter.Panel2.Controls[0].Dispose();        // and we dispose(close) the right/bot one

        insidesplitter.Panel1.Controls.Clear();             // clear the control list on the inside splitter so it does not kill the keep list
        insidesplitter.Dispose();                            // get rid of the inside splitter

        topsplitter.Controls[panel].Controls.Add(keep);     // add the keep list back onto the the top splitter panel.
    }

    static public void Split(this SplitContainer currentsplitter, int panel ,  SplitContainer sc, Control ctrl )    // currentsplitter, split panel into a SC with a ctrl
    {
        Control cur = currentsplitter.Controls[panel].Controls[0];      // what we current have attached..
        currentsplitter.Controls[panel].Controls.Clear();   // clear list
        sc.Panel1.Controls.Add(cur);
        sc.Panel2.Controls.Add(ctrl);
        currentsplitter.Controls[panel].Controls.Add(sc);
    }

    #endregion

    #region Data Grid Views

    static public void SortDataGridViewColumnNumeric(this DataGridViewSortCompareEventArgs e, string removetext= null)
    {
        string s1 = e.CellValue1?.ToString();
        string s2 = e.CellValue2?.ToString();

        if (removetext != null)
        {
            if ( s1 != null )
                s1 = s1.Replace(removetext, "");
            if ( s2 != null )
                s2 = s2.Replace(removetext, "");
        }

        double v1=0, v2=0;

        bool v1hasval = s1 != null && Double.TryParse(s1, out v1);
        bool v2hasval = s2 != null && Double.TryParse(s2, out v2);

        if (!v1hasval)
        {
            e.SortResult = 1;
        }
        else if (!v2hasval)
        {
            e.SortResult = -1;
        }
        else
        {
            e.SortResult = v1.CompareTo(v2);
        }

        e.Handled = true;
    }

    static public void SortDataGridViewColumnDate(this DataGridViewSortCompareEventArgs e, bool userowtagtodistinguish = false)
    {
        string s1 = e.CellValue1?.ToString();
        string s2 = e.CellValue2?.ToString();

        DateTime v1 = DateTime.MinValue, v2 = DateTime.MinValue;

        bool v1hasval = s1!=null && DateTime.TryParse(e.CellValue1?.ToString(), out v1);
        bool v2hasval = s2!=null && DateTime.TryParse(e.CellValue2?.ToString(), out v2);

        if (!v1hasval)
        {
            e.SortResult = 1;
        }
        else if (!v2hasval)
        {
            e.SortResult = -1;
        }
        else
        {
            e.SortResult = v1.CompareTo(v2);
        }

        if ( e.SortResult == 0 && userowtagtodistinguish)
        {
            var left = e.Column.DataGridView.Rows[e.RowIndex1].Tag;
            var right = e.Column.DataGridView.Rows[e.RowIndex2].Tag;
            if (left != null && right != null)
            {
                long lleft = (long)left;
                long lright = (long)right;

                e.SortResult = lleft.CompareTo(lright);
            }
        }

        e.Handled = true;
    }

    static public void SortDataGridViewColumnTagsAsStringsLists(this DataGridViewSortCompareEventArgs e, DataGridView dataGridView)
    {
        DataGridViewCell left = dataGridView.Rows[e.RowIndex1].Cells[4];
        DataGridViewCell right = dataGridView.Rows[e.RowIndex2].Cells[4];

        var lleft = left.Tag as List<string>;
        var lright = right.Tag as List<string>;

        if (lleft != null)
        {
            if (lright != null)
            {
                string sleft = string.Join(";", left.Tag as List<string>);
                string sright = string.Join(";", right.Tag as List<string>);
                e.SortResult = sleft.CompareTo(sright);
            }
            else
                e.SortResult = 1;       // left exists, right doesn't, its bigger (null is smaller)
        }
        else
            e.SortResult = lright != null ? -1 : 0;

        e.Handled = true;
    }

    static public void SortDataGridViewColumnTagsAsStrings(this DataGridViewSortCompareEventArgs e, DataGridView dataGridView)
    {
        DataGridViewCell left = dataGridView.Rows[e.RowIndex1].Cells[4];
        DataGridViewCell right = dataGridView.Rows[e.RowIndex2].Cells[4];

        var sleft = left.Tag as string;
        var sright = right.Tag as string;

        if (sleft != null)
        {
            if (sright != null)
            {
                e.SortResult = sleft.CompareTo(sright);
            }
            else
                e.SortResult = 1;       // left exists, right doesn't, its bigger (null is smaller)
        }
        else
            e.SortResult = sright != null ? -1 : 0;

        e.Handled = true;
    }

    // Find text in coloun

    static public int FindRowWithValue(this DataGridView grid, int coln, string text, StringComparison sc = StringComparison.InvariantCultureIgnoreCase)
    {
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.Cells[coln].Value.ToString().Equals(text,sc))
            {
                return row.Index;
            }
        }

        return -1;
    }

    // try and force this row to centre or top
    static public void DisplayRow(this DataGridView grid, int rown, bool centre)
    {
        int drows = centre ? grid.DisplayedRowCount(false) : 0;

        while (!grid.Rows[rown].Displayed && drows >= 0)
        {
            //System.Diagnostics.Debug.WriteLine("Set row to " + Math.Max(0, rowclosest - drows / 2));
            grid.FirstDisplayedScrollingRowIndex = Math.Max(0, rown - drows / 2);
            grid.Update();      //FORCE the update so we get an idea if its displayed
            drows--;
        }
    }

    public static void FilterGridView(this DataGridView vw, string searchstr, bool checktags = false)       // can be VERY SLOW for large grids
    {
        vw.SuspendLayout();
        vw.Enabled = false;

        bool[] visible = new bool[vw.RowCount];
        bool visibleChanged = false;

        foreach (DataGridViewRow row in vw.Rows.OfType<DataGridViewRow>())
        {
            bool found = false;

            if (searchstr.Length < 1)
                found = true;
            else
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value != null)
                    {
                        if (cell.Value.ToString().IndexOf(searchstr, 0, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (checktags)
                    {
                        List<string> slist = cell.Tag as List<string>;
                        if (slist != null)
                        {
                            if (slist.ContainsIn(searchstr, StringComparison.CurrentCultureIgnoreCase) >= 0)
                            {
                                found = true;
                                break;
                            }
                        }

                        string str = cell.Tag as string;
                        if (str != null)
                        {
                            if (str.IndexOf(searchstr, StringComparison.CurrentCultureIgnoreCase) >= 0)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                }
            }

            visible[row.Index] = found;
            visibleChanged |= found != row.Visible;
        }

        if (visibleChanged)
        {
            var selectedrow = vw.SelectedRows.OfType<DataGridViewRow>().Select(r => r.Index).FirstOrDefault();
            DataGridViewRow[] rows = vw.Rows.OfType<DataGridViewRow>().ToArray();
            vw.Rows.Clear();

            for (int i = 0; i < rows.Length; i++)
            {
                rows[i].Visible = visible[i];
            }

            vw.Rows.Clear();
            vw.Rows.AddRange(rows.ToArray());

            vw.Rows[selectedrow].Selected = true;
        }

        vw.Enabled = true;
        vw.ResumeLayout();
    }

    public static bool IsNullOrEmpty( this DataGridViewCell cell)
    {
        return cell.Value == null || cell.Value.ToString().Length == 0;
    }

    public static int GetNumberOfVisibleRowsAbove( this DataGridViewRowCollection table, int rowindex )
    {
        int visible = 0;
        for( int i = 0; i < rowindex; i++ )
        {
            if (table[i].Visible)
                visible++;
        }
        return visible;
    }

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

    public static Size FindMaxSubControlArea(this Control parent, int hpad, int vpad , Type[] excludedtypes = null, bool debugout = false)
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

        if (debugout )
            System.Diagnostics.Debug.WriteLine("Control Measured " + s);
        s.Width += hpad;
        s.Height += vpad;
        return s;
    }

    public static Font GetFontToFitRectangle(this Graphics g, string text, Font fnt, Rectangle textarea, StringFormat fmt)
    {
        bool ownfont = false;
        while (true)
        {
            SizeF drawnsize = g.MeasureString(text, fnt, new Point(0,0), fmt);

            if ((int)(drawnsize.Width + 0.99f) <= textarea.Width && (int)(drawnsize.Height + 0.99f) <= textarea.Height)
                return fnt;

            if (ownfont)
                fnt.Dispose();

            fnt = BaseUtils.FontLoader.GetFont(fnt.FontFamily.Name, fnt.Size - 0.5f, fnt.Style);
            ownfont = true;
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


    public static int XCenter(this Rectangle r)
    {
        return (r.Right + r.Left) / 2;
    }

    public static int YCenter(this Rectangle r)
    {
        return (r.Top + r.Bottom) / 2;
    }

    static public string GetHeirarchy(this Control c, bool name = false)
    {
        string str = c.GetType().Name + (name && c.Name.HasChars() ? (" '" + c.Name + "'") : "");
        while ( c.Parent != null )
        {
            c = c.Parent;
            str = c.GetType().Name + (name && c.Name.HasChars() ? (" '" + c.Name + "'") : "") + ":" + str;
        }
        return str;
    }

    static public SizeF CurrentAutoScaleFactor(this Form f)
    {
        return new SizeF(f.CurrentAutoScaleDimensions.Width / 6, f.CurrentAutoScaleDimensions.Height / 13);
    }

    static public SizeF InvCurrentAutoScaleFactor(this Form f)
    {
        return new SizeF(6 / f.CurrentAutoScaleDimensions.Width, 13 / f.CurrentAutoScaleDimensions.Height);
    }

    static public Rectangle RectangleScreenCoords(this Control c)
    {
        Point p = c.PointToScreen(new Point(0, 0));
        return new Rectangle(p.X, p.Y, c.Width, c.Height);
    }

    static public void DebugSizePosition(this Control p, ToolTip t)     // assign info to tooltip
    {
        t.SetToolTip(p, p.Name + " " + p.Location + p.Size +"F:" + p.ForeColor + "B:" + p.BackColor);
        foreach (Control c in p.Controls)
            c.DebugSizePosition(t);
    }

    static public Control FirstY(this Control.ControlCollection cc, Type[] t)
    {
        int miny = int.MaxValue;
        int minx = int.MaxValue;
        Control highest = null;
        foreach( Control c in cc)
        {
            if (t.Contains(c.GetType()) )
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


    #endregion
}
