/*
 * Copyright © 2017-2023 EDDiscovery development team
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

using System;
using System.IO;
using System.Linq;
using System.Text;

namespace BaseUtils
{
    public static class FileHelpers
    {
        public static string TryReadAllTextFromFile(string filename, Encoding encoding = null, FileShare fs = FileShare.ReadWrite)
        {
            if (File.Exists(filename))
            {
                try
                {
                    using (Stream s = File.Open(filename, FileMode.Open, FileAccess.Read, fs))
                    {
                        if (encoding == null)
                            encoding = Encoding.UTF8;

                        using (StreamReader sr = new StreamReader(s, encoding, true, 1024))
                            return sr.ReadToEnd();
                    }
                }
                catch
                {
                    return null;
                }
            }
            else
                return null;
        }

        public static string[] TryReadAllLinesFromFile(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    return File.ReadAllLines(filename, Encoding.UTF8);
                }
                catch
                {
                    return null;
                }
            }
            else
                return null;
        }

        public static bool TryAppendToFile(string filename, string content, bool makefile = false)
        {
            if (makefile == true || File.Exists(filename))
            {
                try
                {
                    File.AppendAllText(filename, content);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
                return false;
        }

        public static bool TryWriteToFile(string filename, string content)
        {
            try
            {
                File.WriteAllText(filename, content);
                return true;
            }
            catch
            {
                return false;
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

        public static bool TryCopy(string source, string file, bool overwrite)
        {
            try
            {
                File.Copy(source, file, overwrite);
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

        public static string AddSuffixToFilename(this string file, string suffix)
        {
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(file), System.IO.Path.GetFileNameWithoutExtension(file) + suffix) + System.IO.Path.GetExtension(file);
        }

        // is file not open for unshared read access - may be because it does not exist note
        public static bool IsFileAvailable(string file)
        {
            try
            {
                FileStream f = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None);
                f.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static int DeleteFiles(string rootpath, string wildcardmatch)
        {
            DirectoryInfo dir = new DirectoryInfo(rootpath);     // in order, time decending
            FileInfo[] files = dir.GetFiles(wildcardmatch).OrderByDescending(f => f.LastWriteTimeUtc).ToArray();

            int number = 0;
            foreach (FileInfo fi in files)
            {
                number += DeleteFileNoError(fi.FullName) ? 1 : 0;
            }

            return number;
        }

        public static void DeleteFiles(string rootpath, string filenamesearch, TimeSpan maxage, long MaxLogDirSizeMB)
        {
            if (Directory.Exists(rootpath))
            {
                long totsize = 0;

                DirectoryInfo dir = new DirectoryInfo(rootpath);     // in order, time decending
                FileInfo[] files = dir.GetFiles(filenamesearch).OrderByDescending(f => f.LastWriteTimeUtc).ToArray();

                foreach (FileInfo fi in files)
                {
                    DateTime time = fi.CreationTimeUtc;
                    TimeSpan fileage = DateTime.UtcNow - time;
                    totsize += fi.Length;

                    try
                    {
                        if (fileage >= maxage)
                        {
                            System.Diagnostics.Trace.WriteLine(String.Format("File {0} is older then maximum age. Removing file from Logs.", fi.Name));
                            fi.Delete();
                        }
                        else if (totsize >= MaxLogDirSizeMB * 1048576)
                        {
                            System.Diagnostics.Trace.WriteLine($"File {fi.Name} pushes total log directory size over limit of {MaxLogDirSizeMB}MB");
                            fi.Delete();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"File {fi.Name} cannot remove {ex}");
                    }
                }
            }
        }
    }
}
