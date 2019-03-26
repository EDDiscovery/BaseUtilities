using SQLLiteExtensions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousCore.SystemDB
{
    public class SQLiteConnectionSystem : SQLExtConnectionWithLockRegister<SQLiteConnectionSystem>
    {
        static public string dbFile = @"c:\code\EDSM\edsm.sql";

        public SQLiteConnectionSystem() : base(dbFile, false, Initialize, AccessMode.ReaderWriter)
        {
        }

        public SQLiteConnectionSystem(AccessMode mode = AccessMode.ReaderWriter) : base(dbFile, false, Initialize, mode)
        {
        }

        private SQLiteConnectionSystem(bool utc, Action init) : base(dbFile, utc, init, AccessMode.ReaderWriter)
        {
        }

        public static void Initialize()
        {
            InitializeIfNeeded(() =>
            {
                using (SQLiteConnectionSystem conn = new SQLiteConnectionSystem(false, null))       // use this special one so we don't get double init.
                {
                    System.Diagnostics.Debug.WriteLine("Initialise EDSM DB");
                    UpgradeSystemsDB(conn);
                }
            });
        }

        protected static bool UpgradeSystemsDB(SQLiteConnectionSystem conn)
        {
            int dbver;
            try
            {
                conn.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS Register (ID TEXT PRIMARY KEY NOT NULL, ValueInt INTEGER, ValueDouble DOUBLE, ValueString TEXT, ValueBlob BLOB)");

                SQLExtRegister reg = new SQLExtRegister(conn);

                dbver = reg.GetSettingInt("DBVer", 0);        // use the constring one, as don't want to go back into ConnectionString code

                if (dbver < 100)
                    UpdateDB100(conn);

                CreateSystemDBTableIndexes(conn);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpgradeSystemsDB error: " + ex.Message + Environment.NewLine + ex.StackTrace);
                return false;
            }
        }

        private static void UpdateDB100(SQLExtConnection conn)
        {
            CreateEDSMTables(conn, 100, "");
            conn.ExecuteNonQueries(new string[] 
                {
                    "DROP TABLE IF EXISTS EDDB",
                    "CREATE TABLE EDDB (edsmid INTEGER PRIMARY KEY NOT NULL UNIQUE, eddbupdatedat INTEGER, properties TEXT)"
                });
        }


        private static void CreateEDSMTables(SQLExtConnection conn, int dbver, string postfix)
        {
            string[] queries = new []
            {
                "DROP TABLE IF EXISTS Sectors" + postfix,
                "DROP TABLE IF EXISTS Systems" + postfix,
                "DROP TABLE IF EXISTS Names" + postfix,
                "CREATE TABLE Sectors" + postfix + " (id INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL  UNIQUE , gridid INTEGER, name TEXT)",
                "CREATE TABLE Systems" + postfix + " (id INTEGER PRIMARY KEY NOT NULL UNIQUE , sector INTEGER, name INTEGER, x INTEGER, y INTEGER, z INTEGER, edsmid INTEGER )",
                "CREATE TABLE Names" + postfix + " (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL  UNIQUE , Name TEXT NOT NULL )",
            };

            conn.PerformUpgrade(dbver, false, false, queries);
        }

        private static void CreateEDDBTables(SQLExtConnection conn, int dbver, string postfix)
        {
            string[] queries = new[]
            {
                "DROP TABLE IF EXISTS Sectors" + postfix,
                "DROP TABLE IF EXISTS Systems" + postfix,
                "DROP TABLE IF EXISTS Names" + postfix,
                "CREATE TABLE Sectors" + postfix + " (id INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL  UNIQUE , gridid INTEGER, name TEXT)",
                "CREATE TABLE Systems" + postfix + " (id INTEGER PRIMARY KEY NOT NULL UNIQUE , sector INTEGER, name INTEGER, x INTEGER, y INTEGER, z INTEGER, edsmid INTEGER )",
                "CREATE TABLE Names" + postfix + " (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL  UNIQUE , Name TEXT NOT NULL )",
            };

            conn.PerformUpgrade(dbver, false, false, queries);
        }

        public static void DropSystemsTableIndexes()        // PERFORM during full table replacement
        {
            //string[] queries = new[]
            //{
            //    "DROP INDEX IF EXISTS SystemsIndex",
            //};
            //using (SQLiteConnectionSystem conn = new SQLiteConnectionSystem())
            //{
            //    foreach (string query in queries)
            //    {
            //        using (DbCommand cmd = conn.CreateCommand(query))
            //        {
            //            cmd.ExecuteNonQuery();
            //        }
            //    }
            //}
        }

        private static void CreateSystemDBTableIndexes(SQLiteConnectionSystem conn)     // UPGRADE
        {
            //string[] queries = new[]
            //{
            //    "CREATE UNIQUE INDEX IF NOT EXISTS SystemAliases_id_edsm ON SystemAliases (id_edsm)",
            //    "CREATE INDEX IF NOT EXISTS SystemAliases_name ON SystemAliases (name)",
            //};

            //foreach (string query in queries)
            //{
            //    using (DbCommand cmd = conn.CreateCommand(query))
            //    {
            //        cmd.ExecuteNonQuery();
            //    }
            //}
        }
    }
}
