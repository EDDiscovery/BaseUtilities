/*
* Copyright © 2018 EDDiscovery development team
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
using BaseUtils.JSON;
using NFluent;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace EDDiscoveryTests
{
    [TestFixture(TestOf = typeof(JToken))]
    public class JSONTests
    {
        void Dump(string s)
        {
            foreach (var c in s)
            {
                System.Diagnostics.Debug.WriteLine((int)c + ":" + ((int)c > 32 ? c : '?'));
            }
        }
        [Test]
        public void JSONBasic()
        {
            { 
                //string json = "{ \"timest\\\"amp\":\"2020-06-29T09:53:54Z\", \"bigint\":298182772762562557788377626262773 \"ulong\":18446744073709551615 \"event\":\"FSDJump\t\", \"StarSystem\":\"Shinrarta Dezhra\", \"SystemAddress\":3932277478106, \"StarPos\":[55.71875,17.59375,27.15625], \"SystemAllegiance\":\"PilotsFederation\", \"SystemEconomy\":\"$economy_HighTech;\", \"SystemEconomy_Localised\":\"High Tech\", \"SystemSecondEconomy\":\"$economy_Industrial;\", \"SystemSecondEconomy_Localised\":\"Industrial\", \"SystemGovernment\":\"$government_Democracy;\", \"SystemGovernment_Localised\":\"Democracy\", \"SystemSecurity\":\"$SYSTEM_SECURITY_high;\", \"SystemSecurity_Localised\":\"High Security\", \"Population\":85206935, \"Body\":\"Shinrarta Dezhra\", \"BodyID\":1, \"BodyType\":\"Star\", \"JumpDist\":5.600, \"FuelUsed\":0.387997, \"FuelLevel\":31.612003, \"Factions\":[ { \"Name\":\"LTT 4487 Industry\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.288000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000, \"RecoveringStates\":[ { \"State\":\"Drought\", \"Trend\":0 } ] }, { \"Name\":\"Future of Arro Naga\", \"FactionState\":\"Outbreak\", \"Government\":\"Democracy\", \"Influence\":0.139000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000, \"ActiveStates\":[ { \"State\":\"Outbreak\" } ] }, { \"Name\":\"The Dark Wheel\", \"FactionState\":\"CivilUnrest\", \"Government\":\"Democracy\", \"Influence\":0.376000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000, \"PendingStates\":[ { \"State\":\"Expansion\", \"Trend\":0 } ], \"RecoveringStates\":[ { \"State\":\"PublicHoliday\", \"Trend\":0 } ], \"ActiveStates\":[ { \"State\":\"CivilUnrest\" } ] }, { \"Name\":\"Los Chupacabras\", \"FactionState\":\"None\", \"Government\":\"PrisonColony\", \"Influence\":0.197000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000, \"RecoveringStates\":[ { \"State\":\"Outbreak\", \"Trend\":0 } ] } ], \"SystemFaction\":{ \"Name\":\"Pilots' Federation Local Branch\" } }";
                string json = "{ \"timest\\\"am\tp\":\"2020-06-29T09:53:54Z\", \"ulong\":18446744073709551615, \"bigint\":-298182772762562557788377626262773, \"array\":[ 10, 20, 30  ], \"object\":{ \"a\":20, \"b\":30}, \"fred\":20029 }";

                //   string json = "{ \"timestamp\":\"2016-09-27T19:43:21Z\", \"event\":\"Fileheader\", \"part\":1, \"language\":\"English\\\\UK\", \"gameversion\":\"2.2 (Beta 3)\", \"build\":\"r121970/r0 \" }";

                JToken decoded = JToken.Parse(json);
                Check.That(decoded).IsNotNull();
                string outstr = decoded.ToString(true);
                System.Diagnostics.Debug.WriteLine("" + outstr);
                Dump(outstr);

                JToken decoded2 = JToken.Parse(outstr);

                string outstr2 = decoded2.ToString(true);
                System.Diagnostics.Debug.WriteLine("" + outstr2);

                Check.That(outstr).IsEqualTo(outstr2);

                JObject jo = decoded as JObject;
                Check.That(jo).IsNotNull();

                // string j = jo["timest\"am\tp"].Str();
                //  Check.That(j).Equals("2020-06-29T09:53:54Z");

                JArray ja = new JArray(20.2, 30.3, 40.4);
                Check.That(ja).IsNotNull();
                Check.That(ja.Count).Equals(3);

                JArray jb = new JArray("hello", "jim", "sheila");
                Check.That(jb).IsNotNull();
                Check.That(jb.Count).Equals(3);

                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict["fred"] = "one";
                dict["jim"] = "two";
                JObject jod = new JObject(dict);

                Dictionary<string, float> dict2 = new Dictionary<string, float>();
                dict2["fred"] = 20.0f;
                dict2["jim"] = 30f;
                JObject jod2 = new JObject(dict2);
            }
            {
                string json1 = "{ \"timest\\\"am\tp\":\"2020-06-29T09:53:54Z\", \"ulong\"w:18446744073709551615, \"bigint\":-298182772762562557788377626262773, \"array\":[ 10, 20, 30  ], \"object\":{ \"a\":20, \"b\":30}, \"fred\":20029 }";
                JToken jo = JToken.Parse(json1, out string error, JToken.ParseOptions.None);
                Check.That(error).Contains("missing : after");
            }
            {
                string json1 = "{ \"timest\\\"am\tp\":\"2020-06-29T09:53:54Z\", \"ulong\":18446744073709551615, \"bigint\":-298182772762562557788377626262773, \"array\":[ 10, 20, 30  ], \"object\":{ \"a\":20, \"b\":30}, \"fred\":20029 } extra";
                JToken jo = JToken.Parse(json1, out string error, JToken.ParseOptions.CheckEOL);
                Check.That(error).Contains("Extra Chars");
            }

            {
                string json1 = "{ \"timest\\\"am\tp\":\"2020-06-29T09:53:54Z\", \"ulong\":18446744073709551615, \"bigint\":-298182772762562557788377626262773, \"array\":[ 10, 20, 30  ], \"object\":{ \"a\":20, \"b\":30}, \"fred\":20029 } extra";
                try
                {
                    JToken jo = JToken.Parse(json1, out string error, JToken.ParseOptions.CheckEOL | JToken.ParseOptions.ThrowOnError);
                    Check.That(true).IsFalse();
                }
                catch (JToken.JsonException ex)
                {
                    Check.That(ex.Error).Contains("Extra Chars");
                }
            }

            {
                string json2 = @"{ ""data"": {""ver"":2, ""commander"":""Irisa Nyira"", ""fromSoftware"":""EDDiscovery"",  ""fromSoftwareVersion"":""11.7.2.0"", ""p0"": { ""name"": ""Hypo Aeb XF-M c8-0"" },   ""refs"": [ { ""name"": """"Hypua Hypoo MJ-S a72-0"""",  ""dist"": 658.84 } ] }  }";
                JToken jo = JToken.Parse(json2, out string error, JToken.ParseOptions.CheckEOL );
                Check.That(jo == null).IsTrue();
            }

            {
                string quotedstr = "\"quote\"\nHello";
                JObject jo = new JObject();
                jo["str1"] = quotedstr;
                string s = jo.ToString();
                JObject jo1 = JObject.Parse(s);
                Check.That(jo1 != null).IsTrue();
                Check.That(jo1["str1"].Equals(quotedstr));

            }
        }

        [Test]
        public void JSONObject1()
        {
            JObject j1 = new JObject();
            j1["One"] = "one";
            j1["Two"] = "two";
            JArray ja = new JArray();
            ja.AddRange(new List<JToken> { "one", "two", 10.23 });
            j1["Array"] = ja;

            System.Diagnostics.Debug.WriteLine("" + j1.ToString().QuoteString());

            string expectedjson = "{\"One\":\"one\",\"Two\":\"two\",\"Array\":[\"one\",\"two\",10.23]}";

            Check.That(j1.ToString()).IsEqualTo(expectedjson);
        }

        [Test]
        public void JSONObject2()
        {
            JObject jo = new JObject
            {
                ["timestamp"] = true,
                ["event"] = true,
                ["StarSystem"] = true,
                ["SystemAddress"] = true,
            };

            System.Diagnostics.Debug.WriteLine("" + jo.ToString().QuoteString());

            string expectedjson = "{\"timestamp\":true,\"event\":true,\"StarSystem\":true,\"SystemAddress\":true}";

            Check.That(jo.ToString()).IsEqualTo(expectedjson);

            int count = 0;
            foreach (KeyValuePair<string, JToken> p in jo)
            {
                count++;
            }
            Check.That(count).Equals(4);

            JToken basv = jo;

            Check.That(count).Equals(4);

            int count2 = 0;
            foreach (var v1 in basv)
            {
                count2++;
            }

            Check.That(count).Equals(4);
        }

        [Test]
        public void JSONArray()
        {
            JArray ja = new JArray
                    {
                        "one",
                        "two",
                        "three",
                        new JObject()
                        {
                            ["SystemAllegiance"] = true,
                        }
                    };

            System.Diagnostics.Debug.WriteLine("" + ja.ToString().QuoteString());

            string expectedjson = "[\"one\",\"two\",\"three\",{\"SystemAllegiance\":true}]";

            Check.That(ja.ToString()).IsEqualTo(expectedjson);

            //     string s = ja.Find<JString>(x => x is JString && ((JString)x).Value.Equals("one"))?.Value;

            //    Check.That(s).IsNotNull().Equals("one");

            JObject o = ja.Find<JObject>(x => x is JObject);
            Check.That(o).IsNotNull();

            int i1 = ja[0].Int(-1);
            Check.That(i1 == -1).IsTrue();

            int count = 0;
            foreach (var v1 in ja)
            {
                count++;
            }
            Check.That(count).Equals(4);
        }



        [Test]
        public void JSONComplexObject()
        {
            JObject AllowedFieldsLocJump = new JObject()
            {
                ["SystemAllegiance"] = true,
                ["Powers"] = new JObject(),
                ["SystemEconomy"] = true,
                ["SystemSecondEconomy"] = true,
                ["SystemFaction"] = new JObject
                {
                    ["Name"] = true,
                    ["FactionState"] = true,
                },
                ["SystemGovernment"] = true,
                ["SystemSecurity"] = true,
                ["Population"] = true,
                ["PowerplayState"] = true,
                ["Factions"] = new JArray
                        {
                            new JObject
                            {
                                ["Name"] = true,
                                ["Allegiance"] = true,
                                ["Government"] = true,
                                ["FactionState"] = true,
                                ["Happiness"] = true,
                                ["Influence"] = true,
                                ["ActiveStates"] = new JArray
                                {
                                    new JObject
                                    {
                                        ["State"] = true
                                    }
                                },
                                ["PendingStates"] = new JArray
                                {
                                    new JObject
                                    {
                                        ["State"] = true,
                                        ["Trend"] = true
                                    }
                                },
                                ["RecoveringStates"] = new JArray
                                {
                                    new JObject
                                    {
                                        ["State"] = true,
                                        ["Trend"] = true
                                    }
                                },
                            }
                        },
                ["Conflicts"] = new JArray
                        {
                            new JObject
                            {
                                ["WarType"] = true,
                                ["Status"] = true,
                                ["Faction1"] = new JObject
                                {
                                    ["Name"] = true,
                                    ["Stake"] = true,
                                    ["WonDays"] = true
                                },
                                ["Faction2"] = new JObject
                                {
                                    ["Name"] = true,
                                    ["Stake"] = true,
                                    ["WonDays"] = true
                                },
                            }
                        }
            };

            System.Diagnostics.Debug.WriteLine("" + AllowedFieldsLocJump.ToString().QuoteString());

            string expectedjson = "{\"SystemAllegiance\":true,\"Powers\":{},\"SystemEconomy\":true,\"SystemSecondEconomy\":true,\"SystemFaction\":{\"Name\":true,\"FactionState\":true},\"SystemGovernment\":true,\"SystemSecurity\":true,\"Population\":true,\"PowerplayState\":true,\"Factions\":[{\"Name\":true,\"Allegiance\":true,\"Government\":true,\"FactionState\":true,\"Happiness\":true,\"Influence\":true,\"ActiveStates\":[{\"State\":true}],\"PendingStates\":[{\"State\":true,\"Trend\":true}],\"RecoveringStates\":[{\"State\":true,\"Trend\":true}]}],\"Conflicts\":[{\"WarType\":true,\"Status\":true,\"Faction1\":{\"Name\":true,\"Stake\":true,\"WonDays\":true},\"Faction2\":{\"Name\":true,\"Stake\":true,\"WonDays\":true}}]}";

            Check.That(AllowedFieldsLocJump.ToString()).IsEqualTo(expectedjson);

            string jsonout = AllowedFieldsLocJump.ToString(true);       // round trip it
            JToken decode = JToken.Parse(jsonout);
            Check.That(decode).IsNotNull();
            Check.That(decode.ToString(true)).IsEqualTo(jsonout);
        }

        [Test]
        public void JSONRemove()
        {
            JObject obj = new JObject()
            {
                ["Factions"] = new JArray
                        {
                            new JObject
                            {
                                ["Faction"] = "1",
                                ["MyReputation"] = "Good",
                                ["Otherstuff"] = "Good",
                            },
                            new JObject
                            {
                                ["Faction"] = "2",
                                ["MyReputation"] = "Good",
                                ["Otherstuff"] = "Good",
                            },
                        }
            };

            System.Diagnostics.Debug.WriteLine("" + obj.ToString().QuoteString());

            string expectedjson = "{\"Factions\":[{\"Faction\":\"1\",\"MyReputation\":\"Good\",\"Otherstuff\":\"Good\"},{\"Faction\":\"2\",\"MyReputation\":\"Good\",\"Otherstuff\":\"Good\"}]}";

            Check.That(obj.ToString()).IsEqualTo(expectedjson);

            JArray factions = obj["Factions"] as JArray;

            if (factions != null)
            {
                foreach (JObject faction in factions)
                {
                    faction.Remove("MyReputation");
                }
            }

            System.Diagnostics.Debug.WriteLine(obj.ToString().QuoteString());

            string expectedjson2 = "{\"Factions\":[{\"Faction\":\"1\",\"Otherstuff\":\"Good\"},{\"Faction\":\"2\",\"Otherstuff\":\"Good\"}]}";

            Check.That(obj.ToString()).IsEqualTo(expectedjson2);

        }

        string jsongithub = @"
        {
          ""url"": ""https://api.github.com/repos/EDDiscovery/EDDiscovery/releases/25769192"",
          ""assets_url"": ""https://api.github.com/repos/EDDiscovery/EDDiscovery/releases/25769192/assets"",
          ""upload_url"": ""https://uploads.github.com/repos/EDDiscovery/EDDiscovery/releases/25769192/assets{?name,label}"",
          ""html_url"": ""https://github.com/EDDiscovery/EDDiscovery/releases/tag/Release_11.4.0"",
          ""id"": 25769192,
          ""node_id"": ""MDc6UmVsZWFzZTI1NzY5MTky"",
          ""tag_name"": ""Release_11.4.0"",
          ""target_commitish"": ""master"",
          ""name"": ""EDDiscovery Release 11.4.0 Material Trader, Scan improvements, lots of others"",
          ""draft"": false,
          ""author"": {
            ""login"": ""robbyxp1"",
            ""id"": 6573992,
            ""node_id"": ""MDQ6VXNlcjY1NzM5OTI="",
            ""avatar_url"": ""https://avatars1.githubusercontent.com/u/6573992?v=4"",
            ""gravatar_id"": """",
            ""url"": ""https://api.github.com/users/robbyxp1"",
            ""html_url"": ""https://github.com/robbyxp1"",
            ""followers_url"": ""https://api.github.com/users/robbyxp1/followers"",
            ""following_url"": ""https://api.github.com/users/robbyxp1/following{/other_user}"",
            ""gists_url"": ""https://api.github.com/users/robbyxp1/gists{/gist_id}"",
            ""starred_url"": ""https://api.github.com/users/robbyxp1/starred{/owner}{/repo}"",
            ""subscriptions_url"": ""https://api.github.com/users/robbyxp1/subscriptions"",
            ""organizations_url"": ""https://api.github.com/users/robbyxp1/orgs"",
            ""repos_url"": ""https://api.github.com/users/robbyxp1/repos"",
            ""events_url"": ""https://api.github.com/users/robbyxp1/events{/privacy}"",
            ""received_events_url"": ""https://api.github.com/users/robbyxp1/received_events"",
            ""type"": ""User"",
            ""site_admin"": false
          },
          ""prerelease"": true,
          ""created_at"": ""2020-04-24T12:32:30Z"",
          ""published_at"": ""2020-04-24T12:37:33Z"",
          ""assets"": [
            {
              ""url"": ""https://api.github.com/repos/EDDiscovery/EDDiscovery/releases/assets/20114552"",
              ""id"": 20114552,
              ""node_id"": ""MDEyOlJlbGVhc2VBc3NldDIwMTE0NTUy"",
              ""name"": ""EDDiscovery.Portable.zip"",
              ""label"": null,
              ""uploader"": {
                ""login"": ""robbyxp1"",
                ""id"": 6573992,
                ""node_id"": ""MDQ6VXNlcjY1NzM5OTI="",
                ""avatar_url"": ""https://avatars1.githubusercontent.com/u/6573992?v=4"",
                ""gravatar_id"": """",
                ""url"": ""https://api.github.com/users/robbyxp1"",
                ""html_url"": ""https://github.com/robbyxp1"",
                ""followers_url"": ""https://api.github.com/users/robbyxp1/followers"",
                ""following_url"": ""https://api.github.com/users/robbyxp1/following{/other_user}"",
                ""gists_url"": ""https://api.github.com/users/robbyxp1/gists{/gist_id}"",
                ""starred_url"": ""https://api.github.com/users/robbyxp1/starred{/owner}{/repo}"",
                ""subscriptions_url"": ""https://api.github.com/users/robbyxp1/subscriptions"",
                ""organizations_url"": ""https://api.github.com/users/robbyxp1/orgs"",
                ""repos_url"": ""https://api.github.com/users/robbyxp1/repos"",
                ""events_url"": ""https://api.github.com/users/robbyxp1/events{/privacy}"",
                ""received_events_url"": ""https://api.github.com/users/robbyxp1/received_events"",
                ""type"": ""User"",
                ""site_admin"": false
              },
              ""content_type"": ""application/x-zip-compressed"",
              ""state"": ""uploaded"",
              ""size"": 11140542,
              ""download_count"": 24,
              ""created_at"": ""2020-04-24T12:35:04Z"",
              ""updated_at"": ""2020-04-24T12:35:13Z"",
              ""browser_download_url"": ""https://github.com/EDDiscovery/EDDiscovery/releases/download/Release_11.4.0/EDDiscovery.Portable.zip""
            },
            {
              ""url"": ""https://api.github.com/repos/EDDiscovery/EDDiscovery/releases/assets/20114548"",
              ""id"": 20114548,
              ""node_id"": ""MDEyOlJlbGVhc2VBc3NldDIwMTE0NTQ4"",
              ""name"": ""EDDiscovery_11.4.0.exe"",
              ""label"": null,
              ""uploader"": {
                ""login"": ""robbyxp1"",
                ""id"": 6573992,
                ""node_id"": ""MDQ6VXNlcjY1NzM5OTI="",
                ""avatar_url"": ""https://avatars1.githubusercontent.com/u/6573992?v=4"",
                ""gravatar_id"": """",
                ""url"": ""https://api.github.com/users/robbyxp1"",
                ""html_url"": ""https://github.com/robbyxp1"",
                ""followers_url"": ""https://api.github.com/users/robbyxp1/followers"",
                ""following_url"": ""https://api.github.com/users/robbyxp1/following{/other_user}"",
                ""gists_url"": ""https://api.github.com/users/robbyxp1/gists{/gist_id}"",
                ""starred_url"": ""https://api.github.com/users/robbyxp1/starred{/owner}{/repo}"",
                ""subscriptions_url"": ""https://api.github.com/users/robbyxp1/subscriptions"",
                ""organizations_url"": ""https://api.github.com/users/robbyxp1/orgs"",
                ""repos_url"": ""https://api.github.com/users/robbyxp1/repos"",
                ""events_url"": ""https://api.github.com/users/robbyxp1/events{/privacy}"",
                ""received_events_url"": ""https://api.github.com/users/robbyxp1/received_events"",
                ""type"": ""User"",
                ""site_admin"": false
              },
              ""content_type"": ""application/x-msdownload"",
              ""state"": ""uploaded"",
              ""size"": 15672578,
              ""download_count"": 55,
              ""created_at"": ""2020-04-24T12:34:52Z"",
              ""updated_at"": ""2020-04-24T12:35:02Z"",
              ""browser_download_url"": ""https://github.com/EDDiscovery/EDDiscovery/releases/download/Release_11.4.0/EDDiscovery_11.4.0.exe""
            }
          ],
          ""tarball_url"": ""https://api.github.com/repos/EDDiscovery/EDDiscovery/tarball/Release_11.4.0"",
          ""zipball_url"": ""https://api.github.com/repos/EDDiscovery/EDDiscovery/zipball/Release_11.4.0"",
          ""body"": ""This is a major overhaul of the Scan panel, addition of Material Trader panels, and general overhaul of lots of the program.\r\n\r\n*** \r\nMajor features\r\n\r\n* Scan panel gets more visuals and a new menu system to select output. Many more options added including distance, star class, planet class, highlighting planets in hab zone. Layout has been optimised.  Since the menu system was reworked all previous selections of display type will need to be reset - use the drop down menu to select them.  The default is everything on.\r\n* UI won't stall when looking up data from EDSM - previous it would stop until EDSM responded. Now just the panel which is asking will stop updating. Rest of the system carries on.\r\n* Material Trader panel added - plan you material trades in advance to see the best outcome before you commit to flying to a trader.\r\n* Surveyor Panel gets many more options for display - show all planets/stars etc and gets more information\r\n* Travel grid, Ships/Loadout, Material Commodities, Engineering, Synthesis get word wrap option to word wrap columns instead of truncating them. Double click on the row now works better expanding/contracting the text.\r\n* Ships/Loadout gets a All modules selection to list all your modules across all ships - useful for engineering\r\n* Synthesis, Engineering and Shopping list panels\r\n\r\nOther Improvements\r\n\r\n* All materials are now classified by Material Group Type\r\n* Improved loading speed when multiple tabbed panels are present - does not sit and use processing time now like it could do\r\n* EDSM Data pick up includes surface gravity\r\n* Journal Missions entry gets faction effects printed\r\n* Can force sell a ship if for some reason your journal has lost the sell ship event\r\n* Various Forms get a close X icon\r\n* Fuel/Reservoir updates are much better, Ships/loadouts auto change with them, and they don't bombard the system with micro changes\r\n* Star Distance panel - fix issue when setting the Max value which could cause it not to look up any stars again\r\n* Workaround for GDI error when saving bitmap\r\n* Bounty Event report correct ship name\r\n* New Y resizer on top of EDD form so you can resize there\r\n* Removed old surface scanner engineering recipes\r\n* Excel output of scan data now works out the value correctly dependent on if you mapped the body\r\n* Can force EDD to use TLS2 \r\n* Asteroid Prospected prints out mats in normal info, Mining refined gets total and type as per MaterialCollected\r\n\r\n***\r\n\r\n|  | EDDiscovery <version>.exe |\r\n|---------|------------------------------------------------------------------|\r\n| SHA-256 | 01D84BF967FE5CDFF2DDC782F0D68FCB4B80F3881EE1F883941454DF9FBB8823 | \r\n\r\n|  |  EDDiscovery.Portable.zip |\r\n|---------|------------------------------------------------------------------|\r\n| SHA-256 | 1D365A30B51280B4676410694C3D1E9F21DF525403E53B735245FD6C7B584DCA |\r\n\r\n![image](https://user-images.githubusercontent.com/6573992/80213091-8d931400-8630-11ea-9f3c-f56d43f7edd8.png)\r\n\r\n\r\n\r\n\r\n""
        }";

        [Test]
        public void JSONGithub()
        {
            JToken decode = JToken.Parse(jsongithub);
            Check.That(decode).IsNotNull();
            string json2 = decode.ToString(true);
            JToken decode2 = JToken.Parse(json2);
            Check.That(decode2).IsNotNull();
            string json3 = decode2.ToString(true);
            Check.That(json2).IsEqualTo(json3);

            var asset = decode["assets"];
            var e1 = asset.FirstOrDefault(func);
            Check.That(e1).IsNotNull();
            Check.That(e1["size"].IsInt).IsTrue();
            Check.That(e1["size1"]?.IsInt ?? true).IsTrue();
            Check.That(e1["size"].Int() == 11140542).IsTrue();
            Check.That(e1["state"].Str() == "uploaded").IsTrue();
        }

        bool func(JToken j)
        {
            if (j["name"].Str().ToLowerInvariant().EndsWith(".zip") && j["name"].Str().ToLowerInvariant().Contains("portable"))
                return true;
            else
                return false;
        }

        struct FileLines
        {
            public string[] filelines;
        }

        [Test]
        public void JSONSpeed()
        {
            string[] files = Directory.EnumerateFiles(@"C:\Users\RK\Saved Games\Frontier Developments\Elite Dangerous", "*.log").ToArray();

            List<FileLines> filelines = new List<FileLines>();

            foreach (var f in files)
            {
                // System.Diagnostics.Debug.WriteLine("Check " + f);
                string[] lines = File.ReadAllLines(f);
                filelines.Add(new FileLines { filelines = lines });
            }

            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            st.Start();

            foreach (var fl in filelines)
            {
                foreach (var l in fl.filelines)
                {
                    JObject t = JObject.Parse(l, out string error, JToken.ParseOptions.CheckEOL);
                    Check.That(t).IsNotNull();
                    JObject t2 = JObject.Parse(l, out string error2, JToken.ParseOptions.CheckEOL);
                    Check.That(t2).IsNotNull();
                    JObject t3 = JObject.Parse(l, out string error3, JToken.ParseOptions.CheckEOL);
                    Check.That(t3).IsNotNull();
                    JObject t4 = JObject.Parse(l, out string error4, JToken.ParseOptions.CheckEOL);
                    Check.That(t4).IsNotNull();
                    JObject t5 = JObject.Parse(l, out string error5, JToken.ParseOptions.CheckEOL);
                    Check.That(t5).IsNotNull();
                    JObject t6 = JObject.Parse(l, out string error6, JToken.ParseOptions.CheckEOL);
                    Check.That(t6).IsNotNull();
                }

            }

            long time = st.ElapsedMilliseconds;
            System.Diagnostics.Debug.WriteLine("Read journals took " + time);

        }

        [Test]
        public void JSONDeepClone()
        {
            JToken decode = JToken.Parse(jsongithub);
            Check.That(decode).IsNotNull();
            JToken copy = decode.Clone();
            Check.That(copy).IsNotNull();
            string json1 = decode.ToString(true);
            string json2 = copy.ToString(true);
            Check.That(json1).Equals(json2);
            System.Diagnostics.Debug.WriteLine(json2);

        }

        [Test]
        public void JSONDeepEquals()
        {
            if ( true )
            {
                JToken decode = JToken.Parse(jsongithub);
                Check.That(decode).IsNotNull();
                JToken copy = decode.Clone();
                Check.That(copy).IsNotNull();
                string json1 = decode.ToString(true);
                string json2 = copy.ToString(true);
                Check.That(json1).Equals(json2);
                System.Diagnostics.Debug.WriteLine(json2);

                Check.That(decode.DeepEquals(copy)).IsTrue();
            }

            if (true)
            {
                string json1 = "{\"SystemAllegiance\":true,\"Array\":[10.0,-20.2321212123,-30.232,-30.0],\"String\":\"string\",\"bool\":true}";
                JToken decode1 = JToken.Parse(json1);
                string json1out = decode1.ToString();

                Check.That(json1.Equals(json1out)).IsTrue();

                string json2 = "{\"SystemAllegiance\":true,\"Array\":[10,-20.2321212123,-30.232,-30.0],\"String\":\"string\",\"bool\":1}";
                JToken decode2 = JToken.Parse(json2);

                Check.That(decode1.DeepEquals(decode2)).IsTrue();

                string json3 = "{\"SystemAllegiance\":true,\"Array\":[10,-20.2321212123,-30.232,-30.0],\"String\":\"string\",\"bool\":\"string\"}";
                JToken decode3 = JToken.Parse(json3);

                Check.That(decode1.DeepEquals(decode3)).IsFalse();
            }


            if (true)
            {
                string json1 = @"{""timestamp"":""2016-09-27T19:59:39Z"",""event"":""ShipyardTransfer"",""ShipType"":""FerDeLance"",""ShipID"":15,""System"":""Lembava"",""Distance"":939379235343040512.0,""TransferPrice"":2693097}";
                string json2 = @"{""timestamp"":""2016-09-27T19:59:39Z"",""event"":""ShipyardTransfer"",""ShipType"":""FerDeLance"",""ShipID"":15,""System"":""Lembava"",""Distance"":939379235343040512.0,""TransferPrice"":2693097}";
                JToken decode1 = JToken.Parse(json1);
                JToken decode2 = JToken.Parse(json2);
                Check.That(decode1.DeepEquals(decode2)).IsTrue();
            }
        }

        public class Unlocked
        {
            public string Name;
            public string Name_Localised;
        }

        public class Commodities
        {
            public string Name;
            public string Name_Localised;
            public string FriendlyName;
            public int Count;
        }

        public class Materials
        {
            public int Count;
            public string Name;
            public string Name_Localised;
            public string FriendlyName;
            public string Category;
            public System.Drawing.Bitmap fred;

            [BaseUtils.JSON.JsonName("QValue")]     // checking for json name override for fields and properties
            public int? qint;
            [BaseUtils.JSON.JsonName("RValue")]
            public int rint { get; set; }

            [BaseUtils.JSON.JsonIgnore]
            public int PropGet { get; }

            public int PropGetSet { get; set; }
        }

        public class SimpleTest
        {
            public string one;
            public string two;
            public int three;
            public bool four;
        }

        public class Material
        {
            public string Name { get; set; }        //FDNAME
            public string Name_Localised { get; set; }     
            public string FriendlyName { get; set; }        //friendly
            public int Count { get; set; }

            public void Normalise()
            {
            }
        }

        public class ProgressInformation
        {
            public string Engineer { get; set; }
            public long EngineerID { get; set; }
            public int? Rank { get; set; }       // only when unlocked
            public string Progress { get; set; }
            public int? RankProgress { get; set; }  // newish 3.x only when unlocked
        }

        public enum TestEnum { one,two, three};

        [Test]
        public void JSONToObject()
        {
            {
                string mats = @"{ ""timestamp"":""2020-04-23T19:18:18Z"", ""event"":""Materials"", ""Raw"":[ { ""Name"":""carbon"", ""Count"":77 }, { ""Name"":""sulphur"", ""Count"":81 }, { ""Name"":""tin"", ""Count"":46 }, { ""Name"":""chromium"", ""Count"":32 }, { ""Name"":""nickel"", ""Count"":83 }, { ""Name"":""zinc"", ""Count"":59 }, { ""Name"":""iron"", ""Count"":48 }, { ""Name"":""phosphorus"", ""Count"":28 }, { ""Name"":""manganese"", ""Count"":60 }, { ""Name"":""niobium"", ""Count"":26 }, { ""Name"":""molybdenum"", ""Count"":25 }, { ""Name"":""antimony"", ""Count"":27 }, { ""Name"":""mercury"", ""Count"":5 }, { ""Name"":""yttrium"", ""Count"":50 }, { ""Name"":""selenium"", ""Count"":23 }, { ""Name"":""zirconium"", ""Count"":16 }, { ""Name"":""cadmium"", ""Count"":65 }, { ""Name"":""germanium"", ""Count"":26 }, { ""Name"":""tellurium"", ""Count"":39 }, { ""Name"":""vanadium"", ""Count"":49 }, { ""Name"":""arsenic"", ""Count"":14 }, { ""Name"":""technetium"", ""Count"":9 }, { ""Name"":""polonium"", ""Count"":21 }, { ""Name"":""tungsten"", ""Count"":54 } ], ""Manufactured"":[ { ""Name"":""focuscrystals"", ""Name_Localised"":""Focus Crystals"", ""Count"":9 }, { ""Name"":""refinedfocuscrystals"", ""Name_Localised"":""Refined Focus Crystals"", ""Count"":20 }, { ""Name"":""shieldingsensors"", ""Name_Localised"":""Shielding Sensors"", ""Count"":11 }, { ""Name"":""wornshieldemitters"", ""Name_Localised"":""Worn Shield Emitters"", ""Count"":29 }, { ""Name"":""shieldemitters"", ""Name_Localised"":""Shield Emitters"", ""Count"":44 }, { ""Name"":""heatdispersionplate"", ""Name_Localised"":""Heat Dispersion Plate"", ""Count"":29 }, { ""Name"":""fedproprietarycomposites"", ""Name_Localised"":""Proprietary Composites"", ""Count"":3 }, { ""Name"":""fedcorecomposites"", ""Name_Localised"":""Core Dynamics Composites"", ""Count"":2 }, { ""Name"":""compoundshielding"", ""Name_Localised"":""Compound Shielding"", ""Count"":25 }, { ""Name"":""salvagedalloys"", ""Name_Localised"":""Salvaged Alloys"", ""Count"":24 }, { ""Name"":""heatconductionwiring"", ""Name_Localised"":""Heat Conduction Wiring"", ""Count"":33 }, { ""Name"":""gridresistors"", ""Name_Localised"":""Grid Resistors"", ""Count"":24 }, { ""Name"":""hybridcapacitors"", ""Name_Localised"":""Hybrid Capacitors"", ""Count"":22 }, { ""Name"":""mechanicalequipment"", ""Name_Localised"":""Mechanical Equipment"", ""Count"":41 }, { ""Name"":""mechanicalscrap"", ""Name_Localised"":""Mechanical Scrap"", ""Count"":35 }, { ""Name"":""polymercapacitors"", ""Name_Localised"":""Polymer Capacitors"", ""Count"":4 }, { ""Name"":""phasealloys"", ""Name_Localised"":""Phase Alloys"", ""Count"":8 }, { ""Name"":""uncutfocuscrystals"", ""Name_Localised"":""Flawed Focus Crystals"", ""Count"":17 }, { ""Name"":""highdensitycomposites"", ""Name_Localised"":""High Density Composites"", ""Count"":36 }, { ""Name"":""mechanicalcomponents"", ""Name_Localised"":""Mechanical Components"", ""Count"":26 }, { ""Name"":""chemicalprocessors"", ""Name_Localised"":""Chemical Processors"", ""Count"":28 }, { ""Name"":""conductivecomponents"", ""Name_Localised"":""Conductive Components"", ""Count"":27 }, { ""Name"":""biotechconductors"", ""Name_Localised"":""Biotech Conductors"", ""Count"":8 }, { ""Name"":""galvanisingalloys"", ""Name_Localised"":""Galvanising Alloys"", ""Count"":27 }, { ""Name"":""heatexchangers"", ""Name_Localised"":""Heat Exchangers"", ""Count"":17 }, { ""Name"":""conductivepolymers"", ""Name_Localised"":""Conductive Polymers"", ""Count"":19 }, { ""Name"":""configurablecomponents"", ""Name_Localised"":""Configurable Components"", ""Count"":13 }, { ""Name"":""heatvanes"", ""Name_Localised"":""Heat Vanes"", ""Count"":18 }, { ""Name"":""chemicalmanipulators"", ""Name_Localised"":""Chemical Manipulators"", ""Count"":26 }, { ""Name"":""heatresistantceramics"", ""Name_Localised"":""Heat Resistant Ceramics"", ""Count"":2 }, { ""Name"":""protoheatradiators"", ""Name_Localised"":""Proto Heat Radiators"", ""Count"":54 }, { ""Name"":""crystalshards"", ""Name_Localised"":""Crystal Shards"", ""Count"":10 }, { ""Name"":""exquisitefocuscrystals"", ""Name_Localised"":""Exquisite Focus Crystals"", ""Count"":16 }, { ""Name"":""unknownenergysource"", ""Name_Localised"":""Sensor Fragment"", ""Count"":11 }, { ""Name"":""protolightalloys"", ""Name_Localised"":""Proto Light Alloys"", ""Count"":1 }, { ""Name"":""thermicalloys"", ""Name_Localised"":""Thermic Alloys"", ""Count"":2 }, { ""Name"":""conductiveceramics"", ""Name_Localised"":""Conductive Ceramics"", ""Count"":18 }, { ""Name"":""chemicaldistillery"", ""Name_Localised"":""Chemical Distillery"", ""Count"":6 }, { ""Name"":""chemicalstorageunits"", ""Name_Localised"":""Chemical Storage Units"", ""Count"":3 } ], ""Encoded"":[ { ""Name"":""shielddensityreports"", ""Name_Localised"":""Untypical Shield Scans "", ""Count"":112 }, { ""Name"":""emissiondata"", ""Name_Localised"":""Unexpected Emission Data"", ""Count"":39 }, { ""Name"":""shieldcyclerecordings"", ""Name_Localised"":""Distorted Shield Cycle Recordings"", ""Count"":208 }, { ""Name"":""scrambledemissiondata"", ""Name_Localised"":""Exceptional Scrambled Emission Data"", ""Count"":29 }, { ""Name"":""decodedemissiondata"", ""Name_Localised"":""Decoded Emission Data"", ""Count"":38 }, { ""Name"":""classifiedscandata"", ""Name_Localised"":""Classified Scan Fragment"", ""Count"":7 }, { ""Name"":""consumerfirmware"", ""Name_Localised"":""Modified Consumer Firmware"", ""Count"":20 }, { ""Name"":""industrialfirmware"", ""Name_Localised"":""Cracked Industrial Firmware"", ""Count"":10 }, { ""Name"":""encryptedfiles"", ""Name_Localised"":""Unusual Encrypted Files"", ""Count"":3 }, { ""Name"":""scanarchives"", ""Name_Localised"":""Unidentified Scan Archives"", ""Count"":106 }, { ""Name"":""legacyfirmware"", ""Name_Localised"":""Specialised Legacy Firmware"", ""Count"":33 }, { ""Name"":""disruptedwakeechoes"", ""Name_Localised"":""Atypical Disrupted Wake Echoes"", ""Count"":67 }, { ""Name"":""hyperspacetrajectories"", ""Name_Localised"":""Eccentric Hyperspace Trajectories"", ""Count"":33 }, { ""Name"":""wakesolutions"", ""Name_Localised"":""Strange Wake Solutions"", ""Count"":25 }, { ""Name"":""encodedscandata"", ""Name_Localised"":""Divergent Scan Data"", ""Count"":16 }, { ""Name"":""archivedemissiondata"", ""Name_Localised"":""Irregular Emission Data"", ""Count"":5 }, { ""Name"":""encryptioncodes"", ""Name_Localised"":""Tagged Encryption Codes"", ""Count"":3 }, { ""Name"":""scandatabanks"", ""Name_Localised"":""Classified Scan Databanks"", ""Count"":110 }, { ""Name"":""shieldfrequencydata"", ""Name_Localised"":""Peculiar Shield Frequency Data"", ""Count"":18 }, { ""Name"":""unknownshipsignature"", ""Name_Localised"":""Thargoid Ship Signature"", ""Count"":3 }, { ""Name"":""unknownwakedata"", ""Name_Localised"":""Thargoid Wake Data"", ""Count"":3 }, { ""Name"":""embeddedfirmware"", ""Name_Localised"":""Modified Embedded Firmware"", ""Count"":3 }, { ""Name"":""securityfirmware"", ""Name_Localised"":""Security Firmware Patch"", ""Count"":5 }, { ""Name"":""shieldpatternanalysis"", ""Name_Localised"":""Aberrant Shield Pattern Analysis"", ""Count"":66 }, { ""Name"":""shieldsoakanalysis"", ""Name_Localised"":""Inconsistent Shield Soak Analysis"", ""Count"":117 }, { ""Name"":""fsdtelemetry"", ""Name_Localised"":""Anomalous FSD Telemetry"", ""Count"":24 }, { ""Name"":""bulkscandata"", ""Name_Localised"":""Anomalous Bulk Scan Data"", ""Count"":147 }, { ""Name"":""compactemissionsdata"", ""Name_Localised"":""Abnormal Compact Emissions Data"", ""Count"":9 }, { ""Name"":""dataminedwake"", ""Name_Localised"":""Datamined Wake Exceptions"", ""Count"":7 } ] }";

                JObject jo = JObject.Parse(mats);

                var matsraw = jo["Raw"].ToObject<Material>();        // check it can handle incorrect type
                Check.That(matsraw).IsNull();
                var matsraw2 = jo["Raw"].ToObject<Material[]>();        // check it can handle nullable types
                Check.That(matsraw2).IsNotNull();

                Stopwatch sw = new Stopwatch();
                sw.Start();

                long tick = sw.ElapsedTicks;

                for( int i = 0; i < 3000; i++ )
                {
                    var matsraw3r = jo["Raw"].ToObjectQ<Material[]>();        // check it can handle nullable types
                    Check.That(matsraw3r).IsNotNull();
                    var matsraw3m = jo["Manufactured"].ToObjectQ<Material[]>();        // check it can handle nullable types
                    Check.That(matsraw3m).IsNotNull();
                    var matsraw3e = jo["Encoded"].ToObjectQ<Material[]>();        // check it can handle nullable types
                    Check.That(matsraw3e).IsNotNull();
                }

                // 1794ms ToObject, 745 ToObjectQ, 526 with change

                long time = sw.ElapsedTicks - tick;
                double timems = (double)time / Stopwatch.Frequency * 1000;
                System.Diagnostics.Trace.WriteLine("Time is " + timems);
                File.WriteAllText(@"c:\code\time.txt", "Time is " + timems);
            }

            {
                string ts = @"{""IsEmpty"":false,""Width"":10.1999998092651,""Height"":12.1999998092651,""Empty"":{""IsEmpty"":true,""Width"":0.0,""Height"":0.0}}";
                JToken j = JToken.Parse(ts);
                SizeF sf = j.ToObject<SizeF>(true);
            }


            {
                string mats = @"{ ""Materials"":{ ""iron"":19.741276, ""sulphur"":17.713514 } }";

                JObject jo = JObject.Parse(mats);

                var matsdict = jo["Materials"].ToObject<Dictionary<string, double?>>();        // check it can handle nullable types
                Check.That(matsdict).IsNotNull();
                Check.That(matsdict["iron"].HasValue && matsdict["iron"].Value == 19.741276);

                var matsdict2 = jo["Materials"].ToObject<Dictionary<string, double>>();        // and normal
                Check.That(matsdict2).IsNotNull();
                Check.That(matsdict2["iron"] == 19.741276);

                string mats3 = @"{ ""Materials"":{ ""iron"":20, ""sulphur"":17.713514 } }";
                JObject jo3 = JObject.Parse(mats3);
                var matsdict3 = jo3["Materials"].ToObject<Dictionary<string, double>>();        // and normal
                Check.That(matsdict3).IsNotNull();
                Check.That(matsdict3["iron"] == 20);

                string mats4 = @"{ ""Materials"":{ ""iron"":null, ""sulphur"":17.713514 } }";
                JObject jo4 = JObject.Parse(mats4);
                var matsdict4 = jo4["Materials"].ToObject<Dictionary<string, double?>>();        // and normal
                Check.That(matsdict4).IsNotNull();
                Check.That(matsdict4["iron"] == null);

                string mats5 = @"{ ""Materials"":{ ""iron"":""present"", ""sulphur"":null } }";
                JObject jo5 = JObject.Parse(mats5);
                var matsdict5 = jo5["Materials"].ToObject<Dictionary<string, string>>();        // and normal
                Check.That(matsdict4).IsNotNull();
                Check.That(matsdict4["iron"] == null);
            }


            {
                string englist = @"{ ""timestamp"":""2020 - 08 - 03T12: 07:15Z"",""event"":""EngineerProgress"",""Engineers"":[{""Engineer"":""Etienne Dorn"",""EngineerID"":2929,""Progress"":""Invited"",""Rank"":null},{""Engineer"":""Zacariah Nemo"",""EngineerID"":300050,""Progress"":""Known""},{""Engineer"":""Tiana Fortune"",""EngineerID"":300270,""Progress"":""Invited""},{""Engineer"":""Chloe Sedesi"",""EngineerID"":300300,""Progress"":""Invited""},{""Engineer"":""Marco Qwent"",""EngineerID"":300200,""Progress"":""Unlocked"",""RankProgress"":55,""Rank"":3},{""Engineer"":""Petra Olmanova"",""EngineerID"":300130,""Progress"":""Invited""},{""Engineer"":""Hera Tani"",""EngineerID"":300090,""Progress"":""Unlocked"",""RankProgress"":59,""Rank"":3},{""Engineer"":""Tod 'The Blaster' McQuinn"",""EngineerID"":300260,""Progress"":""Unlocked"",""RankProgress"":0,""Rank"":5},{""Engineer"":""Marsha Hicks"",""EngineerID"":300150,""Progress"":""Invited""},{""Engineer"":""Selene Jean"",""EngineerID"":300210,""Progress"":""Unlocked"",""RankProgress"":0,""Rank"":5},{""Engineer"":""Lei Cheung"",""EngineerID"":300120,""Progress"":""Unlocked"",""RankProgress"":0,""Rank"":5},{""Engineer"":""Juri Ishmaak"",""EngineerID"":300250,""Progress"":""Unlocked"",""RankProgress"":0,""Rank"":5},{""Engineer"":""Felicity Farseer"",""EngineerID"":300100,""Progress"":""Unlocked"",""RankProgress"":0,""Rank"":5},{""Engineer"":""Broo Tarquin"",""EngineerID"":300030,""Progress"":""Unlocked"",""RankProgress"":0,""Rank"":5},{""Engineer"":""Professor Palin"",""EngineerID"":300220,""Progress"":""Unlocked"",""RankProgress"":0,""Rank"":5},{""Engineer"":""Colonel Bris Dekker"",""EngineerID"":300140,""Progress"":""Invited""},{""Engineer"":""Elvira Martuuk"",""EngineerID"":300160,""Progress"":""Unlocked"",""RankProgress"":0,""Rank"":5},{""Engineer"":""Lori Jameson"",""EngineerID"":300230,""Progress"":""Invited""},{""Engineer"":""The Dweller"",""EngineerID"":300180,""Progress"":""Unlocked"",""RankProgress"":0,""Rank"":5},{""Engineer"":""Liz Ryder"",""EngineerID"":300080,""Progress"":""Unlocked"",""RankProgress"":81,""Rank"":3},{""Engineer"":""Didi Vatermann"",""EngineerID"":300000,""Progress"":""Invited""},{""Engineer"":""The Sarge"",""EngineerID"":300040,""Progress"":""Invited""},{""Engineer"":""Mel Brandon"",""EngineerID"":300280,""Progress"":""Known""},{""Engineer"":""Ram Tah"",""EngineerID"":300110,""Progress"":""Invited""},{""Engineer"":""Bill Turner"",""EngineerID"":300010,""Progress"":""Invited""}]}";
                JToken englistj = JToken.Parse(englist);

                var pinfo = englistj["Engineers"]?.ToObject<ProgressInformation[]>();
                Check.That(pinfo).IsNotNull();
                Check.That(pinfo.Count()).Equals(25);
            }

            {
                string json = "[ \"one\",\"two\",\"three\" ] ";
                JToken decode = JToken.Parse(json);

                var decoded = decode.ToObject(typeof(string[]), false, true);
                if (decoded is JTokenExtensions.ToObjectError)
                    System.Diagnostics.Debug.WriteLine("Err " + ((JTokenExtensions.ToObjectError)decoded).ErrorString);

                var decoded2 = decode.ToObject(typeof(string), false, true);
                Check.That(decoded2).IsInstanceOfType(typeof(JTokenExtensions.ToObjectError));
                if (decoded2 is JTokenExtensions.ToObjectError)
                    System.Diagnostics.Debug.WriteLine("Err " + ((JTokenExtensions.ToObjectError)decoded2).ErrorString);
            }

            {
                string json = "{ \"one\":\"one\", \"two\":\"two\" , \"three\":30, \"four\":true }";
                JToken decode = JToken.Parse(json);

                var decoded = decode.ToObject(typeof(SimpleTest), false, true);
                if (decoded is JTokenExtensions.ToObjectError)
                    System.Diagnostics.Debug.WriteLine("Err " + ((JTokenExtensions.ToObjectError)decoded).ErrorString);
            }

            {
                string jmd = @"
{
  ""timestamp"": ""2018 - 04 - 24T21: 25:46Z"",
  ""event"": ""TechnologyBroker"",
  ""BrokerType"": ""guardian"",
  ""MarketID"": 3223529472,
  ""ItemsUnlocked"": [
    {
      ""Name"": ""Int_GuardianPowerplant_Size2"",
      ""Name_Localised"": ""Guardian Power Plant""
    },
    {
      ""Name"": ""Int_GuardianPowerplant_Size3"",
      ""Name_Localised"": ""$Int_GuardianPowerplant_Size2_Name;""
    },
    {
      ""Name"": ""Int_GuardianPowerplant_Size4"",
      ""Name_Localised"": ""$Int_GuardianPowerplant_Size2_Name;""
    },
    {
      ""Name"": ""Int_GuardianPowerplant_Size5"",
      ""Name_Localised"": ""$Int_GuardianPowerplant_Size2_Name;""
    },
    {
      ""Name"": ""Int_GuardianPowerplant_Size6"",
      ""Name_Localised"": ""$Int_GuardianPowerplant_Size2_Name;""
    },
    {
      ""Name"": ""Int_GuardianPowerplant_Size7"",
      ""Name_Localised"": ""$Int_GuardianPowerplant_Size2_Name;""
    },
    {
      ""Name"": ""Int_GuardianPowerplant_Size8"",
      ""Name_Localised"": ""$Int_GuardianPowerplant_Size2_Name;""
    }
  ],
  ""Commodities"": [
    {
      ""Name"": ""powergridassembly"",
      ""Name_Localised"": ""Energy Grid Assembly"",
      ""Count"": 10
    }
  ],
  ""Materials"": [
    {
      ""Name"": ""guardian_moduleblueprint"",
      ""Name_Localised"": ""Guardian Module Blueprint Segment"",
      ""Count"": 4,
      ""Category"": ""Encoded""
    },
    {
      ""Name"": ""guardian_powerconduit"",
      ""Name_Localised"": ""Guardian Power Conduit"",
      ""Count"": 36,
      ""Category"": ""Manufactured""
    },
    {
      ""Name"": ""ancienttechnologicaldata"",
      ""Name_Localised"": ""Pattern Epsilon Obelisk Data"",
      ""Count"": 42,
      ""Category"": ""Encoded""
    },
    {
      ""Name"": ""heatresistantceramics"",
      ""Name_Localised"": ""Heat Resistant Ceramics"",
      ""Count"": 30,
      ""Category"": ""Manufactured"",
      ""QValue"": 20,
      ""RValue"": 2000
    }
  ]
}";


                JToken decode = JToken.Parse(jmd);
                Check.That(decode).IsNotNull();
                string json1 = decode.ToString(true);
                System.Diagnostics.Debug.WriteLine(json1);

                var ItemsUnlocked1 = decode["WrongNameItemsUnlocked"].ToObject(typeof(Unlocked[]), false, true);
                Check.That(ItemsUnlocked1).IsNull();
                var ItemsUnlocked = decode["ItemsUnlocked"].ToObject(typeof(Unlocked[]), false, true);
                Check.That(ItemsUnlocked).IsNotNull();
                var CommodityList = decode["Commodities"].ToObject<Commodities[]>();
                Check.That(CommodityList).IsNotNull();
                var MaterialList = decode["Materials"].ToObject<Materials[]>();
                Check.That(MaterialList).IsNotNull();
                Check.That(MaterialList.Length).IsEqualTo(4);
                Check.That(MaterialList[3].qint).IsEqualTo(20);
                Check.That(MaterialList[3].rint).IsEqualTo(2000);


            }
            {
                string listp2 = @"{ ""Materials"":[ ""iron"" , ""nickel"" ]}";
                JToken evt3 = JObject.Parse(listp2);
                var liste = evt3["Materials"].ToObject<List<string>>();  // name in fd logs is lower case
                Check.That(liste).IsNotNull();
                Check.That(liste.Count).IsEqualTo(2);

                string dicp2 = @"{ ""Materials"":{ ""iron"":22.1, ""nickel"":16.7, ""sulphur"":15.6, ""carbon"":13.2, ""chromium"":9.9, ""phosphorus"":8.4 }}";
                JToken evt2 = JObject.Parse(dicp2);
                var Materials2 = evt2["Materials"].ToObject<Dictionary<string, double>>();  // name in fd logs is lower case
                Check.That(Materials2).IsNotNull();
                Check.That(Materials2.Count).IsEqualTo(6);

                var Materials3fail = evt2["Materials"].ToObject<Dictionary<string, string>>();  // name in fd logs is lower case
                Check.That(Materials3fail).IsNull();



                string dicpair = @"{ ""Materials"":[ { ""Name"":""iron"", ""Percent"":19.741276 }, { ""Name"":""sulphur"", ""Percent"":17.713514 }, { ""Name"":""nickel"", ""Percent"":14.931473 }, { ""Name"":""carbon"", ""Percent"":14.895230 }, { ""Name"":""phosphorus"", ""Percent"":9.536182 } ] }";
                JToken evt = JObject.Parse(dicpair);
                JToken mats = evt["Materials"];

                if (mats != null)
                {
                    var Materials = new Dictionary<string, double>();
                    foreach (JObject jo in mats)                                        // name in fd logs is lower case
                    {
                        string name = jo["Name"].Str();

                        Materials[name.ToLowerInvariant()] = jo["Percent"].Double();
                    }
                }

                string matlist = @"{ ""Raw"":[ { ""Name"":""iron"", ""Count"":10 }, { ""Name"":""sulphur"", ""Count"":17 } ] }";
                JToken matlistj = JToken.Parse(matlist);
                var Raw = matlistj["Raw"]?.ToObject<Material[]>();
                Check.That(Raw).IsNotNull();
                Check.That(Raw.Count()).Equals(2);
            }

        }

        class FromTest
        {
            public int v1;
            public string v2;
            public FromTest2 other1;
        }
        class FromTest2
        {
            public int v1;
            public string v2;
            public FromTest other2;
        }

        public class FromObjectTest
        {
            public TestEnum t1;
        }

        [Test]
        public void JSONFromObject()
        {
            //{     check code for types.. see what type() is saying
            //    int a = 10;
            //    float f = 20;
            //    Type tta = a.GetType();
            //    string b = "hello";
            //    Type ttb = b.GetType();
            //    SizeF sf = new SizeF(10.2f, 12.2f);
            //    Type ttc = sf.GetType();
            //    DateTime tme = DateTime.UtcNow;
            //    Type ttt = tme.GetType();
            //    JArray ja = new JArray();
            //    double? dt = 10.0;
            //    TestEnum enumvalue = TestEnum.one;
            //   // JToken t = JToken.FromObject(dt);
            //}

            {
                SizeF sf = new SizeF(10.2f, 12.2f);

                JToken t = JToken.FromObject(sf, true);
                string ts = @"{""IsEmpty"":false,""Width"":10.1999998092651,""Height"":12.1999998092651,""Empty"":{""IsEmpty"":true,""Width"":0.0,""Height"":0.0}}";
                Check.That(t.ToString()).Equals(ts);                // check ignores self ref and does as much as possible
            }

            {
                var fm = new FromTest() { v1 = 10, v2 = "Hello1" };
                var fm2 = new FromTest2() { v1 = 20, v2 = "Hello2" };
                fm.other1 = fm2;
                fm2.other2 = fm;

                JToken t = JToken.FromObjectWithError(fm, false);
                Check.That(t.IsInError).IsTrue();
                Check.That(((string)t.Value).Contains("Self")).IsTrue();        // check self ref fails
                System.Diagnostics.Debug.WriteLine(t.Value.ToString());

                JToken t2 = JToken.FromObjectWithError(fm, true);
                Check.That(t2.IsObject).IsTrue();
                Check.That(t2["other1"]["v1"].Int()).Equals(20);                // check ignores self ref and does as much as possible
                System.Diagnostics.Debug.WriteLine(t.Value.ToString());
            }

            {
                var mats = new Materials[2];
                mats[0] = new Materials();
                mats[0].Name = "0";
                mats[0].Name_Localised = "L0";
                mats[0].fred = new System.Drawing.Bitmap(20, 20);
                mats[1] = new Materials();
                mats[1].Name = "1";
                mats[1].Name_Localised = "L1";
                mats[1].qint = 20;

                JToken t = JToken.FromObject(mats, true, new System.Type[] { typeof(System.Drawing.Bitmap) });
                Check.That(t).IsNotNull();
                string json = t.ToString(true);
                System.Diagnostics.Debug.WriteLine("JSON " + json);
            }

            {
                string mats = @"{ ""Materials"":{ ""iron"":19.741276, ""sulphur"":17.713514 } }";
                JObject jo = JObject.Parse(mats);
                var matsdict = jo["Materials"].ToObject<Dictionary<string, double?>>();        // check it can handle nullable types
                Check.That(matsdict).IsNotNull();
                Check.That(matsdict["iron"].HasValue && matsdict["iron"].Value == 19.741276);
                var json = JToken.FromObject(matsdict);
                Check.That(json).IsNotNull();
                var jsonw = new JObject();
                jsonw["Materials"] = json;
                Check.That(jsonw.DeepEquals(jo)).IsTrue();
            }

            {
                var jo = new JArray();
                jo.Add(10.23);
                jo.Add(20.23);
                var var1 = jo.ToObject<List<double>>();
                var jback = JToken.FromObject(var1);
                Check.That(jback.DeepEquals(jo)).IsTrue();
            }

            {
                var mats2 = new Dictionary<string, double>();
                mats2["Iron"] = 20.2;
                mats2["Steel"] = 10;

                var json = JObject.FromObject(mats2);
                Check.That(json).IsNotNull();
                Check.That(json["Iron"].Double()).Equals(20.2);
                Check.That(json["Steel"].Double()).Equals(10);


            }

            {
                string propertyv =
@"[
  {
    ""Count"":0,
    ""Name"":""0"",
    ""Name_Localised"":""L0"",
    ""FriendlyName"":null,
    ""Category"":null,
    ""QValue"":null,
    ""PropGetSet"":1
  },
  {
    ""Count"":0,
    ""Name"":""1"",
    ""Name_Localised"":""L1"",
    ""FriendlyName"":null,
    ""Category"":null,
    ""QValue"":20
  }
]
";
                JToken matpro = JToken.Parse(propertyv);
                var Materials = matpro.ToObject<Materials[]>();

                JToken t = JToken.FromObject(Materials, true, new System.Type[] { typeof(System.Drawing.Bitmap) });
                string s = t.ToString();
                System.Diagnostics.Debug.WriteLine("JSON is " + s);

                string expout = @"[{""RValue"":0,""PropGetSet"":1,""Count"":0,""Name"":""0"",""Name_Localised"":""L0"",""FriendlyName"":null,""Category"":null,""QValue"":null},{""RValue"":0,""PropGetSet"":0,""Count"":0,""Name"":""1"",""Name_Localised"":""L1"",""FriendlyName"":null,""Category"":null,""QValue"":20}]";
                System.Diagnostics.Debug.WriteLine("exp is " + expout);

                Check.That(s).Equals(expout);
            }

            {
                FromObjectTest s = new FromObjectTest();
                s.t1 = TestEnum.three;

                JToken t = JToken.FromObject(s);

                FromObjectTest r = t.ToObject<FromObjectTest>();
                Check.That(r.t1 == TestEnum.three);
            }


        }

        [Test]
        public void JSONtextReader()
        {
                string propertyv =
@"    [
  {
    ""Count"":0,
    ""Name"":""0"",
    ""Name_Localised"":""L0"",
    ""FriendlyName"":null,
    ""Category"":null,
    ""QValue"":null,
    ""PropGetSet"":1
  },
  {
    ""Count"":0,
    ""Name"":""1"",
    ""Name_Localised"":""This is a long string to try and make the thing break"",
    ""FriendlyName"":null,
    ""Category"":null,
    ""QValue"":20
  }
]
";
            {
                using (StringReader sr = new StringReader(propertyv))         // read directly from file..
                {
                    foreach (var t in JToken.ParseToken(sr, JToken.ParseOptions.None))
                    {
                        if (t.IsProperty)
                            System.Diagnostics.Debug.WriteLine("Property " + t.Name + " " + t.TokenType + " `" + t.Value + "`");
                        else
                            System.Diagnostics.Debug.WriteLine("Token " + t.TokenType + " " + t.Value);
                    }

                }

            }

            {

                using (StringReader sr = new StringReader(jsongithub))         // read directly from file..
                {
                    foreach (var t in JToken.ParseToken(sr,JToken.ParseOptions.None))
                    {
                        if (t.IsProperty)
                            System.Diagnostics.Debug.WriteLine("Property " + t.Name + " " + t.TokenType + " `" + t.Value + "`");
                        else
                            System.Diagnostics.Debug.WriteLine("Token " + t.TokenType + " " + t.Value);
                    }

                }
            }
            {
                using (StringReader sr = new StringReader(jsongithub))         // read directly from file..
                {
                    var enumerator = JToken.ParseToken(sr,JToken.ParseOptions.None).GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        var t = enumerator.Current;
                        if (t.IsProperty)
                            System.Diagnostics.Debug.WriteLine("Property " + t.Name + " " + t.TokenType + " `" + t.Value + "`");
                        else
                            System.Diagnostics.Debug.WriteLine("Token " + t.TokenType + " " + t.Value);
                    }
                }
            }

            {
                string propertyq =
@"    [
  {
    ""Count"":0,
    ""Name"":""0"",
    ""Name_Localised"":""L0"",
    ""FriendlyName"":null,
    ""ArrayValue"":[1,2,3],
    ""Category"":null,
    ""QValue"":null,
    ""PropGetSet"":1
  },
  {
    ""Count"":0,
    ""Name"":""1"",
    ""Name_Localised"":""This is a long string to try and make the thing break"",
    ""FriendlyName"":null,
    ""Category"":null,
    ""QValue"":20
  }
]
";

                using (StringReader sr = new StringReader(propertyq))         // read directly from file..
                {
                    var enumerator = JToken.ParseToken(sr,JToken.ParseOptions.None,128).GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        var t = enumerator.Current;
                        if (t.IsObject)
                        {
                            JObject to = t as JObject;
                            bool res = enumerator.Load();
                            Check.That(res).IsTrue();
                            Check.That(to["Category"]).IsNotNull();
                        }
                    }
                }
            }


            string filename = @"c:\code\edsm\edsmsystems.10000.json";
            if ( File.Exists(filename))
            {
                using (FileStream originalFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(originalFileStream))
                    {
                        foreach (var t in JToken.ParseToken(sr,JToken.ParseOptions.None,2999))
                        {
                            if (t.IsProperty)
                                System.Diagnostics.Debug.WriteLine("Property " + t.Name + " " + t.TokenType + " `" + t.Value + "`");
                            else
                                System.Diagnostics.Debug.WriteLine("Token " + t.TokenType + " " + t.Value);
                        }
                    }
                }
            }

        }

    }
}