/*
 * Copyright © 2016-2024 EDDiscovery development team
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

using QuickJSON;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BaseUtils
{
    [System.Diagnostics.DebuggerDisplay("{ReleaseName} {ReleaseVersion}")]
    public class GitHubRelease
    {
        JObject jo;

        public GitHubRelease(JObject jo)
        {
            this.jo = jo;
        }

        public string ReleaseName { get { return  jo["name"].Str(); } }

        public string ReleaseVersion {
            get
            {
                string str = jo["tag_name"].Str();
                int indexof = str.IndexOfAny("0123456789".ToCharArray());       // find first number
                if (indexof >= 0)
                    return str.Substring(indexof).Replace("_",".");     // added in case using _ for separators
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

        static public GitHubRelease CheckForNewInstaller(string url, string currentVersion, bool force = false)
        {
            try
            {
                GitHubClass github = new GitHubClass(url);

                GitHubRelease rel = github.GetLatestRelease();

                if (rel != null)
                {
                    var releaseVersion = rel.ReleaseVersion;

                    Version v1 = new Version(releaseVersion);
                    Version v2 = new Version(currentVersion);

                    if (force || v1.CompareTo(v2) > 0) // Test if newer installer exists:
                    {
                        return rel;
                    }
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        static public Task CheckForNewInstallerAsync(string url, string version, Action<GitHubRelease> callbackinthread, bool force = false)
        {
            return Task.Factory.StartNew(() =>
            {
#if DEBUG
                // for debugging it
                //callbackinthread?.Invoke(new BaseUtils.GitHubRelease("Test","10.9.1.9","http", "2018-09-25T09:17:02Z", "Test","1.exe","2.msi","portable.zip"));
#endif 
                GitHubRelease rel = CheckForNewInstaller(url,version,force);

                if (rel != null)
                    callbackinthread?.Invoke(rel);
            });
        }
    }
}
