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
using System.Linq;

namespace EDDiscoveryTests
{
    class ConditionTestFixtureSub
    {
        public int V20 { get; set; } = 20;
    }
    class ConditionTestFixture
    {
        public int V10 { get; set; } = 10;
        public int[] VA { get; set; } = new int[2] { 100, 200 };
        public ConditionTestFixtureSub vsub { get; set; } = new ConditionTestFixtureSub();
    }

    [TestFixture(TestOf = typeof(Eval))]
    public class ConditionTests
    {
        [Test]
        public void Conditions()
        {
            {
                var cond = new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                new ConditionEntry("V10",ConditionEntry.MatchType.NumericEquals,"10"),
                                new ConditionEntry("vsub_V20",ConditionEntry.MatchType.NumericEquals,"20"),
                                new ConditionEntry("VA[1]",ConditionEntry.MatchType.NumericEquals,"100"),
                            },
                            Condition.LogicalCondition.Or,    // inner
                            Condition.LogicalCondition.Or
                        );

                Variables indicated = new Variables();
                cond.IndicateValuesNeeded(ref indicated);
                Check.That(indicated.Count).Equals(3);
                Check.That(indicated.Exists("V10")).IsTrue();

                ConditionTestFixture ctf = new ConditionTestFixture();

                Variables expanded = new Variables();
                expanded.AddPropertiesFieldsOfClass(ctf, "", null, 5);
                Check.That(expanded.Exists("vsub_V20")).IsTrue();

                indicated.GetValuesIndicated(ctf, null, 5, new string[] { "[", "_" });
                Check.That(indicated.Exists("V10")).IsTrue();
                Check.That(indicated.GetInt("V10")).IsEqualTo(10);
                Check.That(indicated.GetInt("vsub_V20")).IsEqualTo(20);
                Check.That(indicated.GetInt("VA[1]")).IsEqualTo(100);

            }

            // test the passed condition - this one bit my bum 23/12/22
            {
                Variables varsc1 = new Variables();
                varsc1["Device"] = "Keyboard";
                varsc1["EventName"] = "RControlKey";

                {
                    ConditionLists cl = new ConditionLists();
                    cl.Add(new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                new ConditionEntry("Device",ConditionEntry.MatchType.Equals,"Keyboard"),      
                                new ConditionEntry("EventName",ConditionEntry.MatchType.Equals,"RControlKey"),     
                            },
                            Condition.LogicalCondition.And,    // inner
                            Condition.LogicalCondition.Or
                        ));

