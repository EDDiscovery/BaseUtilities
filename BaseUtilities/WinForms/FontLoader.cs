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
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;

namespace BaseUtils
{
    public static class FontLoader
    {
        private static PrivateFontCollection PrivateFonts = new PrivateFontCollection();

        public static void AddFontFile(string path)
        {
            PrivateFonts.AddFontFile(path);
        }

        public static FontFamily GetFontFamily(string name)
        {
            return PrivateFonts.Families.FirstOrDefault(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public static Font GetFont(string name, float size)
        {
            var family = GetFontFamily(name);

            if (family != null)
            {
                return new Font(family, size);
            }

            return new Font(name, size);
        }

        public static Font GetFont(string name, float size, FontStyle style)
        {
            var family = GetFontFamily(name);

            if (family != null)
            {
                return new Font(family, size, style);
            }

            return new Font(name, size, style);
        }
    }
}
