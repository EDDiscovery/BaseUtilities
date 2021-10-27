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

        private TextReader indata;

        public CSVRead(TextReader s)
        {
            indata = s;
        }

        public void SetCSVDelimiter(bool usecomma)
        {
            Delimiter = usecomma ? "," : ";";
        }

        public enum State { EOF, Item, ItemEOL, Error };

        public State Next(out string s)
        {
            s = "";

            int c = indata.Peek();

            if (c == -1)
                return State.EOF;
            else if (c == '"')
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

            while(true)
            {
                int e = indata.Read();       // after terminator
                if (e == '\r')
                {
                    e = indata.Read();      // crlf
                    return State.ItemEOL;
                }
                else if (e == -1 || e == Delimiter[0])  // if eof, or delimiter, its fine for this item, return
                    return State.Item;
                else if (char.IsWhiteSpace((char)e))    // if whitespace, skip
                    continue;
                else
                    return State.Error;
            }
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
            public Row(System.Globalization.CultureInfo culture, System.Globalization.NumberStyles ns)
            {
                Cells = new List<string>();
                formatculture = culture;
                numberstyles = ns;
            }

            private System.Globalization.CultureInfo formatculture;
            private System.Globalization.NumberStyles numberstyles;

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
                return v != null ? v.ParseIntNull(formatculture, numberstyles) : null;
            }

            public int? GetInt(string cellname)
            {
                string v = this[cellname];
                return v != null ? v.ParseIntNull(formatculture, numberstyles) : null;
            }

            public long? GetLong(int cell)
            {
                string v = this[cell];
                return v != null ? v.ParseLongNull(formatculture, numberstyles) : null;
            }

            public long? GetLong(string cellname)
            {
                string v = this[cellname];
                return v != null ? v.ParseLongNull(formatculture, numberstyles) : null;
            }

            public double? GetDouble(int cell)
            {
                string v = this[cell];
                return v != null ? v.ParseDoubleNull(formatculture, numberstyles) : null;
            }
            public double? GetDouble(string cellname)
            {
                string v = this[cellname];
                return v != null ? v.ParseDoubleNull(formatculture, numberstyles) : null;
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
                return v != null ? v.ParseIntNull(formatculture, numberstyles) : null;
            }

            public long? NextLong()
            {
                string v = this[nextcell++];
                return v != null ? v.ParseLongNull(formatculture, numberstyles) : null;
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

        public string Delimiter { get; private set; }  = ",";


        public bool Read(string file, FileShare fs = FileShare.None, bool commadelimit = true, Action<int, Row> rowoutput = null,
                            System.Globalization.NumberStyles ns = System.Globalization.NumberStyles.None,  // if to allow thousands seperator etc
                            string noncommacountry = "sv"               // for finwen, space is the default thousands.
            )
        {
            if (!File.Exists(file))
                return false;

            try
            {
                using (Stream s = File.Open(file, FileMode.Open, FileAccess.Read, fs))
                {
                    return Read(s, commadelimit, rowoutput, ns, noncommacountry);
                }
            }
            catch
            {
                return false;
            }
        }

        public bool Read(Stream s, bool commadelimit = true, Action<int, Row> rowoutput = null,
                            System.Globalization.NumberStyles ns = System.Globalization.NumberStyles.None,  // if to allow thousands seperator etc
                            string noncommacountry = "sv"               // space is the default thousands.
            )
        {
            using (StreamReader sr = new StreamReader(s))
            {
                return Read(sr, commadelimit, rowoutput, ns, noncommacountry);
            }
        }

        // read from TR with comma/semi selection
        // optionally send rows to rowoutput instead of storing
        public bool Read(TextReader tr, 
                            bool commadelimit = true,       // true means us/uk dot and comma, else its the noncommacountry to select the format.
                            Action<int,Row> rowoutput = null ,
                            System.Globalization.NumberStyles ns = System.Globalization.NumberStyles.None,  // if to allow thousands seperator etc
                            string noncommacountry = "sv"               // space is the default thousands.
            )
        {
            Rows = new List<Row>();

            System.Globalization.CultureInfo formatculture = new System.Globalization.CultureInfo(commadelimit ? "en-US" : noncommacountry);   // select format culture based on comma

            CSVRead csv = new CSVRead(tr);
            csv.SetCSVDelimiter(commadelimit);

            Row l = new Row(formatculture,ns);
            int r = 0;

            while (true)
            {
                var state = csv.Next(out string str);
                if (state == CSVRead.State.Item)
                {
                    l.Cells.Add(str);
                }
                else if (state == CSVRead.State.ItemEOL)
                {
                    l.Cells.Add(str);
                    
                    if ( rowoutput!=null )
                        rowoutput.Invoke(r++, l);
                    else
                        Rows.Add(l);

                    l = new Row(formatculture,ns);
                }
                else if (state == CSVRead.State.EOF)
                {
                    if (l.Cells.Count > 0)
                    {
                        if ( rowoutput != null )
                            rowoutput?.Invoke(r++, l);
                        else
                            Rows.Add(l);
                    }

                    return true;
                }
                else
                    return false;       // error, end
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


