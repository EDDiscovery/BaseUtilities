/*
 * Copyright © 2016-2020 EDDiscovery development team
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

using Newtonsoft.Json.Linq;
using System.Linq;

namespace BaseUtils
{
    public class GitHubRelease
    {
        JObject jo;

        public GitHubRelease(JObject jo)
        {
            this.jo = jo;
        }

        // For debugging it!

        public GitHubRelease(string name, string tag, string url, string created, string descr, string exe, string msi, string zip) 
        {
            jo = new JObject();
            jo["name"] = name;
            jo["tag_name"] = tag;
            jo["html_url"] = url;
            jo["created_at"] = created;
            jo["body"] = descr;

            JArray aa = new JArray();
            JObject a1 = new JObject();
            JObject a2 = new JObject();
            JObject a3 = new JObject();

            aa.Add(a1);
            aa.Add(a2);
            aa.Add(a3);

            a1["name"] = exe;
            a1["browser_download_url"] = "http://www.bbc.co.uk";
            a2["name"] = msi;
            a2["browser_download_url"] = "http://google.co.uk";
            a3["name"] = zip;
            a3["browser_download_url"] = "http://news.bbc.co.uk";

            jo["assets"] = aa;
            System.Diagnostics.Debug.WriteLine("Jo is " + jo.ToString(Newtonsoft.Json.Formatting.Indented));
        }

        public string ReleaseName { get { return  jo["name"].Str(); } }

        public string ReleaseVersion {
            get
            {
                string str = jo["tag_name"].Str();
                int indexof = str.IndexOfAny("0123456789".ToCharArray());
                if (indexof >= 0)
                    return str.Substring(indexof);
                else
                    return "";
            }
        }

        public string HtmlURL { get { return jo["html_url"].Str(); } }
        public string Time { get { return jo["created_at"].Str(); } }

        public string Description { get { return jo["body"].Str(); } }


        public string ExeInstallerLink
        {
            get
            {
                var asset = jo["assets"]?.FirstOrDefault(j => j["name"].Str().ToLowerInvariant().EndsWith(".exe"));
                if (asset != null)
                {
                    string url = asset["browser_download_url"].Str();
                    return url;
                }
                return null;
            }
        }
        public string MsiInstallerLink
        {
            get
            {
                var asset = jo["assets"]?.FirstOrDefault(j => j["name"].Str().ToLowerInvariant().EndsWith(".msi"));
                if (asset != null)
                {
                    string url = asset["browser_download_url"].Str();
                    return url;
                }
                return null;
            }
        }
        public string PortableInstallerLink
        {
            get
            {
                var asset = jo["assets"]?.FirstOrDefault(j => j["name"].Str().ToLowerInvariant().EndsWith(".zip") && j["name"].Str().ToLowerInvariant().Contains("portable"));
                if (asset != null)
                {
                    string url = asset["browser_download_url"].Str();
                    return url;
                }
                return null;
            }
        }

    }
}
