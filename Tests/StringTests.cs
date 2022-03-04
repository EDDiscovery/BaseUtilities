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
using QuickJSON;
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

        [Test]
        public void ObjectExtensions_Strings()
        {
            {
                string s = ObjectExtensionsStrings.RegExWildCardToRegular("*fred");
                System.Diagnostics.Debug.WriteLine($"Pattern is {s}");
                Check.That(System.Text.RegularExpressions.Regex.IsMatch("wwwfred", s, System.Text.RegularExpressions.RegexOptions.IgnoreCase)).IsTrue();
                Check.That(System.Text.RegularExpressions.Regex.IsMatch("wwwfredwww", s, System.Text.RegularExpressions.RegexOptions.IgnoreCase)).IsFalse();
            }
            {
                string s = ObjectExtensionsStrings.RegExWildCardToRegular("*fred*");
                System.Diagnostics.Debug.WriteLine($"Pattern is {s}");
                Check.That(System.Text.RegularExpressions.Regex.IsMatch("fre", s, System.Text.RegularExpressions.RegexOptions.IgnoreCase)).IsFalse();
                Check.That(System.Text.RegularExpressions.Regex.IsMatch("fred", s, System.Text.RegularExpressions.RegexOptions.IgnoreCase)).IsTrue();
                Check.That(System.Text.RegularExpressions.Regex.IsMatch("wwwfred", s, System.Text.RegularExpressions.RegexOptions.IgnoreCase)).IsTrue();
                Check.That(System.Text.RegularExpressions.Regex.IsMatch("wwwfredwww", s, System.Text.RegularExpressions.RegexOptions.IgnoreCase)).IsTrue();
            }
            {
                string s = ObjectExtensionsStrings.RegExWildCardToRegular("*fr?d*");
                System.Diagnostics.Debug.WriteLine($"Pattern is {s}");
                Check.That(System.Text.RegularExpressions.Regex.IsMatch("fre", s, System.Text.RegularExpressions.RegexOptions.IgnoreCase)).IsFalse();
                Check.That(System.Text.RegularExpressions.Regex.IsMatch("fred", s, System.Text.RegularExpressions.RegexOptions.IgnoreCase)).IsTrue();
                Check.That(System.Text.RegularExpressions.Regex.IsMatch("frxd", s, System.Text.RegularExpressions.RegexOptions.IgnoreCase)).IsTrue();
                Check.That(System.Text.RegularExpressions.Regex.IsMatch("wwwfred", s, System.Text.RegularExpressions.RegexOptions.IgnoreCase)).IsTrue();
                Check.That(System.Text.RegularExpressions.Regex.IsMatch("wwwfredwww", s, System.Text.RegularExpressions.RegexOptions.IgnoreCase)).IsTrue();
            }
            {
                string s = ObjectExtensionsStrings.RegExWildCardToRegular("*f()r?d*");
                System.Diagnostics.Debug.WriteLine($"Pattern is {s}");
                Check.That(System.Text.RegularExpressions.Regex.IsMatch("...f()red...", s, System.Text.RegularExpressions.RegexOptions.IgnoreCase)).IsTrue();
            }
            {
                string s = ObjectExtensionsStrings.RegExWildCardToRegular("fr$(^ed");
                System.Diagnostics.Debug.WriteLine($"Pattern is {s}");
                Check.That(System.Text.RegularExpressions.Regex.IsMatch("fr$(^ed", s, System.Text.RegularExpressions.RegexOptions.IgnoreCase)).IsTrue();
                Check.That(System.Text.RegularExpressions.Regex.IsMatch("xfr$(^ed", s, System.Text.RegularExpressions.RegexOptions.IgnoreCase)).IsFalse();
                Check.That(System.Text.RegularExpressions.Regex.IsMatch("fr$(^edx", s, System.Text.RegularExpressions.RegexOptions.IgnoreCase)).IsFalse();
            }
        }


    }
}