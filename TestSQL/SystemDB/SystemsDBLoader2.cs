﻿/*
 * Copyright 2023 - 2023 EDDiscovery development team
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

using QuickJSON;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace EliteDangerousCore.DB
{
    // EDSM Store - db.edsmid contains edsmid, and db.info is null
    // Spansh Store - db.edsmid contains the system address, and db.info is non null

    public partial class SystemsDB
    {
        // Star system loader

        public class Loader2
        {
            // create - postfix allows a different table set to be created
            // maxblocksize - write back when reached this
            // gridids - null or array of allowed gridid
            // debugoutputfile - write file of loaded systems
            public Loader2(string ptablepostfix, int pmaxblocksize, bool[] gridids, bool poverlapped, string debugoutputfile = null)
            {
                tablepostfix = ptablepostfix;
                maxblocksize = pmaxblocksize;
                grididallowed = gridids;
                overlapped = poverlapped;

                nextsectorid = SystemsDatabase.Instance.GetSectorIDNext();
                maxdate = SystemsDatabase.Instance.GetLastRecordTimeUTC();

                if (debugoutputfile != null)
                    debugfile = new StreamWriter(debugoutputfile);

                sectorcache = new Dictionary<Tuple<long, string>, long>();

                SystemsDatabase.Instance.DBRead(db =>
                {
                    using (var selectSectorCmd = db.CreateSelect("Sectors" + tablepostfix, "id,gridid,name"))
                    {
                        using (DbDataReader reader = selectSectorCmd.ExecuteReader())       // Read all sectors into the cache
                        {
                            while (reader.Read())
                            {
                                Tuple<long, string> key = new Tuple<long, string>((long)reader[1], (string)reader[2]);
                                sectorcache.Add(key, (long)reader[0]);
                            }
                        };
                    };
                });
            }

            public void Finish()
            {
                // do not need to store back sector table - new sectors are made as they are created below

                if (debugfile != null)
                    debugfile.Close();

                SystemsDatabase.Instance.SetSectorIDNext(nextsectorid);
                SystemsDatabase.Instance.SetLastRecordTimeUTC(maxdate);
            }

            public long ParseJSONFile(string filename, Func<bool> cancelRequested, Action<string> reportProgress)
            {
                // if the filename ends in .gz, then decompress it on the fly
                if (filename.EndsWith("gz"))
                {
                    using (FileStream originalFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                    {
                        using (GZipStream gz = new GZipStream(originalFileStream, CompressionMode.Decompress))
                        {
                            using (StreamReader sr = new StreamReader(gz))
                            {
                                return ParseJSONTextReader(sr, cancelRequested, reportProgress);
                            }
                        }
                    }
                }
                else
                {
                    using (StreamReader sr = new StreamReader(filename))         // read directly from file..
                        return ParseJSONTextReader(sr, cancelRequested, reportProgress);
                }
            }

            public long ParseJSONString(string data, Func<bool> cancelRequested, Action<string> reportProgress)
            {
                using (StringReader sr = new StringReader(data))         // read directly from file..
                    return ParseJSONTextReader(sr, cancelRequested, reportProgress);
            }

            // parse this textreader, allowing cancelling, reporting progress
            public long ParseJSONTextReader(TextReader textreader, Func<bool> cancelRequested, Action<string> reportProgress)
            {
                long updates = 0;

                System.Diagnostics.Trace.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS", true)} System DB L2 store start");

                int recordstostore = 0;
                int wbno = 0;
                WriteBlock curwb = new WriteBlock(++wbno);
                WriteBlock prevwb = null;           // if one is in flight, its recorded here

                var parser = new QuickJSON.Utils.StringParserQuickTextReader(textreader, 512);
                char[] charbuf = new char[256];
                char[] charbuf2 = new char[256];

                bool stop = false;
                char nextchar = '!';    // any value
                string maxdatetimestr = "2000-05-00 19:54:30+00";

                while (!stop)     // while not cancel, and got another char..
                {
                    stop = cancelRequested() || (nextchar = parser.GetChar()) == char.MinValue;

                    if (!stop && nextchar == '{')       // if not stopping, and object (ignore anything between objects)
                    {
                        string starname = null;
                        double? x = null, y = null, z = null;
                        ulong? id = null, systemaddress = null;
                        int? startype = null;
                        bool spansh = false;

                        // skip to next non white, do not remove anything after, while not at EOL
                        while ((nextchar = parser.GetNextNonSpaceChar(false)) != char.MinValue)
                        {
                            if (nextchar == '"')            // property name
                            {
                                int textfieldlen = parser.NextQuotedString('"', charbuf, replaceescape: true, skipafter:true);     // get prop name and skip spaces

                                // next char must be a colon, then skip further
                                if (parser.IsCharMoveOn(':',true))
                                {
                                    if (charbuf.Equals("name", textfieldlen))
                                    {
                                        if (parser.IsCharMoveOn('"', false) && (textfieldlen = parser.NextQuotedString('"', charbuf, replaceescape: true, skipafter: true)) >= 0)
                                            starname = new string(charbuf, 0, textfieldlen);
                                        else
                                            break;
                                    }
                                    else if (charbuf.Equals("id", textfieldlen))     // edsm name
                                    {
                                        int len = parser.NextCharBlock(charbuf, (ch) => char.IsDigit(ch), skipafter:true);
                                        if (len > 0)
                                            id = new string(charbuf, 0, len).InvariantParseULongNull();
                                        else
                                            break;
                                        spansh = false;
                                    }
                                    else if (charbuf.Equals("id64", textfieldlen))   // edsm and spansh name
                                    {
                                        int len = parser.NextCharBlock(charbuf, (ch) => char.IsDigit(ch), skipafter:true);
                                        if (len > 0)
                                            systemaddress = new string(charbuf, 0, len).InvariantParseULongNull();
                                        else
                                            break;
                                    }
                                    else if (charbuf.Equals("date", textfieldlen))   // edsm name
                                    {
                                        if (parser.IsCharMoveOn('"', false) && (textfieldlen = parser.NextQuotedString('"', charbuf, replaceescape: true, skipafter: true)) >= 0)
                                        {
                                            string dt = new string(charbuf, 0, textfieldlen);
                                            if (dt.CompareTo(maxdatetimestr) > 0)      // by doing a alpha compare, its quicker than a try parse
                                                maxdatetimestr = dt;
                                            spansh = false;
                                        }
                                        else
                                            break;
                                    }
                                    else if (charbuf.Equals("updateTime", textfieldlen)) // spansh name
                                    {
                                        if (parser.IsCharMoveOn('"', false) && (textfieldlen = parser.NextQuotedString('"', charbuf, replaceescape: true, skipafter: true)) >= 0)
                                        {
                                            string dt = new string(charbuf, 0, textfieldlen);
                                            if (dt.CompareTo(maxdatetimestr) > 0)      // by doing a alpha compare, its quicker than a try parse
                                                maxdatetimestr = dt;
                                            spansh = true;
                                        }
                                        else
                                            break;
                                    }
                                    else if (charbuf.Equals("coords", textfieldlen)) // both
                                    {
                                        if (parser.IsCharMoveOn('{', true))       // must have {, and move on
                                        {
                                            while (parser.IsCharMoveOn('"'))        // if quote, we are another property
                                            {
                                                textfieldlen = parser.NextQuotedString('"', charbuf, replaceescape: true, skipafter: true);     // get prop name

                                                if (textfieldlen == 1 && parser.IsCharMoveOn(':', true))       // 1 char, followed by a colon, move on
                                                {
                                                    // get a block for the number and skip
                                                    int len = parser.NextCharBlock(charbuf2, (ch) => ch.IsDigitOrDotOrNegSign(),skipafter:true);
                                                    if (len > 0)
                                                    {
                                                        if (charbuf[0] == 'x')
                                                            x = new string(charbuf2, 0, len).InvariantParseDoubleNull();
                                                        else if (charbuf[0] == 'y')
                                                            y = new string(charbuf2, 0, len).InvariantParseDoubleNull();
                                                        else                // anything double null, if its not right, it will be a null
                                                            z = new string(charbuf2, 0, len).InvariantParseDoubleNull();
                                                    }

                                                    parser.IsCharMoveOn(',', true);            // move past comma and space it
                                                }
                                                else
                                                    break;      // error
                                            }

                                            if (!x.HasValue || !y.HasValue || !z.HasValue)
                                                break;

                                            if (!parser.IsCharMoveOn('}', true))          // if not on }, error
                                                break;
                                        }
                                        else
                                            break;
                                    }
                                    else if (charbuf.Equals("mainStar", textfieldlen)) // spansh
                                    {
                                        if (parser.IsCharMoveOn('"', false) && (textfieldlen = parser.NextQuotedString('"', charbuf, replaceescape: true, skipafter: true)) >= 0)
                                        {
                                            string sname = new string(charbuf, 0, textfieldlen);
                                            if (spanshtoedstar.TryGetValue(sname, out EDStar value))
                                            {
                                                startype = (int)value;
                                                spansh = true;
                                            }
                                            else
                                                System.Diagnostics.Debug.WriteLine($"DB read of spansh unknown star type {sname}");
                                        }
                                        else
                                            break;
                                    }
                                    else if (charbuf.Equals("needsPermit", textfieldlen)) // spansh
                                    {
                                        // we don't store this
                                        bool _ = parser.IsStringMoveOn("true") ? true : !parser.IsStringMoveOn("false");    
                                    }
                                    else
                                    {
                                        var token = parser.JNextValue(charbuf, false);      // don't know what it is, so use standard token reader
                                        System.Diagnostics.Debug.WriteLine($"Read DB unknown field in json import value {token}");
                                    }

                                    parser.IsCharMoveOn(',', true);            // move past comma and space it
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else if (nextchar == '}')      // end of object
                            {
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (spansh)                 // hold in id the record identifier
                        {
                            if (startype == null)      // for spansh, we always set startype non null, so that we know the db.edsmid is really the system address
                                startype = (int)EDStar.Unknown;
                            id = systemaddress;
                        }

                        // if a valid star is found - check the essential fields

                        if (starname != null && x.HasValue && y.HasValue && z.HasValue && id.HasValue)
                        {
                           // System.Diagnostics.Debug.WriteLine($"Read {systemaddress} {id} {starname} {x} {y} {z}");

                            if (true)
                            {
                                int xi = (int)(x * SystemClass.XYZScalar);
                                int yi = (int)(y * SystemClass.XYZScalar);
                                int zi = (int)(z * SystemClass.XYZScalar);
                                int gridid = GridId.Id128(xi, zi);

                                if (grididallowed == null || (grididallowed.Length > gridid && grididallowed[gridid]))    // allows a null or small grid
                                {
                                    var classifier = new EliteNameClassifier(starname);

                                    var skey = new Tuple<long, string>(gridid, classifier.SectorName);

                                    if (!sectorcache.TryGetValue(skey, out long sectorid))     // if we dont have a sector with this grid id/name pair
                                    {
                                        // System.Diagnostics.Debug.WriteLine($"In {wb.wbno} write sector {wb.sectorinsertcmd}");
                                        if (curwb.sectorinsertcmd.Length > 0)
                                            curwb.sectorinsertcmd.Append(',');

                                        sectorid = nextsectorid++;
                                        curwb.sectorinsertcmd.Append('(');                            // add (id,gridid,name) to sector insert string
                                        curwb.sectorinsertcmd.Append(sectorid.ToStringInvariant());
                                        curwb.sectorinsertcmd.Append(',');
                                        curwb.sectorinsertcmd.Append(gridid.ToStringInvariant());
                                        curwb.sectorinsertcmd.Append(",'");
                                        curwb.sectorinsertcmd.Append(classifier.SectorName.Replace("'", "''"));
                                        curwb.sectorinsertcmd.Append("') ");

                                        sectorcache.Add(skey, sectorid);        // add to sector cache
                                    }

                                    if (classifier.IsNamed)
                                    {
                                        if (curwb.nameinsertcmd.Length > 0)
                                            curwb.nameinsertcmd.Append(',');

                                        curwb.nameinsertcmd.Append('(');                            // add (id,name) to names insert string
                                        curwb.nameinsertcmd.Append(id);
                                        curwb.nameinsertcmd.Append(",'");
                                        curwb.nameinsertcmd.Append(classifier.StarName.Replace("'", "''"));
                                        curwb.nameinsertcmd.Append("') ");
                                        classifier.NameIdNumeric = id.Value;                      // the name becomes the id of the entry
                                    }

                                    if (curwb.systeminsertcmd.Length > 0)
                                        curwb.systeminsertcmd.Append(",");

                                    curwb.systeminsertcmd.Append('(');                            // add (id,sectorid,nameid,x,y,z,info) to systems insert string
                                    curwb.systeminsertcmd.Append(id);                           // locale independent, because its just a decimal with no N formatting
                                    curwb.systeminsertcmd.Append(',');
                                    curwb.systeminsertcmd.Append(sectorid);
                                    curwb.systeminsertcmd.Append(',');
                                    curwb.systeminsertcmd.Append(classifier.ID);
                                    curwb.systeminsertcmd.Append(',');
                                    curwb.systeminsertcmd.Append(xi);
                                    curwb.systeminsertcmd.Append(',');
                                    curwb.systeminsertcmd.Append(yi);
                                    curwb.systeminsertcmd.Append(',');
                                    curwb.systeminsertcmd.Append(zi);
                                    curwb.systeminsertcmd.Append(",");
                                    if (startype.HasValue)
                                        curwb.systeminsertcmd.Append(startype);
                                    else
                                        curwb.systeminsertcmd.Append("NULL");

                                    curwb.systeminsertcmd.Append(")");

                                    if (debugfile != null)
                                        debugfile.WriteLine($"{id} {starname} {x} {y} {z} SID:{sectorid} GID:{gridid}");

                                    recordstostore++;
                                }
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Loader Error failed to make a star");
                        }
                    }

                    // between objects, we see if we need to store, or we are stopping..

                    if (recordstostore >= maxblocksize || stop)
                    {
                        if (prevwb != null)     // if we have a pending one, we wait for it to finish, as we don't overlap them 
                        {
                            System.Diagnostics.Trace.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS")} ready with next block");

                            uint readyforblocktime = (uint)Environment.TickCount;
                            
                            var metric = SystemsDatabase.Instance.DBWait(prevwb.sqlop, 5000);        // wait for job, and get its metrics

                            uint waitfortime = readyforblocktime - metric.Item2;                    // if job completed before read of json, it will be positive
                            int dbfinishedbeforeread = (int)waitfortime;                            // experiments with changing block size, but it does not really work well.
        
                            prevwb.sqlop = null;

                            System.Diagnostics.Trace.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS")} ready jobfinished {metric.Item2} delta {dbfinishedbeforeread}  ************ next BS {maxblocksize}");
                        }

                        if (recordstostore > 0)
                        {
                            System.Diagnostics.Trace.WriteLine($"\r\n{BaseUtils.AppTicks.TickCountLap("SDBS")} Begin block size {recordstostore} {updates} block {curwb.wbno}");

                            Write(curwb);       // creat a no wait db write - need to do this in a function because we are about to change curwb

                            if (overlapped)
                            {
                                prevwb = curwb;         // set previous and we will check next time to see if we need to pend
                            }
                            else
                            {
                                SystemsDatabase.Instance.DBWait(curwb.sqlop, 5000);    // else not overlapped, finish it now
                            }
                        }

                        curwb = new WriteBlock(++wbno); // make a new one

                        updates += recordstostore;
                    //    reportProgress?.Invoke($"Star database updated {recordstostore:N0} total so far {(updates):N0}");
                        recordstostore = 0;
                    }
                }

                if (prevwb != null)     // if we have an outstanding one
                {
                    System.Diagnostics.Trace.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS")} Wait for last write {prevwb.wbno} to complete");
                    SystemsDatabase.Instance.DBWait(prevwb.sqlop, 5000);
                    System.Diagnostics.Trace.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS")} last block {prevwb.wbno} complete");
                    prevwb.sqlop = null;
                }

                reportProgress?.Invoke($"Star database updated {updates:N0}");
                System.Diagnostics.Trace.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS")} System DB L2 finish {updates}");

                // update max date - from string
                System.DateTime.TryParse(maxdatetimestr, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out maxdate);

                return updates;
            }

            // we need this in a func. The function executes the c.Write in a thread, so we can't let c change
            void Write(WriteBlock c)
            {
                c.sqlop = SystemsDatabase.Instance.DBWriteNoWait(db => c.Write(db, tablepostfix), jobname: "SystemDBLoad");
            }

            private class WriteBlock
            {
                public Object sqlop;
                public int wbno;
                public StringBuilder sectorinsertcmd = new StringBuilder(100000);
                public StringBuilder nameinsertcmd = new StringBuilder(300000);
                public StringBuilder systeminsertcmd = new StringBuilder(32000000);

                public WriteBlock(int n)
                {
                    wbno = n;
                }

                public void Write(SQLiteConnectionSystem db, string tablepostfix)
                {
                    using (var txn = db.BeginTransaction())
                    {
                        if (sectorinsertcmd.Length > 0)
                        {
                            string cmdt = "INSERT INTO Sectors" + tablepostfix + " (id,gridid,name) VALUES " + sectorinsertcmd.ToString();
                            // we should never enter the same sector twice due to the sector caching.. so INSERT INTO is ok
                            using (var cmd = db.CreateCommand(cmdt, txn))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }

                        if (nameinsertcmd.Length > 0)
                        {
                            // we may double insert Names if we are processing the same item again.  We do not cache names.
                            // if we have a duplicate, we update the name because it will be a name update

                            using (var cmd = db.CreateCommand("INSERT OR REPLACE INTO Names" + tablepostfix + " (id,Name) VALUES " + nameinsertcmd.ToString(), txn))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }

                        if (systeminsertcmd.Length > 0)     // if we stopped, right after storing, we may have no systems to store. Or the json is empty etc.
                        {
                            // experimented with using (var cmd = db.CreateCommand("INSERT INTO SystemTable" + tablepostfix + " (edsmid,sectorid,nameid,x,y,z,info) VALUES " + systeminsertcmd.ToString() + " ON CONFLICT(edsmid) DO UPDATE SET sectorid=excluded.sectorid,nameid=excluded.nameid,x=excluded.x,y=excluded.y,z=excluded.z,info=excluded.info", txn))
                            // no difference in speed

                            //System.Diagnostics.Debug.Assert(!systeminsertcmd.ToString().Contains("."));

                            using (var cmd = db.CreateCommand("INSERT OR REPLACE INTO SystemTable" + tablepostfix + " (edsmid,sectorid,nameid,x,y,z,info) VALUES " + systeminsertcmd.ToString(), txn))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }

                        txn.Commit();
                    }
                }
            }

            private Dictionary<Tuple<long, string>, long> sectorcache;
            private string tablepostfix;
            private int nextsectorid;
            private int maxblocksize;
            private bool overlapped;
            private bool[] grididallowed;
            private DateTime maxdate;
            private StreamWriter debugfile = null;

            // from https://spansh.co.uk/api/bodies/field_values/subtype
            static private Dictionary<string, EDStar> spanshtoedstar = new Dictionary<string, EDStar>
            {
                { "O (Blue-White) Star", EDStar.O },
                { "B (Blue-White) Star", EDStar.B },
                { "A (Blue-White) Star", EDStar.A },
                { "F (White) Star", EDStar.F },
                { "G (White-Yellow) Star", EDStar.G },
                { "K (Yellow-Orange) Star", EDStar.K },
                { "M (Red dwarf) Star", EDStar.M },

                { "L (Brown dwarf) Star", EDStar.L },
                { "T (Brown dwarf) Star", EDStar.T },
                { "Y (Brown dwarf) Star", EDStar.Y },

                { "Herbig Ae Be Star", EDStar.AeBe },
                { "T Tauri Star", EDStar.TTS },

                { "Wolf-Rayet Star", EDStar.W },
                { "Wolf-Rayet N Star", EDStar.WN },
                { "Wolf-Rayet NC Star", EDStar.WNC },
                { "Wolf-Rayet C Star", EDStar.WC },
                { "Wolf-Rayet O Star", EDStar.WO },

                // missing CS
                { "C Star", EDStar.C },
                { "CN Star", EDStar.CN },
                { "CJ Star", EDStar.CJ },
                // missing CHd

                { "MS-type Star", EDStar.MS },
                { "S-type Star", EDStar.S },

                { "White Dwarf (D) Star", EDStar.D },
                { "White Dwarf (DA) Star", EDStar.DA },
                { "White Dwarf (DAB) Star", EDStar.DAB },
                // missing DAO
                { "White Dwarf (DAZ) Star", EDStar.DAZ },
                { "White Dwarf (DAV) Star", EDStar.DAV },
                { "White Dwarf (DB) Star", EDStar.DB },
                { "White Dwarf (DBZ) Star", EDStar.DBZ },
                { "White Dwarf (DBV) Star", EDStar.DBV },
                // missing DO,DOV
                { "White Dwarf (DQ) Star", EDStar.DQ },
                { "White Dwarf (DC) Star", EDStar.DC },
                { "White Dwarf (DCV) Star", EDStar.DCV },
                // missing DX
                { "Neutron Star", EDStar.N },
                { "Black Hole", EDStar.H },
                // missing X but not confirmed with actual journal data


                { "A (Blue-White super giant) Star", EDStar.A_BlueWhiteSuperGiant },
                { "F (White super giant) Star", EDStar.F_WhiteSuperGiant },
                { "M (Red super giant) Star", EDStar.M_RedSuperGiant },
                { "M (Red giant) Star", EDStar.M_RedGiant},
                { "K (Yellow-Orange giant) Star", EDStar.K_OrangeGiant },
                // missing rogueplanet, nebula, stellarremanant
                { "Supermassive Black Hole", EDStar.SuperMassiveBlackHole },
                { "B (Blue-White super giant) Star", EDStar.B_BlueWhiteSuperGiant },
                { "G (White-Yellow super giant) Star", EDStar.G_WhiteSuperGiant },
            };

        }
    }
}

