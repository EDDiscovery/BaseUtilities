/*
 * Copyright © 2016 - 2020 EDDiscovery development team
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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BaseUtils
{
    public class HotSpotMap
    {
        private string hotSpotName = "";
        private Point hotSpotLocation;

        private readonly Dictionary<string, double[]> hotSpotZones = new Dictionary<string, double[]>();

        /// <summary>
        /// Create a map of hotspots regions
        /// </summary>
        /// <param name="plotHotSpot"></param>
        /// <param name="HotSpotRadius"></param>
        public void CalculateHotSpotRegions(List<object[]> plotHotSpot, double HotSpotRadius = 10)
        {
            hotSpotZones.Clear();

            foreach (var hotSpot in plotHotSpot)
            {
                hotSpotZones.Add(hotSpot[0].ToString(), new double[]
                {
                    (double)hotSpot[1],
                    (double)hotSpot[2],
                    (double)hotSpot[1] - HotSpotRadius,
                    (double)hotSpot[1] + HotSpotRadius,
                    (double)hotSpot[2] - HotSpotRadius,
                    (double)hotSpot[2] + HotSpotRadius
                });
            }
        }
                
        /// <summary>
        /// Define a delegate and an event
        /// </summary>
        public delegate void OnMouseHower();
        public event OnMouseHower OnHotSpot;

        /// <summary>
        /// Check if the mouse pointer is inside an hotspot region.
        /// If so, fire the event!
        /// </summary>
        /// <param name="mousePosition"></param>
        public void CheckForMouseInHotSpot(Point mousePosition)
        {
            hotSpotName = "";

            if (hotSpotZones != null)
            {
                foreach (KeyValuePair<string, double[]> item in hotSpotZones)
                {
                    if (mousePosition.X > item.Value[2] && mousePosition.X < item.Value[3] &&
                        mousePosition.Y > item.Value[4] && mousePosition.Y < item.Value[5])
                    {
                        hotSpotName = item.Key;
                        hotSpotLocation = new Point((int)item.Value[1], (int)item.Value[2]);
                        //Debug.WriteLine(hotSpotName);
                        { OnHotSpot(); } // fire the event!
                    }
                }
            }
        }

        /// <summary>
        /// Get the name related to the hovered hotspot
        /// </summary>
        /// <returns>hotSpotName</returns>
        public string GetHotSpotName()
        {
            return hotSpotName;
        }

        /// <summary>
        /// Get the location of the center of the hovered hotspot
        /// </summary>
        /// <returns>hotSpotLocation</returns>
        public Point GetHotSpotLocation()
        {
            return hotSpotLocation;
        }
    }
}
