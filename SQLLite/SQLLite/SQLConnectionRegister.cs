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
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Threading;

namespace SQLLiteExtensions
{
    // Base class
    // Its a connection class which has a DB factory and delegates the connection/transactions to its derived classes.
    // has a upgrade function 

    public class SQLExtConnectionRegister<TConn> : SQLExtConnection where TConn : SQLExtConnection, new()
    {
        public SQLExtConnectionRegister(string dbfile, bool utctimeindicator, AccessMode mode = AccessMode.ReaderWriter) : base(mode)
        {
            try
            {
                DBFile = dbfile;
                connection = DbFactory.CreateConnection();

                // Use the database selected by maindb as the 'main' database
                connection.ConnectionString = "Data Source=" + DBFile.Replace("\\", "\\\\") + ";Pooling=true;";

                if (utctimeindicator)   // indicate treat dates as UTC.
                    connection.ConnectionString += "DateTimeKind=Utc;";

                // System.Diagnostics.Debug.WriteLine("Created connection " + connection.ConnectionString);

                connection.Open();
            }
            catch
            {
                throw;
            }
        }


        public override DbTransaction BeginTransaction(IsolationLevel isolevel)
        {
            AssertThreadOwner();
            return connection.BeginTransaction(isolevel);
        }

        public override DbTransaction BeginTransaction()
        {
            AssertThreadOwner();
            return connection.BeginTransaction();
        }

        public override DbCommand CreateCommand(string query, DbTransaction tn = null)
        {
            AssertThreadOwner();
            DbCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 30;
            cmd.CommandText = query;
            return cmd;
        }

        public override DbDataAdapter CreateDataAdapter(DbCommand cmd)
        {
            DbDataAdapter da = DbFactory.CreateDataAdapter();
            da.SelectCommand = cmd;
            return da;
        }

        public override void Dispose()
        {
            Dispose(true);
        }

        // disposing: true if Dispose() was called, false
        // if being finalized by the garbage collector
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (connection != null)
                {
                    //   System.Diagnostics.Debug.WriteLine("Closed connection " + connection.ConnectionString);
                    connection.Close();
                    connection.Dispose();
                    connection = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Register

        static public bool keyExists(string sKey, TConn conn = null)
        {
            return RegisterGet(sKey, false, cn => { var reg = new SQLExtRegister(cn); return reg.keyExists(sKey); }, conn);
        }

        static public bool DeleteKey(string key, TConn conn = null)
        {
            return RegisterDelete(key, cn => { var reg = new SQLExtRegister(cn); return reg.DeleteKey(key); }, conn);
        }

        static public int GetSettingInt(string key, int defaultvalue, TConn conn = null)
        {
            return (int)RegisterGet(key, defaultvalue, cn => { var reg = new SQLExtRegister(cn); return reg.GetSettingInt(key, defaultvalue); }, conn);
        }

        static public bool PutSettingInt(string key, int intvalue, TConn conn = null)
        {
            return RegisterPut(cn => { var reg = new SQLExtRegister(cn); return reg.PutSettingInt(key, intvalue); }, conn);
        }

        static public double GetSettingDouble(string key, double defaultvalue, TConn conn = null)
        {
            return RegisterGet(key, defaultvalue, cn => { var reg = new SQLExtRegister(cn); return reg.GetSettingDouble(key, defaultvalue); }, conn);
        }

        static public bool PutSettingDouble(string key, double doublevalue, TConn conn = null)
        {
            return RegisterPut(cn => { var reg = new SQLExtRegister(cn); return reg.PutSettingDouble(key, doublevalue); }, conn);
        }

        static public bool GetSettingBool(string key, bool defaultvalue, TConn conn = null)
        {
            return RegisterGet(key, defaultvalue, cn => { var reg = new SQLExtRegister(cn); return reg.GetSettingBool(key, defaultvalue); }, conn);
        }

        static public bool PutSettingBool(string key, bool boolvalue, TConn conn = null)
        {
            return RegisterPut(cn => { var reg = new SQLExtRegister(cn); return reg.PutSettingBool(key, boolvalue); }, conn);
        }

        static public string GetSettingString(string key, string defaultvalue, TConn conn = null)
        {
            return RegisterGet(key, defaultvalue, cn => { var reg = new SQLExtRegister(cn); return reg.GetSettingString(key, defaultvalue); }, conn);
        }

        static public bool PutSettingString(string key, string strvalue, TConn conn = null)        // public IF
        {
            return RegisterPut(cn => { var reg = new SQLExtRegister(cn); return reg.PutSettingString(key, strvalue); }, conn);
        }

        static public DateTime GetSettingDate(string key, DateTime defaultvalue, TConn conn = null)
        {
            string s = RegisterGet(key, "--", cn => { var reg = new SQLExtRegister(cn); return reg.GetSettingString(key, "--"); }, conn);

            if (!DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime date))
            {
                date = defaultvalue;
            }

            return date;
        }

        static public bool PutSettingDate(string key, DateTime value, TConn conn = null)        // public IF
        {
            return RegisterPut(cn => { var reg = new SQLExtRegister(cn); return reg.PutSettingString(key, value.ToStringZulu()); }, conn);
        }

#endregion

        #region Generics

        public static T RegisterGet<T>(string key, T defval, Func<TConn, T> normal, TConn conn)
        {
            if (conn != null)
            {
                return normal(conn);
            }
            using (TConn cn = new TConn())
            {
                return normal(cn);
            }
        }

        public static bool RegisterPut(Func<TConn, bool> action, TConn conn)
        {
            if (conn != null)
            {
                return action(conn);
            }

            using (TConn cn = new TConn())
            {
                return action(cn);
            }
        }

        public static bool RegisterDelete(string key, Func<TConn, bool> action, TConn conn)
        {
            if (conn != null)
            {
                return action(conn);
            }

            using (TConn cn = new TConn())
            {
                return action(cn);
            }
        }

        #endregion

    }
}
