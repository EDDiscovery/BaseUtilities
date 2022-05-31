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

using QuickJSON;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaseUtils
{
    public partial class ConditionLists
    {
        public enum ErrorClass      // errors are passed back by functions in ErrorList (null okay, else string description) plus ErrorClass
        {
            None,
            LeftSideBadFormat,
            RightSideBadFormat,
            LeftSideVarUndefined,
            RightSideVarUndefined,
            ExprFormatError,
        };

        private List<Condition> conditionlist = new List<Condition>();

        #region Condition List setup and read/write

        public ConditionLists()
        {
        }

        public ConditionLists(ConditionLists other)
        {
            foreach (Condition c in other.conditionlist)
                conditionlist.Add(new Condition(c));        //copy, not reference
        }

        public ConditionLists(string condtext)
        {
            Read(condtext);
        }

        public int Count { get { return conditionlist.Count; } }
        public Condition this[int n] { get { return (n < conditionlist.Count) ? conditionlist[n] : null; } }

        public IEnumerable<Condition> Enumerable { get { return conditionlist; } }

        public List<Condition> List { get { return conditionlist; } }

        public void Add(Condition fe)
        {
            conditionlist.Add(fe);
        }

        public void Remove(Condition fe)
        {
            conditionlist.Remove(fe);
        }

        public void Clear()
        {
            conditionlist.Clear();
        }

        // Do we have this condition? Case insensitivity only for groupbname,eventname,action.  Others the case is significant (variables, condition)
        public bool Contains( Condition c, StringComparison ci = StringComparison.InvariantCultureIgnoreCase)
        {
            foreach( var e in conditionlist)
            {
                if (e.Equals(c,ci))
                    return true;
            }
            return false;
        }

        // if field is empty/null, not considered. wildcard match
        public List<Condition> Find(string groupname, string eventname, string action, string actionvarstr, string condition, bool caseinsensitive = true)
        {
            List<Condition> res = new List<Condition>();
            for (int i = 0; i < conditionlist.Count(); i++)
            {
                if (conditionlist[i].WildCardMatch(groupname, eventname, action, actionvarstr, condition,caseinsensitive))
                    res.Add(conditionlist[i]);
            }
            return res;
        }

        public string GetJSON()
        {
            return GetJSONObject().ToString();
        }

        // Find all variable names, optionally including matchstrings if they conform to variable format (_A plus _A0 following)
        public HashSet<string> VariablesUsed(bool matchstrings = false, bool allowmembers = false)
        {
            HashSet<string> str = new HashSet<string>();
            foreach (Condition c in conditionlist)
                c.VariableNamesUsed(str, matchstrings, allowmembers);
            return str;
        }

        // verified 31/7/2020 with QuickJSON 
        public JObject GetJSONObject() 
        {
            JObject evt = new JObject();

            JArray jf = new JArray();

            foreach (Condition f in conditionlist)
            {
                JObject j1 = new JObject();
                j1["EventName"] = f.EventName;
                if (f.InnerCondition != ConditionEntry.LogicalCondition.Or)
                    j1["ICond"] = f.InnerCondition.ToString();
                if (f.OuterCondition != ConditionEntry.LogicalCondition.Or)
                    j1["OCond"] = f.OuterCondition.ToString();
                if (f.Action.Length > 0)
                    j1["Actions"] = f.Action;
                if (f.ActionVars.Count > 0)
                    j1["ActionData"] = f.ActionVars.ToString();

                JArray jfields = new JArray();

                foreach (ConditionEntry fd in f.Fields)
                {
                    JObject j2 = new JObject();
                    j2["Item"] = fd.ItemName;
                    j2["Content"] = fd.MatchString;
                    j2["Matchtype"] = fd.MatchCondition.ToString();
                    jfields.Add(j2);
                }

                j1["Filters"] = jfields;

                jf.Add(j1);
            }

            evt["FilterSet"] = jf;

            return evt;
        }

        public bool FromJSON(string s)
        {
            Clear();

            try
            {
                JObject jo = (JObject)JObject.ParseThrowCommaEOL(s);
                return FromJSON(jo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Bad condition JSON:" + ex);
                return false;
            }
        }

        public bool FromJSON(JObject jo)        // rechecked 31/7/2020 with quickjson
        {
            try
            {
                Clear();

                JArray jf = (JArray)jo["FilterSet"];

                foreach (JObject j in jf)
                {
                    // verified 31/7/2020 with QuickJSON   If object not present, returns JNotPresent and Str() returns default
                    string evname = (string)j["EventName"];
                    ConditionEntry.LogicalCondition ftinner = (ConditionEntry.LogicalCondition)Enum.Parse(typeof(ConditionEntry.LogicalCondition), j["ICond"].Str("Or"));
                    ConditionEntry.LogicalCondition ftouter = (ConditionEntry.LogicalCondition)Enum.Parse(typeof(ConditionEntry.LogicalCondition), j["OCond"].Str("Or"));
                    string act = j["Actions"].Str();
                    string actd = j["ActionData"].Str();

                    JArray filset = (JArray)j["Filters"];

                    List<ConditionEntry> fieldlist = new List<ConditionEntry>();

                    foreach (JObject j2 in filset)
                    {
                        string item = (string)j2["Item"];
                        string content = (string)j2["Content"];
                        string matchtype = (string)j2["Matchtype"];

                        fieldlist.Add(new ConditionEntry()
                        {
                            ItemName = item,
                            MatchString = content,
                            MatchCondition = (ConditionEntry.MatchType)Enum.Parse(typeof(ConditionEntry.MatchType), matchtype)
                        });
                    }

                    conditionlist.Add(new Condition()
                    {
                        EventName = evname,
                        InnerCondition = ftinner,
                        OuterCondition = ftouter,
                        Fields = fieldlist,
                        Action = act,
                        ActionVars = new Variables(actd, Variables.FromMode.MultiEntryComma)
                    });
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Bad condition JSON:" + ex);
            }

            return false;
        }

        public override string ToString()
        {
            string ret = "";
            bool multi = conditionlist.Count > 1;

            if (multi)
                ret += "(";

            for (int j = 0; j < conditionlist.Count; j++)       // First outer is NOT used, second on is..
            {
                Condition f = conditionlist[j];

                if (j > 0)
                    ret += ") " + f.OuterCondition.ToString() + " (";

                ret += f.ToString(multi: multi);
            }

            if (multi)
                ret += ")";

            return ret;
        }

        public string Read(string line)         // decode a set of multi conditions (<cond> Or <cond>) Outer (<cond> And <cond>) etc 
        {
            StringParser sp = new StringParser(line);

            bool multi = false;

            string delimchars = " ";
            if (sp.IsCharMoveOn('('))
            {
                multi = true;
                delimchars = ") ";
            }

            List<Condition> cllist = new List<Condition>();

            ConditionEntry.LogicalCondition outercond = ConditionEntry.LogicalCondition.Or;         // first outer condition is ignored in a list.  Or is the default.

            while (true)
            {
                Condition c = new Condition();

                string err = c.Read(sp, delimchars: delimchars);
                if (err.Length > 0)
                    return err;

                c.OuterCondition = outercond;
                cllist.Add(c);      // add..

                if (sp.IsCharMoveOn(')'))      // if closing bracket..
                {
                    if (!multi)
                        return "Closing condition bracket found but no opening bracket present";

                    if (sp.IsEOL)  // EOL, end of  (cond..cond) outercond ( cond cond)
                    {
                        Clear();
                        conditionlist = cllist;
                        return null;
                    }
                    else
                    {
                        err = ConditionEntry.GetLogicalCondition(sp, delimchars, out outercond);
                        if (err.Length > 0)
                            return err + " for outer condition";

                        if (!sp.IsCharMoveOn('(')) // must have another (
                            return "Missing opening bracket in multiple condition list after " + outercond.ToString();
                    }
                }
                else if (sp.IsEOL) // last condition
                {
                    if (multi)
                        return "Missing closing braket in multiple condition list";

                    Clear();
                    conditionlist = cllist;
                    return null;
                }
                else
                    return "Extra characters beyond expression";
            }
        }

        #endregion

        #region Helpers

        // is condition variable flag in actiondata set
        // obeys disabled flag

        public bool IsActionVarDefined(string flagvar)
        {
            foreach (Condition l in conditionlist)
            {
                if ( !l.Disabled && l.ActionVars.Exists(flagvar))
                    return true;
            }

            return false;
        }

        // Event name.. give me conditions which match that name or ALL
        // flagstart, if not null ,compare with start of action data and include only if matches
        // obeys disabled flag

        public List<Condition> GetConditionListByEventName(string eventname, string flagvar = null)
        {
            List<Condition> fel;

            if (flagvar != null)
                fel = (from fil in conditionlist
                       where
                       !fil.Disabled &&
                     (fil.EventName.Equals("All", StringComparison.InvariantCultureIgnoreCase) || fil.EventName.Equals(eventname, StringComparison.InvariantCultureIgnoreCase)) &&
                     fil.ActionVars.Exists(flagvar)
                       select fil).ToList();

            else
                fel = (from fil in conditionlist
                       where
                       !fil.Disabled &&
                     (fil.EventName.Equals("All", StringComparison.InvariantCultureIgnoreCase) || fil.EventName.Equals(eventname, StringComparison.InvariantCultureIgnoreCase))
                       select fil).ToList();

            return (fel.Count == 0) ? null : fel;
        }

        // give back all conditions which match itemname and have a compatible matchtype.. used for key presses/voice input to compile a list of condition data to check for
        // obeys disabled flag
        public List<Tuple<string,ConditionEntry>> ReturnSpecificConditions(string eventname, string itemname, List<ConditionEntry.MatchType> matchtypes)      // given itemname, give me a list of values it is matched against
        {
            var ret = new List<Tuple<string, ConditionEntry>>();

            foreach (Condition fe in conditionlist)        // find all values needed
            {
                if (!fe.Disabled)
                {
                    if (fe.EventName == eventname)
                    {
                        foreach (ConditionEntry ce in fe.Fields)
                        {
                            if (ce.ItemName.Equals(itemname) && matchtypes.Contains(ce.MatchCondition))
                                ret.Add(new Tuple<string, ConditionEntry>(fe.GroupName, ce));
                        }
                    }
                }
            }

            return ret;
        }

        #endregion

        #region Check conditions public functions

        // TRUE if filter is True and has value
        public bool CheckFilterTrue(Object cls, Variables[] othervars, out string errlist, List<Condition> passed)      // if none, true, if false, true.. 
        {                                                                                         // only if the filter passes do we get a false..
            bool? v = CheckConditionWithObjectData(conditionlist, cls, othervars, out errlist, out ErrorClass errclassunused, passed);
            return (v.HasValue && v.Value);     // true IF we have a positive result
        }

        // Filter OUT if condition matches..
        public bool CheckFilterFalse(Object cls, string eventname, Variables[] othervars, out string errlist , List<Condition> passed)      // if none, true, if false, true.. 
        {
            List<Condition> fel = GetConditionListByEventName(eventname);       // first find conditions applicable, filtered by eventname

            if (fel != null)        // if we have matching filters..
            {
                bool? v = CheckConditionWithObjectData(fel, cls, othervars, out errlist, out ErrorClass errclassunused, passed);  // true means filter matched
                bool res = !v.HasValue || v.Value == false;
                //System.Diagnostics.Debug.WriteLine("Event " + eventname + " res " + res + " v " + v + " v.hv " + v.HasValue);
                return res; // no value, true .. false did not match, thus true
            }
            else
            {
                errlist = null;
                return true;
            }
        }

        // member function
        // check all conditions against these values, one by one.  Outercondition of each Condition determines if this is an OR or AND etc operation
        // shortcircuit stop
        // no functions
        // left side is always a variable
        // right side is constant or a variable name or a "quoted escaped string"
        public bool? CheckAgainstVariables(Variables values, out string errlist, out ErrorClass errclass)            // Check all conditions..
        {
            if (conditionlist.Count == 0)            // no filters match, null
            {
                errlist = null;
                errclass = ErrorClass.None;
                return null;
            }

            var res = CheckConditions(conditionlist, values, out errlist, out errclass, shortcircuitouter: true, variablesonright: true, allowmembersyntaxonright: true);
            //  if (errlist.HasChars()) System.Diagnostics.Debug.WriteLine($"Note {errclass} {errlist}");
            return res;
        }

        // member function for statements like IF, with functions
        // Check conditions against variables
        // shortcircuit stop
        public bool? CheckAll(Variables values, out string errlist, Functions cf )
        {
            if (conditionlist.Count == 0)            // no filters match, null
            {
                errlist = null;
                return null;
            }

            var res = CheckConditions(conditionlist, values, out errlist, out ErrorClass errclassunused, null, cf, shortcircuitouter: true);
            //  if (errlist.HasChars()) System.Diagnostics.Debug.WriteLine($"Note {errclass} {errlist}");
            return res;
        }


        #endregion
    }
}
