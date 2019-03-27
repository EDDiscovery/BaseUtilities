using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using System.Data;
using System.Drawing;
using EMK.LightGeometry;

namespace EliteDangerousCore.DB
{
    public partial class SystemsDB
    {
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

        // Beware with no extra conditions, you get them all..  No EDDB info. Mostly used for debugging
        public static List<ISystem> FindStars(string extraconditions)  
        {
            List<ISystem> ret = new List<ISystem>();

            using (SQLiteConnectionSystem cn = new SQLiteConnectionSystem(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Writer))
            {
                DbCommand selectSysCmd = cn.CreateCommand("SELECT c.name,s.name,s.x,s.y,s.z,s.edsmid,n.Name,c.gridid FROM Systems s LEFT OUTER JOIN Names n On s.name=n.id  LEFT OUTER JOIN Sectors c on c.id=s.sector " + (extraconditions.HasChars() ? (" " + extraconditions) : ""));

                using (DbDataReader reader = selectSysCmd.ExecuteReader())
                {
                    while (reader.Read())      // if there..
                    {
                        SystemClass s = FromReaderDBInStdOrder(reader);
                        ret.Add(s);
                    }
                }
            }

            return ret;
        }

        public static ISystem FindStar(string name)
        {
            using (SQLiteConnectionSystem cn = new SQLiteConnectionSystem(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Reader))
            {
                EliteNameClassifier ec = new EliteNameClassifier(name);

                if (ec.IsStandard)
                {
                    using (DbCommand selectSysCmd = cn.CreateCommand(
                        "SELECT s.x,s.y,s.z,s.edsmid,e.properties " + 
                        "FROM Systems s " +
                        "LEFT OUTER JOIN EDDB e ON e.edsmid = s.edsmid " +
                        "WHERE s.name = @nid AND s.sector IN (Select id FROM Sectors c WHERE c.name=@sname)"
                        ))
                    {
                        selectSysCmd.AddParameterWithValue("@nid", ec.ID);
                        selectSysCmd.AddParameterWithValue("@sname", ec.SectorName);

                        using (DbDataReader reader = selectSysCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var x = reader[4];
                                return new SystemClass(ec.ToString(), (int)(long)reader[0], (int)(long)reader[1], (int)(long)reader[2], (long)reader[3], eddbproperties: reader.ConvertTo<string>(4));
                            }
                        }
                    }
                }
                else
                {
                    using (DbCommand selectSysCmd = cn.CreateCommand(
                        "SELECT s.x,s.y,s.z,s.edsmid,e.properties " +
                        "FROM Systems s " +
                        "LEFT OUTER JOIN EDDB e ON e.edsmid = s.edsmid " +
                        "WHERE s.name IN (Select id FROM Names WHERE name=@starname) AND s.sector IN (Select id FROM Sectors c WHERE c.name=@sname)"
                        ))
                    {
                        selectSysCmd.AddParameterWithValue("@starname", ec.StarName);
                        selectSysCmd.AddParameterWithValue("@sname", ec.SectorName);

                        using (DbDataReader reader = selectSysCmd.ExecuteReader())
                        {
                            if (reader.Read())
                                return new SystemClass(ec.ToString(), (int)(long)reader[0], (int)(long)reader[1], (int)(long)reader[2], (long)reader[3], eddbproperties: reader.ConvertTo<string>(4));
                        }
                    }
                }
            }

            return null;
        }

        public static ISystem FindStar(long edsmid)
        {
            return null;
        }

