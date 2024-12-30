using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestNetMQ
{
    public partial class Form1 : Form
    {
        NetMQUtils.NetMQJsonServer server;
        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            server = new NetMQUtils.NetMQJsonServer();
            server.Init("tcp://localhost", 6000, "thread");
        }
    }
}
