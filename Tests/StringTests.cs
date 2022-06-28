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
                System.Diagnostics.Debug.Write(" " + (int)x + ":" + ((int)x >= 32 ? x : '?'));
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

        void CheckRep(string org, string expected)
        {
            DumpStr(org);
            string s2 = org.ReplaceEscapeControlCharsFull();
            DumpStr(s2);
            Check.That(s2 == expected).IsTrue();
        }

        [Test]
        public void StringCompareTest()
        {
            Check.That("Sol 4 a".CompareAlphaInt("Sol 4 a")).IsEqualTo(0);
            Check.That("Sol 4 a".CompareAlphaInt("Sol 4 b")).IsEqualTo(-1);
            Check.That("Sol 4 c".CompareAlphaInt("Sol 4 b")).IsEqualTo(1);
            Check.That("aaaaa".CompareAlphaInt("aaaab")).IsEqualTo(-1);
            Check.That("aaaac".CompareAlphaInt("aaaab")).IsEqualTo(1);
            Check.That("S 10 c".CompareAlphaInt("S 10 c")).IsEqualTo(0);
            Check.That("S 10c".CompareAlphaInt("S 10c")).IsEqualTo(0);
            Check.That("S 10c".CompareAlphaInt("S 2c")).IsEqualTo(1);
            Check.That("S 1 c".CompareAlphaInt("S 10 c")).IsEqualTo(-1);
            Check.That("S 112 c".CompareAlphaInt("S 10 c")).IsEqualTo(1);
        }

        [Test]
        public void Escape()
        {
            CheckStr("A\tA\nA\rA\bA\fA\"A");
            CheckRep(@"A\tA\u23ABA", "A\tA\u23ABA");
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


        [Test]
        public void FieldBuilder()
        {
            bool OnCrime = false;
            bool Telepresence = false;
            string Crew = "Jim";
            string info;
            info = BaseUtils.FieldBuilder.Build("Crew Member: ", Crew, "; Due to Crime", OnCrime, "; Telepresence", Telepresence);
            System.Diagnostics.Debug.WriteLine($"<{info}>");
            info = BaseUtils.FieldBuilder.Build("Crew Member: ", Crew, ";Due to Crime", OnCrime, ";Telepresence", Telepresence);
            System.Diagnostics.Debug.WriteLine($"<{info}>");
            OnCrime = true;
            info = BaseUtils.FieldBuilder.Build("Crew Member: ", Crew, ";Due to Crime", OnCrime, ";Telepresence", Telepresence);
            System.Diagnostics.Debug.WriteLine($"<{info}>");
            info = BaseUtils.FieldBuilder.Build("Crew Member: ", Crew, "; Due to Crime", OnCrime, "; Telepresence", Telepresence);
            System.Diagnostics.Debug.WriteLine($"<{info}>");
            Telepresence = true;
            info = BaseUtils.FieldBuilder.Build("Crew Member: ", Crew, "; Due to Crime", OnCrime, "; Telepresence", Telepresence);
            System.Diagnostics.Debug.WriteLine($"<{info}>");
        }

        [Test]
        public void StringSearch()
        {
            {
                var ss = new StringSearchTerms("", "");
                Check.That(ss.Terms).IsNull();
                Check.That(ss.Enabled).IsFalse();
            }
            {
                var ss = new StringSearchTerms("hello there", "");
                Check.That(ss.Terms.Length).IsEqualTo(1);
                Check.That(ss.Terms[0]).IsEqualTo("hello there");
            }
            {
                var ss = new StringSearchTerms("hello there", "station");
                Check.That(ss.Terms.Length).IsEqualTo(2);
                Check.That(ss.Terms[0]).IsEqualTo("hello there");
                Check.That(ss.Terms[1]).IsNull();
            }
            {
                var ss = new StringSearchTerms("hello station:fred there", "station");
                Check.That(ss.Terms.Length).IsEqualTo(2);
                Check.That(ss.Terms[0]).IsEqualTo("hello there");
                Check.That(ss.Terms[1]).IsEqualTo("fred");
            }
            {
                var ss = new StringSearchTerms("hello station:'fred and jim' there", "station");
                Check.That(ss.Terms.Length).IsEqualTo(2);
                Check.That(ss.Terms[0]).IsEqualTo("hello there");
                Check.That(ss.Terms[1]).IsEqualTo("fred and jim");
            }
            {
                var ss = new StringSearchTerms("hello station:", "station");
                Check.That(ss.Terms.Length).IsEqualTo(2);
                Check.That(ss.Terms[0]).IsEqualTo("hello");
                Check.That(ss.Terms[1]).IsNull();
            }
            {
                var ss = new StringSearchTerms("hello station:'fred and jim' there", "station:body");
                Check.That(ss.Terms.Length).IsEqualTo(3);
                Check.That(ss.Terms[0]).IsEqualTo("hello there");
                Check.That(ss.Terms[1]).IsEqualTo("fred and jim");
                Check.That(ss.Terms[2]).IsNull();
            }
            {
                var ss = new StringSearchTerms("hello station:'fred and jim' there body:jim", "station:body");
                Check.That(ss.Terms.Length).IsEqualTo(3);
                Check.That(ss.Terms[0]).IsEqualTo("hello there");
                Check.That(ss.Terms[1]).IsEqualTo("fred and jim");
                Check.That(ss.Terms[2]).IsEqualTo("jim");
            }
            {
                var ss = new StringSearchTerms("hello there body:jim", "station:body");
                Check.That(ss.Terms.Length).IsEqualTo(3);
                Check.That(ss.Terms[0]).IsEqualTo("hello there");
                Check.That(ss.Terms[1]).IsNull();
                Check.That(ss.Terms[2]).IsEqualTo("jim");
            }
            {
                var ss = new StringSearchTerms("body:jim", "station:body");
                Check.That(ss.Terms.Length).IsEqualTo(3);
                Check.That(ss.Terms[0]).IsNull();
                Check.That(ss.Terms[1]).IsNull();
                Check.That(ss.Terms[2]).IsEqualTo("jim");
            }
            {
                var ss = new StringSearchTerms("body:jim station:fred", "station:body");
                Check.That(ss.Terms.Length).IsEqualTo(3);
                Check.That(ss.Terms[0]).IsNull();
                Check.That(ss.Terms[1]).IsEqualTo("fred");
                Check.That(ss.Terms[2]).IsEqualTo("jim");
            }
        }

    }
}