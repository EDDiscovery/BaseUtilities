/*
 * Copyright © 2020 EDDiscovery development team
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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;

namespace BaseUtils
{
    static public class SVG                                            
    {
        static public List<PointF> ReadSVGPath(string s)              // simple L,M,Z path only
        {
            StringParser sp = new StringParser(s);
            List<PointF> points = new List<PointF>();

            while (!sp.IsEOL)
            {
                char ctrl = sp.GetChar(true);
                ctrl = char.ToLower(ctrl);
                if (ctrl == 'z')
                {
                    if (points.Count > 0)
                    {
                        points.Add(points[0]);
                    }
                    else
                        return null;
                }
                else if (ctrl == 'm' || ctrl == 'l')
                {
                    if ((ctrl == 'm' && points.Count == 0) || (ctrl == 'l'))
                    {
                        double? x = sp.NextDoubleComma(", ");
                        double? y = sp.NextDouble();

                        if (x.HasValue && y.HasValue)
                        {
                            points.Add(new PointF((float)x, (float)y));
                        }
                        else
                            return null;
                    }

                }
                else
                    return null;
            }

            return points;
        }
    }
}
