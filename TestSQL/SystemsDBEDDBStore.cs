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

                string[] dbfields = { "edsmid", "eddbid", "eddbupdatedat", "population",
                                        "faction", "government", "allegiance", "state",
                                        "security", "primaryeconomy", "needspermit", "power",
                                        "powerstate" , "properties" };
                DbType[] dbfieldtypes = { DbType.Int64, DbType.Int64, DbType.Int64, DbType.Int64, DbType.String, DbType.Int64, DbType.Int64, DbType.Int64, DbType.Int64, DbType.Int64, DbType.Int64, DbType.String ,DbType.String ,DbType.String };

                DbCommand insertCmd = cn.CreateInsert("EDDB", dbfields, dbfieldtypes, txn);
                DbCommand updateCmd = cn.CreateUpdate("EDDB", "WHERE edsmid=@edsmid", dbfields, dbfieldtypes, txn);

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

                           // if (dbupdated_at == 0 || jsonupdatedat != dbupdated_at)
                            {
                                DbCommand cmd = dbupdated_at != 0 ? updateCmd : insertCmd;

                                cmd.Parameters["@edsmid"].Value = jsonedsmid;
                                cmd.Parameters["@eddbid"].Value = jo["id"].Long();
                                cmd.Parameters["@eddbupdatedat"].Value = jsonupdatedat;
                                cmd.Parameters["@population"].Value = jo["population"].Long();
                                cmd.Parameters["@faction"].Value = jo["controlling_minor_faction"].Str("Unknown");
                                cmd.Parameters["@government"].Value = EliteDangerousTypesFromJSON.Government2ID(jo["government"].Str("Unknown"));
                                cmd.Parameters["@allegiance"].Value = EliteDangerousTypesFromJSON.Allegiance2ID(jo["allegiance"].Str("Unknown"));

                                EDState edstate = EDState.Unknown;

                                try
                                {
                                    if (jo["states"] != null && jo["states"].HasValues)
                                    {
                                        JToken tk = jo["states"].First;     // we take the first one whatever
                                        JObject jostate = (JObject)tk;
                                        edstate = EliteDangerousTypesFromJSON.EDState2ID(jostate["name"].Str("Unknown"));
                                    }
                                }
                                catch ( Exception ex )
                                {
                                    System.Diagnostics.Debug.WriteLine("EDDB JSON file exception for states " + ex.ToString());
                                }

                                cmd.Parameters["@state"].Value = edstate;
                                cmd.Parameters["@security"].Value = EliteDangerousTypesFromJSON.EDSecurity2ID(jo["security"].Str("Unknown"));
                                cmd.Parameters["@primaryeconomy"].Value = EliteDangerousTypesFromJSON.EDEconomy2ID(jo["primary_economy"].Str("Unknown"));
                                cmd.Parameters["@needspermit"].Value = jo["needs_permit"].Int(0);
                                cmd.Parameters["@power"].Value = jo["power"].Str("None");
                                cmd.Parameters["@powerstate"].Value = jo["power_state"].Str("N/A");
                                cmd.Parameters["@properties"].Value = RemoveFieldsFromJSON(jo);
                                cmd.ExecuteNonQuery();
                                updated++;
                            }

                            processed++;
                            if (processed % 1000 == 0)
                            {
                                System.Diagnostics.Debug.WriteLine("Process " + BaseUtils.AppTicks.TickCountLap("EDDB") + "   " + updated);
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

            return updated;
        }

        static private string RemoveFieldsFromJSON(JObject jo)
        {
            jo.Remove("x");
            jo.Remove("y");
            jo.Remove("z");
            jo.Remove("edsm_id");
            jo.Remove("id");
            jo.Remove("name");
            jo.Remove("is_populated");
            jo.Remove("population");
            jo.Remove("government");
            jo.Remove("government_id");
            jo.Remove("allegiance");
            jo.Remove("allegiance_id");
            jo.Remove("state");
            jo.Remove("security");
            jo.Remove("security_id");
            jo.Remove("primary_economy");
            jo.Remove("primary_economy_id");
            jo.Remove("power");
            jo.Remove("power_state");
            jo.Remove("power_state_id");
            jo.Remove("needs_permit");
            jo.Remove("updated_at");
            jo.Remove("controlling_minor_faction");

            return jo.ToString(Formatting.None);
        }
    }
}


