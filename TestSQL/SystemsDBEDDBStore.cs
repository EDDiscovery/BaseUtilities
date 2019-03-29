using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using SQLLiteExtensions;
using System.Data.Common;
using System.Data;
using Newtonsoft.Json.Linq;

namespace EliteDangerousCore.DB
{
    public partial class SystemsDB
    {
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

                DbCommand insertCmd = cn.CreateCommand("INSERT INTO EDDB (edsmid, eddbupdatedat,properties) VALUES (@edsmid, @eddbupdatedat, @properties)", txn);
                insertCmd.AddParameter("@edsmid", DbType.Int64);
                insertCmd.AddParameter("@eddbupdatedat", DbType.Int64);
                insertCmd.AddParameter("@properties", DbType.String);

                DbCommand updateCmd = cn.CreateCommand("UPDATE EDDB SET eddbupdatedat=@eddbupdatedat, properties=@properties WHERE edsmid=@edsmid", txn);
                updateCmd.AddParameter("@edsmid", DbType.Int64);
                updateCmd.AddParameter("@eddbupdatedat", DbType.Int64);
                updateCmd.AddParameter("@properties", DbType.String);

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
                        bool jsonispopulated = jo["is_populated"].Bool();

                        if (jsonispopulated)        // double check that the flag is set - population itself may be zero, for some systems, but its the flag we care about
                        {
                            long dbupdated_at = 0;

                            selectCmd.Parameters["@edsmid"].Value = jsonedsmid;

                            using (DbDataReader reader = selectCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    dbupdated_at = (long)reader[0];
                                }
                            }

                            if (dbupdated_at != 0)
                            {
                                //if (jsonupdatedat != dbupdated_at)
                                {
                                    updateCmd.Parameters["@edsmid"].Value = jsonedsmid;
                                    updateCmd.Parameters["@eddbupdatedat"].Value = jsonupdatedat;
                                    updateCmd.Parameters["@properties"].Value = RemoveStuff(jo);
                                    updateCmd.ExecuteNonQuery();
                                    updated++;
                                }
                            }
                            else
                            {
                                insertCmd.Parameters["@edsmid"].Value = jsonedsmid;
                                insertCmd.Parameters["@eddbupdatedat"].Value = jsonupdatedat;
                                insertCmd.Parameters["@properties"].Value = RemoveStuff(jo);
                                insertCmd.ExecuteNonQuery();
                                inserted++;
                            }

                            processed++;
                            if (processed % 1000 == 0)
                            {
                                System.Diagnostics.Debug.WriteLine("Process " + BaseUtils.AppTicks.TickCountLap("EDDB") + "   " + updated + " " + inserted);
                            }
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

        const string splitchar = "\u2B94";

        static private string RemoveStuff(JObject jo)
        {
            return jo["id"].Long().ToStringInvariant() +
            splitchar + jo["controlling_minor_faction"].Str() +
            splitchar + jo["population"].Long().ToStringInvariant() +
            splitchar + jo["government"].Str() +
            splitchar + jo["allegiance"].Str() +
            splitchar + jo["state"].Str() +
            splitchar + jo["security"].Str() +
            splitchar + jo["primary_economy"].Str() +
            splitchar + jo["needs_permit"].Int().ToStringInvariant() +
            splitchar + jo["updated_at"].Int().ToStringInvariant();
       }




        //    jo.Remove("x");
        //    jo.Remove("y");
        //    jo.Remove("z");
        //    jo.Remove("edsm_id");
        //    jo.Remove("name");
        //    jo.Remove("is_populated");
        //    return jo.ToString(Formatting.None);
        //}
    }
}


