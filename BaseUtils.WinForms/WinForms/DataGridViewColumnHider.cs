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
            this.hideColumnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unhideAllColumnsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.columnContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hideColumnToolStripMenuItem,
            this.unhideAllColumnsToolStripMenuItem});
            this.columnContextMenu.Size = new System.Drawing.Size(179, 48);
            this.columnContextMenu.Opening += ColumnContextMenu_Opening;

            this.hideColumnToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.hideColumnToolStripMenuItem.Text = "Hide Column".Tx();
            this.hideColumnToolStripMenuItem.Click += HideColumnToolStripMenuItem_Click;

            this.unhideAllColumnsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.unhideAllColumnsToolStripMenuItem.Text = "Unhide all Columns".Tx();
            this.unhideAllColumnsToolStripMenuItem.Click += UnhideAllColumnsToolStripMenuItem_Click;

            TopLeftHeaderMenuStrip = ColumnHeaderMenuStrip = columnContextMenu;
        }

        private void ColumnContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int colshidden = this.ColumnsHidden();

            unhideAllColumnsToolStripMenuItem.Enabled =  colshidden> 0;
            hideColumnToolStripMenuItem.Enabled = HitIndex >= 0 && HitIndex < Columns.Count && Columns.Count - colshidden > 1;
        }

        private void UnhideAllColumnsToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            foreach(DataGridViewColumn c in this.Columns)
                c.Visible = true;
        }

        private void HideColumnToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (HitIndex >= 0)
                Columns[HitIndex].Visible = false;
        }

        private System.Windows.Forms.ContextMenuStrip columnContextMenu;
        private System.Windows.Forms.ToolStripMenuItem hideColumnToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unhideAllColumnsToolStripMenuItem;

    }
}
