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
        public GitHubClass(string server, string useragent) : base(server, useragent)
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

        // convert a branch and subpath of the repo pointed to by the URL into a download URI
        // ServerPath should be set to something like https://api.github.com/repos/EDDiscovery/EDDiscoveryData/
        // which gets converted to https://raw.githubusercontent.com/EDDiscovery/EDDiscoveryData/<branch>/<subpath>

        public string GetDownloadURI(string branch, string subpath)
        {
            return "https://raw.githubusercontent.com/" + ServerAddress.Substring(ServerAddress.IndexOf("/repos/") + 7) + branch + "/" + subpath;
        }
        public static string GetDownloadURI(string server, string branch, string subpath)
        {
            return "https://raw.githubusercontent.com/" + server.Substring(server.IndexOf("/repos/") + 7) + branch + "/" + subpath;
        }

        // given a download URI, get a download URI for another file in the same folder
        public static string GetDownloadURIFromRoot(string rootdownload, string filename)
        {
            int lastslash = rootdownload.LastIndexOf('/');
            return lastslash > 0 ? rootdownload.Substring(0, lastslash + 1) + filename : null;
        }

        // Read a whole tree, on branch, given a folder node (such as VideoFiles or ActionFiles/V1)
        // NULL if github denies or folder not found 
        // Empty list if files not there
        // local path is localpath + relative path on server from gitfolder.
        public List<RemoteFile> ReadFolderTree(System.Threading.CancellationToken cancel, string branch, string gitfolder, string localpath, int timeout = DefaultTimeout)
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
                        string serverlocalpath = entry["path"].Str();
                        string url = entry["url"].Str();
                        string sha = entry["sha"].Str();
                        long size = entry["size"].Long();

                        //System.Diagnostics.Debug.WriteLine($"Folder tree {gitfolder} {Path.Combine(gitfolder, serverlocalpath)} {url}");

                        // stupid thing does not give a download url, just a blob url (which we don't want). Synthesise one up - see format in GetDownloadURI
                        string synthurl = url.Replace("//api.github.com/repos/", "//raw.githubusercontent.com/");
                        int pos = synthurl.IndexOf("git/blobs/");
                        if (pos >= 0)
                            synthurl = synthurl.Substring(0, pos) + branch + "/" + gitfolder + "/" + serverlocalpath;

                        //System.Diagnostics.Debug.WriteLine($"... synth download url {synthurl}");

                        rf.Add(new RemoteFile(Path.GetFileName(serverlocalpath),  // local name
                                                Path.Combine(localpath, Path.GetDirectoryName(serverlocalpath)), // local path
                                                synthurl, size, sha));
                    }
                }

                return rf;
            }
            else if (response?.StatusCode == HttpStatusCode.NoContent) // no content in folder, empty file list
            {
                return new List<RemoteFile>();
            }
            else
                return null;
        }

        // Read a folder, which will come from master, given a folder node (such as VideoFiles or ActionFiles/V1)
        // NULL if github denies or folder not found 
        // Empty list if files not there
        public List<RemoteFile> ReadFolder(System.Threading.CancellationToken cancel, string gitfolder, int timeout = DefaultTimeout)
        {
            // call blocking request, cancel will result in null
            var response = BlockingRequest(cancel, Method.GET, "contents/" + Uri.EscapeDataString(gitfolder), timeout: timeout);

            if (response?.StatusCode == HttpStatusCode.OK)      // response will be null if cancelled, bug #3548
            {
                JArray ja = JArray.Parse(response.Body);
                //System.Diagnostics.Debug.WriteLine($"Read folder {gitfolder} : {ja.ToString(true)}");

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
            else if (response?.StatusCode == HttpStatusCode.NoContent) // no content in folder, empty file list
            {
                return new List<RemoteFile>();
            }

            return null;
        }

        // Blocking, download from remote git folder all files matching wildcardmatch to localdownloadfilder
        // and you can cancel it from another thread
        // optionally clean the local folder so only files downloaded are left
        // returns list of remote files downloaded
        // a local backup folder where you can get a copy of the file can be provided if the download from the internet failed
        // or null on error

        public List<RemoteFile> DownloadFolder(System.Threading.CancellationToken cancel, string localdownloadfolder, string gitfolder, string wildcardmatch,
                                bool dontuseetagdownfiles, bool synchronisefolder, int timeout = DefaultTimeout, string localbackupfolder = null)
        {
            List<RemoteFile> remotefiles = ReadFolder(cancel, gitfolder, timeout);  // will return null if cancelled or error occurred such as github denying us

            if (remotefiles != null)        // if not got an error return
            {
                // wildcard match, download only files matching this wildcard

                remotefiles = (from f in remotefiles where f.Name.WildCardMatch(wildcardmatch) select f).ToList();

                // download, may be an empty list, code copes with this.
                bool succeededall = DownloadFiles(cancel, localdownloadfolder, remotefiles, dontuseetagdownfiles, synchronisefolder, timeout, localbackupfolder);

                if (succeededall)
                {
                    return remotefiles;
                }
            }

            return null;
        }
    }
}
