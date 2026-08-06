/*war
 * Copyright 2026 - 2026 EDDiscovery development team
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
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BaseUtils.UnitTests
{
    public sealed class Test : Attribute
    {
        public int Priority { get; set; }

        public Test()
        {
            Priority = 0;
        }
        public Test(int p)
        {
            Priority = p;
        }
    }

    public class Check
    {
        public dynamic CheckObject;

        // hook up
        static public Action<bool, string> TestResult;
        static public Action<string> NewSection;

        public Check(dynamic o) { CheckObject = o; }

        [System.Diagnostics.DebuggerHidden()]
        public Check IsTrue()
        {
            if (!Test(CheckObject == true))
                System.Diagnostics.Debugger.Break();
            return this;
        }
        [System.Diagnostics.DebuggerHidden()]
        public Check IsFalse()
        {
            if (!Test(CheckObject == false))
                System.Diagnostics.Debugger.Break();
            return this;
        }
        [System.Diagnostics.DebuggerHidden()]
        public Check IsNull()
        {
            if (!Test(CheckObject == null))
                System.Diagnostics.Debugger.Break();
            return this;
        }
        [System.Diagnostics.DebuggerHidden()]
        public Check IsNotNull()
        {
            if (!Test(CheckObject != null))
                System.Diagnostics.Debugger.Break();
            return this;
        }
        [System.Diagnostics.DebuggerHidden()]
        public Check Is(dynamic s)
        {
            if (!Test(CheckObject != null && CheckObject.Equals(s)))      // can't be null, should use IsNull
                System.Diagnostics.Debugger.Break();
            return this;
        }
        [System.Diagnostics.DebuggerHidden()]
        public Check Contains(string s, StringComparison sc = StringComparison.InvariantCultureIgnoreCase)
        {
            if (!Test(CheckObject is string && ((string)CheckObject).IndexOf(s,sc)>=0))      // can't be null, should use IsNull
                System.Diagnostics.Debugger.Break();
            return this;
        }
        [System.Diagnostics.DebuggerHidden()]
        public new Check Equals(dynamic s)
        {
            if (!Test(CheckObject != null && CheckObject.Equals(s)))
                System.Diagnostics.Debugger.Break();
            return this;
        }

        public bool Test(bool passed)
        {
            TestResult(passed, "");
            return passed;
        }
        public static void Section(string s)
        {
            NewSection(s);
        }

        public static List<MethodInfo> GetTests(Assembly assembly)
        {
            // all test marked with [Test] and are static in this assembly

            var testswithattr = assembly.GetMethods((x) =>
            {
                if (x.GetParameters().Length == 0)
                {
                    var testattr = x.GetCustomAttributes(typeof(Test), false) as Test[];
                    return testattr.Length == 1 && testattr[0].Priority>=0;
                }
                else
                    return false;
            });

            testswithattr.Sort(delegate (MethodInfo l, MethodInfo r)
                {
                    var testattrleft = l.GetCustomAttributes(typeof(Test), false) as Test[];
                    var testattrright = r.GetCustomAttributes(typeof(Test), false) as Test[];
                    return testattrright[0].Priority.CompareTo(testattrleft[0].Priority);       // higher first
                }
                );


            return testswithattr;
        }
    }

    public static class CheckerHelpers
    {
        public static void CheckSection(string s)
        {
            BaseUtils.UnitTests.Check.Section(s);
        }
        [System.Diagnostics.DebuggerHidden()]
        public static void Check(dynamic x)
        {
            var chk = new Check(x);
            if (!chk.Test(x == true))
                System.Diagnostics.Debugger.Break();
        }
        [System.Diagnostics.DebuggerHidden()]
        public static Check CheckThat(object o)
        {
            return new Check(o);
        }

        // reads a @ json quote file and sends completed lines without decoration to debugit
        public static bool ReadAttedJsonFile(string file, Func<string,string,bool> debugit)
        {
            using (StreamReader sr = new StreamReader(file))         // read directly from file..
            {
                string line;
                string laststatement = "";
                string idline = "";

                while ((line = sr.ReadLine()) != null)
                {
                    if (line.HasChars())
                    {
                        if (line.StartsWith("@\""))
                        {
                            if (line.EndsWith(";"))
                            {
                                line = line.Replace("\"\"", "\"");
                                line = line.ReplaceIfEndsWith("\";", "");
                                line = line.ReplaceIfStartsWith("@\"", "");
                                laststatement += line;
                                if (!debugit(laststatement,idline))
                                    break;
                                laststatement = "";
                                idline = "";
                            }
                            else if (line.EndsWith("+"))
                            {
                                line = line.Replace("\"\"", "\"");
                                line = line.ReplaceIfEndsWith("\" +", "");
                                line = line.ReplaceIfStartsWith("@\"", "");
                                laststatement += line;
                            }
                        }
                        else if ( line.StartsWith(">"))
                        {
                            idline = line.Substring(1);
                        }
                    }

                }
            }

            return true;
        }
    }
}

