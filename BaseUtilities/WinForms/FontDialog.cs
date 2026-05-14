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
        public string SelectedFont { get; set; } = "MS Sans Serif";
        public float SelectedSize { get; set; } = 8.25F;
        public FontStyle SelectedStyle { get; set; } = FontStyle.Regular;
        public bool ShowPrivateFonts { get; set; } = false;

        public Color SelectedColor { get; set; } = Color.FromArgb(192,192, 192);
        public Color HoverColor { get; set; } = Color.FromArgb(220, 220, 220);
        public Color NormalColor { get; set; } = Color.FromArgb(240, 240, 240);

        public void Set(Font fnt)
        {
            SelectedFont = fnt.Name;
            SelectedSize = fnt.Size;
            SelectedStyle = fnt.Style;
        }

        public Font GetFont()
        {
            return FontHandler.GetFont(SelectedFont, SelectedSize, SelectedStyle);
        }

        public bool AllowStyleSelect { get; set; } = true;

        public string Saying { get; set; } = "The quick brown fox jumps over the lazy dog";


        // Static font caller, null if Cancel.
        public static Font SelectFont(System.Windows.Forms.Control parent, Font curfont, bool showprivatefonts = false)
        {
            var frm = new FontDialog();
            frm.Set(curfont);
            frm.ShowPrivateFonts = showprivatefonts;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                return FontHandler.GetFont(frm.SelectedFont, frm.SelectedSize, frm.SelectedStyle);
            }
            else
                return null;
        }


        public FontDialog()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var fontFamilies = FontHandler.GetFontFamilies(ShowPrivateFonts);

            if (fontFamilies.Find(x => x.Name == SelectedFont) == null)
            {
                SelectedFont = fontFamilies.Find(x=>x.Name == "Arial")?.Name ?? fontFamilies[0].Name;
            }

            int vpos = 0;
            foreach (var fontFamily in fontFamilies)
            {
                if (fontFamily.IsStyleAvailable(FontStyle.Regular))
                {
                    Label l = new Label();
                    l.Text = fontFamily.Name + " : " + Saying;
                    l.AutoSize = true;
                    l.Location = new Point(4, vpos);
                    l.BackColor = NormalColor;
                    l.Font = FontHandler.GetFont(fontFamily, 12, FontStyle.Regular);

                    panelFonts.Controls.Add(l);
                    if ( l.Height > 32)     // must add to size
                    {
                        panelFonts.Controls.Remove(l);
                      //  System.Diagnostics.Debug.WriteLine($"Rejected font {fontFamily.Name}");
                    }
                    else
                    {
                        fontpos[fontFamily.Name] = vpos;
                        fontlab[fontFamily.Name] = l;
                        vpos += 32;
                        l.MouseEnter += (s1, e1) => { l.BackColor = ((string)l.Tag) == SelectedFont ? SelectedColor : HoverColor; };
                        l.MouseLeave += (s2, e2) => { l.BackColor = ((string)l.Tag) == SelectedFont ? SelectedColor : NormalColor; };
                        l.Tag = fontFamily.Name;
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
                labelFontName.Text = SelectedFont;
                labelSample.Font = FontHandler.GetFont(SelectedFont, SelectedSize, SelectedStyle);
                labelSample.Text = Saying;
                fontlab[SelectedFont].BackColor = SelectedColor;
                System.Diagnostics.Debug.WriteLine($"Applied selected to {fontlab[SelectedFont].Text}");
                string v = SelectedSize.ToString("N0");
                comboBoxSize.SelectedItem = v;

                comboBoxStyle.Items.Clear();

                var fontfamily = FontHandler.GetFontFamily(SelectedFont);

                if (fontfamily.IsStyleAvailable(FontStyle.Regular))
                    comboBoxStyle.Items.Add("Regular");
                if (fontfamily.IsStyleAvailable(FontStyle.Bold))
                    comboBoxStyle.Items.Add("Bold");
                if (fontfamily.IsStyleAvailable(FontStyle.Italic))
                    comboBoxStyle.Items.Add("Italic");
                if (fontfamily.IsStyleAvailable(FontStyle.Underline))
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
                labelFontName.Font = FontHandler.GetFont("MS Sans Serif", 12, FontStyle.Regular);
                labelSample.Text = ""; 
                comboBoxSize.Enabled = false;
                comboBoxStyle.Enabled = false;
            }
        }


        private void L_Click(object sender, EventArgs e)
        {
            if (SelectedFont != null)
            {
                fontlab[SelectedFont].BackColor = NormalColor;
            }

            SelectedFont = ((Control)sender).Tag as string;

            SetFont();
            panelFonts.Refresh();
        }

        private void ComboBoxSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            float v = float.Parse(comboBoxSize.SelectedItem.ToString());
            var font = FontHandler.GetFont(SelectedFont, v, FontStyle.Regular);
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

        private Dictionary<string, int> fontpos = new Dictionary<string, int>();
        private Dictionary<string, Label> fontlab = new Dictionary<string, Label>();

    
    }
}
