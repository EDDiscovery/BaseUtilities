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

public static partial class DataGridViewControlHelpersStaticFunc
{
    // try and force this row to centre or top so its displayed - using Current is not good enough to ensure its on screen
    static public void DisplayRow(this DataGridView grid, int rown, bool centre)
    {
        grid.Update();  // force update so we get an updated display property - this seems to lag

        int drows = centre ? grid.DisplayedRowCount(false) : 0;

        while (!grid.Rows[rown].Displayed && drows >= 0)
        {
            //System.Diagnostics.Debug.WriteLine("Set row to " + Math.Max(0, rowclosest - drows / 2));

            grid.SafeFirstDisplayedScrollingRowIndex(Math.Max(0, rown - drows / 2));
            grid.Update();      //FORCE the update so we get an idea if its displayed
            drows--;
        }
    }

    public static bool IsNullOrEmpty(this DataGridViewCell cell)
    {
        return cell.Value == null || cell.Value.ToString().Length == 0;
    }

    public static int GetNumberOfVisibleRowsAbove(this DataGridViewRowCollection table, int rowindex)
    {
        int visible = 0;
        for (int i = 0; i < rowindex; i++)
        {
            if (table[i].Visible)
                visible++;
        }
        return visible;
    }

    // use instead of direct access for Mono compatibility

