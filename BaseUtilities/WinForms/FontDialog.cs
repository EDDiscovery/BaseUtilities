/*
 * Copyright 2026 - 2026 EDDiscovery development team
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
using System.Windows.Forms;

namespace BaseUtils
{
    public partial class FontDialog : Form
    {
        public FontFamily SelectedFont { get; set; } = null;
        public float SelectedSize { get; set; } = 8.25F;
        public FontStyle SelectedStyle { get; set; } = FontStyle.Regular;
        public bool AllowStyleSelect { get; set; } = true;
        public string Saying { get; set; } = "The quick brown fox jumps over the lazy dog";

        public FontDialog()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var fontFamilies = FontLoader.GetFontFamilies();

            int vpos = 0;
            foreach (var fontFamily in fontFamilies)
            {
                if (fontFamily.IsStyleAvailable(FontStyle.Regular))
                {
                    Label l = new Label();
                    l.Text = fontFamily.Name + " : " + Saying;
                    l.AutoSize = true;
                    l.Location = new Point(4, vpos);
                    l.Font = FontLoader.GetFont(fontFamily, 12, FontStyle.Regular);
                    l.Tag = fontFamily;
                    panelFonts.Controls.Add(l);
                    if ( l.Height > 32)
                    {
                        panelFonts.Controls.Remove(l);
                      //  System.Diagnostics.Debug.WriteLine($"Rejected font {fontFamily.Name}");
                    }
                    else
                    {
                        fontpos[fontFamily] = vpos;
                        fontlab[fontFamily] = l;
                        vpos += 32;
                        l.Click += L_Click;
                       // System.Diagnostics.Debug.WriteLine($"Added font {fontFamily.Name} reg {fontFamily.IsStyleAvailable(FontStyle.Regular)} italic {fontFamily.IsStyleAvailable(FontStyle.Italic)} bold {fontFamily.IsStyleAvailable(FontStyle.Bold)} under {fontFamily.IsStyleAvailable(FontStyle.Underline)}  ");
                    }
                }
            }

            comboBoxSize.Items.AddRange(new string[] { "6", "8", "9", "10", "11", "12", "13", "14", "16", "18", "20", "22", "24", "26", "28", "30", "32", "34", "36", "40", "48", "56", "64" });
            if ( !comboBoxSize.Items.Contains(Math.Floor(SelectedSize).ToString()))     // ensure its there
                comboBoxSize.Items.Add(SelectedSize.ToString());

            SetFont();

            if (SelectedFont != null)
            {
                int pos = fontpos[SelectedFont];
                panelFonts.VerticalScroll.Value = pos;
            }
        }

        void SetFont()
        {
            comboBoxSize.SelectedIndexChanged -= ComboBoxSize_SelectedIndexChanged;
            comboBoxStyle.SelectedIndexChanged -= ComboBoxStyle_SelectedIndexChanged;

            comboBoxStyle.Visible = labelsyleprompt.Visible = AllowStyleSelect;

            if (SelectedFont != null)
            {
                labelFontName.Text = SelectedFont.Name;
                labelSample.Font = FontLoader.GetFont(SelectedFont, SelectedSize, SelectedStyle);
                labelSample.Text = Saying;

                fontlab[SelectedFont].BackColor = Color.FromArgb(0,192,0);
                string v = SelectedSize.ToString("N0");
                comboBoxSize.SelectedItem = v;

                comboBoxStyle.Items.Clear();
                if (SelectedFont.IsStyleAvailable(FontStyle.Regular))
                    comboBoxStyle.Items.Add("Regular");
                if (SelectedFont.IsStyleAvailable(FontStyle.Bold))
                    comboBoxStyle.Items.Add("Bold");
                if (SelectedFont.IsStyleAvailable(FontStyle.Italic))
                    comboBoxStyle.Items.Add("Italic");
                if (SelectedFont.IsStyleAvailable(FontStyle.Underline))
                    comboBoxStyle.Items.Add("Underline");

                comboBoxStyle.SelectedItem = SelectedStyle.ToString();

                if (comboBoxStyle.SelectedItem == null) // if not available..
                {
                    comboBoxStyle.SelectedIndex = 0;
                    Enum.TryParse<FontStyle>(comboBoxStyle.Text, out FontStyle st);
                    SelectedStyle = st;     // set to first
                }

                comboBoxSize.Enabled = true;
                comboBoxStyle.Enabled = true;

                comboBoxSize.SelectedIndexChanged += ComboBoxSize_SelectedIndexChanged;
                comboBoxStyle.SelectedIndexChanged += ComboBoxStyle_SelectedIndexChanged;
            }
            else
            {
                labelFontName.Text = "Select font..";
                labelFontName.Font = FontLoader.GetFont("MS Sans Serif", 12, FontStyle.Regular);
                labelSample.Text = ""; 
                comboBoxSize.Enabled = false;
                comboBoxStyle.Enabled = false;
            }
        }


        private void L_Click(object sender, EventArgs e)
        {
            if (SelectedFont != null)
                fontlab[SelectedFont].BackColor = Color.Transparent;

            SelectedFont = ((Control)sender).Tag as FontFamily;
            SetFont();
            panelFonts.Refresh();
        }

        private void ComboBoxSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            float v = float.Parse(comboBoxSize.SelectedItem.ToString());
            var font = FontLoader.GetFont(SelectedFont, v, FontStyle.Regular);
            SelectedSize = font.Size;
            SetFont();
        }

        private void ComboBoxStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Enum.TryParse<FontStyle>(comboBoxStyle.Text, out FontStyle st))
            {
                SelectedStyle = st;
                SetFont();
            }
        }

        private Dictionary<FontFamily, int> fontpos = new Dictionary<FontFamily, int>();
        private Dictionary<FontFamily, Label> fontlab = new Dictionary<FontFamily, Label>();

        // if you want the older dialog..
        public static Font FontSelection(System.Windows.Forms.Control parent, Font curfont, int min = 4, int max = 36, bool musthaveregular = false)
        {
            using (var fd = new System.Windows.Forms.FontDialog())
            {
                fd.Font = curfont;
                fd.MinSize = min;
                fd.MaxSize = max;
                System.Windows.Forms.DialogResult result;

                try
                {
                    result = fd.ShowDialog(parent);
                }
                catch (ArgumentException ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                    return null;
                }

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    if (!musthaveregular || fd.Font.Style == FontStyle.Regular)
                    {
                        return fd.Font;
                    }
                    else
                        System.Windows.Forms.MessageBox.Show("Font does not have regular style");
                }

                return null;
            }

        }

    }
}
