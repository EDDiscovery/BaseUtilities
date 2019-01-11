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
    // This class uses a monitor to ensure only one can be
    // active at any one time
    public class SQLExtTransactionLock<TConn> : IDisposable where TConn : SQLExtConnection
    {
        public static bool IsReadWaiting
        {
            get
            {
                return _lock.IsWriteLockHeld && _readsWaiting > 0;
            }
        }
        private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private static SQLExtTransactionLock<TConn> _writeLockOwner;
        private static int _readsWaiting;
        private Thread _owningThread;
        public DbCommand _executingCommand;
        public bool _commandExecuting = false;
        private bool _isLongRunning = false;
        private string _commandText = null;
        private bool _longRunningLogged = false;
        private bool _isWriter = false;
        private bool _isReader = false;

        #region Constructor and Destructor
        public SQLExtTransactionLock()
        {
            _owningThread = Thread.CurrentThread;
        }

        ~SQLExtTransactionLock()
        {
            this.Dispose(false);
        }
        #endregion

        #region Opening and Disposal
        private static void DebugLongRunningOperation(object state)
        {
            WeakReference weakref = state as WeakReference;

            if (weakref != null)
            {
                SQLExtTransactionLock<TConn> txnlock = weakref.Target as SQLExtTransactionLock<TConn>;

                DebugLongRunningOperation(txnlock);
            }
        }

        private static void DebugLongRunningOperation(SQLExtTransactionLock<TConn> txnlock)
        {
            if (txnlock != null)
            {
                txnlock._isLongRunning = true;

                if (txnlock._commandExecuting)
                {
                    if (txnlock._isLongRunning)
                    {
                        Trace.WriteLine($"{Environment.TickCount % 10000} The following command is taking a long time to execute:\n{txnlock._commandText}");
                    }
                    if (txnlock._owningThread == Thread.CurrentThread)
                    {
                        StackTrace trace = new StackTrace(1, true);
                        Trace.WriteLine(trace.ToString());
                    }
                }
                else
                {
                    Trace.WriteLine($"{Environment.TickCount % 10000} The transaction lock has been held for a long time.");

                    if (txnlock._commandText != null)
                    {
                        Trace.WriteLine($"{Environment.TickCount % 10000} Last command to execute:\n{txnlock._commandText}");
                    }
                }
            }
        }

        public void BeginCommand(DbCommand cmd)
        {
            this._executingCommand = cmd;
            this._commandText = cmd.CommandText;
            this._commandExecuting = true;

            if (this._isLongRunning && !this._longRunningLogged)
            {
                this._isLongRunning = false;
                DebugLongRunningOperation(this);
                this._longRunningLogged = true;
            }
        }

        public void EndCommand()
        {
            this._commandExecuting = false;
        }

        public void OpenReader()
        {
            if (_owningThread != Thread.CurrentThread)
            {
                throw new InvalidOperationException("Transaction lock passed between threads");
            }

            if (!_lock.IsWriteLockHeld)
            {
                if (!_isReader)
                {
                    try
                    {
                        Interlocked.Increment(ref _readsWaiting);
                        bool warned = false;
                        while (!_lock.TryEnterReadLock(1000))
                        {
                            SQLExtTransactionLock<TConn> lockowner = _writeLockOwner;
                            if (lockowner != null)
                            {
                                warned = true;
                                Trace.WriteLine($"{Environment.TickCount % 10000} Thread {Thread.CurrentThread.Name} waiting for thread {lockowner._owningThread.Name} to finish writer");
                                DebugLongRunningOperation(lockowner);
                            }
                        }

                        if (warned)
                            Trace.WriteLine($"{Environment.TickCount % 10000} Thread {Thread.CurrentThread.Name} Released for read");

                        _isReader = true;
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _readsWaiting);
                    }
                }
            }
        }

        public void OpenWriter()
        {
            if (_owningThread != Thread.CurrentThread)
            {
                throw new InvalidOperationException("Transaction lock passed between threads");
            }

            if (_lock.IsReadLockHeld)
            {
                throw new InvalidOperationException("Write attempted in read-only connection");
            }

            if (!_isWriter)
            {
                try
                {
                    if (!_lock.IsUpgradeableReadLockHeld)
                    {
                        bool warned = false;

                        while (!_lock.TryEnterUpgradeableReadLock(1000))
                        {
                            SQLExtTransactionLock<TConn> lockowner = _writeLockOwner;
                            if (lockowner != null)
                            {
                                warned = true;
                                Trace.WriteLine($"{Environment.TickCount % 10000} Thread {Thread.CurrentThread.Name} waiting for thread {lockowner._owningThread.Name} to finish writer");
                                DebugLongRunningOperation(lockowner);
                            }
                        }

                        if (warned)
                            Trace.WriteLine($"{Environment.TickCount % 10000} Thread {Thread.CurrentThread.Name} Released for write");

                        _isWriter = true;
                        _writeLockOwner = this;
                    }

                    while (!_lock.TryEnterWriteLock(1000))
                    {
                        Trace.WriteLine($"{Environment.TickCount % 10000}Thread {Thread.CurrentThread.Name} waiting for readers to finish");
                    }
                }
                catch
                {
                    if (_isWriter)
                    {
                        if (_lock.IsWriteLockHeld)
                        {
                            _lock.ExitWriteLock();
                        }

                        if (_lock.IsUpgradeableReadLockHeld)
                        {
                            _lock.ExitUpgradeableReadLock();
                        }
                    }
                }
            }
        }

        public void CloseWriter()
        {
            if (_lock.IsWriteLockHeld)
            {
                _lock.ExitWriteLock();

                if (!_lock.IsWriteLockHeld && _lock.IsUpgradeableReadLockHeld)
                {
                    _lock.ExitUpgradeableReadLock();
                }
            }
        }

        public void CloseReader()
        {
            if (_lock.IsReadLockHeld)
            {
                _lock.ExitReadLock();
            }
        }

        public void Close()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // disposing: true if Dispose() was called, false
        // if being finalized by the garbage collector
        protected void Dispose(bool disposing)
        {
            if (_owningThread != Thread.CurrentThread)
            {
                Trace.WriteLine("ERROR: Transaction lock leaked");
            }
            else
            {
                if (_isWriter)
                {
                    CloseWriter();
                }
                else if (_isReader)
                {
                    CloseReader();
                }
            }
        }
        #endregion
    }


}
