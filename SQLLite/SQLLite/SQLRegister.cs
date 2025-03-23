/*
 * Copyright © 2019-2023 EDDiscovery development team
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

        public int DeleteKey(string sKey)        // SQL wildcards
        {
            using (DbCommand cmd = cn.CreateCommand("Delete from Register WHERE ID LIKE @key"))
            {
                cmd.AddParameterWithValue("@key", sKey);
                var res = cmd.ExecuteNonQuery();
                return res;
            }
        }

        // Date times return them in UTC Kind

        // return setting or default value
        public T GetSetting<T>(string key, T defaultvalue)
        {
            return GetSetting(key, defaultvalue, out bool _);
        }

        // return setting or default value, return if default value
        public T GetSetting<T>(string key, T defaultvalue, out bool usedret)
        {
            Object ret = null;
            Type tt = typeof(T);

            try
            {

                if (tt == typeof(int))
                {
                    ret = GetSetting(key, "ValueInt");
                    if (ret != null)            // DB returns Long, so we need to convert
                        ret = Convert.ToInt32(ret);
                }
                else if (tt == typeof(long))
                    ret = GetSetting(key, "ValueInt");
                else if (tt == typeof(double))
                    ret = GetSetting(key, "ValueDouble");
                else if (tt == typeof(float))
                    ret = (float)(double)GetSetting(key, "ValueDouble");        // GetSetting returns a object, so need to go to its native type double then to float (Crash!)
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
            }
            catch (Exception ex)
            {
                // Protect against Robert!
                System.Diagnostics.Trace.WriteLine($"SQL Register exception for key {key} type {tt.Name} def {defaultvalue} {ex}");
            }

            if (ret != null)
            { 
                usedret = false;
                return (T)ret;
            }
            else
            {
                usedret = true;
                return (T)defaultvalue;
            }
        }

        public bool PutSetting<T>(string key, T value)
        {
            Type tt = typeof(T);

            if (tt == typeof(int))
                return PutSetting(key, "ValueInt", value);
            else if (tt == typeof(long))
                return PutSetting(key, "ValueInt", value);
            else if (tt == typeof(double))
                return PutSetting(key, "ValueDouble", value);
            else if (tt == typeof(float))
            {
                var dv = Convert.ChangeType(value, typeof(double));
                return PutSetting(key, "ValueDouble", dv);
            }
            else if (tt == typeof(string))
                return PutSetting(key, "ValueString", value);
            else if (tt == typeof(bool))
                return PutSetting(key, "ValueInt", Convert.ToBoolean(value) ? 1 : 0);
            else if (tt == typeof(DateTime))
                return PutSetting(key, "ValueString", Convert.ToDateTime(value).ToStringZulu());
            else
            {
                System.Diagnostics.Debug.Assert(false, "Not valid type");
                return false;
            }
        }

        private Object GetSetting(string key, string sqlname)
        {
            using (DbCommand cmd = cn.CreateCommand("SELECT " + sqlname + " from Register WHERE ID = @ID"))
            {
                cmd.AddParameterWithValue("@ID", key);
                var ret = cmd.ExecuteScalar();
                return ret is System.DBNull ? null : ret;
            }
        }

        private bool PutSetting(string key, string sqlname, object value)
        {
            using (DbCommand cmd = cn.CreateCommand("INSERT OR REPLACE INTO Register (ID," + sqlname + ") VALUES (@ID,@Value)"))
            {
                //System.Diagnostics.Debug.WriteLine("DB Write " + key + ": " + value + " " + cmd.CommandText);
                cmd.AddParameterWithValue("@ID", key);
                cmd.AddParameterWithValue("@Value", value);
                cmd.ExecuteNonQuery();
                return true;
            }
        }
    }
}