                    List<Condition> passed = new List<Condition>();
                    var ev = ConditionLists.CheckConditions(cl.List, varsc1, out string errlist, out ConditionLists.ErrorClass errclass, passed);
                    Check.That(ev).Equals(true);
                    Check.That(passed.Count).Equals(1);
                }
                {
                    ConditionLists cl = new ConditionLists();
                    cl.Add(new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                new ConditionEntry("Device",ConditionEntry.MatchType.Equals,"Keyboard1"),
                                new ConditionEntry("EventName",ConditionEntry.MatchType.Equals,"RControlKey"),
                            },
                            Condition.LogicalCondition.And,    // inner
                            Condition.LogicalCondition.Or
                        ));

                    List<Condition> passed = new List<Condition>();
                    var ev = ConditionLists.CheckConditions(cl.List, varsc1, out string errlist, out ConditionLists.ErrorClass errclass, passed);
                    Check.That(ev).Equals(false);
                    Check.That(passed.Count).Equals(0);
                }
                {
                    ConditionLists cl = new ConditionLists();
                    cl.Add(new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                new ConditionEntry("Device",ConditionEntry.MatchType.Equals,"Keyboard"),
                                new ConditionEntry("EventName",ConditionEntry.MatchType.Equals,"RControlKey1"),
                            },
                            Condition.LogicalCondition.And,    // inner
                            Condition.LogicalCondition.Or
                        ));

                    List<Condition> passed = new List<Condition>();
                    var ev = ConditionLists.CheckConditions(cl.List, varsc1, out string errlist, out ConditionLists.ErrorClass errclass, passed);
                    Check.That(ev).Equals(false);
                    Check.That(passed.Count).Equals(0);
                }
            }


            {
                Variables vars = new Variables();
                vars["IsPlanet"] = "1";
                vars["IsBig"] = "1";
                vars["IsSmall"] = "0";
                vars["V10"] = "10";
                vars["V202"] = "20.2";
                vars["Vstr1"] = "string1";
                vars["Vstr4"] = "string4";
                vars["Rings[0].outerrad"] = "20";
                vars["Other[1].outerrad"] = "20";
                vars["Other[2].outerrad"] = "40";
                vars["Mult[1].Var[1]"] = "10";
                vars["Mult[1].Var[2]"] = "20";
                vars["Mult[2].Var[1]"] = "30";
                vars["Mult[2].Var[2]"] = "40";

                Variables actionv = new Variables(new string[] { "o1", "1", "o2", "2" });

                {
                    ConditionLists cl = new ConditionLists();
                    cl.Add(new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                new ConditionEntry("IsPlanet*x",ConditionEntry.MatchType.NumericEquals,"20+20"),      // causes an error
                                new ConditionEntry("IsPlanet",ConditionEntry.MatchType.IsTrue,""),      // but this passes, and since its an OR..
                            },
                            Condition.LogicalCondition.Or,    // inner
                            Condition.LogicalCondition.Or
                        ));

                    var ev = ConditionLists.CheckConditionsEvalIterate(cl.List, vars, true);
                    Check.That(ev.Item1).Equals(true);
                    Check.That(ev.Item3[0]).Contains("Left side did not evaluate: IsPlanet*x");
                    Check.That(ev.Item3[1]).IsNull();

                }

                {
                    ConditionLists cl = new ConditionLists();
                    cl.Add(new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                new ConditionEntry("IsPlanet",ConditionEntry.MatchType.IsTrue,""),      // both passes
                                new ConditionEntry("IsSmall",ConditionEntry.MatchType.IsFalse,""),
                            },
                            Condition.LogicalCondition.And,    // inner
                            Condition.LogicalCondition.Or
                        ));

                    cl.Add(new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                new ConditionEntry("Other[Iter1].outerrad",ConditionEntry.MatchType.NumericEquals,"40"),        // does pass on Other[2]
                            },
                            Condition.LogicalCondition.Or,
                            Condition.LogicalCondition.And
                        ));

                    Variables vcopy = new Variables(vars);
                    vcopy["Iter1"] = "1";

                    var ev = ConditionLists.CheckConditionsEvalIterate(cl.List, vcopy, true);
                    Check.That(ev.Item1).Equals(true);

                }


                {
                    ConditionLists cl = new ConditionLists();
                    cl.Add(new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                new ConditionEntry("IsPlanet",ConditionEntry.MatchType.IsTrue,""),      // both passes
                                new ConditionEntry("IsSmall",ConditionEntry.MatchType.IsFalse,""),
                            },
                            Condition.LogicalCondition.And,    // inner
                            Condition.LogicalCondition.Or
                        ));

                    cl.Add(new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                new ConditionEntry("Mult[Iter1].Var[Iter2]",ConditionEntry.MatchType.NumericEquals,"40"),
                            },
                            Condition.LogicalCondition.Or,
                            Condition.LogicalCondition.And
                        ));

                    Variables vcopy = new Variables(vars);
                    vcopy["Iter1"] = "1";
                    vcopy["Iter2"] = "1";

                    var ev = ConditionLists.CheckConditionsEvalIterate(cl.List, vcopy, true);
                    Check.That(ev.Item1).Equals(true);
                }

                {
                    ConditionLists cl = new ConditionLists();
                    cl.Add(new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                new ConditionEntry("IsPlanet",ConditionEntry.MatchType.IsTrue,""),      // both passes
                                new ConditionEntry("IsSmall",ConditionEntry.MatchType.IsFalse,""),
                            },
                            Condition.LogicalCondition.And,    // inner
                            Condition.LogicalCondition.Or
                        ));

                    cl.Add(new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                new ConditionEntry("Other[Iter1].outerrad",ConditionEntry.MatchType.NumericEquals,"41"),        // does not pass anything
                            },
                            Condition.LogicalCondition.Or,
                            Condition.LogicalCondition.And
                        ));

                    Variables vcopy = new Variables(vars);
                    vcopy["Iter1"] = "1";

                    var ev = ConditionLists.CheckConditionsEvalIterate(cl.List, vcopy, true);
                    Check.That(ev.Item1).Equals(false);
                    Check.That(ev.Item3[2]).Equals("Left side did not evaluate: Other[Iter1].outerrad");
                }



                {
                    ConditionLists cl = new ConditionLists();
                    cl.Add(new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                new ConditionEntry("IsPlanet",ConditionEntry.MatchType.IsTrue,""),
                                new ConditionEntry("IsBig",ConditionEntry.MatchType.IsTrue,""), // both passes
                            },
                            Condition.LogicalCondition.And,    // inner
                            Condition.LogicalCondition.Or
                        ));

                    cl.Add(new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                new ConditionEntry("Rings[0].outerrad",ConditionEntry.MatchType.NumericEquals,"20"),    // passes, and passes this set due to OR
                                new ConditionEntry("Rings[1].outerrad",ConditionEntry.MatchType.NumericEquals,"40"),        // does not exist, should not make a difference due to Or
                            },
                            Condition.LogicalCondition.Or,
                            Condition.LogicalCondition.And
                        ));


                    var ev = ConditionLists.CheckConditionsEvalIterate(cl.List, vars, false);
                    Check.That(ev.Item1).Equals(true);
                    Check.That(ev.Item3.Where(x => x != null).Count()).Equals(0);
                }

                {
                    ConditionLists cl = new ConditionLists();
                    cl.Add(new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                        new ConditionEntry("IsPlanet",ConditionEntry.MatchType.IsTrue,""),
                                        new ConditionEntry("IsSmall",ConditionEntry.MatchType.IsTrue,""),       // fails
                            },
                            Condition.LogicalCondition.And,    // inner
                            Condition.LogicalCondition.Or
                        ));

                    cl.Add(new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                        new ConditionEntry("Rings[0].outerrad",ConditionEntry.MatchType.NumericEquals,"20"),        // should not get to this
                                        new ConditionEntry("Rings[1].outerrad",ConditionEntry.MatchType.NumericEquals,"40"),
                            },
                            Condition.LogicalCondition.Or,
                            Condition.LogicalCondition.And
                        ));


                    var ev = ConditionLists.CheckConditionsEvalIterate(cl.List, vars, false);
                    Check.That(ev.Item1).Equals(false);
                    Check.That(ev.Item3.Where(x => x != null).Count()).Equals(0);
                }

                {
                    ConditionLists cl = new ConditionLists();
                    cl.Add(new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                        new ConditionEntry("IsPlanet",ConditionEntry.MatchType.IsTrue,""), // both passes
                                        new ConditionEntry("IsSmall",ConditionEntry.MatchType.IsFalse,""),
                            },
                            Condition.LogicalCondition.And,    // inner
                            Condition.LogicalCondition.Or
                        ));

                    cl.Add(new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                        new ConditionEntry("Rings[0].outerrad",ConditionEntry.MatchType.NumericEquals,"22"),        // does not pass
                                        new ConditionEntry("Rings[1].outerrad",ConditionEntry.MatchType.NumericEquals,"40"),        // does not exist, check error returned
                            },
                            Condition.LogicalCondition.Or,
                            Condition.LogicalCondition.And
                        ));


                    var ev = ConditionLists.CheckConditionsEvalIterate(cl.List, vars, false);
                    Check.That(ev.Item1).Equals(false);
                    Check.That(ev.Item3[3].Contains("Left side did not evaluate: Rings[1].outerrad")).IsTrue();
                }

                {
                    ConditionLists cl = new ConditionLists();
                    cl.Add(new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                        new ConditionEntry("IsPlanet",ConditionEntry.MatchType.IsTrue,""),      // both passes
                                        new ConditionEntry("IsSmall",ConditionEntry.MatchType.IsFalse,""),
                            },
                            Condition.LogicalCondition.And,    // inner
                            Condition.LogicalCondition.Or
                        ));

                    cl.Add(new Condition("e", "f", new Variables(),
                            new List<ConditionEntry>
                            {
                                        new ConditionEntry("Rings[0].outerrad",ConditionEntry.MatchType.NumericEquals,"22"),        // does not pass
                                        new ConditionEntry("Rings[1].outerrad",ConditionEntry.MatchType.NumericEquals,"40"),        // now exists, passes
                            },
                            Condition.LogicalCondition.Or,
                            Condition.LogicalCondition.And
                        ));

                    vars["Rings[1].outerrad"] = "40";

                    var ev = ConditionLists.CheckConditionsEvalIterate(cl.List, vars, false);
                    Check.That(ev.Item1).Equals(true);
                    Check.That(ev.Item3.Where(x => x != null).Count()).Equals(0);
                }


                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("Rings[0].outerrad*2*V10", ConditionEntry.MatchType.NumericEquals, "40"));         // complex symbol with mult vs number

                    Condition cd = new Condition("e1", "f1", actionv, ce1, Condition.LogicalCondition.And);
                    ConditionLists cl = new ConditionLists();
                    cl.Add(cd);

                    var hashset = BaseUtils.Condition.EvalVariablesUsed(cl.List);
                    Check.That(hashset.Count).Equals(2);
                    Check.That(hashset.Contains("Rings[1].outerrad")).IsTrue();
                    Check.That(hashset.Contains("V10")).IsTrue();
                }


                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("Rings[0].outerrad*2", ConditionEntry.MatchType.NumericEquals, "40"));         // complex symbol with mult vs number
                    Condition cd = new Condition("e1", "f1", actionv, ce1, Condition.LogicalCondition.And);

                    bool? check = ConditionLists.CheckConditionsEval(new List<Condition> { cd }, vars);
                    Check.That(check.Value).Equals(true);
                }
                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("Rings[0].outerrad*2.1", ConditionEntry.MatchType.NumericEquals, "42"));         // complex symbol with mult vs number double
                    Condition cd = new Condition("e1", "f1", actionv, ce1, Condition.LogicalCondition.And);

                    bool? check = ConditionLists.CheckConditionsEval(new List<Condition> { cd }, vars);
                    Check.That(check.Value).Equals(true);
                }

                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("V10*10", ConditionEntry.MatchType.NumericEquals, "V10*10"));        // multiplication on both sides
                    Condition cd = new Condition("e1", "f1", actionv, ce1, Condition.LogicalCondition.And);

                    bool? check = ConditionLists.CheckConditionsEval(new List<Condition> { cd }, vars);
                    Check.That(check.Value).Equals(true);
                }

                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("Vstr1", ConditionEntry.MatchType.Equals, "string1"));           // var vs bare string
                    Condition cd = new Condition("e1", "f1", actionv, ce1, Condition.LogicalCondition.And);

                    bool? check = ConditionLists.CheckConditionsEval(new List<Condition> { cd }, vars);
                    Check.That(check.Value).Equals(true);
                }

                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("Vstr1", ConditionEntry.MatchType.Equals, "Vstr1"));         // var string vs var string
                    Condition cd = new Condition("e1", "f1", actionv, ce1, Condition.LogicalCondition.And);

                    bool? check = ConditionLists.CheckConditionsEval(new List<Condition> { cd }, vars);
                    Check.That(check.Value).Equals(true);
                }

                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("Vstr1", ConditionEntry.MatchType.Equals, "string1"));         // var string vs bare
                    Condition cd = new Condition("e1", "f1", actionv, ce1, Condition.LogicalCondition.And);

                    bool? check = ConditionLists.CheckConditionsEval(new List<Condition> { cd }, vars);
                    Check.That(check.Value).Equals(true);
                }

                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("Vstr1", ConditionEntry.MatchType.Equals, "\"string1\""));         // var string vs quoted string
                    Condition cd = new Condition("e1", "f1", actionv, ce1, Condition.LogicalCondition.And);

                    bool? check = ConditionLists.CheckConditionsEval(new List<Condition> { cd }, vars);
                    Check.That(check.Value).Equals(true);
                }

                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("Vstr1", ConditionEntry.MatchType.Equals, "V10"));         // var string vs number, should produce an error
                    Condition cd = new Condition("e1", "f1", actionv, ce1, Condition.LogicalCondition.And);

                    var testerrors = new List<string>();
                    bool? check = ConditionLists.CheckConditionsEval(new List<Condition> { cd }, vars, testerrors:testerrors);
                    Check.That(check.Value).Equals(false);
                    Check.That(testerrors[0].Contains("Right side is not a string: Vstr1")).IsTrue();
                }


                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("Rings[0].outerrad*2", ConditionEntry.MatchType.NumericEquals, "s40"));         // unknown var right side
                    Condition cd = new Condition("e1", "f1", actionv, ce1, Condition.LogicalCondition.And);

                    var testerrors = new List<string>();
                    bool? check = ConditionLists.CheckConditionsEval(new List<Condition> { cd }, vars, testerrors: testerrors);
                    Check.That(check.Value).Equals(false);
                    Check.That(testerrors[0].Contains("Right side did not evaluate")).IsTrue();
                }
                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("wRings[0].outerrad*2", ConditionEntry.MatchType.NumericEquals, "s40"));         // unknown var left side
                    Condition cd = new Condition("e1", "f1", actionv, ce1, Condition.LogicalCondition.And);

                    var testerrors = new List<string>();
                    bool? check = ConditionLists.CheckConditionsEval(new List<Condition> { cd }, vars, testerrors: testerrors);
                    Check.That(check.Value).Equals(false);
                    Check.That(testerrors[0].Contains("Left side did not evaluate: wRings[0].outerrad*2")).IsTrue();
                    
                }

                {
                    List<ConditionEntry> ce1 = new List<ConditionEntry>();
                    ce1.Add(new ConditionEntry("Rings[0].outerrad", ConditionEntry.MatchType.IsPresent, ""));         // var present
                    Condition cd = new Condition("e1", "f1", actionv, ce1, Condition.LogicalCondition.And);

                    bool? check = ConditionLists.CheckConditionsEval(new List<Condition> { cd }, vars);
                    Check.That(check.Value).Equals(true);
                }

            }

            {
                List<ConditionEntry> lfields = new List<ConditionEntry>();
                lfields.Add(new ConditionEntry("V1", ConditionEntry.MatchType.Equals, "A1"));
                lfields.Add(new ConditionEntry("V2", ConditionEntry.MatchType.Equals, "A2"));

                Condition left = new Condition("E1", "A1", new Variables(new string[] { "o1", "1", "o2", "2" }), lfields, Condition.LogicalCondition.Or, Condition.LogicalCondition.Or);

                List<ConditionEntry> rfields = new List<ConditionEntry>();
                rfields.Add(new ConditionEntry("V3", ConditionEntry.MatchType.Equals, "A3"));
                rfields.Add(new ConditionEntry("V4", ConditionEntry.MatchType.Equals, "A4"));

                Condition right = new Condition("E1", "A1", new Variables(new string[] { "o1", "1", "o2", "2" }), rfields, Condition.LogicalCondition.Or, Condition.LogicalCondition.And);

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

                Condition left = new Condition("E1", "A1", new Variables(new string[] { "o1", "1", "o2", "2" }), lfields, Condition.LogicalCondition.And, Condition.LogicalCondition.Or);

                List<ConditionEntry> rfields = new List<ConditionEntry>();
                rfields.Add(new ConditionEntry("V3", ConditionEntry.MatchType.Equals, "A3"));
                rfields.Add(new ConditionEntry("V4", ConditionEntry.MatchType.Equals, "A4"));

                Condition right = new Condition("E1", "A1", new Variables(new string[] { "o1", "1", "o2", "2" }), rfields, Condition.LogicalCondition.Or, Condition.LogicalCondition.And);

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

                Condition left = new Condition("E1", "A1", new Variables(new string[] { "o1", "1", "o2", "2" }), lfields, Condition.LogicalCondition.Or, Condition.LogicalCondition.Or);

                List<ConditionEntry> rfields = new List<ConditionEntry>();
                rfields.Add(new ConditionEntry("V3", ConditionEntry.MatchType.Equals, "A3"));
                rfields.Add(new ConditionEntry("V4", ConditionEntry.MatchType.Equals, "A4"));

                Condition right = new Condition("E1", "A1", new Variables(new string[] { "o1", "1", "o2", "2" }), rfields, Condition.LogicalCondition.And, Condition.LogicalCondition.And);

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
