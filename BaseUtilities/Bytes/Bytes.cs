using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseUtils
{
    public static class Bytes
    {
        // given a character, return hex value (case insensitive) or null
        // best to keep it an int because it gets naturally prompted during computation
        static public int? ToHex(this char c)
        {
            if (char.IsDigit(c))
                return c - '0';
            else if ("ABCDEF".Contains(c))
                return c - 'A' + 10;
            else if ("abcdef".Contains(c))
                return c - 'a' + 10;
            else
                return null;
        }

        // given a string, at position p, return hex double value ("...A1....") or null if not hex
        static public int? ToHex(this string s, int position)
        {
            if (s.Length > position + 1)
            {
                int? top = ToHex(s[position]);
                int? bot = ToHex(s[position + 1]);
                if (top.HasValue && bot.HasValue)
                    return (top << 4) | bot;
            }
            return null;
        }

        // given a string, containing hex only double byte values ("A109A4"), convert, or null if failed
        static public string FromHexString(this string ascii)
        {
            string s = "";
            for (int i = 0; i < ascii.Length; i += 2)
            {
                int? v = ascii.ToHex(i);
                if (v.HasValue)
                    s += Convert.ToChar(v.Value);
                else
                    return null;
            }

            return s;
        }

        // given a string containing hex pairs return byte stream, or null if failed. 
        static public byte[] ToByte(this string ascii)
        {
            if ((ascii.Length & 1) == 1)   // odd count, fail
                return null;
            byte[] ret = new byte[ascii.Length / 2];
            for (int i = 0; i < ret.Length; i++)
            {
                int? v = ascii.ToHex(i * 2);
                if (!v.HasValue)
                    return null;
                ret[i] = (byte)v;
            }

            return ret;
        }


        static public string ToHexString(this byte[] data, int offset = 0, int length = -1, bool lowercase = false)
        {
            if (length == -1)
                length = data.Length;
            string str = BitConverter.ToString(data, offset, length).Replace("-", "");
            if (lowercase)
                str = str.ToLower();
            return str;
        }


    }
}
