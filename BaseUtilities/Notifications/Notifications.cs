/*
 * Copyright 2022-2025 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License") {get;set;} you may not use this
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BaseUtils
{
    public static class Notifications
    {
        [System.Diagnostics.DebuggerDisplay("`{Caption}` `{Text}`")]
        public class NotificationMessages
        {
            public string Text { get; set; }
            public string Caption { get; set; }
        }

        [System.Diagnostics.DebuggerDisplay("{StartUTC}..{EndUTC} {VersionMin}..{VersionMax} {AlwaysShow} {Type} c{Conditions.Count} n{NotificationsByLanguage.Count}")]
        public class Notification
        {
            public DateTime StartUTC {get;set; }         // StartUTC="2025-12-01T00:00:00Z"
            public DateTime EndUTC {get;set;}            // EndUTC="2026-03-01T23:00:00Z"
            public string VersionMin {get;set; }         // VersionMin="19.0.11.0"
            public string VersionMax {get;set; }         // VersionMax="19.0.11.0"
            public bool AlwaysShow {get;set;}            // AlwaysShow = 1/0 : set to always show, else a condition must pass
            public Dictionary<string, string[]> Conditions {get;set;}     // list of CONDITIONxxx = "string,string"
            public string Type {get;set;}               // Type="Popup" (pop out a window), "Log" (push it to log), "New" (put it on the New Feature Button)
            public float PointSize {get;set;}           // PointSize="N"
            public bool HighLight {get;set;}            // HighLight="Yes"  for "Log" do we show it in highlight colour
            
            public Dictionary<string, NotificationMessages> NotificationsByLanguage {get;set;}       // List of Notifications, contains Text and Caption. Keyed by "Lang"="en" in the Body Section
            // <Body Caption="Odyssey Update" Lang="en">
            //Test Notification Present Enabled JMKV2
            //</Body>

            public NotificationMessages Select(string lang)
            {
                return NotificationsByLanguage.ContainsKey(lang) ? NotificationsByLanguage[lang] : (NotificationsByLanguage.ContainsKey("en") ? NotificationsByLanguage["en"] : null);
            }

            public string Key
            {
                get
                {
                    string key = "<" + StartUTC.ToStringZulu() + ":" + EndUTC.ToStringZulu() + ":" + AlwaysShow.ToStringIntValue() + ":";
                    foreach (var x in Conditions)
                        key += "(" + x.Key + "=" + string.Join(";", x.Value) + ")";
                    key += ">";
                    return key;
                }
            }
        }

        // read XML file and compile notifications
        static public List<Notification> ReadNotificationsFile(string notfile, string notificationsection)
        {
            List<Notification> notes = new List<Notification>();

            try
            {       // protect against rouge xml
                XElement items = XElement.Load(notfile);
                if (items.Name != "Items")
                    return notes;

                foreach (XElement toplevel in items.Elements())
                {
                    //System.Diagnostics.Debug.WriteLine("Item " + toplevel.Name);

                    if (toplevel.Name == notificationsection)
                    {
                        foreach (XElement entry in toplevel.Elements())
                        {
                            try
                            {
                                // protect each notification from each other..

                                Notification n = new Notification();
                                n.StartUTC = DateTime.Parse(entry.Attribute("StartUTC").Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AdjustToUniversal);
                                n.EndUTC = entry.Attribute("EndUTC") != null ? DateTime.Parse(entry.Attribute("EndUTC").Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AdjustToUniversal) : DateTime.MaxValue;

                                n.Type = entry.Attribute("Type").Value;

                                n.PointSize = entry.Attribute("PointSize") != null ? entry.Attribute("PointSize").Value.InvariantParseFloat(12) : -1;
                                n.HighLight = entry.Attribute("Highlight") != null && entry.Attribute("Highlight").Value == "Yes";

                                if (entry.Attribute("VersionMax") != null)
                                    n.VersionMax = entry.Attribute("VersionMax").Value;

                                if (entry.Attribute("VersionMin") != null)
                                    n.VersionMin = entry.Attribute("VersionMin").Value;

                                if (entry.Attribute("AlwaysShow") != null)      // always show has been added - its either this now (feb 25) or a condition passes
                                    n.AlwaysShow = entry.Attribute("AlwaysShow").Value == "1";

                                n.Conditions = new Dictionary<string, string[]>();

                                foreach( XAttribute at in entry.Attributes())
                                {
                                    //System.Diagnostics.Debug.WriteLine($"{at.Name.LocalName} = {at.Value}");
                                    if ( at.Name.LocalName.StartsWith("Condition"))
                                    {
                                        n.Conditions[at.Name.LocalName] = at.Value.Split(",");
                                    }
                                }

                                n.NotificationsByLanguage = new Dictionary<string, NotificationMessages>();

                                foreach (XElement body in entry.Elements())
                                {
                                    string lang = body.Attribute("Lang").Value;
                                    n.NotificationsByLanguage[lang] = new NotificationMessages() { Text = body.Value, Caption = body.Attribute("Caption").Value };

                                    // System.Diagnostics.Debug.WriteLine("    " + body.Attribute("Lang").Value + " Body " + body.Value);
                                }

                                notes.Add(n);
                               // System.Diagnostics.Debug.WriteLine($"Notification {n.StartUTC}..{n.EndUTC} {n.EntryType} {n.AlwaysShow}");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine("Notification XML File " + notfile + " inner exception " + ex.Message);
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Notification XML File " + notfile + " failed to read " + ex.Message);
            }

            return notes;
        }

        static public Task CheckForNewNotifications( bool checkgithub,
                                                    string githubfoldername,
                                                    string localnotificationfolder,
                                                    string githuburl,
                                                    string notificationsection,
                                                    Action<List<Notification>> callbackinthread)
        {
            return Task.Factory.StartNew(() =>
            {
                if (checkgithub)      // if download from github first..
                {
                    BaseUtils.GitHubClass github = new BaseUtils.GitHubClass(githuburl);
                    github.DownloadFolder(new System.Threading.CancellationToken(), localnotificationfolder, githubfoldername, "*.xml", true, true);
                }

                // always go thru what we have in that folder.. 
                FileInfo[] allfiles = Directory.EnumerateFiles(localnotificationfolder, "*.xml", SearchOption.TopDirectoryOnly).Select(f => new System.IO.FileInfo(f)).OrderByDescending(p => p.LastWriteTime).ToArray();

                List<Notification> nlist = new List<Notification>();

                foreach (FileInfo f in allfiles)       // process all files found..
                {
                    var list = ReadNotificationsFile(f.FullName, notificationsection);
                    nlist.AddRange(list);
                }

                if (nlist.Count > 0)                     // if there are any, indicate..
                {
                    nlist.Sort(delegate (Notification left, Notification right)     // in order, oldest first
                    {
                        return left.StartUTC.CompareTo(right.StartUTC);
                    });

                    callbackinthread?.Invoke(nlist);
                }
            });
        }
    }

}
