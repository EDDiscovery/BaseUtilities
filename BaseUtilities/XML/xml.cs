/*
 * Copyright © 2016 EDDiscovery development team + Robbyxp1 @ github.com
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
 * 
 */
using System;
using System.IO;
using System.Xml;

namespace BaseUtils
{
    public static class XML
    {
        // reads the xml, and tries to extract all text elements from it

        public static string XMLtoText(this string xml)
        {
            try
            {
                XmlReader reader = XmlReader.Create(new StringReader(xml));
                string text = "";
                string paratext = "";
                bool inlist = false;
                while (reader.Read())
                {
                    //System.Diagnostics.Debug.WriteLine($"Read: {reader.NodeType} {reader.Name} {reader.Value}");
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.EndElement:
                            if (reader.Name == "p")
                            {
                                paratext = paratext.WordWrap(120, true);
                                text = text.AppendPrePad(paratext, Environment.NewLine) + Environment.NewLine;
                                paratext = "";
                            }
                            else if (reader.Name == "ul")
                            {
                                inlist = false;
                                text += paratext;
                                paratext = "";
                            }
                            break;

                        case XmlNodeType.Element:
                            if (reader.Name == "a")
                            {
                                if (reader.HasAttributes)
                                {
                                    while (reader.MoveToNextAttribute())
                                    {
                                        if (reader.Name == "href")
                                        {
                                            paratext += " " + reader.Value + " ";
                                        }
                                    }
                                }
                            }
                            else if (reader.Name == "img")
                            {
                                if (reader.HasAttributes)
                                {
                                    while (reader.MoveToNextAttribute())
                                    {
                                        if (reader.Name == "src")
                                        {
                                            paratext += " " + reader.Value + " ";
                                        }
                                    }
                                }
                            }
                            else if (reader.Name == "ul")
                                inlist = true;
                            break;
                        case XmlNodeType.Text:
                            paratext += reader.Value;
                            if (inlist)
                                paratext += Environment.NewLine;
                            break;
                    }
                }

                return text;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"xml parse failed {ex}");
                return null;
            }

        }
    }
}
