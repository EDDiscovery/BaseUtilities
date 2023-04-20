﻿/*
 * Copyright © 2016 EDDiscovery development team
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousCore
{
    public enum EDStar
    {
        Unknown = 0,
        O = 1,
        B,
        A,
        F,
        G,
        K,
        M,

        // Dwarf
        L,
        T,
        Y,

        // proto stars
        AeBe,
        TTS,


        // wolf rayet
        W,
        WN,
        WNC,
        WC,
        WO,

        // Carbon
        CS,
        C,
        CN,
        CJ,
        CHd,


        MS,  //seen in log
        S,   // seen in log

        // white dwarf
        D,
        DA,
        DAB,
        DAO,
        DAZ,
        DAV,
        DB,
        DBZ,
        DBV,
        DO,
        DOV,
        DQ,
        DC,
        DCV,
        DX,

        N,   // Neutron

        H,   // Black Hole

        X,    // currently speculative, not confirmed with actual data... in journal

        A_BlueWhiteSuperGiant,
        F_WhiteSuperGiant,
        M_RedSuperGiant,
        M_RedGiant,
        K_OrangeGiant,
        RoguePlanet,
        Nebula,
        StellarRemnantNebula,
        SuperMassiveBlackHole,
        B_BlueWhiteSuperGiant,
        G_WhiteSuperGiant,
    };

    public enum EDPlanet
    {
        Unknown_Body_Type = 0,
        Metal_rich_body = 1000,     // no idea why it does this, but keeping it
        High_metal_content_body,
        Rocky_body,
        Icy_body,
        Rocky_ice_body,
        Earthlike_body,
        Water_world,
        Ammonia_world,
        Water_giant,
        Water_giant_with_life,
        Gas_giant_with_water_based_life,
        Gas_giant_with_ammonia_based_life,
        Sudarsky_class_I_gas_giant,
        Sudarsky_class_II_gas_giant,
        Sudarsky_class_III_gas_giant,
        Sudarsky_class_IV_gas_giant,
        Sudarsky_class_V_gas_giant,
        Helium_rich_gas_giant,
        Helium_gas_giant,
    }

    [Flags]
    public enum EDAtmosphereProperty
    {
        None = 0,
        Rich = 1,
        Thick = 2,
        Thin = 4,
        Hot = 8,
    }

    public enum EDAtmosphereType   // from the journal
    {
        Earth_Like = 900,
        Ammonia = 1000,
        Water = 2000,
        Carbon_dioxide = 3000,
        Methane = 4000,
        Helium = 5000,
        Argon = 6000,
        Neon = 7000,
        Sulphur_dioxide = 8000,
        Nitrogen = 9000,
        Silicate_vapour = 10000,
        Metallic_vapour = 11000,
        Oxygen = 12000,

        Unknown = 0,
        No_atmosphere = 1,                        
    }


    [Flags]
    public enum EDVolcanismProperty
    {
        None = 0,
        Minor = 1,
        Major = 2,
    }

    public enum EDVolcanism
    {
        Unknown = 0,
        None,
        Water_Magma = 100,
        Sulphur_Dioxide_Magma = 200,
        Ammonia_Magma = 300,
        Methane_Magma = 400,
        Nitrogen_Magma = 500,
        Silicate_Magma = 600,
        Metallic_Magma = 700,
        Water_Geysers = 800,
        Carbon_Dioxide_Geysers = 900,
        Ammonia_Geysers = 1000,
        Methane_Geysers = 1100,
        Nitrogen_Geysers = 1200,
        Helium_Geysers = 1300,
        Silicate_Vapour_Geysers = 1400,
        Rocky_Magma = 1500,
    }

    public enum EDReserve
    {
        None = 0,
        Depleted,
        Low,
        Common,
        Major,
        Pristine,
    }

    public class Bodies
    {
        private static Dictionary<string, EDStar> StarStr2EnumLookup = null;

        private static Dictionary<string, EDPlanet> PlanetStr2EnumLookup = null;

        private static Dictionary<EDAtmosphereType, string> Atmoscomparestrings = null;

        private static Dictionary<string, EDVolcanism> VolcanismStr2EnumLookup = null;

        private static Dictionary<string, EDReserve> ReserveStr2EnumLookup = null;

        public static void Prepopulate()
        {
            StarStr2EnumLookup = new Dictionary<string, EDStar>(StringComparer.InvariantCultureIgnoreCase);
            PlanetStr2EnumLookup = new Dictionary<string, EDPlanet>(StringComparer.InvariantCultureIgnoreCase);
            Atmoscomparestrings = new Dictionary<EDAtmosphereType, string>();
            VolcanismStr2EnumLookup = new Dictionary<string, EDVolcanism>(StringComparer.InvariantCultureIgnoreCase);
            ReserveStr2EnumLookup = new Dictionary<string, EDReserve>(StringComparer.InvariantCultureIgnoreCase);

            foreach (EDStar atm in Enum.GetValues(typeof(EDStar)))
            {
                StarStr2EnumLookup[atm.ToString().Replace("_", "")] = atm;
            }
            foreach (EDPlanet atm in Enum.GetValues(typeof(EDPlanet)))
            {
                PlanetStr2EnumLookup[atm.ToString().Replace("_", "")] = atm;
            }
            foreach (EDAtmosphereType atm in Enum.GetValues(typeof(EDAtmosphereType)))
            {
                Atmoscomparestrings[atm] = atm.ToString().ToLowerInvariant().Replace("_", " ");
            }
            foreach (EDVolcanism atm in Enum.GetValues(typeof(EDVolcanism)))
            {
                VolcanismStr2EnumLookup[atm.ToString().Replace("_", "")] = atm;
            }
            foreach (EDReserve atm in Enum.GetValues(typeof(EDReserve)))
            {
                ReserveStr2EnumLookup[atm.ToString().Replace("_", "")] = atm;
            }
        }

        public static EDStar StarStr2Enum(string star)
        {
            if (star.IsEmpty())
                return EDStar.Unknown;

            var searchstr = star.Replace("_", "").Replace(" ", "").Replace("-", "").ToLowerInvariant();

            if (StarStr2EnumLookup.ContainsKey(searchstr))
                return StarStr2EnumLookup[searchstr];

            return EDStar.Unknown;
        }

        public static EDPlanet PlanetStr2Enum(string planet)
        {
            if (planet.IsEmpty())
                return EDPlanet.Unknown_Body_Type;

            var searchstr = planet.Replace("_", "").Replace(" ", "").Replace("-", "").ToLowerInvariant();

            if (PlanetStr2EnumLookup.ContainsKey(searchstr))
                return PlanetStr2EnumLookup[searchstr];

            return EDPlanet.Unknown_Body_Type;
        }

        public static EDAtmosphereType AtmosphereStr2Enum(string v, out EDAtmosphereProperty atmprop)
        {
            atmprop = EDAtmosphereProperty.None;

            if (v.IsEmpty())
                return EDAtmosphereType.Unknown;

            if (v.Equals("None", StringComparison.InvariantCultureIgnoreCase))
                return EDAtmosphereType.No_atmosphere;

            var searchstr = v.ToLowerInvariant();

            if (searchstr.Contains("rich"))
            {
                atmprop |= EDAtmosphereProperty.Rich;
            }
            if (searchstr.Contains("thick"))
            {
                atmprop |= EDAtmosphereProperty.Thick;
            }
            if (searchstr.Contains("thin"))
            {
                atmprop |= EDAtmosphereProperty.Thin;
            }
            if (searchstr.Contains("hot"))
            {
                atmprop |= EDAtmosphereProperty.Hot;
            }

            foreach( var kvp in Atmoscomparestrings)
            {
                if (searchstr.Contains(kvp.Value))     // both are lower case, does it contain it?
                    return kvp.Key;
            }

            return EDAtmosphereType.Unknown;
        }

        public static EDVolcanism VolcanismStr2Enum(string v, out EDVolcanismProperty vprop )
        {
            vprop = EDVolcanismProperty.None;

            if (v.IsEmpty())
                return EDVolcanism.Unknown;

            string searchstr = v.ToLowerInvariant().Replace("_", "").Replace(" ", "").Replace("-", "").Replace("volcanism", "");

            if (searchstr.Contains("minor"))
            {
                vprop |= EDVolcanismProperty.Minor;
                searchstr = searchstr.Replace("minor", "");
            }
            if (searchstr.Contains("major"))
            {
                vprop |= EDVolcanismProperty.Major;
                searchstr = searchstr.Replace("major", "");
            }

            if (VolcanismStr2EnumLookup.ContainsKey(searchstr))
                return VolcanismStr2EnumLookup[searchstr];

            return EDVolcanism.Unknown;
        }

        public static EDReserve ReserveStr2Enum(string star)
        {
            if (star.IsEmpty())
                return EDReserve.None;


            var searchstr = star.Replace("_", "").Replace(" ", "").Replace("-", "").ToLowerInvariant();

            if (ReserveStr2EnumLookup.ContainsKey(searchstr))
                return ReserveStr2EnumLookup[searchstr];

            return EDReserve.None;
        }

        //public static string StarName( EDStar id )
        //{
        //    switch (id)       // see journal, section 11.2
        //    {
        //        case EDStar.O:
        //            return string.Format("Luminous Hot {0} class star".T(EDCTx.Bodies_HMS), id.ToString());

        //        case EDStar.B:
        //            // also have an B1V
        //            return string.Format("Luminous Blue {0} class star".T(EDCTx.Bodies_BMS), id.ToString());

        //        case EDStar.A:
        //            // also have an A3V..
        //            return string.Format("Bluish-White {0} class star".T(EDCTx.Bodies_BWMS), id.ToString());

        //        case EDStar.F:
        //            return string.Format("White {0} class star".T(EDCTx.Bodies_WMS), id.ToString());

        //        case EDStar.G:
        //            // also have a G8V
        //            return string.Format("Yellow {0} class star".T(EDCTx.Bodies_YMS), id.ToString());

        //        case EDStar.K:
        //            // also have a K0V
        //            return string.Format("Orange {0} class star".T(EDCTx.Bodies_OMS), id.ToString());
        //        case EDStar.M:
        //            // also have a M1VA
        //            return string.Format("Red {0} class star".T(EDCTx.Bodies_RMS), id.ToString());

        //        // dwarfs
        //        case EDStar.L:
        //            return string.Format("Dark Red {0} class star".T(EDCTx.Bodies_DRNS), id.ToString());
        //        case EDStar.T:
        //            return string.Format("Methane Dwarf T class star".T(EDCTx.Bodies_MD));
        //        case EDStar.Y:
        //            return string.Format("Brown Dwarf Y class star".T(EDCTx.Bodies_BD));

        //        // proto stars
        //        case EDStar.AeBe:    // Herbig
        //            return "Herbig Ae/Be class star".T(EDCTx.Bodies_Herbig);
        //        case EDStar.TTS:     // seen in logs
        //            return "T Tauri star".T(EDCTx.Bodies_TTauri);

        //        // wolf rayet
        //        case EDStar.W:
        //        case EDStar.WN:
        //        case EDStar.WNC:
        //        case EDStar.WC:
        //        case EDStar.WO:
        //            return string.Format("Wolf-Rayet {0} class star".T(EDCTx.Bodies_WR), id.ToString());

        //        // Carbon
        //        case EDStar.CS:
        //        case EDStar.C:
        //        case EDStar.CN:
        //        case EDStar.CJ:
        //        case EDStar.CHd:
        //            return string.Format("Carbon {0} class star".T(EDCTx.Bodies_C), id.ToString());

        //        case EDStar.MS: //seen in log https://en.wikipedia.org/wiki/S-type_star
        //            return string.Format("Intermediate low Zirconium Monoxide MS class star".T(EDCTx.Bodies_IZ));

        //        case EDStar.S:   // seen in log, data from http://elite-dangerous.wikia.com/wiki/Stars
        //            return string.Format("Cool Giant Zirconium Monoxide rich S class star".T(EDCTx.Bodies_CGZ));

        //        // white dwarf
        //        case EDStar.D:
        //        case EDStar.DA:
        //        case EDStar.DAB:
        //        case EDStar.DAO:
        //        case EDStar.DAZ:
        //        case EDStar.DAV:
        //        case EDStar.DB:
        //        case EDStar.DBZ:
        //        case EDStar.DBV:
        //        case EDStar.DO:
        //        case EDStar.DOV:
        //        case EDStar.DQ:
        //        case EDStar.DC:
        //        case EDStar.DCV:
        //        case EDStar.DX:
        //            return string.Format("White Dwarf {0} class star".T(EDCTx.Bodies_WD), id.ToString());

        //        case EDStar.N:
        //            return "Neutron Star".T(EDCTx.Bodies_NS);

        //        case EDStar.H:

        //            return "Black Hole".T(EDCTx.Bodies_BH);

        //        case EDStar.X:
        //            // currently speculative, not confirmed with actual data... in journal
        //            return "Exotic".T(EDCTx.Bodies_EX);

        //        // Journal.. really?  need evidence these actually are formatted like this.

        //        case EDStar.SuperMassiveBlackHole:
        //            return "Super Massive Black Hole".T(EDCTx.Bodies_SMBH);
        //        case EDStar.A_BlueWhiteSuperGiant:
        //            return "A Blue White Super Giant".T(EDCTx.Bodies_ABSG);
        //        case EDStar.B_BlueWhiteSuperGiant:
        //            return "B Blue White Super Giant".T(EDCTx.Bodies_BBSG);
        //        case EDStar.F_WhiteSuperGiant:
        //            return "F White Super Giant".T(EDCTx.Bodies_FWSG);
        //        case EDStar.G_WhiteSuperGiant:
        //            return "G White Super Giant".T(EDCTx.Bodies_GWSG);
        //        case EDStar.M_RedSuperGiant:
        //            return "M Red Super Giant".T(EDCTx.Bodies_MSR);
        //        case EDStar.M_RedGiant:
        //            return "M Red Giant".T(EDCTx.Bodies_MOG);
        //        case EDStar.K_OrangeGiant:
        //            return "K Orange Giant".T(EDCTx.Bodies_KOG);
        //        case EDStar.Nebula:
        //            return "Nebula".T(EDCTx.Bodies_Nebula);
        //        case EDStar.StellarRemnantNebula:
        //            return "Stellar Remnant Nebula".T(EDCTx.Bodies_StellarRemnantNebula);
        //        case EDStar.RoguePlanet:
        //            return "Rogue Planet".T(EDCTx.Bodies_RP);
        //        case EDStar.Unknown:
        //            return "Unknown star class".T(EDCTx.Bodies_SUnknown);

        //        default:
        //            return string.Format("Class {0} star".T(EDCTx.Bodies_UNK), id.ToString());
        //    }
        //}

        //// These should be translated to match the in-game planet types
        //private static readonly Dictionary<EDPlanet, string> PlanetEnumToNameLookup = new Dictionary<EDPlanet, string>
        //{
        //    [EDPlanet.Metal_rich_body] = "Metal-rich body".T(EDCTx.EDPlanet_Metalrichbody),
        //    [EDPlanet.High_metal_content_body] = "High metal content world".T(EDCTx.EDPlanet_Highmetalcontentbody),
        //    [EDPlanet.Rocky_body] = "Rocky body".T(EDCTx.EDPlanet_Rockybody),
        //    [EDPlanet.Icy_body] = "Icy body".T(EDCTx.EDPlanet_Icybody),
        //    [EDPlanet.Rocky_ice_body] = "Rocky ice world".T(EDCTx.EDPlanet_Rockyicebody),
        //    [EDPlanet.Earthlike_body] = "Earth-like world".T(EDCTx.EDPlanet_Earthlikebody),
        //    [EDPlanet.Water_world] = "Water world".T(EDCTx.EDPlanet_Waterworld),
        //    [EDPlanet.Ammonia_world] = "Ammonia world".T(EDCTx.EDPlanet_Ammoniaworld),
        //    [EDPlanet.Water_giant] = "Water giant".T(EDCTx.EDPlanet_Watergiant),
        //    [EDPlanet.Water_giant_with_life] = "Water giant with life".T(EDCTx.EDPlanet_Watergiantwithlife),
        //    [EDPlanet.Gas_giant_with_water_based_life] = "Gas giant with water-based life".T(EDCTx.EDPlanet_Gasgiantwithwaterbasedlife),
        //    [EDPlanet.Gas_giant_with_ammonia_based_life] = "Gas giant with ammonia-based life".T(EDCTx.EDPlanet_Gasgiantwithammoniabasedlife),
        //    [EDPlanet.Sudarsky_class_I_gas_giant] = "Class I gas giant".T(EDCTx.EDPlanet_SudarskyclassIgasgiant),
        //    [EDPlanet.Sudarsky_class_II_gas_giant] = "Class II gas giant".T(EDCTx.EDPlanet_SudarskyclassIIgasgiant),
        //    [EDPlanet.Sudarsky_class_III_gas_giant] = "Class III gas giant".T(EDCTx.EDPlanet_SudarskyclassIIIgasgiant),
        //    [EDPlanet.Sudarsky_class_IV_gas_giant] = "Class IV gas giant".T(EDCTx.EDPlanet_SudarskyclassIVgasgiant),
        //    [EDPlanet.Sudarsky_class_V_gas_giant] = "Class V gas giant".T(EDCTx.EDPlanet_SudarskyclassVgasgiant),
        //    [EDPlanet.Helium_rich_gas_giant] = "Helium-rich gas giant".T(EDCTx.EDPlanet_Heliumrichgasgiant),
        //    [EDPlanet.Helium_gas_giant] = "Helium gas giant".T(EDCTx.EDPlanet_Heliumgasgiant),
        //    [EDPlanet.Unknown_Body_Type] = "Unknown planet type".T(EDCTx.EDPlanet_Unknown),
        //};

        //public static string PlanetTypeName(EDPlanet type)
        //{
        //    string name;
        //    if (PlanetEnumToNameLookup.TryGetValue(type, out name))
        //    {
        //        return name;
        //    }
        //    else
        //    {
        //        return type.ToString().Replace("_", " ");
        //    }
        //}

        public static bool AmmoniaWorld(EDPlanet PlanetTypeID) { return PlanetTypeID == EDPlanet.Ammonia_world; }
        public static bool Earthlike(EDPlanet PlanetTypeID) { return PlanetTypeID == EDPlanet.Earthlike_body; } 
        public static bool WaterWorld(EDPlanet PlanetTypeID) { return PlanetTypeID == EDPlanet.Water_world; } 
        public static bool SudarskyGasGiant(EDPlanet PlanetTypeID) { return PlanetTypeID >= EDPlanet.Sudarsky_class_I_gas_giant && PlanetTypeID <= EDPlanet.Sudarsky_class_V_gas_giant; }
        public static bool GasGiant(EDPlanet PlanetTypeID) { return PlanetTypeID >= EDPlanet.Gas_giant_with_water_based_life && PlanetTypeID <= EDPlanet.Gas_giant_with_ammonia_based_life; }
        public static bool WaterGiant(EDPlanet PlanetTypeID) { return PlanetTypeID >= EDPlanet.Water_giant && PlanetTypeID <= EDPlanet.Water_giant_with_life; }
        public static bool HeliumGasGiant(EDPlanet PlanetTypeID) { return PlanetTypeID >= EDPlanet.Helium_rich_gas_giant && PlanetTypeID <= EDPlanet.Helium_gas_giant; }
        public static bool GasWorld(EDPlanet PlanetTypeID) { return PlanetTypeID >= EDPlanet.Gas_giant_with_water_based_life && PlanetTypeID <= EDPlanet.Helium_gas_giant; }

        public static string SudarskyClass(EDPlanet PlanetTypeID) { return (new string[] { "I", "II", "III", "IV", "V" })[(int)(PlanetTypeID - EDPlanet.Sudarsky_class_I_gas_giant)]; }

        private static string[] ClassificationAbv = new string[]
        {
            "MR","HMC","R","I","R+I","E","W","A","WG","WGL","GWL","GAL","S-I","S-II","S-III","S-IV","S-V","HRG","HG"
        };

        public static string PlanetAbv(EDPlanet PlanetTypeID)
        {
            if (PlanetTypeID == EDPlanet.Unknown_Body_Type)
                return "U";
            else
                return ClassificationAbv[(int)PlanetTypeID - (int)EDPlanet.Metal_rich_body];
        }
    }

    public class BodyPhysicalConstants
    {
        // stellar references
        public const double oneSolRadius_m = 695700000; // 695,700km

        // planetary bodies
        public const double oneEarthRadius_m = 6371000;
        public const double oneAtmosphere_Pa = 101325;
        public const double oneGee_m_s2 = 9.80665;
        public const double oneSol_KG = 1.989e30;
        public const double oneEarth_KG = 5.972e24;
        public const double oneMoon_KG = 7.34767309e22;
        public const double oneEarthMoonMassRatio = oneEarth_KG / oneMoon_KG;

        // astrometric
        public const double oneLS_m = 299792458;
        public const double oneAU_m = 149597870700;
        public const double oneAU_LS = oneAU_m / oneLS_m;
        public const double oneDay_s = 86400;
    }

}
