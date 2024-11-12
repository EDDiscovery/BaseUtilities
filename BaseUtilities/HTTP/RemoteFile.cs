/*
 * Copyright © 2017-2024 EDDiscovery development team
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

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BaseUtils
{
    // holds a description not the data of a remote file.

    [System.Diagnostics.DebuggerDisplay("{Path} : {Name} {Size} {DownloadURI}")]
    public class RemoteFile
    {
        public RemoteFile(string name, string path, string uri, long size = -1, string sha = "")
        {
            Name = name;
            Path = path;
            DownloadURI = uri;
            Size = size;
            SHA = sha;
        }
        public string Name { get; private set; }        // filename (fred.txt)
        public string Path { get; private set; }        // relative path to root folder for local storage. Can be ""
        public string DownloadURI { get; private set; } // where to get the file on the web
        public long Size { get; private set; }          // set if known
        public string SHA { get; private set; }         // may not be present. If its not, DownloadNeeded will return true always

        public bool DownloadNeeded(string destFile)
        {
            if (!System.IO.File.Exists(destFile))
            {
                return true;
            }
            else if ( SHA.HasChars() )
            {
                // Calculate sha
                string localsha = BaseUtils.SHA.CalcSha1(destFile).ToLowerInvariant();

                if (localsha.Equals(SHA))
                    return false;
            }

            return true;
        }


        // given a list of these downloaded files, make sure the folders are synchronised to have only the files listed in them

        public static bool SynchroniseFolders(string localdownloadfolderroot, List<RemoteFile> files)
        {
            localdownloadfolderroot = System.IO.Path.GetFullPath(localdownloadfolderroot);        // make canonical

            HashSet<string> foldersvisited = new HashSet<string> { localdownloadfolderroot };

            foreach (var ghf in files)
            {
                if (ghf.Path.HasChars())
                {
                    string downloadfolder = System.IO.Path.Combine(localdownloadfolderroot, ghf.Path);
                    foldersvisited.Add(downloadfolder);
                }
            }

            foreach (var folder in foldersvisited)
            {
                FileInfo[] allFiles = Directory.EnumerateFiles(folder, "*.*", SearchOption.TopDirectoryOnly).Select(f => new FileInfo(f)).OrderBy(p => p.Name).ToArray();

                string subpath = folder.Substring(localdownloadfolderroot.Length);      // get sub path
                if (subpath.Length > 0)
                    subpath = subpath.Substring(1);     // remove the first /

                //System.Diagnostics.Debug.WriteLine($"HTTPcom check {folder} for files in subpath `{subpath}`");

                foreach (var file in allFiles)
                {
                    if (files.FindIndex(x => x.Path.EqualsIIC(subpath) && x.Name.EqualsIIC(file.Name)) == -1)
                    {
                        FileHelpers.DeleteFileNoError(file.FullName);
                    }
                }
            }

            return true;
        }
    }
}
