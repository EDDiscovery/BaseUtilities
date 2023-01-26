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
using System.Collections.Generic;
using System.IO;

namespace BaseUtils
{
    public class CSVRead
    {
        public string Delimiter { get; set; } = ",";        // only a single char supported

        public CSVRead(TextReader s)
        {
            indata = s;
        }

        public enum State { EOF, Item, ItemEOL, Error };

        public void ReadString(int c, out string s)        // read string part, on quote
        {
            s = "";

            if (c == '"')
            {
                indata.Read();  // read off quote

                while ((c = indata.Read()) != -1 && (c != '"' || indata.Peek() == '"'))
                {
                    //System.Diagnostics.Debug.WriteLine("Char " + c);
                    if (c == '"')  // ""
                    {
                        indata.Read();
                        s += (char)c;
                    }
                    else if (c == '\r')         // \r\n
                    {
                        if ( indata.Peek() == '\n')
                            indata.Read();
                        s += "\r\n";
                    }
                    else if (c == '\n')
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
                while ((c = indata.Peek()) != -1 && c != Delimiter[0] && c!= '\r' && c !='\n')  // if not eof, not delimiter, not \r or \n
                {
                    //System.Diagnostics.Debug.WriteLine("NChar " + c);
                    c = indata.Read();
                    s += (char)c;
                }
            }
        }

        public State ReadDelimiter(char delimit, out int lastch)
        {
            while (true)
            {
                lastch = indata.Read();       // after terminator
               // System.Diagnostics.Debug.WriteLine($".. Delimit {(int)lastch}");

                if ( lastch == '\n')
                {
                    return State.ItemEOL;       
                }
                else if (lastch == '\r' )       // presuming \r\n
                {
                    if ( indata.Peek() == '\n') 
                        indata.Read();      // read off \n

                    return State.ItemEOL;
                }
                else if (lastch == -1 || lastch == delimit || delimit == '!')  // if eof, or delimiter, or special delimit of space mean accept anything, its fine for this item, return
                {
                    return State.Item;
                }
                else if (char.IsWhiteSpace((char)lastch))    // if whitespace, skip
                {
                    continue;
                }
                else
                    return State.Error;
            }
        }

        public State Next(out string s)
        {
            s = "";

            int c = indata.Peek();

            if (c == -1)
                return State.EOF;

            ReadString(c, out s);
            //System.Diagnostics.Debug.WriteLine($"String {s}");

            return ReadDelimiter(Delimiter[0], out int unused);
        }

        public static char FindDelimiter(string input)
        {
            int comma = input.IndexOf(',');
            int semicolon = input.IndexOf(';');
            int quote = input.IndexOf('"');
            int tab = input.IndexOf('\t');

            if (quote > 0)
            {
                CSVRead csv = new CSVRead(new StringReader(input.Substring(quote+1)));        // goto quote
                csv.ReadString('"',out string unusedstr); // read it to find delimiter
                var state = csv.ReadDelimiter('!', out int delimit);
                if (state == State.Item && (delimit == ',' || delimit == ';' || delimit == '\t'))        // return it - if at eol, won't have an delimit, so impossible to tell
                    return (char)delimit;
            }

            if (tab != -1)
                return '\t';
            else if ((comma == -1 && semicolon >= 0) || (comma>=0 && semicolon>=0 && semicolon<comma))
                return ';';
            else
                return ',';
        }

        private TextReader indata;
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

        public List<Row> Rows { get; set; }

        public List<Row> RowsExcludingHeaderRow { get { return (Rows != null && Rows.Count > 1) ? Rows.GetRange(1, Rows.Count - 1) : null; } }

        public Row this[int row]
        {
            get
            {
                return (row < Rows.Count) ? Rows[row] : null;
            }
        }

        public string Delimiter { get; private set; }  = ",";
        public System.Globalization.CultureInfo FormatCulture { get; set; } = new System.Globalization.CultureInfo("en-US");
        public System.Globalization.NumberStyles NumberStyle { get; set; } = System.Globalization.NumberStyles.None;

        public CSVFile() { }
        public CSVFile(string delimiter) 
        {
            SetDelimiter(delimiter);
        }

        public void SetDelimiter(string delimiter)
        {
            Delimiter = delimiter;
            if (Delimiter != ";")
                FormatCulture = new System.Globalization.CultureInfo("en-US");
            else
                FormatCulture = new System.Globalization.CultureInfo("sv");
        }

        public bool Read(string file, FileShare fs = FileShare.ReadWrite, Action<int, Row> rowoutput = null)
        {
            if (!File.Exists(file))
                return false;

            try
            {
                //using (Stream s = File.Open(file, FileMode.Open, FileAccess.Read, fs))
                //{
                //    using (StreamReader sr = new StreamReader(s))
                //    {
                //        while (true)
                //        {
                //            int v = sr.Read();
                //            System.Diagnostics.Debug.WriteLine($"{v} {(char)v}");

                //            if (v == -1)
                //                break;
                //        }
                //    }
                //}

                using (Stream s = File.Open(file, FileMode.Open, FileAccess.Read, fs))
                {
                    return Read(s, rowoutput);
                }
            }
            catch
            {
                return false;
            }
        }

        public bool ReadString(string str, Action<int, Row> rowoutput = null)
        {
            //foreach (var s in str) System.Diagnostics.Debug.WriteLine($"{(int)s} {s}");

            using (StringReader sr = new StringReader(str))
            {
                return Read(sr, rowoutput);
            }
        }

        public bool Read(Stream s, Action<int, Row> rowoutput = null)
        {
            using (StreamReader sr = new StreamReader(s))
            {
                return Read(sr, rowoutput);
            }
        }

        // read from TR with delimiter, format culture. If format culture = null, use it based on delimiter (semicomma = sv, else en-US)
        // optionally send rows to rowoutput instead of storing
        public bool Read(TextReader tr, Action<int,Row> rowoutput = null)
        {
            Rows = new List<Row>();

            CSVRead csv = new CSVRead(tr);
            csv.Delimiter = Delimiter;

            Row l = new Row(FormatCulture,NumberStyle);
            int r = 0;

            while (true)
            {
                var state = csv.Next(out string str);
                //System.Diagnostics.Debug.WriteLine($"CVS Item {state} {str}");

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

                    l = new Row(FormatCulture,NumberStyle);
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


