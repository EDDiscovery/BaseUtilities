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
        // take conditions and Class Variables, find out which variables are needed, expand them, decode it, execute..
        // null if error, else true/false
        static public bool? CheckConditionWithObjectData(List<Condition> fel,
                                        Object cls, // object with data in it
                                        Variables[] othervars,   // any other variables to present to the condition, in addition to the class variables
                                        out string errlist,     // null if okay..
                                        out ErrorClass errclass)
                                       
        {
            errlist = null;
            errclass = ErrorClass.None;

            Variables valuesneeded = new Variables();

            foreach (Condition fe in fel)        // find all values needed
            {
                fe.IndicateValuesNeeded(ref valuesneeded);
            }

            try
            {
                valuesneeded.GetValuesIndicated(cls);       // given the class data, and the list of values needed, add it
                valuesneeded.Add(othervars);
                return CheckConditions(fel, valuesneeded, out errlist, out errclass);    // and check, passing in the values collected against the conditions to test.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Bad check condition:" + ex);
                errlist = "class failed to parse";
                return null;
            }
        }


        // static
        // Check condition list fel, using the outercondition on each to combine the results
        // values are the set of values to use for variable lookups
        // pass back errlist, errclass
        // optionally pass back conditions which passed
        // optional use the function/macro expander on both sides
        // optionally shortcircuit on outer AND condition
        // obeys disabled

        static public bool? CheckConditions(List<Condition> fel, Variables values, out string errlist, out ErrorClass errclass, 
                                            List<Condition> passed = null, 
                                            Functions functionmacroexpander = null, 
                                            bool shortcircuitouter = false,
                                            bool debugit = false)
        {
            errlist = null;
            errclass = ErrorClass.None;

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
                    {   // at least a left side
                        string leftside = null;
                        Functions.ExpandResult leftexpansionresult = Functions.ExpandResult.NoExpansion;

                        if (functionmacroexpander != null)     // if we have a string expander, try the left side
                        {
                            leftexpansionresult = functionmacroexpander.ExpandString(ce.ItemName, out leftside);

                            if (leftexpansionresult == Functions.ExpandResult.Failed)        // stop on error
                            {
                                errlist += leftside;     // add on errors..
                                innerres = false;   // stop loop, false
                                break;
                            }
                        }

                        // variable names

                        if (ce.MatchCondition == ConditionEntry.MatchType.IsPresent)         
                        {
                            if (leftside == null || leftexpansionresult == Functions.ExpandResult.NoExpansion)     // no expansion, must be a variable name
                                leftside = values.Qualify(ce.ItemName);                 // its a straight variable name, allow any special formatting

                            if (values.Exists(leftside) && values[leftside] != null)
                                matched = true;
                        }
                        else if (ce.MatchCondition == ConditionEntry.MatchType.IsNotPresent)
                        {
                            if (leftside == null || leftexpansionresult == Functions.ExpandResult.NoExpansion)     // no expansion, must be a variable name
                                leftside = values.Qualify(ce.ItemName);                 // its a straight variable name, allow any special formatting

                            if (!values.Exists(leftside) || values[leftside] == null)
                                matched = true;
                        }
                        else
                        {   
                            // if no expansion, then pass thru the eval engine now or its a variable name

                            if (leftexpansionresult == Functions.ExpandResult.NoExpansion)     
                            {
                                string qualname = values.Qualify(ce.ItemName);
                                leftside = values.Exists(qualname) ? values[qualname] : null;   // then lookup.. lookup may also be null if its a pre-def

                                if (leftside == null)
                                {
                                    errlist += "Variable '" + qualname + "' does not exist" + Environment.NewLine;
                                    errclass = ErrorClass.LeftSideVarUndefined;
                                    innerres = false;
                                    break;                       // stop the loop, its a false
                                }
                            }

                         //   System.Diagnostics.Debug.WriteLine($".. left side {leftside}");

                            // left side only

                            if (ce.MatchCondition == ConditionEntry.MatchType.IsEmpty)
                            {
                                matched = leftside.Length == 0;
                            }
                            else if (ce.MatchCondition == ConditionEntry.MatchType.IsNotEmpty)
                            {
                                matched = leftside.Length > 0;
                            }
                            else if (ce.MatchCondition == ConditionEntry.MatchType.IsTrue || ce.MatchCondition == ConditionEntry.MatchType.IsFalse)
                            {
                                int inum = 0;

                                if (leftside.InvariantParse(out inum))
                                    matched = (ce.MatchCondition == ConditionEntry.MatchType.IsTrue) ? (inum != 0) : (inum == 0);
                                else
                                {
                                    errlist += "True/False value is not an integer on left side" + Environment.NewLine;
                                    errclass = ErrorClass.LeftSideBadFormat;
                                    innerres = false;
                                    break;
                                }
                            }
                            else
                            {
                                // require a right side

                                string rightside = null;
                                Functions.ExpandResult rightexpansionresult = Functions.ExpandResult.NoExpansion;

                                if (functionmacroexpander != null)         // if we have a string expander, pass it thru
                                {
                                    rightexpansionresult = functionmacroexpander.ExpandString(ce.MatchString, out rightside);

                                    if (rightexpansionresult == Functions.ExpandResult.Failed)        //  if error, abort
                                    {
                                        errlist += rightside;     // add on errors..
                                        innerres = false;   // stop loop, false
                                        break;
                                    }
                                }
                                else
                                    rightside = ce.MatchString;      // no eval, we just use the string

                                if (debugit)
                                    System.Diagnostics.Debug.WriteLine($"Condition `{leftside}` {ce.MatchCondition} `{rightside}`");

                                if (ce.MatchCondition == ConditionEntry.MatchType.DateBefore || ce.MatchCondition == ConditionEntry.MatchType.DateAfter)
                                {
                                    DateTime tmevalue, tmecontent;
                                    if (!DateTime.TryParse(leftside, System.Globalization.CultureInfo.CreateSpecificCulture("en-US"), System.Globalization.DateTimeStyles.None, out tmevalue))
                                    {
                                        errlist += "Date time not in correct format on left side: " + leftside +  Environment.NewLine;
                                        errclass = ErrorClass.LeftSideBadFormat;
                                        innerres = false;
                                        break;

                                    }
                                    else if (!DateTime.TryParse(rightside, System.Globalization.CultureInfo.CreateSpecificCulture("en-US"), System.Globalization.DateTimeStyles.None, out tmecontent))
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
                                    matched = leftside.Equals(rightside, StringComparison.InvariantCultureIgnoreCase);
                                else if (ce.MatchCondition == ConditionEntry.MatchType.EqualsCaseSensitive)
                                    matched = leftside.Equals(rightside);

                                else if (ce.MatchCondition == ConditionEntry.MatchType.NotEqual)
                                    matched = !leftside.Equals(rightside, StringComparison.InvariantCultureIgnoreCase);
                                else if (ce.MatchCondition == ConditionEntry.MatchType.NotEqualCaseSensitive)
                                    matched = !leftside.Equals(rightside);

                                else if (ce.MatchCondition == ConditionEntry.MatchType.Contains)
                                    matched = leftside.IndexOf(rightside, StringComparison.InvariantCultureIgnoreCase) >= 0;
                                else if (ce.MatchCondition == ConditionEntry.MatchType.ContainsCaseSensitive)
                                    matched = leftside.Contains(rightside);

                                else if (ce.MatchCondition == ConditionEntry.MatchType.DoesNotContain)
                                    matched = leftside.IndexOf(rightside, StringComparison.InvariantCultureIgnoreCase) < 0;
                                else if (ce.MatchCondition == ConditionEntry.MatchType.DoesNotContainCaseSensitive)
                                    matched = !leftside.Contains(rightside);
                                else if (ce.MatchCondition == ConditionEntry.MatchType.IsOneOf)
                                {
                                    StringParser p = new StringParser(rightside);
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
                                        matched = ret.Contains(leftside, StringComparer.InvariantCultureIgnoreCase);
                                    }
                                }
                                else if (ce.MatchCondition == ConditionEntry.MatchType.MatchSemicolon)
                                {
                                    string[] list = rightside.Split(';').Select(x => x.Trim()).ToArray();     // split and trim
                                    matched = list.Contains(leftside.Trim(), StringComparer.InvariantCultureIgnoreCase); // compare, trimmed, case insensitive
                                }
                                else if (ce.MatchCondition == ConditionEntry.MatchType.MatchCommaList)
                                {
                                    StringCombinations sc = new StringCombinations(',');
                                    sc.ParseString(rightside);      // parse, give all combinations
                                    matched = sc.Permutations.Contains(leftside.Trim(), StringComparer.InvariantCultureIgnoreCase); // compare, trimmed, case insensitive
                                }
                                else if (ce.MatchCondition == ConditionEntry.MatchType.MatchSemicolonList)
                                {
                                    StringCombinations sc = new StringCombinations(';');
                                    sc.ParseString(rightside);      // parse, give all combinations
                                    matched = sc.Permutations.Contains(leftside.Trim(), StringComparer.InvariantCultureIgnoreCase); // compare, trimmed, case insensitive
                                }
                                else if (ce.MatchCondition == ConditionEntry.MatchType.AnyOfAny)
                                {
                                    StringParser l = new StringParser(leftside);
                                    List<string> ll = l.NextQuotedWordList();

                                    StringParser r = new StringParser(rightside);
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
                                    double fnum = 0, num = 0;

                                    if (!leftside.InvariantParse(out num))
                                    {
                                        errlist += "Number not in correct format on left side: " + leftside + Environment.NewLine;
                                        errclass = ErrorClass.LeftSideBadFormat;
                                        innerres = false;
                                        break;
                                    }
                                    else if (!rightside.InvariantParse(out fnum))
                                    {
                                        errlist += "Number not in correct format on right side: " + rightside + Environment.NewLine;
                                        errclass = ErrorClass.RightSideBadFormat;
                                        innerres = false;
                                        break;
                                    }
                                    else
                                    {
                                        if (ce.MatchCondition == ConditionEntry.MatchType.NumericEquals)
                                            matched = num.ApproxEquals(fnum);

                                        else if (ce.MatchCondition == ConditionEntry.MatchType.NumericNotEquals)
                                            matched = !num.ApproxEquals(fnum); 

                                        else if (ce.MatchCondition == ConditionEntry.MatchType.NumericGreater)
                                            matched = num > fnum;

                                        else if (ce.MatchCondition == ConditionEntry.MatchType.NumericGreaterEqual)
                                            matched = num >= fnum;

                                        else if (ce.MatchCondition == ConditionEntry.MatchType.NumericLessThan)
                                            matched = num < fnum;

                                        else if (ce.MatchCondition == ConditionEntry.MatchType.NumericLessThanEqual)
                                            matched = num <= fnum;
                                        else
                                            System.Diagnostics.Debug.Assert(false);
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
                else if ( innerres == true)
                {
                   passed?.Add(cond);
                }

                if (!outerres.HasValue)                             // if first time, its just the value
                {
                    outerres = innerres.Value;

                    if (shortcircuitouter && oc < fel.Count - 1)       // check short circuits on NEXT ONE! if we have a next one..
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

                    if (shortcircuitouter && outerres.Value == true)      // no point continuing, first one true wins
                    {
                        //System.Diagnostics.Debug.WriteLine("Short circuit second on {0} cur {1}", fe.OuterCondition, outerres);
                        break;
                    }
                }
                else if (cond.OuterCondition == ConditionEntry.LogicalCondition.And)
                {
                    outerres &= innerres.Value;

                    if (shortcircuitouter && outerres.Value == false)      // no point continuing, first one false wins
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
