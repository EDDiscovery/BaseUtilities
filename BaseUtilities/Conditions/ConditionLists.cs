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
        public ConditionLists(ConditionEntry ce)
        {
            Condition c = new Condition();
            c.Add(ce);
            Add(c);
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

        // verified 31/7/2020 with QuickJSON 
        public JObject GetJSONObject() 
        {
            JObject evt = new JObject();

            JArray jf = new JArray();

            foreach (Condition f in conditionlist)
            {
                JObject j1 = new JObject();
                j1["EventName"] = f.EventName;
                if (f.InnerCondition != Condition.LogicalCondition.Or)
                    j1["ICond"] = f.InnerCondition.ToString();
                if (f.OuterCondition != Condition.LogicalCondition.Or)
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
                    Condition.LogicalCondition ftinner = (Condition.LogicalCondition)Enum.Parse(typeof(Condition.LogicalCondition), j["ICond"].Str("Or"));
                    Condition.LogicalCondition ftouter = (Condition.LogicalCondition)Enum.Parse(typeof(Condition.LogicalCondition), j["OCond"].Str("Or"));
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
            return ToString(List);
        }

        public static string ToString(List<Condition> list)
        {
            string ret = "";
            bool multi = list.Count > 1;

            if (multi)
                ret += "(";

            for (int j = 0; j < list.Count; j++)       // First outer is NOT used, second on is..
            {
                Condition f = list[j];

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

            Condition.LogicalCondition outercond = Condition.LogicalCondition.Or;         // first outer condition is ignored in a list.  Or is the default.

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
                        err = Condition.GetLogicalCondition(sp, delimchars, out outercond);
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

        public Dictionary<string, List<Condition>> GetConditionListDictionaryByEventName()
        {
            var dict = new Dictionary<string, List<Condition>>();
            foreach (var v in conditionlist)
            {
                if (!dict.ContainsKey(v.EventName))
                    dict[v.EventName] = new List<Condition>();
                dict[v.EventName].Add(v);
            }

            return dict;
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

        #region Check conditions Member func

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
