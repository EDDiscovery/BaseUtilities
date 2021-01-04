/*
 * Copyright ┬® 2019 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 *
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using BaseUtils;
using BaseUtils.JSON;
using BaseUtils.WebServer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestWebServer
{
    public class WebServer
    {
        public Action<string> LogIt;

        public int Port { get { return port; } set { port = value; } }
        private int port = 0;

        Server httpws;
        HTTPDispatcher httpdispatcher;

        HTTPFileNode mainwebsitefiles;
        HTTPZipNode mainwebsitezipfiles;

        EDDIconNodes iconnodes;
        JSONDispatcher jsondispatch;
        EDDInterface eddif;

        public WebServer()
        {
        }

        public bool Start()
        {
            httpdispatcher = new HTTPDispatcher();
            httpdispatcher.RootNodeTranslation = "/index.html";

            // Serve ICONS from path - order is important 
            iconnodes = new EDDIconNodes();
            httpdispatcher.AddPartialPathNode("/journalicons/", iconnodes);     // journal icons come from this dynamic source
            httpdispatcher.AddPartialPathNode("/statusicons/", iconnodes);     // status icons come from this dynamic source

            // Serve files from path
            mainwebsitefiles = new HTTPFileNode(@"c:\code\html\ASPWEB2\ASPWEB2");
            mainwebsitezipfiles = new HTTPZipNode(@"c:\code\html\ASPWEB2\ASPWEB2\Website.zip");

            // pick one - the zipper or the file server
            httpdispatcher.AddPartialPathNode("/", mainwebsitefiles);
            //httpdispatcher.AddPartialPathNode("/", mainwebsitezipfiles);

            // HTTP server
            //httpws = new Server("http://localhost:8080/");
            httpws = new Server("http://127.0.0.1:8080/");
            httpws.ServerLog = (s) => { LogIt?.Invoke(s); };

            // add to the server a HTTP responser
            httpws.AddHTTPResponder((lr, lrdata) => { return httpdispatcher.Response(lr); }, httpdispatcher);

            // JSON dispatcher..
            jsondispatch = new JSONDispatcher();
            eddif = new EDDInterface();
            jsondispatch.Add("journal", eddif.jr);      // event journal
            jsondispatch.Add("status", eddif.status);   // event status
            jsondispatch.Add("indicator", eddif.indicator);   // indicator
            eddif.presskey.ireq = eddif.indicator;
            eddif.presskey.httpws = httpws;
            jsondispatch.Add("presskey", eddif.presskey);   // and a key press

            // add for protocol EDDJSON the responder.

            httpws.AddWebSocketsResponder("EDDJSON",
                    (ctx, ws, wsrr, buf, lrdata) => { jsondispatch.Response(ctx, ws, wsrr, buf, lrdata); },
                    jsondispatch);

            httpws.Run();

            return true;
        }

        public bool Stop()      // note it does not wait for threadpools
        {
            if ( httpws != null )
            {
                httpws.Stop();
                httpws = null;
            }

            return true;
        }

        public void PushRecord()
        {
            JToken data = eddif.jr.PushRecord();
            httpws.SendWebSockets(data.ToString(), true);
        }

        public void SupercruiseClick()
        {
            eddif.indicator.Supercruise = !eddif.indicator.Supercruise;
            JToken data = eddif.indicator.PushRecord();
            httpws.SendWebSockets(data.ToString(), true);

        }

        public void ShieldChange()
        {
            eddif.indicator.ShieldsUp = !eddif.indicator.ShieldsUp;
            JToken data = eddif.indicator.PushRecord();
            httpws.SendWebSockets(data.ToString(), true);
        }

        public void NightVision()
        {
            eddif.indicator.NightVision = !eddif.indicator.NightVision;
            JToken data = eddif.indicator.PushRecord();
            httpws.SendWebSockets(data.ToString(), true);
        }

        class EDDIconNodes : IHTTPNode
        {
            public byte[] Response(string partialpath, HttpListenerRequest request)
            {
                System.Diagnostics.Debug.WriteLine("Serve icon " + partialpath);

                if (partialpath.Contains(".png"))
                {
                    Object obj = BaseUtils.ResourceHelpers.GetResource(Assembly.GetExecutingAssembly(), "TestWebServer.Properties.Resources", partialpath.Replace(".png", ""));
                    Bitmap bmp = obj as Bitmap;

                    if (bmp == null)
                        bmp = Properties.Resources.AfmuRepair;
                    return bmp.ConvertTo(System.Drawing.Imaging.ImageFormat.Png);   // this converts to png and returns the raw PNG bytes..
                }

                return null;
            }
        }

        class EDDInterface
        {
            public JournalRequest jr = new JournalRequest();
            public StatusRequest status = new StatusRequest();
            public IndicatorRequest indicator = new IndicatorRequest();
            public PressKeyRequest presskey = new PressKeyRequest();

            public class JournalRequest : IJSONNode
            {
                int maxjindex = 32000;

                public JToken Response(string key, JToken message, HttpListenerRequest request)
                {
                    System.Diagnostics.Debug.WriteLine("Journal Request " + key + " Fields " + message.ToString());
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
                        jent.Add("AfmuRepairs");        // journal type, used to find icon in /journalicons
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
                public JToken PushRecord()
                {
                    return NewSRec("statuspush", 1);
                }

                public JToken Response(string key, JToken message, HttpListenerRequest request)
                {
                    System.Diagnostics.Debug.WriteLine("Status Request " + key + " Fields " + message.ToString());
                    int entry = message["entry"].Int(0);

                    return NewSRec("status", entry);
                }

                public JToken NewSRec(string type, int entry)       // entry = -1 means latest
                {
                    JObject response = new JObject();
                    response["responsetype"] = type;
                    response["entry"] = entry;

                    JObject systemdata = new JObject();
                    systemdata["System"] = "Sol";
                    systemdata["PosX"] = 202.2;
                    systemdata["PosY"] = -292;
                    systemdata["PosZ"] = 2929.2;
                    systemdata["EDSMID"] = 27;
                    response["SystemData"] = systemdata;

                    JObject eddb = new JObject();
                    eddb["EDDBID"] = 4238;
                    eddb["State"] = "War";
                    eddb["Allegiance"] = "Wara";
                    eddb["Gov"] = "Warg";
                    eddb["Economy"] = "Ware";
                    response["EDDB"] = eddb;

                    JObject ship = new JObject();
                    ship["Ship"] = "asp";
                    ship["Fuel"] = 10.2;
                    ship["FuelReservoir"] = 0.23;
                    ship["TankSize"] = 32;
                    ship["Cargo"] = 20;
                    ship["Data"] = 30;
                    ship["Materials"] = 60;
                    ship["Range"] = 20.3;
                    response["Ship"] = ship;

                    JObject travel = new JObject();
                    travel["Dist"] = 60;
                    travel["Time"] = "12:02:22";
                    travel["Jumps"] = 202;
                    response["Travel"] = travel;

                    response["Bodyname"] = "Body " + entry;
                    response["HomeDist"] = 2002.2;
                    response["SolDist"] = 2929.2;
                    response["GameMode"] = "Open";
                    response["Credits"] = 929292;

                    return response;
                }
            }

            public class IndicatorRequest : IJSONNode
            {
                public bool Supercruise = true;
                public bool ShieldsUp = true;
                public bool NightVision = true;

                public JToken PushRecord()
                {
                    return NewIRec("indicatorpush");
                }

                public JToken Response(string key, JToken message, HttpListenerRequest request)
                {
                    System.Diagnostics.Debug.WriteLine("indicator Request " + key + " Fields " + message.ToString());
                    return NewIRec("indicator");
                }

                public JToken NewIRec(string type)       // entry = -1 means latest
                {
                    JObject response = new JObject();
                    response["responsetype"] = type;

                    // all

                    response["GUIFocus"] = 10;

                    JArray pips = new JArray();
                    pips.Add(4);
                    pips.Add(6);
                    pips.Add(4);
                    response["Pips"] = pips;

                    response["Lights"] = false;       // MS
                    response["Firegroup"] = 0;

                    response["HasLatLong"] = false;

                    JArray pos = new JArray();
                    pos.Add(10.2);
                    pos.Add(-20.2);
                    pos.Add(20292);
                    pos.Add(23.32); // heading
                    response["Position"] = pos;

                    // main ship

                    response["Docked"] = false;       // S
                    response["Landed"] = false;  // S
                    response["LandingGear"] = false;   // S
                    response["ShieldsUp"] = ShieldsUp;         //S
                    response["Supercruise"] = Supercruise;   //S
                    response["FlightAssist"] = false;     //S
                    response["HardpointsDeployed"] = false; //S
                    response["InWing"] = false;   // S
                    response["CargoScoopDeployed"] = false;   // S
                    response["SilentRunning"] = false;    // S
                    response["ScoopingFuel"] = false;     // S

                    // srv

                    response["SrvHandbrake"] = false;
                    response["SrvTurret"] = false;
                    response["SrvUnderShip"] = false;
                    response["SrvDriveAssist"] = false;

                    // main ship
                    response["FsdMassLocked"] = false;
                    response["FsdCharging"] = false;
                    response["FsdCooldown"] = false;

                    // both

                    response["LowFuel"] = false;

                    // main ship
                    response["OverHeating"] = false;
                    response["IsInDanger"] = false;
                    response["BeingInterdicted"] = false;
                    response["HUDInAnalysisMode"] = false;
                    response["NightVision"] = NightVision;

                    // all

                    response["LegalState"] = "Clean";
                    response["PlanetRadius"] = 29289292.2;
                    response["ShipType"] = "MainShip";

                    return response;
                }
            }

            public class PressKeyRequest : IJSONNode
            {
                public IndicatorRequest ireq;
                public Server httpws;

                public JToken Response(string key, JToken message, HttpListenerRequest request)
                {
                    System.Diagnostics.Debug.WriteLine("indicator Request " + key + " Fields " + message.ToString());
                    JObject response = new JObject();

                    string keyname = (string)message["key"];

                    if (keyname == "NightVisionToggle")
                    {
                        ireq.NightVision = !ireq.NightVision;
                        JToken data = ireq.PushRecord();
                        httpws.SendWebSockets(data.ToString(), true);
                        ireq.PushRecord();
                    }

                    response["responsetype"] = "presskey";
                    response["status"] = "100";
                    return response;
                }
            }
        }
    }
}
