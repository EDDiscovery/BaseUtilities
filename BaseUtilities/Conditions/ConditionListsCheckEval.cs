/*
 * Copyright © 2017-2023 EDDiscovery development team
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
using System.Collections.Generic;
using System.Linq;

namespace BaseUtils
{
    public partial class ConditionLists
    {
        // this one uses an evaluate engine allowing complex expressions on both sides.
        // check all conditions against these values, one by one.  Outercondition of each Condition determines if this is an OR or AND etc operation
        // shortcircuit stop
        // Variable can be in complex format Rings[0].member
        // Supports Iter1-N 
        static public Tuple<bool?, List<ConditionEntry>, List<string>> CheckConditionsEvalIterate(Eval evl, Variables vars, List<Condition> fel, int iterators, bool debugit = false)            // Check all conditions..
        {
            if ( iterators>0)
            {
                for (int i = 1; i <= iterators; i++)
                    vars["Iter" + i] = "1";
            }

            if ( debugit) System.Diagnostics.Debug.WriteLine($"\n*** Check {ConditionLists.ToString(fel)}");

            while (true)
            {
                var tests = new List<ConditionEntry>();
                var testres = new List<string>();

                if (debugit) System.Diagnostics.Debug.WriteLine($"\nCheck Iter {vars.GetInt("Iter1", -1)} {vars.GetInt("Iter2", -1)} {vars.GetInt("Iter3", -1)} {vars.GetInt("Iter4", -1)}");

                var res = CheckConditionsEval(evl, vars, fel, tests,testres, debugit: debugit);

                if (iterators>0 && res == false && tests.Count >= 1)        // if not true, and with tests just in case
                {
                    List<Condition> cll = new List<Condition>() { new Condition(tests.Last()) };    // get the last test which failed

                    HashSet<string> symbols = new HashSet<string>();

                    Condition.InUse(cll, evl, out HashSet<string> varsinlast, out HashSet<string> _);    // what vars are in the last test..

                    bool cont = false;
                    for (int i = iterators; i >= 1; i--)
                    {
                        string name = "Iter" + i;

                        if (varsinlast.Contains(name))           // if it contains iter, lets try the next iter, and check condition
                        {
                            vars[name] = (vars.GetInt(name, -1) + 1).ToStringInvariant();      // set to next value and retry
                            var testres2 = new List<string>();
                            CheckConditionsEval(evl, vars, cll, null, testres2, debugit);  // check it and see if it errored on any variables
                            if (testres2[0] == null)                            // did not error, so go with this
                            {
                                cont = true;
                                break;
                            }
                        }

                        vars[name] = "1";            // reset iterator
                    }

                    if (cont)
                        continue;
                }

                return new Tuple<bool?,List<ConditionEntry>,List<string>>(res,tests,testres);
            }
        }

        // Check condition list fel, using the outercondition on each to combine the results.
        // Use the eval engine to assemble arguments. Keep arguments as original types (long,double,strings)
        // values are the set of values to use for variable lookups
        // pass back errlist, errclass
        // optionally pass back test executed in order
        // shortcircuit on outer AND condition
        // obeys disabled
        static public bool? CheckConditionsEval(Eval evl, Variables vars, List<Condition> fel,
                                            List<ConditionEntry> testconditions = null, List<string> testerrors = null,
                                            bool debugit = false)
        {
            bool? outerres = null;

            for( int oc = 0; oc < fel.Count; oc++)
            {
                Condition cond = fel[oc];
                if (cond.Disabled)                // disabled means that its ignored
                    continue;

                bool? innerres = null;

                foreach (ConditionEntry ce in cond.Fields)
                {
                    bool matched = false;

                    if (debugit)
                        System.Diagnostics.Debug.WriteLine($"CE `{ce.ItemName}`  {ce.MatchCondition} `{ce.MatchString}`");

                    testconditions?.Add(ce);
                    testerrors?.Add(null);

                    // these require no left or right

                    if (ce.MatchCondition == ConditionEntry.MatchType.AlwaysTrue || ce.MatchCondition == ConditionEntry.MatchType.AlwaysFalse)
                    {
                        if (ce.ItemName.Length == 0 || ce.ItemName.Equals("Condition", StringComparison.InvariantCultureIgnoreCase))     // empty (legacy) or Condition
                        {
                            if (ce.MatchCondition == ConditionEntry.MatchType.AlwaysTrue)
                                matched = true;         // matched, else if false, leave as false.
                        }
                        else
                        {
                            if (testerrors != null) testerrors[testerrors.Count - 1] = "AlwaysFalse/True does not have on the left side the word 'Condition'" ;
                            innerres = false;
                            break;
                        }
                    }
                    else
                    {   
                        // variable names

                        if (ce.MatchCondition == ConditionEntry.MatchType.IsPresent)         
                        {
                            string name = vars.Qualify(ce.ItemName);                 // its a straight variable name, allow any special formatting

                            if (vars.Exists(name) && vars[name] != null)
                                matched = true;
                        }
                        else if (ce.MatchCondition == ConditionEntry.MatchType.IsNotPresent)
                        {
                            string name = vars.Qualify(ce.ItemName);                 // its a straight variable name, allow any special formatting

                            if (!vars.Exists(name) || vars[name] == null)
                                matched = true;
                        }
                        else
                        {
                            var clf = ConditionEntry.Classify(ce.MatchCondition);
                            bool comparisionisstringordate = clf == ConditionEntry.Classification.String || clf == ConditionEntry.Classification.Date;

                            Object leftside;        // double, long, string

                            if (vars.Contains(ce.ItemName))     // if a direct variable name
                            {
                                string text = vars[ce.ItemName];
                                if (double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double d))    // if a double..
                                {
                                    if (long.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out long v))    // if its a number, return number
                                        leftside = v;
                                    else
                                        leftside = d;
                                }
                                else
                                {
                                    leftside = text;    // else its a string
                                }
                            }
                            else
                            {
                                leftside = evl.EvaluateQuickCheck(ce.ItemName);            // evaluate left side
                                bool leftsideisnumber = leftside is long || leftside is double;

                                if (evl.InError)        // in error, must not be here
                                {
                                    if (debugit)
                                        System.Diagnostics.Debug.WriteLine($" .. Left side in error ${((StringParser.ConvertError)leftside).ErrorValue}");

                                    if (testerrors != null) testerrors[testerrors.Count - 1] = "Left side did not evaluate: " + ce.ItemName;

                                    leftside = null;        // indicate condition has failed
                                }
                                else if ( comparisionisstringordate && leftsideisnumber)    // if we ended up with a number, but we are doing a string comparision, its probably just a 3 so treat as string
                                {
                                    leftside = ce.ItemName;
                                }
                            }

                            string lstring = leftside as string;        // used below

                            if (leftside != null)   // if we have a leftside, check it for stringness 
                            {
                                if (comparisionisstringordate)
                                {
                                    if (lstring == null)
                                    {
                                        if (debugit)
                                            System.Diagnostics.Debug.WriteLine(" .. Left side not string");

                                        if (testerrors != null) testerrors[testerrors.Count - 1] = "Left side is not a string: " + ce.ItemName ;

                                        leftside = null;        // indicate condition has failed
                                    }
                                }
                                else if (!(leftside is double || leftside is long))     // must be long or double
                                {
                                    if (debugit)
                                        System.Diagnostics.Debug.WriteLine(" .. Left side not number");
                                    
                                    if (testerrors != null) testerrors[testerrors.Count - 1] = "Left side is not a number: " + ce.ItemName ;

                                    leftside = null;        // indicate condition has failed
                                }
                            }

                            if ( leftside != null )     // we have a good left side
                            {
                                //   System.Diagnostics.Debug.WriteLine($".. left side {leftside}");

                                if (ce.MatchCondition == ConditionEntry.MatchType.IsEmpty)
                                {
                                    matched = lstring.Length == 0;
                                }
                                else if (ce.MatchCondition == ConditionEntry.MatchType.IsNotEmpty)
                                {
                                    matched = lstring.Length > 0;
                                }
                                else if (ce.MatchCondition == ConditionEntry.MatchType.IsTrue || ce.MatchCondition == ConditionEntry.MatchType.IsFalse)
                                {
                                    if (leftside is long)       // we already checked about that leftside is double or long
                                        matched = (ce.MatchCondition == ConditionEntry.MatchType.IsTrue) ? ((long)leftside != 0) : ((long)leftside == 0);
                                    else 
                                        matched = (ce.MatchCondition == ConditionEntry.MatchType.IsTrue) ? ((double)leftside != 0) : ((double)leftside == 0);
                                }
                                else
                                {
                                    // require a right side

                                    Object rightside = evl.EvaluateQuickCheck(ce.MatchString);
                                    bool rightsideisnumber = rightside is long || rightside is double;

                                    if (evl.InError)        // could be due to just a bit of text
                                    {
                                        if (comparisionisstringordate)           // if in error, and we are doing string date comparisions, allow bare on right
                                        {
                                            rightside = ce.MatchString;
                                        }
                                        else
                                        {
                                            if (testerrors != null) testerrors[testerrors.Count - 1] = "Right side did not evaluate: " + ce.MatchString;

                                            rightside = null;   // indicate bad right side
                                        }
                                    }
                                    else if (comparisionisstringordate && right)
                                    {
                                        rightside = ce.MatchString;
                                    }

                                    string rstring = rightside as string;

                                    if (rightside != null)      // if good right side
                                    {
                                        if (comparisionisstringordate)
                                        {
                                            if (rstring == null)      // must have a string
                                            {
                                                if (testerrors != null) testerrors[testerrors.Count - 1] = "Right side is not a string: " + ce.ItemName ;

                                                innerres = false;
                                                rightside = null;
                                            }
                                        }
                                        else if (!(rightside is double || rightside is long))
                                        {
                                            if (testerrors != null) testerrors[testerrors.Count - 1] = "Right side is not a number: " + ce.ItemName ;

                                            innerres = false;
                                            rightside = null;
                                        }
                                    }

                                    if (debugit)
                                        System.Diagnostics.Debug.WriteLine($" .. `{leftside}` {ce.MatchCondition} `{rightside}`");

                                    if (rightside != null)
                                    {
                                        if (ce.MatchCondition == ConditionEntry.MatchType.DateBefore || ce.MatchCondition == ConditionEntry.MatchType.DateAfter)
                                        {
                                            DateTime tmevalue, tmecontent;
                                            if (!DateTime.TryParse(lstring, System.Globalization.CultureInfo.CreateSpecificCulture("en-US"), System.Globalization.DateTimeStyles.None, out tmevalue))
                                            {
                                                if (testerrors != null) testerrors[testerrors.Count - 1] = "Date time not in correct format on left side: " + leftside ;
           
                                                innerres = false;
                                                break;

                                            }
                                            else if (!DateTime.TryParse(rstring, System.Globalization.CultureInfo.CreateSpecificCulture("en-US"), System.Globalization.DateTimeStyles.None, out tmecontent))
                                            {
                                                if (testerrors != null) testerrors[testerrors.Count - 1] = "Date time not in correct format on right side: " + rightside ;
 
                                                innerres = false;
                                                break;
                                            }
                                            else
                                            {
                                                if (ce.MatchCondition == ConditionEntry.MatchType.DateBefore)
                                                    matched = tmevalue.CompareTo(tmecontent) < 0;
                                                else
                                                    matched = tmevalue.CompareTo(tmecontent) >= 0;
                                            }
                                        }
                                        else if (ce.MatchCondition == ConditionEntry.MatchType.Equals)
                                            matched = lstring.Equals(rstring, StringComparison.InvariantCultureIgnoreCase);
                                        else if (ce.MatchCondition == ConditionEntry.MatchType.EqualsCaseSensitive)
                                            matched = lstring.Equals(rstring);

                                        else if (ce.MatchCondition == ConditionEntry.MatchType.NotEqual)
                                            matched = !lstring.Equals(rstring, StringComparison.InvariantCultureIgnoreCase);
                                        else if (ce.MatchCondition == ConditionEntry.MatchType.NotEqualCaseSensitive)
                                            matched = !lstring.Equals(rstring);

                                        else if (ce.MatchCondition == ConditionEntry.MatchType.Contains)
                                            matched = lstring.IndexOf(rstring, StringComparison.InvariantCultureIgnoreCase) >= 0;
                                        else if (ce.MatchCondition == ConditionEntry.MatchType.ContainsCaseSensitive)
                                            matched = lstring.Contains(rstring);

                                        else if (ce.MatchCondition == ConditionEntry.MatchType.DoesNotContain)
                                            matched = lstring.IndexOf(rstring, StringComparison.InvariantCultureIgnoreCase) < 0;
                                        else if (ce.MatchCondition == ConditionEntry.MatchType.DoesNotContainCaseSensitive)
                                            matched = !lstring.Contains(rstring);
                                        else if (ce.MatchCondition == ConditionEntry.MatchType.IsOneOf || ce.MatchCondition == ConditionEntry.MatchType.NotOneOf)
                                        {
                                            StringParser p = new StringParser(rstring);
                                            List<string> ret = p.NextQuotedWordList();

                                            if (ret == null)
                                            {
                                                if (testerrors != null) testerrors[testerrors.Count - 1] = "IsOneOf value list is not in a optionally quoted comma separated form" ;
          
                                                innerres = false;
                                                break;                       // stop the loop, its a false
                                            }
                                            else
                                            {
                                                matched = ret.Contains(lstring, StringComparer.InvariantCultureIgnoreCase);
                                                if (ce.MatchCondition == ConditionEntry.MatchType.NotOneOf)
                                                    matched = !matched;
                                            }
                                        }
                                        else if (ce.MatchCondition == ConditionEntry.MatchType.MatchSemicolon || ce.MatchCondition == ConditionEntry.MatchType.NotMatchSemicolon)
                                        {
                                            string[] list = rstring.Split(';').Select(x => x.Trim()).ToArray();     // split and trim
                                            matched = list.Contains(lstring.Trim(), StringComparer.InvariantCultureIgnoreCase); // compare, trimmed, case insensitive
                                            if (ce.MatchCondition == ConditionEntry.MatchType.NotMatchSemicolon)
                                                matched = !matched;
                                        }
                                        else if (ce.MatchCondition == ConditionEntry.MatchType.MatchCommaList || ce.MatchCondition == ConditionEntry.MatchType.NotMatchCommaList)
                                        {
                                            StringCombinations sc = new StringCombinations(',');
                                            sc.ParseString(rstring);      // parse, give all combinations
                                            matched = sc.Permutations.Contains(lstring.Trim(), StringComparer.InvariantCultureIgnoreCase); // compare, trimmed, case insensitive
                                            if (ce.MatchCondition == ConditionEntry.MatchType.NotMatchCommaList)
                                                matched = !matched;
                                        }
                                        else if (ce.MatchCondition == ConditionEntry.MatchType.MatchSemicolonList || ce.MatchCondition == ConditionEntry.MatchType.NotMatchSemicolonList)
                                        {
                                            StringCombinations sc = new StringCombinations(';');
                                            sc.ParseString(rstring);      // parse, give all combinations
                                            matched = sc.Permutations.Contains(lstring.Trim(), StringComparer.InvariantCultureIgnoreCase); // compare, trimmed, case insensitive
                                            if (ce.MatchCondition == ConditionEntry.MatchType.NotMatchSemicolonList)
                                                matched = !matched;
                                        }
                                        else if (ce.MatchCondition == ConditionEntry.MatchType.AnyOfAny || ce.MatchCondition == ConditionEntry.MatchType.NotAnyOfAny)
                                        {
                                            StringParser l = new StringParser(lstring);
                                            List<string> ll = l.NextQuotedWordList();

                                            StringParser r = new StringParser(rstring);
                                            List<string> rl = r.NextQuotedWordList();

                                            if (ll == null || rl == null)
                                            {
                                                if (testerrors != null) testerrors[testerrors.Count - 1] = "AnyOfAny value list is not in a optionally quoted comma separated form on both sides" ;
                 
                                                innerres = false;
                                                break;                       // stop the loop, its a false
                                            }
                                            else
                                            {
                                                foreach (string s in ll)        // for all left strings
                                                {
                                                    if (rl.Contains(s, StringComparer.InvariantCultureIgnoreCase))  // if right has it..
                                                    {
                                                        matched = true;     // matched and break
                                                        break;
                                                    }
                                                }

                                                if (ce.MatchCondition == ConditionEntry.MatchType.NotAnyOfAny)
                                                    matched = !matched;
                                            }
                                        }
                                        else
                                        {
                                            if (leftside is double || rightside is double)
                                            {
                                                double lnum = leftside is long ? (double)(long)leftside : (double)leftside;
                                                double rnum = rightside is long ? (double)(long)rightside : (double)rightside;

                                                if (ce.MatchCondition == ConditionEntry.MatchType.NumericEquals)
                                                    matched = lnum.ApproxEquals(rnum);

                                                else if (ce.MatchCondition == ConditionEntry.MatchType.NumericNotEquals)
                                                    matched = !lnum.ApproxEquals(rnum);

                                                else if (ce.MatchCondition == ConditionEntry.MatchType.NumericGreater)
                                                    matched = lnum > rnum;

                                                else if (ce.MatchCondition == ConditionEntry.MatchType.NumericGreaterEqual)
                                                    matched = lnum >= rnum;

                                                else if (ce.MatchCondition == ConditionEntry.MatchType.NumericLessThan)
                                                    matched = lnum < rnum;

                                                else if (ce.MatchCondition == ConditionEntry.MatchType.NumericLessThanEqual)
                                                    matched = lnum <= rnum;
                                            }
                                            else
                                            {
                                                long lnum = (long)leftside;
                                                long rnum = (long)rightside;

                                                if (ce.MatchCondition == ConditionEntry.MatchType.NumericEquals)
                                                    matched = lnum == rnum;

                                                else if (ce.MatchCondition == ConditionEntry.MatchType.NumericNotEquals)
                                                    matched = lnum != rnum;

                                                else if (ce.MatchCondition == ConditionEntry.MatchType.NumericGreater)
                                                    matched = lnum > rnum;

                                                else if (ce.MatchCondition == ConditionEntry.MatchType.NumericGreaterEqual)
                                                    matched = lnum >= rnum;

                                                else if (ce.MatchCondition == ConditionEntry.MatchType.NumericLessThan)
                                                    matched = lnum < rnum;

                                                else if (ce.MatchCondition == ConditionEntry.MatchType.NumericLessThanEqual)
                                                    matched = lnum <= rnum;

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (debugit)
                        System.Diagnostics.Debug.WriteLine($" .. match result {matched}");

                    //  System.Diagnostics.Debug.WriteLine(fe.eventname + ":Compare " + f.matchtype + " '" + f.contentmatch + "' with '" + vr.value + "' res " + matched + " IC " + fe.innercondition);

                    if (cond.InnerCondition == Condition.LogicalCondition.And)       // Short cut, if AND, all must pass, and it did not
                    {
                        if (!matched)
                        {
                            innerres = false;
                            break;
                        }
                    }
                    else if (cond.InnerCondition == Condition.LogicalCondition.Nand)  // Short cut, if NAND, and not matched
                    {
                        if (!matched)
                        {
                            innerres = true;                        // positive non match - NAND produces a true
                            break;
                        }
                    }
                    else if (cond.InnerCondition == Condition.LogicalCondition.Or)    // Short cut, if OR, and matched
                    {
                        if (matched)
                        {
                            innerres = true;
                            break;
                        }
                    }
                    else
                    {                                               // short cut, if NOR, and matched, its false
                        if (matched)
                        {
                            innerres = false;
                            break;
                        }
                    }
                } // end of inner condition list look

             //   System.Diagnostics.Debug.WriteLine($"Condition list {innerres} {errlist}");

                if (!innerres.HasValue)                             // if does not have a value
                {
                    if (cond.InnerCondition == Condition.LogicalCondition.And)        // none did not matched producing a false, so therefore AND is true
                        innerres = true;
                    else if (cond.InnerCondition == Condition.LogicalCondition.Or)    // none did matched producing a true, so therefore OR must be false
                        innerres = false;
                    else if (cond.InnerCondition == Condition.LogicalCondition.Nor)   // none did matched producing a false, so therefore NOR must be true
                        innerres = true;
                    else                                            // NAND none did matched producing a true, so therefore NAND must be false
                        innerres = false;
                }

                if (!outerres.HasValue)                             // if first time, its just the value
                {
                    outerres = innerres.Value;

                    if (oc < fel.Count - 1)       // check short circuits on NEXT ONE! if we have a next one..
                    {
                        // if NEXT outer condition is an OR, and we are true
                        // if NEXT outer condition is an AND, and we are false

                        if ((fel[oc + 1].OuterCondition == Condition.LogicalCondition.Or && outerres == true) ||
                            (fel[oc + 1].OuterCondition == Condition.LogicalCondition.And && outerres == false))
                        {
                           // System.Diagnostics.Debug.WriteLine("Short circuit on {0} cur {1}", fel[oc + 1].OuterCondition, outerres);
                            break;
                        }
                    }
                }
                else if (cond.OuterCondition == Condition.LogicalCondition.Or)
                {
                    outerres |= innerres.Value;

                    if (outerres.Value == true)      // no point continuing, first one true wins
                    {
                        //System.Diagnostics.Debug.WriteLine("Short circuit second on {0} cur {1}", fe.OuterCondition, outerres);
                        break;
                    }
                }
                else if (cond.OuterCondition == Condition.LogicalCondition.And)
                {
                    outerres &= innerres.Value;

                    if (outerres.Value == false)      // no point continuing, first one false wins
                    {
                        //System.Diagnostics.Debug.WriteLine("Short circuit second on {0} cur {1}", fe.OuterCondition, outerres);
                        break;
                    }
                }
                else if (cond.OuterCondition == Condition.LogicalCondition.Nor)
                    outerres = !(outerres | innerres.Value);
                else if (cond.OuterCondition == Condition.LogicalCondition.Nand)
                    outerres = !(outerres & innerres.Value);
                else
                    System.Diagnostics.Debug.Assert(false, "Bad outer condition");
            }

            return outerres;
        }
    }
}
