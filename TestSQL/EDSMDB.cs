using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliteDangerousCore.EDSM;
using SQLLiteExtensions;
using System.Data.Common;
using System.Data;

namespace EliteDangerousCore.DB
{
    class EDSMDB
    {
        public class Star           // returned when you get Star data
        {
            public string Name;
            public int X, Y, Z;
            public long EDSMId;
            public float Xf { get { return (float)X / XYZScalar; } }
            public float Yf { get { return (float)Y / XYZScalar; } }
            public float Zf { get { return (float)Z / XYZScalar; } }

            public Star(string name, int x, int y, int z, long edsmid)
            {
                Name = name; this.X = x; this.Y = y; this.Z = z; EDSMId = edsmid;
            }
        }

        public static void DeleteCache()    // for debugging mostly
        {
            sectoridcache = null;
            sectornamecache = null;
            nameidcache = null;
            sectoridcache = new Dictionary<long, Sector>();           // speeds up access, less than 20k of sector names
            sectornamecache = new Dictionary<string, Sector>();   // speeds up access, less than 20k of sector names
            nameidcache = new Dictionary<long, string>();         // speeds up access, less than 20k ish of unique names
        }


        // to do..
        // gridid allowed in during table write
        // GetSystemVector
        // GetStarPositions
        // GetEDSMFromNames?
        // GetSystemsByName
        // GetSystem - by name, by edsmid
        // get system list by position..
        // get system nearest to

        // also eddb database - need another class to handle that in the db
        // keep seperate
        // do not use Isystem
        // use SystemCache to return Isystem from data gleaned from EDSMDB and EDDDB classes - thats the thing that brings it together




        #region Get Functions

        public static Star FindStar(string name)
        {
            EliteNameClassifier ec = new EliteNameClassifier(name);

            using (SQLiteConnectionEDSM cn = new SQLiteConnectionEDSM(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Reader))
            {
                Sector t = GetSector(ec.SectorName, cn);            // sectorname is "NonStandard" or a real sector name

                if (t != null)
                {
                    if (ec.IsStandard)                              // standard is easy - code is just the sectorid + the classifer from the name
                    {
                        Star s = GetStar(t.id, ec.ID, cn);

                        if (s != null)
                        {
                            s.Name = ec.ToString();
                            return s;
                        }
                    }
                    else
                    {
                        //List<long> names = GetNames(ec.StarName, cn);     // all the nid of the names called starname.. there may be >1 since names are not unique in the name table

                        //foreach (long id in names)
                        //{
                        //    Star s = GetStar((ulong)id | ((ulong)(t.id) << EliteNameClassifier.SectorPos), cn);     // they will be unique by sectoreid+nameid.
                        //    if (s != null)
                        //    {
                        //        s.Name = ec.ToString();
                        //        return s;
                        //    }
                        //}
                    }
                }
            }

            return null;
        }

        public static List<Star> FindStars(string extraconditions = null)
        {
            List<Star> ret = new List<Star>();

            using (SQLiteConnectionEDSM cn = new SQLiteConnectionEDSM(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Writer))
            {
                DbCommand selectSysCmd = cn.CreateCommand("SELECT c.name,s.name,s.x,s.y,s.z,s.edsmid,n.Name FROM Systems s LEFT OUTER JOIN Names n On s.name=n.id  LEFT OUTER JOIN Sectors c on c.id=s.sector " + (extraconditions.HasChars() ? (" " + extraconditions) : ""));


                using (DbDataReader reader = selectSysCmd.ExecuteReader())
                {
                    while (reader.Read())      // if there..
                    {
                        string sectorname = (string)reader[0];
                        EliteNameClassifier ec = new EliteNameClassifier((ulong)(long)reader[1]);
                        string name = sectorname + (ec.IsStandard ? ec.ToString() : (string)reader[6]);
                        Star s = new Star(name, (int)(long)reader[2], (int)(long)reader[3], (int)(long)reader[4], (long)reader[5]);
                        ret.Add(s);
                    }
                }
            }

            return ret;
        }

        //public static List<Star> FindStarsWildcard(string name)     // partial name
        //{
        //    List<Star> ret = new List<Star>();

