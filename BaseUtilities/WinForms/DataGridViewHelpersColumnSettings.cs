/*
 * Copyright © 2016 - 2023 EDDiscovery development team
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
    // write out to the savers info on column settings, visibility, fill weight, header column width
    public static void SaveColumnSettings(this DataGridView dgv, string root, Action<string, int> saveint, Action<string, double> savedouble)
    {
        for (int i = 0; i < dgv.Columns.Count; i++)
        {
            string k = root + (i + 1).ToString();
            double fillw = dgv.Columns[i].Visible ? dgv.Columns[i].FillWeight : -dgv.Columns[i].FillWeight;
            savedouble(k, fillw);
            k += "_DI";
            saveint(k, dgv.Columns[i].DisplayIndex);
            //   System.Diagnostics.Debug.WriteLine($"DGV {root} {i} with v {dgv.Columns[i].Visible} w {dgv.Columns[i].FillWeight} di {dgv.Columns[i].DisplayIndex}");
        }

        saveint(root + "HW", dgv.RowHeadersWidth);
        saveint(root + "HWVisible", dgv.RowHeadersVisible?1:0);
    }

    // Set column fill weight and visibility from getdouble, and set header width from getint.
    // Return MinValue in getdouble to say you don't have a value
    public static bool LoadColumnSettings(this DataGridView dgv, string root, bool rowheadersel , Func<string, int> getint, Func<string, double> getdouble)
    {
        int hw = getint(root + "HW");
        if (hw >= 0)
        {
            dgv.SuspendLayout();

            dgv.RowHeadersWidth = hw;

            if (rowheadersel)       // if we allowing row header visiblity to be set..
            {
                int sv = getint(root + "HWVisible");
                dgv.RowHeadersVisible = sv != 0;        // on if not zero, so default (MinInt) will have it on
            }

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

            if (dicount == dgv.ColumnCount)
            {
                // when you change the display index, the others shuffle around. So need to set them it seems in display index increasing order.
                // hence the array, and we set in this order.
                for (int d = 0; d < dgv.ColumnCount; d++)
                {
                    //System.Diagnostics.Debug.WriteLine($"DGV {root} {displayindexes[d]} => di {d}");
                    dgv.Columns[displayindexes[d]].DisplayIndex = d;
                }
            }

            // for (int i = 0; i < dgv.ColumnCount; i++)  System.Diagnostics.Debug.WriteLine($"DGV {root} {i} with v {dgv.Columns[i].Visible} w {dgv.Columns[i].FillWeight} di {dgv.Columns[i].DisplayIndex}");

            dgv.ResumeLayout();

            return true;
        }
        else
            return false;

    }



}
