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
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace SQLLiteExtensions
{
    // Builds on basic SQLExtConnection and add schema lock
    // provides initialiser locking as well

    public abstract class SQLExtConnectionWithLock<TConn> : SQLExtConnection where TConn : SQLExtConnection, new()
    {
        public static bool IsReadWaiting => SQLExtTransactionLock<TConn>.IsReadWaiting;
        public static bool IsInitialized => initialized;
        public static bool IsSchemaLocked => schemaLock.IsWriteLockHeld;

        private static ReaderWriterLockSlim schemaLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private static ManualResetEvent initbarrier = new ManualResetEvent(false);
        private SQLExtTransactionLock<TConn> transactionLock;
        private static ThreadLocal<Dictionary<Guid, Tuple<WeakReference<SQLExtConnection>, StackTrace>>> threadConnection = new ThreadLocal<Dictionary<Guid, Tuple<WeakReference<SQLExtConnection>, StackTrace>>>(() => new Dictionary<Guid, Tuple<WeakReference<SQLExtConnection>, StackTrace>>());
        private Guid ConnectionGuid = Guid.NewGuid();

        private static bool initialized = false;
        private static int initsem = 0;

        public SQLExtConnectionWithLock(string dbfile, bool utctimeindicator, Action initializercallback = null, AccessMode mode = AccessMode.ReaderWriter)  : base(mode)
        {
            bool locktaken = false;
            try
            {
                if (initializercallback != null && !IsInitialized)
                {
                    System.Diagnostics.Trace.WriteLine($"Database {typeof(TConn).Name} initialized before Initialize()");
                    System.Diagnostics.Trace.WriteLine(new System.Diagnostics.StackTrace(2, true).ToString());

                    initializercallback();  // call back up to initialise
                }

                var threadconns = threadConnection.Value;
                var stacktrace = new StackTrace(1, true);

                if (threadConnection.Value.Count != 0)
                {
                    Trace.WriteLine($"ERROR: Connection opened twice, expect deadlock");
                    foreach (var kvp in threadconns)
                    {
                        if (kvp.Value.Item1.TryGetTarget(out _))
                        {
                            Trace.WriteLine($"Original connection opened {kvp.Value.Item2.ToString()}");
                        }
                        else
                        {
                            Trace.WriteLine($"Leaked original connection opened {kvp.Value.Item2.ToString()}");
                        }
                    }
                    Trace.WriteLine($"New connection opened {stacktrace.ToString()}");

                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }

                threadconns[this.ConnectionGuid] = new Tuple<WeakReference<SQLExtConnection>, StackTrace>(new WeakReference<SQLExtConnection>(this, false), stacktrace);

                schemaLock.EnterReadLock();
                locktaken = true;

                // System.Threading.Monitor.Enter(monitor);
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

                transactionLock = new SQLExtTransactionLock<TConn>();
                connection.Open();
            }
            catch
            {
                if (transactionLock != null)
                {
                    transactionLock.Dispose();
                }

                if (locktaken)
                {
                    schemaLock.ExitReadLock();
                }
                throw;
            }
        }

        public override DbDataAdapter CreateDataAdapter(DbCommand cmd)
        {
            DbDataAdapter da = DbFactory.CreateDataAdapter();
            da.SelectCommand = cmd;
            return da;
        }

        public override DbCommand CreateCommand(string query, DbTransaction tn = null)
        {
            AssertThreadOwner();
            DbCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 30;
            cmd.CommandText = query;
            return new SQLExtCommand<TConn>(cmd, this, transactionLock, tn);
        }

        public override DbTransaction BeginTransaction(IsolationLevel isolevel)
        {
            // Take the transaction lock before beginning the
            // transaction to avoid a deadlock
            AssertThreadOwner();
            transactionLock.OpenWriter();
            return new SQLExtTransaction<TConn>(connection.BeginTransaction(isolevel), transactionLock);
        }

        public override DbTransaction BeginTransaction()
        {
            // Take the transaction lock before beginning the
            // transaction to avoid a deadlock
            AssertThreadOwner();
            transactionLock.OpenWriter();
            return new SQLExtTransaction<TConn>(connection.BeginTransaction(), transactionLock);
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
                if (threadConnection.Value.ContainsKey(this.ConnectionGuid))
                {
                    threadConnection.Value.Remove(this.ConnectionGuid);
                }

                if (connection != null)
                {
                 //   System.Diagnostics.Debug.WriteLine("Closed connection " + connection.ConnectionString);
                    connection.Close();
                    connection.Dispose();
                    connection = null;
                }

                if (schemaLock.IsReadLockHeld)
                {
                    schemaLock.ExitReadLock();
                }

                if (transactionLock != null)
                {
                    transactionLock.Dispose();
                    transactionLock = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Intialisation Lock

        public class SchemaLock : IDisposable
        {
            public SchemaLock()
            {
                if (schemaLock.RecursiveReadCount != 0)
                {
                    throw new InvalidOperationException("Cannot take a schema lock while holding an open database connection");
                }

                schemaLock.EnterWriteLock();
            }

            public void Dispose()
            {
                if (schemaLock.IsWriteLockHeld)
                {
                    schemaLock.ExitWriteLock();
                }
            }
        }

        protected static void InitializeIfNeeded(Action initializer)        // call this to get your initialiser called and to let it know its been initialised.
        {
            if (!initialized)
            {
                int cur = Interlocked.Increment(ref initsem);

                if (cur == 1)
                {
                    using (var slock = new SchemaLock())
                    {
                        initbarrier.Set();
                        initialized = true;     // stop any call backs thru this causing it to see it again
                        initializer();
                    }
                }

                if (!initialized)
                {
                    initbarrier.WaitOne();
                }
            }
        }

        #endregion

    }
}
