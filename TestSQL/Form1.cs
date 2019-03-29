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

            bool reloadstars = false;

            if ( reloadstars )
                BaseUtils.FileHelpers.DeleteFileNoError(SQLiteConnectionSystem.dbFile);

            SQLiteConnectionSystem.Initialize();
            SQLiteConnectionSystem.UpgradeFrom102TypeDB(()=> { return false; },(s)=>System.Diagnostics.Debug.WriteLine(s));
            //SQLiteConnectionSystem.UpgradeFrom102TypeDB(null);

            if (reloadstars)
            {
                string infile = @"c:\code\edsm\edsmsystems.1e6.json";
                DateTime maxdate = DateTime.MinValue;
                SystemsDB.ParseEDSMJSONFile(infile, null, ref maxdate, () => false, (s) => System.Diagnostics.Debug.WriteLine(s), presumeempty: true, debugoutputfile: @"c:\code\process.lst");

                //List<ISystem> stars = SystemsDB.FindStars("");
                //using (StreamWriter wr = new StreamWriter(@"c:\code\starlist.lst"))
                //{
                //    foreach (var s in stars)
                //    {
                //        wr.WriteLine(s.Name + " " + s.X + "," + s.Y + "," + s.Z + ", EDSM:" + s.EDSMID + " Grid:" + s.GridID);
                //    }
                //}
            }


            bool reloadeddb = true;

            if ( reloadeddb )
            {
                string infile = @"c:\code\edsm\eddbsystems.json";
                SystemsDB.ParseEDDBJSONFile(infile, () => false);
            }

            { // 100:346 no eddb
                BaseUtils.AppTicks.TickCountLap();
                ISystem s;

                for (int I = 0; I < 100; I++)
                {
                    s = SystemsDB.FindStar("HIP 112535");
                    System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("HIP 112535"));
                }

                System.Diagnostics.Debug.WriteLine("FindStar HIP x for 100: " + BaseUtils.AppTicks.TickCountLap());
            }

            { // 100:46
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
                s = SystemsDB.FindStar("HIP 73368");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("HIP 73368"));
                s = SystemsDB.FindStar("Byua Eurk GL-Y d107");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Byua Eurk GL-Y d107") && s.X == -3555.5625 && s.Y == 119.25 && s.Z == 5478.59375);
                s = SystemsDB.FindStar("BD+18 711");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("BD+18 711") && s.Xi == 1700 && s.Yi == -68224 && s.Zi == -225284);
                s = SystemsDB.FindStar("Chamaeleon Sector FG-W b2-3");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Chamaeleon Sector FG-W b2-3") && s.Xi == 71440 && s.Yi == -12288 && s.Zi == 35092);
            }

            { // 10:10
                List<ISystem> slist;

                BaseUtils.AppTicks.TickCountLap();

                for (int I = 0; I < 10; I++)
                {
                    slist = SystemsDB.FindStarWildcard("Tucanae Sector CQ-Y");
                    System.Diagnostics.Debug.Assert(slist != null && slist.Count > 50);
                }

                System.Diagnostics.Debug.WriteLine("Find Wildcard Standard trunced: " + BaseUtils.AppTicks.TickCountLap());
            }

            { // 10:307
                BaseUtils.AppTicks.TickCountLap();
                List<ISystem> slist;
                for (int I = 0; I < 10; I++)
                {
                    slist = SystemsDB.FindStarWildcard("HIP 6");
                    System.Diagnostics.Debug.Assert(slist != null && slist.Count > 48);
                }

                System.Diagnostics.Debug.WriteLine("Find HIP 6: " + BaseUtils.AppTicks.TickCountLap());
            }

            { // 10:1997, 2379 with EDDB lookup, 2557 ascii parse, 2724 minor parse, 4317 primary, 4431 with EDDB JO parse.
                // 3554 with string format parse.
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

            { // 160
                BaseUtils.SortedListDoubleDuplicate<ISystem> list = new BaseUtils.SortedListDoubleDuplicate<ISystem>();

                BaseUtils.AppTicks.TickCountLap();
                double x = 0, y = 0, z = 0;

                SystemsDB.GetSystemListBySqDistancesFrom(list, x, y, z, 20000, 0.5, 20, true);
                System.Diagnostics.Debug.WriteLine("Stars Near Sol: " + BaseUtils.AppTicks.TickCountLap());
                System.Diagnostics.Debug.Assert(list != null && list.Count >= 20);

                //foreach (var k in list)   System.Diagnostics.Debug.WriteLine(Math.Sqrt(k.Key).ToString("N1") + " Star " + k.Value.ToString());
            }

            { // 251
                BaseUtils.SortedListDoubleDuplicate<ISystem> list = new BaseUtils.SortedListDoubleDuplicate<ISystem>();

                BaseUtils.AppTicks.TickCountLap();
                double x = 490, y = 0, z = 0;

                SystemsDB.GetSystemListBySqDistancesFrom(list, x, y, z, 20000, 0.5, 50, true); //should span 2 grids 810/811
                System.Diagnostics.Debug.WriteLine("Stars Near x490: " + BaseUtils.AppTicks.TickCountLap());
                System.Diagnostics.Debug.Assert(list != null && list.Count >= 20);

                //foreach (var k in list) System.Diagnostics.Debug.WriteLine(Math.Sqrt(k.Key).ToString("N1") + " Star " + k.Value.ToString());
            }

            { // 33ms
                BaseUtils.AppTicks.TickCountLap();
                ISystem s = SystemsDB.GetSystemByPosition(-100.7, 166.4, -36.8);
                System.Diagnostics.Debug.Assert(s!=null && s.Name == "Col 285 Sector IZ-B b15-2");
                System.Diagnostics.Debug.WriteLine("SysPos1 : " + BaseUtils.AppTicks.TickCountLap());
            }


            { //1188
                BaseUtils.AppTicks.TickCountLap();
                uint[] colours = null;
                Vector3[] vertices = null;
                int v = SystemsDB.GetSystemVector(810, ref vertices, ref colours, 100, (x, y, z) => { return new Vector3((float)x / 128.0f, (float)y / 128.0f, (float)z / 128.0f); });
                System.Diagnostics.Debug.WriteLine("810 load : " + BaseUtils.AppTicks.TickCountLap());

            }

            {
                var v = SystemsDB.GetStarPositions(50, (x, y, z) => { return new Vector3((float)x / 128.0f, (float)y / 128.0f, (float)z / 128.0f); });
         //       var v2 = SystemClassDB.GetStarPositions(100, (x, y, z) => { return new Vector3((float)x / 128.0f, (float)y / 128.0f, (float)z / 128.0f); });
            }
        }
    }
}
