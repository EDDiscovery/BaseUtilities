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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BaseUtils;

public static class WinFormTranslatorExtensions
{
    public static void Translate(this Translator translator, Control ctrl, Control[] ignorelist = null)
    {
        Translate(translator, ctrl, ctrl.GetType().Name, ignorelist);
    }

    // Call direct only for debugging, normally use the one above.
    public static void Translate(this Translator translator, Control ctrl, string subname, Control[] ignorelist, bool debugit = false)
    {
        if (translator.Translating)
        {
            if (debugit)
                System.Diagnostics.Debug.WriteLine("T: " + subname + " .. " + ctrl.Name + " (" + ctrl.GetType().Name + ")");

            if ((ignorelist == null || !ignorelist.Contains(ctrl)) && !translator.IsExcludedControl(ctrl.GetType().Name))
            {
                if (ctrl.Text.HasChars())
                {
                    string id = (ctrl is GroupBox || ctrl is TabPage) ? (subname + "." + ctrl.Name) : subname;
                    if (debugit)
                        System.Diagnostics.Debug.WriteLine($" -> Check {id} {ctrl.Text}");
                    ctrl.Text = translator.Translate(ctrl.Text, id);
                    if (debugit)
                        System.Diagnostics.Debug.WriteLine($" -> Ctrl now is {ctrl.Text}");
                }

                if (ctrl is DataGridView)
                {
                    DataGridView v = ctrl as DataGridView;
                    foreach (DataGridViewColumn c in v.Columns)
                    {
                        if (c.HeaderText.HasChars())
                            c.HeaderText = translator.Translate(c.HeaderText, subname.AppendPrePad(c.Name, "."));
                    }
                }

                if (ctrl is TabPage)
                    subname = subname.AppendPrePad(ctrl.Name, ".");

                foreach (Control c in ctrl.Controls)
                {
                    string name = subname;
                    if (NameControl(translator, c))
                        name = name.AppendPrePad(c.Name, ".");

                    Translate(translator, c, name, ignorelist, debugit);
                }
            }
            else
            {
                //logger?.WriteLine("Rejected " + subname);
            }
        }
    }

    public static void Translate(this Translator translator, ToolStrip ctrl, Control parent)
    {
        if (translator.Translating)
        {
            string subname = parent.GetType().Name;

            foreach (ToolStripItem msi in ctrl.Items)
            {
                Translate(translator, msi, subname);
            }
        }
    }

    private static void Translate(Translator translator, ToolStripItem msi, string subname)
    {
        string itemname = msi.Name;

        if (msi.Text.HasChars())
            msi.Text = translator.Translate(msi.Text, subname.AppendPrePad(itemname, "."));

        var ddi = msi as ToolStripDropDownItem;
        if (ddi != null)
        {
            foreach (ToolStripItem dd in ddi.DropDownItems)
                Translate(translator, dd, subname.AppendPrePad(itemname, "."));
        }
    }

    public static void Translate(this Translator translator, ToolTip tt, Control parent)
    {
        Translate(translator, parent, tt, parent.GetType().Name);
    }

    public static bool NameControl(this Translator translator, Control c)
    {
        return c.GetType().Name == "PanelNoTheme" || !(c is Panel || c is DataGridView || c is GroupBox || c is SplitContainer);
    }

    private static void Translate(Translator translator, Control ctrl, ToolTip tt, string subname)
    {
        if (translator.Translating)
        {
            string s = tt.GetToolTip(ctrl);
            if (s.HasChars())
                tt.SetToolTip(ctrl, translator.Translate(s, subname.AppendPrePad("ToolTip", ".")));

            foreach (Control c in ctrl.Controls)
            {
                string name = subname;
                if (NameControl(translator, c))      // containers don't send thru 
                    name = name.AppendPrePad(c.Name, ".");

                Translate(translator, c, tt, name);
            }
        }
    }
}
