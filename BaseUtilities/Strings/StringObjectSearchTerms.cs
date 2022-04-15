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
using System.Collections.Generic;
using System.Linq;

namespace BaseUtils
{
    public class StringSearchTerms
    {
        public string[] Terms { get; set; }     // 0 is default text, 1 = first name, etc.  If null ,no text found in that position
        public string[] Names { get; set; }     // expanded search terms, 0 = empty
        public bool Enabled { get; set; }       // if userinput gave a search

        // keywrods is x:y:z
        //
        public StringSearchTerms(string userinput, string keywords)
        {
            if (userinput.HasChars())
            {
                Names = (":" + keywords).Split(":");          // 0 is default
                Terms = new string[Names.Length];           // all null to start

                StringParser sp = new StringParser(userinput);

                while (!sp.IsEOL)
                {
                    string nextword = sp.NextWord(": ");                // stop on next : or space
                    int term;

                    if (sp.IsCharMoveOn(':') && (term = Names.IndexOf(nextword, StringComparison.InvariantCultureIgnoreCase)) > 0)       // if :, and it matches nextword
                    {
                        string search = sp.NextQuotedWord(" ");     // next quoted word
                        if (search != null)
                        {
                            Terms[term] = search;
                            Enabled = true;
                        }
                    }
                    else
                    {
                        if (Terms[0] == null)
                            Terms[0] = "";
                        Terms[0] = Terms[0].AppendPrePad(nextword, " ");
                        Enabled = true;
                    }
                }
            }
        }
    }
}