        //    EliteNameClassifier ec = new EliteNameClassifier(name);

        //    using (SQLiteConnectionEDSM cn = new SQLiteConnectionEDSM(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Reader))
        //    {
        //        if (ec.PartsComplete >= EliteNameClassifier.NameType.Identifier)  // if its def a standard name
        //        {
        //            Sector t = GetSector(ec.SectorName, cn);            // get the sector - selection if from this sector

        //            if (t != null)
        //            {
        //                ec.SectorId = t.id;

        //            }
        //        }
        //        else
        //        {
        //            if (ec.SectorName != EliteNameClassifier.NonStandard)
        //            {
        //                List<Sector> seclist = GetSectors(ec.SectorName, cn);       // sectors named like this - all stars in them named
        //            }
        //            else
        //            {
        //                Sector t = GetSector(ec.SectorName, cn);            // get the standard sector - selection if from this sector

        //                List<long> names = GetNames(ec.StarName, cn, true);       // all the nid of the names like starname

        //                foreach (long id in names)
        //                {
        //                    Star s = GetStar((ulong)id | ((ulong)(t.id) << EliteNameClassifier.SectorPos), cn);   

        //                    if (s != null)
        //                    {
        //                        s.Name = ec.ToString();
        //                        return s;
        //                    }
        //                }


        //            }
        //        }
        //    }

        //    return ret;
        //}

        // public enum SystemAskType { AnyStars, PopulatedStars, UnPopulatedStars };            -- this is mixing EDDB and EDSM, belongs in another class
        //public static int GetSystemVector<V>(int gridid, ref V[] vertices, ref uint[] colours,
        //SystemAskType ask, int percentage,
        //Func<float, float, float, V> tovect)

        //public static List<Point3D> GetStarPositions(int percentage)  // return star positions.. whole galaxy, of this percentage, 2dmap... hmmm


        // public static List<long> GetEdsmIdsFromName(string name, SQLiteConnectionSystem cn = null, bool uselike = false)
        //public static List<ISystem> GetSystemsByName(string name, SQLiteConnectionSystem cn = null, bool uselike = false)









        #endregion

        #region Table Update

        public static long ParseEDSMJSONFile(string filename, bool[] grididallow, ref DateTime date, Func<bool> cancelRequested, string tableposfix = "", bool presumeempty = false, string debugoutputfile = null)
        {
            using (StreamReader sr = new StreamReader(filename))         // read directly from file..
                return ParseEDSMJSON(sr, grididallow, ref date, cancelRequested, tableposfix, presumeempty, debugoutputfile);
        }

        public static long ParseEDSMJSONString(string data, bool[] grididallow, ref DateTime date, Func<bool> cancelRequested, string tableposfix = "", bool presumeempty = false, string debugoutputfile = null)
        {
            using (StringReader sr = new StringReader(data))         // read directly from file..
                return ParseEDSMJSON(sr, grididallow, ref date, cancelRequested, tableposfix, presumeempty, debugoutputfile);
        }

        public static long ParseEDSMJSON(TextReader sr, bool[] grididallow, ref DateTime date, Func<bool> cancelRequested, string tablepostfix = "", bool presumeempty = false, string debugoutputfile = null)
        {
            using (JsonTextReader jr = new JsonTextReader(sr))
                return ParseEDSMJSON(jr, grididallow, ref date, cancelRequested, tablepostfix, presumeempty, debugoutputfile);
        }

        // set tempostfix to use another set of tables

