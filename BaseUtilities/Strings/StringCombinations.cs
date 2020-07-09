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
    // given a list of words, separated by wordsepar, find all combinations.  c is the OR operator.  
    // Quoted strings are treated as single words
    // [text] indicate text is optional.  If there is one in there, there will be an empty string on the vertical line showing its optional.
    // [] indicates the whole OR sequence is optional
    // examples : a [b] 'hello there'|'goodbye'|lucky  a|b|[]

    public class StringCombinations
    {
        public List<List<string>> WordLists;    // calculated by constructor, lists of words, each one in inner part of list optional to each other
        public List<String> Permutations;       // calculated by Permutations
        private char groupchar;
        private string wordchar;
        private char orchar;
        private char openbracketchar;
        private char closebracketchar;

        private string stopword;        // compounds
        private string stopwordinbracket;

        public StringCombinations(char groupchar = ';', string wordchar = " ", char orchar = '|', char openbracket = '[', char closebracket = ']')
        {
            this.groupchar = groupchar;
            this.wordchar = wordchar;
            this.orchar = orchar;
            this.openbracketchar = openbracket;
            this.closebracketchar = closebracket;
            stopword = wordchar + orchar + groupchar;
            stopwordinbracket = "" + closebracket;

            Permutations = new List<string>();
        }

        public bool ParseString(string s)       // process the whole string, generate all the Permutations..
        {
            StringParser sp = new StringParser(s);

            while (!sp.IsEOL)
            {
                ParseGroup(sp);
                CalculatePermutations();

                if (!sp.IsCharMoveOn(groupchar))
                    break;
            }

            return sp.IsEOL;
        }

        public IEnumerable<List<List<string>>> ParseGroup(string s)     // parse whole string, return one after one the word list for each subsection separ by stopchar
        {
            StringParser sp = new StringParser(s);

            while (!sp.IsEOL)
            {
                ParseGroup(sp);

                yield return WordLists;

                if (!sp.IsCharMoveOn(groupchar))
                    break;
            }
        }

        public void ParseGroup(StringParser sp)      // parse until stopchar.  WordLists has all the word combinations
        {
            WordLists = new List<List<string>>();

            int list = 0;
            bool addedempty = false;

            while( !sp.IsEOL && sp.PeekChar() != groupchar)
            {
                bool bracketed = sp.IsCharMoveOn(openbracketchar);

                string p = sp.NextQuotedWord( bracketed ? stopwordinbracket : stopword);  // get the quoted item.  stop on right point

                if (p == null || ( bracketed && !sp.IsCharMoveOn(closebracketchar) ) )// nothing, stop, EOL after bracket.  Or bracketed but no bracket
                    break;

                if (WordLists.Count <= list)        // do we need another list..
                {
                    WordLists.Add(new List<string>());
                    addedempty = false;
                }

                WordLists.Last().Add(p);    // add it, even if empty

                if (bracketed && p.Length > 0 && addedempty == false)
                {
                    WordLists.Last().Add("");   // add an empty entry
                    addedempty = true;      // only add one please
                }

                if (!sp.IsCharMoveOn(orchar))        // if |, stay on same list, else move to next
                    list++;
            }
        }


        public void CalculatePermutations() // using the word list, calculate all perms, ADD them to the result list
        {
            Perform(0, "");
        }

        private void Perform(int depth, string current)
        {
            if (depth == WordLists.Count())
            {
                Permutations.Add(current);
            }
            else
            {
                for (int i = 0; i < WordLists[depth].Count(); ++i)
                {
                    Perform(depth + 1, current + ((current.Length > 0 && WordLists[depth][i].Length > 0) ? wordchar : "") + WordLists[depth][i]);
                }
            }
        }
    }
}
