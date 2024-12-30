/*
 * Copyright © 2024 - 2024 EDDiscovery development team
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

using QuickJSON;
using System;
using System.Windows.Forms;

public static partial class DataGridViewControlHelpersStaticFunc
{
    // write out to the savers info on column settings, visibility, fill weight, header column width
    public static JToken GetColumnSettings(this DataGridView dgv)
    {
        JObject jo = new JObject();
        JArray fillweight = new JArray();
        JArray di = new JArray();
        for (int i = 0; i < dgv.Columns.Count; i++)
        {
            double fillw = dgv.Columns[i].Visible ? dgv.Columns[i].FillWeight : -dgv.Columns[i].FillWeight;
            fillweight.Add(fillw);
            di.Add(dgv.Columns[i].DisplayIndex);
        }

        jo["FillWeight"] = fillweight;
        jo["DisplayIndex"] = di;
        jo["HW"] = dgv.RowHeadersWidth;
        jo["HWVisible"] = dgv.RowHeadersVisible;
        return jo;
    }


    // Set column fill weight and visibility from getdouble, and set header width from getint.
    // Return MinValue in getdouble to say you don't have a value
    public static bool LoadColumnSettings(this DataGridView dgv, JToken jt, bool rowheadersel)
    {
        JObject jo = jt.Object();
        if (jo == null)
            return false;

        int hw = jo["HW"].Int();

        if (hw >= 0)        // need this for a good setting
        {
            dgv.SuspendLayout();

            dgv.RowHeadersWidth = hw;

            if (rowheadersel)       // if we allowing row header visiblity to be set..
            {
                dgv.RowHeadersVisible = jo["HWVisible"].Bool(true);
            }

            int[] displayindexes = new int[dgv.ColumnCount];
            int dicount = 0;

            var fillwja = jo["FillWeight"].Array();
            var dija = jo["DisplayIndex"].Array();

            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                double fillw = i < fillwja.Count ? fillwja[i].Double(100) : double.MinValue;        // ensure i is in range - bug found
                if (fillw > double.MinValue)
                {
                    dgv.Columns[i].Visible = fillw > 0;
                    dgv.Columns[i].FillWeight = (float)Math.Abs(fillw);
                }

                int di = i < dija.Count ? dija[i].Int(i) : int.MinValue;        // ensure i is in range
                if (di >= 0 && di < dgv.ColumnCount)
                {
                    displayindexes[di] = i;
                    dicount++;
                }
            }

            if (dicount == dgv.ColumnCount)     // if we have set the display indexes of all columns, we can update it. otherwise ignore
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
