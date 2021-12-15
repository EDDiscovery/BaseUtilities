
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
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

public static class DataGridViewControlHelpersStaticFunc
{
    // sort using number. Removetext will remove text suggested
    // usetag means use cell tag as subsititute text

    static public void SortDataGridViewColumnNumeric(this DataGridViewSortCompareEventArgs e, string removetext = null, bool usecelltag = false)
    {
        if (usecelltag)
        {
            var tagl = e.Column.DataGridView.Rows[e.RowIndex1].Cells[e.Column.Index].Tag as string;
            var tagr = e.Column.DataGridView.Rows[e.RowIndex2].Cells[e.Column.Index].Tag as string;

            var left = tagl != null ? tagl : e.CellValue1?.ToString();      // tags preferred if present
            var right = tagr != null ? tagr : e.CellValue2?.ToString();

            e.SortResult = left.CompareNumeric(right, removetext);
        }
        else
        {
            string left = e.CellValue1?.ToString();
            string right = e.CellValue2?.ToString();
            e.SortResult = left.CompareNumeric(right, removetext);
        }

        e.Handled = true;
    }

    // sort using date. userowtagtodistinquish = true use .row as long tag
    // usetag means use cell tag as subsititute text

    static public void SortDataGridViewColumnDate(this DataGridViewSortCompareEventArgs e, bool userowtagtodistinguish = false, bool usecelltag = false)
    {
        if (usecelltag)
        {
            var tagl = e.Column.DataGridView.Rows[e.RowIndex1].Cells[e.Column.Index].Tag as string;
            var tagr = e.Column.DataGridView.Rows[e.RowIndex2].Cells[e.Column.Index].Tag as string;

            var left = tagl != null ? tagl : e.CellValue1?.ToString();      // tags preferred if present
            var right = tagr != null ? tagr : e.CellValue2?.ToString();

            e.SortResult = left.CompareDate(right);
        }
        else
        {
            string left = e.CellValue1?.ToString();
            string right = e.CellValue2?.ToString();
            e.SortResult = left.CompareDate(right);
        }

        if (e.SortResult == 0 && userowtagtodistinguish)
        {
            var lefttag = e.Column.DataGridView.Rows[e.RowIndex1].Tag;
            var righttag = e.Column.DataGridView.Rows[e.RowIndex2].Tag;
            if (lefttag != null && righttag != null)
            {
                long lleft = (long)lefttag;
                long lright = (long)righttag;
                e.SortResult = lleft.CompareTo(lright);
            }
        }

        e.Handled = true;
    }

    // sort using alpha culture/case sensitive
    // usetag means use cell tag as subsititute text

    static public void SortDataGridViewColumnAlpha(this DataGridViewSortCompareEventArgs e, bool usecelltag = false)
    {
        if (usecelltag)
        {
            var tagl = e.Column.DataGridView.Rows[e.RowIndex1].Cells[e.Column.Index].Tag as string;
            var tagr = e.Column.DataGridView.Rows[e.RowIndex2].Cells[e.Column.Index].Tag as string;

            var left = tagl != null ? tagl : e.CellValue1?.ToString();      // tags preferred if present
            var right = tagr != null ? tagr : e.CellValue2?.ToString();

            e.SortResult = left == null ? 1 : right == null ? -1 : left.CompareTo(right);
        }
        else
        {
            string left = e.CellValue1?.ToString();
            string right = e.CellValue2?.ToString();
            e.SortResult = left == null ? 1 : right == null ? -1 : left.CompareTo(right);
        }

        e.Handled = true;
    }

    // sort using alpha, from a List<string> held in the tag of column
    // usetag means use cell tag as subsititute text

    static public void SortDataGridViewColumnTagsAsStringsLists(this DataGridViewSortCompareEventArgs e, int column)
    {
        DataGridView dataGridView = e.Column.DataGridView;
        DataGridViewCell leftcell = dataGridView.Rows[e.RowIndex1].Cells[column];
        DataGridViewCell rightcell = dataGridView.Rows[e.RowIndex2].Cells[column];

        var lleft = leftcell.Tag as List<string>;
        var lright = rightcell.Tag as List<string>;

        if (lleft != null)
        {
            if (lright != null)
            {
                string sleft = string.Join(";", leftcell.Tag as List<string>);
                string sright = string.Join(";", rightcell.Tag as List<string>);
                e.SortResult = sleft.CompareTo(sright);
            }
            else
                e.SortResult = 1;       // left exists, right doesn't, its bigger (null is smaller)
        }
        else
            e.SortResult = lright != null ? -1 : 0;

        e.Handled = true;
    }

    // Find text in coloun

