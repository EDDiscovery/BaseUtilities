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
    // This class wraps a DbCommand so it can take the
    // above transaction wrapper, and to work around
    // SQLite not using a monitor or mutex when locking
    // the database
    public class SQLExtCommand<TConn> : DbCommand where TConn : SQLExtConnection
    {
        // This is the wrapped transaction
        protected SQLExtTransaction<TConn> _transaction;
        protected SQLExtConnection _connection;
        protected SQLExtTransactionLock<TConn> _txnlock;

        public SQLExtCommand(DbCommand cmd, SQLExtConnection conn, SQLExtTransactionLock<TConn> txnlock, DbTransaction txn = null)
        {
            _connection = conn;
            _txnlock = txnlock;
            InnerCommand = cmd;
            if (txn != null)
            {
                SetTransaction(txn);
            }
        }

        public DbCommand InnerCommand { get; set; }

        #region Overridden methods and properties passed to inner command
        public override string CommandText { get { return InnerCommand.CommandText; } set { InnerCommand.CommandText = value; } }
        public override int CommandTimeout { get { return InnerCommand.CommandTimeout; } set { InnerCommand.CommandTimeout = value; } }
        public override CommandType CommandType { get { return InnerCommand.CommandType; } set { InnerCommand.CommandType = value; } }
        protected override DbConnection DbConnection { get { return InnerCommand.Connection; } set { throw new InvalidOperationException("Cannot change connection of command"); } }
        protected override DbParameterCollection DbParameterCollection { get { return InnerCommand.Parameters; } }
        protected override DbTransaction DbTransaction { get { return _transaction; } set { SetTransaction(value); } }
        public override bool DesignTimeVisible { get { return InnerCommand.DesignTimeVisible; } set { InnerCommand.DesignTimeVisible = value; } }
        public override UpdateRowSource UpdatedRowSource { get { return InnerCommand.UpdatedRowSource; } set { InnerCommand.UpdatedRowSource = value; } }

        protected override DbParameter CreateDbParameter() { return InnerCommand.CreateParameter(); }
        public override void Cancel() { InnerCommand.Cancel(); }
        public override void Prepare() { InnerCommand.Prepare(); }
        #endregion

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            int retries = 3;

            while (true)
            {
                _txnlock.OpenReader();
                try
                {
                    return new SQLExtDataReader<TConn>(this.InnerCommand, behavior, txnlock: _txnlock);
                }
                catch (Exception ex)
                {
                    _txnlock.CloseReader();

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

                    throw;
                }
            }
        }

        public override object ExecuteScalar()
        {
            try
            {
                _txnlock.OpenReader();
                _txnlock.BeginCommand(this);
                return InnerCommand.ExecuteScalar();
            }
            finally
            {
                _txnlock.EndCommand();
                _txnlock.CloseReader();
            }
        }

        public override int ExecuteNonQuery()
        {
            if (_transaction != null)
            {
                try
                {
                    _transaction.BeginCommand(this);
                    return InnerCommand.ExecuteNonQuery();
                }
                finally
                {
                    _transaction.EndCommand();
                }
            }
            else
            {
                try
                {
                    _txnlock.OpenWriter();
                    _txnlock.BeginCommand(this);
                    return InnerCommand.ExecuteNonQuery();
                }
                finally
                {
                    _txnlock.EndCommand();
                    _txnlock.CloseWriter();
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
            }

            base.Dispose(disposing);
        }

        protected void SetTransaction(DbTransaction txn)
        {
            // We only accept wrapped transactions in order to avoid deadlocks
            if (txn == null || txn is SQLExtTransaction<TConn>)
            {
                _transaction = (SQLExtTransaction<TConn>)txn;
                InnerCommand.Transaction = _transaction.InnerTransaction;
            }
            else
            {
                throw new InvalidOperationException(String.Format("Expected a {0}; got a {1}", typeof(SQLExtTransaction<TConn>).FullName, txn.GetType().FullName));
            }
        }
    }


}
