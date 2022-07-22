/*
 * Copyright © 2020-2021 EDDiscovery development team
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

using System.Windows.Forms;

namespace BaseUtils
{
    public class DataGridViewColumnControl : DataGridViewBaseEnhancements
    {
        public bool ColumnReorder { get; set; } = true;                     // default is to allow column reordering via right click dragging
        public bool PerColumnWordWrapControl { get; set; } = true;                     // default is to allow column reordering via right click dragging

        public DataGridViewColumnControl()
        {
            columnContextMenu = new ContextMenuStrip();
            columnContextMenu.Opening += ColumnContextMenu_Opening;

            TopLeftHeaderMenuStrip = ColumnHeaderMenuStrip = columnContextMenu;

            var dummy = new System.Windows.Forms.ToolStripMenuItem();       // need to have this, otherwise it won't work
            columnContextMenu.Items.Add(dummy);
        }

        public void SetWordWrap(bool state)
        {
            SuspendLayout();
            foreach (DataGridViewColumn c in Columns)
                c.DefaultCellStyle.WrapMode = DataGridViewTriState.NotSet;

            DefaultCellStyle.WrapMode = state ? DataGridViewTriState.True : DataGridViewTriState.False;
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
        }

        private void ColumnContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // if we have dragged, and not onto the HitIndex, which was the initial click point, we have a move column
            if (ColumnReorder && dragto >= 0 && dragto != HitIndex)
            {
                e.Cancel = true;        // cancel the menu
                Columns[HitIndex].DisplayIndex = Columns[dragto].DisplayIndex;      // move the display index
                dragto = -1;    // cancel the drag
            }
            else
            {
                columnContextMenu.Items.Clear();

                foreach (DataGridViewColumn c in this.Columns)      // add in ticks for all columns
                {
                    var tsitem = new System.Windows.Forms.ToolStripMenuItem();
                    tsitem.Checked = true;
                    tsitem.CheckOnClick = true;
                    tsitem.CheckState = c.Visible ? CheckState.Checked : CheckState.Unchecked;
                    tsitem.Text = c.HeaderText.HasChars() ? c.HeaderText : ("C" + (c.Index + 1).ToString());
                    tsitem.Tag = c;
                    tsitem.Size = new System.Drawing.Size(178, 22);
                    tsitem.Click += Tsitem_Click;
                    columnContextMenu.Items.Add(tsitem);
                }

                if (PerColumnWordWrapControl)
                {
                    var tsww = new System.Windows.Forms.ToolStripMenuItem();
                    tsww.Checked = true;
                    var globalwrapmode = this.DefaultCellStyle.WrapMode;
                    tsww.CheckState = Columns[HitIndex].DefaultCellStyle.WrapMode == DataGridViewTriState.True ? CheckState.Checked :
                                                        Columns[HitIndex].DefaultCellStyle.WrapMode == DataGridViewTriState.NotSet ? CheckState.Indeterminate : CheckState.Unchecked;
                    tsww.Text = "Column Word Wrap (Override)".TxID("DataGridView", "WordWrap");
                    tsww.Tag = Columns[HitIndex];
                    tsww.Size = new System.Drawing.Size(178, 22);
                    tsww.Click += Tsww_Click;
                    columnContextMenu.Items.Add(tsww);
                }
            }
        }

        private void Tsitem_Click(object sender, System.EventArgs e)
        {
            var t = sender as ToolStripMenuItem;
            var c = t.Tag as DataGridViewColumn;

            int colshidden = this.ColumnsHidden();

            if (colshidden < ColumnCount - 1 || t.Checked)        // not at last column, or its turning on, action, can't remove last one
                c.Visible = t.Checked;
        }

        private void Tsww_Click(object sender, System.EventArgs e)
        {
            var t = sender as ToolStripMenuItem;
            var c = t.Tag as DataGridViewColumn;

            var globalwrapmode = this.DefaultCellStyle.WrapMode;

            if (t.CheckState == CheckState.Indeterminate)
            {
                c.DefaultCellStyle.WrapMode = globalwrapmode == DataGridViewTriState.True ? DataGridViewTriState.False : DataGridViewTriState.True;
            }
            else if (t.CheckState == CheckState.Checked)
            {
                c.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            }
            else
            {
                c.DefaultCellStyle.WrapMode = DataGridViewTriState.NotSet;
            }

            t.CheckState = c.DefaultCellStyle.WrapMode == DataGridViewTriState.True ? CheckState.Checked :
                                                            c.DefaultCellStyle.WrapMode == DataGridViewTriState.NotSet ? CheckState.Indeterminate : CheckState.Unchecked;
        }

        private int dragto = -1;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // if allowed to reorder, and we are right clicked 
            if (ColumnReorder && e.Button == MouseButtons.Right)
            {
                var ht = HitTest(e.X, e.Y);
                if (ht.Type == DataGridViewHitTestType.ColumnHeader)    // on a column header, remember the dragto position
                {
                    dragto = ht.ColumnIndex;
                    Cursor.Current = Cursors.SizeWE;  // cursor gets changed to show
                }
                else
                {
                    dragto = -1;
                    Cursor.Current = Cursors.Default;       // not over a column, cancel the drag and cursor
                }
            }
        }

        private System.Windows.Forms.ContextMenuStrip columnContextMenu;
    }
}

