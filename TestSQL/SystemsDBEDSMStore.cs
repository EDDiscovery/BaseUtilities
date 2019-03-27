using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using SQLLiteExtensions;
using System.Data.Common;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace EliteDangerousCore.DB
{
    public partial class SystemsDB
    {
        #region Table Update

        public static long ParseEDSMJSONFile(string filename, bool[] grididallow, ref DateTime date, Func<bool> cancelRequested, Action<string> reportProgress, string tableposfix = "", bool presumeempty = false, string debugoutputfile = null)
        {
            using (StreamReader sr = new StreamReader(filename))         // read directly from file..
                return ParseEDSMJSON(sr, grididallow, ref date, cancelRequested, reportProgress, tableposfix, presumeempty, debugoutputfile);
        }

        public static long ParseEDSMJSONString(string data, bool[] grididallow, ref DateTime date, Func<bool> cancelRequested, Action<string> reportProgress, string tableposfix = "", bool presumeempty = false, string debugoutputfile = null)
        {
            using (StringReader sr = new StringReader(data))         // read directly from file..
                return ParseEDSMJSON(sr, grididallow, ref date, cancelRequested, reportProgress, tableposfix, presumeempty, debugoutputfile);
        }

        public static long ParseEDSMJSON(TextReader sr, bool[] grididallow, ref DateTime date, Func<bool> cancelRequested, Action<string> reportProgress, string tablepostfix = "", bool presumeempty = false, string debugoutputfile = null)
        {
            using (JsonTextReader jr = new JsonTextReader(sr))
                return ParseEDSMJSON(jr, grididallow, ref date, cancelRequested, reportProgress, tablepostfix, presumeempty, debugoutputfile);
        }

        // set tempostfix to use another set of tables


        public static long ParseEDSMJSON(JsonTextReader jr, 
                                        bool[] grididallowed,       // null = all, else grid bool value
                                        ref DateTime maxdate,       // updated with latest date
                                        Func<bool> cancelRequested,
                                        Action<string> reportProgress,
                                        string tablepostfix = "",        // set to add on text to table names to redirect to another table
                                        bool tablesareempty = false,     // set to presume table is empty, so we don't have to do look up queries
                                        string debugoutputfile = null
                                        )
        {
            sectoridcache = new Dictionary<long, Sector>();     
            sectornamecache = new Dictionary<string, Sector>(); 

            SQLiteConnectionSystem cn = new SQLiteConnectionSystem(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Writer);

            GetStartingIds(cn, tablepostfix, tablesareempty, out long nextnameid, out long nextsectorid);

            StreamWriter sw = debugoutputfile != null ? new StreamWriter(debugoutputfile) : null;

            long updates = 0;
            const int BlockSize = 100000;
            int Limit = int.MaxValue;
            bool jr_eof = false;

            DbCommand selectSectorCmd = cn.CreateCommand("SELECT id FROM Sectors" + tablepostfix + " WHERE name = @sname AND gridid = @gid");
            selectSectorCmd.AddParameter("@sname", DbType.String);
            selectSectorCmd.AddParameter("@gid", DbType.Int32);

            reportProgress?.Invoke("Being EDSM Download");

            while (!cancelRequested() && jr_eof == false)
            {
                int recordstostore = 0;

                while (true)
                {
                    try
                    {
                        if (jr.Read())                                                      // collect a decent amount
                        {
                            if (jr.TokenType == JsonToken.StartObject)
                            {
                                EDSMFileEntry d = new EDSMFileEntry();

                                if (d.Deserialize(jr) && d.id >= 0 && d.name.HasChars() && d.z != int.MinValue)     // if we have a valid record
                                {
                                    int gridid = GridId.Id(d.x, d.y);
                                    if (grididallowed == null || ( grididallowed.Length > gridid && grididallowed[gridid]))    // allows a null or small grid
                                    {
                                        CreateNewUpdate(selectSectorCmd, d, gridid, tablesareempty, ref maxdate, ref nextsectorid);
                                        recordstostore++;
                                    }
                                }

                                if (--Limit == 0)
                                {
                                    jr_eof = true;
                                    break;
                                }

                                if (recordstostore >= BlockSize)
                                    break;
                            }
                        }
                        else
                        {
                            jr_eof = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("EDSM JSON file exception " + ex.ToString());
                        jr_eof = true;                                                                              // stop read, but let it continue to finish this section
                    }
                }

                System.Diagnostics.Debug.WriteLine("Process " + BaseUtils.AppTicks.TickCountLap("L1") + "   " + updates);

                if (recordstostore > 0)
                {
                    if (!tablesareempty)
                        updates += ProcessUpdates(cn, ref nextnameid, tablepostfix, sw);

                   updates += StoreNewEntries(cn,  ref nextnameid, tablepostfix, tablesareempty, sw);

                    reportProgress?.Invoke("EDSM Download updated " + updates);
                }

                if (jr_eof)
                    break;

                if (SQLiteConnectionSystem.IsReadWaiting)
                {
                    System.Threading.Thread.Sleep(20);      // just sleepy for a bit to let others use the db
                }
            }

            System.Diagnostics.Debug.WriteLine("Process " + BaseUtils.AppTicks.TickCountLap("L1") + "   " + updates);
            reportProgress?.Invoke( "EDSM Download finished, updated " + updates);

            if (sw != null)
                sw.Close();

            selectSectorCmd.Dispose();
            cn.Dispose();

            sectoridcache = null;
            sectornamecache = null;

            return updates;
        }




        // create a new entry for insert in the sector tables 
        private static void CreateNewUpdate(DbCommand selectSectorCmd , EDSMFileEntry d, int gid, bool tablesareempty, ref DateTime maxdate, ref long nextsectorid)
        {
            TableWriteData data = new TableWriteData() { edsm = d, classifier = new EliteNameClassifier(d.name), gridid = gid };

            if (d.date > maxdate)                                   // for all, record last recorded date processed
                maxdate = d.date;

            Sector t = null, prev = null;

            if (!sectornamecache.ContainsKey(data.classifier.SectorName))   // if unknown to cache
            {
                sectornamecache[data.classifier.SectorName] = t = new Sector(data.classifier.SectorName, gridid: data.gridid);   // make a sector of sectorname and with gridID n , id == -1
            }
            else
            {
                t = sectornamecache[data.classifier.SectorName];        // find the first sector of name
                while (t != null && t.GId != data.gridid)        // if GID of sector disagrees
                {
                    prev = t;                          // go thru list
                    t = t.NextSector;
                }

                if (t == null)      // still not got it, its a new one.
                {
                    prev.NextSector = t = new Sector(data.classifier.SectorName, gridid: data.gridid);   // make a sector of sectorname and with gridID n , id == -1
                }
            }

            if (t.Id == -1)   // if unknown sector ID..
            {
                if (tablesareempty)     // if tables are empty, we can just presume its id
                {
                    t.Id = nextsectorid++;      // insert the sector with the guessed ID
                    t.insertsec = true;
                    sectoridcache[t.Id] = t;    // and cache
                    //System.Diagnostics.Debug.WriteLine("Made sector " + t.Name + ":" + t.GId);
                }
                else
                {
                    selectSectorCmd.Parameters[0].Value = t.Name;   
                    selectSectorCmd.Parameters[1].Value = t.GId;

                    using (DbDataReader reader = selectSectorCmd.ExecuteReader())       // find name:gid
                    {
                        if (reader.Read())      // if found name:gid
                        {
                            t.Id = (long)reader[0];
                        }
                        else
                        {
                            t.Id = nextsectorid++;      // insert the sector with the guessed ID
                            t.insertsec = true;
                        }

                        sectoridcache[t.Id] = t;                // and cache
                      //  System.Diagnostics.Debug.WriteLine("Made sector " + t.Name + ":" + t.GId);
                    }
                }
            }

            if (t.edsmdatalist == null)
                t.edsmdatalist = new List<TableWriteData>();

            t.edsmdatalist.Add(data);                       // add to list of systems to process for this sector
        }

        // used only when updating existing tables - see if any entries are there..

        private static long ProcessUpdates(SQLiteConnectionSystem cn,
                                           ref long nextnameid,
                                           string tablepostfix = "",       // set to add on text to table names to redirect to another table
                                           StreamWriter sw = null
            )
        {
            long updates = 0;
            
            DbCommand selectSysCmd = cn.CreateCommand("SELECT name,x,y,z FROM Systems" + tablepostfix + " WHERE edsmid == @nedsm");
            selectSysCmd.AddParameter("@nedsm", DbType.Int64);

            DbCommand updateSysCmd = cn.CreateCommand("UPDATE Systems" + tablepostfix + " SET sector=@sec, name=@name, x=@XP, y=@YP, z=@ZP WHERE edsmid == @nedsm");
            updateSysCmd.AddParameter("@sec", DbType.Int64);
            updateSysCmd.AddParameter("@name", DbType.Int64);
            updateSysCmd.AddParameter("@XP", DbType.Int32);
            updateSysCmd.AddParameter("@YP", DbType.Int32);
            updateSysCmd.AddParameter("@ZP", DbType.Int32);
            updateSysCmd.AddParameter("@nedsm", DbType.Int64);

            /////////////////////////////////////////////////////////// Normal mode

            foreach (var kvp in sectoridcache)                                              // sector id cache is unique - every sector in the db gets a cache entry (name cache is not)
            {
                Sector t = kvp.Value;

                if (t.edsmdatalist != null)
                {
                    foreach (var data in t.edsmdatalist)            // now write the star list in this sector
                    {
                        if (!data.classifier.IsStandard)            // standard names use the ID field and sector ID.  If not, its a name entry
                            data.classifier.NameId = nextnameid++;  // get next name ID - we always allocate a new one each time, because its unlikely its exactly the same name as one in there

                        selectSysCmd.Parameters[0].Value = data.edsm.id;

                        using (DbDataReader reader = selectSysCmd.ExecuteReader())
                        {
                            if (reader.Read())      // if there..
                            {
                                ulong dbname = (ulong)(long)reader[1];

                                if (dbname != data.classifier.ID ||
                                    Math.Abs((int)(long)reader[2] - data.edsm.x) >= 4 ||
                                    Math.Abs((int)(long)reader[3] - data.edsm.y) >= 4 ||
                                    Math.Abs((int)(long)reader[4] - data.edsm.z) >= 4)
                                {
                                    updateSysCmd.Parameters[0].Value = t.Id;
                                    updateSysCmd.Parameters[1].Value = data.classifier.ID;
                                    updateSysCmd.Parameters[2].Value = data.edsm.x;
                                    updateSysCmd.Parameters[3].Value = data.edsm.y;
                                    updateSysCmd.Parameters[4].Value = data.edsm.z;
                                    updateSysCmd.Parameters[5].Value = data.edsm.id;
                                    updateSysCmd.ExecuteNonQuery();
                                    updates++;

                                    if (sw != null)
                                        sw.WriteLine(data.classifier.ToString() + " " + data.edsm.x + "," + data.edsm.y + "," + data.edsm.z + ", EDSM:" + data.edsm.id + " Grid:" + data.gridid);
                                }

                                data.alreadyupdated = true;
                            }
                        }
                    }
                }
            }

            updateSysCmd.Dispose();
            selectSysCmd.Dispose();

            return updates;
        }

        private static long StoreNewEntries(SQLiteConnectionSystem cn,
                                           ref long nextnameid,
                                           string tablepostfix = "",        // set to add on text to table names to redirect to another table
                                           bool tablesareempty = false,     // set to presume table is empty, so we don't have to do look up queries
                                           StreamWriter sw = null
                                        )
        {
            long updates = 0;

            ////////////////////////////////////////////////////////////// push all new data to the db without any selects

            SQLExtTransactionLock<SQLiteConnectionSystem> tl = new SQLExtTransactionLock<SQLiteConnectionSystem>();     // not using on purpose.
            tl.OpenWriter();
            DbTransaction txn = cn.BeginTransaction();
            DbCommand insertSectorCmd = cn.CreateCommand("INSERT INTO Sectors" + tablepostfix + " (name,gridid) VALUES (@sname, @gridid)", txn);
            insertSectorCmd.AddParameter("@sname", DbType.String);
            insertSectorCmd.AddParameter("@gridid", DbType.Int32);

            DbCommand insertSysCmd = cn.CreateCommand("INSERT INTO Systems" + tablepostfix + " (sector,name, x, y, z, edsmid) VALUES (@sec,@name, @XP, @YP, @ZP, @nedsm)", txn);
            insertSysCmd.AddParameter("@sec", DbType.Int64);
            insertSysCmd.AddParameter("@name", DbType.Int64);
            insertSysCmd.AddParameter("@XP", DbType.Int32);
            insertSysCmd.AddParameter("@YP", DbType.Int32);
            insertSysCmd.AddParameter("@ZP", DbType.Int32);
            insertSysCmd.AddParameter("@nedsm", DbType.Int64);

            DbCommand insertNameCmd = cn.CreateCommand("INSERT INTO Names" + tablepostfix + " (name) VALUES (@sname)", txn);
            insertNameCmd.AddParameter("@sname", DbType.String);

            foreach (var kvp in sectoridcache)                  // all sectors cached, id is unique so its got all sectors                           
            {
                Sector t = kvp.Value;

                if (t.insertsec)         // if we have been told to insert the sector, do it
                {
                    insertSectorCmd.Parameters[0].Value = t.Name;     // make a new one so we can get the ID
                    insertSectorCmd.Parameters[1].Value = t.GId;
                    insertSectorCmd.ExecuteNonQuery();
                    //System.Diagnostics.Debug.WriteLine("Written sector " + t.id + " " +t.Name + " now clean");
                    t.insertsec = false;
                }

                if (t.edsmdatalist != null)       // if updated..
                {
                    foreach (var data in t.edsmdatalist)            // now write the star list in this sector
                    {
                        if (!data.alreadyupdated)                 // in normal mode, we may already have checked it
                        {
                            if (tablesareempty)
                            {
                                if (!data.classifier.IsStandard)    // if non standard, we assign a new ID
                                    data.classifier.NameId = nextnameid++;
                            }

                            if (!data.classifier.IsStandard)        // if non standard, need a new name
                            {
                                insertNameCmd.Parameters[0].Value = data.classifier.StarName;       // insert
                                insertNameCmd.ExecuteNonQuery();
                            }

                            try
                            {
                                insertSysCmd.Parameters[0].Value = t.Id;
                                insertSysCmd.Parameters[1].Value = data.classifier.ID;
                                insertSysCmd.Parameters[2].Value = data.edsm.x;
                                insertSysCmd.Parameters[3].Value = data.edsm.y;
                                insertSysCmd.Parameters[4].Value = data.edsm.z;
                                insertSysCmd.Parameters[5].Value = data.edsm.id;
                                insertSysCmd.ExecuteNonQuery();

                                if (sw != null)
                                    sw.WriteLine(data.classifier.ToString() + " " + data.edsm.x + "," + data.edsm.y + "," + data.edsm.z + ", EDSM:" + data.edsm.id + " Grid:" + data.gridid);

                                updates++;
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine("general exception during insert - ignoring " + ex.ToString());
                            }

                            // System.Diagnostics.Debug.WriteLine("Made Sys Entry " + t.Name + ":" + data.classifier.StarName + " " + data.classifier.ID.ToString("x"));
                        }
                    }
                }

                t.edsmdatalist = null;     // and delete back
            }

            txn.Commit();

            insertSectorCmd.Dispose();
            insertSysCmd.Dispose();
            insertNameCmd.Dispose();
            txn.Dispose();
            tl.Dispose();

            return updates;

        }

        static private void GetStartingIds(SQLiteConnectionSystem cn, string tablepostfix, bool tablesareempty, out long nextnameid, out long nextsectorid)
        {
            nextnameid = nextsectorid = 1;

            if ( !tablesareempty)
            {
                using (DbCommand queryNameCmd = cn.CreateCommand("SELECT Max(id) as id FROM Names" + tablepostfix))
                    nextnameid = (long)cn.SQLScalar(queryNameCmd) + 1;     // get next ID we would make..

                using (DbCommand querySectorCmd = cn.CreateCommand("SELECT Max(id) as id FROM Sectors" + tablepostfix))
                    nextsectorid = (long)cn.SQLScalar(querySectorCmd) + 1;     // get next ID we would make..
            }
        }

        #endregion

        #region Upgrade from 102

        public static long UpgradeDB102to200( Func<bool> cancelRequested, Action<string> reportProgress, string tablepostfix)
        {
            sectoridcache = new Dictionary<long, Sector>();
            sectornamecache = new Dictionary<string, Sector>();

            SQLiteConnectionSystem cn1 = new SQLiteConnectionSystem(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.ReaderWriter);
            SQLiteConnectionSystem cn2 = new SQLiteConnectionSystem(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Writer);

            long nextnameid = 1;                // tables are empty..
            long nextsectorid = 1;

            long recordpos = 0;
            long updates = 0;
            const int BlockSize = 100000;
            long Limit = long.MaxValue;

            DbCommand selectSectorCmd = cn2.CreateCommand("SELECT id FROM Sectors" + tablepostfix + " WHERE name = @sname AND gridid = @gid");
            selectSectorCmd.AddParameter("@sname", DbType.String);
            selectSectorCmd.AddParameter("@gid", DbType.Int32);

            DateTime maxdate = DateTime.MinValue;       // we don't pass this back due to using the same date
            reportProgress?.Invoke("Being System DB upgrade");

            while ( !cancelRequested() )
            { 
                DbCommand selectPrev = cn1.CreateCommand(
                    "Select s.EdsmId,s.x,s.y,s.z,n.Name,s.UpdateTimeStamp " +
                    "From EdsmSystems s left Outer Join SystemNames n On n.EdsmId=s.EdsmId " + 
                    "LIMIT " + BlockSize.ToStringInvariant() + " OFFSET " + recordpos.ToStringInvariant() );

                BaseUtils.AppTicks.TickCountLap("U1");

                int recordstostore = 0;

                using (DbDataReader reader = selectPrev.ExecuteReader())       // find name:gid
                {
                    while (reader.Read())      // if found name:gid
                    {
                        try
                        {
                            EDSMFileEntry d = new EDSMFileEntry();
                            d.id = (long)reader[0];
                            d.x = (int)(long)reader[1];
                            d.y = (int)(long)reader[2];
                            d.z = (int)(long)reader[3];
                            d.name = (string)reader[4];
                            d.date = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromSeconds((long)reader["UpdateTimestamp"]);

                            int gridid = GridId.Id(d.x, d.y);
                            CreateNewUpdate(selectSectorCmd, d, gridid, true, ref maxdate, ref nextsectorid);
                            recordstostore++;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Reading prev table" + ex);
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("Reader took " + BaseUtils.AppTicks.TickCountLap("U1") + "   " + recordpos);
                selectPrev.Dispose();

                if (recordstostore >= 0) 
                {
                    updates += StoreNewEntries(cn2, ref nextnameid, tablepostfix, true, null);
                    reportProgress?.Invoke("System DB upgrade processed " + updates);

                    recordpos += recordstostore;

                    Limit -= recordstostore;

                    if (Limit <= 0)
                        break;

                    if (SQLiteConnectionSystem.IsReadWaiting)
                    {
                        System.Threading.Thread.Sleep(20);      // just sleepy for a bit to let others use the db
                    }

                    if (recordstostore < BlockSize)             // must have run out
                        break;
                }

                System.Diagnostics.Debug.WriteLine("Writer took " + BaseUtils.AppTicks.TickCountLap("U1") + "   " + recordpos);
            }

            reportProgress?.Invoke("System DB complete, processed " + updates);

            selectSectorCmd.Dispose();
            cn1.Dispose();
            cn2.Dispose();

            sectoridcache = null;
            sectornamecache = null;

            return updates;
        }

        #endregion

        #region Internal Vars and Classes

        static Dictionary<long, Sector> sectoridcache;          // only used during store operation
        static Dictionary<string, Sector> sectornamecache;

        private class Sector
        {
            public long Id;
            public int GId;
            public string Name;

            public Sector NextSector;       // memory only field, link to next in list

            public Sector(string name, long id = -1, int gridid = -1 )
            {
                this.Name = name;
                this.GId = gridid;
                this.Id = id;
                this.NextSector = null;
            }

            // for write table purposes only

            public List<TableWriteData> edsmdatalist;
            public bool insertsec = false;
        };

        private class TableWriteData
        {
            public EDSMFileEntry edsm;
            public EliteNameClassifier classifier;
            public int gridid;
            public bool alreadyupdated;
        }

        public class EDSMFileEntry
        {
            public bool Deserialize(JsonReader rdr)
            {
                while (rdr.Read() && rdr.TokenType == JsonToken.PropertyName)
                {
                    string field = rdr.Value as string;
                    switch (field)
                    {
                        case "name":
                            name = rdr.ReadAsString();
                            break;
                        case "id":
                            id = rdr.ReadAsInt32() ?? 0;
                            break;
                        case "date":
                            date = rdr.ReadAsDateTime() ?? DateTime.MinValue;
                            break;
                        case "coords":
                            {
                                if (rdr.TokenType != JsonToken.StartObject)
                                    rdr.Read();

                                while (rdr.Read() && rdr.TokenType == JsonToken.PropertyName)
                                {
                                    field = rdr.Value as string;
                                    double? v = rdr.ReadAsDouble();
                                    if (v == null)
                                        return false;
                                    int vi = (int)(v * 128.0);

                                    switch (field)
                                    {
                                        case "x":
                                            x = vi;
                                            break;
                                        case "y":
                                            y = vi;
                                            break;
                                        case "z":
                                            z = vi;
                                            break;
                                    }
                                }

                                break;
                            }
                        default:
                            rdr.Read();
                            JToken.Load(rdr);
                            break;
                    }
                }

                return true;
            }

            public string name;
            public long id = -1;
            public DateTime date;
            public int x = int.MinValue;
            public int y = int.MinValue;
            public int z = int.MinValue;
        }

        #endregion
    }
}


