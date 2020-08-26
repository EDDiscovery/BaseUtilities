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

using System;
using System.Collections.Generic;
using System.IO;

namespace BaseUtils
{
    public class CSVWriteGrid : BaseUtils.CSVWrite
    {
        public Func<int, Object[]> GetPreHeader;// optional, return pre header items, return empty array for blank line, return null to stop pre-header

        // One shot interface - get header (new/old way) then line. Only one of these is possible

        public Func<int, Object[]> GetLineHeader;// optional, return all header items by line, return empty array for blank line, return null to stop header
        // or use the older
        public Func<int, string> GetHeader;     // optional, return header column items one by one, return null when out of header items

        public enum LineStatus { EOF, Skip, OK};
        public Func<int, LineStatus> GetLineStatus; // optional, or either EOF, Skip or OK for a line
        public Func<int,bool> VerifyLine;   // Second optional call, to screen out line again.
        public Func<int, Object[]> GetLine;// mandatory, empty array for no items on line, null to stop

        // Multi Header/Line data interface - allow for multiple more header/tables. the count of GetSetsData controls how many sets. 

        public List<Func<int, int, Object[]>> GetSetsHeader; // Return empty array, null (stop) or data
        public List<Func<int, int, Object[]>> GetSetsData;
        public List<Func<int, int, Object[]>> GetSetsFooter;
        public Func<int, int, Object[]> GetSetsPad;     // padding between sets, only used if >1 set

        // after above, post header

        public Func<int, Object[]> GetPostHeader;// optional, return post header items, return empty array for blank line, return null to stop pre-header

        public CSVWriteGrid()
        {
            GetSetsHeader = new List<Func<int, int, object[]>>();
            GetSetsFooter = new List<Func<int, int, object[]>>();
            GetSetsData = new List<Func<int, int, object[]>>();
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
                            ExportObjectList(writer, objs);
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

                    if (GetLine != null)
                    {
                        LineStatus ls = LineStatus.OK;

                        for (int r = 0; GetLineStatus == null || (ls = GetLineStatus(r)) != LineStatus.EOF; r++)
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
                    }

                    if (GetSetsData.Count > 0)
                    {
                        for (int s = 0; s < GetSetsData.Count; s++)
                        {
                            Object[] objs;

                            if (s < GetSetsHeader.Count)    // may not have header
                            {
                                for (int l = 0; (objs = GetSetsHeader[s](s, l)) != null; l++)
                                {
                                    ExportObjectList(writer, objs);
                                }
                            }

                            for (int l = 0; (objs = GetSetsData[s](s, l)) != null; l++)
                            {
                                ExportObjectList(writer, objs);
                            }

                            if (s < GetSetsFooter.Count)    // may not have footer
                            {
                                for (int l = 0; (objs = GetSetsFooter[s](s, l)) != null; l++)
                                {
                                    ExportObjectList(writer, objs);
                                }
                            }

                            if ( GetSetsPad != null && s < GetSetsData.Count - 1)   // pad between sets if present
                            {
                                for (int l = 0; (objs = GetSetsPad(s, l)) != null; l++)
                                {
                                    ExportObjectList(writer, objs);
                                }
                            }
                        }
                    }

                    if (GetPostHeader != null)
                    {
                        Object[] objs;
                        for (int l = 0; (objs = GetPostHeader(l)) != null; l++)
                        {
                            ExportObjectList(writer, objs);
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

        void ExportObjectList(StreamWriter writer, Object[] objs)
        {
            for (int i = 0; i < objs.Length; i++)
            {
                writer.Write(Format(objs[i], (i != objs.Length - 1)));
            }
            writer.WriteLine();
        }
    }
}
