using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestSQL2
{
    public partial class Form1 : Form
    {
        SQLiteThread procthread;
        public Form1()
        {
            InitializeComponent();

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (procthread != null)
                procthread.Stop();

        }

        private void Write(string s)
        {
            richTextBox.AppendText(s);
            richTextBox.ScrollToCaret();
        }
        private void WriteLine(string s)
        {
            richTextBox.AppendText(s + Environment.NewLine);
            richTextBox.ScrollToCaret();
        }

        private void buttonSimple_Click(object sender, EventArgs e)
        {
            SQLiteConnectionSystem connection;

            connection = new SQLiteConnectionSystem(@"c:\code\edsm\edsm.sql", false);

            var query1 = connection.CreateSelect("Sectors", "*",limit:20);

            using (DbDataReader reader = query1.ExecuteReader())
            {
                while (reader.Read())
                {
                    WriteLine($"{reader[0]} {reader[1]} {reader[2]}");
                }
            }

            connection.Dispose();

        }


        private void buttonThread_Click(object sender, EventArgs e)
        {
            if (procthread == null)
            {
                procthread = new SQLiteThread();
                WriteLine("Started");
            }
        }

        private List<string> Query(int limit, string name, bool write = false)
        {
            List<string> output = new List<string>();

            if (write)
            {
                procthread.DBWrite((connection) =>            // does not matter the contents of the operation, point is to send a write down
                {
                    var query1 = connection.CreateSelect("Sectors", "*", limit: limit);

                    using (DbDataReader reader = query1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            output.Add($"{reader[0]} {reader[1]} {reader[2]}");
                        }
                    }

                }, 1000, name);

            }
            else
            {
                procthread.DBRead((connection) =>
                {
                    var query1 = connection.CreateSelect("Sectors", "*", limit: limit);

                    using (DbDataReader reader = query1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            output.Add($"{reader[0]} {reader[1]} {reader[2]}");
                        }
                    }

                }, 1000, name);
            }

            return output;
        }

        private void buttonTQuery1_Click(object sender, EventArgs e)
        {
            if (procthread == null)
                return;

            var output = Query(20, "-Q1");
            foreach (var s in output)
                WriteLine(s);
        }

        private void buttonST_Click(object sender, EventArgs e)
        {
            if (procthread == null)
                return;

            procthread.MultiThreaded = false;
            WriteLine("Single thread");
        }

        private void buttonMT_Click(object sender, EventArgs e)
        {
            if (procthread == null)
                return;

            procthread.MultiThreaded = true;
            WriteLine("Multithread");
        }

        private void buttonMQ1_Click(object sender, EventArgs e)
        {
            if (procthread == null)
                return;

            for (int i = 0; i < 20; i++)
            {
                Thread t1 = new Thread(new ParameterizedThreadStart(MQ1)) { Name = "MQ-" + i };
                t1.Start(i);
                //Thread.Sleep(5);
            }
        }

        private void MQ1(Object o)
        {

            int threadno = (int)o;
            System.Diagnostics.Debug.WriteLine($"Query Start {threadno}");
            var res = Query(40, "-Q" + threadno , threadno == 10);
            System.Diagnostics.Debug.Assert(res.Count == 40);


            Thread.Sleep(1000);
            System.Diagnostics.Debug.WriteLine($"Query Stop {threadno}");

        }
    }
}
