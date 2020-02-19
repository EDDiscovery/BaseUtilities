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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public class GLTableLayoutPanel : GLPanel
    {
        public GLTableLayoutPanel(string name, Rectangle location, Color back) : base(name, location, back)
        {
        }

        public GLTableLayoutPanel() : this("TLP?",DefaultWindowRectangle,DefaultControlBackColor)
        {
        }

        public struct Style
        {
            public enum SizeTypeEnum { Absolute, Relative, Autosize };
            public SizeTypeEnum SizeType { get; set; }
            public int Value { get; set; }

            public Style(SizeTypeEnum ste, int v) { SizeType = ste; Value = v; }
        }

        public List<Style> Rows { get { return rows; } set { rows = value; InvalidateLayout(); } }
        public List<Style> Columns { get { return columns; } set { columns = value; InvalidateLayout(); } }
        public GL4.Controls.Padding CellPadding { get { return cellPadding; } set { cellPadding = value; InvalidateLayout(); } }

        private List<Style> rows { get; set; } = null;
        private List<Style> columns { get; set; } = null;
        private GL4.Controls.Padding cellPadding { get; set; } = new Padding(1);

        // Sizing has been recursively done for all children
        // now we are laying out from top down

        public override void PerformRecursiveLayout()
        {
            bool okay = true;

            if (Columns != null && Rows != null)
            {
                int[] maxcolsize = new int[Columns.Count];
                int[] maxrowsize = new int[Rows.Count];
                Dictionary<Tuple<int, int>, List<GLBaseControl>> sortedbycell = new Dictionary<Tuple<int, int>, List<GLBaseControl>>();

                foreach (var c in ControlsZ)         // first let all children autosize
                {
                    if (c.Column < maxcolsize.Length && c.Row < maxrowsize.Length)
                    {
                        maxcolsize[c.Column] = Math.Max(maxcolsize[c.Column], c.Width + CellPadding.TotalWidth);
                        maxrowsize[c.Row] = Math.Max(maxrowsize[c.Row], c.Height + CellPadding.TotalHeight);

                        Tuple<int, int> ad = new Tuple<int, int>(c.Column, c.Row);
                        if (!sortedbycell.ContainsKey(ad))
                            sortedbycell[ad] = new List<GLBaseControl>();
                        sortedbycell[ad].Add(c);
                    }
                    else
                        okay = false;
                }

                if (okay)
                {
                    Rectangle panelarea = ClientRectangle;      // in terms of our client area

                    var cols = CalcPos(Columns, panelarea.Width, maxcolsize);       // calculate the positions
                    var rows = CalcPos(Rows, panelarea.Height, maxrowsize);

                    if (cols.Count > 0 && rows.Count > 0)
                    {
                        foreach (var k in sortedbycell)
                        {
                            var col = k.Key.Item1;
                            var row = k.Key.Item2;
                            var clist = k.Value;

                            Rectangle cellarea = new Rectangle(cols[col], rows[row], cols[col + 1] - cols[col], rows[row + 1] - rows[row]);
                            Rectangle flowarea = new Rectangle(cellarea.Left, cellarea.Top,0,0);
                            cellarea.X += CellPadding.Left;
                            cellarea.Width -= CellPadding.TotalWidth;
                            cellarea.Y += CellPadding.Top;
                            cellarea.Height -= CellPadding.TotalHeight;

                            foreach (GLBaseControl c in clist)
                            {
                                System.Diagnostics.Debug.WriteLine("Table layout " + c.Name + " " + cellarea);

                                if (c.Dock != DockingType.None)        // if docking,
                                    c.Layout(ref cellarea);     // allow docking to work in the cell area, it uses the area to set position
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("Top Left layout " + c.Name + " " + cellarea);
                                    c.SetLocationSizeNI(cellarea.Location, cellarea.Size, true);
                                }

                                c.PerformRecursiveLayout();
                            }
                        }
                    }
                    else
                        okay = false;
                }
            }
            else
                okay = false;

            if ( !okay )
                base.PerformRecursiveLayout();      // default
        }

        protected List<int> CalcPos(List<Style> cr, int available, int[] foundsizes)
        {
            int cabs = 0;
            int cweight = 0;
            for (int c = 0; c < cr.Count; c++)
            {
                if (cr[c].SizeType == Style.SizeTypeEnum.Absolute)
                    cabs += cr[c].Value;
                else if (cr[c].SizeType == Style.SizeTypeEnum.Autosize)
                    cabs += foundsizes[c];
                else
                    cweight += cr[c].Value;
            }

            List<int> xpos = new List<int>();

            int crelleft = available - cabs;

            if (cabs <= available && (cweight == 0 || crelleft > 0))
            {
                int x = 0;
                for (int c = 0; c < cr.Count; c++)
                {
                    xpos.Add(x);
                    if (cr[c].SizeType == Style.SizeTypeEnum.Absolute)
                        x += cr[c].Value;
                    else if (cr[c].SizeType == Style.SizeTypeEnum.Autosize)
                        x += foundsizes[c];
                    else
                        x += cr[c].Value * crelleft / cweight;
                }
                xpos.Add(x);
            }

            return xpos;
        }
    }
}

