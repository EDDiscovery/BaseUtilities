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
using System.Diagnostics;
using System.Threading;

namespace SQLLiteExtensions
{
    // This class wraps a DbCommand so it can take the
    // transaction wrapper, and to work around
    // SQLite not using a monitor or mutex when locking
    // the database
    public class SQLExtCommand<TConn> : DbCommand where TConn : SQLExtConnection
    {
        // Overridden methods and properties passed to inner command

        public override string CommandText { get { return InnerCommand.CommandText; } set { InnerCommand.CommandText = value; } }
        public override int CommandTimeout { get { return InnerCommand.CommandTimeout; } set { InnerCommand.CommandTimeout = value; } }
        public override CommandType CommandType { get { return InnerCommand.CommandType; } set { InnerCommand.CommandType = value; } }
        protected override DbConnection DbConnection { get { return InnerCommand.Connection; } set { throw new InvalidOperationException("Cannot change connection of command"); } }
        protected override DbParameterCollection DbParameterCollection { get { return InnerCommand.Parameters; } }
        public override bool DesignTimeVisible { get { return InnerCommand.DesignTimeVisible; } set { InnerCommand.DesignTimeVisible = value; } }
        public override UpdateRowSource UpdatedRowSource { get { return InnerCommand.UpdatedRowSource; } set { InnerCommand.UpdatedRowSource = value; } }
        protected override DbParameter CreateDbParameter() { return InnerCommand.CreateParameter(); }
        public override void Cancel() { InnerCommand.Cancel(); }
        public override void Prepare() { InnerCommand.Prepare(); }

        // transaction

        protected override DbTransaction DbTransaction { get { return transaction; } set { SetTransaction(value); } }

        // interface

        public SQLExtCommand(DbCommand cmd, SQLExtConnection conn, SQLExtTransactionLock<TConn> txnlock, DbTransaction txn = null)
        {
            connection = conn;
            this.txnlock = txnlock;
            InnerCommand = cmd;
            if (txn != null)
            {
                SetTransaction(txn);
            }
        }

        // implementation

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            int retries = 3;

            while (true)
            {
                txnlock.OpenReader();
                try
                {
                    return new SQLExtDataReader<TConn>(this.InnerCommand, behavior, txnlock: txnlock);
                }
                catch (Exception ex)
                {
                    txnlock.CloseReader();

                    if (retries > 0)
                    {
                        var sqlex = ex as System.Data.SQLite.SQLiteException;

                        if (sqlex != null && sqlex.ErrorCode == 5) // Database is locked
                        {
                            Trace.WriteLine($"Database locked - retrying\n{ex.StackTrace}");
                            Thread.Sleep(100);
                            retries--;
                            continue;
                        }
                    }
                    System.Diagnostics.Debug.WriteLine("SQL Command: " + this.InnerCommand.CommandText);
                    throw;
                }
            }
        }

        public override object ExecuteScalar()
        {
            try
            {
                txnlock.OpenReader();
                txnlock.BeginCommand(this);
                return InnerCommand.ExecuteScalar();
            }
            finally
            {
                txnlock.EndCommand();
                txnlock.CloseReader();
            }
        }

        public override int ExecuteNonQuery()
        {
            if (transaction != null)
            {
                try
                {
                    transaction.BeginCommand(this);
                    return InnerCommand.ExecuteNonQuery();
                }
                finally
                {
                    transaction.EndCommand();
                }
            }
            else
            {
                try
                {
                    txnlock.OpenWriter();
                    txnlock.BeginCommand(this);
                    return InnerCommand.ExecuteNonQuery();
                }
                finally
                {
                    txnlock.EndCommand();
                    txnlock.CloseWriter();
                }
            }
        }

        // disposing: true if Dispose() was called, false
        // if being finalized by the garbage collector
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (InnerCommand != null)
                {
                    InnerCommand.Dispose();
                    InnerCommand = null;
                }

                hasbeendisposed = true;
            }

            base.Dispose(disposing);
        }

#if DEBUG
        //since finalisers impose a penalty, we shall check only in debug mode
        ~SQLExtCommand()
        {
            if (!hasbeendisposed)       // finalisation may come very late.. not immediately as its done on garbage collection.  Warn by message and assert.
            {
                System.Diagnostics.Debug.Assert(hasbeendisposed, "Missing dispose for command " + this.CommandText);       // must have been disposed
            }
        }
#endif

        protected void SetTransaction(DbTransaction txn)
        {
            // We only accept wrapped transactions in order to avoid deadlocks
            if (txn == null || txn is SQLExtTransaction<TConn>)
            {
                transaction = (SQLExtTransaction<TConn>)txn;
                InnerCommand.Transaction = transaction.InnerTransaction;
            }
            else
            {
                throw new InvalidOperationException(String.Format("Expected a {0}; got a {1}", typeof(SQLExtTransaction<TConn>).FullName, txn.GetType().FullName));
            }
        }


        protected SQLExtTransaction<TConn> transaction;
        protected SQLExtConnection connection;
        protected SQLExtTransactionLock<TConn> txnlock;
        private bool hasbeendisposed = false;
        public DbCommand InnerCommand { get; set; }
    }
}