        public static long ParseEDSMJSON(JsonTextReader jr, bool[] grididallowed, ref DateTime maxdate, Func<bool> cancelRequested, 
                                        string tablepostfix = "",        // set to add on text to table names to redirect to another table
                                        bool tablesareempty = false,     // set to presume table is empty, so we don't have to do look up queries
                                        string debugoutputfile = null
                                        )
        {
            long updates = 0;

            const int BlockSize = 10000;
            int Limit = int.MaxValue;

            bool jr_eof = false;

            long nextnameid = 1;       // whats the next id available
            long nextsectorid = 1;

            SQLiteConnectionEDSM cn = new SQLiteConnectionEDSM(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Writer);

            if (!tablesareempty)        // here, if we are not doing a empty write,  we need the next free ID for names and sectors
            {
                // TBD check what happens on empty table
                using (DbCommand queryNameCmd = cn.CreateCommand("SELECT Max(id) as id FROM Names" + tablepostfix))
                    nextnameid = (long)cn.SQLScalar(queryNameCmd) + 1;     // get next ID we would make..

                using (DbCommand querySectorCmd = cn.CreateCommand("SELECT Max(id) as id FROM Sectors" + tablepostfix))
                    nextsectorid = (long)cn.SQLScalar(querySectorCmd) + 1;     // get next ID we would make..
            }

            StreamWriter sw = null;
            if ( debugoutputfile != null )
                sw = new StreamWriter(debugoutputfile);

            DbCommand selectSectorCmd = cn.CreateCommand("SELECT id,minx,minz,maxx,maxz FROM Sectors" + tablepostfix + " WHERE name = @sname");
            selectSectorCmd.AddParameter("@sname", DbType.String);

            DbCommand selectSysCmd = cn.CreateCommand("SELECT name,x,y,z FROM Systems" + tablepostfix + " WHERE edsmid == @nedsm");
            selectSysCmd.AddParameter("@nedsm", DbType.Int64);

            DbCommand updateSysCmd = cn.CreateCommand("UPDATE Systems" + tablepostfix + " SET sector=@sec, name=@name, x=@XP, y=@YP, z=@ZP WHERE edsmid == @nedsm");
            updateSysCmd.AddParameter("@sec", DbType.Int64);
            updateSysCmd.AddParameter("@name", DbType.Int64);
            updateSysCmd.AddParameter("@XP", DbType.Int32);
            updateSysCmd.AddParameter("@YP", DbType.Int32);
            updateSysCmd.AddParameter("@ZP", DbType.Int32);
            updateSysCmd.AddParameter("@nedsm", DbType.Int64);

            while (!cancelRequested() && jr_eof == false)
            {
                System.Diagnostics.Debug.WriteLine("Lap " + BaseUtils.AppTicks.TickCountLap("L1") + "   "  + updates);

                int recordstostore = 0;

                while (true)
                {
                    try
                    {
                        if (jr.Read())                                                      // collect a decent amount
                        {
                            if (jr.TokenType == JsonToken.StartObject)
                            {
                                EDSMDumpSystem d = new EDSMDumpSystem();
                                if (d.Deserialize(jr) && d.id >= 0 && d.name.HasChars() && d.z != int.MinValue)     // if we have a valid record
                                {
                                    TableWriteData data = new TableWriteData() { edsm = d, classifier = new EliteNameClassifier(d.name) };

                                    if (d.date > maxdate)                                   // for all, record last recorded date processed
                                        maxdate = d.date;

                                    Sector t;

                                    if (!sectornamecache.ContainsKey(data.classifier.SectorName))               // cache the sector
                                    {
                                        sectornamecache[data.classifier.SectorName] = t = new Sector(data.classifier.SectorName);
                                        t.insertsec = tablesareempty;       // if working on new table, and we made a sector, we must insert it.  If not a new table, we use the code below to see if the sector is there
                                    }
                                    else
                                        t = sectornamecache[data.classifier.SectorName];

                                    t.maxx = Math.Max(t.maxx, data.edsm.x);                // update the min.max
                                    t.maxz = Math.Max(t.maxz, data.edsm.z);
                                    t.minx = Math.Min(t.minx, data.edsm.x);
                                    t.minz = Math.Min(t.minz, data.edsm.z);
                                    t.dirty = true;     // because we have changed it
                                    //System.Diagnostics.Debug.WriteLine("Changed sector " + t.Name + " due to new system");

                                    if (t.edsmdatalist == null)
                                        t.edsmdatalist = new List<TableWriteData>();

                                    t.edsmdatalist.Add(data);                       // add to list of systems to process for this sector
                                    recordstostore++;
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
                    catch( Exception ex )
                    {
                        System.Diagnostics.Debug.WriteLine("EDSM JSON file exception " + ex.ToString());
                        jr_eof = true;                                                                              // stop read, but let it continue to finish this section
                    }
                }

                if (recordstostore == 0)
                    break;

                /////////////////////////////////////////////////////////// Normal mode

                if (!tablesareempty)      // we we are not going for it, we need to see if we can resolve those -1 in sectors.  And see if we have duplicate systems. 
                {
                    foreach (var kvp in sectornamecache)                                              // we need to have the dbid filled out in each sector as its part of the system name
                    {
                        Sector t = kvp.Value;

                        if (t.id == -1)         // if we have not got a lock on this sector 
                        {
                            selectSectorCmd.Parameters[0].Value = t.Name;

                            using (DbDataReader reader = selectSectorCmd.ExecuteReader())       // find it
                            {
                                if (reader.Read())      // if there..  if not, its a fresh sector, so leave at -1 for the below code to write it
                                {
                                    t.id = (long)reader[0];
                                    t.minx = Math.Max(t.minx, (int)(long)reader[1]);
                                    t.minz = Math.Max(t.minz, (int)(long)reader[2]);
                                    t.maxx = Math.Max(t.maxx, (int)(long)reader[3]);      // update cache with values
                                    t.maxz = Math.Max(t.maxz, (int)(long)reader[4]);
                                   // System.Diagnostics.Debug.WriteLine("Found sector " + t.id + " " + t.Name + " now clean");
                                    t.dirty = true;     // its dirty since we max/min the sector with the reader data
                                }
                                else
                                {
                                    t.id = nextsectorid++;      // its fresh, insert the sector 
                                    t.insertsec = true;
                                   // System.Diagnostics.Debug.WriteLine("To make sector " + t.id + " " + t.Name);
                                }

                                sectoridcache[t.id] = t;                // and cache
                            }

                        }

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
                                            updateSysCmd.Parameters[0].Value = t.id;
                                            updateSysCmd.Parameters[1].Value = data.classifier.ID;
                                            updateSysCmd.Parameters[2].Value = data.edsm.x;
                                            updateSysCmd.Parameters[3].Value = data.edsm.y;
                                            updateSysCmd.Parameters[4].Value = data.edsm.z;
                                            updateSysCmd.Parameters[5].Value = data.edsm.id;
                                            updateSysCmd.ExecuteNonQuery();
                                        }

                                        data.alreadyupdated = true;
                                    }
                                }
                            }
                        }
                    }
                }

                ////////////////////////////////////////////////////////////// push all new data to the db without any selects

                {
                    SQLExtTransactionLock<SQLiteConnectionEDSM> tl = new SQLExtTransactionLock<SQLiteConnectionEDSM>();     // not using on purpose.
                    tl.OpenWriter();
                    DbTransaction txn = cn.BeginTransaction();
                    DbCommand insertSectorCmd = cn.CreateCommand("INSERT INTO Sectors" + tablepostfix + " (name,minx,minz,maxx,maxz,gridlist) VALUES (@sname, @minx,@minz,@maxx,@maxz,@gridlist)", txn);
                    insertSectorCmd.AddParameter("@sname", DbType.String);
                    insertSectorCmd.AddParameter("@minx", DbType.Int32);
                    insertSectorCmd.AddParameter("@minz", DbType.Int32);
                    insertSectorCmd.AddParameter("@maxx", DbType.Int32);
                    insertSectorCmd.AddParameter("@maxz", DbType.Int32);
                    insertSectorCmd.AddParameter("@gridlist", DbType.String);

                    DbCommand insertSysCmd = cn.CreateCommand("INSERT INTO Systems" + tablepostfix + " (sector,name, x, y, z, edsmid) VALUES (@sec,@name, @XP, @YP, @ZP, @nedsm)", txn);
                    insertSysCmd.AddParameter("@sec", DbType.Int64);
                    insertSysCmd.AddParameter("@name", DbType.Int64);
                    insertSysCmd.AddParameter("@XP", DbType.Int32);
                    insertSysCmd.AddParameter("@YP", DbType.Int32);
                    insertSysCmd.AddParameter("@ZP", DbType.Int32);
                    insertSysCmd.AddParameter("@nedsm", DbType.Int64);

                    DbCommand insertNameCmd = cn.CreateCommand("INSERT INTO Names" + tablepostfix + " (name) VALUES (@sname)", txn);
                    insertNameCmd.AddParameter("@sname", DbType.String);

                    foreach (var kvp in sectornamecache)                                              // we need to have the dbid filled out in each sector as its part of the system name
                    {
                        Sector t = kvp.Value;

                        if (t.insertsec)         // if we have been told to insert the sector, do it
                        {
                            if (tablesareempty)       // in table empty mode, we won't have assigned a sector id, so do so
                            {
                                t.id = nextsectorid++;      // its fresh, insert the sector 
                                sectoridcache[t.id] = t;
                            }

                            insertSectorCmd.Parameters[0].Value = t.Name;     // make a new one so we can get the ID
                            insertSectorCmd.Parameters[1].Value = t.minx;
                            insertSectorCmd.Parameters[2].Value = t.minz;
                            insertSectorCmd.Parameters[3].Value = t.maxx;
                            insertSectorCmd.Parameters[4].Value = t.maxz;
                            insertSectorCmd.Parameters[5].Value = GridId.Grids((float)t.minx / XYZScalar, (float)t.maxx / XYZScalar, (float)t.minz / XYZScalar, (float)t.maxz / XYZScalar);
                            insertSectorCmd.ExecuteNonQuery();
                            //System.Diagnostics.Debug.WriteLine("Written sector " + t.id + " " +t.Name + " now clean");
                            t.insertsec = false;
                            t.dirty = false;            // clean now since the db has the same data as t.
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
                                        insertSysCmd.Parameters[0].Value = t.id;
                                        insertSysCmd.Parameters[1].Value = data.classifier.ID;
                                        insertSysCmd.Parameters[2].Value = data.edsm.x;
                                        insertSysCmd.Parameters[3].Value = data.edsm.y;
                                        insertSysCmd.Parameters[4].Value = data.edsm.z;
                                        insertSysCmd.Parameters[5].Value = data.edsm.id;
                                        insertSysCmd.ExecuteNonQuery();

                                        if (sw != null)
                                            sw.WriteLine(data.classifier.ToString() + " " + data.edsm.x + " " + data.edsm.y + " " + data.edsm.z + " " + data.edsm.id);

                                        updates++;
                                    }
                                    catch( Exception ex)
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
                }

                if ( SQLiteConnectionEDSM.IsReadWaiting )
                {
                    System.Threading.Thread.Sleep(20);      // just sleepy for a bit to let others use the db
                }
            }   // main while

            System.Diagnostics.Debug.WriteLine("Sector Updates " + BaseUtils.AppTicks.TickCountLap("L1"));

            /////////////////////////////////////////////////////////////// Write back any dirty sectors

            { 
                SQLExtTransactionLock<SQLiteConnectionEDSM> tl = new SQLExtTransactionLock<SQLiteConnectionEDSM>();     // using with its forced indentation is a PINA with so many 
                tl.OpenWriter();
                DbTransaction txn = cn.BeginTransaction();
                DbCommand updateSectorCmd = cn.CreateCommand("UPDATE Sectors" + tablepostfix + " SET minx= @minx, minz=@minz, maxx=@maxx, maxz=@maxz, gridlist=@gridlist WHERE name = @sname", txn);
                updateSectorCmd.AddParameter("@sname", DbType.String);
                updateSectorCmd.AddParameter("@minx", DbType.Double);
                updateSectorCmd.AddParameter("@minz", DbType.Double);
                updateSectorCmd.AddParameter("@maxx", DbType.Double);
                updateSectorCmd.AddParameter("@maxz", DbType.Double);
                updateSectorCmd.AddParameter("@gridlist", DbType.String);

                foreach (var kvp in sectornamecache)
                {
                    Sector t = kvp.Value;

                    if (t.dirty)        // any dirty ones (either already present, or made then changed again) write back
                    {
                        System.Diagnostics.Debug.WriteLine("Update sector " + t.Name + " due to dirtyness");

                        updateSectorCmd.Parameters[0].Value = t.Name;
                        updateSectorCmd.Parameters[1].Value = t.minx;
                        updateSectorCmd.Parameters[2].Value = t.minz;
                        updateSectorCmd.Parameters[3].Value = t.maxx;
                        updateSectorCmd.Parameters[4].Value = t.maxz;
                        updateSectorCmd.Parameters[5].Value = GridId.Grids((float)t.minx / XYZScalar, (float)t.maxx / XYZScalar, (float)t.minz / XYZScalar, (float)t.maxz / XYZScalar);
                        updateSectorCmd.ExecuteNonQuery();
                        t.dirty = false;
                    }

                    t.edsmdatalist = null;  // ensure empty..
                }

                txn.Commit();
                updateSectorCmd.Dispose();
                txn.Dispose();
                tl.Dispose();
            }

            if (sw != null)
                sw.Close();

            updateSysCmd.Dispose();
            selectSysCmd.Dispose();
            selectSectorCmd.Dispose();
            cn.Dispose();

            System.Diagnostics.Debug.Assert(sectoridcache.Count == sectornamecache.Count);
            
            System.Diagnostics.Debug.WriteLine("End " + BaseUtils.AppTicks.TickCountLap("L1"));
            return updates;
        }

