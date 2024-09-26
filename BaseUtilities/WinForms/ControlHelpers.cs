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

    // move all controls after startcontrol by shift in Y
    static public void ShiftControls(this Control.ControlCollection coll, Control startcontrol, Point offset)
    {
        bool active = false;
        foreach (Control c in coll)
        {
            System.Diagnostics.Debug.WriteLine($"Control {c.Name} at {c.Location} {c.Size}");
            if (c == startcontrol)
                active = true;
            else if (active)
                c.Location = new Point(c.Location.X + offset.X, c.Location.Y + offset.Y);
        }
    }

    // move all controls at or below y by offset, and optionally change size
    static public void ShiftControls(this Control control, int y, Point offset, bool adjustheight = true)
    {
        List<Control> controls = new List<Control>();
        foreach (Control c in control.Controls)
        {
            if (c.Top >= y)
                controls.Add(c);
        }

        foreach (Control c in controls)
        {
            c.Location = new Point(c.Location.X + offset.X, c.Location.Y + offset.Y);
        }

        if (adjustheight)
            control.Height -= offset.Y;
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

    // this class applies an anchor to a control, with its initial location/size, its minimum size
    static public void ApplyAnchor(this Control c, AnchorStyles ac, Point originalcontrolloc, Size originalcontrolsize, Size minsize, int widthdelta, int heightdelta)
    {
        if (ac == AnchorStyles.None)
            return;

        //System.Diagnostics.Debug.WriteLine("Control {0} is at {1}, initialpos {2} ", c.Name, c.Location, initialpos);
        int left = originalcontrolloc.X;
        int width = originalcontrolsize.Width;
        if ((ac & AnchorStyles.Right) != 0)     // if anchored right
        {
            if ((ac & AnchorStyles.Left) != 0)  // if anchored right and left, we need to change its width
            {
                width = Math.Max(minsize.Width, originalcontrolsize.Width + widthdelta);
            }
            else
            {
                left = Math.Max(0,originalcontrolloc.X + widthdelta);       // slide to position, don't allow it to slide off the left
            }
        }

        int top = originalcontrolloc.Y;
        int height = originalcontrolsize.Height;
        if ((ac & AnchorStyles.Bottom) != 0)
        {
            if ((ac & AnchorStyles.Top) != 0)
            {
                height = Math.Max(minsize.Height, originalcontrolsize.Height + heightdelta);
            }
            else
            {
                top = Math.Max(0, originalcontrolloc.Y + heightdelta);
            }
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



    #region Screen Alignment

    static public Size SizeWithinScreen(this Control p, Size size, int wmargin = 128, int hmargin = 128)
    {
        Screen scr = Screen.FromPoint(p.Location);
        Rectangle scrb = scr.WorkingArea;
        //System.Diagnostics.Debug.WriteLine("Screen is " + scrb);
        return new Size(Math.Min(size.Width, scrb.Width - wmargin), Math.Min(size.Height, scrb.Height - hmargin));
    }

    static public Rectangle ScreenRectangleAvailable(this Point p)
    {
        Screen scr = Screen.FromPoint(p);
        return new Rectangle(p.X, p.Y, scr.WorkingArea.Width - (p.X-scr.WorkingArea.X), scr.WorkingArea.Height - (p.Y-scr.WorkingArea.Y));
    }

    public enum VerticalAlignment { Top, Middle, Bottom };

    // the Location has been set to the initial pos, then rework to make sure it shows on screen. Locky means try to keep to Y position unless its too small
    static public void PositionSizeWithinScreen(this Control p, int wantedwidth, int wantedheight, bool lockY,
                                                    Size margin, HorizontalAlignment? halign = null, VerticalAlignment? vertalign = null, int scrollbarallowwidth = 0)
    {
        var rect = p.Location.CalculateRectangleWithinScreen(wantedwidth, wantedheight, lockY, margin, halign, vertalign, scrollbarallowwidth);
        p.Bounds = rect;
    }

    static public Rectangle CalculateRectangleWithinScreen(this Point position, int wantedwidth, int wantedheight, bool lockY,
                                                Size margin, HorizontalAlignment? halign = null, VerticalAlignment? vertalign = null, int scrollbarallowwidth = 0)
    {
        Screen scr = Screen.FromPoint(position);
        Rectangle wa = scr.WorkingArea;

        int left = position.X;
        int width = Math.Min(wantedwidth, wa.Width - margin.Width * 2);         // ensure within screen limits taking off margins

        if (halign == HorizontalAlignment.Right)
        {
            left = wa.Left + Math.Max(wa.Width-margin.Width-width, margin.Width);               
        }
        else if (halign == HorizontalAlignment.Center)
        {
            left = wa.Left + wa.Width / 2 - width / 2;
        }
        else if (halign == HorizontalAlignment.Left)
        {
            left = wa.Left + margin.Width;
        }

        int top = position.Y;
        int height = Math.Min(wantedheight, wa.Height - margin.Height * 2);        // ensure within screen

        if (vertalign == VerticalAlignment.Bottom )
        {
            top = wa.Top + Math.Max(wa.Height - margin.Height - height, margin.Height);
        }
        else if (vertalign == VerticalAlignment.Middle)
        {
            top = wa.Top + wa.Height / 2 - height / 2;
        }
        else if (vertalign == VerticalAlignment.Top)
        {
            top = wa.Top + margin.Height;
        }

        int botscreen = wa.Bottom;

        int availableh = botscreen - top - margin.Height;                        // available height from top to bottom less margin

        if (height > availableh)                                            // if not enough height available
        {
            if (lockY && availableh >= wa.Height / 4)                     // if locky and available is reasonable
            {
                height = availableh;                                        // lock height to it, keep y
            }
            else
            {
                top = wa.Top + Math.Max(margin.Height, wa.Height - margin.Height - height);      // at least margin, or at least height-margin-wantedheight
                height = Math.Min(wa.Height - margin.Height * 2, height);        // and limit to margin*2
            }

            width += scrollbarallowwidth;                                   // need a scroll bar
        }

        if (left + width >= wa.Right - margin.Width)                      // too far right
        {
            left = wa.Right - margin.Width - width;
        }

        if (left < wa.Left + margin.Width)                                // too far left
        {
            left = wa.Left + margin.Width;
        }

        return new Rectangle(left, top, width, height);
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

    static public ToolTip FindToolTipControl(this Control c)
    {
        var p = c.Parent;
        while (p != null)
        {
            var cc = p as System.ComponentModel.IContainer;
            if (cc != null)
            {
                var clisttt = cc.Components.OfType<ToolTip>().ToList(); // find all tooltips
                if (clisttt.Count > 0)
                    return clisttt[0];
            }
            p = p.Parent;
        }

        return null;
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
}
