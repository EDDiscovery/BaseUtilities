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
using System;
using System.Collections.Generic;


namespace EDDiscoveryTests
{
    [TestFixture(TestOf = typeof(Eval))]
    public class FunctionStringExpanderTests
    {
        [Test]
        public void BasicTests()
        {
            {
                Variables v = new Variables();
                v["One"] = "1";
                v["Two"] = "20";

                Functions fn = new Functions(v, null);

                string test = "Fred %(One)";
                var er = fn.ExpandStringFull(test, out string res, 0);
                Check.That(er).IsEqualTo(Functions.ExpandResult.Expansion);
                Check.That(res).IsEqualTo("Fred 1");

            }
            {
                Variables v = new Variables();
                v["json.One"] = "1";
                v["json.Two"] = "20";

                Functions fn = new Functions(v, null);

                string test = "Fred %tojson(json,0)";
                var er = fn.ExpandStringFull(test, out string res, 0);
                Check.That(er).IsEqualTo(Functions.ExpandResult.Expansion);
                Check.That(res).IsEqualTo("Fred {\"One\":1,\"Two\":20}");

            }
            {
                Variables v = new Variables();
                v["json.One"] = "1";
                v["json.Two[1]"] = "201";
                v["json.Two[2]"] = "202";

                Functions fn = new Functions(v, null);

                string test = "Fred %tojson(json,0)";
                var er = fn.ExpandStringFull(test, out string res, 0);
                Check.That(er).IsEqualTo(Functions.ExpandResult.Expansion);
                Check.That(res).IsEqualTo("Fred {\"One\":1,\"Two\":[201,202]}");

            }
        }

    }
}
