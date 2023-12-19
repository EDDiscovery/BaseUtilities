#define APT

using BaseUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestSQL2
{
    public partial class MainForm : Form
    {
        string DBFile = @"c:\code\test.sqlite";
#if APT
        SQLiteDBAPT database;
#else
        SQLiteDBWALProcessor database;
#endif

        public MainForm()
        {
            InitializeComponent();
            SetButtons(false);
        }

        private void Write(string s)
        {
            if (!Application.MessageLoop)
                this.BeginInvoke((MethodInvoker)delegate { Write(s); });
            else
            {
                richTextBox.AppendText(s);
                richTextBox.ScrollToCaret();
            }
        }
        private void WriteLine(string s)
        {
            Write(s + Environment.NewLine);
        }

        void SetButtons(bool open)
        {
            buttonOpen.Enabled = buttonCreate.Enabled = !open;
            buttonFill1.Enabled = buttonFill2.Enabled = buttonRepeatFill.Enabled = buttonRead.Enabled = 
                buttonReadRepeat.Enabled = 
            buttonClose.Enabled = buttonFill1.Enabled = open;
        }

        private void buttonCreate_Click(object sender, EventArgs e)
        {
            File.Delete(DBFile);

            buttonOpen_Click(null, null);

            database.DBWrite((cn) =>
            {
                cn.ExecuteNonQueries(new string[]                  // always set up
                           {
                                "CREATE TABLE t1 (id INTEGER PRIMARY KEY NOT NULL, name TEXT)"
                           });

            });

            WriteLine("Created db");
            SetButtons(true);
        }


        private void buttonOpen_Click(object sender, EventArgs e)
        {
#if APT
            database = new SQLiteDBAPT(DBFile);
            database.MaxThreads = database.MinThreads = 4;     database.MultiThreaded = true;
#else
            database = new SQLiteDBWALProcessor(DBFile);
#endif
            WriteLine("Open db");
            SetButtons(true);
        }
        private void buttonClose_Click(object sender, EventArgs e)
        {
            database.Dispose();
            WriteLine("Closed db");
            SetButtons(false);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            database?.Dispose();
            base.OnClosing(e);
            
        }

        private void buttonFill1_Click(object sender, EventArgs e)
        {
            database.DBWrite(cn => {
                StringBuilder data = new StringBuilder();
                for (int i = 0; i < 100; i++)
                {
                    if (data.Length > 0)
                        data.Append(",");
                    data.Append($"({i},'Fred ''{i}''')");
                }

                string cmdt = "INSERT INTO t1 (id,name) VALUES " + data.ToString();

                using (var cmd = cn.CreateCommand(cmdt))
                {
                    cmd.ExecuteNonQuery();
                }

            });
            
            WriteLine("Filled");
        }

        private void FillNoIndex(int size)
        {
            database.DBWrite(cn => {
                StringBuilder data = new StringBuilder();
                for (int i = 0; i < size; i++)
                {
                    if (data.Length > 0)
                        data.Append(",");
                    data.Append($"('Fred ''{i}''')");
                }

                string cmdt = "INSERT INTO t1 (name) VALUES " + data.ToString();

                using (var cmd = cn.CreateCommand(cmdt))
                {
                    cmd.ExecuteNonQuery();
                }

            });
        }

        private void buttonFill2_Click(object sender, EventArgs e)
        {
            FillNoIndex(100);
            WriteLine("Filled autoid");
        }

        int repeater = 0;
        private void buttonRepeatFill_Click(object sender, EventArgs e)
        {
            Task.Run(() => {
                int v = ++repeater;
                for (int i = 0; i < 100; i++)
                {
                    WriteLine($"RepeatFill {v}/{i}");
                    FillNoIndex(1000);
                    Thread.Sleep(500);
                }
                repeater--;
            });

        }

        private void Read(int size, int c)
        {
            database.DBRead(cn => 
            {
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                System.Diagnostics.Debug.WriteLine($"{Environment.TickCount} Read {size} {c}");

                using (var cmd = cn.CreateSelect("t1", "id,name", limit: size))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        int read = 0;
                        while (reader.Read())
                        {
                            var id = (long)reader[0];
                            var str = (string)reader[1];
                            read++;
                        }

                        if (size != read)
                            WriteLine("Read Size wrong!");
                    }
                }

                var time = sw.ElapsedMilliseconds;
                System.Diagnostics.Debug.WriteLine($"{AppTicks.TickCountFromStart("Read")} Read complete {size} {c} delta {time}");
                WriteLine($"{AppTicks.TickCountFromStart("Read")} Read {c} time {time}");
            });
        }

        private void buttonRead_Click(object sender, EventArgs e)
        {
            AppTicks.Start("Read");
            Read(800000,0);
            WriteLine($"Read complete");
        }

        private void buttonReadRepeat_Click(object sender, EventArgs e)
        {
            AppTicks.Start("Read");
            for (int i = 0; i < 4; i++)
            {
                var x = i;
                Task.Run(() =>
                {
                    Read(800000,x);
                });
            }
            WriteLine($"{AppTicks.TickCountFromStart("Read")} Sent reads");

        }
    }
}
