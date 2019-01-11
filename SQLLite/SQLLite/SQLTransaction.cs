using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SQLLiteExtensions
{
    // This class wraps a DbTransaction to work around
    // SQLite not using a monitor or mutex when locking
    // the database
    public class SQLExtTransaction<TConn> : DbTransaction where TConn : SQLExtConnection
    {
        private SQLExtTransactionLock<TConn> _transactionLock = null;

        public DbTransaction InnerTransaction { get; private set; }

        public SQLExtTransaction(DbTransaction txn, SQLExtTransactionLock<TConn> txnlock)
        {
            _transactionLock = txnlock;
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
            _transactionLock.BeginCommand(cmd);
        }

        public void EndCommand()
        {
            _transactionLock.EndCommand();
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

                if (_transactionLock != null)
                {
                    _transactionLock.CloseWriter();
                }
            }

            base.Dispose(disposing);
        }
    }
}
