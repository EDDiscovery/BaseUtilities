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

                registerclass = new SQLExtRegister(this);
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

        private SQLExtRegister registerclass;

        public bool keyExists(string sKey)
        {
            return registerclass.keyExists(sKey);
        }

        public bool DeleteKey(string key)
        {
            return registerclass.DeleteKey(key);
        }

        public int GetSettingInt(string key, int defaultvalue)
        {
            return registerclass.GetSettingInt(key, defaultvalue);
        }

        public bool PutSettingInt(string key, int intvalue)
        {
            return registerclass.PutSettingInt(key, intvalue);
        }

        public double GetSettingDouble(string key, double defaultvalue)
        {
            return registerclass.GetSettingDouble(key, defaultvalue);
        }

        public bool PutSettingDouble(string key, double doublevalue)
        {
            return registerclass.PutSettingDouble(key, doublevalue); 
        }

        public bool GetSettingBool(string key, bool defaultvalue)
        {
            return registerclass.GetSettingBool(key, defaultvalue); 
        }

        public bool PutSettingBool(string key, bool boolvalue)
        {
            return registerclass.PutSettingBool(key, boolvalue);
        }

        public string GetSettingString(string key, string defaultvalue)
        {
            return registerclass.GetSettingString(key, defaultvalue);
        }

        public bool PutSettingString(string key, string strvalue)       
        {
            return registerclass.PutSettingString(key, strvalue); 
        }

        public DateTime GetSettingDate(string key, DateTime defaultvalue)
        {
            string s = registerclass.GetSettingString(key, "--"); 

            if (!DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime date))
            {
                date = defaultvalue;
            }

            return date;
        }

        public bool PutSettingDate(string key, DateTime value)      
        {
            return registerclass.PutSettingString(key, value.ToStringZulu());
        }

        #endregion


    }
}
