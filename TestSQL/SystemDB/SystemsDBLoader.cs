/*
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

        public class Loader
        {
            // create - postfix allows a different table set to be created
            // maxblocksize - write back when reached this
            // gridids - null or array of allowed gridid
            // debugoutputfile - write file of loaded systems
            public Loader(string ptablepostfix, int pmaxblocksize, bool[] gridids, bool poverlapped, string debugoutputfile = null)
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

                System.Diagnostics.Trace.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS", true)} System DB store start");

                var parser = new QuickJSON.Utils.StringParserQuickTextReader(textreader, 32768);
                var enumerator = JToken.ParseToken(parser, JToken.ParseOptions.None).GetEnumerator();       // Parser may throw note

                int wbno = 0;
                WriteBlock curwb = new WriteBlock(++wbno);
                WriteBlock prevwb = null;

                int recordstostore = 0;

                bool stop = false;

                while (!stop)
                {
                    if (cancelRequested() || !enumerator.MoveNext())        // get next token, if not, stop eof
                        stop = true;

                    if (stop == false)      // if not stopping, try for next record
                    {
                        JToken token = enumerator.Current;

                        if (token.IsObject)                   // if start of object..
                        {
                            StarFileEntry d = new StarFileEntry();

                            // if we have a valid record
                            if (d.Deserialize(enumerator))
                            {
                                int gridid = GridId.Id128(d.x, d.z);

                                if (grididallowed == null || (grididallowed.Length > gridid && grididallowed[gridid]))    // allows a null or small grid
                                {
                                    if (d.date > maxdate)                                   // for all, record last recorded date processed
                                        maxdate = d.date;

                                    var classifier = new EliteNameClassifier(d.name);

                                    var skey = new Tuple<long, string>(gridid, classifier.SectorName);

                                    System.Diagnostics.Debug.Assert(curwb.wbno == wbno);

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
                                        curwb.nameinsertcmd.Append(d.id.ToStringInvariant());
                                        curwb.nameinsertcmd.Append(",'");
                                        curwb.nameinsertcmd.Append(classifier.StarName.Replace("'", "''"));
                                        curwb.nameinsertcmd.Append("') ");
                                        classifier.NameIdNumeric = d.id;                        // the name becomes the id of the entry
                                    }

                                    if (curwb.systeminsertcmd.Length > 0)
                                        curwb.systeminsertcmd.Append(',');

                                    curwb.systeminsertcmd.Append('(');                            // add (id,sectorid,nameid,x,y,z,info) to systems insert string
                                    curwb.systeminsertcmd.Append(d.id.ToStringInvariant());
                                    curwb.systeminsertcmd.Append(',');
                                    curwb.systeminsertcmd.Append(sectorid.ToStringInvariant());
                                    curwb.systeminsertcmd.Append(',');
                                    curwb.systeminsertcmd.Append(classifier.ID.ToStringInvariant());
                                    curwb.systeminsertcmd.Append(',');
                                    curwb.systeminsertcmd.Append(d.x);
                                    curwb.systeminsertcmd.Append(',');
                                    curwb.systeminsertcmd.Append(d.y);
                                    curwb.systeminsertcmd.Append(',');
                                    curwb.systeminsertcmd.Append(d.z);
                                    curwb.systeminsertcmd.Append(",");
                                    if (d.startype != null)
                                        curwb.systeminsertcmd.Append((int)d.startype);
                                    else
                                        curwb.systeminsertcmd.Append("NULL");
                                    curwb.systeminsertcmd.Append(") ");

                                    if (debugfile != null)
                                        debugfile.WriteLine(d.name + " " + d.x + "," + d.y + "," + d.z + ", ID:" + d.id + " SEC " + sectorid + " Grid " + gridid);

                                    recordstostore++;
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"JSON System DB not in good form");
                            }
                        }
                    }

                    if (recordstostore >= maxblocksize || stop)     // if too many, or we are stopping
                    {
                        if (prevwb != null)     // if we have a pending one, we wait for it to finish, as we don't overlap them 
                        {
                            System.Diagnostics.Trace.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS")} Wait for previous write {prevwb.wbno} to complete");
                            SystemsDatabase.Instance.DBWait(prevwb.sqlop, 5000);
                            System.Diagnostics.Trace.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS")} previous block {prevwb.wbno} complete");
                            prevwb.sqlop = null;
                        }

                        System.Diagnostics.Trace.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS")} Begin write next block {recordstostore} {updates} block {curwb.wbno}");
                      
                        Write(curwb);       // creat a no wait db write - need to do this in a function because we are about to change curwb

                        if ( overlapped )
                        {
                            prevwb = curwb;         // set previous and we will check next time to see if we need to pend
                        }
                        else
                        {
                            SystemsDatabase.Instance.DBWait(curwb.sqlop, 5000);    // else not overlapped, finish it now
                        }

                        curwb = new WriteBlock(++wbno); // make a new one

                        updates += recordstostore;
                        reportProgress?.Invoke($"Star database updated {recordstostore:N0} total so far {(updates):N0}");
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

                System.Diagnostics.Trace.WriteLine($"{BaseUtils.AppTicks.TickCountLap("SDBS")} System DB store end");

                reportProgress?.Invoke($"Star database updated {updates:N0}");

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
        }
    }
}


