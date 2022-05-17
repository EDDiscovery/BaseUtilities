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
    public class CollectionTests
    {
        [Test]
        public void Collections()
        {
            StackOfDictionaries<string, string> sd = new StackOfDictionaries<string, string>();

            foreach (var v in sd)
            {
                System.Diagnostics.Debug.WriteLine("0.Value " + v);
            }

            sd["l0one"] = "l0one";
            sd["l0two"] = "l0two";
            sd["v1"] = "v1at0";

            Check.That(sd["l0one"] == "l0one").IsTrue();
            Check.That(sd["l0two"] == "l0two").IsTrue();
            Check.That(sd["v1"] == "v1at0").IsTrue();
            Check.That(sd.Count == 3).IsTrue();
            Check.That(sd.ContainsKey("v1")).IsTrue();


            sd.Push();
            sd["l1one"] = "l1one";
            sd["l1two"] = "l1two";
            sd["v1"] = "v1at1";

            Check.That(sd["l1one"] == "l1one").IsTrue();
            Check.That(sd["l1two"] == "l1two").IsTrue();
            Check.That(sd["v1"] == "v1at1").IsTrue();
            Check.That(sd.Count == 6).IsTrue();
            Check.That(sd.ContainsKey("l0one")).IsTrue();

            foreach (var v in sd)
            {
                System.Diagnostics.Debug.WriteLine("0.Value " + v);
            }

            sd.Pop();

            Check.That(sd["l0one"] == "l0one").IsTrue();
            Check.That(sd["l0two"] == "l0two").IsTrue();
            Check.That(sd["v1"] == "v1at0").IsTrue();
            Check.That(sd.Count == 3).IsTrue();


            System.Diagnostics.Debug.WriteLine("ALL COLLECTION TESTS FINISHED");

            Dictionary<string, string> test = new Dictionary<string, string>();
            test["on"] = "oN";
            foreach (KeyValuePair<string, string> v in test)
            {

            }
        }

        [Test]
        public void GenerationalDictionary()
        {
            GenerationalDictionary<int, uint> gd = new GenerationalDictionary<int, uint>();

            Random rnd = new Random(1001);

            const int generations = 1004;
            const int depth = 10000;
            const int modulo = 2;

            int[] genskip = new int[depth];
            for (int i = 0; i < depth; i++)
                genskip[i] = rnd.Next(23) + modulo + 1;     // need to be bigger than modulo

            for (uint g = 0; g < generations; g++)
            {
                gd.NextGeneration();

                for (int i = 0; i < depth; i++)
                {
                    if (g % genskip[i] == modulo)
                    {
                        //  System.Diagnostics.Debug.WriteLine("{0} Add {1}", (g+1), i);
                        gd[i] = g;
                    }
                }
            }

            Stopwatch sw = new Stopwatch();

            sw.Start();

            long time = sw.ElapsedMilliseconds;
            //File.WriteAllText(@"c:\code\time.txt", "Time taken " + time);

            for (uint g = 0; g < generations; g++)
            {
                var dict = gd.Get(g + 1);
                //  System.Diagnostics.Debug.WriteLine("At gen {0} get {1} {2}", g + 1, dict.Count, string.Join(",",dict.Values));
                for (int i = 0; i < depth; i++)
                {
                    bool present = g % genskip[i] == modulo;
                    if (present)
                        Check.That(dict[i]).Equals(g);
                    else if (g < modulo)
                        Check.That(dict.ContainsKey(i)).IsFalse();
                    else
                        Check.That(dict[i]).IsNotEqualTo(g);
                }

                //foreach( var kvp in dict)  {         System.Diagnostics.Debug.WriteLine("{0} {1}={2}", g+1, kvp.Key, kvp.Value);    }
                //System.Diagnostics.Debug.WriteLine("");
            }

            for (uint g = 0; g < generations; g++)
            {
                var values = gd.GetValues(g + 1);
                for (int i = 0; i < depth; i++)
                {
                    bool present = g % genskip[i] == modulo;
                    if (present)
                        Check.That(values[i]).Equals(g);
                    else if (g < modulo)
                        Check.That(values.Contains((uint)i)).IsFalse();
                    else
                        Check.That(values[i]).IsNotEqualTo(g);
                }

                //foreach( var kvp in dict)  {         System.Diagnostics.Debug.WriteLine("{0} {1}={2}", g+1, kvp.Key, kvp.Value);    }
                //System.Diagnostics.Debug.WriteLine("");
            }

            //1004x10000 = release 3035

        }

        [Test]
        public void GenerationalDictionary2()
        {
            GenerationalDictionary<string, string > gd = new GenerationalDictionary<string,string>();

            gd["one"] = "g0-one";
            gd["two"] = "g0-two";
            gd.NextGeneration();
            gd["two"] = "g1-two";
            gd.NextGeneration();
            gd["one"] = "g2-one";
            gd.NextGeneration();
            gd["three"] = "g3-three";

            var onekey = gd.GetHistoryOfKey("one");
            Check.That(onekey.Count).Equals(2);
            var twokey = gd.GetHistoryOfKey("two");
            Check.That(twokey.Count).Equals(2);
            var oneg2 = gd.GetHistoryOfKey(2, "one");
            Check.That(oneg2.Count).Equals(2);
            var oneg1 = gd.GetHistoryOfKey(1, "one");
            Check.That(oneg1.Count).Equals(1);
            var threeg1 = gd.GetHistoryOfKey(1, "three");
            Check.That(threeg1.Count).Equals(0);
            var threeg10 = gd.GetHistoryOfKey(10, "three");
            Check.That(threeg10.Count).Equals(1);

        }
    }
}
