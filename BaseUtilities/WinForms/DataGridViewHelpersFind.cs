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
using System.Windows.Forms;

public static partial class DataGridViewControlHelpersStaticFunc
{
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

    // Somewhere in the row or cell data or the row is a date time- the getdate function returns this
    // try and find nearest row to time within withinms span.
    // -1 if none found.  Rows not presumed ordered
    static public int FindRowWithDateTagWithin(this DataGridView grid, Func<DataGridViewRow, DateTime> getdate, DateTime time, long withinms)
    {
        int bestrow = -1;
        long bestdelta = long.MaxValue;
        foreach (DataGridViewRow row in grid.Rows)
        {
            DateTime rowdt = getdate(row);                      // get date from row
            var delta = Math.Abs(rowdt.Ticks - time.Ticks);     // find delta
            if (delta < withinms && delta < bestdelta)         // if within, and is best
            {
                bestrow = row.Index;                            // this is the row!
                bestdelta = delta;
            }
        }

        return bestrow;
    }

    static public int GetLastRowWithValue(this DataGridView grid)
    {
        for (int i = grid.RowCount - 1; i >= 0; i--)
        {
            var row = grid.Rows[i];
            if (row.Cells.Count > 0)
            {
                foreach (DataGridViewCell c in row.Cells)
                {
                    if (c.Value != null && (!(c.Value is string) || ((string)c.Value).HasChars()))
                        return i;
                }
            }
        }
        return -1;
    }


}
