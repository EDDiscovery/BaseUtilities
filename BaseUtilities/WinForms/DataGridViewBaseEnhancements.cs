﻿/*
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

using System;
using System.Windows.Forms;

namespace BaseUtils
{
    // Allows for alternate context menus for column and row header clicks
    
    // Class, on mouse down, computes the hit type, Hit Button, and HitIndex.
    // Using RightClickRow/LeftClickRow you can tell in your mouse down if the click was on a valid row

    // if cell is in edit mode, and it has a member called ReturnPressedInEditMode, it can handle return differently

    // fix the #1487 issue for all inheritors, https://stackoverflow.com/questions/34344499/invalidoperationexception-this-operation-cannot-be-performed-while-an-auto-fill
    // by making sure the top left header cell is present

    public class DataGridViewBaseEnhancements : DataGridView
    {
        public ContextMenuStrip ColumnHeaderMenuStrip { get; set; } = null;
        public ContextMenuStrip RowHeaderMenuStrip { get; set; } = null;
        public ContextMenuStrip TopLeftHeaderMenuStrip { get; set; } = null;

        public bool SingleRowSelect { get; set; } = true;           // if true, only accept clicks on cells if a single row is selected

        public DataGridViewHitTestType HitType { get; private set; }
        public MouseButtons HitButton { get; private set; }
        public int HitIndex { get; private set; }

        public bool RowSelect { get { return HitType == DataGridViewHitTestType.Cell || HitType == DataGridViewHitTestType.RowHeader; } }

        public bool RightClickRowValid { get { return RightClickRow >= 0; } }
        public int RightClickRow { get { return RowSelect && HitButton == MouseButtons.Right ? HitIndex : -1; } }
        public bool LeftClickRowValid { get { return LeftClickRow >= 0; } }
        public int LeftClickRow { get { return RowSelect && HitButton == MouseButtons.Left ? HitIndex : -1; } }

        private ContextMenuStrip defaultstrip = null;
        private bool cmschangingoverride = false;

        public DataGridViewBaseEnhancements()
        {
        }

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



    }
}
