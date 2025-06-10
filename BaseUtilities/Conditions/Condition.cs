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
 *
 */

using System;
using System.Collections.Generic;

namespace BaseUtils
{
    public class Condition
    {
        public List<ConditionEntry> Fields { get; set; }             // its condition fields
        public LogicalCondition InnerCondition { get; set; }         // condition between fields
        public LogicalCondition OuterCondition { get; set; }         // condition between this set of Condition a nd the next set of Condition
        public string EventName { get; set; }                        // logical event its associated with (definition is up to user)
        public string Action { get; set; }                           // action associated with a pass (definition is up to user)
        public Variables ActionVars { get; set; }                    // any variables associated with the action (definition is up to user)
        public bool Disabled { get; set; }                           // if condition is currently disabled for consideration
        public string GroupName { get; set; }                        // group its assocated with. Can be null. (definition is up to user)
        public object Tag { get; set; }                              // User tag data

        public enum LogicalCondition
        {
            Or,     // any true     (DEFAULT)
            And,    // all true
            Nor,    // any true produces a false
            Nand,   // any not true produces a true
        }

        #region Init

        public Condition()
        {
            EventName = Action;
            ActionVars = new Variables();
        }

        public Condition(string eventname, string action, Variables actionvars, List<ConditionEntry> conditions, 
                            LogicalCondition inner = LogicalCondition.Or , 
                            LogicalCondition outer = LogicalCondition.Or)
        {
            EventName = eventname;
            Action = action;
            ActionVars = new Variables(actionvars);
            InnerCondition = inner;
            OuterCondition = outer;
            Fields = conditions;
        }

        public Condition(ConditionEntry ce)
        {
            Fields = new List<ConditionEntry>() { ce };
        }

        public Condition(Condition other)   // full clone
        {
            EventName = other.EventName;
            if (other.Fields != null)
            {
                Fields = new List<ConditionEntry>();
                foreach (ConditionEntry e in other.Fields)
                    Fields.Add(new ConditionEntry(e));
            }
            InnerCondition = other.InnerCondition;
            OuterCondition = other.OuterCondition;
            Action = other.Action;
            ActionVars = new Variables(other.ActionVars);
            Disabled = other.Disabled;
            GroupName = other.GroupName;
            Tag = other.Tag;
        }

        // Create from string. i,o can have spaces inserted into enum
        public bool Create(string eventname, string innercondition, string outercondition)   
        {
            try
            {
                EventName = eventname;
                Action = "";
                ActionVars = new Variables();
                InnerCondition = (LogicalCondition)Enum.Parse(typeof(LogicalCondition), innercondition.Replace(" ", ""), true);       // must work, exception otherwise
                OuterCondition = (LogicalCondition)Enum.Parse(typeof(LogicalCondition), outercondition.Replace(" ", ""), true);       // must work, exception otherwise
                return true;
            }
            catch { }

            return false;
        }

        #endregion

        #region Manip

        public bool IsAlwaysTrue()
        {
            foreach (ConditionEntry c in Fields)
            {
                if (c.MatchCondition == ConditionEntry.MatchType.AlwaysTrue)
                    return true;
            }

            return false;
        }

        public bool IsAlwaysFalse()
        {
            foreach (ConditionEntry c in Fields)
            {
                if (c.MatchCondition != ConditionEntry.MatchType.AlwaysFalse)
                    return false;
            }

            return true;
        }

        public bool Is(string itemname, ConditionEntry.MatchType mt)        // one condition, of this type
        {
            return Fields.Count == 1 && Fields[0].ItemName == itemname && Fields[0].MatchCondition == mt;
        }

        public void SetAlwaysTrue()
        {
            Fields = new List<ConditionEntry>();
            Fields.Add(new ConditionEntry("Condition", ConditionEntry.MatchType.AlwaysTrue, ""));
        }

        public void SetAlwaysFalse()
        {
            Fields = new List<ConditionEntry>();
            Fields.Add(new ConditionEntry("Condition", ConditionEntry.MatchType.AlwaysFalse, ""));
        }

