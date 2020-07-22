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
using BaseUtils;
using BaseUtils.JSON;
using NFluent;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDDiscoveryTests
{
    [TestFixture(TestOf = typeof(JToken))]
    public class JSONTests
    {
        [Test]
        public void JSONBasic()
        {
            string json = "{ \"timestamp\":\"2020-06-29T09:53:54Z\", \"event\":\"FSDJump\t\", \"StarSystem\":\"Shinrarta Dezhra\", \"SystemAddress\":3932277478106, \"StarPos\":[55.71875,17.59375,27.15625], \"SystemAllegiance\":\"PilotsFederation\", \"SystemEconomy\":\"$economy_HighTech;\", \"SystemEconomy_Localised\":\"High Tech\", \"SystemSecondEconomy\":\"$economy_Industrial;\", \"SystemSecondEconomy_Localised\":\"Industrial\", \"SystemGovernment\":\"$government_Democracy;\", \"SystemGovernment_Localised\":\"Democracy\", \"SystemSecurity\":\"$SYSTEM_SECURITY_high;\", \"SystemSecurity_Localised\":\"High Security\", \"Population\":85206935, \"Body\":\"Shinrarta Dezhra\", \"BodyID\":1, \"BodyType\":\"Star\", \"JumpDist\":5.600, \"FuelUsed\":0.387997, \"FuelLevel\":31.612003, \"Factions\":[ { \"Name\":\"LTT 4487 Industry\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.288000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000, \"RecoveringStates\":[ { \"State\":\"Drought\", \"Trend\":0 } ] }, { \"Name\":\"Future of Arro Naga\", \"FactionState\":\"Outbreak\", \"Government\":\"Democracy\", \"Influence\":0.139000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000, \"ActiveStates\":[ { \"State\":\"Outbreak\" } ] }, { \"Name\":\"The Dark Wheel\", \"FactionState\":\"CivilUnrest\", \"Government\":\"Democracy\", \"Influence\":0.376000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000, \"PendingStates\":[ { \"State\":\"Expansion\", \"Trend\":0 } ], \"RecoveringStates\":[ { \"State\":\"PublicHoliday\", \"Trend\":0 } ], \"ActiveStates\":[ { \"State\":\"CivilUnrest\" } ] }, { \"Name\":\"Los Chupacabras\", \"FactionState\":\"None\", \"Government\":\"PrisonColony\", \"Influence\":0.197000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000, \"RecoveringStates\":[ { \"State\":\"Outbreak\", \"Trend\":0 } ] } ], \"SystemFaction\":{ \"Name\":\"Pilots' Federation Local Branch\" } }";

            JToken decoded = JToken.Parse(json);
            Check.That(decoded).IsNotNull();
            string outstr = decoded.ToString( true);
            System.Diagnostics.Debug.WriteLine("" + outstr);

            JToken decoded2 = JToken.Parse(outstr);

            string outstr2 = decoded2.ToString(true);
            System.Diagnostics.Debug.WriteLine("" + outstr2);

            Check.That(outstr).IsEqualTo(outstr2);

            JObject jo = decoded as JObject;
            Check.That(jo).IsNotNull();

            string j = jo["timestamp"].Str();
            Check.That(j).Equals("2020-06-29T09:53:54Z");

        }

        [Test]
        public void JSONObject1()
        {
            JObject j1 = new JObject();
            j1["One"] = "one";
            j1["Two"] = "two";
            JArray ja = new JArray();
            ja.Elements = new List<JToken> { "one", "two", 10.23 };
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
            foreach (KeyValuePair<string,JToken> p in jo)
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

            string s = ja.Find<JString>(x => x is JString && ((JString)x).Value.Equals("one"))?.Value;

            Check.That(s).IsNotNull().Equals("one");

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

        [Test]
        public void JSONGithub()
        {
            string json = @"
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

            JToken decode = JToken.Parse(json);
            Check.That(decode).IsNotNull();
            string json2 = decode.ToString(true);
            JToken decode2 = JToken.Parse(json2);
            Check.That(decode2).IsNotNull();
            string json3 = decode2.ToString(true);
            Check.That(json2).IsEqualTo(json3);

            var asset = decode["assets"];
            var e1 = asset.FirstOrDefault(func);
            Check.That(e1).IsNotNull();
            Check.That(e1["size"] is JLong).IsTrue();
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
    }
}