        #endregion

        #region Helpers

        //public static Star MakeStar(long sector, ulong nameid, int x, int y, int z, long edsmid, SQLiteConnectionEDSM cn)
        //{
        //    EliteNameClassifier ec = new EliteNameClassifier(nameid);

        //    if (ec.IsStandard)
        //    {
        //        if (sectoridcache.ContainsKey(sector))
        //            return new Star(sectoridcache[sector].Name + " " + ec.ToString(), x, y, z, edsmid);
        //        else
        //        {
        //            // get name and sector..
        //        }
        //    }
        //    else
        //    {
        //        if (sectoridcache.ContainsKey(sector))
        //        {
        //            string name = GetName(ec.NameId, cn);
        //            return new Star(name, x, y, z, edsmid);
        //        }
        //        else
        //        {
        //            // get name and sector
        //        }

        //    }

            

        //    Sector s = GetSector(sector, cn);

        //    if (s.Name != EliteNameClassifier.NonStandard)            // if we looked up NonStandard, its not used, else add it to name
        //        name = s.Name + " " + name;

        //    return new Star(name, x, y, z, edsmid);
        //}

        private static Sector GetSector(long id, SQLiteConnectionEDSM cn)
        {
            if (sectoridcache.ContainsKey(id))
                return sectoridcache[id];
            else
            {
                using (DbCommand selectSectorCmd = cn.CreateCommand("SELECT name,minx,minz,maxx,maxz,gridlist FROM Sectors WHERE id = @nid"))
                {
                    selectSectorCmd.AddParameterWithValue("@nid", id);

                    using (DbDataReader reader = selectSectorCmd.ExecuteReader())
                    {
                        if (reader.Read())      // if there..
                        {
                            Sector t = new Sector((string)reader[0], (int)(long)reader[1], (int)(long)reader[2], (int)(long)reader[3], (int)(long)reader[4],id, (string)reader[5]);
                            sectoridcache[t.id] = t;
                            sectornamecache[t.Name] = t;
                            return t;
                        }
                        else
                            return null;
                    }
                }
            }
        }

