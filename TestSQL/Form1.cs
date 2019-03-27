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
            SQLiteConnectionSystem.UpgradeFrom102TypeDB((s)=>System.Diagnostics.Debug.WriteLine(s));

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

            bool reloadeddb = false;

            if ( reloadeddb )
            {
                string infile = @"c:\code\edsm\eddbsystems.json";
                SystemsDB.ParseEDDBJSONFile(infile, () => false);
            }

            {
                ISystem s = SystemsDB.FindStar("HIP 112535");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("HIP 112535"));
                 s = SystemsDB.FindStar("Tucanae Sector CQ-Y d79");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Tucanae Sector CQ-Y d79"));
                s = SystemsDB.FindStar("HIP 73368");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("HIP 73368"));
                s = SystemsDB.FindStar("Byua Eurk GL-Y d107");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Byua Eurk GL-Y d107") && s.X == -3555.5625 && s.Y == 119.25 && s.Z == 5478.59375);
                s = SystemsDB.FindStar("BD+18 711");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("BD+18 711") && s.Xi == 1700 && s.Yi == -68224 && s.Zi == -225284);
                s = SystemsDB.FindStar("Chamaeleon Sector FG-W b2-3");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Chamaeleon Sector FG-W b2-3") && s.Xi == 71440 && s.Yi == -12288 && s.Zi == 35092);
            }

            {
                List<ISystem> slist = SystemsDB.FindStarWildcard("Tucanae Sector CQ-Y");
                System.Diagnostics.Debug.Assert(slist != null);

                slist = SystemsDB.FindStarWildcard("Synuefai MC-H");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 3);
                slist = SystemsDB.FindStarWildcard("Synuefai MC-H c");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 3);
                slist = SystemsDB.FindStarWildcard("Synuefai MC-H c12");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count == 0);
                slist = SystemsDB.FindStarWildcard("Synuefai MC-H c12-");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 3);

                slist = SystemsDB.FindStarWildcard("HIP");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count > 48);

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
                double x = 0, y = 0, z = 0;

                SystemsDB.GetSystemListBySqDistancesFrom(list, x,y,z, 20000, 0.5, 200, true);
                foreach( var k in list )
                {
                    System.Diagnostics.Debug.WriteLine(Math.Sqrt(k.Key).ToString("N1") + " Star " + k.Value.ToString());
                }

                ISystem s = SystemsDB.GetSystemByPosition(-100.7, 166.4, -36.8);
                System.Diagnostics.Debug.Assert(s!=null && s.Name == "Col 285 Sector IZ-B b15-2");
            }


            {
                uint[] colours = null;
                Vector3[] vertices = null;
                int v = SystemsDB.GetSystemVector(810, ref vertices, ref colours, 100, (x, y, z) => { return new Vector3((float)x / 128.0f, (float)y / 128.0f, (float)z / 128.0f); });

            }

            {
                var v = SystemsDB.GetStarPositions(50, (x, y, z) => { return new Vector3((float)x / 128.0f, (float)y / 128.0f, (float)z / 128.0f); });
         //       var v2 = SystemClassDB.GetStarPositions(100, (x, y, z) => { return new Vector3((float)x / 128.0f, (float)y / 128.0f, (float)z / 128.0f); });
            }
        }
    }
}
