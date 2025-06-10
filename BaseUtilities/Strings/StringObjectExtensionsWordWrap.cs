/*
 * Copyright © 2016 - 2021 EDDiscovery development team
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

public static class ObjectExtensionsStrings2
{
    static public string WordWrap(this string input, int linelen, bool dontcuthttp = false)
    {
        input = input.Replace(Environment.NewLine, " ");
        string res = "";
        int resll = 0;
        int nextspace = input.IndexOf(' ', 0);
        int i = 0;

        while (true)
        {
            if (nextspace == -1)
                nextspace = input.Length;

            int chars = nextspace - i;

            if (chars > linelen)    // if too many, hyphen it.
            {
                if (chars < 4 || !dontcuthttp || !input.Substring(i, 4).Equals("http", StringComparison.InvariantCultureIgnoreCase))
                {
                    int cutpoint = linelen - 1;
                    input = input.Substring(0, i) + input.Substring(i, cutpoint) + "- " + input.Substring(i + cutpoint);
                    nextspace = input.IndexOf(' ', i);
                    chars = nextspace - i;
                }
            }

            if (res.HasChars())
            {
                bool shortenough = (resll + chars + 1 < linelen);           // short enough for a space add
                string separ = shortenough ? " " : Environment.NewLine;
                res += separ;
                resll = shortenough ? resll + separ.Length : 0;                        // reset line length if LF added, else inc by separ
            }

            res += input.Substring(i, chars);       // add on this chunk
            resll += chars;
            i += chars + 1;                         // +1 to skip space
            if (i >= input.Length)                  // this may break it
                break;

            while (i < input.Length && input[i] == ' ')
                i++;

            nextspace = input.IndexOf(' ', i);
        }
        return res;
    }
}