        private static Sector GetSector(string name, SQLiteConnectionEDSM cn)
        {
            if (sectornamecache.ContainsKey(name))
                return sectornamecache[name];
            else
            {
                using (DbCommand selectSectorCmd = cn.CreateCommand("SELECT id,minx,minz,maxx,maxz,gridlist FROM Sectors WHERE name = @sname"))
                {
                    selectSectorCmd.AddParameterWithValue("@sname", name);

                    using (DbDataReader reader = selectSectorCmd.ExecuteReader())
                    {
                        if (reader.Read())      // if there..
                        {
                            Sector t = new Sector(name, (int)(long)reader[1], (int)(long)reader[2], (int)(long)reader[3], (int)(long)reader[4], (long)reader[0], (string)reader[5]);
                            sectoridcache[t.id] = t;
                            sectornamecache[t.Name] = t;
                            return t;
                        }
                        else
                            return null;
                    }
                }
            }
        }

        private static List<Sector> GetSectors(string wildcardname, SQLiteConnectionEDSM cn)
        {
            using (DbCommand selectSectorCmd = cn.CreateCommand("SELECT name,minx,minz,maxx,maxz,id,gridlist FROM Sectors WHERE name like @sname"))
            {
                selectSectorCmd.AddParameterWithValue("@sname", wildcardname);

                List<Sector> ret = new List<Sector>();

                using (DbDataReader reader = selectSectorCmd.ExecuteReader())
                {
                    while (reader.Read())      // if there..
                    {
                        Sector t = new Sector((string)reader[0], (int)(long)reader[1], (int)(long)reader[2], (int)(long)reader[3], (int)(long)reader[4], (long)reader[5], (string)reader[6]);
                        sectoridcache[t.id] = t;
                        sectornamecache[t.Name] = t;
                        ret.Add(t);
                    }

                    return ret;
                }
            }
        }


