/*
 * Copyright 2016 - 2024 EDDiscovery development team
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
using System.CodeDom.Compiler;
using System.Collections.Generic;

public static class ObjectExtensionsStringsSplitWord
{

    enum State { whitespace, alpha, nonalpha, digits0, digits };

    // Splits camel case apart, and numbers apart.
    // allows for search list replacement of text at the start of an alpha sequence
    // allows for replacement of alpha numerics at the start of an alpha sequence
    // keeps 's together with alpah

    static public string SplitCapsWordFull(this string capslower, Dictionary<string, string> alphanumericreplace = null, Dictionary<string, string> searchlist = null)     
    {
        if (!capslower.HasChars())
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

                if (char.IsLetter(c))               // if alpha char
                {
                    if (state == State.alpha)       // if in alpha, just append
                        sb.Append(c);
                    else
                    {
                        if (state != State.whitespace)  // not in alpha, if in another state, whitespace it
                            sb.Append(' ');

                        state = State.alpha;        // now in alpha
                        bool done = false;

                        if (searchlist != null)     // check to see if text left matches a replacement string..
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

                        if (done == false && alphanumericreplace != null)           // at alpha start, see if we have any global subs of alpha numerics
                        {
                            int j = i + 1;
                            for (; j < s.Length && char.IsLetterOrDigit(s[j]); j++)
                                ;

                            string keyname = s.Substring(i, j - i);
                            //                        string keyname = namekeys.Find(x => s.Substring(i).StartsWith(x));

                            if (alphanumericreplace.ContainsKey(keyname))
                            {
                                //System.Diagnostics.Debug.WriteLine("Check " + keyname + " Replace " + namerep[keyname]);
                                sb.Append(alphanumericreplace[keyname]);
                                i += keyname.Length - 1;                  // skip this, we are in alpha, -1 because of i++ at top
                                done = true;
                            }
                        }

                        if (!done)
                            sb.Append(char.ToUpperInvariant(c));
                    }
                }
                else
                {                                                   // NOT a letter
                    if (c == '_')                                   // _ is space
                        c = ' ';

                    if (char.IsWhiteSpace(c))       // if whitespace
                    {
                        state = State.whitespace;   // now in whitespace, and append
                        sb.Append(c);
                    }
                    else if ( c == '\'' && state == State.alpha )   // in alpha and quote, text'text stay in alpha
                    {
                        sb.Append(c);
                    }
                    else
                    {                                           // any other than 0-9 and a-z
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

    // this splits captialised words apart only, to a string

    public static string SplitCapsWord(this string capslower)
    {
        var words = SplitCapsWordToList(capslower);
        string res = String.Join(" ", words);
        // System.Diagnostics.Debug.WriteLine($"SplitCapsWord `{capslower}` => `{res}`");
        return res;
    }

    // this splits captialised words apart only, to a word list, 
    public static List<string> SplitCapsWordToList(this string capslower)
    {
        if (!capslower.HasChars())
            return new List<string> { "" };

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
                (((char.IsLower(c0)) || (char.IsDigit(c0))) && char.IsUpper(c1)) || // (Lower|digit)Upper
                ((c1iswhitespace = c1 == '-' || char.IsWhiteSpace(c1)) == true)) // dash or whitespace
            {
                if (i > start)
                    words.Add(capslower.Substring(start, i - start));       // new word

                if (i < capslower.Length && c1iswhitespace) // move start on..
                    start = i + 1;
                else
                    start = i;
            }
        }

        return words;
    }


    public static string SplitCapsWordNumbersConjoined(this string capslower)
    {
        var words = SplitCapsWordToListNumbersConjoined(capslower);
        string res = String.Join(" ", words);
        return res;
    }

    // Fred 3DMake becomes two entries
    public static List<string> SplitCapsWordToListNumbersConjoined(this string capslower)
    {
        if (!capslower.HasChars())
            return new List<string> { "" };

        List<string> words = new List<string>();

        int start = 0;

        for (int i = 1; i <= capslower.Length; i++)
        {
            char c0 = capslower[i - 1];                                 // first character                     
            char c1 = i < capslower.Length ? capslower[i] : '\0';       // second 
            bool c1iswhitespace = false;

            if (i == capslower.Length || // End of string
                (char.IsLetter(c0) && char.IsDigit(c1)) ||        // letter number = split
                (char.IsLower(c0) && char.IsUpper(c1)) ||        // lower upper = split
                (c1iswhitespace = char.IsWhiteSpace(c1))
              )
            {
                if (i > start)
                    words.Add(capslower.Substring(start, i - start));       // new word

                if (i < capslower.Length && c1iswhitespace) // move start on..
                    start = i + 1;
                else
                    start = i;
            }
        }

        return words;
    }


}

