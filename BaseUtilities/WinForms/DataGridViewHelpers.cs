
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
using System.Linq;
using System.Windows.Forms;

public static class DataGridViewControlHelpersStaticFunc
{
    static public void SortDataGridViewColumnNumeric(this DataGridViewSortCompareEventArgs e, string removetext= null)
    {
        string s1 = e.CellValue1?.ToString();
        string s2 = e.CellValue2?.ToString();

        if (removetext != null)
        {
            if ( s1 != null )
                s1 = s1.Replace(removetext, "");
            if ( s2 != null )
                s2 = s2.Replace(removetext, "");
        }

        double v1=0, v2=0;

        bool v1hasval = s1 != null && Double.TryParse(s1, out v1);
        bool v2hasval = s2 != null && Double.TryParse(s2, out v2);

        if (!v1hasval)
        {
            e.SortResult = 1;
        }
        else if (!v2hasval)
        {
            e.SortResult = -1;
        }
        else
        {
            e.SortResult = v1.CompareTo(v2);
        }

        e.Handled = true;
    }

    static public void SortDataGridViewColumnDate(this DataGridViewSortCompareEventArgs e, bool userowtagtodistinguish = false)
    {
        string s1 = e.CellValue1?.ToString();
        string s2 = e.CellValue2?.ToString();

        DateTime v1 = DateTime.MinValue, v2 = DateTime.MinValue;

        bool v1hasval = s1!=null && DateTime.TryParse(e.CellValue1?.ToString(), out v1);
        bool v2hasval = s2!=null && DateTime.TryParse(e.CellValue2?.ToString(), out v2);

        if (!v1hasval)
        {
            e.SortResult = 1;
        }
        else if (!v2hasval)
        {
            e.SortResult = -1;
        }
        else
        {
            e.SortResult = v1.CompareTo(v2);
        }

        if ( e.SortResult == 0 && userowtagtodistinguish)
        {
            var left = e.Column.DataGridView.Rows[e.RowIndex1].Tag;
            var right = e.Column.DataGridView.Rows[e.RowIndex2].Tag;
            if (left != null && right != null)
            {
                long lleft = (long)left;
                long lright = (long)right;

                e.SortResult = lleft.CompareTo(lright);
            }
        }

        e.Handled = true;
    }

    static public void SortDataGridViewColumnTagsAsStringsLists(this DataGridViewSortCompareEventArgs e, DataGridView dataGridView)
    {
        DataGridViewCell left = dataGridView.Rows[e.RowIndex1].Cells[4];
        DataGridViewCell right = dataGridView.Rows[e.RowIndex2].Cells[4];

        var lleft = left.Tag as List<string>;
        var lright = right.Tag as List<string>;

        if (lleft != null)
        {
            if (lright != null)
            {
                string sleft = string.Join(";", left.Tag as List<string>);
                string sright = string.Join(";", right.Tag as List<string>);
                e.SortResult = sleft.CompareTo(sright);
            }
            else
                e.SortResult = 1;       // left exists, right doesn't, its bigger (null is smaller)
        }
        else
            e.SortResult = lright != null ? -1 : 0;

        e.Handled = true;
    }

    static public void SortDataGridViewColumnTagsAsStrings(this DataGridViewSortCompareEventArgs e, DataGridView dataGridView)
    {
        DataGridViewCell left = dataGridView.Rows[e.RowIndex1].Cells[4];
        DataGridViewCell right = dataGridView.Rows[e.RowIndex2].Cells[4];

        var sleft = left.Tag as string;
        var sright = right.Tag as string;

        if (sleft != null)
        {
            if (sright != null)
            {
                e.SortResult = sleft.CompareTo(sright);
            }
            else
                e.SortResult = 1;       // left exists, right doesn't, its bigger (null is smaller)
        }
        else
            e.SortResult = sright != null ? -1 : 0;

        e.Handled = true;
    }

    // Find text in coloun

    static public int FindRowWithValue(this DataGridView grid, int coln, string text, StringComparison sc = StringComparison.InvariantCultureIgnoreCase)
    {
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.Cells[coln].Value.ToString().Equals(text,sc))
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

            grid.SafeFirstDisplayedScrollingRowIndex( Math.Max(0, rown - drows / 2) );
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

    public static bool IsNullOrEmpty( this DataGridViewCell cell)
    {
        return cell.Value == null || cell.Value.ToString().Length == 0;
    }

    public static int GetNumberOfVisibleRowsAbove( this DataGridViewRowCollection table, int rowindex )
    {
        int visible = 0;
        for( int i = 0; i < rowindex; i++ )
        {
            if (table[i].Visible)
                visible++;
        }
        return visible;
    }

    public static void SafeFirstDisplayedScrollingRowIndex(this DataGridView dgv, int rowno)
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

    public static bool AllSelectionsOnSameRow(this DataGridView dgv)
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

    public static void SetCurrentCellOrRow(this DataGridView dgv, int row, int preferredcolumn)
    {
        if (row >= 0 && row < dgv.Rows.Count)
        {
            if (preferredcolumn < dgv.Columns.Count && dgv.Columns[preferredcolumn].Visible)
                dgv.CurrentCell = dgv.Rows[row].Cells[preferredcolumn];
            else
                dgv.Rows[row].Selected = true;
        }
    }

    // index is some key and a DGV row. Move to it, try and display it, return true if we moved.
    // force means we must move somewhere.

    public static bool MoveToSelection(this DataGridView dgv, Dictionary<long,DataGridViewRow> index, ref Tuple<long, int> pos, bool force, int preferredcolumn)
    {
        if (pos.Item1 == -2)          // if done, ignore
        {
        }
        else if (pos.Item1 < 0)        // if not set..
        {
            if (dgv.Rows.GetRowCount(DataGridViewElementStates.Visible) > 0)
            {
                int rowno = dgv.Rows.GetFirstRow(DataGridViewElementStates.Visible);

                dgv.SetCurrentCellOrRow(rowno, preferredcolumn);

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

                dgv.SetCurrentCellOrRow(rowno, pos.Item2);

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

                    dgv.SetCurrentCellOrRow(rowno, preferredcolumn);

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

}
