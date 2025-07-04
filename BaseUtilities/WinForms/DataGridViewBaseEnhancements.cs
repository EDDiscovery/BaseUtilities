/*
 * Copyright © 2020-2024 EDDiscovery development team
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
using System.Windows.Forms;

namespace BaseUtils
{
    // Allows for alternate context menus for column and row header clicks
    
    // Class, on mouse down, computes the hit type, Hit Button, and HitIndex.
    // Using RightClickRow/LeftClickRow you can tell in your mouse down if the click was on a valid row

    // if cell is in edit mode, and it has a member called ReturnPressedInEditMode, it can handle return differently

    // fix the #1487 issue for all inheritors, https://stackoverflow.com/questions/34344499/invalidoperationexception-this-operation-cannot-be-performed-while-an-auto-fill
    // by making sure the top left header cell is present

    // also add in a ColumnFillWeight change event

    // Add in autosort by column name option

    public class DataGridViewBaseEnhancements : DataGridView
    {
        // alternate context menus for parts of grid
        public ContextMenuStrip ColumnHeaderMenuStrip { get; set; } = null;
        public ContextMenuStrip RowHeaderMenuStrip { get; set; } = null;
        public ContextMenuStrip TopLeftHeaderMenuStrip { get; set; } = null;

        // if true, only accept clicks on cells if a single row is selected
        public bool SingleRowSelect { get; set; } = true;           

        // on mouse down, set cell type, button pressed, row or column index or -1
        public DataGridViewHitTestType HitType { get; private set; }
        public MouseButtons HitButton { get; private set; }
        public int HitIndex { get; private set; }

        // is row selected
        public bool RowSelect { get { return HitType == DataGridViewHitTestType.Cell || HitType == DataGridViewHitTestType.RowHeader; } }

        // if right on row, row number, else -1
        public int RightClickRow { get { return RowSelect && HitButton == MouseButtons.Right ? HitIndex : -1; } }
        public bool RightClickRowValid { get { return RightClickRow >= 0; } }
        public DataGridViewRow ClickedRightRow { get { return RowSelect && HitButton == MouseButtons.Right ? Rows[HitIndex] : null; } }

        // if left click on row, row number, else -1
        public int LeftClickRow { get { return RowSelect && HitButton == MouseButtons.Left ? HitIndex : -1; } }
        public bool LeftClickRowValid { get { return LeftClickRow >= 0; } }
        public DataGridViewRow ClickedLeftRow { get { return RowSelect && HitButton == MouseButtons.Left ? Rows[HitIndex] : null; } }

        // if either click on row, row number, else -1
        public int ClickRow { get { return RowSelect ? HitIndex : -1; } }
        public bool ClickRowValid { get { return ClickRow >= 0; } }
        public DataGridViewRow ClickedRow { get { return RowSelect ? Rows[HitIndex] : null; } }

        // callback on weight change

        public Action<object, DataGridViewColumnEventArgs, bool> ColumnFillWeightChanged = null;   // add missing ColumnFillWeight change (bool = first time)

        // if set, sort is selected by column name, which has a column name containing somewhere in it one of: Numeric, Date, AlphaInt
        public bool AutoSortByColumnName { get; set; } = false; 

        // get rows and work out grid height including headers
        public int CalculateGridHeightByContents(DataGridViewElementStates vs = DataGridViewElementStates.Visible)
        {
            int gridheight = this.Rows.GetRowsHeight(vs);
            gridheight += 1 * this.RowCount + this.ColumnHeadersHeight + 2;
            return gridheight;
        }

        public DataGridViewBaseEnhancements()
        {
        }

        // return text size if placed in a column
        public Size CalculateTextSize(string text, int colindex)
        {
            using (Graphics g = Parent.CreateGraphics())        // just work out size of text by using the same GR as parent
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; // recommended
                SizeF sz = g.MeasureString(text + ".", this.Font, new SizeF(Columns[colindex].Width - 4, 30000));
                return new Size((int)(sz.Width + 0.99), (int)(sz.Height + 0.99));
            }
        }

        // return fractional percent value of text vs data grid height in a column
        public float CalculateTextHeightPercentage(string text, int colindex)
        {
            using (Graphics g = Parent.CreateGraphics())        // just work out size of text by using the same GR as parent
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; // recommended
                SizeF sz = g.MeasureString(text + ".", this.Font, new SizeF(Columns[colindex].Width - 4, 30000));
                return sz.Height / (float)Height;
            }
        }

        // Add a row, return the row object
        public DataGridViewRow Add(Object[] cells)
        {
            int rowno = Rows.Add(cells);
            return Rows[rowno];
        }

        public Action<DataGridViewCell, Rectangle, Point> HoverOverCell;

        public void EnableCellHoverOverCallback()
        {
            this.CellMouseEnter += (s, e) =>
            {
                if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
                {
                    var cell = this[e.ColumnIndex, e.RowIndex];
                    var rec = GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    var sloc = PointToScreen(rec.Location);

                    HoverOverCell?.Invoke(cell, rec, sloc);
                }
            };
        }

        #region Implementation
        // Touching the TopLeftHeaderCell here prevents
        // System.InvalidOperationException: This operation cannot be performed while an auto-filled column is being resized.

        protected override void OnHandleCreated(EventArgs e)
        {
            var hc = this.TopLeftHeaderCell;        // this works, before base function. doing this.TopLeftHeaderCell = new .. does not work
            //System.Diagnostics.Debug.WriteLine("Fix #1487 for " + this.Name); // name is not reliable due to when handle create is called in sequence
            base.OnHandleCreated(e);
        }

        protected override void OnContextMenuStripChanged(EventArgs e)
        {
            if ( !cmschangingoverride )
                defaultstrip = ContextMenuStrip;
        }

        private void SetCMS(ContextMenuStrip c)
        {
            cmschangingoverride = true;
            ContextMenuStrip = c;
            cmschangingoverride = false;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            HitButton = e.Button;

            var ht = HitTest(e.X, e.Y);
            HitType = ht.Type;

            if (HitType == DataGridViewHitTestType.Cell)
            {
                if (!SingleRowSelect || this.IsAllSelectionsOnSameRow())
                {
                    if (e.Button == MouseButtons.Right && SingleRowSelect)
                    {
                        ClearSelection();                // select row under cursor.
                        Rows[ht.RowIndex].Selected = true;
                    }

                    HitIndex = ht.RowIndex;
                    SetCMS(defaultstrip);
                }
                else
                    HitType = DataGridViewHitTestType.None;     // cancel
            }
            else if (HitType == DataGridViewHitTestType.ColumnHeader)
            {
                HitIndex = ht.ColumnIndex;
                SetCMS((e.Button == MouseButtons.Right && ColumnHeaderMenuStrip != null) ? ColumnHeaderMenuStrip : defaultstrip);
            }
            else if (HitType == DataGridViewHitTestType.RowHeader)
            {
                HitIndex = ht.RowIndex;
                SetCMS((e.Button == MouseButtons.Right && RowHeaderMenuStrip != null) ? RowHeaderMenuStrip : defaultstrip);
            }
            else if (HitType == DataGridViewHitTestType.TopLeftHeader)
            {
                HitIndex = -1;
                SetCMS((e.Button == MouseButtons.Right && TopLeftHeaderMenuStrip != null) ? TopLeftHeaderMenuStrip : defaultstrip);
            }
            else
                SetCMS(defaultstrip);

            base.OnMouseDown(e);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            //System.Diagnostics.Debug.WriteLine("PDK " + keyData + "on " + CurrentCell.ColumnIndex + ":" + CurrentCell.RowIndex);

            Keys key = (keyData & Keys.KeyCode);

            if (key == Keys.Return)
            {
                if (CurrentCell.EditType.GetMethod("ReturnPressedInEditMode") != null)
                {
                    dynamic ak = EditingControl;
                    if (ak.ReturnPressedInEditMode())
                        return true;
                }
            }
            return base.ProcessDialogKey(keyData);
        }

        protected override void OnColumnWidthChanged(DataGridViewColumnEventArgs e)
        {
            base.OnColumnWidthChanged(e);

            bool there = FillWeight.TryGetValue(e.Column, out float fillw);
            bool fire = !there || fillw != e.Column.FillWeight;

            FillWeight[e.Column] = e.Column.FillWeight;
            if (fire && ColumnFillWeightChanged != null)
                ColumnFillWeightChanged(this, e, !there);
        }

        protected override void OnSortCompare(DataGridViewSortCompareEventArgs e)
        {
            if (!AutoSortByColumnName)
                base.OnSortCompare(e);
            else
            {
              //  System.Diagnostics.Debug.WriteLine($"Autosort {Name} col {e.Column.Index} on {e.Column.Name}");
                if (e.Column.Name.Contains("Numeric"))
                    e.SortDataGridViewColumnNumeric();
                else if (e.Column.Name.Contains("Date"))
                    e.SortDataGridViewColumnDate();
                else if (e.Column.Name.Contains("AlphaInt"))
                    e.SortDataGridViewColumnAlphaInt();
                else
                    base.OnSortCompare(e);
            }
        }



        private ContextMenuStrip defaultstrip = null;
        private bool cmschangingoverride = false;
        private Dictionary<DataGridViewColumn, float> FillWeight = new Dictionary<DataGridViewColumn, float>();

        #endregion
    }
}
