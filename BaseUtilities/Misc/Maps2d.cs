/*
 * Copyright © 2015 - 2020 EDDiscovery development team
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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace BaseUtils
{
    public class Map2d : IDisposable
    {
        public string FilePath { get; private set; }
        public string FileName { get { return Path.GetFileNameWithoutExtension(FilePath); } }

        public Point TopLeft, TopRight, BottomLeft, BottomRight; // galaxy loc
        public double Area;

        public Point pxTopLeft, pxTopRight, pxBottomLeft, pxBottomRight;        // bitmap

        public int PixelWidth { get { return pxTopRight.X - pxTopLeft.X; } }
        public int PixelHeight { get { return pxBottomRight.Y - pxTopRight.Y; } }

        public int LYWidth { get { return BottomRight.X - BottomLeft.X; } }
        public int LYHeight { get { return (TopLeft.Y - BottomLeft.Y); } }

        public Image Image { get          // MAY except
            {
                if (img == null)
                {
                    img = new Bitmap(FilePath);
                    own = true;
                }
                return img;
            } }

        private Image img = null;       // loaded only on reference

        private bool own = false;       // do we own the bitmap

        public Map2d()
        {
        }

        public Map2d(string name, string json, Image bmp)
        {
            LoadImage(name, json, bmp);
        }

        public void Dispose()
        {
            if (img != null)
            {
                if ( own )
                    img.Dispose();
                img = null;
            }
        }

        public Point LYPos(Point p)     // p is pixel.. (0,0) = top of map
        {
            return new Point(p.X * LYWidth / PixelWidth + TopLeft.X, TopRight.Y - p.Y * LYHeight / PixelHeight);
        }

        public Point TransformCoordinate(Point coordinately)    // to point screen
        {
            int diffx1, diffx2, diffy1, diffy2;
            int diffpx1, diffpx2, diffpy1, diffpy2;

            //Transform trans;

            diffx1 = TopRight.X - TopLeft.X;
            diffx2 = BottomRight.X - BottomLeft.X;
            diffy1 = TopLeft.Y - BottomLeft.Y;
            diffy2 = TopRight.Y - BottomRight.Y;

            diffpx1 = pxTopRight.X - pxTopLeft.X;
            diffpx2 = pxBottomRight.X - pxBottomLeft.X;
            diffpy1 = pxTopLeft.Y - pxBottomLeft.Y;
            diffpy2 = pxTopRight.Y - pxBottomRight.Y;

            double dx1, dx2, dy1, dy2;

            dx1 = diffpx1 / (double)diffx1;
            dx2 = diffpx2 / (double)diffx2;
            dy1 = diffpy1 / (double)diffy1;
            dy2 = diffpy2 / (double)diffy2;

            Point newPoint = new Point(coordinately.X - BottomLeft.X, coordinately.Y - BottomLeft.Y);

            // Calculate dx and dy for point;
            double dx, dy;

            dx = dx2 + newPoint.Y / (double)diffy1 * (dx1 - dx2);
            dy = dy2 + newPoint.X / (double)diffx1 * (dy1 - dy2);


            int x, y;

            x = (int)(newPoint.X * dx + pxBottomLeft.X + newPoint.Y / (double)diffy1 * (pxTopLeft.X - pxBottomLeft.X));
            y = (int)(newPoint.Y * dy + pxBottomLeft.Y + newPoint.X / (double)diffx1 * (pxTopRight.Y - pxTopLeft.Y));

            return new Point(x, y);
        }

        public void LoadImage(string filepath, string json, Image def, bool own = false)
        {
            FilePath = filepath;

            var jo = (JObject)JObject.Parse(json);

            TopLeft = new Point(jo["x1"].Int(), jo["y1"].Int());
            pxTopLeft = new Point(jo["px1"].Int(), jo["py1"].Int());

            TopRight = new Point(jo["x2"].Int(), jo["y1"].Int());
            pxTopRight = new Point(jo["px2"].Int(), jo["py1"].Int());

            BottomLeft = new Point(jo["x1"].Int(), jo["y2"].Int());
            pxBottomLeft = new Point(jo["px1"].Int(), jo["py2"].Int());

            BottomRight = new Point(jo["x2"].Int(), jo["y2"].Int());
            pxBottomRight = new Point(jo["px2"].Int(), jo["py2"].Int());

            Area = (double)(TopRight.X - TopLeft.X) * (double)(TopLeft.Y - BottomRight.Y);

            img = def;
            this.own = own;
        }

        public bool LoadImage(string jsonpath)
        {
            string imagename = File.Exists(jsonpath.Replace(".json", ".png")) ? jsonpath.Replace(".json", ".png") : jsonpath.Replace(".json", ".jpg");

            if (File.Exists(imagename))
            {
                try
                {
                    LoadImage(imagename, File.ReadAllText(jsonpath), null);      // lazy load this one.  We own it
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Map exception on load " + ex);
                }
            }

            return false;
        }

        public static List<Map2d> LoadFromFolder(string datapath)
        {
            List<Map2d> maps = new List<Map2d>();

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(datapath);
                FileInfo[] allFiles = dirInfo.GetFiles("*.json");

                if (allFiles != null)
                {
                    foreach (FileInfo fi in allFiles)
                    {
                        Map2d map = new Map2d();
                        if (map.LoadImage(fi.FullName))
                            maps.Add(map);
                    }

                    maps.Sort(delegate (Map2d p1, Map2d p2)      // biggest first.. name if same.. 
                    {
                        if (p1.Area == p2.Area)
                            return p1.FileName.CompareTo(p2.FileName);
                        else if (p1.Area < p2.Area)
                            return 1;
                        else
                            return -1;
                    }
                    );
                }
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine("Map exception on load " + ex);
            }

            return maps;
        }
    }
}
