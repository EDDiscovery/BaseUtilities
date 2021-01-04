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


namespace EDDiscoveryTests
{
    [TestFixture(TestOf = typeof(QuickJsonDecoder))]
    public class JSONTests
    {
        [Test]
        public void JSONBasic()
        {
            string json = "{ \"timestamp\":\"2020-06-29T09:53:54Z\", \"event\":\"FSDJump\t\", \"StarSystem\":\"Shinrarta Dezhra\", \"SystemAddress\":3932277478106, \"StarPos\":[55.71875,17.59375,27.15625], \"SystemAllegiance\":\"PilotsFederation\", \"SystemEconomy\":\"$economy_HighTech;\", \"SystemEconomy_Localised\":\"High Tech\", \"SystemSecondEconomy\":\"$economy_Industrial;\", \"SystemSecondEconomy_Localised\":\"Industrial\", \"SystemGovernment\":\"$government_Democracy;\", \"SystemGovernment_Localised\":\"Democracy\", \"SystemSecurity\":\"$SYSTEM_SECURITY_high;\", \"SystemSecurity_Localised\":\"High Security\", \"Population\":85206935, \"Body\":\"Shinrarta Dezhra\", \"BodyID\":1, \"BodyType\":\"Star\", \"JumpDist\":5.600, \"FuelUsed\":0.387997, \"FuelLevel\":31.612003, \"Factions\":[ { \"Name\":\"LTT 4487 Industry\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.288000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000, \"RecoveringStates\":[ { \"State\":\"Drought\", \"Trend\":0 } ] }, { \"Name\":\"Future of Arro Naga\", \"FactionState\":\"Outbreak\", \"Government\":\"Democracy\", \"Influence\":0.139000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000, \"ActiveStates\":[ { \"State\":\"Outbreak\" } ] }, { \"Name\":\"The Dark Wheel\", \"FactionState\":\"CivilUnrest\", \"Government\":\"Democracy\", \"Influence\":0.376000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000, \"PendingStates\":[ { \"State\":\"Expansion\", \"Trend\":0 } ], \"RecoveringStates\":[ { \"State\":\"PublicHoliday\", \"Trend\":0 } ], \"ActiveStates\":[ { \"State\":\"CivilUnrest\" } ] }, { \"Name\":\"Los Chupacabras\", \"FactionState\":\"None\", \"Government\":\"PrisonColony\", \"Influence\":0.197000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000, \"RecoveringStates\":[ { \"State\":\"Outbreak\", \"Trend\":0 } ] } ], \"SystemFaction\":{ \"Name\":\"Pilots' Federation Local Branch\" } }";

            QuickJsonDecoder qjd = new QuickJsonDecoder(json);
            Object decoded = qjd.Decode();

            string outstr = QuickJsonDecoder.ToString(decoded, true);
            System.Diagnostics.Debug.WriteLine("" + outstr);

            QuickJsonDecoder qjd2 = new QuickJsonDecoder(outstr);
            Object decoded2 = qjd2.Decode();

            string outstr2 = QuickJsonDecoder.ToString(decoded2, true);
            System.Diagnostics.Debug.WriteLine("" + outstr2);

            Check.That(outstr).IsEqualTo(outstr2);
        }

        [Test]
        public void JSONObject1()
        {
            JObject j1 = new JObject();
            j1["One"] = "one";
            j1["Two"] = "two";
            JArray ja = new JArray();
            ja.Elements = new List<Object> { "one", "two", 10.23 };
            j1["Array"] = ja;

            System.Diagnostics.Debug.WriteLine("" + j1.ToString().QuoteString());

            string expectedjson = "{\"One\":\"one\",\"Two\":\"two\",\"Array\":[\"one\",\"two\",10.23]}";

            Check.That(j1.ToString()).IsEqualTo(expectedjson);
        }

        [Test]
        public void JSONObject2()
        {
            JObject AllowedFieldsCommon = new JObject
            {
                ["timestamp"] = true,
                ["event"] = true,
                ["StarSystem"] = true,
                ["SystemAddress"] = true,
            };

            System.Diagnostics.Debug.WriteLine("" + AllowedFieldsCommon.ToString().QuoteString());

            string expectedjson = "{\"timestamp\":true,\"event\":true,\"StarSystem\":true,\"SystemAddress\":true}";

            Check.That(AllowedFieldsCommon.ToString()).IsEqualTo(expectedjson);
        }

        [Test]
        public void JSONArray()
        {
            JArray ja = new JArray
            {
                "one",
                "two",
                "three"
            };

            System.Diagnostics.Debug.WriteLine("" + ja.ToString().QuoteString());

            string expectedjson = "[\"one\",\"two\",\"three\"]";

            Check.That(ja.ToString()).IsEqualTo(expectedjson);
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
            QuickJsonDecoder qjd = new QuickJsonDecoder(jsonout);
            Object decode = qjd.Decode();
            Check.That(decode).IsNotNull();
            Check.That(QuickJsonDecoder.ToString(decode, true)).IsEqualTo(jsonout);
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
    }
}