/*
 * Copyright © 2016 - 2024 EDDiscovery development team
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

public static partial class ObjectExtensionsStrings
{
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
            if (stop)
            {
                if (--number == 0)
                    return s.Substring(startpos, i - startpos - 1);
                else
                    startpos = i;
            }
        }

        return (number == 1) ? s.Substring(startpos) : null;      // if only 1 left, the EOL is the end char, so return
    }

}