    public static int SafeFirstDisplayedScrollingRowIndex(this DataGridView dgv)
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            return dgv.CurrentCell != null ? dgv.CurrentCell.RowIndex : 0;
        }
        else
        {
            return dgv.FirstDisplayedScrollingRowIndex;
        }
    }

    // use instead of direct access for Mono compatibility and protection against exception

    public static void SafeFirstDisplayedScrollingRowIndex(this DataGridView dgv, int rowno)
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            // MONO does not implement SafeFirstDisplayedScrollingRowIndex
            if (rowno >= 0 && rowno < dgv.Rows.Count)
            {
                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    if (dgv.Columns[i].Visible)
                    {
                        while (!dgv.Rows[rowno].Visible && rowno < dgv.Rows.Count)
                            rowno++;
                        int rowsvisible = dgv.DisplayedRowCount(false);
                        int rownobot = Math.Min(rowsvisible + rowno - 1, dgv.Rows.Count - 1);
                        while (!dgv.Rows[rownobot].Visible && rownobot > 1)
                            rownobot--;
                        dgv.CurrentCell = dgv.Rows[rownobot].Cells[i];      // blam top and bottom to try and get the best view
                        dgv.CurrentCell = dgv.Rows[rowno].Cells[i];
                        dgv.Rows[rowno].Selected = true;
                        break;
                    }
                }
            }
        }
        else
        {
            try
            {
                dgv.FirstDisplayedScrollingRowIndex = rowno;    // SAFE VERSION
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("DGV exception FDR " + e);       // v.rare.
            }
        }
    }

    public static bool IsAllSelectionsOnSameRow(this DataGridView dgv)
    {
        if (dgv.SelectedCells.Count > 0)
        {
            int rowno = dgv.SelectedCells[0].RowIndex;
            for (int i = 1; i < dgv.SelectedCells.Count; i++)
            {
                if (dgv.SelectedCells[i].RowIndex != rowno)
                    return false;
            }

            return true;
        }
        else
            return false;
    }

    public static int ColumnsHidden(this DataGridView dgv)
    {
        int count = 0;
        foreach (DataGridViewColumn c in dgv.Columns)
            count += c.Visible ? 0 : 1;
        return count;
    }

    // tries to set row and preferredcolumn, else tries another one on same row
    public static bool SetCurrentSelOnRow(this DataGridView dgv, int row, int preferredcolumn)
    {
        if (row >= 0 && row < dgv.Rows.Count && dgv.Rows[row].Visible)
        {
            if (preferredcolumn < dgv.Columns.Count && dgv.Columns[preferredcolumn].Visible)
            {
                dgv.CurrentCell = dgv.Rows[row].Cells[preferredcolumn];
                return true;
            }
            else
            {
                for (int i = preferredcolumn + 1; i < dgv.Columns.Count; i++)
                {
                    if (dgv.Columns[i].Visible)
                    {
                        dgv.CurrentCell = dgv.Rows[row].Cells[i];
                        return true;
                    }
                }
                for (int i = preferredcolumn - 1; i >= 0; i--)
                {
                    if (dgv.Columns[i].Visible)
                    {
                        dgv.CurrentCell = dgv.Rows[row].Cells[i];
                        return true;
                    }
                }
            }
        }

        return false;
    }

    // Sets selected to all cells on row and sets Current to first visible
    public static bool SetCurrentAndSelectAllCellsOnRow(this DataGridView dgv, int row)
    {
        if (row >= 0 && row < dgv.Rows.Count && dgv.Rows[row].Visible)
        {
            DataGridViewRow rw = dgv.Rows[row];
            bool setcurrent = false;
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                if (dgv.Columns[i].Visible)
                {
                    if (!setcurrent)
                    {
                        dgv.CurrentCell = rw.Cells[i];
                        setcurrent = true;
                    }

                    rw.Cells[i].Selected = true;
                }
            }

            //System.Diagnostics.Debug.WriteLine($"Current cell {dgv.CurrentCell.RowIndex}, {dgv.CurrentCell.ColumnIndex} disp {dgv.CurrentCell.Displayed}");
            return true;
        }

        return false;
    }

    // if row selected, return (rowindex,-1), else if cells selected, return (cell row,cell col), else null
    static public Tuple<int, int> GetSelectedRowOrCellPosition(this DataGridView dgv)
    {
        if (dgv.SelectedRows.Count > 0)
            return new Tuple<int, int>(dgv.SelectedRows[0].Index, -1);
        else if (dgv.SelectedCells.Count > 0)
            return new Tuple<int, int>(dgv.SelectedCells[0].RowIndex, dgv.SelectedCells[0].ColumnIndex);
        else
            return null;
    }

    // index is some key and a DGV row. Move to it, try and display it, return true if we moved.
    // pos.Item1 is -1 if no idea
    // pos.Item2 is changed to -2 if moved.
    // force means we must move somewhere.

    public static bool SelectAndMove(this DataGridView dgv, Dictionary<long, DataGridViewRow> index, ref Tuple<long, int> pos, bool force)
    {
        if (pos.Item1 == -2)          // if done, ignore
        {
        }
        else if (pos.Item1 < 0)        // if not set..
        {
            if (dgv.Rows.GetRowCount(DataGridViewElementStates.Visible) > 0)
            {
                int rowno = dgv.Rows.GetFirstRow(DataGridViewElementStates.Visible);

                dgv.SetCurrentAndSelectAllCellsOnRow(rowno);
                dgv.DisplayRow(rowno, true);
                pos = new Tuple<long, int>(-2, 0);      // done
                return true;        // moved
            }

            if (force) // last chance
            {
                pos = new Tuple<long, int>(-2, 0);      // done
                return true;
            }
        }
        else
        {
            int rowno = FindGridPosByID(index, pos.Item1, true);     // find row.. must be visible..  -1 if not found/not visible

            //System.Diagnostics.Debug.WriteLine("Tried to find " + pos.Item1 + " " + rowno);

            if (rowno >= 0)     // found..
            {
                dgv.SafeFirstDisplayedScrollingRowIndex(rowno);

                dgv.ClearSelection();

                if (pos.Item2 < 0)
                    dgv.SetCurrentAndSelectAllCellsOnRow(rowno);
                else
                    dgv.SetCurrentSelOnRow(rowno, pos.Item2);

                dgv.DisplayRow(rowno, true);
                pos = new Tuple<long, int>(-2, 0);    // cancel next find
                return true;
            }
            else if (force)       // must select
            {
                if (dgv.Rows.GetRowCount(DataGridViewElementStates.Visible) > 0)
                {
                    rowno = dgv.Rows.GetFirstRow(DataGridViewElementStates.Visible);

                    dgv.SetCurrentAndSelectAllCellsOnRow(rowno);
                    dgv.DisplayRow(rowno, true); 
                }

                pos = new Tuple<long, int>(-2, 0);      // done
                return true;
            }
        }

        return false;
    }

    public static int FindGridPosByID(Dictionary<long, DataGridViewRow> index, long id, bool checkvisible)
    {
        if (index.ContainsKey(id) && (!checkvisible || index[id].Visible))
            return index[id].Index;
        else
            return -1;
    }

    public static void CreateTextColumns(this DataGridView grid, params Object[] paras)
    {
        for (int i = 0; i < paras.Length - 1; i += 3)
        {
            DataGridViewTextBoxColumn cl = new DataGridViewTextBoxColumn()
            { FillWeight = (int)paras[i + 1], Name = (string)paras[i], HeaderText = (string)paras[i], MinimumWidth = (int)paras[i + 2] };
            grid.Columns.Add(cl);
        }
    }

    // returns an array of selected rows, in order given. Taken from row collection, or from selected cell area
    public static int[] SelectedRows(this DataGridView grid, bool ascending, bool usecellsifnorowselection)
    {
        var rows = grid.SelectedRows.OfType<DataGridViewRow>().       // pick out rows
                            Where(c => c.Index != grid.NewRowIndex).    // not this pesky one
                            Select(c => c.Index).Distinct();

        if ( rows.Count() == 0 && usecellsifnorowselection)
        {
            rows = grid.SelectedCells.OfType<DataGridViewCell>().Select(x=>x.RowIndex).Distinct();
        }

        var selectedrows = ascending ? rows.OrderBy(x => x).ToArray() : rows.OrderByDescending(x => x).ToArray();

        //foreach (var x in selectedrows) System.Diagnostics.Debug.WriteLine($"DGV Row selected {x}");

        return selectedrows;
    }


    // first selectedrows entry, with a default, and with new row nerf
    public static Tuple<int,int> SelectedRowAndCount(this DataGridView grid, bool ascending, bool usecellsifnorowselection, 
                                              int defaultnoselection = 0, bool nonewrow = true)
    {
        var rows = SelectedRows(grid,ascending,usecellsifnorowselection);
        int rowno = rows.Length > 0 ? rows[0] : defaultnoselection;
        int count = Math.Max(1, rows.Length);
        // if no new row, use one before, if possible
        if (nonewrow && rowno > 0 && rowno == grid.NewRowIndex)
            rowno--;
        //System.Diagnostics.Debug.WriteLine($"DGV Selected row or current {rowno} len {count}");
        return new Tuple<int,int>(rowno,count);
    }

    // return range (inclusive) as objects. end = -1 means to end
    public static object[] CellsObjects(this DataGridViewRow rw, int start = 0, int end = -1)
    {
        if (end == -1)
            end = rw.Cells.Count - 1;
        object[] obj = new object[end - start + 1];
        for (int i = start; i <= end; i++)
            obj[i - start] = rw.Cells[i].Value;
        return obj;
    }

    // can be VERY SLOW for large grids
    public static void FilterGridView(this DataGridView grid, Func<DataGridViewRow,bool> condition)      
    {
        grid.SuspendLayout();
        grid.Enabled = false;

        bool[] visible = new bool[grid.RowCount];
        bool visibleChanged = false;

        foreach (DataGridViewRow row in grid.Rows.OfType<DataGridViewRow>())
        {
            bool found = condition(row);
            visible[row.Index] = found;
            visibleChanged |= found != row.Visible;
        }

        if (visibleChanged)
        {
            var selectedrow = grid.SelectedRows.OfType<DataGridViewRow>().Select(r => r.Index).FirstOrDefault();
            DataGridViewRow[] rows = grid.Rows.OfType<DataGridViewRow>().Where(rw=>!rw.IsNewRow).ToArray();

            for (int i = 0; i < rows.Length; i++)
            {
                rows[i].Visible = visible[i];
            }

            grid.Rows.Clear();
            grid.Rows.AddRange(rows.ToArray());
            grid.Rows[selectedrow].Selected = true;
        }

        grid.Enabled = true;
        grid.ResumeLayout();
    }

    public static bool IsTextInRow(this DataGridViewRow row, string searchstr, bool checktags = false)
    {
        if (!searchstr.HasChars())
            return true;

        foreach (DataGridViewCell cell in row.Cells)
        {
            if (cell.Value != null)
            {
                if (cell.Value.ToString().IndexOf(searchstr, 0, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    return true;
            }

            if (checktags)
            {
                List<string> slist = cell.Tag as List<string>;
                if (slist != null)
                {
                    if (slist.ContainsIn(searchstr, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        return true;
                }

                string str = cell.Tag as string;
                if (str != null)
                {
                    if (str.IndexOf(searchstr, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        return true;
                }
            }
        }

        return false;
    }

    static public StringFormat StringFormatFromDataGridViewContentAlignment(this DataGridViewContentAlignment c)
    {
        StringFormat f = new StringFormat();
        if (c == DataGridViewContentAlignment.BottomCenter || c == DataGridViewContentAlignment.MiddleCenter || c == DataGridViewContentAlignment.TopCenter)
            f.Alignment = StringAlignment.Center;
        else if (c == DataGridViewContentAlignment.BottomLeft || c == DataGridViewContentAlignment.MiddleLeft || c == DataGridViewContentAlignment.TopLeft)
            f.Alignment = StringAlignment.Near;
        else
            f.Alignment = StringAlignment.Far;

        if (c == DataGridViewContentAlignment.BottomCenter || c == DataGridViewContentAlignment.BottomLeft || c == DataGridViewContentAlignment.BottomRight)
            f.LineAlignment = StringAlignment.Far;
        else if (c == DataGridViewContentAlignment.MiddleLeft || c == DataGridViewContentAlignment.MiddleCenter || c == DataGridViewContentAlignment.MiddleRight)
            f.LineAlignment = StringAlignment.Center;
        else
            f.LineAlignment = StringAlignment.Near;

        return f;
    }

}
