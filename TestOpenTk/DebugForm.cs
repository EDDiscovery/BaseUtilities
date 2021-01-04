using BaseUtils.JSON;
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

namespace TestOpenTk
{
    public partial class DebugForm : Form
    {
        public DebugForm()
        {
            InitializeComponent();
        }

        struct FileLines
        {
            public string[] filelines;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] files = Directory.EnumerateFiles(@"C:\Users\RK\Saved Games\Frontier Developments\Elite Dangerous", "*.log").ToArray();

            List<FileLines> filelines = new List<FileLines>();
            System.Diagnostics.Trace.WriteLine("Read files");

            foreach (var f in files)
            {
                // System.Diagnostics.Trace.WriteLine("Check " + f);
                string[] lines = File.ReadAllLines(f);
                filelines.Add(new FileLines { filelines = lines });
            }

            System.Diagnostics.Trace.WriteLine("Go JSON");
            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            st.Start();

            foreach (var fl in filelines)
            {
                foreach (var l in fl.filelines)
                {
                    JObject t = JObject.Parse(l, out string error, true);
                    System.Diagnostics.Trace.Assert(t["timestamp"] != null);
                    JObject t2 = JObject.Parse(l, out string error2, true);
                    System.Diagnostics.Trace.Assert(t2["timestamp"] != null);
                    JObject t3 = JObject.Parse(l, out string error3, true);
                    System.Diagnostics.Trace.Assert(t3["timestamp"] != null);
                    JObject t4 = JObject.Parse(l, out string error4, true);
                    System.Diagnostics.Trace.Assert(t4["timestamp"] != null);
                    JObject t5 = JObject.Parse(l, out string error5, true);
                    System.Diagnostics.Trace.Assert(t5["timestamp"] != null);
                    JObject t6 = JObject.Parse(l, out string error6, true);
                    System.Diagnostics.Trace.Assert(t6["timestamp"] != null);
                }

            }

            long time = st.ElapsedMilliseconds;
            System.Diagnostics.Trace.WriteLine("Read journals took " + time);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string[] files = Directory.EnumerateFiles(@"C:\Users\RK\Saved Games\Frontier Developments\Elite Dangerous", "*.log").ToArray();

            List<FileLines> filelines = new List<FileLines>();
            System.Diagnostics.Trace.WriteLine("Read files");

            foreach (var f in files)
            {
                // System.Diagnostics.Trace.WriteLine("Check " + f);
                string[] lines = File.ReadAllLines(f);
                filelines.Add(new FileLines { filelines = lines });
            }

            System.Diagnostics.Trace.WriteLine("Go newton");
            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            st.Start();

            foreach (var f in filelines)
            {
                // System.Diagnostics.Trace.WriteLine("Check " + f);
                foreach (var l in f.filelines)
                {
                    Newtonsoft.Json.Linq.JToken t = Newtonsoft.Json.Linq.JToken.Parse(l);
                    System.Diagnostics.Trace.Assert(t["timestamp"] != null);
                    Newtonsoft.Json.Linq.JToken t2 = Newtonsoft.Json.Linq.JToken.Parse(l);
                    System.Diagnostics.Trace.Assert(t2["timestamp"] != null);
                    Newtonsoft.Json.Linq.JToken t3 = Newtonsoft.Json.Linq.JToken.Parse(l);
                    System.Diagnostics.Trace.Assert(t3["timestamp"] != null);
                    Newtonsoft.Json.Linq.JToken t4 = Newtonsoft.Json.Linq.JToken.Parse(l);
                    System.Diagnostics.Trace.Assert(t4["timestamp"] != null);
                    Newtonsoft.Json.Linq.JToken t5 = Newtonsoft.Json.Linq.JToken.Parse(l);
                    System.Diagnostics.Trace.Assert(t5["timestamp"] != null);
                    Newtonsoft.Json.Linq.JToken t6 = Newtonsoft.Json.Linq.JToken.Parse(l);
                    System.Diagnostics.Trace.Assert(t6["timestamp"] != null);
                }

            }

            long time = st.ElapsedMilliseconds;
            System.Diagnostics.Trace.WriteLine("Read journals took " + time);

        }
    }
}
