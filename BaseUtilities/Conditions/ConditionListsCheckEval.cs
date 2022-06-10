/*
 * Copyright © 2017-2022 EDDiscovery development team
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
    public partial class ConditionLists
    {
        // this one uses an evaluate engine allowing complex expressions on both sides.
        // check all conditions against these values, one by one.  Outercondition of each Condition determines if this is an OR or AND etc operation
        // shortcircuit stop
        // Variable can be in complex format Rings[0].member
        // Supports Rings[Iter1].value[Iter2] - Iter1/2 should be predefined if present to 1, and function iterates it until it fails with a missing symbol
        static public bool? CheckConditionsEvalIterate(List<Condition> fel, Variables values, out string errlist, out ErrorClass errclass, bool iterators, bool debugit = false)            // Check all conditions..
        { 
            while (true)
            {
                var tests = new List<ConditionEntry>();

                var res = CheckConditionsEval(fel, values, out errlist, out errclass, tests:tests, debugit: debugit);

                if (iterators && res == false && tests.Count >= 1)        // if not true, and with tests just in case
                {
                    List<Condition> cll = new List<Condition>() { new Condition(tests.Last()) };
                    var varsinlast = Condition.EvalVariablesUsed(cll);    // what vars are in the last test..

                    if ( varsinlast.Contains("Iter1") )     // iteration..
                    { 
                        int v1 = values.GetInt("Iter1", -1);  // get iters
                        int v2 = values.GetInt("Iter2", -1);

                        if (v2 == -1)         // if no iter2, just iter1
                        {
                            if (errclass == ErrorClass.None)      // if not failed, we can try next. Else we have failed, and exausted the array
                            {
                                values["Iter1"] = (v1 + 1).ToStringInvariant();      // set to next value and retry
                                continue;
                            }
                        }
                        else 
                        {
                            if (errclass == ErrorClass.None)      // if not failed, we can try next iter2
                            {
                                values["Iter2"] = (v2 + 1).ToStringInvariant();      // set to next value and retry
                                continue;
                            }
                            else if (v2 > 1)                // if iter2>1, we stopped on this, so go back to 1 on iter2 and increment iter1
                            {
                                values["Iter1"] = (v1 + 1).ToStringInvariant();      // set to next value and retry
                                values["Iter2"] = "1";        // reset iter2
                                continue;
                            }
                        }
                    }
                }

                return res;
            }
        }


        // Check condition list fel, using the outercondition on each to combine the results.
        // Use the eval engine to assemble arguments. Keep arguments as original types (long,double,strings)
        // values are the set of values to use for variable lookups
        // pass back errlist, errclass
        // optionally pass back test executed in order
        // shortcircuit on outer AND condition
        // obeys disabled
        static public bool? CheckConditionsEval(List<Condition> fel, Variables values, out string errlist, out ErrorClass errclass, 
                                            List<ConditionEntry> tests = null, 
                                            bool debugit = false)
        {
            errlist = null;
            errclass = ErrorClass.None;

            Eval evl = new Eval(true, true, true, true, true);  // check end, allow fp, allow strings, allow members, allow arrays

            evl.ReturnFunctionValue = BaseFunctionsForEval.BaseFunctions;       // allow functions
        
            evl.ReturnSymbolValue += (str) =>       // on symbol lookup
            {
                string qualname = values.Qualify(str);

                if (values.Exists(qualname))        //  if we have a variable
                {
                    string text = values[qualname];
                    if (long.TryParse(text, out long v))    // if its a number, return number
                        return v;
                    else if (double.TryParse(text, out double d))
                        return d;
                    else
                        return text;    // else its a string
                }
                else
                    return new StringParser.ConvertError("Unknown symbol " + qualname);
            };

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

                    tests?.Add(ce);

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
                            errlist += "AlwaysFalse/True does not have on the left side the word 'Condition'" + Environment.NewLine;
                            errclass = ErrorClass.ExprFormatError;
                            innerres = false;
                            break;
                        }
                    }
                    else
                    {   
                        // variable names

                        if (ce.MatchCondition == ConditionEntry.MatchType.IsPresent)         
                        {
                            string name = values.Qualify(ce.ItemName);                 // its a straight variable name, allow any special formatting

                            if (values.Exists(name) && values[name] != null)
                                matched = true;
                        }
                        else if (ce.MatchCondition == ConditionEntry.MatchType.IsNotPresent)
                        {
                            string name = values.Qualify(ce.ItemName);                 // its a straight variable name, allow any special formatting

                            if (!values.Exists(name) || values[name] == null)
                                matched = true;
                        }
                        else
                        {
                            Object leftside;
                            
                            if ( values.Contains(ce.ItemName))
                            {
                                string text = values[ce.ItemName];
                                if (double.TryParse(text, out double d))    // if a double..
                                {
                                    if (long.TryParse(text, out long v))    // if its a number, return number
                                        leftside = v;
                                    else
                                        leftside = d;
                                }
                                else
                                    leftside = text;    // else its a string
                            }
                            else
                                leftside = evl.EvaluateQuickCheck(ce.ItemName);            // evaluate left side

                            if (evl.InError)
                            {
                                errlist += "Left side did not evaluate: " + ce.ItemName + Environment.NewLine;
                                errclass = ErrorClass.LeftSideVarUndefined;
                                innerres = false;
                                break;                       // stop the loop, its a false
                            }

                            var clf = ConditionEntry.Classify(ce.MatchCondition);
                            bool stringordate = clf == ConditionEntry.Classification.String || clf == ConditionEntry.Classification.Date;

                            string lstring = leftside as string;
                            if (stringordate)
                            {
                                if (lstring == null)
                                {
                                    errlist += "Left side is not a string: " + ce.ItemName + Environment.NewLine;
                                    errclass = ErrorClass.LeftSideBadFormat;
                                    innerres = false;
                                    break;                       // stop the loop, its a false
                                }
                            }
                            else if (!(leftside is double || leftside is long))     // must be long or double
                            {
                                errlist += "Left side is not a number: " + ce.ItemName + Environment.NewLine;
                                errclass = ErrorClass.LeftSideBadFormat;
                                innerres = false;
                                break;                       // stop the loop, its a false
                            }

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
                                if (leftside is long)
                                    matched = (ce.MatchCondition == ConditionEntry.MatchType.IsTrue) ? ((long)leftside != 0) : ((long)leftside == 0);
                                else if (leftside is double)
                                    matched = (ce.MatchCondition == ConditionEntry.MatchType.IsTrue) ? ((double)leftside != 0) : ((double)leftside == 0);
                                else
                                {
                                    errlist += "True/False value is not an integer/double on left side" + Environment.NewLine;
                                    errclass = ErrorClass.LeftSideBadFormat;
                                    innerres = false;
                                    break;
                                }
                            }
                            else
                            {
                                // require a right side

                                Object rightside = evl.EvaluateQuickCheck(ce.MatchString);

                                if (evl.InError)
                                {
                                    if (stringordate)            // if in error, and we are doing string date comparisions, allow bare on right
                                    {
                                        rightside = ce.MatchString;
                                    }
                                    else
                                    {
                                        errlist += "Right side did not evaluate: " + ce.MatchString + Environment.NewLine;
                                        errclass = ErrorClass.RightSideVarUndefined;
                                        innerres = false;
                                        break;                       // stop the loop, its a false
                                    }
                                }

                                string rstring = rightside as string;      

                                if (stringordate)
                                {
                                    if ( rstring == null )      // must have a string
                                    {
                                        errlist += "Right side is not a string: " + ce.ItemName + Environment.NewLine;
                                        errclass = ErrorClass.RightSideBadFormat;
                                        innerres = false;
                                        break;                       // stop the loop, its a false
                                    }
                                }
                                else if (!(rightside is double || rightside is long))
                                {
                                    errlist += "Right side is not a number: " + ce.ItemName + Environment.NewLine;
                                    errclass = ErrorClass.LeftSideBadFormat;
                                    innerres = false;
                                    break;                       // stop the loop, its a false
                                }

                                if (debugit)
                                    System.Diagnostics.Debug.WriteLine($"Condition `{leftside}` {ce.MatchCondition} `{rightside}`");

                                if (ce.MatchCondition == ConditionEntry.MatchType.DateBefore || ce.MatchCondition == ConditionEntry.MatchType.DateAfter)
                                {
                                    DateTime tmevalue, tmecontent;
                                    if ( !DateTime.TryParse(lstring, System.Globalization.CultureInfo.CreateSpecificCulture("en-US"), System.Globalization.DateTimeStyles.None, out tmevalue))
                                    {
                                        errlist += "Date time not in correct format on left side: " + leftside +  Environment.NewLine;
                                        errclass = ErrorClass.LeftSideBadFormat;
                                        innerres = false;
                                        break;

                                    }
                                    else if ( !DateTime.TryParse(rstring, System.Globalization.CultureInfo.CreateSpecificCulture("en-US"), System.Globalization.DateTimeStyles.None, out tmecontent))
                                    {
                                        errlist += "Date time not in correct format on right side: " + rightside + Environment.NewLine;
                                        errclass = ErrorClass.RightSideBadFormat;
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
                                else if (ce.MatchCondition == ConditionEntry.MatchType.IsOneOf)
                                {
                                    StringParser p = new StringParser(rstring);
                                    List<string> ret = p.NextQuotedWordList();

                                    if (ret == null)
                                    {
                                        errlist += "IsOneOf value list is not in a optionally quoted comma separated form" + Environment.NewLine;
                                        errclass = ErrorClass.RightSideBadFormat;
                                        innerres = false;
                                        break;                       // stop the loop, its a false
                                    }
                                    else
                                    {
                                        matched = ret.Contains(lstring, StringComparer.InvariantCultureIgnoreCase);
                                    }
                                }
                                else if (ce.MatchCondition == ConditionEntry.MatchType.MatchSemicolon)
                                {
                                    string[] list = rstring.Split(';').Select(x => x.Trim()).ToArray();     // split and trim
                                    matched = list.Contains(lstring.Trim(), StringComparer.InvariantCultureIgnoreCase); // compare, trimmed, case insensitive
                                }
                                else if (ce.MatchCondition == ConditionEntry.MatchType.MatchCommaList)
                                {
                                    StringCombinations sc = new StringCombinations(',');
                                    sc.ParseString(rstring);      // parse, give all combinations
                                    matched = sc.Permutations.Contains(lstring.Trim(), StringComparer.InvariantCultureIgnoreCase); // compare, trimmed, case insensitive
                                }
                                else if (ce.MatchCondition == ConditionEntry.MatchType.MatchSemicolonList)
                                {
                                    StringCombinations sc = new StringCombinations(';');
                                    sc.ParseString(rstring);      // parse, give all combinations
                                    matched = sc.Permutations.Contains(lstring.Trim(), StringComparer.InvariantCultureIgnoreCase); // compare, trimmed, case insensitive
                                }
                                else if (ce.MatchCondition == ConditionEntry.MatchType.AnyOfAny)
                                {
                                    StringParser l = new StringParser(lstring);
                                    List<string> ll = l.NextQuotedWordList();

                                    StringParser r = new StringParser(rstring);
                                    List<string> rl = r.NextQuotedWordList();

                                    if (ll == null || rl == null)
                                    {
                                        errlist += "AnyOfAny value list is not in a optionally quoted comma separated form on both sides" + Environment.NewLine;
                                        errclass = ErrorClass.RightSideBadFormat;
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
                                    }
                                }
                                else
                                {
                                    if ( leftside is double || rightside is double)
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

                    //  System.Diagnostics.Debug.WriteLine(fe.eventname + ":Compare " + f.matchtype + " '" + f.contentmatch + "' with '" + vr.value + "' res " + matched + " IC " + fe.innercondition);

                    if (cond.InnerCondition == ConditionEntry.LogicalCondition.And)       // Short cut, if AND, all must pass, and it did not
                    {
                        if (!matched)
                        {
                            innerres = false;
                            break;
                        }
                    }
                    else if (cond.InnerCondition == ConditionEntry.LogicalCondition.Nand)  // Short cut, if NAND, and not matched
                    {
                        if (!matched)
                        {
                            innerres = true;                        // positive non match - NAND produces a true
                            break;
                        }
                    }
                    else if (cond.InnerCondition == ConditionEntry.LogicalCondition.Or)    // Short cut, if OR, and matched
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
                    if (cond.InnerCondition == ConditionEntry.LogicalCondition.And)        // none did not matched producing a false, so therefore AND is true
                        innerres = true;
                    else if (cond.InnerCondition == ConditionEntry.LogicalCondition.Or)    // none did matched producing a true, so therefore OR must be false
                        innerres = false;
                    else if (cond.InnerCondition == ConditionEntry.LogicalCondition.Nor)   // none did matched producing a false, so therefore NOR must be true
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

                        if ((fel[oc + 1].OuterCondition == ConditionEntry.LogicalCondition.Or && outerres == true) ||
                            (fel[oc + 1].OuterCondition == ConditionEntry.LogicalCondition.And && outerres == false))
                        {
                           // System.Diagnostics.Debug.WriteLine("Short circuit on {0} cur {1}", fel[oc + 1].OuterCondition, outerres);
                            break;
                        }
                    }
                }
                else if (cond.OuterCondition == ConditionEntry.LogicalCondition.Or)
                {
                    outerres |= innerres.Value;

                    if (outerres.Value == true)      // no point continuing, first one true wins
                    {
                        //System.Diagnostics.Debug.WriteLine("Short circuit second on {0} cur {1}", fe.OuterCondition, outerres);
                        break;
                    }
                }
                else if (cond.OuterCondition == ConditionEntry.LogicalCondition.And)
                {
                    outerres &= innerres.Value;

                    if (outerres.Value == false)      // no point continuing, first one false wins
                    {
                        //System.Diagnostics.Debug.WriteLine("Short circuit second on {0} cur {1}", fe.OuterCondition, outerres);
                        break;
                    }
                }
                else if (cond.OuterCondition == ConditionEntry.LogicalCondition.Nor)
                    outerres = !(outerres | innerres.Value);
                else if (cond.OuterCondition == ConditionEntry.LogicalCondition.Nand)
                    outerres = !(outerres & innerres.Value);
                else
                    System.Diagnostics.Debug.Assert(false, "Bad outer condition");
            }

            return outerres;
        }
    }
}
