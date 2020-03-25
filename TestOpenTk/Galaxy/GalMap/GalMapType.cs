/*
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
using System.Drawing;
using System.Linq;
using System.Text;
using EliteDangerousCore;

namespace EliteDangerousCore.EDSM
{
    public enum GalMapTypeEnum
    {
        EDSMUnknown,
        historicalLocation,
        nebula,
        planetaryNebula,
        stellarRemnant,
        blackHole,
        starCluster,
        pulsar,
        minorPOI,
        beacon,
        surfacePOI,
        cometaryBody,
        jumponiumRichSystem,
        planetFeatures,
        deepSpaceOutpost,
        mysteryPOI,
        restrictedSectors,
    }

    public class GalMapType
    {
        public enum GalMapGroup
        {
            Markers = 1,
            Routes,
            Regions,
            Quadrants,
        }

        public string Typeid;
        public string Description;
        public Image Image;
        public GalMapGroup Group;
        public bool Enabled;
        public int Index;

        public GalMapType(string id, string desc, GalMapGroup g, Image b, int i)
        {
            Typeid = id;
            Description = desc;
            Group = g;
            Image = b;
            Enabled = false;
            Index = i;
        }

        static public List<GalMapType> GetTypes()
        {
            List<GalMapType> type = new List<GalMapType>();

            int index = 0;

            type.Add(new GalMapType("historicalLocation", "η Historical Location", GalMapGroup.Markers, TestOpenTk.Properties.Resources.historicalLocation, index++));
            type.Add(new GalMapType("nebula", "α Nebula", GalMapGroup.Markers, TestOpenTk.Properties.Resources.nebula, index++));
            type.Add(new GalMapType("planetaryNebula", "β Planetary Nebula", GalMapGroup.Markers, TestOpenTk.Properties.Resources.planetaryNebula, index++));
            type.Add(new GalMapType("stellarRemnant", "γ Stellar Features", GalMapGroup.Markers, TestOpenTk.Properties.Resources.stellarRemnant, index++));
            type.Add(new GalMapType("blackHole", "δ Black Hole", GalMapGroup.Markers, TestOpenTk.Properties.Resources.blackHole, index++));
            type.Add(new GalMapType("starCluster", "σ Star Cluster", GalMapGroup.Markers, TestOpenTk.Properties.Resources.starCluster, index++));
            type.Add(new GalMapType("pulsar", "ζ Pulsar", GalMapGroup.Markers , TestOpenTk.Properties.Resources.pulsar, index++));
            type.Add(new GalMapType("minorPOI", "★ Minor POI or Star", GalMapGroup.Markers , TestOpenTk.Properties.Resources.minorPOI, index++));
            type.Add(new GalMapType("beacon", "⛛ Beacon", GalMapGroup.Markers , TestOpenTk.Properties.Resources.beacon, index++));
            type.Add(new GalMapType("surfacePOI", "∅ Surface POI", GalMapGroup.Markers , TestOpenTk.Properties.Resources.surfacePOI, index++));
            type.Add(new GalMapType("cometaryBody", "☄ Cometary Body", GalMapGroup.Markers , TestOpenTk.Properties.Resources.cometaryBody, index++));
            type.Add(new GalMapType("jumponiumRichSystem", "☢ Jumponium-Rich System", GalMapGroup.Markers, TestOpenTk.Properties.Resources.jumponiumRichSystem, index++));
            type.Add(new GalMapType("planetFeatures", "∅ Planetary Features", GalMapGroup.Markers, TestOpenTk.Properties.Resources.planetFeatures, index++));
            type.Add(new GalMapType("deepSpaceOutpost", "Deep space outpost", GalMapGroup.Markers, TestOpenTk.Properties.Resources.deepSpaceOutpost, index++));
            type.Add(new GalMapType("mysteryPOI", "Mystery POI", GalMapGroup.Markers, TestOpenTk.Properties.Resources.mysteryPOI, index++));
            type.Add(new GalMapType("restrictedSectors", "Restricted Sectors", GalMapGroup.Markers, TestOpenTk.Properties.Resources.restrictedSectors, index++));
     
            type.Add(new GalMapType("travelRoute", "Travel Route", GalMapGroup.Routes , null, index++));
            type.Add(new GalMapType("historicalRoute", "Historical Route", GalMapGroup.Routes , null, index++));
            type.Add(new GalMapType("minorRoute", "Minor Route", GalMapGroup.Routes, null, index++));
            type.Add(new GalMapType("neutronRoute", "Neutron highway", GalMapGroup.Routes, null, index++));

            type.Add(new GalMapType("region", "Region", GalMapGroup.Regions, null, index++));
            type.Add(new GalMapType("regionQuadrants", "Galactic Quadrants", GalMapGroup.Quadrants , null, index++));

            type.Add(new GalMapType("regional", "Regional Marker", GalMapGroup.Markers, TestOpenTk.Properties.Resources.minorPOI, index++));
            type.Add(new GalMapType("geyserPOI", "Geyser", GalMapGroup.Markers, TestOpenTk.Properties.Resources.minorPOI, index++));
            type.Add(new GalMapType("organicPOI", "Organic Material", GalMapGroup.Markers, TestOpenTk.Properties.Resources.minorPOI, index++));

            type.Add(new GalMapType("EDSMUnknown", "EDSM other POI type", GalMapGroup.Markers, TestOpenTk.Properties.Resources.EDSMUnknown, index++));

            return type;
        }
    }
}