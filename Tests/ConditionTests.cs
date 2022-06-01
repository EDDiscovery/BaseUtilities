/*
* Copyright © 2018 EDDiscovery development team
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
using BaseUtils;
using NFluent;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace EDDiscoveryTests
{
    [TestFixture(TestOf = typeof(Eval))]
    public class ConditionTests
    {
        [Test]
        public void Conditions()
        {
            {
                Variables vars = new Variables();
                vars["V10"] = "10";
                vars["V202"] = "20.2";
                vars["Vstr1"] = "string1";
                vars["Vstr4"] = "string4";
                vars["Rings[0].outerrad"] = "20";

                Variables actionv = new Variables(new string[] { "o1", "1", "o2", "2" });

                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("V10*10", ConditionEntry.MatchType.NumericEquals, "V10*10"));        // multiplication on both sides
                    Condition cd = new Condition("e1", "f1", actionv, ce1, ConditionEntry.LogicalCondition.And);

                    bool? check = ConditionLists.CheckConditions(new List<Condition> { cd }, vars, out string errlist, out ConditionLists.ErrorClass err, shortcircuitouter: true, useeval: true);
                    Check.That(check.Value).Equals(true);
                }

                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("Vstr1", ConditionEntry.MatchType.Equals, "string1"));           // var vs bare string
                    Condition cd = new Condition("e1", "f1", actionv, ce1, ConditionEntry.LogicalCondition.And);

                    bool? check = ConditionLists.CheckConditions(new List<Condition> { cd }, vars, out string errlist, out ConditionLists.ErrorClass err, shortcircuitouter: true, useeval: true);
                    Check.That(check.Value).Equals(true);
                }

                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("Vstr1", ConditionEntry.MatchType.Equals, "Vstr1"));         // var string vs var string
                    Condition cd = new Condition("e1", "f1", actionv, ce1, ConditionEntry.LogicalCondition.And);

                    bool? check = ConditionLists.CheckConditions(new List<Condition> { cd }, vars, out string errlist, out ConditionLists.ErrorClass err, shortcircuitouter: true, useeval: true);
                    Check.That(check.Value).Equals(true);
                }

                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("Rings[0].outerrad*2", ConditionEntry.MatchType.NumericEquals, "40"));         // complex symbol with mult vs number
                    Condition cd = new Condition("e1", "f1", actionv, ce1, ConditionEntry.LogicalCondition.And);

                    bool? check = ConditionLists.CheckConditions(new List<Condition> { cd }, vars, out string errlist, out ConditionLists.ErrorClass err, shortcircuitouter: true, useeval: true);
                    Check.That(check.Value).Equals(true);
                }

                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("Rings[0].outerrad*2", ConditionEntry.MatchType.NumericEquals, "s40"));         // unknown var right side
                    Condition cd = new Condition("e1", "f1", actionv, ce1, ConditionEntry.LogicalCondition.And);

                    bool? check = ConditionLists.CheckConditions(new List<Condition> { cd }, vars, out string errlist, out ConditionLists.ErrorClass err, shortcircuitouter: true, useeval: true);
                    Check.That(err).Equals(ConditionLists.ErrorClass.RightSideVarUndefined);
                    Check.That(check.Value).Equals(false);
                }
                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("wRings[0].outerrad*2", ConditionEntry.MatchType.NumericEquals, "s40"));         // unknown var left side
                    Condition cd = new Condition("e1", "f1", actionv, ce1, ConditionEntry.LogicalCondition.And);

                    bool? check = ConditionLists.CheckConditions(new List<Condition> { cd }, vars, out string errlist, out ConditionLists.ErrorClass err, shortcircuitouter: true, useeval: true);
                    Check.That(err).Equals(ConditionLists.ErrorClass.LeftSideVarUndefined);
                    Check.That(check.Value).Equals(false);
                }

                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("Rings[0].outerrad", ConditionEntry.MatchType.IsPresent, ""));         // var present
                    Condition cd = new Condition("e1", "f1", actionv, ce1, ConditionEntry.LogicalCondition.And);

                    bool? check = ConditionLists.CheckConditions(new List<Condition> { cd }, vars, out string errlist, out ConditionLists.ErrorClass err, shortcircuitouter: true, useeval: true);
                    Check.That(check.Value).Equals(true);
                }

            }

            {
                List<ConditionEntry> lfields = new List<ConditionEntry>();
                lfields.Add(new ConditionEntry("V1", ConditionEntry.MatchType.Equals, "A1"));
                lfields.Add(new ConditionEntry("V2", ConditionEntry.MatchType.Equals, "A2"));

                Condition left = new Condition("E1", "A1", new Variables(new string[] { "o1", "1", "o2", "2" }), lfields, ConditionEntry.LogicalCondition.Or, ConditionEntry.LogicalCondition.NA);

                List<ConditionEntry> rfields = new List<ConditionEntry>();
                rfields.Add(new ConditionEntry("V3", ConditionEntry.MatchType.Equals, "A3"));
                rfields.Add(new ConditionEntry("V4", ConditionEntry.MatchType.Equals, "A4"));

                Condition right = new Condition("E1", "A1", new Variables(new string[] { "o1", "1", "o2", "2" }), rfields, ConditionEntry.LogicalCondition.Or, ConditionEntry.LogicalCondition.And);

                ConditionLists cl = new ConditionLists();
                cl.Add(left);
                cl.Add(right);

                {
                    Variables vars = new Variables();
                    vars["V1"] = "A1";
                    vars["V2"] = "A2";
                    vars["V3"] = "A3";
                    vars["V4"] = "A4";
                    bool? check = ConditionLists.CheckConditions(cl.List, vars, out string errlist, out ConditionLists.ErrorClass err);
                    Check.That(check.Value).Equals(true);
                }
                {
                    Variables vars = new Variables();
                    vars["V1"] = "A1";
                    vars["V2"] = "X";
                    vars["V3"] = "A3";
                    vars["V4"] = "A4";
                    bool? check = ConditionLists.CheckConditions(cl.List, vars, out string errlist, out ConditionLists.ErrorClass err);
                    Check.That(check.Value).Equals(true);
                }
                {
                    Variables vars = new Variables();
                    vars["V1"] = "X";
                    vars["V2"] = "X";
                    vars["V3"] = "A3";
                    vars["V4"] = "A4";
                    bool? check = ConditionLists.CheckConditions(cl.List, vars, out string errlist, out ConditionLists.ErrorClass err, shortcircuitouter: true);
                    Check.That(check.Value).Equals(false);
                }
                {
                    Variables vars = new Variables();
                    vars["V1"] = "A1";
                    vars["V2"] = "X";
                    vars["V3"] = "X";
                    vars["V4"] = "A4";
                    bool? check = ConditionLists.CheckConditions(cl.List, vars, out string errlist, out ConditionLists.ErrorClass err);
                    Check.That(check.Value).Equals(true);
                }

                {
                    Variables vars = new Variables();
                    vars["V1"] = "A1";
                    vars["V2"] = "X";
                    vars["V3"] = "X";
                    vars["V4"] = "X";
                    bool? check = ConditionLists.CheckConditions(cl.List, vars, out string errlist, out ConditionLists.ErrorClass err, shortcircuitouter: true);
                    Check.That(check.Value).Equals(false);
                }

            }


            {
                List<ConditionEntry> lfields = new List<ConditionEntry>();
                lfields.Add(new ConditionEntry("V1", ConditionEntry.MatchType.Equals, "A1"));
                lfields.Add(new ConditionEntry("V2", ConditionEntry.MatchType.Equals, "A2"));

                Condition left = new Condition("E1", "A1", new Variables(new string[] { "o1", "1", "o2", "2" }), lfields, ConditionEntry.LogicalCondition.And, ConditionEntry.LogicalCondition.NA);

                List<ConditionEntry> rfields = new List<ConditionEntry>();
                rfields.Add(new ConditionEntry("V3", ConditionEntry.MatchType.Equals, "A3"));
                rfields.Add(new ConditionEntry("V4", ConditionEntry.MatchType.Equals, "A4"));

                Condition right = new Condition("E1", "A1", new Variables(new string[] { "o1", "1", "o2", "2" }), rfields, ConditionEntry.LogicalCondition.Or, ConditionEntry.LogicalCondition.And);

                ConditionLists cl = new ConditionLists();
                cl.Add(left);
                cl.Add(right);

                {
                    Variables vars = new Variables();
                    vars["V1"] = "A1";
                    vars["V2"] = "A2";
                    vars["V3"] = "A3";
                    vars["V4"] = "A4";
                    bool? check = ConditionLists.CheckConditions(cl.List, vars, out string errlist, out ConditionLists.ErrorClass err, shortcircuitouter: true);
                    Check.That(check.Value).Equals(true);
                }
                {
                    Variables vars = new Variables();
                    vars["V1"] = "A1";
                    vars["V2"] = "X";
                    vars["V3"] = "A3";
                    vars["V4"] = "A4";
                    bool? check = ConditionLists.CheckConditions(cl.List, vars, out string errlist, out ConditionLists.ErrorClass err, shortcircuitouter: true);
                    Check.That(check.Value).Equals(false);
                }
                {
                    Variables vars = new Variables();
                    vars["V1"] = "X";
                    vars["V2"] = "X";
                    vars["V3"] = "A3";
                    vars["V4"] = "A4";
                    bool? check = ConditionLists.CheckConditions(cl.List, vars, out string errlist, out ConditionLists.ErrorClass err, shortcircuitouter: true);
                    Check.That(check.Value).Equals(false);
                }
                {
                    Variables vars = new Variables();
                    vars["V1"] = "A1";
                    vars["V2"] = "X";
                    vars["V3"] = "X";
                    vars["V4"] = "A4";
                    bool? check = ConditionLists.CheckConditions(cl.List, vars, out string errlist, out ConditionLists.ErrorClass err);
                    Check.That(check.Value).Equals(false);
                }

                {
                    Variables vars = new Variables();
                    vars["V1"] = "A1";
                    vars["V2"] = "X";
                    vars["V3"] = "X";
                    vars["V4"] = "X";
                    bool? check = ConditionLists.CheckConditions(cl.List, vars, out string errlist, out ConditionLists.ErrorClass err, shortcircuitouter: true);
                    Check.That(check.Value).Equals(false);
                }

            }

            {
                List<ConditionEntry> lfields = new List<ConditionEntry>();
                lfields.Add(new ConditionEntry("V1", ConditionEntry.MatchType.Equals, "A1"));
                lfields.Add(new ConditionEntry("V2", ConditionEntry.MatchType.Equals, "A2"));

                Condition left = new Condition("E1", "A1", new Variables(new string[] { "o1", "1", "o2", "2" }), lfields, ConditionEntry.LogicalCondition.Or, ConditionEntry.LogicalCondition.NA);

                List<ConditionEntry> rfields = new List<ConditionEntry>();
                rfields.Add(new ConditionEntry("V3", ConditionEntry.MatchType.Equals, "A3"));
                rfields.Add(new ConditionEntry("V4", ConditionEntry.MatchType.Equals, "A4"));

                Condition right = new Condition("E1", "A1", new Variables(new string[] { "o1", "1", "o2", "2" }), rfields, ConditionEntry.LogicalCondition.And, ConditionEntry.LogicalCondition.And);

                ConditionLists cl = new ConditionLists();
                cl.Add(left);
                cl.Add(right);

                {
                    Variables vars = new Variables();
                    vars["V1"] = "A1";
                    vars["V2"] = "A2";
                    vars["V3"] = "A3";
                    vars["V4"] = "A4";
                    bool? check = ConditionLists.CheckConditions(cl.List, vars, out string errlist, out ConditionLists.ErrorClass err, shortcircuitouter: true);
                    Check.That(check.Value).Equals(true);
                }
                {
                    Variables vars = new Variables();
                    vars["V1"] = "A1";
                    vars["V2"] = "X";
                    vars["V3"] = "A3";
                    vars["V4"] = "A4";
                    bool? check = ConditionLists.CheckConditions(cl.List, vars, out string errlist, out ConditionLists.ErrorClass err, shortcircuitouter: true);
                    Check.That(check.Value).Equals(true);
                }
                {
                    Variables vars = new Variables();
                    vars["V1"] = "X";
                    vars["V2"] = "X";
                    vars["V3"] = "A3";
                    vars["V4"] = "A4";
                    bool? check = ConditionLists.CheckConditions(cl.List, vars, out string errlist, out ConditionLists.ErrorClass err, shortcircuitouter: true);
                    Check.That(check.Value).Equals(false);
                }
                {
                    Variables vars = new Variables();
                    vars["V1"] = "A1";
                    vars["V2"] = "A2";
                    vars["V3"] = "X";
                    vars["V4"] = "A4";
                    bool? check = ConditionLists.CheckConditions(cl.List, vars, out string errlist, out ConditionLists.ErrorClass err);
                    Check.That(check.Value).Equals(false);
                }

                {
                    Variables vars = new Variables();
                    vars["V1"] = "A1";
                    vars["V2"] = "X";
                    vars["V3"] = "X";
                    vars["V4"] = "X";
                    bool? check = ConditionLists.CheckConditions(cl.List, vars, out string errlist, out ConditionLists.ErrorClass err, shortcircuitouter: true);
                    Check.That(check.Value).Equals(false);
                }
            }

        }

    }
}
