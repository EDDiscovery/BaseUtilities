
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
using System.Windows.Forms;

public static partial class ControlHelpersStaticFunc
{
   
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

}
