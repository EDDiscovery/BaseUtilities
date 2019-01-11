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
using System.Data.Common;
using System.Diagnostics;
using System.Threading;

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
                return rwlock.IsWriteLockHeld && readsWaiting > 0;
            }
        }
        private static ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private static SQLExtTransactionLock<TConn> writeLockOwner;
        private static int readsWaiting;
        private Thread owningThread;
        public DbCommand executingCommand;
        public bool commandExecuting = false;
        private bool isLongRunning = false;
        private string commandText = null;
        private bool longRunningLogged = false;
        private bool isWriter = false;
        private bool isReader = false;

        #region Constructor and Destructor
        public SQLExtTransactionLock()
        {
            owningThread = Thread.CurrentThread;
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
                txnlock.isLongRunning = true;

                if (txnlock.commandExecuting)
                {
                    if (txnlock.isLongRunning)
                    {
                        Trace.WriteLine($"{Environment.TickCount % 10000} The following command is taking a long time to execute:\n{txnlock.commandText}");
                    }
                    if (txnlock.owningThread == Thread.CurrentThread)
                    {
                        StackTrace trace = new StackTrace(1, true);
                        Trace.WriteLine(trace.ToString());
                    }
                }
                else
                {
                    Trace.WriteLine($"{Environment.TickCount % 10000} The transaction lock has been held for a long time.");

                    if (txnlock.commandText != null)
                    {
                        Trace.WriteLine($"{Environment.TickCount % 10000} Last command to execute:\n{txnlock.commandText}");
                    }
                }
            }
        }

        public void BeginCommand(DbCommand cmd)
        {
            this.executingCommand = cmd;
            this.commandText = cmd.CommandText;
            this.commandExecuting = true;

            if (this.isLongRunning && !this.longRunningLogged)
            {
                this.isLongRunning = false;
                DebugLongRunningOperation(this);
                this.longRunningLogged = true;
            }
        }

        public void EndCommand()
        {
            this.commandExecuting = false;
        }

        public void OpenReader()
        {
            if (owningThread != Thread.CurrentThread)
            {
                throw new InvalidOperationException("Transaction lock passed between threads");
            }

            if (!rwlock.IsWriteLockHeld)
            {
                if (!isReader)
                {
                    try
                    {
                        Interlocked.Increment(ref readsWaiting);
                        bool warned = false;
                        while (!rwlock.TryEnterReadLock(1000))
                        {
                            SQLExtTransactionLock<TConn> lockowner = writeLockOwner;
                            if (lockowner != null)
                            {
                                warned = true;
                                Trace.WriteLine($"{Environment.TickCount % 10000} Thread {Thread.CurrentThread.Name} waiting for thread {lockowner.owningThread.Name} to finish writer");
                                DebugLongRunningOperation(lockowner);
                            }
                        }

                        if (warned)
                            Trace.WriteLine($"{Environment.TickCount % 10000} Thread {Thread.CurrentThread.Name} Released for read");

                        isReader = true;
                    }
                    finally
                    {
                        Interlocked.Decrement(ref readsWaiting);
                    }
                }
            }
        }

        public void OpenWriter()
        {
            if (owningThread != Thread.CurrentThread)
            {
                throw new InvalidOperationException("Transaction lock passed between threads");
            }

            if (rwlock.IsReadLockHeld)
            {
                throw new InvalidOperationException("Write attempted in read-only connection");
            }

            if (!isWriter)
            {
                try
                {
                    if (!rwlock.IsUpgradeableReadLockHeld)
                    {
                        bool warned = false;

                        while (!rwlock.TryEnterUpgradeableReadLock(1000))
                        {
                            SQLExtTransactionLock<TConn> lockowner = writeLockOwner;
                            if (lockowner != null)
                            {
                                warned = true;
                                Trace.WriteLine($"{Environment.TickCount % 10000} Thread {Thread.CurrentThread.Name} waiting for thread {lockowner.owningThread.Name} to finish writer");
                                DebugLongRunningOperation(lockowner);
                            }
                        }

                        if (warned)
                            Trace.WriteLine($"{Environment.TickCount % 10000} Thread {Thread.CurrentThread.Name} Released for write");

                        isWriter = true;
                        writeLockOwner = this;
                    }

                    while (!rwlock.TryEnterWriteLock(1000))
                    {
                        Trace.WriteLine($"{Environment.TickCount % 10000}Thread {Thread.CurrentThread.Name} waiting for readers to finish");
                    }
                }
                catch
                {
                    if (isWriter)
                    {
                        if (rwlock.IsWriteLockHeld)
                        {
                            rwlock.ExitWriteLock();
                        }

                        if (rwlock.IsUpgradeableReadLockHeld)
                        {
                            rwlock.ExitUpgradeableReadLock();
                        }
                    }
                }
            }
        }

        public void CloseWriter()
        {
            if (rwlock.IsWriteLockHeld)
            {
                rwlock.ExitWriteLock();

                if (!rwlock.IsWriteLockHeld && rwlock.IsUpgradeableReadLockHeld)
                {
                    rwlock.ExitUpgradeableReadLock();
                }
            }
        }

        public void CloseReader()
        {
            if (rwlock.IsReadLockHeld)
            {
                rwlock.ExitReadLock();
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
            if (owningThread != Thread.CurrentThread)
            {
                Trace.WriteLine("ERROR: Transaction lock leaked");
            }
            else
            {
                if (isWriter)
                {
                    CloseWriter();
                }
                else if (isReader)
                {
                    CloseReader();
                }
            }
        }
        #endregion
    }


}
