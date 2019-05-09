using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BaseUtils;

public static class WinFormTranslatorExtensions
{
    public static void Translate(this BaseUtils.Translator translator, Control ctrl, Control[] ignorelist = null)
    {
        ((WinFormTranslator)translator).Translate(ctrl, ignorelist);
    }

    public static void Translate(this Translator translator, Control ctrl, string subname, Control[] ignorelist, bool debugit = false)
    {
        ((WinFormTranslator)translator).Translate(ctrl, subname, ignorelist, debugit);
    }

    public static void Translate(this Translator translator, ToolStrip ctrl, Control parent)
    {
        ((WinFormTranslator)translator).Translate(ctrl, parent);
    }

    public static void Translate(this Translator translator, ToolTip tt, Control parent)
    {
        ((WinFormTranslator)translator).Translate(tt, parent);
    }

    public static bool NameControl(this Translator translator, Control c)
    {
        return ((WinFormTranslator)translator).NameControl(c);
    }
}

namespace BaseUtils
{

    public class WinFormTranslator : Translator
    {
        public void Translate(Control ctrl, Control[] ignorelist = null)
        {
            Translate(ctrl, ctrl.GetType().Name, ignorelist);
        }

        // Call direct only for debugging, normally use the one above.
        public void Translate(Control ctrl, string subname, Control[] ignorelist, bool debugit = false)
        {
            if (translations != null)
            {
                if (debugit)
                    System.Diagnostics.Debug.WriteLine("T: " + subname + " .. " + ctrl.Name + " (" + ctrl.GetType().Name + ")");

                if ((ignorelist == null || !ignorelist.Contains(ctrl)) && !ExcludedControls.Contains(ctrl.GetType().Name))
                {
                    if (ctrl.Text.HasChars())
                    {
                        string id = (ctrl is GroupBox || ctrl is TabPage) ? (subname + "." + ctrl.Name) : subname;
                        if (debugit)
                            System.Diagnostics.Debug.WriteLine(" -> Check " + id);
                        ctrl.Text = Translate(ctrl.Text, id);
                    }

                    if (ctrl is DataGridView)
                    {
                        DataGridView v = ctrl as DataGridView;
                        foreach (DataGridViewColumn c in v.Columns)
                        {
                            if (c.HeaderText.HasChars())
                                c.HeaderText = Translate(c.HeaderText, subname.AppendPrePad(c.Name, "."));
                        }
                    }

                    if (ctrl is TabPage)
                        subname = subname.AppendPrePad(ctrl.Name, ".");

                    foreach (Control c in ctrl.Controls)
                    {
                        string name = subname;
                        if (NameControl(c))
                            name = name.AppendPrePad(c.Name, ".");

                        Translate(c, name, ignorelist, debugit);
                    }
                }
                else
                {
                    //   logger?.WriteLine("Rejected " + subname);
                }
            }
        }

        public void Translate(ToolStrip ctrl, Control parent)
        {
            if (translations != null)
            {
                string subname = parent.GetType().Name;

                foreach (ToolStripItem msi in ctrl.Items)
                {
                    Translate(msi, subname);
                }
            }
        }

        private void Translate(ToolStripItem msi, string subname)
        {
            string itemname = msi.Name;

            if (msi.Text.HasChars())
                msi.Text = Translate(msi.Text, subname.AppendPrePad(itemname, "."));

            var ddi = msi as ToolStripDropDownItem;
            if (ddi != null)
            {
                foreach (ToolStripItem dd in ddi.DropDownItems)
                    Translate(dd, subname.AppendPrePad(itemname, "."));
            }
        }

        public void Translate(ToolTip tt, Control parent)
        {
            Translate(parent, tt, parent.GetType().Name);
        }

        public bool NameControl(Control c)
        {
            return c.GetType().Name == "PanelNoTheme" || !(c is Panel || c is DataGridView || c is GroupBox || c is SplitContainer);
        }

        private void Translate(Control ctrl, ToolTip tt, string subname)
        {
            if (translations != null)
            {
                string s = tt.GetToolTip(ctrl);
                if (s.HasChars())
                    tt.SetToolTip(ctrl, Translate(s, subname.AppendPrePad("ToolTip", ".")));

                foreach (Control c in ctrl.Controls)
                {
                    string name = subname;
                    if (NameControl(c))      // containers don't send thru 
                        name = name.AppendPrePad(c.Name, ".");

                    Translate(c, tt, name);
                }
            }
        }
    }
}
