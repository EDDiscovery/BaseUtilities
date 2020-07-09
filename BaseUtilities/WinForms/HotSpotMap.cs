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
using System.Drawing;

namespace BaseUtils
{
    public class HotSpotMap
    {
        public Action<Object, PointF> OnHotSpot = null;        // call back when a hotspot is detected by Check, may be null

        /// Create a map of hotspots regions

        public void CalculateHotSpotRegions(List<Tuple<Object,PointF>> plotHotSpot, int HotSpotRadius = 10)
        {
            hotSpotZones.Clear();

            foreach (var hotSpot in plotHotSpot)
            {
                hotSpotZones.Add(hotSpot, new float[]
                {
                hotSpot.Item2.X,
                hotSpot.Item2.Y,
                hotSpot.Item2.X - HotSpotRadius,
                hotSpot.Item2.X + HotSpotRadius,
                hotSpot.Item2.Y - HotSpotRadius,
                hotSpot.Item2.Y + HotSpotRadius
                });
            }
        }

        /// Check if the mouse pointer is inside an hotspot region.
        /// If so, fire the event!.  Return point found, or null
        /// 
        public Tuple<Object,PointF> CheckForMouseInHotSpot(Point mousePosition)
        {
            if (hotSpotZones != null)
            {
                foreach (var item in hotSpotZones)
                {
                    if (mousePosition.X > item.Value[2] && mousePosition.X < item.Value[3] &&
                        mousePosition.Y > item.Value[4] && mousePosition.Y < item.Value[5])
                    {
                        OnHotSpot?.Invoke(item.Key, new PointF(item.Value[0], item.Value[1]));
                        return item.Key;
                    }
                }
            }

            return null;
        }

        private readonly Dictionary<Tuple<Object, PointF>, float[]> hotSpotZones = new Dictionary<Tuple<Object, PointF>, float[]>();
    }
}
