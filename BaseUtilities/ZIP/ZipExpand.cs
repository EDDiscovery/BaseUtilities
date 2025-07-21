/*
 * Copyright © 2019 EDDiscovery development team
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
 *
 */

using System;
using System.IO;
using System.Net;
using System.Text;
using System.IO.Compression;

namespace BaseUtils.ZIP
{
    public class Zip
    {
        private string zipfilepath;
        private string expandpath;

        public Zip(string zipfile, string expandtopath)
        {
            this.zipfilepath = zipfile;
            this.expandpath = expandtopath;
        }

        public bool Unzip(string partialpath)
        {
            try
            {
                using (var zipfile = ZipFile.Open(zipfilepath, ZipArchiveMode.Read))
                {
                    //foreach (var x in zipfile.Entries) System.Diagnostics.Debug.WriteLine("Zip file " + x.FullName);

                    partialpath = partialpath.Replace('/', '\\');       // zip files use back slashes and urls are forward slashes.

                    var file = zipfile.GetEntry(partialpath);       // find file

                    if (file != null)
                    {
                        using (var zipstrm = file.Open())
                        {
                            string saveto = Path.Combine(expandpath, partialpath);

                            using (var filestream = new FileStream(saveto, FileMode.Create))
                            {
                                zipstrm.CopyTo(filestream);
                                filestream.Close();
                                zipstrm.Close();
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Zip File unzip exception " + e);
            }

            return false;
        }
    }
}
