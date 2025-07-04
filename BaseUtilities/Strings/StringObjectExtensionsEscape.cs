﻿/*
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
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class ObjectExtensionsStringsEscape
{
    public static string EscapeControlChars(this string obj)
    {
        string s = obj.Replace(@"\", @"\\");        // order vital \->\\
        s = s.Replace("\r", @"\r");     // CR -> \r
        return s.Replace("\n", @"\n");  // LF -> \n
    }

    public static string EscapeControlCharsFull(this string obj)        // unicode points not escaped out
    {
        string s = obj.Replace(@"\", @"\\");        // \->\\
        s = s.Replace("\r", @"\r");     // CR -> \r
        s = s.Replace("\"", "\\\"");     // " -> \"
        s = s.Replace("\t", @"\t");     // TAB - > \t
        s = s.Replace("\b", @"\b");     // BACKSPACE - > \b
        s = s.Replace("\f", @"\f");     // FORMFEED -> \f
        s = s.Replace("\n", @"\n");     // LF -> \n
        return s;
    }

    public static string ReplaceEscapeControlChars(this string obj)
    {
        string s = obj.Replace(@"\n", "\n");        // \n -> LF
        s = s.Replace(@"\r", "\r");                 // \r -> CR
        return s.Replace(@"\\", "\\");              // \\ -> \
    }

    public static string ReplaceEscapeControlCharsFull(this string s)     // JSON compatible
    {
        if (s.Contains('\\'))
        {
            StringBuilder b = new StringBuilder(s.Length);

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\\' && i < s.Length-1)
                {
                    switch (s[++i])
                    {
                        case '\\':
                            b.Append('\\');
                            break;
                        case '/':
                            b.Append('/');
                            break;
                        case '"':
                            b.Append('"');
                            break;
                        case 'b':
                            b.Append('\b');
                            break;
                        case 'f':
                            b.Append('\f');
                            break;
                        case 'n':
                            b.Append('\n');
                            break;
                        case 'r':
                            b.Append('\r');
                            break;
                        case 't':
                            b.Append('\t');
                            break;
                        case 'u':
                            if ( i < s.Length -4 )
                            {
                                int? v1 = s[++i].ToHex();
                                int? v2 = s[++i].ToHex();
                                int? v3 = s[++i].ToHex();
                                int? v4 = s[++i].ToHex();
                                if (v1 != null && v2 != null && v3 != null && v4 != null)
                                {
                                    char c = (char)((v1 << 12) | (v2 << 8) | (v3 << 4) | (v4 << 0));
                                    b.Append(c);
                                }
                            }
                            break;
                    }
                }
                else
                    b.Append(s[i]);
            }

            return b.ToString();
        }
        else
            return s;
    }

    public static string SafeVariableString(this string normal)
    {
        string ret = "";
        foreach (char c in normal)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
                ret += c;
            else
                ret += "_";
        }
        return ret;
    }

    public static string SafeFileString(this string normal)
    {
        normal = normal.Replace("*", "_star");      // common ones rename
        normal = normal.Replace("/", "_slash");
        normal = normal.Replace("\\", "_slash");
        normal = normal.Replace(":", "_colon");
        normal = normal.Replace("?", "_qmark");

        char[] invalid = System.IO.Path.GetInvalidFileNameChars();
        foreach (char c in invalid)
            normal = normal.Replace(c, '_'); // all others _

        return normal;
    }

    // find first alpha text and quote it.. strange function
    public static string QuoteFirstAlphaDigit(this string obj, char quotemark = '\'')
    {
        if (obj == null)
            return null;
        else
        {
            int i = 0;
            while (i < obj.Length && !char.IsLetter(obj[i]))
                i++;

            int s = i;

            while (i < obj.Length && (char.IsLetterOrDigit(obj[i]) || char.IsWhiteSpace(obj[i])))
                i++;

            string ret = obj.Substring(0, s) + quotemark + obj.Substring(s, i - s) + quotemark + obj.Mid(i);
            return ret;
        }
    }



}

