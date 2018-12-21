using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Text;

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
