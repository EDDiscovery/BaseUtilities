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
    public abstract class SQLExtConnection : IDisposable              // USE this for connections.. 
    {
        //static Object monitor = new Object();                 // monitor disabled for now - it will prevent SQLite DB locked errors but 
        // causes the program to become unresponsive during big DB updates
        protected DbConnection _cn;
        protected Thread _owningThread;
        protected static List<SQLExtConnection> _openConnections = new List<SQLExtConnection>();
        protected static DbProviderFactory DbFactory = GetSqliteProviderFactory();

        protected SQLExtConnection(bool initializing)
        {
            lock (_openConnections)
            {
                _openConnections.Add(this);
            }
            _owningThread = Thread.CurrentThread;
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
            if (Thread.CurrentThread != _owningThread)
            {
                throw new InvalidOperationException($"DB connection was passed between threads.  Owning thread: {_owningThread.Name} ({_owningThread.ManagedThreadId}); this thread: {Thread.CurrentThread.Name} ({Thread.CurrentThread.ManagedThreadId})");
            }
        }

        public string DBFile { get; protected set; }

        protected static void PerformUpgrade(SQLExtConnection conn, int newVersion, bool catchErrors, bool backupDbFile, string[] queries, Action doAfterQueries = null)
        {
            if (backupDbFile)
            {
                string dbfile = conn.DBFile;

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
                    ExecuteQuery(conn, query);
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

            conn.PutSettingIntCN("DBVer", newVersion);
        }

        protected static void ExecuteQuery(SQLExtConnection conn, string query)
        {
            conn.ExecuteQuery(query);
        }

        public void ExecuteQuery(string query)
        {
            using (DbCommand command = CreateCommand(query))
                command.ExecuteNonQuery();
        }

        public abstract DbCommand CreateCommand(string cmd, DbTransaction tn = null);
        public abstract DbTransaction BeginTransaction(IsolationLevel isolevel);
        public abstract DbTransaction BeginTransaction();
        public abstract void Dispose();
        public abstract DbDataAdapter CreateDataAdapter(DbCommand cmd);

        protected virtual void Dispose(bool disposing)
        {
            lock (_openConnections)
            {
                SQLExtConnection._openConnections.Remove(this);
            }
        }

        public bool keyExistsCN(string sKey)
        {
            using (DbCommand cmd = CreateCommand("select ID from Register WHERE ID=@key"))
            {
                cmd.AddParameterWithValue("@key", sKey);
                return cmd.ExecuteScalar() != null;
            }
        }

        public bool DeleteKeyCN(string sKey)        // SQL wildcards
        {
            using (DbCommand cmd = CreateCommand("Delete from Register WHERE ID like @key"))
            {
                cmd.AddParameterWithValue("@key", sKey);
                return cmd.ExecuteScalar() != null;
            }
        }

        public int GetSettingIntCN(string key, int defaultvalue)
        {
            try
            {
                using (DbCommand cmd = CreateCommand("SELECT ValueInt from Register WHERE ID = @ID"))
                {
                    cmd.AddParameterWithValue("@ID", key);

                    object ob = cmd.ExecuteScalar();

                    if (ob == null || ob == DBNull.Value)
                        return defaultvalue;

                    int val = Convert.ToInt32(ob);

                    return val;
                }
            }
            catch
            {
                return defaultvalue;
            }
        }

        public bool PutSettingIntCN(string key, int intvalue)
        {
            try
            {
                if (keyExistsCN(key))
                {
                    using (DbCommand cmd = CreateCommand("Update Register set ValueInt = @ValueInt Where ID=@ID"))
                    {
                        cmd.AddParameterWithValue("@ID", key);
                        cmd.AddParameterWithValue("@ValueInt", intvalue);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
                else
                {
                    using (DbCommand cmd = CreateCommand("Insert into Register (ID, ValueInt) values (@ID, @valint)"))
                    {
                        cmd.AddParameterWithValue("@ID", key);
                        cmd.AddParameterWithValue("@valint", intvalue);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public double GetSettingDoubleCN(string key, double defaultvalue)
        {
            try
            {
                using (DbCommand cmd = CreateCommand("SELECT ValueDouble from Register WHERE ID = @ID"))
                {
                    cmd.AddParameterWithValue("@ID", key);

                    object ob = cmd.ExecuteScalar();

                    if (ob == null || ob == DBNull.Value)
                        return defaultvalue;

                    double val = Convert.ToDouble(ob);

                    return val;
                }
            }
            catch
            {
                return defaultvalue;
            }
        }

        public bool PutSettingDoubleCN(string key, double doublevalue)
        {
            try
            {
                if (keyExistsCN(key))
                {
                    using (DbCommand cmd = CreateCommand("Update Register set ValueDouble = @ValueDouble Where ID=@ID"))
                    {
                        cmd.AddParameterWithValue("@ID", key);
                        cmd.AddParameterWithValue("@ValueDouble", doublevalue);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
                else
                {
                    using (DbCommand cmd = CreateCommand("Insert into Register (ID, ValueDouble) values (@ID, @valdbl)"))
                    {
                        cmd.AddParameterWithValue("@ID", key);
                        cmd.AddParameterWithValue("@valdbl", doublevalue);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public bool GetSettingBoolCN(string key, bool defaultvalue)
        {
            return GetSettingIntCN(key, defaultvalue ? 1 : 0) != 0;
        }

        public bool PutSettingBoolCN(string key, bool boolvalue)
        {
            return PutSettingIntCN(key, boolvalue ? 1 : 0);
        }

        public string GetSettingStringCN(string key, string defaultvalue)
        {
            try
            {
                using (DbCommand cmd = CreateCommand("SELECT ValueString from Register WHERE ID = @ID"))
                {
                    cmd.AddParameterWithValue("@ID", key);
                    object ob = cmd.ExecuteScalar();

                    if (ob == null || ob == DBNull.Value)
                        return defaultvalue;

                    string val = (string)ob;

                    return val;
                }
            }
            catch
            {
                return defaultvalue;
            }
        }

        public bool PutSettingStringCN(string key, string strvalue)
        {
            try
            {
                if (keyExistsCN(key))
                {
                    using (DbCommand cmd = CreateCommand("Update Register set ValueString = @ValueString Where ID=@ID"))
                    {
                        cmd.AddParameterWithValue("@ID", key);
                        cmd.AddParameterWithValue("@ValueString", strvalue);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
                else
                {
                    using (DbCommand cmd = CreateCommand("Insert into Register (ID, ValueString) values (@ID, @valint)"))
                    {
                        cmd.AddParameterWithValue("@ID", key);
                        cmd.AddParameterWithValue("@valint", strvalue);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