        static public Condition AlwaysTrue()
        {
            Condition cd = new Condition();
            cd.SetAlwaysTrue();
            return cd;
        }

        static public Condition AlwaysFalse()
        {
            Condition cd = new Condition();
            cd.SetAlwaysFalse();
            return cd;
        }

        public void Set(ConditionEntry f)
        {
            Fields = new List<ConditionEntry>() { f };
        }

        public void Add(ConditionEntry f)
        {
            if (Fields == null)
                Fields = new List<ConditionEntry>();
            Fields.Add(f);
        }

        // list into CV the variables needed for the condition entry list
        public void IndicateValuesNeeded(ref Variables vr)
        {
            if (!Disabled)
            {
                foreach (ConditionEntry fd in Fields)
                {
                    if (!ConditionEntry.IsNullOperation(fd.MatchCondition) && !fd.ItemName.Contains("%"))     // nulls need no data..  nor does anything with expand in
                        vr[fd.ItemName] = null;
                }
            }
        }

        // using the Eval engine
        // Find all funcs/symbols. 
        static public void InUse(List<Condition> fel, Eval exp, out HashSet<string> symnames, out HashSet<string> funcnames)
        {
            symnames = new HashSet<string>();
            funcnames = new HashSet<string>();

            foreach (Condition c in fel)
            {
                if (!c.Disabled)
                {
                    foreach (ConditionEntry ce in c.Fields)
                    {
                        exp.SymbolsFuncsInExpression(ce.ItemName, symnames, funcnames);
                        exp.SymbolsFuncsInExpression(ce.MatchString, symnames, funcnames);
                    }
                }
            }
        }


        static public string GetLogicalCondition(BaseUtils.StringParser sp, string delimchars, out LogicalCondition value)
        {
            value = LogicalCondition.Or;

            string condi = sp.NextQuotedWord(delimchars);       // next is the inner condition..

            if (condi == null)
                return "Condition operator missing";

            condi = condi.Replace(" ", "");

            if (Enum.TryParse<LogicalCondition>(condi.Replace(" ", ""), true, out value))
                return "";
            else
                return "Condition operator " + condi + " is not recognised";
        }

        #endregion

        #region Comparision

        // if field is empty/null, not considered. wildcard match of any of the fields
        public bool WildCardMatch(string groupname, string eventname, string actionstr, string actionvarstr, string condition, bool caseinsensitive = true)
        {
            if ((groupname.HasChars() && GroupName.HasChars() && GroupName.WildCardMatch(groupname, caseinsensitive)) ||        // groupname can be null
                (eventname.HasChars() && EventName.WildCardMatch(eventname, caseinsensitive)) ||
                (actionstr.HasChars() && Action.WildCardMatch(actionstr, caseinsensitive)) ||
                (actionvarstr.HasChars() && ActionVars.ToString().WildCardMatch(actionvarstr, caseinsensitive)))
            {
                return true;
            }
            if (condition.HasChars())
            {
                string cond = ToString(false);
                if (cond.WildCardMatch(condition, caseinsensitive))
                    return true;
            }
            return false;
        }

        // Sees if two conditions are equal. Case insensitivity only for groupbname,eventname,action.  Others the case is significant (variables, condition)
        public bool Equals(Condition c, StringComparison ci = StringComparison.InvariantCultureIgnoreCase)
        {
            return (GroupName.HasChars() && GroupName.Equals(c.GroupName, ci) &&         // groupname can be null
                            EventName.Equals(c.EventName, ci) &&
                            Action.Equals(c.Action, ci) &&
                            ActionVars.ToString() == c.ActionVars.ToString()
                            && ToString(false) == c.ToString(false));
        }

        #endregion

        #region Input/Output

