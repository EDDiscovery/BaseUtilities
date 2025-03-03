/*
 * Copyright (C) 2020 EDDiscovery development team
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

using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace BaseUtils
{
    static public class PythonLaunch
    {
        static private string PythonCheckSpecificInstall(string root)
        {
            RegistryKey k2 = Registry.LocalMachine.OpenSubKey(root);
            if (k2 != null)
            {
                string[] keys = k2.GetSubKeyNames();

                if (keys.Length > 0)
                {
                    try
                    {
                        Array.Sort(keys, delegate (string l, string r) { return new System.Version(l).CompareTo(new System.Version(r)); });   // assending

                        string last = @"SOFTWARE\Python\PythonCore\" + keys[keys.Length - 1] + @"\InstallPath";

                        RegistryKey k3 = Registry.LocalMachine.OpenSubKey(last);

                        if (k3 != null)
                        {
                            Object o2 = k3.GetValue("ExecutablePath");
                            if (o2 is string)
                                return o2 as string;
                        }
                    }
                    catch ( Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Py " + ex);
                    }
                }
            }

            return null;
        }

        static private string PythonCheckPyLauncher(string root)
        {
            RegistryKey k = Registry.LocalMachine.OpenSubKey(root);
            if (k != null)
            {
                Object o1 = k.GetValue("");      // default has path
                if (o1 is string)
                    return o1 as string;
            }

            return null;
        }

        static public string PythonLauncher()
        {
            string py32bit = PythonCheckPyLauncher(@"SOFTWARE\WOW6432Node\Python\PyLauncher");
            if (py32bit != null)
                 return py32bit;

            string py64bit = PythonCheckPyLauncher(@"SOFTWARE\Python\PyLauncher");
            if (py64bit != null)
                return py64bit;

            string chk1 = PythonCheckSpecificInstall(@"SOFTWARE\Python\PythonCore");
            if (chk1 != null)
                return chk1;

            string chk2 = PythonCheckSpecificInstall(@"SOFTWARE\WOW6432Node\Python\PythonCore");
            if (chk2 != null)
                return chk2;

            return null;
        }

        static public object PyExeLaunch(string pyfile, string arguments, string workindir, string runas, bool waitforexit, bool createnowindow = false, string forceversion = null)
        {
            Process p = new Process();
            p.StartInfo.FileName = "py.exe";
            p.StartInfo.Arguments = pyfile + (arguments.HasChars() ? (" " + arguments) : "");
            if (forceversion != null)
                p.StartInfo.Arguments = "-" + forceversion + " " + p.StartInfo.Arguments;
            p.StartInfo.WorkingDirectory = workindir;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.Verb = runas;
            p.StartInfo.RedirectStandardOutput = waitforexit;
            p.StartInfo.RedirectStandardError = waitforexit;
            p.StartInfo.CreateNoWindow = createnowindow;

            try
            {
                System.Diagnostics.Trace.WriteLine($"Python (Snake) Running {p.StartInfo.Arguments}");
                if (p.Start())
                {
                    if (waitforexit)
                    {
                        p.WaitForExit();
                        return new Tuple<string, string>(p.StandardOutput.ReadToEnd(), p.StandardError.ReadToEnd());
                    }
                    else
                        return p;
                }
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Trace.WriteLine($"PyExeLaunch error {ex}");
            }

            return null;
        }
    }
}