    static public int FindRowWithValue(this DataGridView grid, int coln, string text, StringComparison sc = StringComparison.InvariantCultureIgnoreCase)
    {
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.Cells[coln].Value.ToString().Equals(text, sc))
            {
                return row.Index;
            }
        }

        return -1;
    }
    static public int FindRowWithTag(this DataGridView grid, object tag)
    {
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.Tag == tag)
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

            grid.SafeFirstDisplayedScrollingRowIndex(Math.Max(0, rown - drows / 2));
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

    // tries to set row, preferredcolumn, else tries another one on same row
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

            return true;
        }

        return false;
    }

    // index is some key and a DGV row. Move to it, try and display it, return true if we moved.
    // force means we must move somewhere.

    public static bool MoveToSelection(this DataGridView dgv, Dictionary<long, DataGridViewRow> index, ref Tuple<long, int> pos, bool force)
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

                if (dgv.DefaultCellStyle.WrapMode == DataGridViewTriState.True) // TBD currentcell does not work with variable lines..
                    dgv.SafeFirstDisplayedScrollingRowIndex(rowno);

                //System.Diagnostics.Debug.WriteLine("No Default Select " + rowno);
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
                //System.Diagnostics.Debug.WriteLine("Found Select " + pos.Item1 + " row " + rowno);
                dgv.SafeFirstDisplayedScrollingRowIndex(rowno);

                dgv.SetCurrentSelOnRow(rowno, pos.Item2);

                if (dgv.DefaultCellStyle.WrapMode == DataGridViewTriState.True) // TBD currentcell does not work with variable lines..
                    dgv.SafeFirstDisplayedScrollingRowIndex(rowno);

                pos = new Tuple<long, int>(-2, 0);    // cancel next find
                return true;
            }
            else if (force)       // must select
            {
                if (dgv.Rows.GetRowCount(DataGridViewElementStates.Visible) > 0)
                {
                    rowno = dgv.Rows.GetFirstRow(DataGridViewElementStates.Visible);

                    dgv.SetCurrentAndSelectAllCellsOnRow(rowno);

                    if (dgv.DefaultCellStyle.WrapMode == DataGridViewTriState.True) // TBD currentcell does not work with variable lines..
                        dgv.SafeFirstDisplayedScrollingRowIndex(rowno);

                    //System.Diagnostics.Debug.WriteLine("Force Default Select " + rowno);
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

    public static void SaveColumnSettings(this DataGridView dgv, string root, Action<string, int> saveint, Action<string, double> savedouble)
    {
        for (int i = 0; i < dgv.Columns.Count; i++)
        {
            string k = root + (i + 1).ToString();
            double fillw = dgv.Columns[i].Visible ? dgv.Columns[i].FillWeight : -dgv.Columns[i].FillWeight;
            savedouble(k, fillw);
            k += "_DI";
            saveint(k, dgv.Columns[i].DisplayIndex);
            System.Diagnostics.Debug.WriteLine($"DGV {root} {i} with v {dgv.Columns[i].Visible} w {dgv.Columns[i].FillWeight} di {dgv.Columns[i].DisplayIndex}");
        }

        saveint(root + "HW", dgv.RowHeadersWidth);
    }

    // gets return MinValue if not there.
    public static void LoadColumnSettings(this DataGridView dgv, string root, Func<string, int> getint, Func<string, double> getdouble)
    {
        int hw = getint(root + "HW");
        if (hw >= 0)
        {
            dgv.SuspendLayout();

            dgv.RowHeadersWidth = hw;

            int[] displayindexes = new int[dgv.ColumnCount];
            int dicount = 0;

            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                string k = root + (i + 1).ToString();
                double fillw = getdouble(k);
                if (fillw > double.MinValue)
                {
                    dgv.Columns[i].Visible = fillw > 0;
                    dgv.Columns[i].FillWeight = (float)Math.Abs(fillw);

                    k += "_DI";
                    int di = getint(k);
                    if (di >= 0 && di < dgv.ColumnCount)
                    {
                        displayindexes[di] = i;
                        dicount++;
                    }
                }
            }

            if ( dicount == dgv.ColumnCount)
            {
                // when you change the display index, the others shuffle around. So need to set them it seems in display index increasing order.
                // hence the array, and we set in this order.
                for (int d = 0; d < dgv.ColumnCount; d++)     
                {
                    //System.Diagnostics.Debug.WriteLine($"DGV {root} {displayindexes[d]} => di {d}");
                    dgv.Columns[displayindexes[d]].DisplayIndex = d;
                }
            }

            for (int i = 0; i < dgv.ColumnCount; i++)  System.Diagnostics.Debug.WriteLine($"DGV {root} {i} with v {dgv.Columns[i].Visible} w {dgv.Columns[i].FillWeight} di {dgv.Columns[i].DisplayIndex}");

            dgv.ResumeLayout();
        }
    }
}
