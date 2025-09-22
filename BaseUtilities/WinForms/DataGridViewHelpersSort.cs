/*
 * Copyright 2016 - 2025 EDDiscovery development team
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
using System.ComponentModel;
using System.Windows.Forms;

public static partial class DataGridViewControlHelpersStaticFunc
{
    // 
    static public void SortDataGridViewColumnNumericThenAlpha(this DataGridViewSortCompareEventArgs e, string removetext = null)
    {
        string left = e.CellValue1?.ToString();
        string right = e.CellValue2?.ToString();
        var datal = ObjectExtensionsStringsCompare.ReadNumeric(left, removetext);
        var datar = ObjectExtensionsStringsCompare.ReadNumeric(right, removetext);

        if (datal.Item2 == false && datar.Item2 == false)
        {
            if (left == null)
                e.SortResult = 1;
            else if (right == null)
                e.SortResult = -1;
            else
                e.SortResult = left.CompareTo(right);
        }
        else if (datal.Item2 == false)
            e.SortResult = 1;
        else if (datar.Item2 == false)
            e.SortResult = -1;
        else
            e.SortResult = datal.Item1.CompareTo(datar.Item1);
        e.Handled = true;
    }

    // sort using number. Removetext will remove text suggested
    // usetag means use cell tag as subsititute text

    static public void SortDataGridViewColumnNumeric(this DataGridViewSortCompareEventArgs e, string removetext = null, bool usecelltag = false, bool striptonumeric = false)
    {
        if (usecelltag)
        {
            var tagl = e.Column.DataGridView.Rows[e.RowIndex1].Cells[e.Column.Index].Tag as string;
            var tagr = e.Column.DataGridView.Rows[e.RowIndex2].Cells[e.Column.Index].Tag as string;

            var left = tagl != null ? tagl : e.CellValue1?.ToString();      // tags preferred if present
            var right = tagr != null ? tagr : e.CellValue2?.ToString();

            e.SortResult = left == null ? 1 : right == null ? -1 : left.CompareNumeric(right, removetext, striptonumeric);
        }
        else
        {
            string left = e.CellValue1?.ToString();
            string right = e.CellValue2?.ToString();
            e.SortResult = left == null ? 1 : right == null ? -1 : left.CompareNumeric(right, removetext, striptonumeric);
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

            e.SortResult = left == null ? 1 : right == null ? -1 : left.CompareDateCurrentCulture(right);
        }
        else
        {
            string left = e.CellValue1?.ToString();
            string right = e.CellValue2?.ToString();
            e.SortResult = left.CompareDateCurrentCulture(right);
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

    // sort using alpha culture/case sensitive, empty cells get pushed down
    // usetag means use cell tag as subsititute text

    static public void SortDataGridViewColumnAlpha(this DataGridViewSortCompareEventArgs e, bool usecelltag = false)
    {
        if (usecelltag)
        {
            var tagl = e.Column.DataGridView.Rows[e.RowIndex1].Cells[e.Column.Index].Tag as string;
            var tagr = e.Column.DataGridView.Rows[e.RowIndex2].Cells[e.Column.Index].Tag as string;

            var left = tagl != null ? tagl : e.CellValue1?.ToString();      // tags preferred if present
            var right = tagr != null ? tagr : e.CellValue2?.ToString();

            e.SortResult = left == null ? 1 : right == null ? -1 : left.Length == 0 ? 1 : right.Length == 0 ? -1 : left.CompareTo(right);
        }
        else
        {
            string left = e.CellValue1?.ToString();
            string right = e.CellValue2?.ToString();
            e.SortResult = left == null ? 1 : right == null ? -1 : left.Length == 0 ? 1 : right.Length == 0 ? -1 : left.CompareTo(right);
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

    static public void SortDataGridViewColumnAlphaInt(this DataGridViewSortCompareEventArgs e, bool usecelltag = false)
    {
        if (usecelltag)
        {
            var tagl = e.Column.DataGridView.Rows[e.RowIndex1].Cells[e.Column.Index].Tag as string;
            var tagr = e.Column.DataGridView.Rows[e.RowIndex2].Cells[e.Column.Index].Tag as string;

            var left = tagl != null ? tagl : e.CellValue1?.ToString();      // tags preferred if present
            var right = tagr != null ? tagr : e.CellValue2?.ToString();

            e.SortResult = left == null ? 1 : right == null ? -1 : left.CompareAlphaInt(right);
        }
        else
        {
            string left = e.CellValue1?.ToString();
            string right = e.CellValue2?.ToString();
            e.SortResult = left == null ? 1 : right == null ? -1 : left.CompareAlphaInt(right);
        }

        e.Handled = true;
    }

    /// <summary>
    /// Get current DGV sort
    /// </summary>
    /// <param name="dgv">DGV</param>
    /// <param name="defcol">default column to sort if no sort is set</param>
    /// <returns>Tuple containing info</returns>

    static public Tuple<DataGridViewColumn, System.Windows.Forms.SortOrder> GetCurrentSort(this DataGridView dgv, int defcol = 0)
    {
        return new Tuple<DataGridViewColumn, System.Windows.Forms.SortOrder>(dgv.SortedColumn != null ? dgv.SortedColumn : dgv.Columns[defcol], dgv.SortOrder);
    }

    /// <summary>
    /// Restore sort given GetCurrentSortInfo
    /// </summary>
    /// <param name="dgv">DGV</param>
    /// <param name="sort">Tuple from GetCurrentSort</param>
    static public void RestoreSort(this DataGridView dgv, Tuple<DataGridViewColumn, System.Windows.Forms.SortOrder> sort)
    {
        dgv.Sort(sort.Item1, (sort.Item2 == System.Windows.Forms.SortOrder.Descending) ? ListSortDirection.Descending : ListSortDirection.Ascending);
        dgv.Columns[sort.Item1.Index].HeaderCell.SortGlyphDirection = sort.Item2;
    }
}