        public string ToString(bool includeaction = false, bool multi = false)          // multi means quoting needed for ) as well as comma space
        {
            string ret = "";

            if (includeaction)
            {
                ret += EventName.QuoteString(comma: true) + ", " + Action.QuoteString(comma: true) + ", ";
                if (ActionVars.Count == 0)
                    ret += "\"\", ";
                else
                {
                    string v = ActionVars.ToString();
                    if (v.Contains("\"") || v.Contains(","))
                        ret += "\"" + v.Replace("\"", "\\\"") + "\", ";     // verified 12/06/2020
                    else
                        ret += v + ", ";
                }
            }

            for (int i = 0; Fields != null && i < Fields.Count; i++)
            {
                if (i > 0)
                    ret += " " + InnerCondition.ToString() + " ";

                if (ConditionEntry.IsNullOperation(Fields[i].MatchCondition))
                    ret += "Condition " + ConditionEntry.OperatorNames[(int)Fields[i].MatchCondition];
                else
                {
                    ret += (Fields[i].ItemName).QuoteString(bracket: multi) +               // commas do not need quoting as conditions at written as if always at EOL.
                            " " + ConditionEntry.OperatorNames[(int)Fields[i].MatchCondition];

                    if (!ConditionEntry.IsUnaryOperation(Fields[i].MatchCondition))
                        ret += " " + Fields[i].MatchString.QuoteString(bracket: multi);     // commas do not need quoting..
                }
            }

            return ret;
        }

        public string Read( string s , bool includeevent = false, string delimchars = " ")
        {
            BaseUtils.StringParser sp = new BaseUtils.StringParser(s);
            return Read(sp, includeevent, delimchars);
        }

        // if includeevent is set, it must be there..
        // demlimchars is normally space, but can be ") " if its inside a multi.

        public string Read(BaseUtils.StringParser sp, bool includeevent = false, string delimchars = " ") 
        {                                                                                           
            Fields = new List<ConditionEntry>();
            InnerCondition = OuterCondition = LogicalCondition.Or;
            EventName = ""; Action = "";
            ActionVars = new Variables();

            if (includeevent)                                                                   
            {
                string actionvarsstr;
                if ((EventName = sp.NextQuotedWord(", ")) == null || !sp.IsCharMoveOn(',') ||
                    (Action = sp.NextQuotedWord(", ")) == null || !sp.IsCharMoveOn(',') ||
                    (actionvarsstr = sp.NextQuotedWord(", ")) == null || !sp.IsCharMoveOn(','))
                {
                    return "Incorrect format of EVENT data associated with condition";
                }

                if ( actionvarsstr.HasChars())
                    ActionVars = new Variables(actionvarsstr, Variables.FromMode.MultiEntryComma); 
            }

            LogicalCondition? ic = null;

            while (true)
            {
                string var = sp.NextQuotedWord(delimchars);             // always has para cond
                if (var == null)
                    return "Missing parameter (left side) of condition";

                string cond = sp.NextQuotedWord(delimchars);
                if (cond == null)
                    return "Missing condition operator";

                ConditionEntry.MatchType mt;
                if (!ConditionEntry.MatchTypeFromString(cond, out mt))
                    return "Condition operator " + cond + " is not recognised";

                string value = "";

                if (ConditionEntry.IsNullOperation(mt)) // null operators (Always..)
                {
                    if (!var.Equals("Condition", StringComparison.InvariantCultureIgnoreCase))
                        return "Condition must preceed fixed result operator";
                    var = "Condition";  // fix case..
                }
                else if (!ConditionEntry.IsUnaryOperation(mt) ) // not unary, require right side
                {
                    value = sp.NextQuotedWord(delimchars);
                    if (value == null)
                        return "Missing value part (right side) of condition";
                }

                ConditionEntry ce = new ConditionEntry() { ItemName = var, MatchCondition = mt, MatchString = value };
                Fields.Add(ce);

                if (sp.IsEOL || sp.PeekChar() == ')')           // end is either ) or EOL
                {
                    InnerCondition = (ic == null) ? LogicalCondition.Or : ic.Value;
                    return "";
                }
                else
                {
                    LogicalCondition nic;
                    string err = GetLogicalCondition(sp, delimchars, out nic);
                    if (err.Length > 0)
                        return err + " for inner condition";

                    if (ic == null)
                        ic = nic;
                    else if (ic.Value != nic)
                        return "Cannot specify different inner conditions between expressions";
                }
            }
        }

        #endregion
    }
}
