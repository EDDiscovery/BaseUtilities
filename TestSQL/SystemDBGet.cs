using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLLiteExtensions;
using System.Data.Common;
using System.Data;
using System.Drawing;

namespace EliteDangerousCore.SystemDB
{
    public partial class SystemClassDB
    {
        public class Star           // returned when you get Star data
        {
            public string Name;
            public int X, Y, Z;
            public long EDSMId;
            public int GridId;
            public float Xf { get { return (float)X / XYZScalar; } }
            public float Yf { get { return (float)Y / XYZScalar; } }
            public float Zf { get { return (float)Z / XYZScalar; } }

            public Star(string name, int x, int y, int z, long edsmid, int gridid)
            {
                Name = name; this.X = x; this.Y = y; this.Z = z; EDSMId = edsmid; GridId = gridid;
            }
            public Star(string name, int x, int y, int z, long edsmid)
            {
                Name = name; this.X = x; this.Y = y; this.Z = z; EDSMId = edsmid; GridId = EliteDangerousCore.SystemDB.GridId.Id(x, z);
            }
        }

        public static List<Star> FindStars(string extraconditions = null)
        {
            List<Star> ret = new List<Star>();

            using (SQLiteConnectionSystem cn = new SQLiteConnectionSystem(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Writer))
            {
                DbCommand selectSysCmd = cn.CreateCommand("SELECT c.name,s.name,s.x,s.y,s.z,s.edsmid,n.Name,c.gridid FROM Systems s LEFT OUTER JOIN Names n On s.name=n.id  LEFT OUTER JOIN Sectors c on c.id=s.sector " + (extraconditions.HasChars() ? (" " + extraconditions) : ""));

                using (DbDataReader reader = selectSysCmd.ExecuteReader())
                {
                    while (reader.Read())      // if there..
                    {
                        EliteNameClassifier ec = new EliteNameClassifier((ulong)(long)reader[1]);
                        Object r6 = reader[6];
                        if (!Convert.IsDBNull(r6))
                            ec.StarName = (string)r6;
                        ec.SectorName = (string)reader[0];
                        Star s = new Star(ec.ToString(), (int)(long)reader[2], (int)(long)reader[3], (int)(long)reader[4], (long)reader[5], (int)(long)reader[7]);
                        ret.Add(s);
                    }
                }
            }

            return ret;
        }

        public static Star FindStar(string name)
        {
            using (SQLiteConnectionSystem cn = new SQLiteConnectionSystem(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Reader))
            {
                EliteNameClassifier ec = new EliteNameClassifier(name);

                if (ec.IsStandard)
                {
                    using (DbCommand selectSysCmd = cn.CreateCommand("SELECT s.x,s.y,s.z,s.edsmid From Systems s WHERE s.name = @nid AND s.sector in (Select id From Sectors c WHERE c.name=@sname)"))
                    {
                        selectSysCmd.AddParameterWithValue("@nid", ec.ID);
                        selectSysCmd.AddParameterWithValue("@sname", ec.SectorName);

                        using (DbDataReader reader = selectSysCmd.ExecuteReader())
                        {
                            if (reader.Read())
                                return new Star(ec.ToString(), (int)(long)reader[0], (int)(long)reader[1], (int)(long)reader[2], (long)reader[3]);
                        }
                    }
                }
                else
                {
                    using (DbCommand selectSysCmd = cn.CreateCommand("SELECT s.x,s.y,s.z,s.edsmid From Systems s WHERE s.name in (Select id From Names where name=@starname) AND s.sector in (Select id From Sectors c WHERE c.name=@sname)"))
                    {
                        selectSysCmd.AddParameterWithValue("@starname", ec.StarName);
                        selectSysCmd.AddParameterWithValue("@sname", ec.SectorName);

                        using (DbDataReader reader = selectSysCmd.ExecuteReader())
                        {
                            if (reader.Read())
                                return new Star(ec.ToString(), (int)(long)reader[0], (int)(long)reader[1], (int)(long)reader[2], (long)reader[3]);
                        }
                    }
                }
            }

            return null;
        }

