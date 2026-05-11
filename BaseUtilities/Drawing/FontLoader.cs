/*
 * Copyright © 2016 - 2020 EDDiscovery development team
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
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace BaseUtils
{
    public static class FontLoader
    {
        private static PrivateFontCollection PrivateFonts = new PrivateFontCollection();

        public static void AddFileFont(string path)
        {
            PrivateFonts.AddFontFile(path);
        }
        public static void AddMemoryFont(byte[] fontBytes)
        {
            var fontData = Marshal.AllocCoTaskMem(fontBytes.Length);
            Marshal.Copy(fontBytes, 0, fontData, fontBytes.Length);
            PrivateFonts.AddMemoryFont(fontData, fontBytes.Length);
        }

        public static Font GetFont(string name, float size)
        {
            var family = GetPrivateFont(name);

            if (family != null)
            {
                return new Font(family, size);
            }

            return new Font(name, size);
        }

        public static Font GetFont(string name, float size, FontStyle style = FontStyle.Regular)
        {
            var family = GetPrivateFont(name, style);

            if (family != null)
            {
                return new Font(family, size, style);
            }

            return new Font(name, size, style);
        }

        public static Font GetFont(FontFamily reqfamily, float size, FontStyle style = FontStyle.Regular)
        {
            var privatefamily = GetPrivateFont(reqfamily.Name, style);

            if (privatefamily != null)
            {
                return new Font(privatefamily, size, style);
            }

            return new Font(reqfamily, size, style);
        }


        // does not check size, since it can be rounded down
        public static bool IsFontAvailable(string name, FontStyle fs = FontStyle.Regular)
        {
            try
            {
                Font fnt = GetFont(name, 12, fs);
                bool res = fnt.Name == name && fnt.Style == fs;
                fnt.Dispose();
                return res;
            }
            catch
            {
                return false;
            }
        }

        private static FontFamily GetPrivateFont(string name)
        {
            return PrivateFonts.Families.FirstOrDefault(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        private static FontFamily GetPrivateFont(string name, FontStyle style)
        {
            return PrivateFonts.Families.FirstOrDefault(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && f.IsStyleAvailable(style));
        }

        private static FontFamily GetPrivateFont(FontFamily find)
        {
            return PrivateFonts.Families.FirstOrDefault(f => f.Name == find.Name);
        }

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
