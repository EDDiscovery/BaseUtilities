/*
 * Copyright © 2019 EDDiscovery development team
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
using System.Data.Common;

namespace SQLLiteExtensions
{
    // operates a register and gets/sets values from it.

    public class SQLExtRegister
    {
        SQLExtConnection cn;

        public SQLExtRegister(SQLExtConnection cn)
        {
            this.cn = cn;
        }

        public bool keyExists(string sKey)
        {
            using (DbCommand cmd = cn.CreateCommand("select ID from Register WHERE ID=@key"))
            {
                cmd.AddParameterWithValue("@key", sKey);
                return cmd.ExecuteScalar() != null;
            }
        }

        public bool DeleteKey(string sKey)        // SQL wildcards
        {
            using (DbCommand cmd = cn.CreateCommand("Delete from Register WHERE ID like @key"))
            {
                cmd.AddParameterWithValue("@key", sKey);
                return cmd.ExecuteScalar() != null;
            }
        }

        public int GetSettingInt(string key, int defaultvalue)
        {
            try
            {
                using (DbCommand cmd = cn.CreateCommand("SELECT ValueInt from Register WHERE ID = @ID"))
                {
                    cmd.AddParameterWithValue("@ID", key);

                    object ob = cmd.ExecuteScalar();

                    if (ob == null || ob == DBNull.Value)
                        return defaultvalue;

                    int val = Convert.ToInt32(ob);

                    return val;
                }
            }
            catch
            {
                return defaultvalue;
            }
        }

        public bool PutSettingInt(string key, int intvalue)
        {
            try
            {
                if (keyExists(key))
                {
                    using (DbCommand cmd = cn.CreateCommand("Update Register set ValueInt = @ValueInt Where ID=@ID"))
                    {
                        cmd.AddParameterWithValue("@ID", key);
                        cmd.AddParameterWithValue("@ValueInt", intvalue);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
                else
                {
                    using (DbCommand cmd = cn.CreateCommand("Insert into Register (ID, ValueInt) values (@ID, @valint)"))
                    {
                        cmd.AddParameterWithValue("@ID", key);
                        cmd.AddParameterWithValue("@valint", intvalue);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public double GetSettingDouble(string key, double defaultvalue)
        {
            try
            {
                using (DbCommand cmd = cn.CreateCommand("SELECT ValueDouble from Register WHERE ID = @ID"))
                {
                    cmd.AddParameterWithValue("@ID", key);

                    object ob = cmd.ExecuteScalar();

                    if (ob == null || ob == DBNull.Value)
                        return defaultvalue;

                    double val = Convert.ToDouble(ob);

                    return val;
                }
            }
            catch
            {
                return defaultvalue;
            }
        }

        public bool PutSettingDouble(string key, double doublevalue)
        {
            try
            {
                if (keyExists(key))
                {
                    using (DbCommand cmd = cn.CreateCommand("Update Register set ValueDouble = @ValueDouble Where ID=@ID"))
                    {
                        cmd.AddParameterWithValue("@ID", key);
                        cmd.AddParameterWithValue("@ValueDouble", doublevalue);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
                else
                {
                    using (DbCommand cmd = cn.CreateCommand("Insert into Register (ID, ValueDouble) values (@ID, @valdbl)"))
                    {
                        cmd.AddParameterWithValue("@ID", key);
                        cmd.AddParameterWithValue("@valdbl", doublevalue);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public bool GetSettingBool(string key, bool defaultvalue)
        {
            return GetSettingInt(key, defaultvalue ? 1 : 0) != 0;
        }

        public bool PutSettingBool(string key, bool boolvalue)
        {
            return PutSettingInt(key, boolvalue ? 1 : 0);
        }

        public string GetSettingString(string key, string defaultvalue)
        {
            try
            {
                using (DbCommand cmd = cn.CreateCommand("SELECT ValueString from Register WHERE ID = @ID"))
                {
                    cmd.AddParameterWithValue("@ID", key);
                    object ob = cmd.ExecuteScalar();

                    if (ob == null || ob == DBNull.Value)
                        return defaultvalue;

                    string val = (string)ob;

                    return val;
                }
            }
            catch
            {
                return defaultvalue;
            }
        }

        public bool PutSettingString(string key, string strvalue)
        {
            try
            {
                if (keyExists(key))
                {
                    using (DbCommand cmd = cn.CreateCommand("Update Register set ValueString = @ValueString Where ID=@ID"))
                    {
                        cmd.AddParameterWithValue("@ID", key);
                        cmd.AddParameterWithValue("@ValueString", strvalue);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
                else
                {
                    using (DbCommand cmd = cn.CreateCommand("Insert into Register (ID, ValueString) values (@ID, @valint)"))
                    {
                        cmd.AddParameterWithValue("@ID", key);
                        cmd.AddParameterWithValue("@valint", strvalue);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public class Entry
        {
            public string ValueString { get; private set; }
            public long ValueInt { get; private set; }
            public double ValueDouble { get; private set; }
            public byte[] ValueBlob { get; private set; }

            protected Entry()
            {
            }

            public Entry(string stringval = null, byte[] blobval = null, long intval = 0, double floatval = Double.NaN)
            {
                ValueString = stringval;
                ValueBlob = blobval;
                ValueInt = intval;
                ValueDouble = floatval;
            }
        }


        public Dictionary<string, Entry> GetRegister()
        {
            Dictionary<string, Entry> regs = new Dictionary<string, Entry>();

            using (DbCommand cmd = cn.CreateCommand("SELECT Id, ValueInt, ValueDouble, ValueBlob, ValueString FROM register"))
            {
                using (DbDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        string id = (string)rdr["Id"];
                        object valint = rdr["ValueInt"];
                        object valdbl = rdr["ValueDouble"];
                        object valblob = rdr["ValueBlob"];
                        object valstr = rdr["ValueString"];
                        regs[id] = new Entry(
                            valstr as string,
                            valblob as byte[],
                            (valint as long?) ?? 0L,
                            (valdbl as double?) ?? Double.NaN
                        );
                    }
                }
            }

            return regs;
        }
    }
}
