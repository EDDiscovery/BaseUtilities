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

    public abstract class SQLExtConnection : IDisposable              // USE this for connections.. 
    {
        // note access mode is not currently used, but its kept for future use in case we can optimise the DB for a particular mode
        public enum AccessMode { Reader, Writer, ReaderWriter };           

        protected DbConnection connection;      // the connection
        protected Thread owningThread;          // tracing who owns the thread to prevent cross thread ops
        protected static List<SQLExtConnection> openConnections = new List<SQLExtConnection>(); // debugging mostly, track connections
        protected static DbProviderFactory DbFactory = GetSqliteProviderFactory();  
        public string DBFile { get; protected set; }

        protected SQLExtConnection( AccessMode mode )
        {
            lock (openConnections)
            {
                openConnections.Add(this);
            }
            owningThread = Thread.CurrentThread;
        }

        private static DbProviderFactory GetSqliteProviderFactory()
        {
            if (WindowsSqliteProviderWorks())
            {
                return GetWindowsSqliteProviderFactory();
            }

            var factory = GetMonoSqliteProviderFactory();

            if (DbFactoryWorks(factory))
            {
                return factory;
            }

            throw new InvalidOperationException("Unable to get a working Sqlite driver");
        }

        private static bool WindowsSqliteProviderWorks()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                return false;
            }

            try
            {
                // This will throw an exception if the SQLite.Interop.dll can't be loaded.
                string sqliteversion = SQLiteConnection.SQLiteVersion;

                if (!String.IsNullOrEmpty(sqliteversion))
                {
                    System.Diagnostics.Trace.WriteLine($"SQLite Version {sqliteversion}");
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private static bool DbFactoryWorks(DbProviderFactory factory)
        {
            if (factory != null)
            {
                try
                {
                    using (var conn = factory.CreateConnection())
                    {
                        conn.ConnectionString = "Data Source=:memory:;Pooling=true;";
                        conn.Open();
                        return true;
                    }
                }
                catch
                {
                }
            }

            return false;
        }

        private static DbProviderFactory GetMonoSqliteProviderFactory()
        {
            try
            {
                // Disable CS0618 warning for LoadWithPartialName
#pragma warning disable 618
                var asm = System.Reflection.Assembly.LoadWithPartialName("Mono.Data.Sqlite");
#pragma warning restore 618
                var factorytype = asm.GetType("Mono.Data.Sqlite.SqliteFactory");
                return (DbProviderFactory)factorytype.GetConstructor(new Type[0]).Invoke(new object[0]);
            }
            catch
            {
                return null;
            }
        }

        private static DbProviderFactory GetWindowsSqliteProviderFactory()
        {
            try
            {
                return new System.Data.SQLite.SQLiteFactory();
            }
            catch
            {
                return null;
            }
        }

        protected void AssertThreadOwner()
        {
            if (Thread.CurrentThread != owningThread)
            {
                throw new InvalidOperationException($"DB connection was passed between threads.  Owning thread: {owningThread.Name} ({owningThread.ManagedThreadId}); this thread: {Thread.CurrentThread.Name} ({Thread.CurrentThread.ManagedThreadId})");
            }
        }

        // subbed to derived class to implement..

        public abstract DbCommand CreateCommand(string cmd, DbTransaction tn = null);
        public abstract DbTransaction BeginTransaction(IsolationLevel isolevel);
        public abstract DbTransaction BeginTransaction();
        public abstract void Dispose();
        public abstract DbDataAdapter CreateDataAdapter(DbCommand cmd);

        protected virtual void Dispose(bool disposing)
        {
            lock (openConnections)
            {
                SQLExtConnection.openConnections.Remove(this);
            }
        }

        // provided for DB upgrade operations at the basic level..

        public void PerformUpgrade( int newVersion, bool catchErrors, bool backupDbFile, string[] queries, Action doAfterQueries = null)
        {
            if (backupDbFile)
            {
                string dbfile = DBFile;

                try
                {
                    File.Copy(dbfile, dbfile.Replace(".sqlite", $"{newVersion - 1}.sqlite"));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("Exception: " + ex.Message);
                    System.Diagnostics.Trace.WriteLine("Trace: " + ex.StackTrace);
                }
            }

            try
            {
                foreach (var query in queries)
                {
                    ExecuteQuery(query);
                }
            }
            catch (Exception ex)
            {
                if (!catchErrors)
                    throw;

                System.Diagnostics.Trace.WriteLine("Exception: " + ex.Message);
                System.Diagnostics.Trace.WriteLine("Trace: " + ex.StackTrace);
                System.Windows.Forms.MessageBox.Show($"UpgradeDB{newVersion} error: " + ex.Message);
            }

            doAfterQueries?.Invoke();

            SQLExtRegister reg = new SQLExtRegister(this);
            reg.PutSettingInt("DBVer", newVersion);
        }

        // Query operator

        public void ExecuteQuery(string query)
        {
            using (DbCommand command = CreateCommand(query))
                command.ExecuteNonQuery();
        }
    }
}
