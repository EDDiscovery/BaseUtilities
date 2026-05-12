/*
 * Copyright © 2016 - 2026 EDDiscovery development team
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
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;

namespace BaseUtils
{
    // Class allows own fonts to be loaded, to get and list fonts, and to get font setting strings

    public static class FontHandler
    {
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

        // return the font, either an installed or private font of this font family name
        public static Font GetFont(string familyname, float size)
        {
            var family = GetPrivateFontFamily(familyname);

            if (family != null)
            {
                return new Font(family, size);
            }

            return new Font(familyname, size);
        }

        // return the font, either an installed or private font of this font family name, with a style
        public static Font GetFont(string familyname, float size, FontStyle style = FontStyle.Regular)
        {
            var family = GetPrivateFontFamily(familyname, style);

            if (family != null)
            {
                return new Font(family, size, style);
            }

            return new Font(familyname, size, style);
        }

        // return the font of a family with the particular size and style
        public static Font GetFont(FontFamily reqfamily, float size, FontStyle style = FontStyle.Regular)
        {
            var privatefamily = GetPrivateFontFamily(reqfamily.Name, style);

            if (privatefamily != null)
            {
                return new Font(privatefamily, size, style);
            }

            return new Font(reqfamily, size, style);
        }

        // return all fonts, both installed and in memory fonts
        public static List<FontFamily> GetFontFamilies(bool includeprivatefonts = false)
        {
            var list = new List<FontFamily>();
            if ( includeprivatefonts)
                list.AddRange(PrivateFonts.Families);
            InstalledFontCollection installedFontCollection = new InstalledFontCollection();
            list.AddRange(installedFontCollection.Families);
            return list;
        }

        // given a name, return the font family
        public static FontFamily GetFontFamily(string name)
        {
            var family = GetPrivateFontFamily(name);
            if (family == null)
            {
                InstalledFontCollection installedFontCollection = new InstalledFontCollection();
                family = installedFontCollection.Families.Where(x=>x.Name == name).FirstOrDefault();
            }
            return family;
        }

        // Is this font available with this style.
        public static bool IsFontAvailable(string familyname, FontStyle fs = FontStyle.Regular)
        {
            try
            {
                Font fnt = GetFont(familyname, 12, fs);
                bool res = fnt.Name == familyname && fnt.Style == fs;
                fnt.Dispose();
                return res;
            }
            catch
            {
                return false;
            }
        }

        // from a setting string, get font
        public static Font GetFontFromSetting(string settingstring, Font deffont)
        {
            string[] values = settingstring.Split('`');
            if (values.Length == 3)
            {
                try
                {
                    return BaseUtils.FontHandler.GetFont(values[0], values[1].InvariantParseFloat(12), (FontStyle)Enum.Parse(typeof(FontStyle), values[2]));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Font sel exception {ex}");
                }
            }
            return deffont;
        }

        public static string GetFontSettingString(Font n)       // font may be null
        {
            return n == null ? "" : n.Name + '`' + n.SizeInPoints.ToStringInvariant() + '`' + n.Style.ToString();
        }

        private static FontFamily GetPrivateFontFamily(string name)
        {
            return PrivateFonts.Families.FirstOrDefault(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        private static FontFamily GetPrivateFontFamily(string name, FontStyle style)
        {
            return PrivateFonts.Families.FirstOrDefault(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && f.IsStyleAvailable(style));
        }

        private static FontFamily GetPrivateFont(FontFamily find)
        {
            return PrivateFonts.Families.FirstOrDefault(f => f.Name == find.Name);
        }

        private static PrivateFontCollection PrivateFonts = new PrivateFontCollection();
    }
}