        public static List<Star> FindStarWildcard(string name)
        {
            using (SQLiteConnectionSystem cn = new SQLiteConnectionSystem(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Reader))
            {
                EliteNameClassifier ec = new EliteNameClassifier(name);

                List<Star> ret = new List<Star>();

                if (ec.EntryType != EliteNameClassifier.NameType.NonStandard)
                {
                    using (DbCommand selectSysCmd = cn.CreateCommand("SELECT s.name,s.x,s.y,s.z,s.edsmid From Systems s WHERE s.name >= @nidlow And s.name <= @nidhigh AND s.sector in (Select id From Sectors c WHERE c.name=@sname)"))
                    {
                        selectSysCmd.AddParameterWithValue("@nidlow", ec.ID);
                        selectSysCmd.AddParameterWithValue("@nidhigh", ec.IDHigh);
                        selectSysCmd.AddParameterWithValue("@sname", ec.SectorName);

                        using (DbDataReader reader = selectSysCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                EliteNameClassifier s = new EliteNameClassifier((ulong)(long)reader[0]);
                                s.SectorName = ec.SectorName;
                                ret.Add(new Star(s.ToString(), (int)(long)reader[1], (int)(long)reader[2], (int)(long)reader[3], (long)reader[4]));
                            }
                        }
                    }
                }
                else
                {
                    ec.StarName += "%";

                    using (DbCommand selectSysCmd = cn.CreateCommand("SELECT s.x,s.y,s.z,s.edsmid,n.name From Systems s JOIN Names n on n.id = s.name " +
                                                            "WHERE s.name in (Select id From Names where name like @starname) AND s.sector in (Select id From Sectors c WHERE c.name=@sname)"))
                    {
                        selectSysCmd.AddParameterWithValue("@starname", ec.StarName);
                        selectSysCmd.AddParameterWithValue("@sname", ec.SectorName);

                        using (DbDataReader reader = selectSysCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ret.Add(new Star((ec.SectorName != EliteNameClassifier.NonStandard ? ec.SectorName + " " : "") + (string)reader[4], (int)(long)reader[0], (int)(long)reader[1], (int)(long)reader[2], (long)reader[3]));
                            }
                        }
                    }

                    if (ec.SectorName == EliteNameClassifier.NonStandard) // it could be the start of a sector name..
                    {
                        //,c.name,c.gridid
                        using (DbCommand selectSysCmd = cn.CreateCommand("SELECT c.name,s.name,s.x,s.y,s.z,s.edsmid,n.name From Systems s " + 
                                                    "left outer JOIN Names n on n.id = s.name " + 
                                                    "JOIN Sectors c on s.sector = c.id " +
                                                    "WHERE s.sector in (Select id From Sectors where name like @secname)"))
                        {
                            selectSysCmd.AddParameterWithValue("@secname", ec.StarName);

                            using (DbDataReader reader = selectSysCmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    EliteNameClassifier ecs = new EliteNameClassifier((ulong)(long)reader[1]);       // s.name, the nid
                                    Object r6 = reader[6];
                                    if (!Convert.IsDBNull(r6))
                                        ecs.StarName = (string)r6;
                                    ecs.SectorName = (string)reader[0];

                                    ret.Add(new Star(ecs.ToString(), (int)(long)reader[2], (int)(long)reader[3], (int)(long)reader[4], (long)reader[5]));
                                }
                            }
                        }
                    }

