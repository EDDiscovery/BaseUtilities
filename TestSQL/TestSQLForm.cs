using EliteDangerousCore;
using EliteDangerousCore.DB;
using EMK.LightGeometry;
using QuickJSON;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using static EliteDangerousCore.DB.SystemsDB;

namespace TestSQL
{
    public partial class TestSQLForm : Form
    {
        string spanshsmallfile = @"c:\code\examples\edsm\systems_1week.json";      
        string spansh1mfile = @"c:\code\examples\edsm\systems_1month.json";       
        string spansh6mfile = @"c:\code\examples\edsm\systems_6months.json";        
        string edsminfile = @"c:\code\examples\edsm\edsmsystems.10e6.json";
        //string testfile = @"c:\code\test.json";

        public TestSQLForm()
        {
          //  System.Globalization.CultureInfo.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("de-de");

            InitializeComponent();

            SystemsDatabase.Instance.MaxThreads = 8;
            SystemsDatabase.Instance.MinThreads = 2;
            SystemsDatabase.Instance.MultiThreaded = true;
            SystemsDatabase.Instance.Initialize();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            SystemsDatabase.Instance.Stop();
        }

        void WriteLog(string s)
        {
            richTextBox.AppendText(s + Environment.NewLine);
            richTextBox.ScrollToCaret();
        }

        void CheckDB(string file, bool spansh, Action<Action> invoke)
        {
            using (StreamReader sr = new StreamReader(file))         // read directly from file..
            {
                var parser = new QuickJSON.Utils.StringParserQuickTextReader(sr, 32768);
                var enumerator = JToken.ParseToken(parser, JToken.ParseOptions.None).GetEnumerator();

                bool stop = false;
                int c = 0;

                while (!stop)
                {
                    if (!enumerator.MoveNext())        // get next token, if not, stop eof
                        stop = true;

                    if (stop == false)      // if not stopping, try for next record
                    {
                        JToken token = enumerator.Current;                  // may throw
                        if (token.IsObject)                   // if start of object..
                        {
                            StarFileEntry d = new StarFileEntry();

                            if (d.Deserialize(enumerator))
                            {
                                if (c++ % 10000 == 0)
                                    System.Diagnostics.Debug.WriteLine($"Star count {c - 1}");

                                //System.Diagnostics.Debug.WriteLine($"{d.date} {d.id} {d.name} {d.x} {d.y} {d.z} {d.startype}");

                                var syslist = SystemsDB.FindStars(d.name);
                                string err = null;

                                if (syslist.Count == 0)
                                {
                                    err = $"{d.date} {d.id} {d.name} {d.x} {d.y} {d.z} {d.startype} NOT FOUND";
                                }
                                else if ( syslist.Count > 1)
                                {
                                    err = $"{d.name} : {syslist.Count} Repeats noted";
                                }
                                else if (syslist[0].Xi != d.x || syslist[0].Yi != d.y || syslist[0].Zi != d.z)
                                {
                                    err = $"{d.date} {d.id} {d.name} {d.x} {d.y} {d.z} {d.startype} BAD COORD";
                                }
                                else if (spansh ? (syslist[0].SystemAddress != (long)d.id) : (syslist[0].EDSMID != (long)d.id))
                                {
                                    err = $"{d.date} {d.id} {d.name} {d.x} {d.y} {d.z} {d.startype}  ID DIFF";
                                }

                                if ( err != null )
                                {
                                    System.Diagnostics.Debug.WriteLine(err);
                                    invoke(() => WriteLog(err));
                                }

                                //if (c == 99999)                           break;
                            }
                        }
                    }
                }
            }

            invoke(() => WriteLog("Finished DB Scan"));
        }

        #region DB
        private void buttonClearDB_Click(object sender, EventArgs e)
        {
            SystemsDatabase.Instance.Stop();
            System.Diagnostics.Debug.Assert(SystemsDatabase.ccount == 0);

            File.Delete(EliteDangerousCore.EliteConfigInstance.InstanceOptions.SystemDatabasePath);

            SystemsDatabase.Reset();
            SystemsDatabase.Instance.MaxThreads = 8;
            SystemsDatabase.Instance.MinThreads = 2;
            SystemsDatabase.Instance.MultiThreaded = true;
            SystemsDatabase.Instance.Initialize();

            WriteLog("Clear DB");
        }

        #endregion

        #region EDSM

        private void buttonMakeEDSMOld_Click(object sender, System.EventArgs e)
        {
            SystemsDatabase.Instance.MakeSystemTableFromFile(edsminfile, null, 500000, () => false, (s) => System.Diagnostics.Debug.WriteLine(s), method:0);
            WriteLog($"EDSM DB Made OLD {SystemsDB.GetTotalSystems()}");
        }

