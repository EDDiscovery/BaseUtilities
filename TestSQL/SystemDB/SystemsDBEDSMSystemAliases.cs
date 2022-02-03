﻿/*
 * Copyright 2015-2021 EDDiscovery development team
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

using QuickJSON;
using System.IO;
using System.Data.Common;
using System.Collections.Generic;
using System;

namespace EliteDangerousCore.DB
{
    public partial class SystemsDB
    {
        public static long ParseAliasFile(string filename)
        {
            using (StreamReader sr = new StreamReader(filename))         // read directly from file..
                return ParseAlias(sr);
        }

        public static long ParseAliasString(string json)
        {
            using (StringReader sr = new StringReader(json))
                return ParseAlias(sr);
        }

        public static long ParseAlias(TextReader textreader)
        {
            return SystemsDatabase.Instance.DBWrite(db =>
            {
                var cn = db;
                System.Diagnostics.Debug.WriteLine("Update aliases");

                long updates = 0;

                using (DbTransaction txn = cn.BeginTransaction())
                {
                    DbCommand selectCmd = null;
                    DbCommand deletesystemcmd = null;
                    DbCommand insertCmd = null;
                    try
                    {
                        selectCmd = cn.CreateSelect("Aliases", "edsmid", "edsmid = @edsmid", inparas: new string[] { "edsmid:int64" }, limit: "1", tx: txn);   // 1 return matching ID
                        deletesystemcmd = cn.CreateDelete("Systems", "edsmid=@edsmid", paras: new string[] { "edsmid:int64" }, tx: txn);
                        insertCmd = cn.CreateReplace("Aliases", paras: new string[] { "edsmid:int64", "edsmid_mergedto:int64", "name:string" }, tx: txn);

                        var parser = new QuickJSON.Utils.StringParserQuickTextReader(textreader, 32768);
                        var enumerator = JToken.ParseToken(parser, JToken.ParseOptions.None).GetEnumerator();

                        try
                        {       // protect against json exceptions
                            while (true)
                            {
                                if ( !enumerator.MoveNext())
                                {
                                    break;
                                }

                                JToken t = enumerator.Current;

                                if (t.IsObject)
                                {
                                    JToken.LoadTokens(enumerator);
                                    JObject jo = t as JObject;

                                    long edsmid = jo["id"].Long();
                                    string name = jo["system"].Str("Default");
                                    string action = jo["action"].Str("NOP");
                                    long mergedto = 0;

                                    if (jo["mergedTo"] != null)
                                    {
                                        mergedto = jo["mergedTo"].Long();
                                    }

                                    if (action.Contains("delete system", System.StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        deletesystemcmd.Parameters[0].Value = edsmid;
                                        deletesystemcmd.ExecuteNonQuery();
                                    }

                                    if (mergedto > 0)
                                    {
                                        selectCmd.Parameters[0].Value = edsmid;
                                        long foundedsmid = selectCmd.ExecuteScalar<long>(-1);

                                        if (foundedsmid == -1)
                                        {
                                            insertCmd.Parameters[0].Value = edsmid;
                                            insertCmd.Parameters[1].Value = mergedto;
                                            insertCmd.Parameters[2].Value = name;
                                            insertCmd.ExecuteNonQuery();
                                            //System.Diagnostics.Debug.WriteLine("Alias " + edsmid + " -> " + mergedto + " " + name);
                                            updates++;
                                        }
                                    }

                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("JSON format error in aliases " + ex);
                        }

                        txn.Commit();
                    }
                    finally
                    {
                        selectCmd.Dispose();
                        deletesystemcmd.Dispose();
                        insertCmd.Dispose();
                    }
                }

                return updates;
            });
        }

        // pass edsmid>0, or name not null with characters, or both

        public static long FindAlias(long edsmid, string name)
        {
            return SystemsDatabase.Instance.DBRead(db =>
            {
                return FindAlias(edsmid, name, db);
            });
        }

        internal static long FindAlias(long edsmid, string name, SQLiteConnectionSystem cn)
        {
            string query = "edsmid = @edsmid OR name = @name";
            if (edsmid < 1)
                query = "name = @name";
            else if (!name.HasChars())
                query = "edsmid = @edsmid";

            using (DbCommand selectCmd = cn.CreateSelect("Aliases", "edsmid_mergedto", query, inparas: new string[] { "edsmid:int64", "name:string" }))
            {
                selectCmd.Parameters[0].Value = edsmid;
                selectCmd.Parameters[1].Value = name;
                return selectCmd.ExecuteScalar<long>(-1);
            }
        }

        public static List<ISystem> FindAliasWildcard(string name)
        {
            return SystemsDatabase.Instance.DBRead(db =>
            {
                return FindAliasWildcard(name, db);
            });
        }

        internal static List<ISystem> FindAliasWildcard(string name, SQLiteConnectionSystem cn)
        {
            List<ISystem> ret = new List<ISystem>();

            using (DbCommand selectSysCmd = cn.CreateSelect("Systems s", MakeSystemQueryNamed,
                                                "s.edsmid IN (Select edsmid_mergedto FROM Aliases WHERE name like @p1)",
                                                new Object[] { name+"%" },
                                                joinlist: MakeSystemQueryNamedJoinList))
            {
                //System.Diagnostics.Debug.WriteLine( cn.ExplainQueryPlanString(selectSysCmd));

                using (DbDataReader reader = selectSysCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ret.Add(MakeSystem(reader));
                    }
                }
            }

            return ret;
        }
    }
}