                    // what about if its a sector start name

                }
                return ret;
            }
        }


        public static int GetSystemVector<V>(int gridid, ref V[] vertices, ref uint[] colours, int percentage, Func<int,int,int, V> tovect)
        {
            int numvertices = 0;

            vertices = null;
            colours = null;

            Color[] fixedc = new Color[4];
            fixedc[0] = Color.Red;
            fixedc[1] = Color.Orange;
            fixedc[2] = Color.Yellow;
            fixedc[3] = Color.White;

            using (SQLiteConnectionSystem cn = new SQLiteConnectionSystem(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Reader))
            {
                using (DbCommand cmd = cn.CreateCommand("SELECT id,x,y,z from Systems s where s.sector in (Select id From Sectors c where c.gridid=@gridid)"))
                {
                    cmd.AddParameterWithValue("@gridid", gridid);

                    if (percentage < 100)
                        cmd.CommandText += " and ((s.id*2331)%100) <" + percentage.ToStringInvariant();     // bit of random mult in id to mix it up

                    vertices = new V[250000];
                    colours = new uint[250000];

                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long id = (long)reader[0];
                            int x = (int)(long)reader[1];
                            int y = (int)(long)reader[2];
                            int z = (int)(long)reader[3];

                            if (numvertices == vertices.Length)
                            {
                                Array.Resize(ref vertices, vertices.Length + 32768);
                                Array.Resize(ref colours, colours.Length + 32768);
                            }

                            Color basec = fixedc[(id) & 3];
                            int fade = 100 - (((int)id >> 2) & 7) * 8;
                            byte red = (byte)(basec.R * fade / 100);
                            byte green = (byte)(basec.G * fade / 100);
                            byte blue = (byte)(basec.B * fade / 100);
                            colours[numvertices] = BitConverter.ToUInt32(new byte[] { red, green, blue, 255 }, 0);
                            vertices[numvertices++] = tovect(x, y, z);
                        }
                    }

                    Array.Resize(ref vertices, numvertices);
                    Array.Resize(ref colours, numvertices);

                    if (gridid == GridId.SolGrid && vertices != null)    // BODGE do here, better once on here than every star for every grid..
                    {                       // replace when we have a better naming system
                        int solindex = Array.IndexOf(vertices, tovect(0,0,0));
                        if (solindex >= 0)
                            colours[solindex] = 0x00ffff;   //yellow
                    }
                }
            }

            return numvertices;
        }

        public static long GetTotalSystems()
        {
            using (SQLiteConnectionSystem cn = new SQLiteConnectionSystem(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Reader))
            {
                using (DbCommand cmd = cn.CreateCommand("select Count(Id) from Systems"))
                {
                    return (long)cmd.ExecuteScalar();
                }
            }
        }

        public static V[] GetStarPositions<V>(int percentage, Func<int, int, int, V> tovect)  // return star positions..
        {
            long totalsystems = GetTotalSystems();
            V[] ret = new V[totalsystems * percentage / 100 + 1];      // 1 is for luck

            int entry = 0;

            using (SQLiteConnectionSystem cn = new SQLiteConnectionSystem(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Reader))
            {
                using (DbCommand cmd = cn.CreateCommand("select x,y,z from Systems s where ((s.id*2331)%100) <" + percentage.ToStringInvariant()))
                {
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ret[entry++] = tovect((int)(long)reader[0], (int)(long)reader[1], (int)(long)reader[2]);
                        }
                    }
                }
            }

            Array.Resize(ref ret, entry);
            return ret;
        }


        implement the below functions





