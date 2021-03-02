/*
 * Copyright © 2019-2020 EDDiscovery development team
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
using System.Data;
using System.Data.Common;

namespace SQLLiteExtensions
{
    // Connection with a register

    public class SQLExtConnectionRegister<TConn> : SQLExtConnection where TConn : SQLExtConnection, new()
    {
        public SQLExtRegister RegisterClass;

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

                if (mode == AccessMode.Reader)
                {
                    connection.ConnectionString += "Read Only=True;";
                }

                // System.Diagnostics.Debug.WriteLine("Created connection " + connection.ConnectionString);

                connection.Open();

                RegisterClass = new SQLExtRegister(this);
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


    }
}