        public static List<ISystem> FindStarWildcard(string name)
        {
            using (SQLiteConnectionSystem cn = new SQLiteConnectionSystem(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Reader))
            {
                EliteNameClassifier ec = new EliteNameClassifier(name);

                List<ISystem> ret = new List<ISystem>();

                if (ec.EntryType != EliteNameClassifier.NameType.NonStandard)
                {
                    using (DbCommand selectSysCmd = cn.CreateCommand(
                            "SELECT s.name,s.x,s.y,s.z,s.edsmid,e.properties " + 
                            "FROM Systems s " +
                            "LEFT OUTER JOIN EDDB e ON e.edsmid = s.edsmid " + // e.properties is optional return
                            "WHERE s.name >= @nidlow AND s.name <= @nidhigh AND s.sector IN (Select id FROM Sectors c WHERE c.name=@sname)"
                            ))
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
                                ret.Add(new SystemClass(s.ToString(), (int)(long)reader[1], (int)(long)reader[2], (int)(long)reader[3], (long)reader[4], eddbproperties: reader.ConvertTo<string>(5)));
                            }
                        }
                    }
                }
                else
                {
                    ec.StarName += "%";

                    using (DbCommand selectSysCmd = cn.CreateCommand(
                            "SELECT s.x,s.y,s.z,s.edsmid,n.name,e.properties " + 
                            "FROM Systems s " +
                            "JOIN Names n ON n.id = s.name " +
                            "LEFT OUTER JOIN EDDB e ON e.edsmid = s.edsmid " +  // e.properites is optional return
                            "WHERE s.name IN (Select id FROM Names WHERE name LIKE @starname) AND s.sector IN (Select id FROM Sectors c WHERE c.name=@sname)"
                            ))
                    {
                        selectSysCmd.AddParameterWithValue("@starname", ec.StarName);
                        selectSysCmd.AddParameterWithValue("@sname", ec.SectorName);

                        using (DbDataReader reader = selectSysCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string nameout = (ec.SectorName != EliteNameClassifier.NonStandard ? ec.SectorName + " " : "") + (string)reader[4];
                                ret.Add(new SystemClass(nameout, (int)(long)reader[0], (int)(long)reader[1], (int)(long)reader[2], (long)reader[3] , eddbproperties: reader.ConvertTo<string>(5)));
                            }
                        }
                    }

                    if (ec.SectorName == EliteNameClassifier.NonStandard) // it could be the start of a sector name..
                    {
                        //,c.name,c.gridid
                        using (DbCommand selectSysCmd = cn.CreateCommand(
                            "SELECT c.name,s.name,s.x,s.y,s.z,s.edsmid,n.name,e.properties " +
                            "FROM Systems s " +
                            "JOIN Sectors c ON s.sector = c.id " +
                            "LEFT OUTER JOIN Names n ON s.name=n.id " +     // n.Name is optional return
                            "LEFT OUTER JOIN EDDB e ON e.edsmid = s.edsmid " +  //e.properites is optional return
                            "WHERE s.sector IN (Select id FROM Sectors WHERE name LIKE @secname)"))
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

                                    ret.Add(new SystemClass(ecs.ToString(), (int)(long)reader[2], (int)(long)reader[3], (int)(long)reader[4], (long)reader[5], eddbproperties: reader.ConvertTo<string>(7)));
                                }
                            }
                        }
                    }
                }

                return ret;
            }
        }

        
        public static void GetSystemListBySqDistancesFrom(BaseUtils.SortedListDoubleDuplicate<ISystem> distlist, double x, double y, double z,
                                                    int maxitems,
                                                    double mindist, double maxdist, bool spherical,
                                                    Action<ISystem> AddedTo = null
                                                    )

        {
            using (SQLiteConnectionSystem cn = new SQLiteConnectionSystem(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Reader))
            {
                using (DbCommand cmd = cn.CreateCommand(
                    "SELECT c.name,s.name,s.x,s.y,s.z,s.edsmid,n.Name,c.gridid,e.properties " +
                    "FROM Systems s " +
                    "JOIN Sectors c ON c.id = s.sector " +
                    "LEFT OUTER JOIN Names n ON s.name=n.id " +     // n.Name is optional return
                    "LEFT OUTER JOIN EDDB e ON e.edsmid = s.edsmid " +  //e.properites is optional return
                    "WHERE s.x >= @xv - @maxdist " +
                    "AND s.x <= @xv + @maxdist " +
                    "AND s.y >= @yv - @maxdist " +
                    "AND s.y <= @yv + @maxdist " +
                    "AND s.z >= @zv - @maxdist " +
                    "AND s.z <= @zv + @maxdist " +
                    "AND (s.x-@xv)*(s.x-@xv)+(s.y-@yv)*(s.y-@yv)+(s.z-@zv)*(s.z-@zv)>=@mindistsq " +     // tried a direct spherical lookup using <=maxdist, too slow
                    "ORDER BY (s.x-@xv)*(s.x-@xv)+(s.y-@yv)*(s.y-@yv)+(s.z-@zv)*(s.z-@zv) " +
                    "LIMIT @max"
                    ))
                {
                    //System.Diagnostics.Debug.WriteLine("DB query " + maxitems + " " + mindist + " " + maxdist);
                    cmd.AddParameterWithValue("@xv", SystemClass.DoubleToInt(x));
                    cmd.AddParameterWithValue("@yv", SystemClass.DoubleToInt(y));
                    cmd.AddParameterWithValue("@zv", SystemClass.DoubleToInt(z));
                    cmd.AddParameterWithValue("@max", maxitems + 1);     // 1 more, because if we artre on a SystemClass, that will be returned
                    cmd.AddParameterWithValue("@maxdist", SystemClass.DoubleToInt(maxdist));
                    cmd.AddParameterWithValue("@mindistsq", SystemClass.DoubleToInt(mindist) * SystemClass.DoubleToInt(mindist));  // note in square terms

                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())// && distlist.Count < maxitems)           // already sorted, and already limited to max items
                        {
                            SystemClass s = FromReaderDBInStdOrder(reader, reader.ConvertTo<string>(8));
                            double distsq = s.DistanceSq(x, y, z);
                            if ((!spherical || distsq <= maxdist * maxdist))// MUST use duplicate double list to protect against EDSM having two at the same point
                            {
                                distlist.Add(distsq, s);                  // which Rob has seen crashing the program! Bad EDSM!
                                AddedTo?.Invoke(s);         // callback to say added to.
                            }
                        }
                    }
                }
            }
        }


        public static ISystem FindNearestSystemTo(double x, double y, double z, double maxdistance = 1000)
        {
            BaseUtils.SortedListDoubleDuplicate<ISystem> distlist = new BaseUtils.SortedListDoubleDuplicate<ISystem>();
            GetSystemListBySqDistancesFrom(distlist, x, y, z, 1, 0.0, maxdistance, false);
            return distlist.Select(v => v.Value).FirstOrDefault();
        }

        // null if not found

        public static ISystem GetSystemByPosition(double x, double y, double z)
        {
            using (SQLiteConnectionSystem cn = new SQLiteConnectionSystem(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Reader))
            {
                using (DbCommand cmd = cn.CreateCommand(
                    "SELECT c.name,s.name,s.x,s.y,s.z,s.edsmid,n.Name,c.gridid,e.properties " +
                    "FROM Systems s " +
                    "JOIN Sectors c ON c.id = s.sector " +
                    "LEFT OUTER JOIN Names n ON s.name=n.id " +     // n.Name is optional return
                    "LEFT OUTER JOIN EDDB e ON e.edsmid = s.edsmid " +  //e.properites is optional return
                    "WHERE s.X >= @X - 16 " +
                    "AND s.X <= @X + 16 " +
                    "AND s.Y >= @Y - 16 " +
                    "AND s.Y <= @Y + 16 " +
                    "AND s.Z >= @Z - 16 " +
                    "AND s.Z <= @Z + 16 " +
                    "LIMIT 1"))
                {
                    cmd.AddParameterWithValue("@X", SystemClass.DoubleToInt(x));
                    cmd.AddParameterWithValue("@Y", SystemClass.DoubleToInt(y));
                    cmd.AddParameterWithValue("@Z", SystemClass.DoubleToInt(z));

                    using (DbDataReader reader = cmd.ExecuteReader())        // MEASURED very fast, <1ms
                    {
                        while (reader.Read())
                        {
                            return FromReaderDBInStdOrder(reader, reader.ConvertTo<string>(8));
                        }
                    }
                }
            }

            return null;
        }


        // TBC
        public const int metric_nearestwaypoint = 0;     // easiest way to synchronise metric selection..
        public const int metric_mindevfrompath = 1;
        public const int metric_maximum100ly = 2;
        public const int metric_maximum250ly = 3;
        public const int metric_maximum500ly = 4;
        public const int metric_waypointdev2 = 5;

        public static ISystem GetSystemNearestTo(Point3D currentpos,
                                              Point3D wantedpos,
                                              double maxfromcurpos,
                                              double maxfromwanted,
                                              int routemethod)
        {
            using (SQLiteConnectionSystem cn = new SQLiteConnectionSystem(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Reader))
            {
                using (DbCommand cmd = cn.CreateCommand(
                    "SELECT c.name,s.name,s.x,s.y,s.z,s.edsmid,n.Name,c.gridid,e.properties " +
                    "FROM Systems s " +
                    "JOIN Sectors c on c.id = s.sector " +
                    "LEFT OUTER JOIN Names n on s.name=n.id " + //n.name is optional return
                    "LEFT OUTER JOIN EDDB e ON e.edsmid = s.edsmid " +  //e.properites is optional return
                    "WHERE x >= @xc - @maxfromcurpos " +
                    "AND x <= @xc + @maxfromcurpos " +
                    "AND y >= @yc - @maxfromcurpos " +
                    "AND y <= @yc + @maxfromcurpos " +
                    "AND z >= @zc - @maxfromcurpos " +
                    "AND z <= @zc + @maxfromcurpos " +
                    "AND x >= @xw - @maxfromwanted " +
                    "AND x <= @xw + @maxfromwanted " +
                    "AND y >= @yw - @maxfromwanted " +
                    "AND y <= @yw + @maxfromwanted " +
                    "AND z >= @zw - @maxfromwanted " +
                    "AND z <= @zw + @maxfromwanted "))
                {
                    cmd.AddParameterWithValue("@xw", SystemClass.DoubleToInt(wantedpos.X));
                    cmd.AddParameterWithValue("@yw", SystemClass.DoubleToInt(wantedpos.Y));
                    cmd.AddParameterWithValue("@zw", SystemClass.DoubleToInt(wantedpos.Z));
                    cmd.AddParameterWithValue("@maxfromwanted", SystemClass.DoubleToInt(maxfromwanted));

                    cmd.AddParameterWithValue("@xc", SystemClass.DoubleToInt(currentpos.X));
                    cmd.AddParameterWithValue("@yc", SystemClass.DoubleToInt(currentpos.Y));
                    cmd.AddParameterWithValue("@zc", SystemClass.DoubleToInt(currentpos.Z));
                    cmd.AddParameterWithValue("@maxfromcurpos", SystemClass.DoubleToInt(maxfromcurpos));

                    double bestmindistance = double.MaxValue;
                    SystemClass nearestsystem = null;

                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SystemClass s = FromReaderDBInStdOrder(reader, reader.ConvertTo<string>(8));

                            Point3D syspos = new Point3D(s.X, s.Y, s.Z);
                            double distancefromwantedx2 = Point3D.DistanceBetweenX2(wantedpos, syspos); // range between the wanted point and this, ^2
                            double distancefromcurposx2 = Point3D.DistanceBetweenX2(currentpos, syspos);    // range between the wanted point and this, ^2

                            // ENSURE its withing the circles now
                            if (distancefromcurposx2 <= (maxfromcurpos * maxfromcurpos) && distancefromwantedx2 <= (maxfromwanted * maxfromwanted))
                            {
                                if (routemethod == metric_nearestwaypoint)
                                {
                                    if (distancefromwantedx2 < bestmindistance)
                                    {
                                        nearestsystem = s;
                                        bestmindistance = distancefromwantedx2;
                                    }
                                }
                                else
                                {
                                    Point3D interceptpoint = currentpos.InterceptPoint(wantedpos, syspos);      // work out where the perp. intercept point is..
                                    double deviation = Point3D.DistanceBetween(interceptpoint, syspos);
                                    double metric = 1E39;

                                    if (routemethod == metric_mindevfrompath)
                                        metric = deviation;
                                    else if (routemethod == metric_maximum100ly)
                                        metric = (deviation <= 100) ? distancefromwantedx2 : metric;        // no need to sqrt it..
                                    else if (routemethod == metric_maximum250ly)
                                        metric = (deviation <= 250) ? distancefromwantedx2 : metric;
                                    else if (routemethod == metric_maximum500ly)
                                        metric = (deviation <= 500) ? distancefromwantedx2 : metric;
                                    else if (routemethod == metric_waypointdev2)
                                        metric = Math.Sqrt(distancefromwantedx2) + deviation / 2;

                                    if (metric < bestmindistance)
                                    {
                                        nearestsystem = s;
                                        bestmindistance = metric;
                                    }
                                }
                            }
                        }
                    }

                    return nearestsystem;
                }
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
                using (DbCommand cmd = cn.CreateCommand(
                    "SELECT id,x,y,z " + 
                    "FROM Systems s " + 
                    "WHERE s.sector IN (Select id FROM Sectors c WHERE c.gridid=@gridid)"))
                {
                    cmd.AddParameterWithValue("@gridid", gridid);

                    if (percentage < 100)
                        cmd.CommandText += " AND ((s.id*2331)%100) <" + percentage.ToStringInvariant();     // bit of random mult in id to mix it up

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

        public static V[] GetStarPositions<V>(int percentage, Func<int, int, int, V> tovect)  // return star positions..
        {
            long totalsystems = GetTotalSystems();
            V[] ret = new V[totalsystems * percentage / 100 + 1];      // 1 is for luck

            int entry = 0;

            using (SQLiteConnectionSystem cn = new SQLiteConnectionSystem(mode: SQLLiteExtensions.SQLExtConnection.AccessMode.Reader))
            {
                using (DbCommand cmd = cn.CreateCommand(
                    "SELECT x,y,z " +
                    "FROM Systems s " + 
                    "WHERE ((s.id*2331)%100) <" + percentage.ToStringInvariant()))
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


        //helper function - order of paras must be SELECT c.name,s.name,s.x,s.y,s.z,s.edsmid,n.Name,c.gridid .  EDDB prop is optional
        private static SystemClass FromReaderDBInStdOrder(DbDataReader reader, string eddbproperties = null)
        {
            EliteNameClassifier ec = new EliteNameClassifier((ulong)(long)reader[1]);
            Object r6 = reader[6];
            if (!Convert.IsDBNull(r6))
                ec.StarName = (string)r6;
            ec.SectorName = (string)reader[0];
            return new SystemClass(ec.ToString(), (int)(long)reader[2], (int)(long)reader[3], (int)(long)reader[4], (long)reader[5], (int)(long)reader[7], eddbproperties);
        }


    }
}