        private void buttonMakeEDSML2_Click(object sender, EventArgs e)
        {
            SystemsDatabase.Instance.MakeSystemTableFromFile(edsminfile, null, 200000, () => false, (s) => System.Diagnostics.Debug.WriteLine(s), method: 2);
            WriteLog($"EDSM DB Made L2 {SystemsDB.GetTotalSystems()}");
        }
        private void buttonMakeEDSML3_Click(object sender, EventArgs e)
        {
            SystemsDatabase.Instance.MakeSystemTableFromFile(edsminfile, null, 200000, () => false, (s) => System.Diagnostics.Debug.WriteLine(s), method: 3);
            WriteLog($"EDSM DB Made L3 {SystemsDB.GetTotalSystems()}");
        }

        private void buttonCheckEDSMMadeStars_Click(object sender, EventArgs e)
        {
            System.Threading.Tasks.Task.Run(() => { CheckDB(edsminfile, false, (k) => BeginInvoke(k)); });
        }


        private void buttonTestEDSM_Click(object sender, System.EventArgs argse)
        {
            bool printstars = false;
            bool testdelete = false;

            if (printstars)
            {
                using (StreamWriter wr = new StreamWriter(@"c:\code\edsm\starlistout.lst"))
                {
                    SystemsDB.ListStars(orderby: "s.sectorid,s.edsmid", starreport: (s) =>
                    {
                        wr.WriteLine(s.Name + " " + s.Xi + "," + s.Yi + "," + s.Zi + " Grid:" + s.GridID);
                    });
                }
            }

            if (testdelete)
            {
                SystemsDB.RemoveGridSystems(new int[] { 810, 911 });

                using (StreamWriter wr = new StreamWriter(@"c:\code\edsm\starlistout2.lst"))
                {
                    SystemsDB.ListStars(orderby: "s.sectorid,s.edsmid", starreport: (s) =>
                    {
                        wr.WriteLine(s.Name + " " + s.Xi + "," + s.Yi + "," + s.Zi + " Grid:" + s.GridID);
                    });
                }
            }

            // ********************************************
            // TESTS BASED on the 10e6 json file
            // ********************************************

            {
                BaseUtils.AppTicks.TickCountLap();
                
                for (int I = 0; I < 50; I++)    // 6/4/18 50 @ 38       (76 no index on systems)
                {
                    var sl = SystemsDB.FindStars("HIP 112535");       // this one is at the front of the DB
                    System.Diagnostics.Debug.Assert(sl.Count > 0 && sl[0].Name.Equals("HIP 112535"));
                }

                WriteLog("FindStar HIP x for X: " + BaseUtils.AppTicks.TickCountLap());
            }


            {
                BaseUtils.AppTicks.TickCountLap();
                string star = "HIP 101456";
                for (int I = 0; I < 50; I++)
                {
                    var sl = SystemsDB.FindStars(star);       // This one is at the back of the DB
                    System.Diagnostics.Debug.Assert(sl.Count > 0 && sl[0].Name.Equals(star));
                    //   AddText("Lap : " + BaseUtils.AppTicks.TickCountLap());
                }

                WriteLog("Find Standard for X: " + BaseUtils.AppTicks.TickCountLap());
            }

            {
                BaseUtils.AppTicks.TickCountLap();

                for (int I = 0; I < 50; I++)        // 6/4/18 50 @ 26ms (No need for system index)
                {
                    var sl = SystemsDB.FindStars("kanur");
                    System.Diagnostics.Debug.Assert(sl.Count > 0 && sl[0].Name.Equals("Kanur") && sl[0].Xi == -2832 && sl[0].Yi == -3188 && sl[0].Zi == 12412);
                }

                WriteLog("Find Kanur for X: " + BaseUtils.AppTicks.TickCountLap());
            }

            {
                List<ISystem> s;
                s = SystemsDB.FindStars("hip 91507");
                System.Diagnostics.Debug.Assert(s.Count>0 && s[0].Name.Equals("HIP 91507"));
                s = SystemsDB.FindStars("Byua Eurk GL-Y d107");
                System.Diagnostics.Debug.Assert(s.Count>0 && s[0].Name.Equals("Byua Eurk GL-Y d107") && s[0].X == -3555.5625 && s[0].Y == 119.25 && s[0].Z == 5478.59375);
                s = SystemsDB.FindStars("BD+18 711");
                System.Diagnostics.Debug.Assert(s.Count>0 && s[0].Name.Equals("BD+18 711") && s[0].Xi == 1700 && s[0].Yi == -68224 && s[0].Zi == -225284);
                s = SystemsDB.FindStars("Chamaeleon Sector FG-W b2-3");
                System.Diagnostics.Debug.Assert(s.Count>0 && s[0].Name.Equals("Chamaeleon Sector FG-W b2-3") && s[0].Xi == 71440 && s[0].Yi == -12288 && s[0].Zi == 35092 && s[0].MainStarType == EDStar.Unknown && s[0].SystemAddress == null);
            }


            { // No system indexes = 4179  xz=10 @21, xz=100 @ 176,  x= 100 @ 1375, xz 100 @ 92 xz vacummed 76.
                WriteLog("Begin Find Pos for 100");
                ISystem s;
                BaseUtils.AppTicks.TickCountLap();

                for (int I = 0; I < 100; I++)
                {
                    SystemsDatabase.Instance.DBRead(db =>
                    {
                        s = SystemsDB.GetSystemByPosition(-100.7, 166.4, -36.8, db);
                        System.Diagnostics.Debug.Assert(s != null && s.Name == "Col 285 Sector IZ-B b15-2");
                    });

                    //  AddText("Lap : " + BaseUtils.AppTicks.TickCountLap());
                }

                WriteLog("Find Pos for 100: " + BaseUtils.AppTicks.TickCountLap());
            }


            {
                ISystem s;
                s = SystemCache.FindSystem("hip 91507");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("HIP 91507"));
                s = SystemCache.FindSystem("hip 91507");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("HIP 91507"));
                s = SystemCache.FindSystem("Byua Eurk GL-Y d107");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Byua Eurk GL-Y d107") && s.X == -3555.5625 && s.Y == 119.25 && s.Z == 5478.59375);
                s = SystemCache.FindSystem("BD+18 711");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("BD+18 711") && s.Xi == 1700 && s.Yi == -68224 && s.Zi == -225284);
                s = SystemCache.FindSystem("Chamaeleon Sector FG-W b2-3");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Chamaeleon Sector FG-W b2-3") && s.Xi == 71440 && s.Yi == -12288 && s.Zi == 35092);
                s = SystemCache.FindSystem("kanur");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Kanur") && s.Xi == -2832 && s.Yi == -3188 && s.Zi == 12412);
            }

            {
                List<ISystem> slist;

                BaseUtils.AppTicks.TickCountLap();

                for (int I = 0; I < 10; I++)
                {
                    slist = SystemsDB.FindStarsWildcard("Tucanae Sector CQ-Y");
                    System.Diagnostics.Debug.Assert(slist != null && slist.Count > 20);
                    //AddText("Lap : " + BaseUtils.AppTicks.TickCountLap());
                }

                WriteLog("Find Wildcard Standard trunced: " + BaseUtils.AppTicks.TickCountLap());
            }


            {
                BaseUtils.AppTicks.TickCountLap();
                List<ISystem> slist;
                for (int I = 0; I < 10; I++)
                {
                    slist = SystemsDB.FindStarsWildcard("HIP 6");
                    System.Diagnostics.Debug.Assert(slist != null && slist.Count > 48);
                    foreach (var e in slist)
                        System.Diagnostics.Debug.Assert(e.Name.StartsWith("HIP 6"));
                }

                WriteLog("Find Wildcard HIP 6: " + BaseUtils.AppTicks.TickCountLap());
            }

            {
                BaseUtils.AppTicks.TickCountLap();
                List<ISystem> slist;
                for (int I = 0; I < 10; I++)
                {
                    slist = SystemsDB.FindStarsWildcard("USNO-A2.0 127");
                    System.Diagnostics.Debug.Assert(slist != null && slist.Count > 185);
                    foreach (var e in slist)
                        System.Diagnostics.Debug.Assert(e.Name.StartsWith("USNO-A2.0"));
                }

                WriteLog("Find Wildcard USNo: " + BaseUtils.AppTicks.TickCountLap());
            }

            {
                List<ISystem> slist;
                BaseUtils.AppTicks.TickCountLap();

                for (int I = 0; I < 1; I++)
                {
                    slist = SystemsDB.FindStarsWildcard("HIP");
                    System.Diagnostics.Debug.Assert(slist != null && slist.Count > 48);
                }

                WriteLog("Find Wildcard HIP: " + BaseUtils.AppTicks.TickCountLap());

            }
            {
                List<ISystem> slist;

                slist = SystemsDB.FindStarsWildcard("Synuefai MC-H");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 3);
                slist = SystemsDB.FindStarsWildcard("Synuefai MC-H c");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 3);
                slist = SystemsDB.FindStarsWildcard("Synuefai MC-H c12");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count == 0);
                slist = SystemsDB.FindStarsWildcard("Synuefai MC-H c12-");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 3);

                slist = SystemsDB.FindStarsWildcard("HIP 6");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count > 5);

                slist = SystemsDB.FindStarsWildcard("Coalsack Sector");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 4);

                slist = SystemsDB.FindStarsWildcard("Coalsack");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 4);

                slist = SystemsDB.FindStarsWildcard("4 S");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 1);

            }

            {   // xz index = 70ms
                BaseUtils.SortedListDoubleDuplicate<ISystem> list = new BaseUtils.SortedListDoubleDuplicate<ISystem>();

                BaseUtils.AppTicks.TickCountLap();
                double x = 0, y = 0, z = 0;

                SystemsDatabase.Instance.DBRead(db =>
                {
                    SystemsDB.GetSystemListBySqDistancesFrom(x, y, z, 20000, 0.5, 20, true, db, (dist, sys) => { list.Add(dist, sys); });
                    System.Diagnostics.Debug.Assert(list != null && list.Count >= 20);
                });

                //foreach (var k in list)   AddText(Math.Sqrt(k.Key).ToString("N1") + " Star " + k.Value.ToStringVerbose());
            }

            { // xz index = 185ms
                BaseUtils.SortedListDoubleDuplicate<ISystem> list = new BaseUtils.SortedListDoubleDuplicate<ISystem>();

                BaseUtils.AppTicks.TickCountLap();
                double x = 490, y = 0, z = 0;

                SystemsDatabase.Instance.DBRead(db =>
                {
                    SystemsDB.GetSystemListBySqDistancesFrom(x, y, z, 20000, 0.5, 50, true, db, (dist, sys) => { list.Add(dist, sys); }); //should span 2 grids 810/811
                    System.Diagnostics.Debug.Assert(list != null && list.Count >= 20);
                });

                //foreach (var k in list) AddText(Math.Sqrt(k.Key).ToString("N1") + " Star " + k.Value.ToStringVerbose());
            }

            { // 142ms with xz and no sector lookup
                BaseUtils.AppTicks.TickCountLap();
                ISystem s;
                s = SystemCache.GetSystemNearestTo(new Point3D(100, 0, 0), new Point3D(1, 0, 0), 110, 20, SystemCache.SystemsNearestMetric.IterativeWaypointDevHalf, 1);
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Alpha Centauri"));
                WriteLog("Find Nearest Star: " + BaseUtils.AppTicks.TickCountLap());
            }

            {
                SystemCache.AddToAutoCompleteList(new List<string>() { "galone", "galtwo", "sol2" });
                SortedSet<string> sys = new SortedSet<string>();
                SystemCache.ReturnSystemAutoCompleteList("Sol", null, sys);
                System.Diagnostics.Debug.Assert(sys != null && sys.Contains("Solati") && sys.Count >= 4);
            }


            {
                var v = SystemsDB.GetStarPositions(5, (x, y, z) => { return new Vector3((float)x / 128.0f, (float)y / 128.0f, (float)z / 128.0f); });
                System.Diagnostics.Debug.Assert(v.Count > 450000);
                //       var v2 = SystemClassDB.GetStarPositions(100, (x, y, z) => { return new Vector3((float)x / 128.0f, (float)y / 128.0f, (float)z / 128.0f); });
            }

            WriteLog($"EDSM Test Complete");
        }


        #endregion
        
        #region Spansh

        private void buttonloadSpanshOld_Click(object sender, EventArgs e)
        {
            SystemsDatabase.Instance.MakeSystemTableFromFile(spansh6mfile, null, 150000, () => false, (s) => System.Diagnostics.Debug.WriteLine(s), method: 0);
            Thread.Sleep(5000);
            WriteLog($"Spansh DB Made OLD {SystemsDB.GetTotalSystems()}");
        }

        private void buttonMakeSpanshL1_Click(object sender, EventArgs e)
        {
            SystemsDatabase.Instance.MakeSystemTableFromFile(spansh6mfile, null, 150000, () => false, (s) => System.Diagnostics.Debug.WriteLine(s), method: 1);
            Thread.Sleep(1000);
            WriteLog($"Spansh DB Made l1 {SystemsDB.GetTotalSystems()}");
        }

        private void buttonMakeSpanshL2_Click(object sender, EventArgs e)
        {
            SystemsDatabase.Instance.MakeSystemTableFromFile(spansh6mfile, null, 200000, () => false, (s) => System.Diagnostics.Debug.WriteLine(s), method: 2);
            Thread.Sleep(1000);
            WriteLog($"Spansh DB Made L2 {SystemsDB.GetTotalSystems()}");
        }

        private void buttonMakeSpanshL3_Click(object sender, EventArgs e)
        {
            SystemsDatabase.Instance.MakeSystemTableFromFile(spansh6mfile, null, 200000, () => false, (s) => System.Diagnostics.Debug.WriteLine(s), method: 3);
            Thread.Sleep(1000);
            WriteLog($"Spansh DB Made L3 {SystemsDB.GetTotalSystems()}");

        }

        private void buttonCheckMadeSpanshStars_Click(object sender, EventArgs e)
        {
            System.Threading.Tasks.Task.Run(() => { CheckDB(spansh6mfile, true, (k) => BeginInvoke(k)); });

        }

        const int spanshreadblocksize = 100000;

        private void buttonReadSpanshL1_Click(object sender, EventArgs e)
        {

            SystemsDB.Loader1 loader = new SystemsDB.Loader1("", spanshreadblocksize, null, false);
            loader.ParseJSONFile(spanshsmallfile, () => false, (s) => System.Diagnostics.Debug.WriteLine(s));
            loader.Finish();

            Thread.Sleep(5000);
            WriteLog($"Spansh DB Read New {SystemsDB.GetTotalSystems()}");
        }

        private void buttonReadSpanshL2_Click(object sender, EventArgs e)
        {
        //  spanshsmallfile = @"c:\code\test.json";
            SystemsDB.Loader2 loader2 = new SystemsDB.Loader2("", spanshreadblocksize, null, false);
            loader2.ParseJSONFile(spanshsmallfile, () => false, (s) => System.Diagnostics.Debug.WriteLine(s));
            loader2.Finish();
            Thread.Sleep(5000);
            WriteLog($"Spansh DB Read New {SystemsDB.GetTotalSystems()}");
        }

        private void buttonReadSpanshOld_Click(object sender, EventArgs e)
        {
            DateTime dts = DateTime.MinValue;
            SystemsDB.ParseJSONFile(spanshsmallfile, null, spanshreadblocksize, ref dts, () => false, (s) => System.Diagnostics.Debug.WriteLine(s), "");

            Thread.Sleep(1000);
            WriteLog($"Spansh DB Read New {SystemsDB.GetTotalSystems()}");
        }

        private void Check(string sys, double x, double y, double z, long sysa, EDStar ty)
        {
            var s = SystemsDB.FindStars(sys); // "id64":22954989341528
            System.Diagnostics.Debug.Assert(s.Count == 1 && s[0].Name == sys && s[0].X == x && s[0].Y == y && s[0].Z == z && s[0].SystemAddress == sysa && s[0].MainStarType == ty);
        }

        private void buttonTestSpansh_Click(object sender, EventArgs e)
        {
            {
                // this tests DBGetStars both types of constructor at the bottom of the file

                Check("Screakou AJ-O c20-1515", -6367.375, 219.34375, 21591.625, 416513211442098, EDStar.K);
                Check("Screakou AJ-O c20-1515", -6367.375, 219.34375, 21591.625, 416513211442098, EDStar.K);
                Check("Sol", 0, 0, 0, 10477373803, EDStar.G);
                Check("HIP 82393", -17.40625, 132.6875, 463.53125, 10477390235, EDStar.G);
            }

            {
                var sl = SystemsDB.FindStars("i Carinae");              // these are case insensitive
                System.Diagnostics.Debug.Assert(sl.Count == 2);
                sl = SystemsDB.FindStars("I Carinae");
                System.Diagnostics.Debug.Assert(sl.Count == 2);
                sl = SystemsDB.FindStarsWildcard("I Carinae");
                System.Diagnostics.Debug.Assert(sl.Count == 2);
            }

            {
                var sl = SystemsDB.FindStarsWildcard("i %");
                var slnames = sl.Select(x => x.Name).ToList();
                System.Diagnostics.Debug.Assert(slnames.Contains("i Bootis"));
                System.Diagnostics.Debug.Assert(slnames.Contains("i Carinae"));
                System.Diagnostics.Debug.Assert(slnames.Contains("I Carinae"));
                System.Diagnostics.Debug.Assert(slnames.Contains("I Puppis"));
            }
            WriteLog($"Spansh Test Complete");
        }

        #endregion

    }
}
