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
using BaseUtils.WebServer;
using System.Web.UI;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Resources;
using System.Reflection;
using BaseUtils;

namespace TestWebServer
{
    public partial class EDDWebServer : Form
    {
        WebServer ws;

        public EDDWebServer()
        {
            InitializeComponent();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (ws == null)
            {
                ws = new WebServer();
                ws.LogIt = (s) => { this.BeginInvoke((MethodInvoker)delegate { Posttolog(s); }); };
                Posttolog("Starting");
                ws.Start();
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            if (ws != null)
            {
                ws.Stop();
                ws = null;
            }

        }

        void Posttolog(string s)
        {
            serverLog.Text += s + Environment.NewLine;
            serverLog.Select(serverLog.Text.Length, serverLog.Text.Length);
            serverLog.ScrollToCaret();
        }

        private void EDDWebServer_Shown(object sender, EventArgs e)
        {
            this.Location = new Point(1920 * 0 + 100, 10);
        }

        private void buttonPushRec_Click(object sender, EventArgs e)
        {
            ws?.PushRecord();
        }

        private void changeSupercruise_Click(object sender, EventArgs e)
        {
            ws?.SupercruiseClick();
        }

        private void shieldChange_Click(object sender, EventArgs e)
        {
            ws?.ShieldChange();
        }

        private void nightvision_Click(object sender, EventArgs e)
        {
            ws?.NightVision();
        }

    }
}
