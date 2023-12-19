/*
 * Copyright 2015 - 2023 EDDiscovery development team
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
 */

using QuickJSON;
using System;
using System.Collections.Generic;

namespace EliteDangerousCore.DB
{
    public partial class SystemsDB
    {
        // read JSON and fill in a Star File entry

        public class StarFileEntry
        {
            public bool Deserialize(IEnumerator<JToken> enumerator)
            {
                bool spansh = false;

                while (enumerator.MoveNext() && enumerator.Current.IsProperty)   // while more tokens, and JProperty
                {
                    var p = enumerator.Current;
                    string field = p.Name;

                    switch (field)
                    {
                        case "name":
                            name = p.StrNull();
                            break;
                        case "id":      // EDSM name
                            id = p.ULong();
                            break;
                        case "id64":      // EDSM and Spansh name. 
                            systemaddress = p.ULong();
                            break;
                        case "date":        // edsm name
                            date = p.DateTimeUTC();
                            spansh = false;
                            break;
                        case "updateTime":        // Spansh name
                            date = p.DateTimeUTC();
                            spansh = true;
                            break;
                        case "coords":
                            {
                                while (enumerator.MoveNext() && enumerator.Current.IsProperty)   // while more tokens, and JProperty
                                {
                                    var cp = enumerator.Current;
                                    field = cp.Name;
                                    double? v = cp.DoubleNull();
                                    if (v == null)
                                        return false;
                                    int vi = (int)(v * SystemClass.XYZScalar);

                                    switch (field)
                                    {
                                        case "x":
                                            x = vi;
                                            break;
                                        case "y":
                                            y = vi;
                                            break;
                                        case "z":
                                            z = vi;
                                            break;
                                    }
                                }

                                break;
                            }

                        case "mainStar":    // spansh
                            {
                                spansh = true;
                                string name = p.Str();
                                if (spanshtoedstar.TryGetValue(name, out EDStar value))
                                {
                                    startype = (int)value;
                                }
                                else
                                    System.Diagnostics.Debug.WriteLine($"DB read of spansh unknown star type {name}");
                                break;
                            }
                        default:        // any other, ignore
                            break;
                    }
                }

                if (spansh )                    // if detected spansh above
                {
                    if ( startype == null)      // for spansh, we always set startype non null, so that we know the db.edsmid is really the system address
                        startype = (int)EDStar.Unknown;
                    id = systemaddress;         // for spansh, the id
                }

                return id != ulong.MaxValue && name.HasChars() && x != int.MinValue && y != int.MinValue && z != int.MinValue && date != DateTime.MinValue;
            }

            public ulong id = ulong.MaxValue;                       //ID to use, either edsmid or system address
            public string name;
            public int x = int.MinValue;
            public int y = int.MinValue;
            public int z = int.MinValue;
            public int? startype;       // null default
            public DateTime date;

            private ulong systemaddress = ulong.MaxValue;

            // from https://spansh.co.uk/api/bodies/field_values/subtype
            static private Dictionary<string, EDStar> spanshtoedstar = new Dictionary<string, EDStar>
            {
                { "O (Blue-White) Star", EDStar.O },
                { "B (Blue-White) Star", EDStar.B },
                { "A (Blue-White) Star", EDStar.A },
                { "F (White) Star", EDStar.F },
                { "G (White-Yellow) Star", EDStar.G },
                { "K (Yellow-Orange) Star", EDStar.K },
                { "M (Red dwarf) Star", EDStar.M },

                { "L (Brown dwarf) Star", EDStar.L },
                { "T (Brown dwarf) Star", EDStar.T },
                { "Y (Brown dwarf) Star", EDStar.Y },

                { "Herbig Ae Be Star", EDStar.AeBe },
                { "T Tauri Star", EDStar.TTS },

                { "Wolf-Rayet Star", EDStar.W },
                { "Wolf-Rayet N Star", EDStar.WN },
                { "Wolf-Rayet NC Star", EDStar.WNC },
                { "Wolf-Rayet C Star", EDStar.WC },
                { "Wolf-Rayet O Star", EDStar.WO },

                // missing CS
                { "C Star", EDStar.C },
                { "CN Star", EDStar.CN },
                { "CJ Star", EDStar.CJ },
                // missing CHd

                { "MS-type Star", EDStar.MS },
                { "S-type Star", EDStar.S },

                { "White Dwarf (D) Star", EDStar.D },
                { "White Dwarf (DA) Star", EDStar.DA },
                { "White Dwarf (DAB) Star", EDStar.DAB },
                // missing DAO
                { "White Dwarf (DAZ) Star", EDStar.DAZ },
                { "White Dwarf (DAV) Star", EDStar.DAV },
                { "White Dwarf (DB) Star", EDStar.DB },
                { "White Dwarf (DBZ) Star", EDStar.DBZ },
                { "White Dwarf (DBV) Star", EDStar.DBV },
                // missing DO,DOV
                { "White Dwarf (DQ) Star", EDStar.DQ },
                { "White Dwarf (DC) Star", EDStar.DC },
                { "White Dwarf (DCV) Star", EDStar.DCV },
                // missing DX
                { "Neutron Star", EDStar.N },
                { "Black Hole", EDStar.H },
                // missing X but not confirmed with actual journal data


                { "A (Blue-White super giant) Star", EDStar.A_BlueWhiteSuperGiant },
                { "F (White super giant) Star", EDStar.F_WhiteSuperGiant },
                { "M (Red super giant) Star", EDStar.M_RedSuperGiant },
                { "M (Red giant) Star", EDStar.M_RedGiant},
                { "K (Yellow-Orange giant) Star", EDStar.K_OrangeGiant },
                // missing rogueplanet, nebula, stellarremanant
                { "Supermassive Black Hole", EDStar.SuperMassiveBlackHole },
                { "B (Blue-White super giant) Star", EDStar.B_BlueWhiteSuperGiant },
                { "G (White-Yellow super giant) Star", EDStar.G_WhiteSuperGiant },
            };

        }
    }
}


