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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Threading;

namespace SQLLiteExtensions
{
    // Base class for Connections
    
    public abstract class SQLExtConnection : IDisposable              // USE this for connections.. 
    {
        public enum AccessMode { Reader, Writer, ReaderWriter };           

        protected SQLExtConnection( AccessMode mode = AccessMode.ReaderWriter )
        {
            lock (openConnections)  // thread lock
            {
                openConnections.Add(this);
                if (openConnections.Count > 50)     // this is a lot of parallel connections.. warn as we may be forgetting to close them
                    System.Diagnostics.Debug.WriteLine("SQLExtConnection warning open connection count now " + openConnections.Count);
            }

            owningThread = Thread.CurrentThread;
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

        protected virtual void Dispose(bool disposing)
        {
            lock (openConnections)
            {
                System.Diagnostics.Debug.WriteLine("SQLConnection dispose");
                openConnections.Remove(this);
                hasbeendisposed = true;
            }
        }

#if DEBUG
        //since finalisers impose a penalty, we shall check only in debug mode
        ~SQLExtConnection()
        {
            if (!hasbeendisposed)       // finalisation may come very late.. not immediately as its done on garbage collection.  Warn by message and assert.
            {
                System.Windows.Forms.MessageBox.Show("Missing dispose for connection " + DBFile);
                System.Diagnostics.Debug.Assert(hasbeendisposed, "Missing dispose for connection" + DBFile);       // must have been disposed
            }
        }
#endif

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
                ExecuteNonQueries(queries);
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
            reg.PutSetting("DBVer", newVersion);
        }

        // Query operators

        public void ExecuteNonQueries(string[] queries)
        {
            foreach (var query in queries)
            {
                ExecuteNonQuery(query);
            }
        }

        public void ExecuteNonQuery(string query)
        {
            using (DbCommand command = CreateCommand(query))
                command.ExecuteNonQuery();
        }

        public Type GetConnectionType()
        {
            return connection.GetType();
        }

        // implemented by derived class

        protected DbConnection connection;      // the connection
        protected Thread owningThread;          // tracing who owns the thread to prevent cross thread ops
        public string DBFile { get; protected set; }    // the File name
        protected static List<SQLExtConnection> openConnections = new List<SQLExtConnection>(); // debugging to track connections
        private bool hasbeendisposed = false;
    }
}
