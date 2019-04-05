using EliteDangerousCore;
using EliteDangerousCore.DB;
using EMK.LightGeometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestSQL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            bool deletedb = false;
            bool reloadjson = false;
            bool reloadeddb = true;

            bool printstars = false;
            bool printstarseddb = false;
            bool testdelete = false;
            bool testaliases = true;

            if ( deletedb )
                BaseUtils.FileHelpers.DeleteFileNoError(SQLiteConnectionSystem.dbFile);

            SQLiteConnectionSystem.Initialize();
            //SQLiteConnectionSystem.UpgradeFrom102TypeDB(()=> { return false; },(s)=>System.Diagnostics.Debug.WriteLine(s));
            SQLiteConnectionSystem.UpgradeSystemTableFrom102TypeDB(() => { return false; }, (s) => System.Diagnostics.Debug.WriteLine(s));

            if (reloadjson)
            {
                string infile = @"c:\code\edsm\edsmsystems.10e6.json";
                SQLiteConnectionSystem.UpgradeSystemTableFromFile(infile, null, () => false, (s) => System.Diagnostics.Debug.WriteLine(s));
            }

            if (reloadeddb)
            {
                string infile = @"c:\code\edsm\eddbsystems.json";
                BaseUtils.AppTicks.TickCountLap();
                long updated = SystemsDB.ParseEDDBJSONFile(infile, () => false);
                System.Diagnostics.Debug.WriteLine("EDDB Load : " + BaseUtils.AppTicks.TickCountLap() + " updated " + updated);
                updated = SystemsDB.ParseEDDBJSONFile(infile, () => false);
                System.Diagnostics.Debug.WriteLine("EDDB Load : " + BaseUtils.AppTicks.TickCountLap() + " updated " + updated);
            }

            if (printstars)
            {
                using (StreamWriter wr = new StreamWriter(@"c:\code\edsm\starlistout.lst"))
                {
                    SystemsDB.FindStars(orderby: "s.sectorid,s.edsmid", eddbinfo: false, starreport: (s) =>
                    {
                        wr.WriteLine(s.Name + " " + s.Xi + "," + s.Yi + "," + s.Zi + ", EDSM:" + s.EDSMID + " Grid:" + s.GridID);
                    });
                }
            }

            if (testdelete)
            {
                SystemsDB.RemoveGridSystems(new int[] { 810, 911 });

                using (StreamWriter wr = new StreamWriter(@"c:\code\edsm\starlistout2.lst"))
                {
                    SystemsDB.FindStars(orderby: "s.sectorid,s.edsmid", eddbinfo: false, starreport: (s) =>
                    {
                        wr.WriteLine(s.Name + " " + s.Xi + "," + s.Yi + "," + s.Zi + ", EDSM:" + s.EDSMID + " Grid:" + s.GridID);
                    });
                }
            }

            if ( testaliases )
            {
                string infile = @"c:\code\edsm\hiddensystems.jsonl";
                BaseUtils.AppTicks.TickCountLap();
                long updated = SystemsDB.ParseAliasFile(infile);
                System.Diagnostics.Debug.WriteLine("Alias Load: " + BaseUtils.AppTicks.TickCountLap() + " updated " + updated);
                BaseUtils.AppTicks.TickCountLap();
                string infile2 = @"c:\code\edsm\hiddensystems2.jsonl";
                updated = SystemsDB.ParseAliasFile(infile2);
                System.Diagnostics.Debug.WriteLine("Alias Load: " + BaseUtils.AppTicks.TickCountLap() + " updated " + updated);

                long aliasn;
                aliasn = SystemsDB.FindAlias(-1, "CM Draco");
                System.Diagnostics.Debug.Assert(aliasn == 19700);
                aliasn = SystemsDB.FindAlias(1, null);
                System.Diagnostics.Debug.Assert(aliasn == 19700);
                aliasn = SystemsDB.FindAlias(-1, "CM qwkqkq");
                System.Diagnostics.Debug.Assert(aliasn == -1);
            }

            if (printstarseddb)
            {
                List<ISystem> stars = SystemsDB.FindStars(orderby: "s.sectorid,s.edsmid", eddbinfo: true);
                using (StreamWriter wr = new StreamWriter(@"c:\code\edsm\starlisteddb.lst"))
                {
                    foreach (var s in stars)
                    {
                        wr.Write(s.Name + " " + s.Xi + "," + s.Yi + "," + s.Zi + ", EDSM:" + s.EDSMID + " Grid:" + s.GridID);
                        if (s.EDDBID != 0)
                            wr.Write(" EDDBID:" + s.EDDBID + " " + s.Population + " " + s.Faction + " " + s.Government + " " + s.Allegiance + " " + s.State + " " + s.Security + " " + s.PrimaryEconomy
                                    + " " + s.Power + " " + s.PowerState + " " + s.NeedsPermit + " " + s.EDDBUpdatedAt);
                        wr.WriteLine("");
                    }
                }
            }


            ///////////////////////////////////////////// main tests

            {
                BaseUtils.AppTicks.TickCountLap();
                ISystem s;

                for (int I = 0; I < 100; I++)
                {
                    s = SystemsDB.FindStar("HIP 112535");
                    System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("HIP 112535"));
                }

                System.Diagnostics.Debug.WriteLine("FindStar HIP x for 100: " + BaseUtils.AppTicks.TickCountLap());
            }


            { 
                ISystem s;

                BaseUtils.AppTicks.TickCountLap();

                for (int I = 0; I < 100; I++)
                {
                    s = SystemsDB.FindStar("Tucanae Sector CQ-Y d79");
                    System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Tucanae Sector CQ-Y d79"));
                }

                System.Diagnostics.Debug.WriteLine("Find Standard for 100: " + BaseUtils.AppTicks.TickCountLap());
            }

            {
                ISystem s;
                s = SystemsDB.FindStar("hip 91507");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("HIP 91507") && s.EDDBID == 8856);
                s = SystemsDB.FindStar("Byua Eurk GL-Y d107");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Byua Eurk GL-Y d107") && s.X == -3555.5625 && s.Y == 119.25 && s.Z == 5478.59375);
                s = SystemsDB.FindStar("BD+18 711");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("BD+18 711") && s.Xi == 1700 && s.Yi == -68224 && s.Zi == -225284);
                s = SystemsDB.FindStar("Chamaeleon Sector FG-W b2-3");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Chamaeleon Sector FG-W b2-3") && s.Xi == 71440 && s.Yi == -12288 && s.Zi == 35092);
                s = SystemsDB.FindStar("kanur");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Kanur") && s.Xi == -2832 && s.Yi == -3188 && s.Zi == 12412 && s.EDDBID == 10442);
            }

            {
                ISystem s;
                BaseUtils.AppTicks.TickCountLap();

                for (int I = 0; I < 100; I++)
                {
                    s = SystemsDB.FindStar(2836547);
                    System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Tucanae Sector SP-N b7-6") && s.Xi == 10844 && s.Yi == -18100 && s.Zi == 28036 && s.EDSMID == 2836547 && s.EDDBID == 0);
                }

                System.Diagnostics.Debug.WriteLine("Find EDSMID for 100: " + BaseUtils.AppTicks.TickCountLap());
            }

            {
                BaseUtils.AppTicks.TickCountLap();
                ISystem s = SystemsDB.GetSystemByPosition(-100.7, 166.4, -36.8);
                System.Diagnostics.Debug.Assert(s != null && s.Name == "Col 285 Sector IZ-B b15-2");
                System.Diagnostics.Debug.WriteLine("SysPos1 : " + BaseUtils.AppTicks.TickCountLap());
            }


            {
                ISystem s;
                s = SystemCache.FindSystem("hip 91507");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("HIP 91507") && s.EDDBID == 8856);
                s = SystemCache.FindSystem("hip 91507");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("HIP 91507") && s.EDDBID == 8856);
                s = SystemCache.FindSystem("Byua Eurk GL-Y d107");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Byua Eurk GL-Y d107") && s.X == -3555.5625 && s.Y == 119.25 && s.Z == 5478.59375);
                s = SystemCache.FindSystem("BD+18 711");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("BD+18 711") && s.Xi == 1700 && s.Yi == -68224 && s.Zi == -225284);
                s = SystemCache.FindSystem("Chamaeleon Sector FG-W b2-3");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Chamaeleon Sector FG-W b2-3") && s.Xi == 71440 && s.Yi == -12288 && s.Zi == 35092);
                s = SystemCache.FindSystem("kanur");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Kanur") && s.Xi == -2832 && s.Yi == -3188 && s.Zi == 12412 && s.EDDBID == 10442);
                s = SystemCache.FindSystem(s.EDSMID);
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Kanur") && s.Xi == -2832 && s.Yi == -3188 && s.Zi == 12412 && s.EDDBID == 10442);
                s = SystemCache.FindSystem("CM DRACO");     // this is an alias system
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("CM Draconis") && s.EDSMID == 19700);
            }

            {
                List<ISystem> slist;

                BaseUtils.AppTicks.TickCountLap();

                for (int I = 0; I < 10; I++)
                {
                    slist = SystemsDB.FindStarWildcard("Tucanae Sector CQ-Y");
                    System.Diagnostics.Debug.Assert(slist != null && slist.Count > 20);
                }

                System.Diagnostics.Debug.WriteLine("Find Wildcard Standard trunced: " + BaseUtils.AppTicks.TickCountLap());
            }


            { 
                BaseUtils.AppTicks.TickCountLap();
                List<ISystem> slist;
                for (int I = 0; I < 10; I++)
                {
                    slist = SystemsDB.FindStarWildcard("HIP 6");
                    System.Diagnostics.Debug.Assert(slist != null && slist.Count > 48);
                    foreach (var e in slist)
                        System.Diagnostics.Debug.Assert(e.Name.StartsWith("HIP 6"));
                }

                System.Diagnostics.Debug.WriteLine("Find HIP 6: " + BaseUtils.AppTicks.TickCountLap());
            }

            { 
                BaseUtils.AppTicks.TickCountLap();
                List<ISystem> slist;
                for (int I = 0; I < 10; I++)
                {
                    slist = SystemsDB.FindStarWildcard("USNO-A2.0 127");
                    System.Diagnostics.Debug.Assert(slist != null && slist.Count > 185);
                    foreach (var e in slist)
                        System.Diagnostics.Debug.Assert(e.Name.StartsWith("USNO-A2.0"));
                }

                System.Diagnostics.Debug.WriteLine("Find USNo: " + BaseUtils.AppTicks.TickCountLap());
            }

            {
                List<ISystem> slist;
                BaseUtils.AppTicks.TickCountLap();

                for (int I = 0; I < 10; I++)
                {
                    slist = SystemsDB.FindStarWildcard("HIP");
                    System.Diagnostics.Debug.Assert(slist != null && slist.Count > 48);
                }

                System.Diagnostics.Debug.WriteLine("Find HIP: " + BaseUtils.AppTicks.TickCountLap());

            }
            {
                List<ISystem> slist;

                slist = SystemsDB.FindStarWildcard("Synuefai MC-H");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 3);
                slist = SystemsDB.FindStarWildcard("Synuefai MC-H c");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 3);
                slist = SystemsDB.FindStarWildcard("Synuefai MC-H c12");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count == 0);
                slist = SystemsDB.FindStarWildcard("Synuefai MC-H c12-");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 3);

                slist = SystemsDB.FindStarWildcard("HIP 6");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count > 5);

                slist = SystemsDB.FindStarWildcard("Coalsack Sector");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 4);

                slist = SystemsDB.FindStarWildcard("Coalsack");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 4);

                slist = SystemsDB.FindStarWildcard("4 S");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 1);


            }




            {
                BaseUtils.SortedListDoubleDuplicate<ISystem> list = new BaseUtils.SortedListDoubleDuplicate<ISystem>();

                BaseUtils.AppTicks.TickCountLap();
                double x = 0, y = 0, z = 0;

                SystemsDB.GetSystemListBySqDistancesFrom(list, x, y, z, 20000, 0.5, 20, true);
                System.Diagnostics.Debug.WriteLine("Stars Near Sol: " + BaseUtils.AppTicks.TickCountLap());
                System.Diagnostics.Debug.Assert(list != null && list.Count >= 20);

                //foreach (var k in list)   System.Diagnostics.Debug.WriteLine(Math.Sqrt(k.Key).ToString("N1") + " Star " + k.Value.ToStringVerbose());
            }

            { 
                BaseUtils.SortedListDoubleDuplicate<ISystem> list = new BaseUtils.SortedListDoubleDuplicate<ISystem>();

                BaseUtils.AppTicks.TickCountLap();
                double x = 490, y = 0, z = 0;

                SystemsDB.GetSystemListBySqDistancesFrom(list, x, y, z, 20000, 0.5, 50, true); //should span 2 grids 810/811
                System.Diagnostics.Debug.WriteLine("Stars Near x490: " + BaseUtils.AppTicks.TickCountLap());
                System.Diagnostics.Debug.Assert(list != null && list.Count >= 20);

                //foreach (var k in list) System.Diagnostics.Debug.WriteLine(Math.Sqrt(k.Key).ToString("N1") + " Star " + k.Value.ToStringVerbose());
            }

            {
                BaseUtils.AppTicks.TickCountLap();
                uint[] colours = null;
                Vector3[] vertices = null;
                SystemsDB.GetSystemVector(810, ref vertices, ref colours, 100, (x, y, z) => { return new Vector3((float)x / 128.0f, (float)y / 128.0f, (float)z / 128.0f); });
                System.Diagnostics.Debug.WriteLine("810 load 100 : " + BaseUtils.AppTicks.TickCountLap());

                BaseUtils.AppTicks.TickCountLap();
                uint[] colours2 = null;
                Vector3[] vertices2 = null;
                SystemsDB.GetSystemVector(810, ref vertices2, ref colours2, 50, (x, y, z) => { return new Vector3((float)x / 128.0f, (float)y / 128.0f, (float)z / 128.0f); });
                System.Diagnostics.Debug.Assert(vertices.Length >= vertices2.Length * 2);
                System.Diagnostics.Debug.WriteLine("810 load 50 : " + BaseUtils.AppTicks.TickCountLap());

                int lengthall = vertices.Length;

                BaseUtils.AppTicks.TickCountLap();
                SystemsDB.GetSystemVector(810, ref vertices, ref colours, ref vertices2, ref colours2, 100, (x, y, z) => { return new Vector3((float)x / 128.0f, (float)y / 128.0f, (float)z / 128.0f); });
                System.Diagnostics.Debug.Assert(vertices.Length >= 20000);
                System.Diagnostics.Debug.Assert(vertices2.Length >= 300000);
                System.Diagnostics.Debug.Assert(vertices.Length + vertices2.Length == lengthall);
                System.Diagnostics.Debug.WriteLine("810 load dual : " + BaseUtils.AppTicks.TickCountLap());


            }

            {
                BaseUtils.AppTicks.TickCountLap();
                ISystem s;
                s = SystemsDB.GetSystemNearestTo(new Point3D(100, 0, 0), new Point3D(1, 0, 0), 110, 20, SystemsDB.metric_waypointdev2);
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Sol"));
                System.Diagnostics.Debug.WriteLine("Find Nearest Star: " + BaseUtils.AppTicks.TickCountLap());

            }

            {
                SystemCache.AddToAutoCompleteList(new List<string>() { "galone", "galtwo", "sol2" });
                List<string> sys;
                sys = SystemCache.ReturnSystemListForAutoComplete("Sol", null);
                System.Diagnostics.Debug.Assert(sys != null && sys.Contains("Sol") && sys.Count >= 6);
            }



            {
                var v = SystemsDB.GetStarPositions(5, (x, y, z) => { return new Vector3((float)x / 128.0f, (float)y / 128.0f, (float)z / 128.0f); });
         //       var v2 = SystemClassDB.GetStarPositions(100, (x, y, z) => { return new Vector3((float)x / 128.0f, (float)y / 128.0f, (float)z / 128.0f); });
            }
        }
    }
}