#if false
        public static void GetSystemListBySqDistancesFrom(BaseUtils.SortedListDoubleDuplicate<ISystem> distlist, double x, double y, double z,
                                                    int maxitems,
                                                    double mindist, double maxdist, bool spherical,
                                                    SQLiteConnectionSystem cn = null)


        public static ISystem GetSystemByPosition(double x, double y, double z, SQLiteConnectionSystem cn)
        {





        public const int metric_nearestwaypoint = 0;     // easiest way to synchronise metric selection..
        public const int metric_mindevfrompath = 1;
        public const int metric_maximum100ly = 2;
        public const int metric_maximum250ly = 3;
        public const int metric_maximum500ly = 4;
        public const int metric_waypointdev2 = 5;

        public static ISystem GetSystemNearestTo(Point3D curpos, Point3D wantedpos, double maxfromcurpos, double maxfromwanted,
                                    int routemethod)
        {


        public static void AddToAutoComplete(List<string> t)
        {
            lock (AutoCompleteAdditionalList)
            {
                AutoCompleteAdditionalList.AddRange(t);
            }
        }

        public static List<string> ReturnSystemListForAutoComplete(string input, Object ctrl)
        {
            List<string> ret = new List<string>();
            ret.AddRange(ReturnOnlyGalMapListForAutoComplete(input, ctrl));
            ret.AddRange(ReturnOnlySystemsListForAutoComplete(input, ctrl));
            return ret;
        }

        public static List<string> ReturnOnlyGalMapListForAutoComplete(string input, Object ctrl)
        {



        system  aliases














        //private static List<Star> GetNames(long sector, string name, SQLiteConnectionSystem cn, bool like = false)  // names are not unique
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


        //        if (t != null)
        //        {
        //            if (ec.IsStandard)                              // standard is easy - code is just the sectorid + the classifer from the name
        //            {
        //                Star s = GetStar(t.id, ec.ID, cn);

        //                if (s != null)
        //                {
        //                    s.Name = ec.ToString();
        //                    return s;
        //                }
        //            }
        //            else
        //            {
        //                //List<long> names = GetNames(ec.StarName, cn);     // all the nid of the names called starname.. there may be >1 since names are not unique in the name table

        //                //foreach (long id in names)
        //                //{
        //                //    Star s = GetStar((ulong)id | ((ulong)(t.id) << EliteNameClassifier.SectorPos), cn);     // they will be unique by sectoreid+nameid.
        //                //    if (s != null)
        //                //    {
        //                //        s.Name = ec.ToString();
        //                //        return s;
        //                //    }
        //                //}
        //            }
        //        }
        //    }

        //    return null;
        //}


        ////public static List<Star> FindStarsWildcard(string name)     // partial name
        ////{
        ////    List<Star> ret = new List<Star>();

        ////    EliteNameClassifier ec = new EliteNameClassifier(name);

        ////    using (SQLiteConnectionSystem cn = new SQLiteConnectionSystem(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Reader))
        ////    {
        ////        if (ec.PartsComplete >= EliteNameClassifier.NameType.Identifier)  // if its def a standard name
        ////        {
        ////            Sector t = GetSector(ec.SectorName, cn);            // get the sector - selection if from this sector

        ////            if (t != null)
        ////            {
        ////                ec.SectorId = t.id;

        ////            }
        ////        }
        ////        else
        ////        {
        ////            if (ec.SectorName != EliteNameClassifier.NonStandard)
        ////            {
        ////                List<Sector> seclist = GetSectors(ec.SectorName, cn);       // sectors named like this - all stars in them named
        ////            }
        ////            else
        ////            {
        ////                Sector t = GetSector(ec.SectorName, cn);            // get the standard sector - selection if from this sector

        ////                List<long> names = GetNames(ec.StarName, cn, true);       // all the nid of the names like starname

        ////                foreach (long id in names)
        ////                {
        ////                    Star s = GetStar((ulong)id | ((ulong)(t.id) << EliteNameClassifier.SectorPos), cn);   

        ////                    if (s != null)
        ////                    {
        ////                        s.Name = ec.ToString();
        ////                        return s;
        ////                    }
        ////                }


        ////            }
        ////        }
        ////    }

        ////    return ret;
        ////}

        //// public enum SystemAskType { AnyStars, PopulatedStars, UnPopulatedStars };            -- this is mixing EDDB and EDSM, belongs in another class
        ////public static int GetSystemVector<V>(int gridid, ref V[] vertices, ref uint[] colours,
        ////SystemAskType ask, int percentage,
        ////Func<float, float, float, V> tovect)

        ////public static List<Point3D> GetStarPositions(int percentage)  // return star positions.. whole galaxy, of this percentage, 2dmap... hmmm


        //// public static List<long> GetEdsmIdsFromName(string name, SQLiteConnectionSystem cn = null, bool uselike = false)
        ////public static List<ISystem> GetSystemsByName(string name, SQLiteConnectionSystem cn = null, bool uselike = false)









        //#endregion


        //#region Helpers

        ////public static Star MakeStar(long sector, ulong nameid, int x, int y, int z, long edsmid, SQLiteConnectionSystem cn)
        ////{
        ////    EliteNameClassifier ec = new EliteNameClassifier(nameid);

        ////    if (ec.IsStandard)
        ////    {
        ////        if (sectoridcache.ContainsKey(sector))
        ////            return new Star(sectoridcache[sector].Name + " " + ec.ToString(), x, y, z, edsmid);
        ////        else
        ////        {
        ////            // get name and sector..
        ////        }
        ////    }
        ////    else
        ////    {
        ////        if (sectoridcache.ContainsKey(sector))
        ////        {
        ////            string name = GetName(ec.NameId, cn);
        ////            return new Star(name, x, y, z, edsmid);
        ////        }
        ////        else
        ////        {
        ////            // get name and sector
        ////        }

        ////    }



        ////    Sector s = GetSector(sector, cn);

        ////    if (s.Name != EliteNameClassifier.NonStandard)            // if we looked up NonStandard, its not used, else add it to name
        ////        name = s.Name + " " + name;

        ////    return new Star(name, x, y, z, edsmid);
        ////}

        //private static Sector GetSector(long id, SQLiteConnectionSystem cn)
        //{
        //    if (sectoridcache.ContainsKey(id))
        //        return sectoridcache[id];
        //    else
        //    {
        //        using (DbCommand selectSectorCmd = cn.CreateCommand("SELECT name,minx,minz,maxx,maxz,gridlist FROM Sectors WHERE id = @nid"))
        //        {
        //            selectSectorCmd.AddParameterWithValue("@nid", id);

        //            using (DbDataReader reader = selectSectorCmd.ExecuteReader())
        //            {
        //                if (reader.Read())      // if there..
        //                {
        //                    Sector t = new Sector((string)reader[0], (int)(long)reader[1], (int)(long)reader[2], (int)(long)reader[3], (int)(long)reader[4],id, (string)reader[5]);
        //                    sectoridcache[t.id] = t;
        //                    sectornamecache[t.Name] = t;
        //                    return t;
        //                }
        //                else
        //                    return null;
        //            }
        //        }
        //    }
        //}

        //private static Sector GetSector(string name, SQLiteConnectionSystem cn)
        //{
        //    if (sectornamecache.ContainsKey(name))
        //        return sectornamecache[name];
        //    else
        //    {
        //        using (DbCommand selectSectorCmd = cn.CreateCommand("SELECT id,minx,minz,maxx,maxz,gridlist FROM Sectors WHERE name = @sname"))
        //        {
        //            selectSectorCmd.AddParameterWithValue("@sname", name);

        //            using (DbDataReader reader = selectSectorCmd.ExecuteReader())
        //            {
        //                if (reader.Read())      // if there..
        //                {
        //                    Sector t = new Sector(name, (int)(long)reader[1], (int)(long)reader[2], (int)(long)reader[3], (int)(long)reader[4], (long)reader[0], (string)reader[5]);
        //                    sectoridcache[t.id] = t;
        //                    sectornamecache[t.Name] = t;
        //                    return t;
        //                }
        //                else
        //                    return null;
        //            }
        //        }
        //    }
        //}

        //private static List<Sector> GetSectors(string wildcardname, SQLiteConnectionSystem cn)
        //{
        //    using (DbCommand selectSectorCmd = cn.CreateCommand("SELECT name,minx,minz,maxx,maxz,id,gridlist FROM Sectors WHERE name like @sname"))
        //    {
        //        selectSectorCmd.AddParameterWithValue("@sname", wildcardname);

        //        List<Sector> ret = new List<Sector>();

        //        using (DbDataReader reader = selectSectorCmd.ExecuteReader())
        //        {
        //            while (reader.Read())      // if there..
        //            {
        //                Sector t = new Sector((string)reader[0], (int)(long)reader[1], (int)(long)reader[2], (int)(long)reader[3], (int)(long)reader[4], (long)reader[5], (string)reader[6]);
        //                sectoridcache[t.id] = t;
        //                sectornamecache[t.Name] = t;
        //                ret.Add(t);
        //            }

        //            return ret;
        //        }
        //    }
        //}


        //private static string GetName(long id, SQLiteConnectionSystem cn)
        //{
        //    if (nameidcache.ContainsKey(id))
        //        return nameidcache[id];
        //    else
        //    {
        //        using (DbCommand selectNameCmd = cn.CreateCommand("SELECT name FROM Names WHERE id=@nid"))
        //        {
        //            selectNameCmd.AddParameterWithValue("@nid", id);

        //            using (DbDataReader reader = selectNameCmd.ExecuteReader())
        //            {
        //                if (reader.Read())      // if there..
        //                {
        //                    string name = (string)reader[0];
        //                    nameidcache[id] = name;
        //                    return name;
        //                }
        //                else
        //                    return null;
        //            }
        //        }
        //    }
        //}

        //private static Star GetStar(long sector, ulong nameid, SQLiteConnectionSystem cn)       // name is returned blank
        //{
        //    using (DbCommand selectNameCmd = cn.CreateCommand("SELECT x,y,z,edsmid FROM Systems WHERE name = @nid AND sector=@sec"))
        //    {
        //        selectNameCmd.AddParameterWithValue("@nid", nameid);
        //        selectNameCmd.AddParameterWithValue("@sec", sector);

        //        using (DbDataReader reader = selectNameCmd.ExecuteReader())
        //        {
        //            if (reader.Read())      // if there..
        //            {
        //                return new Star("", (int)(long)reader[0], (int)(long)reader[1], (int)(long)reader[2], (long)reader[3]);
        //            }
        //            else
        //                return null;
        //        }
        //    }
        //}

#endif

    }
}


