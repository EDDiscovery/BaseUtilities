/*
* Copyright © 2018-2023 EDDiscovery development team
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
using BaseUtils;
using NFluent;
using NUnit.Framework;
using QuickJSON;

namespace EDDiscoveryTests
{
    [TestFixture(TestOf = typeof(Eval))]
    public class VariablesTests
    {
        [Test]
        public void Variables()
        {
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

                Check.That(vars.Exists("V202")).IsTrue();
                Check.That(vars.GetDouble("V202")).IsEqualTo(20.2);

            }


        }
        [Test]
        public void JSON()
        {
            {
                JToken tk = new JObject { ["Fred"] = new JArray { 1, 2, 3 } };
                System.Diagnostics.Debug.WriteLine($"{tk.ToString(true)}");
                Variables vars = new Variables();
                vars.FromJSON(tk, "json");
                JToken tkout = vars.ToJSON("json");
                Check.That(tkout.ToString(true)).IsEqualTo(tk.ToString(true));
            }
            {
                JToken tk = new JObject { ["Fred"] = 10, ["Jim"] = 20, ["Abby"] = new JObject { ["Clancy"] = 10, ["George"] = new JObject { ["david"] = 10, ["edward"] = 20 } }, ["End"] = 20 };
                System.Diagnostics.Debug.WriteLine($"{tk.ToString(false)}");
                Variables vars = new Variables();
                vars.FromJSON(tk, "json");

                JToken tkout = vars.ToJSON("json");
                Check.That(tkout.ToString(true)).IsEqualTo(tk.ToString(true));
            }
        }
    }
}