        private static string GetName(long id, SQLiteConnectionEDSM cn)
        {
            if (nameidcache.ContainsKey(id))
                return nameidcache[id];
            else
            {
                using (DbCommand selectNameCmd = cn.CreateCommand("SELECT name FROM Names WHERE id=@nid"))
                {
                    selectNameCmd.AddParameterWithValue("@nid", id);

                    using (DbDataReader reader = selectNameCmd.ExecuteReader())
                    {
                        if (reader.Read())      // if there..
                        {
                            string name = (string)reader[0];
                            nameidcache[id] = name;
                            return name;
                        }
                        else
                            return null;
                    }
                }
            }
        }

        //private static List<Star> GetNames(long sector, string name, SQLiteConnectionEDSM cn, bool like = false)  // names are not unique
        //{
        //    List<Star> ret = new List<Star>();

        //    using (DbCommand selectNameCmd = cn.CreateCommand("SELECT n.id,s.x,s.y,s.z FROM Names n inner join Systems s On )==n.id where name " + (like ? "like" : "=") + " @nname"))
        //    {
        //        selectNameCmd.AddParameterWithValue("@nname", name);

        //        using (DbDataReader reader = selectNameCmd.ExecuteReader())
        //        {
        //            while (reader.Read())      // if there..
        //            {
        //                ret.Add((long)reader[0]);
        //            }
        //        }
        //    }

