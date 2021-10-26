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
    public class CSVRead
    {
        public string Delimiter { get; private set; } = ",";

        private StreamReader indata;

        public CSVRead(StreamReader s)
        {
            indata = s;
        }

        public void SetCSVDelimiter(bool usecomma)
        {
            Delimiter = usecomma ? "," : ";";
        }

        public enum State { EOF, Item, ItemEOL };

        public State Next(out string s)
        {
            s = "";

            if (indata.EndOfStream)
                return State.EOF;

            int c;

            if (indata.Peek() == '"')
            {
                indata.Read();

                while ((c = indata.Read()) != -1 && (c != '"' || indata.Peek() == '"'))
                {
                    //System.Diagnostics.Debug.WriteLine("Char " + c);
                    if (c == '"')  // ""
                    {
                        indata.Read();
                        s += (char)c;
                    }
                    else if (c == '\r')
                    {
                        s += "\r\n";
                    }
                    else
                    {
                        s += (char)c;
                    }
                }
            }
            else
            {
                while ((c = indata.Peek()) != -1 && c != Delimiter[0] && c != '\r')
                {
                    //System.Diagnostics.Debug.WriteLine("NChar " + c);
                    s += (char)c;
                    indata.Read();
                }
            }

            int e = indata.Read();
            //System.Diagnostics.Debug.WriteLine("SChar " + e);

            if (e == '\r')
            {
                e = indata.Read();
                return State.ItemEOL;
            }
            else
                return State.Item;
        }

        void RemoveSpaces()
        {
            while ((char)indata.Peek() == ' ')
                indata.Read();
        }
    }

    [System.Diagnostics.DebuggerDisplay("CSV File Rows {Rows.Count}")]
    public class CSVFile
    {
        public class Row
        {
            public Row()
            {
                Cells = new List<string>();
            }

            public List<string> Cells;
            public int nextcell = 0;        // next cell to Next()

            static public int CellNameToIndex(string s)
            {
                s = s.ToLowerInvariant();
                int col = int.MaxValue;
                if (s.Length >= 1)      // later, cope with An, Bn etc.
                    col = s[0] - 'a';

                if (s.Length >= 2)
                    col = (col + 1) * 26 + (s[1] - 'a');

                return col;
            }

            public string this[int cell]    // root of all..
            {
                get
                {
                    if (cell>=0 && cell < Cells.Count)
                    {
                        nextcell = cell + 1;
                        return Cells[cell];
                    }
                    else
                        return null;
                }
            }

            public string this[string cellname]
            {
                get
                {
                    return this[CellNameToIndex(cellname)];
                }
            }

            public string this[string cellname, int offset]
            {
                get
                {
                    return this[CellNameToIndex(cellname) + offset];
                }
            }

            public int? GetInt(int cell)
            {
                string v = this[cell];
                return v != null ? v.InvariantParseIntNull() : null;
            }

            public int? GetInt(string cellname)
            {
                string v = this[cellname];
                return v != null ? v.InvariantParseIntNull() : null;
            }

            public long? GetLong(int cell)
            {
                string v = this[cell];
                return v != null ? v.InvariantParseLongNull() : null;
            }

            public long? GetLong(string cellname)
            {
                string v = this[cellname];
                return v != null ? v.InvariantParseLongNull() : null;
            }

            public void SetPosition(int cell)           // set position to N
            {
                nextcell = cell;
            }

            public void SetPosition(string cellname)    // set position to name
            {
                nextcell = CellNameToIndex(cellname);
            }

            public string Next()
            {
                return this[nextcell++];
            }

            public int? NextInt()
            {
                string v = this[nextcell++];
                return v != null ? v.InvariantParseIntNull() : null;
            }

            public long? NextLong()
            {
                string v = this[nextcell++];
                return v != null ? v.InvariantParseLongNull() : null;
            }
        }

        public List<Row> Rows;

        public List<Row> RowsExcludingHeaderRow { get { return (Rows != null && Rows.Count > 1) ? Rows.GetRange(1, Rows.Count - 1) : null; } }

        public Row this[int row]
        {
            get
            {
                return (row < Rows.Count) ? Rows[row] : null;
            }
        }

        public bool Read(string file, FileShare fs = FileShare.None, bool commadelimit = true )
        {
            Rows = new List<Row>();

            if (!File.Exists(file))
                return false;

            try
            {
                using (Stream s = File.Open(file,FileMode.Open,FileAccess.Read,fs))
                {
                    using (StreamReader sr = new StreamReader(s))
                    {
                        CSVRead csv = new CSVRead(sr);
                        csv.SetCSVDelimiter(commadelimit);

                        CSVRead.State st;

                        Row l = new Row();

                        string str;
                        while ((st = csv.Next(out str)) != CSVRead.State.EOF)
                        {
                            l.Cells.Add(str);

                            if (st == CSVRead.State.ItemEOL)
                            {
                                Rows.Add(l);
                                l = new Row();
                            }
                        }

                        if (l.Cells.Count > 0)
                            Rows.Add(l);

                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public Tuple<int, int> Find(string s, StringComparison cmp = StringComparison.InvariantCulture, bool trim = false)
        {
            for (int r = 0; r < Rows.Count; r++)
            {
                for (int c = 0; c < Rows[r].Cells.Count; c++)
                {
                    string cv = Rows[r].Cells[c];

                    if (trim)
                        cv = cv.Trim();

                    if (cv.Equals(s, cmp))
                        return new Tuple<int, int>(r, c);
                }
            }

            return null;
        }

        public int FindInRow(int r, string s, StringComparison cmp = StringComparison.InvariantCulture, bool trim = false)
        {
            if (r < Rows.Count)
            {
                for (int c = 0; c < Rows[r].Cells.Count; c++)
                {
                    string cv = Rows[r].Cells[c];

                    if (trim)
                        cv = cv.Trim();

                    if (cv.Equals(s, cmp))
                        return r;
                }
            }

            return -1;
        }

        public int FindInColumn(string cellname, string s, StringComparison cmp = StringComparison.InvariantCulture, bool trim = false)
        {
            int cell = Row.CellNameToIndex(cellname);
            return cell != int.MaxValue ? FindInColumn(cell, s, cmp) : -1;
        }

        public int FindInColumn(int c, string s, StringComparison cmp = StringComparison.InvariantCulture, bool trim = false)
        {
            for (int r = 0; r < Rows.Count; r++)
            {
                string cv = Rows[r].Cells[c];
                if (cv != null)
                {
                    if (trim)
                        cv = cv.Trim();

                    if (cv.Equals(s, cmp))
                        return r;
                }
            }

            return -1;
        }
    }
}


