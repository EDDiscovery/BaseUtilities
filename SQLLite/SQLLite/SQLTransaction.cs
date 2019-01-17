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

using System.Data;
using System.Data.Common;

namespace SQLLiteExtensions
{
    // This class wraps a DbTransaction to work around
    // SQLite not using a monitor or mutex when locking
    // the database
    public class SQLExtTransaction<TConn> : DbTransaction where TConn : SQLExtConnection
    {
        private SQLExtTransactionLock<TConn> transactionLock = null;

        public DbTransaction InnerTransaction { get; private set; }

        public SQLExtTransaction(DbTransaction txn, SQLExtTransactionLock<TConn> txnlock)
        {
            transactionLock = txnlock;
            InnerTransaction = txn;
        }

        #region Overridden methods and properties passed to inner transaction
        protected override DbConnection DbConnection { get { return InnerTransaction.Connection; } }
        public override IsolationLevel IsolationLevel { get { return InnerTransaction.IsolationLevel; } }

        public override void Commit() { InnerTransaction.Commit(); }
        public override void Rollback() { InnerTransaction.Rollback(); }
        #endregion

        public void BeginCommand(DbCommand cmd)
        {
            transactionLock.BeginCommand(cmd);
        }

        public void EndCommand()
        {
            transactionLock.EndCommand();
        }

        // disposing: true if Dispose() was called, false
        // if being finalized by the garbage collector
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Close the transaction before closing the lock
                if (InnerTransaction != null)
                {
                    InnerTransaction.Dispose();
                    InnerTransaction = null;
                }

                if (transactionLock != null)
                {
                    transactionLock.CloseWriter();
                }
            }

            base.Dispose(disposing);
        }
    }
}
