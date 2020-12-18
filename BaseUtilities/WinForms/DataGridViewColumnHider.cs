/*
 * Copyright © 2020 EDDiscovery development team
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
    public class DataGridViewColumnHider : DataGridViewBaseEnhancements
    {
        public DataGridViewColumnHider()
        {
            columnContextMenu = new ContextMenuStrip();
            columnContextMenu.Opening += ColumnContextMenu_Opening;

            TopLeftHeaderMenuStrip = ColumnHeaderMenuStrip = columnContextMenu;

            var dummy = new System.Windows.Forms.ToolStripMenuItem();       // need to have this, otherwise it won't work
            columnContextMenu.Items.Add(dummy);
        }

        private void ColumnContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            columnContextMenu.Items.Clear();

            foreach (DataGridViewColumn c in this.Columns)      // add in ticks for all columns
            {
                var tsitem = new System.Windows.Forms.ToolStripMenuItem();
                tsitem.Checked = true;
                tsitem.CheckOnClick = true;
                tsitem.CheckState = c.Visible ? CheckState.Checked : CheckState.Unchecked;
                tsitem.Text = c.HeaderText.HasChars() ? c.HeaderText : ("C" + (c.Index+1).ToString());
                tsitem.Tag = c;
                tsitem.Size = new System.Drawing.Size(178, 22);
                tsitem.Click += Tsitem_Click;
                columnContextMenu.Items.Add(tsitem);
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


        private System.Windows.Forms.ContextMenuStrip columnContextMenu;
    }
}
