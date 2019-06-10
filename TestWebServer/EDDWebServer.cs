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
using Newtonsoft.Json.Linq;

namespace TestWebServer
{
    public partial class EDDWebServer : Form
    {
        Server httpws;
        HTTPDispatcher httpwsd;
        HTTPFileNode fn1;
        JSONDispatcher jsondispatch;
        EDDInterface eddif;

        public EDDWebServer()
        {
            InitializeComponent();

            httpwsd = new HTTPDispatcher();
            httpwsd.RootNodeTranslation = "/index.html";
            fn1 = new HTTPFileNode(@"c:\code\html\ASPWEB2\ASPWEB2");
            httpwsd.AddPartialPathNode("/", fn1);

            httpws = new Server("http://127.0.0.1:8080/");
            httpws.AddHTTPResponder((lr, lrdata) => { return httpwsd.Response(lr); }, httpwsd);

            httpws.ServerLog = (s) => { this.BeginInvoke((MethodInvoker)delegate { Posttolog(s); }); };

            jsondispatch = new JSONDispatcher();
            eddif = new EDDInterface();

            jsondispatch.Add("journal", eddif.jr);
            jsondispatch.Add("status", eddif.status);

            httpws.AddWebServerResponder("EDDJSON",
                    (ctx, ws, wsrr, buf, lrdata) => { jsondispatch.Response(ctx, ws, wsrr, buf, lrdata); },
                    jsondispatch);

            httpws.Run();
        }

        void Posttolog(string s)
        {
            serverLog.Text += s + Environment.NewLine;
            serverLog.Select(serverLog.Text.Length, serverLog.Text.Length);
            serverLog.ScrollToCaret();
        }

        private void EDDWebServer_Shown(object sender, EventArgs e)
        {
            this.Location = new Point(1920 * 2 + 10, 10);
        }

        private void buttonPushRec_Click(object sender, EventArgs e)
        {
            JToken data = eddif.jr.PushRecord();
            httpws.SendWebSocket(data.ToString(Newtonsoft.Json.Formatting.None), true);
        }
    }

    class EDDInterface
    {
        public JournalRequest jr = new JournalRequest();
        public StatusRequest status = new StatusRequest();

        public class JournalRequest : IJSONNode
        {
            int maxjindex = 32000;

            public JToken Response(string key, JToken message, HttpListenerRequest request)
            {
                System.Diagnostics.Debug.WriteLine("Journal Request " + key + " Fields " + message.ToString(Newtonsoft.Json.Formatting.None));
                int startindex = message["start"].Int(0);
                int length = message["length"].Int(0);

                if (startindex < 0)
                    startindex = maxjindex;

                return NewJRec("journalrequest", startindex, length);
            }

            public JToken PushRecord()
            {
                return NewJRec("journalpush", ++maxjindex, 1);
            }

            public JToken NewJRec(string type, int startindex, int length)
            {
                JObject response = new JObject();
                response["responsetype"] = type;

                response["firstrow"] = startindex;

                JArray jarray = new JArray();
                for (int i = startindex; i > Math.Max(0, startindex - length); i--)
                {
                    JArray jent = new JArray();
                    jent.Add("Icons/CodexEntry.png");
                    jent.Add("12-20-20 23:00:23");
                    jent.Add("JEntry " + i);
                    jent.Add("journal data");
                    jent.Add("note");

                    jarray.Add(jent);
                }

                response["rows"] = jarray;

                return response;
            }

        }


        public class StatusRequest : IJSONNode
        {
            public JToken Response(string key, JToken message, HttpListenerRequest request)
            {
                System.Diagnostics.Debug.WriteLine("Status Request " + key + " Fields " + message.ToString(Newtonsoft.Json.Formatting.None));
                int entry = message["entry"].Int(0);

                return NewSRec("status", entry);
            }

            public JToken NewSRec(string type, int entry)
            {
                JObject response = new JObject();
                response["responsetype"] = type;
                response["entry"] = entry;

               // do this, get displayed on screen
              //      then theme the page

                return response;
            }

        }
    }



}

