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
        DbTransaction txn;

        public SQLExtRegister(SQLExtConnection cn)
        {
            this.cn = cn;
        }

        public SQLExtRegister(SQLExtConnection cn, DbTransaction txn)
        {
            this.cn = cn;
            this.txn = txn;
        }

        public bool keyExists(string sKey)
        {
            using (DbCommand cmd = cn.CreateCommand("select ID from Register WHERE ID=@key", txn))
            {
                cmd.AddParameterWithValue("@key", sKey);
                return cmd.ExecuteScalar() != null;
            }
        }

        public bool DeleteKey(string sKey)        // SQL wildcards
        {
            using (DbCommand cmd = cn.CreateCommand("Delete from Register WHERE ID like @key", txn))
            {
                cmd.AddParameterWithValue("@key", sKey);
                return cmd.ExecuteScalar() != null;
            }
        }

        public T GetSetting<T>(string key, T defaultvalue)
        {
            Type tt = typeof(T);
            Object ret = null;

            if (tt == typeof(int))
            {
                ret = GetSetting(key, "ValueInt");
                if (ret != null)            // DB returns Long, so we need to convert
                    ret = Convert.ToInt32(ret);
            }
            else if (tt == typeof(long))
                ret = GetSetting(key, "ValueInt"c;
            else if (tt == typeof(double))
                ret = GetSetting(key, "ValueDouble");
            else if (tt == typeof(string))
                ret = GetSetting(key, "ValueString");
            else if (tt == typeof(bool))
            {
                ret = GetSetting(key, "ValueInt");
                if (ret != null)        // convert return to bool
                    ret = Convert.ToInt32(ret) != 0;
            }
            else if (tt == typeof(DateTime))
            {
                ret = GetSetting(key, "ValueString");
                if (ret != null)
                {
                    string s = Convert.ToString(ret);
                    if (DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime date))
                        ret = date;
                    else
                        ret = null;
                }
            }
            else
                System.Diagnostics.Debug.Assert(false, "Not valid type");

            return ret != null ? (T)ret : (T)defaultvalue;
        }

        public bool PutSetting<T>(string key, T value)
        {
            Type tt = typeof(T);

            if (tt == typeof(int))
                return PutSetting(key, "ValueInt:int32", value);
            else if (tt == typeof(long))
                return PutSetting(key, "ValueInt:int64", value);
            else if (tt == typeof(double))
                return PutSetting(key, "ValueDouble:Double", value);
            else if (tt == typeof(string))
                return PutSetting(key, "ValueString:string", value);
            else if (tt == typeof(bool))
                return PutSetting(key, "ValueInt:int32", Convert.ToBoolean(value) ? 1 : 0);
            else if (tt == typeof(DateTime))
                return PutSetting(key, "ValueString:string", Convert.ToDateTime(value).ToStringZulu());
            else
            {
                System.Diagnostics.Debug.Assert(false, "Not valid type");
                return false;
            }
        }

        private Object GetSetting(string key, string sqlname)
        {
            using (DbCommand cmd = cn.CreateSelect("Register", outparas: sqlname, inparas: new string[] { "ID:string" }, where: "ID=@ID"))
            {
                cmd.Parameters[0].Value = key;
                return cmd.ExecuteScalar();
            }
        }

        private bool PutSetting(string key, string sqlnametype, object value)
        {
            using (DbCommand cmd = cn.CreateReplace("Register", new string[] { "ID:string", sqlnametype }))
            {
                cmd.Parameters[0].Value = key;
                cmd.Parameters[1].Value = value;
                cmd.ExecuteNonQuery();
                return true;
            }
        }
    }
}
