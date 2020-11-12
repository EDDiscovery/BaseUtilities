/*
 * Copyright © 2017 EDDiscovery development team
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseUtils
{
    public static class FileHelpers
    {
        public static string TryReadAllTextFromFile(string filename)
        {
            try
            {
                return File.ReadAllText(filename, Encoding.UTF8);
            }
            catch
            {
                return null;
            }
        }

        // if erroriftoobig = false, returns top folder if above is too big for directory depth
        public static DirectoryInfo GetDirectoryAbove( this DirectoryInfo di, int above, bool errorifpastroot = false )        
        {
            while( above > 0 && di.Parent != null )
            {
                di = di.Parent;
                above--;
            }

            return (errorifpastroot && above >0 ) ? null : di;
        }

        public static bool DeleteFileNoError(string path)
        {
            try
            {
                File.Delete(path);
                return true;
            }
            catch
            {       // on purpose no error - thats the point of it
                //System.Diagnostics.Debug.WriteLine("Exception " + ex);
                return false;
            }
        }

        public static bool CreateDirectoryNoError(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                return true;
            }
            catch
            {       // on purpose no error - thats the point of it
                //System.Diagnostics.Debug.WriteLine("Exception " + ex);
                return false;
            }
        }

        public static bool VerifyWriteToDirectory(string path)
        {
            try
            {
                for( int i = 0; i < int.MaxValue; i++)
                {
                    string tempfilename = Path.Combine(path, "tempfiletotestwrite" + i.ToStringInvariant());
                    if ( !File.Exists(tempfilename))
                    {
                        File.WriteAllText(tempfilename, "Test content");        // will except if can't write
                        if (!File.Exists(tempfilename))     // check its there
                            return false;
                        File.Delete(tempfilename);      // will except if can't delete
                        return true;
                    }
                }

                return false;       // lets hope we never get here
            }
            catch
            {
                return false;       // exception, can't write
            }
        }
    }
}