        //    return ret;
        //}

        private static Star GetStar(long sector, ulong nameid, SQLiteConnectionEDSM cn)       // name is returned blank
        {
            using (DbCommand selectNameCmd = cn.CreateCommand("SELECT x,y,z,edsmid FROM Systems WHERE name = @nid AND sector=@sec"))
            {
                selectNameCmd.AddParameterWithValue("@nid", nameid);
                selectNameCmd.AddParameterWithValue("@sec", sector);

                using (DbDataReader reader = selectNameCmd.ExecuteReader())
                {
                    if (reader.Read())      // if there..
                    {
                        return new Star("", (int)(long)reader[0], (int)(long)reader[1], (int)(long)reader[2], (long)reader[3]);
                    }
                    else
                        return null;
                }
            }
        }


        #endregion

        #region Internal Vars and Classes

        static Dictionary<long, Sector> sectoridcache = new Dictionary<long, Sector>();           // speeds up access, less than 20k of sector names
        static Dictionary<string, Sector> sectornamecache = new Dictionary<string, Sector>();   // speeds up access, less than 20k of sector names
        static Dictionary<long, string> nameidcache = new Dictionary<long, string>();         // speeds up access, less than 20k ish of unique names

        private const float XYZScalar = 128.0F;     // scaling between DB stored values and floats

        private class Sector
        {
            public long id;
            public string Name;
            public int maxx;
            public int minx;
            public int maxz;
            public int minz;
            public string gridlist;

            public Sector(string name, int minx = int.MaxValue, int minz = int.MaxValue, int maxx = int.MinValue, int maxz = int.MinValue, long id = -1, string gridlist="" )
            {
                Name = name;
                this.maxx = maxx; this.maxz = maxz;
                this.minx = minx; this.minz = minz;
                this.id = id;
                this.gridlist = gridlist;
            }

            // for write table purposes only

            public List<TableWriteData> edsmdatalist;
            public bool dirty = false;
            public bool insertsec = false;
        };

        private class TableWriteData
        {
            public EDSMDumpSystem edsm;
            public EliteNameClassifier classifier;
            public bool alreadyupdated;
        }

        #endregion
    }
}


