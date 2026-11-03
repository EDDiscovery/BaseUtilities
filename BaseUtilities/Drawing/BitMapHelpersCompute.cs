/*
 * Copyright 2016-2025 EDDiscovery development team
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

using System;
using System.Collections.Generic;
using System.Drawing;

namespace BaseUtils
{
    public static partial class BitMapHelpers
    {
        // return the image intensity of the central region of an image under GDI lock
        public static float CentralImageIntensity(this Image source)
        {
            lock (bitmapgdilock)
            {
                if (imageintensities.TryGetValue(source, out float value))
                    return value;
                else
                {
                    float ii = ((Bitmap)source).Function(BitMapHelpers.BitmapFunction.Brightness, source.Width * 3 / 8, source.Height * 3 / 8, source.Width * 2 / 8, source.Height * 2 / 8).Item2;
                    imageintensities[source] = ii;
                    return ii;
                }
            }
        }


        // not the quickest way in the world, but not supposed to do this at run time
        // can disable a channel, or get a brightness.  If avg granulatity set, you can average over a wider area than the granularity for more smoothing

        public enum BitmapFunction
        {
            Average,
            HeatMap,

            Maximum,
            Brightness,
        };

        public static Color AverageColour(this Bitmap bmp, RectangleF areainpercent)
        {
            System.Drawing.Imaging.BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                            System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);     // 32 bit words, ARGB format

            int xstart = (int)(bmp.Width * areainpercent.Left / 100.0);
            int xwidth = (int)(bmp.Width * areainpercent.Width / 100.0);
            int ystart = (int)(bmp.Height * areainpercent.Top / 100.0);
            int yheight = (int)(bmp.Height * areainpercent.Height / 100.0);
            int linestride = bmp.Width * 4;

            IntPtr baseptr = bmpdata.Scan0;     // its a byte ptr
            baseptr += 4 * xstart + linestride * ystart;

            long alpha = 0, red = 0, green = 0, blue = 0;

            for (int y = 0; y < yheight; y++)
            {
                for( int x = 0; x < xwidth; x++)
                {
                    int v = System.Runtime.InteropServices.Marshal.ReadInt32(baseptr);  // ARBG
                    alpha += (uint)((v >> 24) & 0xff);
                    red += (uint)((v >> 16) & 0xff);
                    green += (uint)((v >> 8) & 0xff);
                    blue += (uint)((v >> 0) & 0xff);
                    baseptr += 4;
                }

                baseptr += linestride - xwidth * 4;
            }

            int points = xwidth * yheight;
            alpha /= points;
            red /= points;
            green /= points;
            blue /= points;

            bmp.UnlockBits(bmpdata);

            return Color.FromArgb((int)alpha, (int)red, (int)green, (int)blue);
        }

        public static Bitmap Function(this Bitmap bmp, int granularityx, int granularityy, int avggranulatityx = 0, int avggranulatityy = 0, BitmapFunction mode = BitmapFunction.Average, 
                        bool enablered = true, bool enablegreen = true, bool enableblue = true, 
                        bool flipx = false, bool flipy = false)
        {
            System.Drawing.Imaging.BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                            System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);     // 32 bit words, ARGB format

            IntPtr baseptr = bmpdata.Scan0;     // its a byte ptr

            Bitmap newbmp = new Bitmap(granularityx, granularityy);  // bitmap to match

            if (avggranulatityx == 0)
                avggranulatityx = granularityx;
            if (avggranulatityy == 0)
                avggranulatityy = granularityy;

            int bmpcellsizex = bmp.Width / granularityx;      // no of avg points 
            int bmpcellsizey = bmp.Height / granularityx;
            int avgwidth = bmp.Width / avggranulatityx;
            int avgheight = bmp.Height / avggranulatityx;
            int linestride = bmp.Width * 4;

            for (int gy = 0; gy < granularityy; gy++)
            {
                for (int gx = 0; gx < granularityx; gx++)
                {
                    int x = bmpcellsizex / 2 + bmpcellsizex * gx - avgwidth/2;
                    int mx = x + avgwidth;
                    x = x.Range(0, bmp.Width-1);
                    mx = mx.Range(0, bmp.Width);

                    int y = bmpcellsizey / 2 + bmpcellsizey * gy - avgheight/2;
                    int my = y + avgheight;
                    y = y.Range(0, bmp.Height-1);
                    my = my.Range(0, bmp.Height);   // yes, let it go to height, its the stop value

                  //  System.Diagnostics.Debug.WriteLine("Avg " + x + "->" + mx + ", " + y +"->" + my);

                    uint red=0, green=0, blue = 0,points=0;

                    for (int ay = y; ay < my; ay++)
                    {
                        IntPtr ptr = baseptr + x * 4 + ay * linestride;
                        for (int ax = x; ax < mx; ax++)
                        {
                            int v = System.Runtime.InteropServices.Marshal.ReadInt32(ptr);  // ARBG
                            red += enablered ? (uint)((v >> 16) & 0xff) : 0;
                            green += enablegreen ? (uint)((v >> 8) & 0xff) : 0;
                            blue += enableblue ? (uint)((v >> 0) & 0xff) : 0;
                            ptr += 4;
                            points++;
                            //System.Diagnostics.Debug.WriteLine("Avg " + ax + "," + ay);
                        }
                    }

                    Color res;
                    if (mode == BitmapFunction.HeatMap)
                    {
                        double ir = (double)red * (double)red + (double)green * (double)green + (double)blue * (double)blue;
                        ir = Math.Sqrt(ir) * 255/442;   // scaling is for sqrt(255*255+255*255+255*255) to bring it back to 255 nom
                        ir /= points;
                        res = Color.FromArgb(255, (int)ir, (int)ir, (int)ir);
                    }
                    else
                        res = Color.FromArgb(255, (int)(red / points), (int)(blue / points), (int)(green / points));

                    newbmp.SetPixel(flipx ? (newbmp.Width-1-gx) : gx, flipy ? (newbmp.Height-1- gy) : gy, res);
                }

            }

            bmp.UnlockBits(bmpdata);
            return newbmp;
        }


        // average,Brightness or maximum over area of bitmap
        public static Tuple<float, float, float, float> Function(this Bitmap bmp, BitmapFunction mode, int x, int y, int width, int height )
        {
            System.Drawing.Imaging.BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                        System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);     // 32 bit words, ARGB format

            IntPtr baseptr = bmpdata.Scan0;     // its a byte ptr
            int linestride = bmp.Width * 4;

            ulong red = 0;
            ulong green = 0;
            ulong blue = 0;
            ulong alpha = 0;

            for (int gy = y; gy < y + height; gy++)
            {
                IntPtr ptr = baseptr + x * 4 + gy * linestride;

                for (int gx = x; gx < x + width; gx++)
                {
                    uint v = (uint)System.Runtime.InteropServices.Marshal.ReadInt32(ptr);  // ARBG
                    if (mode != BitmapFunction.Maximum)
                    {
                        alpha += ((v >> 24) & 0xff);
                        red += ((v >> 16) & 0xff);
                        green += ((v >> 8) & 0xff);
                        blue += ((v >> 0) & 0xff);
                        ptr += 4;
                    }
                    else 
                    {
                        alpha = Math.Max(alpha, ((v >> 24) & 0xff));
                        red += Math.Max(red, ((v >> 16) & 0xff));
                        green += Math.Max(green, ((v >> 8) & 0xff));
                        blue += Math.Max(blue, ((v >> 0) & 0xff));
                    }
                }
            }

            bmp.UnlockBits(bmpdata);

            int pixels = width * height;
            float ac = (float)alpha / pixels;
            float rc = (float)red / pixels;
            float gc = (float)green / pixels;
            float bc = (float)blue / pixels;

            if (mode == BitmapFunction.Average)
            {
                return new Tuple<float, float, float, float>(ac,rc,gc,bc);
            }
            else if (mode == BitmapFunction.Brightness)
            {
                double v = (float)rc * (float)rc + (float)gc * (float)gc + (float)bc * (float)bc;
                v = Math.Sqrt(v);
                return new Tuple<float, float, float, float>(ac, (float)v / 441.67296f,0,0);        // that Math.Sqrt(255^2+255^2+255^2)
            }
            else
                return new Tuple<float, float, float, float>(alpha, red, green, blue);
        }
    }
}
