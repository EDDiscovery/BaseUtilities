using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using SQLLiteExtensions;
using System.Data.Common;
using System.Data;
using Newtonsoft.Json.Linq;

namespace EliteDangerousCore.SystemDB
{
    public class EDDB
    {
        public class EDDBInfo : EliteDangerousCore.ISystemEDDB
        {
            public EDDBInfo()
            {
            }

            public long EDDBID { get; set; }
            public string Faction { get; set; }
            public long Population { get; set; }
            public EDGovernment Government { get; set; }
            public EDAllegiance Allegiance { get; set; }
            public EDState State { get; set; }
            public EDSecurity Security { get; set; }
            public EDEconomy PrimaryEconomy { get; set; }
            public int NeedsPermit { get; set; }
            public int EDDBUpdatedAt { get; set; }
            public bool HasEDDBInformation { get; }
            public long EdsmId;

            public override string ToString()
            {
                return "";// + " (" + Xf.ToString("N1") + "," + Yf.ToString("N1") + "," + Zf.ToString("N1") + ") " + EDSMId + ":" + GridId;
            }
        }

        public static long ParseEDDBJSONFile(string filename, Func<bool> cancelRequested)
        {
            using (StreamReader sr = new StreamReader(filename))         // read directly from file..
                return ParseEDDBJSON(sr, cancelRequested);
        }

        public static long ParseEDDBJSONString(string data, Func<bool> cancelRequested)
        {
            using (StringReader sr = new StringReader(data))         // read directly from file..
                return ParseEDDBJSON(sr, cancelRequested);
        }


        public static long ParseEDDBJSON(TextReader tr, Func<bool> cancelRequested)
        {
            long updated = 0;
            long inserted = 0;
            long processed = 0;

            bool eof = false;

            while (!eof)
            {
                SQLExtTransactionLock<SQLiteConnectionSystem> tl = new SQLExtTransactionLock<SQLiteConnectionSystem>();
                tl.OpenWriter();

                SQLiteConnectionSystem cn = new SQLiteConnectionSystem(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.ReaderWriter);

                DbTransaction txn = cn.BeginTransaction();
                DbCommand selectCmd = cn.CreateCommand("SELECT eddbupdatedat FROM EDDB WHERE edsmid = @edsmid LIMIT 1", txn);   // 1 return matching ID
                selectCmd.AddParameter("@Edsmid", DbType.Int64);

                DbCommand insertCmd = cn.CreateCommand("INSERT INTO EDDB (edsmid, eddbupdatedat, properties) VALUES (@edsmid, @eddbupdatedat, @properties)", txn);
                insertCmd.AddParameter("@edsmid", DbType.Int64);
                insertCmd.AddParameter("@eddbupdatedat", DbType.Int64);
                insertCmd.AddParameter("@properties", DbType.String);

                DbCommand updateCmd = cn.CreateCommand("UPDATE EDDB SET eddbupdatedat=@eddbupdatedat, properties=@properties WHERE edsmid=@edsmid", txn);
                insertCmd.AddParameter("@edsmid", DbType.Int64);
                insertCmd.AddParameter("@eddbupdatedat", DbType.Int64);
                insertCmd.AddParameter("@properties", DbType.String);

                while (!SQLiteConnectionSystem.IsReadWaiting)
                {
                    string line = tr.ReadLine();

                    if (line == null)  // End of stream
                    {
                        eof = true;
                        break;
                    }

                    try
                    {
                        JObject jo = JObject.Parse(line);
                        long jsonupdatedat = jo["updated_at"].Int();
                        long jsonedsmid = jo["edsm_id"].Long();

                        long dbupdated_at = 0;

                        selectCmd.Parameters["@edsmid"].Value = jsonedsmid;

                        using (DbDataReader reader = selectCmd.ExecuteReader())
                        {
                            if (reader.Read())
                                dbupdated_at = (long)reader[0];
                        }

                        if (dbupdated_at != 0)
                        {
                            if (jsonupdatedat != dbupdated_at)
                            {
                                updateCmd.Parameters["@edsmid"].Value = jsonedsmid;
                                updateCmd.Parameters["@eddbupdatedat"].Value = jsonupdatedat;
                                updateCmd.Parameters["@properties"].Value = line;
                                updateCmd.ExecuteNonQuery();
                                updated++;
                            }
                        }
                        else
                        {
                            insertCmd.Parameters["@edsmid"].Value = jsonedsmid;
                            insertCmd.Parameters["@eddbupdatedat"].Value = jsonupdatedat;
                            insertCmd.Parameters["@properties"].Value = line;
                            insertCmd.ExecuteNonQuery();
                            inserted++;
                        }

                        processed++;
                        if (processed % 1000 == 0)
                        {
                            System.Diagnostics.Debug.WriteLine("Process " + BaseUtils.AppTicks.TickCountLap("EDDB") + "   " + updated + " " + inserted);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("EDDB JSON file exception " + ex.ToString());
                    }
                }

                txn.Commit();
                updateCmd.Dispose();
                insertCmd.Dispose();
                selectCmd.Dispose();
                tl.Dispose();
                cn.Dispose();
            }

            return updated + inserted;
        }



    }
}


