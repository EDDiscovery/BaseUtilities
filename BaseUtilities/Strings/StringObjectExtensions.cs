/*
 * Copyright © 2016 - 2019 EDDiscovery development team
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

public static class ObjectExtensionsStrings
{
    static public bool HasChars(this string obj)
    {
        return obj != null && obj.Length > 0;
    }
    static public bool IsEmpty(this string obj)
    {
        return obj == null || obj.Length == 0;
    }

    public static string ToNullSafeString(this object obj)
    {
        return (obj ?? string.Empty).ToString();
    }

    public static string ToNANSafeString(this double obj, string format)
    {
        return (obj != double.NaN) ? obj.ToString(format) : string.Empty;
    }

    public static string ToNANNullSafeString(this double? obj, string format)
    {
        return (obj.HasValue && obj != double.NaN) ? obj.Value.ToString(format) : string.Empty;
    }

    // obj = null, return "".  Length can be > string
    public static string Left(this string obj, int length)      
    {
        if (obj != null)
        {
            if (length < obj.Length)
                return obj.Substring(0, length);
            else
                return obj;
        }
        else
            return string.Empty;
    }

    // obj = null/empty, return "". If text in string, return stuff left of it. If text not in string, return either empty or all of it
    public static string LeftOf(this string obj, string match, StringComparison cmp = StringComparison.CurrentCulture, bool allifnotthere = false)
    {
        if (obj != null && obj.Length > 0)
        {
            int indexof = obj.IndexOf(match, cmp);
            if (indexof == -1)
                return allifnotthere ? obj : "";
            else
                return obj.Substring(0,indexof);
        }
        else
            return string.Empty;
    }

    // obj = null, return "". start/length can be out of limits
    public static string Mid(this string obj, int start, int length = 999999)   
    {
        if (obj != null)
        {
            if (start < obj.Length)        // if in range
            {
                int left = obj.Length - start;      // what is left..
                return obj.Substring(start, Math.Min(left, length));    // min of left, length
            }
        }

        return string.Empty;
    }

    // obj = null/empty, return "". If text in string, return stuff from it onwards. If text not in string, return either empty or all of it
    public static string MidOf(this string obj, string match, StringComparison cmp = StringComparison.CurrentCulture, bool allifnotthere = false)
    {
        if (obj != null && obj.Length > 0)
        {
            int indexof = obj.IndexOf(match, cmp);
            if (indexof == -1)
                return allifnotthere ? obj : "";
            else
                return obj.Substring(indexof);
        }
        else
            return string.Empty;
    }

    //extend for case
    public static bool Contains(this string data, string comparision, StringComparison c = StringComparison.CurrentCulture)        
    {
        return data.IndexOf(comparision, c) >= 0;
    }

    public static string Alt(this string obj, string alt)
    {
        return (obj == null || obj.Length == 0) ? alt : obj;
    }

    // if object.ToString equals unknowntext, return "" else return string, but do a space replace
    public static string ToNullUnknownString(this object obj, string unknowntext = "Unknown", string spacereplacetext="_")
    {
        if (obj == null)
            return string.Empty;
        else
        {
            string str = obj.ToString();
            return str.Equals(unknowntext) ? "" : str.Replace(spacereplacetext, " ");
        }
    }

    // if it starts with this, skip it
    public static string Skip(this string s, string t, StringComparison c = StringComparison.InvariantCulture)
    {
        if (s.StartsWith(t, c))
            s = s.Substring(t.Length);
        return s;
    }

    // if it starts with start, and if extra is there (configurable), replace it with replacestring..
    public static string ReplaceIfStartsWith(this string obj, string start, string replacestring = "", bool musthaveextra = true, StringComparison sc = StringComparison.InvariantCultureIgnoreCase)
    {
        if (start != null && obj.StartsWith(start, sc) && (!musthaveextra || obj.Length > start.Length))
            return replacestring + obj.Substring(start.Length).TrimStart();
        else
            return obj;
    }

    // if it ends with ends, replace it with replacestring..
    public static string ReplaceIfEndsWith(this string obj, string ends, string replacestring = "", StringComparison sc = StringComparison.InvariantCultureIgnoreCase)
    {
        if (ends != null && obj.EndsWith(ends, sc))
            return obj.Substring(0, obj.Length - ends.Length) + replacestring;
        else
            return obj;
    }

    // trim, then if it ends with this, trim it
    public static string TrimReplaceEnd(this string obj, char endreplace)
    {
        obj = obj.Trim();
        int ep = obj.Length - 1;
        while (ep >= 0 && obj[ep] == endreplace)
            ep--;
        return obj.Substring(0, ep+1);
    }

    // skip to find first alpha text ignoring whitespace
    public static string FirstAlphaNumericText(this string obj)     
    {
        if (obj == null)
            return null;
        else
        {
            string ret = "";
            int i = 0;
            while (i < obj.Length && !char.IsLetterOrDigit(obj[i]))
                i++;

            for (; i < obj.Length; i++)
            {
                if (char.IsLetterOrDigit(obj[i]))
                    ret += obj[i];
                else if (!char.IsWhiteSpace(obj[i]) )
                    break;
            }

            return ret;

        }
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

            while (i < obj.Length && ( char.IsLetterOrDigit(obj[i]) || char.IsWhiteSpace(obj[i])))
                i++;

            string ret = obj.Substring(0, s) + quotemark + obj.Substring(s, i - s) + quotemark + obj.Mid(i);
            return ret;
        }
    }

    // only keep IsLetterOrDigit
    public static string ReplaceNonAlphaNumeric(this string obj)
    {
        char[] arr = obj.ToCharArray();
        arr = Array.FindAll<char>(arr, (c => char.IsLetterOrDigit(c)));
        return new string(arr);
    }

    public static void AppendPrePad(this System.Text.StringBuilder sb, string other, string prepad = " ")
    {
        if (other != null && other.Length > 0)
        {
            if (sb.Length > 0)
                sb.Append(prepad);
            sb.Append(other);
        }
    }

    public static bool AppendPrePad(this System.Text.StringBuilder sb, string other, string prefix, string prepad )
    {
        if (other != null && other.Length > 0)
        {
            if (sb.Length > 0)
                sb.Append(prepad);
            if (prefix.Length > 0)
                sb.Append(prefix);
            sb.Append(other);
            return true;
        }
        else
            return false;
    }

    public static string AppendPrePad(this string sb, string other, string prepad = " ")
    {
        if (other != null && other.Length > 0)
        {
            if (sb.Length > 0)
                sb += prepad;
            sb += other;
        }
        return sb;
    }

    // extend for case
    public static string Replace(this string str, string oldValue, string newValue, StringComparison comparison)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder(str.Length * 4);

        int previousIndex = 0;
        int index = str.IndexOf(oldValue, comparison);
        while (index != -1)
        {
            sb.Append(str.Substring(previousIndex, index - previousIndex));
            sb.Append(newValue);
            index += oldValue.Length;

            previousIndex = index;
            index = str.IndexOf(oldValue, index, comparison);
        }
        sb.Append(str.Substring(previousIndex));

        return sb.ToString();
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
        foreach( char c in invalid)
            normal = normal.Replace(c,'_'); // all others _

        return normal;
    }

    public static bool EqualsAlphaNumOnlyNoCase(this string left, string right)
    {
        left = left.Replace("_", "").Replace(" ", "").ToLowerInvariant();        // remove _, spaces and lower
        right = right.Replace("_", "").Replace(" ", "").ToLowerInvariant();
        return left.Equals(right);
    }

    public static string RemoveTrailingCZeros(this string str)
    {
        int index = str.IndexOf('\0');
        if (index >= 0)
            str = str.Substring(0, index);
        return str;
    }

    public static int ApproxMatch(this string str, string other, int min)       // how many runs match between the two strings
    {
        int total = 0;
        for (int i = 0; i < str.Length; i++)
        {
            for (int j = 0; i < str.Length && j < other.Length; j++)
            {
                if (str[i] == other[j])
                {
                    int i2 = i + 1, j2 = j + 1;

                    int count = 1;
                    while (i2 < str.Length && j2 < other.Length && str[i2] == other[j2])
                    {
                        count++;
                        i2++;
                        j2++;
                    }

                    //if ( count>1)  System.Diagnostics.Debug.WriteLine("Match " + str.Substring(i) + " vs " + other.Substring(j) + " " + count);
                    if (count >= min)   // at least this number of chars in a row.
                    {
                        total += count;
                        i += count;
                        //System.Diagnostics.Debug.WriteLine(" left " + str.Substring(i));
                    }
                }
            }
        }

        //System.Diagnostics.Debug.WriteLine("** TOTAL " + str + " vs " + other + " " + total);

        return total;
    }

    public static string Truncate(this string str, int start, int length, string endmarker = "")
    {
        if (str == null)                // nothing, return empty
            return "";

        int len = str.Length - start;
        if (len < 1)                    // if start beyond length
            return "";
        else if (len > length)           // if we need to cut, because len left > length allowed
            return str.Substring(start, length) + endmarker;
        else
            return str.Substring(start);        // len left is less than length, return the whole lot
    }

    static public string WordWrap(this string input, int linelen)
    {
        String[] split = input.Split(new char[] { ' ' });

        string ans = "";
        int l = 0;
        for (int i = 0; i < split.Length; i++)
        {
            ans += split[i];
            l += split[i].Length;
            if (l > linelen)
            {
                ans += Environment.NewLine;
                l = 0;
            }
            else
                ans += " ";
        }

        return ans;
    }

    static public int IndexOf(this string s, string[] array, out int fi)   // in array, find one with first occurance, return which one in i
    {
        int found = -1;
        fi = -1;
        for (int av = 0; av < array.Length; av++)
        {
            int pos = s.IndexOf(array[av]);
            if (pos != -1 && (found == -1 || pos < found))
            {
                found = pos;
                fi = av;
            }
        }
        return found;
    }

    public enum StartWithResult
    {
        None,
        Keyword,
        KeywordCont
    };

    public static StartWithResult StringStartsWith(ref string s, string part, string cstr, StringComparison c = StringComparison.InvariantCultureIgnoreCase)
    {
        if (part != null && s.StartsWith(part, c))
        {
            s = s.Substring(part.Length);
            if (s.StartsWith(cstr, c))
            {
                s = s.Substring(cstr.Length);
                return StartWithResult.KeywordCont;
            }
            else
                return StartWithResult.Keyword;
        }
        else
            return StartWithResult.None;
    }

    public static string FirstWord(ref string s, char[] stopchars)
    {
        int i = 0;
        while (i < s.Length && Array.IndexOf(stopchars, s[i]) == -1)
            i++;

        string ret = s.Substring(0, i);
        s = s.Substring(i).TrimStart();
        return ret;
    }

    public static string Word(this string s, char[] stopchars)
    {
        int i = 0;
        while (i < s.Length && Array.IndexOf(stopchars, s[i]) == -1)
            i++;
        return s.Substring(0, i);
    }

    public static string Word(this string s, char[] stopchars, int number)      // word 1,2,3 etc or NULL if out of words
    {
        int startpos = 0;
        int i = 0;
        while (i < s.Length)
        {
            bool stop = Array.IndexOf(stopchars, s[i]) != -1;
            i++;
            if ( stop )
            {
                if ( --number == 0 )
                    return s.Substring(startpos, i - startpos - 1);
                else
                    startpos = i;
            }
        }

        return (number==1) ? s.Substring(startpos) : null;      // if only 1 left, the EOL is the end char, so return
    }

    public static bool IsPrefix(ref string s, string t, StringComparison c = StringComparison.InvariantCulture)
    {
        if (s.StartsWith(t, c))
        {
            s = s.Substring(t.Length);
            return true;
        }
        return false;
    }

    public static string RegExWildCardToRegular(this string value)
    {
        if (value.Contains("*") || value.Contains("?"))
            return "^" + System.Text.RegularExpressions.Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        else
            return "^" + value + ".*$";
    }

    public static bool WildCardMatch(this string value, string match, bool caseinsensitive = false)
    {
        match = match.RegExWildCardToRegular();
        return System.Text.RegularExpressions.Regex.IsMatch(value, match,caseinsensitive ? System.Text.RegularExpressions.RegexOptions.IgnoreCase : System.Text.RegularExpressions.RegexOptions.None);
    }

    // find start, find terminate, if found replace with replace plus any intermidate text if keepafter>0 (keeping after this no of char)
    // replace can have {plural|notplural} inside it, and if plural is defined, replaced with the approriate selection

    public static string ReplaceArea(this string text , string start, string terminate , string replace , int keepafter = 0 , bool? plural = null)    
    {
        int index = text.IndexOf(start);
        if ( index >= 0 )
        {
            int endindex = text.IndexOf(terminate, index + 1);

            if ( endindex > 0 )
            {
                string insert = replace + (keepafter > 0 ? text.Mid(index + keepafter, endindex - index - keepafter) : "");

                if (plural != null)
                {
                    int pi = insert.IndexOf("{");
                    if ( pi >= 0 )
                    {
                        int pie = insert.IndexOf("}", pi+1);
                        if ( pie > 0 )
                        {
                            string[] options = insert.Mid(pi + 1, pie - pi - 1).Split('|');
                            insert = insert.Left(pi) + ((plural.Value || options.Length == 1) ? options[0] : options[1]) + insert.Substring(pie + 1);
                        }
                    }
                }

                text = text.Left(index) + insert + text.Substring(endindex + terminate.Length);
            }
        }

        return text;
    }


    // find the next instance of one of the chars in set, in str, and return it in res. Return string after it.  Null if not found 
    static public string NextOneOf(this string str, char[] set, out char res)
    {
        res = char.MinValue;
        int i = str.IndexOfAny(set);
        if (i >= 0)
        {
            res = str[i];
            return str.Substring(i + 1);
        }

        return null;
    }

    // find the index of the first char matching expression, like Array.FindIndex.
    static public int IndexOf(this string str, Predicate<char> predicate)
    {
        for (int i = 0; i < str.Length; i++)
        {
            if (predicate(str[i]))
                return i;
        }
        return -1;
    }

    // find the index of the first char not a number character
    static public int IndexOfNonNumberDigit(this string str, System.Globalization.CultureInfo ci)
    {
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            if (char.IsDigit(c) || str.Substring(i).StartsWith(ci.NumberFormat.NumberDecimalSeparator) ||
                        str.Substring(i).StartsWith(ci.NumberFormat.NumberGroupSeparator) ||
                        str.Substring(i).StartsWith(ci.NumberFormat.NegativeSign) ||
                        c == 'e' || c == 'E')
            {
            }
            else
                return i;
        }
        return -1;
    }

    static public string[] Split(this string s, string splitchars, StringComparison cmp = StringComparison.InvariantCultureIgnoreCase)
    {
        if (s == null)
            return null;

        var start = new int[s.Length];
        var len = new int[s.Length];
        var sections = 0;

        int ipos = 0;
        for( ipos = 0; ipos < s.Length;)            // ipos is left at start of last section, or may be at s.Length
        {
            int nextpos = s.IndexOf(splitchars, ipos, cmp);
            if (nextpos >= 0)
            {
                start[sections] = ipos;
                len[sections++] = nextpos - ipos;
                ipos = nextpos + splitchars.Length;
            }
            else
            {
                start[sections] = ipos;             // if not found, add last section
                len[sections++] = s.Length - ipos;
                break;
            }
        }

        if (sections == 0)
            return new string[] { "" };     // mimic "".split('') behaviour
        else
        {
            string[] ret = new string[sections];
            for (int j = 0; j < sections; j++)
                ret[j] = s.Substring(start[j], len[j]);
            return ret;
        }
    }
}

