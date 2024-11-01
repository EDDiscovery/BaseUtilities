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

namespace BaseUtils
{
    // holds a description not the data of a remote file.

    [System.Diagnostics.DebuggerDisplay("{Path} : {Name} {Size} {DownloadURL}")]
    public class RemoteFile
    {
        public RemoteFile(string name, string path, string uri, long size = -1, string sha = "")
        {
            Name = name;
            Path = path;
            DownloadURL = uri;
            Size = size;
            SHA = sha;
        }
        public string Name { get; private set; }
        public string Path { get; private set; }        // path to Name, empty if none
        public string DownloadURL { get; private set; }
        public long Size { get; private set; }
        public string SHA { get; private set; }     // may not be present. If its not, DownloadNeeded will return true always

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
    }
}
