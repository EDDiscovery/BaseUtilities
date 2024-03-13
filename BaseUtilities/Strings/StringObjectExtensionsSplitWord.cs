﻿/*
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

public static class ObjectExtensionsStringsSplitWord
{
    // fix word_word to Word Word
    //  s = Regex.Replace(s, @"([A-Za-z]+)([_])([A-Za-z]+)", m => { return m.Groups[1].Value.FixTitleCase() + " " + m.Groups[3].Value.FixTitleCase(); });
    // fix _word to spc Word
    //  s = Regex.Replace(s, @"([_])([A-Za-z]+)", m => { return " " + m.Groups[2].Value.FixTitleCase(); });
    // fix zeros
    //  s = Regex.Replace(s, @"([A-Za-z]+)([0-9])", "$1 $2");       // Any ascii followed by number, split
    //  s = Regex.Replace(s, @"(^0)(0+)", "");     // any 000 at start of line, remove
    //  s = Regex.Replace(s, @"( 0)(0+)", " ");     // any space 000 in middle of line, remove
    //  s = Regex.Replace(s, @"(0)([0-9]+)", "$2");   // any 0Ns left, remove 0

    // at each alpha start, we search using a for loop searchlist first, so it can match stuff with _ and digits/spaces in, 
    // then we do a quick namerep lookup, but this is alpha numeric only

    enum State { whitespace, alpha, nonalpha, digits0, digits };

    // fixes numbers, does replacement of alpha sequences
    static public string SplitCapsWordFull(this string capslower, Dictionary<string, string> namerep = null, Dictionary<string, string> searchlist = null)     
    {
        if (capslower == null || capslower.Length == 0)
            return "";

        string s = SplitCapsWord(capslower);

        System.Text.StringBuilder sb = new System.Text.StringBuilder(256);

        State state = State.whitespace;

        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];

            if (c == '0') // 00..
            {
                if (state == State.digits)      // if in digits, print
                    sb.Append(c);
                else if (state != State.digits0)  // digits0, we just ignore, otherwise we jump into it
                {
                    if (state != State.whitespace)  // if not in space, space it out, but don't print it
                        sb.Append(' ');
                    state = State.digits0;     // digits 0.
                }
            }
            else if ( char.IsDigit(c))  // rest of the numbers
            {
                if (state == State.digits)
                    sb.Append(c);                   // in digits, so print it, as we have removed 0 front stuff.
                else
                {
                    if (state != State.whitespace && state != State.digits0)
                        sb.Append(' ');     // so, we space out if came from not these two states

                    state = State.digits;           // else jump into digits, and append
                    sb.Append(c);
                }
            }
            else
            {
                if (state == State.digits0)        // left in digit 0, therefore a run of 0's, so don't lose it (since they are not inserted)
                    sb.Append('0');

                if (char.IsLetter(c))   // if now alpha
                {
                    if (state == State.alpha)
                        sb.Append(c);
                    else
                    {
                        if (state != State.whitespace)
                            sb.Append(' ');

                        state = State.alpha;
                        bool done = false;

                        if (searchlist != null)
                        {
                            string strleft = s.Substring(i);

                            foreach (string keyname in searchlist.Keys)
                            {
                                if (strleft.StartsWith(keyname, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    //System.Diagnostics.Debug.WriteLine("Check List " + keyname + " Replace " + searchlist[keyname]);
                                    sb.Append(searchlist[keyname]);
                                    i += keyname.Length - 1;                  // skip this, we are in alpha, -1 because of i++ at top
                                    done = true;
                                    break;
                                }
                            }

                        }

                        if (done == false && namerep != null)           // at alpha start, see if we have any global subs of alpha numerics
                        {
                            int j = i + 1;
                            for (; j < s.Length && char.IsLetterOrDigit(s[j]); j++)
                                ;

                            string keyname = s.Substring(i, j - i);
                            //                        string keyname = namekeys.Find(x => s.Substring(i).StartsWith(x));

                            if (namerep.ContainsKey(keyname))
                            {
                                //System.Diagnostics.Debug.WriteLine("Check " + keyname + " Replace " + namerep[keyname]);
                                sb.Append(namerep[keyname]);
                                i += keyname.Length - 1;                  // skip this, we are in alpha, -1 because of i++ at top
                                done = true;
                            }
                        }

                        if (!done)
                            sb.Append(char.ToUpperInvariant(c));
                    }
                }
                else
                {
                    if (c == '_')       // _ is space
                        c = ' ';

                    if (char.IsWhiteSpace(c))       // if whitespace
                    {
                        state = State.whitespace;
                        sb.Append(c);
                    }
                    else
                    {                                       // any other than 0-9 and a-z
                        if (state != State.nonalpha)
                        {
                            if (state != State.whitespace)       // space it
                                sb.Append(' ');

                            state = State.nonalpha;
                        }

                        sb.Append(c);
                    }
                }
            }
        }

        if (state == State.digits0)     // if trailing 0, append.
            sb.Append("0");

        string res = sb.ToString();
        //System.Diagnostics.Debug.WriteLine($"..SplitCapsWordFull `{capslower}` => `{res}`");

        return res;
    }

    // this split captialised words apart only

    // regexp of below : string s = Regex.Replace(capslower, @"([A-Z]+)([A-Z][a-z])", "$1 $2"); //Upper(rep)UpperLower = Upper(rep) UpperLower
    // s = Regex.Replace(s, @"([a-z\d])([A-Z])", "$1 $2");     // lowerdecUpper split
    // s = Regex.Replace(s, @"[-\s]", " "); // -orwhitespace with spc

    public static string SplitCapsWord(this string capslower)
    {
        if (capslower == null || capslower.Length == 0)
            return "";

        List<string> words = new List<string>();

        int start = 0;

        if (capslower[0] == '-' || char.IsWhiteSpace(capslower[0]))  // Remove leading dash or whitespace
            start = 1;

        for (int i = 1; i <= capslower.Length; i++)
        {
            char c0 = capslower[i - 1];                                 // first character                     
            char c1 = i < capslower.Length ? capslower[i] : '\0';       // second 
            bool c1iswhitespace = false;
            char c2 = i < capslower.Length - 1 ? capslower[i + 1] : '\0';   // third

            if (i == capslower.Length || // End of string
                (i < capslower.Length - 1 && char.IsUpper(c0) && char.IsUpper(c1) && char.IsLower(c2)) || // UpperUpperLower
                (((char.IsLower(c0)) || (char.IsDigit(c0))) && char.IsUpper(c1)) || // Lower|digitUpper
                ((c1iswhitespace = c1 == '-' || char.IsWhiteSpace(c1))==true)) // dash or whitespace
            {
                if (i > start)
                    words.Add(capslower.Substring(start, i - start));

                if (i < capslower.Length && c1iswhitespace)
                    start = i + 1;
                else
                    start = i;
            }
        }
        
        string res = String.Join(" ", words);
       // System.Diagnostics.Debug.WriteLine($"SplitCapsWord `{capslower}` => `{res}`");
        return res;
    }


}

