using SQLLiteExtensions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousCore.DB
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

                conn.ExecuteNonQueries(new string[]             // always kill these old tables and make EDDB new table
                    {
                    "DROP TABLE IF EXISTS EddbSystems",
                    "DROP TABLE IF EXISTS Distances",
                    "DROP TABLE IF EXISTS Stations",
                    "DROP TABLE IF EXISTS SystemAliases",
                    "DROP TABLE IF EXISTS station_commodities",
                    "CREATE TABLE IF NOT EXISTS EDDB (edsmid INTEGER PRIMARY KEY NOT NULL UNIQUE, eddbid INTEGER, eddbupdatedat INTEGER, population INTEGER, faction TEXT, government INTEGER, allegiance INTEGER, state INTEGER, security INTEGER, primaryeconomy INTEGER, needspermit INTEGER, power TEXT, powerstate TEXT, properties TEXT)",
                    });

                CreateStarTables(conn);                     // ensure we have
                CreateSystemDBTableIndexes(conn);           // ensure they are there 

                dbver = reg.GetSettingInt("DBVer", 0);      
                if (dbver < 200)
                {
                    reg.PutSettingInt("DBVer", 200);
                    reg.DeleteKey("EDDBSystemsTime");       // force a reload of EDDB
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpgradeSystemsDB error: " + ex.Message + Environment.NewLine + ex.StackTrace);
                return false;
            }
        }

        // should have indexes present, empty new tables, full old tables..

        public static void UpgradeFrom102TypeDB(Func<bool> cancelRequested, Action<string> reportProgress )
        {
            using (SQLiteConnectionSystem conn = new SQLiteConnectionSystem(AccessMode.ReaderWriter))      // use this special one so we don't get double init.
            {
                var list = conn.Tables();

                if ( list.Contains("EdsmSystems"))
                {
                    string tablepostfix = "temp";

                    DropStarTables(conn, tablepostfix);     // just in case, kill the old tables
                    CreateStarTables(conn, tablepostfix);     // and make new temp tables

                    long updates = SystemsDB.UpgradeDB102to200(cancelRequested, reportProgress, tablepostfix);

                    if ( updates >= 0 ) // a cancel will result in -1
                    {
                        DropStarTables(conn);     // drop the main ones - this also kills the indexes

                        RenameStarTables(conn, tablepostfix, "");     // rename the temp to main ones

                        reportProgress?.Invoke("Removing old system tables");

                        conn.ExecuteNonQueries( new string[]
                        {
                            "DROP TABLE IF EXISTS EdsmSystems",
                            "DROP TABLE IF EXISTS SystemNames",
                        });

                        reportProgress?.Invoke("Shrinking database");
                        conn.Vacuum();

                        reportProgress?.Invoke("Creating indexes");
                        CreateSystemDBTableIndexes(conn);
                    }
                    else
                    {
                        DropStarTables(conn, tablepostfix);     // just in case, kill the old tables
                    }
                }
            }
        }

        private static void CreateStarTables(SQLExtConnection conn, string postfix = "")
        {
            conn.ExecuteNonQueries(new string[]
            {
                "CREATE TABLE IF NOT EXISTS Sectors" + postfix + " (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE , gridid INTEGER, name TEXT NOT NULL COLLATE NOCASE)",
                "CREATE TABLE IF NOT EXISTS Systems" + postfix + " (id INTEGER PRIMARY KEY NOT NULL UNIQUE , sector INTEGER, name INTEGER, x INTEGER, y INTEGER, z INTEGER, edsmid INTEGER )",
                "CREATE TABLE IF NOT EXISTS Names" + postfix + " (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL  UNIQUE , Name TEXT NOT NULL  COLLATE NOCASE )",
            });
        }

        private static void DropStarTables(SQLExtConnection conn, string postfix = "")
        {
            conn.ExecuteNonQueries(new string[]
            {
                "DROP TABLE IF EXISTS Sectors" + postfix,       // dropping the tables kills the indexes
                "DROP TABLE IF EXISTS Systems" + postfix,
                "DROP TABLE IF EXISTS Names" + postfix,
            });
        }

        private static void RenameStarTables(SQLExtConnection conn, string frompostfix, string topostfix)
        {
            conn.ExecuteNonQueries(new string[]
            {
                "ALTER TABLE Sectors" + frompostfix + " RENAME TO Sectors" + topostfix,       
                "ALTER TABLE Systems" + frompostfix + " RENAME TO Systems" + topostfix,       
                "ALTER TABLE Names" + frompostfix + " RENAME TO Names" + topostfix,       
            });
        }

        private static void CreateSystemDBTableIndexes(SQLiteConnectionSystem conn) 
        {
            string[] queries = new[]
            {
                 "CREATE INDEX IF NOT EXISTS SystemsName ON Systems (name)",        // on 32Msys, about 500mb cost, massive speed increase in find star
                 "CREATE INDEX IF NOT EXISTS SystemsSector ON Systems (sector)",    // on 32Msys, about 500mb cost, massive speed increase in find star

                 "CREATE INDEX IF NOT EXISTS NamesName ON Names (Name)",            // improved speed from 9038 (named)/1564 (std) to 516/446ms at minimal cost

                 "CREATE INDEX IF NOT EXISTS SectorName ON Sectors (name)",         // name - > entry
                 "CREATE INDEX IF NOT EXISTS SectorGridid ON Sectors (gridid)",     // gridid -> entry
            };

            conn.ExecuteNonQueries(queries);
        }

    }
}
