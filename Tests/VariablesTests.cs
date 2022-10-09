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

    }
}
