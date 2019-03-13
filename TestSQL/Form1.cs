﻿using EliteDangerousCore.SystemDB;
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

            BaseUtils.FileHelpers.DeleteFileNoError(SQLiteConnectionSystem.dbFile);
            SQLiteConnectionSystem.Initialize();

            //string infile = @"c:\code\edsm\input.json";
            string infile = @"c:\code\edsm\edsmsystems.10e6.json";
            DateTime maxdate = DateTime.MinValue;
            SystemClassDB.ParseEDSMJSONFile(infile, new bool[] { false, false, false }, ref maxdate, () => false, presumeempty: true, debugoutputfile:@"c:\code\process.lst");

            List<SystemClassDB.Star> stars = SystemClassDB.FindStars();
            using (StreamWriter wr = new StreamWriter(@"c:\code\starlist.lst"))
            {
                foreach (var s in stars)
                {
                    wr.WriteLine(s.Name + " " + s.X + "," + s.Y + "," + s.Z + ", EDSM:" + s.EDSMId + " Grid:" + s.GridId);
                }
            }

            SystemClassDB.DeleteCache();

            {
                SystemClassDB.Star s = SystemClassDB.FindStar("Tucanae Sector CQ-Y d79");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Tucanae Sector CQ-Y d79"));
                s = SystemClassDB.FindStar("HIP 73368");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("HIP 73368"));
                s = SystemClassDB.FindStar("Byua Eurk GL-Y d107");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Byua Eurk GL-Y d107") && s.Xf == -3555.5625 && s.Yf == 119.25 && s.Zf == 5478.59375);
                s = SystemClassDB.FindStar("BD+18 711");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("BD+18 711") && s.X == 1700 && s.Y == -68224 && s.Z == -225284);
                s = SystemClassDB.FindStar("Chamaeleon Sector FG-W b2-3");
                System.Diagnostics.Debug.Assert(s != null && s.Name.Equals("Chamaeleon Sector FG-W b2-3") && s.X == 71440 && s.Y == -12288 && s.Z == 35092);
            }

            {
                List<SystemClassDB.Star> slist = SystemClassDB.FindStarWildcard("Tucanae Sector CQ-Y");
                System.Diagnostics.Debug.Assert(slist != null);

                slist = SystemClassDB.FindStarWildcard("Synuefai MC-H");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 3);
                slist = SystemClassDB.FindStarWildcard("Synuefai MC-H c");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 3);
                slist = SystemClassDB.FindStarWildcard("Synuefai MC-H c12");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count == 0);
                slist = SystemClassDB.FindStarWildcard("Synuefai MC-H c12-");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 3);

                slist = SystemClassDB.FindStarWildcard("HIP");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count > 48);

                slist = SystemClassDB.FindStarWildcard("HIP 6");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count > 5);

                slist = SystemClassDB.FindStarWildcard("Coalsack Sector");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 4);

                slist = SystemClassDB.FindStarWildcard("Coalsack");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 4);

                slist = SystemClassDB.FindStarWildcard("4 S");
                System.Diagnostics.Debug.Assert(slist != null && slist.Count >= 1);


            }

            {
                uint[] colours = null;
                Vector3[] vertices = null;
                int v = SystemClassDB.GetSystemVector(810, ref vertices, ref colours, 100, (x, y, z) => { return new Vector3((float)x / 128.0f, (float)y / 128.0f, (float)z / 128.0f); });

            }

            {
                var v = SystemClassDB.GetStarPositions(50, (x, y, z) => { return new Vector3((float)x / 128.0f, (float)y / 128.0f, (float)z / 128.0f); });
                var v2 = SystemClassDB.GetStarPositions(100, (x, y, z) => { return new Vector3((float)x / 128.0f, (float)y / 128.0f, (float)z / 128.0f); });

            }

            //// new one, in date range, so it will check the db
            //{
            //    string update1 = "[ { \"id\":34202020,\"id64\":2724930226555,\"name\":\"Tucanae Sector CQ-X d79\",\"coords\":{ \"x\":165.375,\"y\":-174.75,\"z\":191.3125},\"date\":\"2015-05-12 15:29:33\"} ]";
            //    EDSMDB.ParseEDSMJSONString(update1, new bool[] { false, false, false }, ref maxdate, () => false, presumeempty: false);

            //    EDSMDB.Star s = EDSMDB.FindStar("Tucanae Sector CQ-X d79");
            //    System.Diagnostics.Debug.WriteLine(s != null);
            //}

            //// update one
            //{
            //    string update1 = "[ { \"id\":26513,\"id64\":2724930226555,\"name\":\"Synuefai II-I b43-0\",\"coords\":{ \"x\":10,\"y\":20,\"z\":30},\"date\":\"2015-05-12 15:29:33\"} ]";
            //    EDSMDB.ParseEDSMJSONString(update1, new bool[] { false, false, false }, ref maxdate, () => false, presumeempty: false);

            //    EDSMDB.Star s = EDSMDB.FindStar("Synuefai II-I b43-0");
            //    System.Diagnostics.Debug.WriteLine(s != null && s.X==10*128 && s.Y==20*128 && s.Z == 30*128);
            //}
        }
    }
}
