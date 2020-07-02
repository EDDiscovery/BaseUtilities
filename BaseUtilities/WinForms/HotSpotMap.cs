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
        private Dictionary<string, double[]> _hotSpots = new Dictionary<string, double[]>();

        public void CalculateHotSpotRegions(List<object[]> plotHotSpot, double HotSpotRadius)
        {
            _hotSpots.Clear();

            foreach (var hotSpot in plotHotSpot)
            {
                _hotSpots.Add(hotSpot[0].ToString(), new double[]
                {
                    (double)hotSpot[1] - HotSpotRadius,
                    (double)hotSpot[1] + HotSpotRadius,
                    (double)hotSpot[2] - HotSpotRadius,
                    (double)hotSpot[2] + HotSpotRadius
                });
            }
        }

        public string CheckForMouseInHotSpot(Point mousePosition)
        {
            var hotSpotName = "";

            if (_hotSpots != null)
            {
                foreach (KeyValuePair<string, double[]> item in _hotSpots)
                {
                    if (mousePosition.X > item.Value[0] && mousePosition.X < item.Value[1] &&
                        mousePosition.Y > item.Value[2] && mousePosition.Y < item.Value[3])
                    {
                        hotSpotName = item.Key;
                        Debug.WriteLine(hotSpotName);
                    }
                }
            }

            //Debug.WriteLine("I'm running!");

            return hotSpotName;
        }
    }
}
