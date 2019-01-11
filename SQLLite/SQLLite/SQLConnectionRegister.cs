using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SQLLiteExtensions
{
    public class RegisterEntry
    {
        public string ValueString { get; private set; }
        public long ValueInt { get; private set; }
        public double ValueDouble { get; private set; }
        public byte[] ValueBlob { get; private set; }

        protected RegisterEntry()
        {
        }

        public RegisterEntry(string stringval = null, byte[] blobval = null, long intval = 0, double floatval = Double.NaN)
        {
            ValueString = stringval;
            ValueBlob = blobval;
            ValueInt = intval;
            ValueDouble = floatval;
        }
    }


    public abstract class SQLExtConnectionWithRegister<TConn> : SQLExtConnection where TConn : SQLExtConnection, new()
    {
        public static bool IsReadWaiting
        {
            get
            {
                return SQLExtTransactionLock<TConn>.IsReadWaiting;
            }
        }

        public static bool IsInitialized { get { return _initialized; } }

        private static ReaderWriterLockSlim _schemaLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private static bool _initialized = false;
        private static int _initsem = 0;
        private static ManualResetEvent _initbarrier = new ManualResetEvent(false);
        private SQLExtTransactionLock<TConn> _transactionLock;
        protected static Dictionary<string, RegisterEntry> EarlyRegister;

        public class SchemaLock : IDisposable
        {
            public SchemaLock()
            {
                if (_schemaLock.RecursiveReadCount != 0)
                {
                    throw new InvalidOperationException("Cannot take a schema lock while holding an open database connection");
                }

                _schemaLock.EnterWriteLock();
            }

            public void Dispose()
            {
                if (_schemaLock.IsWriteLockHeld)
                {
                    _schemaLock.ExitWriteLock();
                }
            }
        }

        protected static void InitializeIfNeeded(Action initializer)
        {
            if (!_initialized)
            {
                int cur = Interlocked.Increment(ref _initsem);

                if (cur == 1)
                {
                    using (var slock = new SchemaLock())
                    {
                        _initbarrier.Set();
                        initializer();
                        _initialized = true;
                    }
                }

                if (!_initialized)
                {
                    _initbarrier.WaitOne();
                }
            }
        }

        public SQLExtConnectionWithRegister(string dbfile, Action initialiser, bool utctimeindicator = false, bool initializing = false, bool shortlived = true)
            : base(initializing)
        {
            bool locktaken = false;
            try
            {
                if (!initializing && !_initialized)
                {
                    System.Diagnostics.Trace.WriteLine($"Database {typeof(TConn).Name} initialized before Initialize()");
                    System.Diagnostics.Trace.WriteLine(new System.Diagnostics.StackTrace(2, true).ToString());

                    initialiser();  // call back up to initialise
                }

                _schemaLock.EnterReadLock();
                locktaken = true;

                // System.Threading.Monitor.Enter(monitor);
                DBFile = dbfile;
                _cn = DbFactory.CreateConnection();

                // Use the database selected by maindb as the 'main' database
                _cn.ConnectionString = "Data Source=" + DBFile.Replace("\\", "\\\\") + ";Pooling=true;";

                if (utctimeindicator)   // indicate treat dates as UTC.
                    _cn.ConnectionString += "DateTimeKind=Utc;";

                _transactionLock = new SQLExtTransactionLock<TConn>();
                _cn.Open();
            }
            catch
            {
                if (_transactionLock != null)
                {
                    _transactionLock.Dispose();
                }

                if (locktaken)
                {
                    _schemaLock.ExitReadLock();
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
            DbCommand cmd = _cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 30;
            cmd.CommandText = query;
            return new SQLExtCommand<TConn>(cmd, this, _transactionLock, tn);
        }

        public override DbTransaction BeginTransaction(IsolationLevel isolevel)
        {
            // Take the transaction lock before beginning the
            // transaction to avoid a deadlock
            AssertThreadOwner();
            _transactionLock.OpenWriter();
            return new SQLExtTransaction<TConn>(_cn.BeginTransaction(isolevel), _transactionLock);
        }

        public override DbTransaction BeginTransaction()
        {
            // Take the transaction lock before beginning the
            // transaction to avoid a deadlock
            AssertThreadOwner();
            _transactionLock.OpenWriter();
            return new SQLExtTransaction<TConn>(_cn.BeginTransaction(), _transactionLock);
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
                if (_cn != null)
                {
                    _cn.Close();
                    _cn.Dispose();
                    _cn = null;
                }

                if (_schemaLock.IsReadLockHeld)
                {
                    _schemaLock.ExitReadLock();
                }

                if (_transactionLock != null)
                {
                    _transactionLock.Dispose();
                    _transactionLock = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Settings
        ///----------------------------
        /// STATIC functions for discrete values
        /// 


        protected static T RegisterGet<T>(string key, T defval, Func<RegisterEntry, T> early, Func<TConn, T> normal, TConn conn)
        {
            if (conn != null)
            {
                return normal(conn);
            }

            if (!_initialized && EarlyRegister != null)
            {
                return EarlyRegister.ContainsKey(key) ? early(EarlyRegister[key]) : defval;
            }
            else
            {
                if (!_initialized && !_schemaLock.IsWriteLockHeld)
                {
                    throw new InvalidOperationException("Read from register before EarlyReadRegister()");
                }

                using (TConn cn = new TConn())
                {
                    return normal(cn);
                }
            }
        }

        protected static bool RegisterPut(Func<TConn, bool> action, TConn conn)
        {
            if (conn != null)
            {
                return action(conn);
            }

            if (!_initialized && !_schemaLock.IsWriteLockHeld)
            {
                System.Diagnostics.Trace.WriteLine("Write to register before Initialize()");
            }

            using (TConn cn = new TConn())
            {
                return action(cn);
            }
        }

        protected static bool RegisterDelete(string key, Func<TConn, bool> action, TConn conn)
        {
            if (conn != null)
            {
                return action(conn);
            }

            if (!_initialized && EarlyRegister != null)
            {
                EarlyRegister.Remove(key);
                return true;
            }
            else
            {
                if (!_initialized && !_schemaLock.IsWriteLockHeld)
                {
                    System.Diagnostics.Trace.WriteLine("Delete register before Initialize()");
                }

                using (TConn cn = new TConn())
                {
                    return action(cn);
                }
            }
        }

        static public bool keyExists(string sKey, TConn conn = null)
        {
            return RegisterGet(sKey, false, r => true, cn => cn.keyExistsCN(sKey), conn);
        }

        static public bool DeleteKey(string key, TConn conn = null)
        {
            return RegisterDelete(key, cn => cn.DeleteKeyCN(key), conn);
        }

        static public int GetSettingInt(string key, int defaultvalue, TConn conn = null)
        {
            return (int)RegisterGet(key, defaultvalue, r => r.ValueInt, cn => cn.GetSettingIntCN(key, defaultvalue), conn);
        }

        static public bool PutSettingInt(string key, int intvalue, TConn conn = null)
        {
            return RegisterPut(cn => cn.PutSettingIntCN(key, intvalue), conn);
        }

        static public double GetSettingDouble(string key, double defaultvalue, TConn conn = null)
        {
            return RegisterGet(key, defaultvalue, r => r.ValueDouble, cn => cn.GetSettingDoubleCN(key, defaultvalue), conn);
        }

        static public bool PutSettingDouble(string key, double doublevalue, TConn conn = null)
        {
            return RegisterPut(cn => cn.PutSettingDoubleCN(key, doublevalue), conn);
        }

        static public bool GetSettingBool(string key, bool defaultvalue, TConn conn = null)
        {
            return RegisterGet(key, defaultvalue, r => r.ValueInt != 0, cn => cn.GetSettingBoolCN(key, defaultvalue), conn);
        }

        static public bool PutSettingBool(string key, bool boolvalue, TConn conn = null)
        {
            return RegisterPut(cn => cn.PutSettingBoolCN(key, boolvalue), conn);
        }

        static public string GetSettingString(string key, string defaultvalue, TConn conn = null)
        {
            return RegisterGet(key, defaultvalue, r => r.ValueString, cn => cn.GetSettingStringCN(key, defaultvalue), conn);
        }

        static public bool PutSettingString(string key, string strvalue, TConn conn = null)        // public IF
        {
            return RegisterPut(cn => cn.PutSettingStringCN(key, strvalue), conn);
        }

        static public DateTime GetSettingDate(string key, DateTime defaultvalue, TConn conn = null)
        {
            string s = RegisterGet(key, "--", r => r.ValueString, cn => cn.GetSettingStringCN(key, "--"), conn);

            if (!DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime date))
            {
                date = defaultvalue;
            }

            return date;
        }

        static public bool PutSettingDate(string key, DateTime value, TConn conn = null)        // public IF
        {
            return RegisterPut(cn => cn.PutSettingStringCN(key, value.ToStringZulu()), conn);
        }

        protected void GetRegister(Dictionary<string, RegisterEntry> regs)
        {
            using (DbCommand cmd = CreateCommand("SELECT Id, ValueInt, ValueDouble, ValueBlob, ValueString FROM register"))
            {
                using (DbDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        string id = (string)rdr["Id"];
                        object valint = rdr["ValueInt"];
                        object valdbl = rdr["ValueDouble"];
                        object valblob = rdr["ValueBlob"];
                        object valstr = rdr["ValueString"];
                        regs[id] = new RegisterEntry(
                            valstr as string,
                            valblob as byte[],
                            (valint as long?) ?? 0L,
                            (valdbl as double?) ?? Double.NaN
                        );
                    }
                }
            }
        }





        #endregion
    }

}
