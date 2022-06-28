/*
 * Copyright © 2016 - 2022 EDDiscovery development team
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

public static partial class ObjectExtensionsStrings
{
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
        return obj.Substring(0, ep + 1);
    }


    // find start, find terminate, if found replace with replace plus any intermidate text if keepafter>0 (keeping after this no of char)
    // replace can have {plural|notplural} inside it, and if plural is defined, replaced with the approriate selection

    public static string ReplaceArea(this string text, string start, string terminate, string replace, int keepafter = 0, bool? plural = null)
    {
        int index = text.IndexOf(start);
        if (index >= 0)
        {
            int endindex = text.IndexOf(terminate, index + 1);

            if (endindex > 0)
            {
                string insert = replace + (keepafter > 0 ? text.Mid(index + keepafter, endindex - index - keepafter) : "");

                if (plural != null)
                {
                    int pi = insert.IndexOf("{");
                    if (pi >= 0)
                    {
                        int pie = insert.IndexOf("}", pi + 1);
                        if (pie > 0)
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


    // only keep IsLetterOrDigit
    public static string ReplaceNonAlphaNumeric(this string obj)
    {
        char[] arr = obj.ToCharArray();
        arr = Array.FindAll<char>(arr, (c => char.IsLetterOrDigit(c)));
        return new string(arr);
    }




}

