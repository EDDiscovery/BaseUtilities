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
using System.Drawing;

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
            foreach( KeyValuePair<string,string> v in test )
            {

            }
        }
    }
}
