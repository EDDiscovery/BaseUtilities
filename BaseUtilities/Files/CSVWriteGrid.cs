/*
 * Copyright © 2016 EDDiscovery development team
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
using System.IO;
using System.Windows.Forms;

namespace BaseUtils
{
    public class CSVWriteGrid : BaseUtils.CSVWrite
    {
        public Func<int, Object[]> GetPreHeader;// optional, return pre header items, return empty array for blank line, return null to stop pre-header

        public Func<int, Object[]> GetLineHeader;// optional, return all header items by line, return empty array for blank line, return null to stop header
        // or use the older
        public Func<int, string> GetHeader;     // optional, return header column items one by one, return null when out of header items

        public enum LineStatus { EOF, Skip, OK};
        public Func<int, LineStatus> GetLineStatus; // optional, or either EOF, Skip or OK for a line

        public Func<int,bool> VerifyLine;   // Second optional call, to screen out line again.

        public Func<int, Object[]> GetLine;// mandatory, empty array for no items on line, null to stop

        public Func<int, Object[]> GetPostHeader;// optional, return post header items, return empty array for blank line, return null to stop pre-header

        public CSVWriteGrid()
        {
        }

        public bool WriteCSV(string filename)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filename))
                {
                    if (GetPreHeader != null)
                    {
                        Object[] objs;
                        for (int l = 0; (objs = GetPreHeader(l)) != null; l++)
                        {
                            for (int i = 0; i < objs.Length; i++)
                            {
                                writer.Write(Format(objs[i], (i != objs.Length - 1)));
                            }
                            writer.WriteLine();
                        }
                    }

                    if (GetLineHeader != null)
                    {
                        Object[] objs;
                        for (int l = 0; (objs = GetLineHeader(l)) != null; l++)
                        {
                            for (int i = 0; i < objs.Length; i++)
                            {
                                writer.Write(Format(objs[i], (i != objs.Length - 1)));
                            }
                            writer.WriteLine();
                        }
                    }

                    if (GetHeader != null)
                    {
                        string t;
                        int cols;
                        for (cols = 0; (t = GetHeader(cols)) != null; cols++)
                        {
                            if (cols > 0)
                                writer.Write(delimiter);
                            writer.Write(Format(t, false));
                        }

                        if (cols > 0)
                            writer.WriteLine();
                    }

                    LineStatus ls = LineStatus.OK;

                    for( int r = 0; GetLineStatus == null || (ls = GetLineStatus(r)) != LineStatus.EOF; r++ )
                    {
                        if (ls != LineStatus.Skip)
                        {
                            if (VerifyLine == null || VerifyLine(r))
                            {
                                Object[] objs = GetLine(r);

                                if (objs == null)
                                    break;
                                else if (objs.Length > 0)
                                {
                                    for (int i = 0; i < objs.Length; i++)
                                    {
                                        writer.Write(Format(objs[i], (i != objs.Length - 1)));
                                    }
                                }
                                writer.WriteLine();
                            }
                        }
                    }

                    if (GetPostHeader != null)
                    {
                        Object[] objs;
                        for (int l = 0; (objs = GetPostHeader(l)) != null; l++)
                        {
                            for (int i = 0; i < objs.Length; i++)
                            {
                                writer.Write(Format(objs[i], (i != objs.Length - 1)));
                            }
                            writer.WriteLine();
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CSV Write failed " + ex.ToString());
                return false;
            }
        }
 
    }
}
