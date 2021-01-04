/*
 * Copyright © 2017-2020 EDDiscovery development team
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

namespace BaseUtils
{
    public static class BrowserInfo
    {
        public static string GetDefault()
        {
            const string userChoice = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";
            using (Microsoft.Win32.RegistryKey userChoiceKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(userChoice))
            {
                if (userChoiceKey != null)
                {
                    object progIdValue = userChoiceKey.GetValue("Progid");
                    if (progIdValue != null)
                        return progIdValue.ToString();
                }
            }

            return null;
        }

        public static string GetPath(string defbrowser, out string args)
        {
            const string exeSuffix = ".exe";
            string path = defbrowser + @"\shell\open\command";
            args = null;

            using (Microsoft.Win32.RegistryKey pathKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(path))
            {
                if (pathKey == null)
                {
                    return null;
                }

                var command = pathKey.GetValue(null)?.ToString();

                if (command != null)
                {
                    // Trim parameters.
                    try
                    {
                        if (command.StartsWith("\"") && command.Substring(1).Contains("\""))
                        {
                            path = command.Substring(1, command.IndexOf(exeSuffix + "\"", 1) + exeSuffix.Length - 1);
                            args = command.Substring(path.Length + 3);
                            return path;
                        }
                        else if (command.Contains(exeSuffix))
                        {
                            path = command.Substring(0, command.IndexOf(exeSuffix));
                            args = command.Substring(path.Length + 1);
                            return path;
                        }

                        path = command.Replace("\"", "");
                        if (!path.EndsWith(exeSuffix))
                        {
                            path = path.Substring(0, path.LastIndexOf(exeSuffix, StringComparison.Ordinal) + exeSuffix.Length);
                            return path;
                        }
                    }
                    catch
                    {
                        // Assume the registry value is set incorrectly, or some funky browser is used which currently is unknown.
                    }
                }
            }

            return null;
        }

        public static bool LaunchBrowser(string uri)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                string browser = GetDefault();

                if (browser != null)
                {
                    string args;
                    string path = BaseUtils.BrowserInfo.GetPath(browser, out args);

                    if (path != null)
                    {
                        try
                        {
                            if (args != null && args.Contains("%1"))
                            {
                                args = args.Replace("%1", uri);
                            }
                            else
                            {
                                args = uri;
                            }

                            System.Diagnostics.ProcessStartInfo p = new System.Diagnostics.ProcessStartInfo(path, args);
                            p.UseShellExecute = false;
                            System.Diagnostics.Process.Start(p);
                            return true;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                }

                return false;
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                if (Environment.OSVersion.Version.Major >= 12 || Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    try
                    {
                        System.Diagnostics.Process.Start("open", uri);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                {
                    try
                    {
                        System.Diagnostics.Process.Start("xdg-open", uri);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        public static void FixIECompatibility(string progexe, int BrowserVer)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                int RegVal;

                // set the appropriate IE version
                if (BrowserVer >= 11)
                    RegVal = 11001;
                else if (BrowserVer == 10)
                    RegVal = 10001;
                else if (BrowserVer == 9)
                    RegVal = 9999;
                else if (BrowserVer == 8)
                    RegVal = 8888;
                else
                    RegVal = 7000;

                // set the actual key
                using (RegistryKey Key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", RegistryKeyPermissionCheck.ReadWriteSubTree))
                    if (Key.GetValue(progexe) == null)
                        Key.SetValue(progexe, RegVal, RegistryValueKind.DWord);

            }
        }

        public static string EstimateLocalHostPreferredIPV4()   // may return empty if no ip stack
        {
            try
            {
                using (System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    System.Net.IPEndPoint endPoint = socket.LocalEndPoint as System.Net.IPEndPoint;
                    return endPoint.Address.ToString();
                }
            }
            catch
            {
                return "";
            }
        }
    }
}
