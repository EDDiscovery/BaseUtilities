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
using BaseUtils.JSON;
using NFluent;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDDiscoveryTests
{
    [TestFixture(TestOf = typeof(string))]
    public class StringTests
    {
        void DumpStr(string c)
        {
            foreach (char x in c)
                System.Diagnostics.Debug.Write(" " + (int)x + ":" + ((int)x>=32 ? x : '?'));
            System.Diagnostics.Debug.WriteLine("");
        }

        void CheckStr(string org)
        {
            DumpStr(org);
            string s1 = org.EscapeControlCharsFull();
            DumpStr(s1);
            string s2 = s1.ReplaceEscapeControlCharsFull();
            DumpStr(s2);
            Check.That(s2 == org).IsTrue();
        }

        void CheckRep(string org,string expected)
        {
            DumpStr(org);
            string s2 = org.ReplaceEscapeControlCharsFull();
            DumpStr(s2);
            Check.That(s2 == expected).IsTrue();
        }

        [Test]
        public void Escape()
        {
            CheckStr("A\tA\nA\rA\bA\fA\"A");
            CheckRep(@"A\tA\u23ABA","A\tA\u23ABA");
        }
    }
}