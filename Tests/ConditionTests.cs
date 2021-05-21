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
