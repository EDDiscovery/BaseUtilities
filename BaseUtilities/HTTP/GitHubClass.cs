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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace BaseUtils
{
    public class GitHubClass : HttpCom
    {
        public GitHubClass(string server) : base(server)
        {
        }
        public GitHubClass(string server,string useragent) : base(server,useragent)
        {
        }

        public JArray GetAllReleases(int reqmax)
        {
            // always returns response
            var response = RequestGet("releases?per_page=" + reqmax.ToString());
            if (response.StatusCode == HttpStatusCode.OK)
            {
                JArray ja = JArray.ParseThrowCommaEOL(response.Body);
                return ja;
            }
            else
                return null;
        }

        public GitHubRelease GetLatestRelease()
        {
            // always returns response
            var response = RequestGet("releases/latest");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                JObject jo = JObject.Parse(response.Body);

                if (jo != null)
                {
                    GitHubRelease rel = new GitHubRelease(jo);
                    return rel;
                }
                else
                    return null;
            }
            else
                return null;
        }


        // Read a whole tree, on branch, given a folder node.
        // NULL if folder not found or not an array return.  Empty list if files not there
        public List<RemoteFile> ReadFolderTree(System.Threading.CancellationToken cancel, string branch, string gitfolder, int timeout = DefaultTimeout)
        {
            var response = BlockingRequest(cancel, Method.GET, "git/trees/" + branch + ":" + Uri.EscapeDataString(gitfolder) + "?recursive=1", timeout: timeout);

            if (response?.StatusCode == HttpStatusCode.OK)      // response will be null if cancelled, bug #3548
            {
                JObject jr = JObject.Parse(response.Body);

                List<RemoteFile> rf = new List<RemoteFile>();

                foreach (var entry in jr["tree"])
                {
                    string type = entry["type"].Str();
                    if (type == "blob")
                    {
                        string path = entry["path"].Str();
                        string url = entry["url"].Str();
                        string sha = entry["sha"].Str();
                        long size = entry["size"].Long();

                        //System.Diagnostics.Debug.WriteLine($"Folder tree {gitfolder} {Path.Combine(gitfolder, path)} {url}");

                        // stupid thing does not give a download url, just a blob url (which I don't want). Synthesise one up
                        string synthurl = url.Replace("//api.github.com/repos/", "//raw.githubusercontent.com/");
                        int pos = synthurl.IndexOf("git/blobs/");
                        if (pos >= 0)
                            synthurl = synthurl.Substring(0, pos) + branch + "/" + gitfolder + "/" + path;
                       
                        //System.Diagnostics.Debug.WriteLine($"... synth download url {synthurl}");

                        rf.Add(new RemoteFile(Path.GetFileName(path), Path.GetDirectoryName(path), synthurl, size, sha));
                    }
                }

                return rf;
            }
            else
                return null;
        }


        // NULL if folder not found or not an array return.  Empty list if files not there
        public List<RemoteFile> ReadFolder(System.Threading.CancellationToken cancel, string gitfolder, int timeout = DefaultTimeout)
        {
            // call blocking request, cancel will result in null
            var response = BlockingRequest(cancel, Method.GET, "contents/" + Uri.EscapeDataString(gitfolder), timeout:timeout);

            if (response?.StatusCode == HttpStatusCode.OK)      // response will be null if cancelled, bug #3548
            {
                JArray ja = JArray.Parse(response.Body);
                System.Diagnostics.Debug.WriteLine($"Read folder {gitfolder} : {ja.ToString(true)}");

                if (ja != null)
                {
                    List<RemoteFile> files = new List<RemoteFile>();
                    foreach (JObject jo in ja)
                    {
                        string type = jo["type"].Str();
                        if (type != "dir")
                        {
                            string name = jo["name"].Str();
                            string url = jo["download_url"].Str();
                            //System.Diagnostics.Debug.WriteLine($"Read folder {name}->{url}");
                            RemoteFile file = new RemoteFile(name, "", url, jo["size"].Int(), jo["sha"].Str());
                            files.Add(file);
                        }
                    }
                    return files;
                }
            }

            return null;
        }

        // download in a task from remote git folder all files matching wildcardmatch to localdownloadfilder
        // you can await on this.
        // and you can cancel it
        // optionally clean the local folder
        public System.Threading.Tasks.Task<bool> DownloadFolderInTask(System.Threading.CancellationToken cancel, string localdownloadfolder, string gitfolder, string wildcardmatch,
                                                               bool dontuseetagdownfiles, bool synchronisefolder, int timeout = DefaultTimeout)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                return DownloadFolder(cancel, localdownloadfolder, gitfolder, wildcardmatch, dontuseetagdownfiles, synchronisefolder, timeout);
            });
        }

        // Blocking, download from remote git folder all files matching wildcardmatch to localdownloadfilder
        // and you can cancel it from another thread
        // optionally clean the local folder
        public bool DownloadFolder(System.Threading.CancellationToken cancel, string localdownloadfolder, string gitfolder, string wildcardmatch,
                                bool dontuseetagdownfiles, bool synchronisefolder, int timeout = DefaultTimeout)
        {
            List<RemoteFile> remotefiles = ReadFolder(cancel, gitfolder, timeout);  // will return null if cancelled

            if (remotefiles != null)
            {
                remotefiles = (from f in remotefiles where f.Name.WildCardMatch(wildcardmatch) select f).ToList();      // wildcard match
                return DownloadFiles(cancel, localdownloadfolder, remotefiles, dontuseetagdownfiles, synchronisefolder, timeout);
            }
            else
                return false;
        }

        // Blocking, download from remote git folder all files matching the list of files in the list to localdownloadfilder
        // and you can cancel it from another thread
        // optionally clean the local folder
        public bool DownloadFolder(System.Threading.CancellationToken cancel, string localdownloadfolder, string gitfolder, List<string> matches,
                                                bool dontuseetagdownfiles, bool synchronisefolder, int timeout = DefaultTimeout)
        {
            var remotefiles = ReadFolder(cancel, gitfolder, timeout);   // will return null if cancelled

            if (remotefiles != null)
            {
                remotefiles = (from f in remotefiles where matches.Contains(f.Name, StringComparer.InvariantCultureIgnoreCase) select f).ToList();
                return DownloadFiles(cancel, localdownloadfolder, remotefiles, dontuseetagdownfiles, synchronisefolder, timeout);
            }
            else
                return false;
        }
    }
}
