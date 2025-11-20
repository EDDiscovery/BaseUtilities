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
    public static class BitMapHelpers
    {
        #region Multithread support
        // Helpers to use Images/Bitmaps in a multithread environment - can't multithread GDI objects so can't access a stored bitmaps in multi thread

        static object bitmapgdilock = new object();

        static public void DrawImageLocked(this Graphics gr, Image image, int x, int y, int width, int height)
        {
            lock (bitmapgdilock)
            {
                gr.DrawImage(image, x, y, width, height);
            }
        }

        static public void DrawImageLocked(this Graphics gr, Image image, Rectangle rect)
        {
            lock (bitmapgdilock)
            {
                gr.DrawImage(image, rect);
            }
        }

        // Clone bitmap
        public static Image CloneLocked(this Image source)
        {
            lock (bitmapgdilock)
            {
                Bitmap newmap = new Bitmap(source);
                return newmap;
            }
        }

        static Dictionary<Image, float> imageintensities = new Dictionary<Image, float>();       // cached locked image intensity database

        // return the image intensity of the central region of an image, protected
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

        // Clone bitmap and recolour
        public static Bitmap CloneReplaceColourLocked(Bitmap source, System.Drawing.Imaging.ColorMap[] remap)
        {
            lock (bitmapgdilock)
            {
                Bitmap newmap = new Bitmap(source.Width, source.Height);

                System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();
                ia.SetRemapTable(remap, System.Drawing.Imaging.ColorAdjustType.Bitmap);

                using (Graphics gr = Graphics.FromImage(newmap))
                    gr.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, ia);

                return newmap;
            }
        }

        // using FromFile or new Bitmap locks the file, this loads, copies it so its unattached
        // may except, protect yourself
        public static Bitmap CloneBitmapFromFileLocked(this string source)
        {
            lock (bitmapgdilock)
            {
                using (var tmp = new Bitmap(source))
                {
                    return new Bitmap(tmp);
                }
            }
        }

        #endregion

        #region Helpers

        public static Bitmap ScaleColourInBitmap(Bitmap source, System.Drawing.Imaging.ColorMatrix cm)
        {
            Bitmap newmap = new Bitmap(source.Width, source.Height);

            System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();
            ia.SetColorMatrix(cm);

            using (Graphics gr = Graphics.FromImage(newmap))
                gr.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, ia);

            return newmap;
        }

        public static Bitmap ScaleColourInBitmapSideBySide(Bitmap source, Bitmap source2, System.Drawing.Imaging.ColorMatrix cm)
        {
            Bitmap newmap = new Bitmap(source.Width + source2.Width, Math.Max(source.Height, source2.Height));

            System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();
            ia.SetColorMatrix(cm);

            using (Graphics gr = Graphics.FromImage(newmap))
            {
                gr.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, ia);
                gr.DrawImage(source2, new Rectangle(source.Width, 0, source2.Width, source2.Height), 0, 0, source2.Width, source2.Height, GraphicsUnit.Pixel, ia);
            }

            return newmap;
        }

        public static void DrawTextCentreIntoBitmap(ref Bitmap img, string text, Font dp, Color c, Color? b = null)
        {
            using (Graphics bgr = Graphics.FromImage(img))
            {
                if ( b!=null)
                {
                    Rectangle backarea = new Rectangle(0, 0, img.Width, img.Height);
                    using (Brush bb = new SolidBrush(b.Value))
                        bgr.FillRectangle(bb, backarea);
                }

                SizeF sizef = bgr.MeasureString(text, dp);

                bgr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (Brush textb = new SolidBrush(c))
                    bgr.DrawString(text, dp, textb, img.Width / 2 - (int)((sizef.Width + 1) / 2), img.Height / 2 - (int)((sizef.Height + 1) / 2));

                bgr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            }
        }

        // if b != Transparent, a back box is drawn.
        // bitmap never bigger than maxsize
        // no frmt means a single line across the bitmap unless there are \n in it.
        // maxsize normally clips the bitmap to this size
        // setting frmt allows you to word wrap etc into a bitmap.
        // if alignment == near, bitmap is restricted to text width/maxsize
        // if alignment != near, bitmap is maxsize width if maxsize.Width>0 (and the text is left/centre aligned in it).  If maxsize.Width==0, its the text width. Useful for centre word wrapped text
        // accepts maxsize having an element <1, if so, returns a 1 pixel image in that direction

        public static Bitmap DrawTextIntoAutoSizedBitmap(string text, Size maxsize, Font dp, Color c, Color b,
                                            float backscale = 1.0F, StringFormat frmt = null)
        {
            Bitmap t = new Bitmap(1, 1);

            using (Graphics bgr = Graphics.FromImage(t))
            {
                // if frmt, we measure the string within the maxsize bounding box.
                SizeF sizef = (frmt != null) ? bgr.MeasureString(text, dp, maxsize, frmt) : bgr.MeasureString(text, dp);
                //System.Diagnostics.Debug.WriteLine("Bit map auto size " + sizef);

                int width = Math.Min((int)(sizef.Width + 1), maxsize.Width); // first default width is the min of text width/maxsize
                if (frmt != null && frmt.Alignment != StringAlignment.Near) // if not near
                {
                    width = maxsize.Width > 0 ? maxsize.Width : (int)(sizef.Width+1);   // we use maxsize width, unless it zero, in which case we use text width
                }

                int height = Math.Min((int)(sizef.Height + 1), maxsize.Height);

                Bitmap img = new Bitmap(Math.Max(1, width), Math.Max(1, height)); // ensure we have a bitmap #2842

                using (Graphics dgr = Graphics.FromImage(img))
                {
                    if (!b.IsFullyTransparent() && text.Length > 0)
                    {
                        Rectangle backarea = new Rectangle(0, 0, img.Width, img.Height);
                        using (Brush bb = new System.Drawing.Drawing2D.LinearGradientBrush(backarea, b, b.Multiply(backscale), 90))
                            dgr.FillRectangle(bb, backarea);

                        dgr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;   // only worth doing this if we have filled it.. if transparent, antialias does not work
                    }

                    using (Brush textb = new SolidBrush(c))
                    {
                        if (frmt != null)
                            dgr.DrawString(text, dp, textb, new Rectangle(0, 0, width, height), frmt); // use the draw into rectangle with formatting function
                        else
                            dgr.DrawString(text, dp, textb, 0, 0);
                    }

                    dgr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

                    return img;
                }
            }
        }

        // draw into fixed sized bitmap. 
        // centretext overrided frmt and just centres it
        // frmt provides full options and draws text into bitmap
        // accepts maxsize having an element <1, if so, returns a 1 pixel image in that direction

        public static Bitmap DrawTextIntoFixedSizeBitmapC(string text, Size size, Font dp, Color c, Color b,
                                                    float backscale = 1.0F, StringFormat frmt = null, Point? pos = null)
        {
            Bitmap img = new Bitmap(Math.Max(1, size.Width), Math.Max(1, size.Height)); // ensure we have a bitmap #2842
            Color? back = null;
            if (!b.IsFullyTransparent() && text.Length > 0)       // transparent means no paint, or text length = 0 means no background paint, for this version
                back = b;
            DrawTextIntoBitmap(img, new Rectangle(0, 0, img.Width, img.Height), text, dp, c, back, backscale, frmt);
            return img;
        }

        // measure string by font and format.  if maxsize=null, its given all the space it needs.  If maxsize != null, limit it to this size (wordwrap, centre etc)
        public static SizeF MeasureStringInBitmap(string text, Font f, StringFormat fmt = null, Size? maxsize = null)
        {
            using (Bitmap t = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(t))
                {
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;      // recommendation from https://docs.microsoft.com/en-us/dotnet/api/system.drawing.graphics.measurestring?view=dotnet-plat-ext-5.0

                    if (maxsize == null)
                        maxsize = new Size(30000, 30000);

                    SizeF size = fmt != null ? g.MeasureString(text, f, maxsize.Value, fmt) : g.MeasureString(text, f, maxsize.Value);
                    return size;
                }
            }
        }

        // measure how long a string would be unformatted.
        public static SizeF MeasureStringUnformattedLengthInBitmap(string text, Font f)
        {
            using (Bitmap t = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(t))
                {
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;      // recommendation from https://docs.microsoft.com/en-us/dotnet/api/system.drawing.graphics.measurestring?view=dotnet-plat-ext-5.0
                    SizeF size = g.MeasureString(text, f);
                    return size;
                }
            }
        }

        // draw into bitmap at position.
        // If back colour is set, back fill area is sized to text used area (limited to maxsize).  This includes transparent back colour painting. Return SizeF
        // if back colour is null, then draw into box with pos and maxsize. Whole back area is coloured.  Return empty sizef

        public static SizeF DrawTextIntoBitmap(Bitmap img, Point pos, Size maxsize, string text, Font dp, Color c, Color? back,
                                                float backscale = 1.0F, StringFormat frmt = null, int angleback = 90)
        {
            if (back == null )
            {
                DrawTextIntoBitmap(img, new Rectangle(pos.X, pos.Y, maxsize.Width, maxsize.Height), text, dp, c, back, backscale, frmt);
                return SizeF.Empty;
            }
            else
            {
                SizeF sizef = MeasureStringInBitmap(text, dp, frmt, maxsize);
                DrawTextIntoBitmap(img, new Rectangle(pos.X, pos.Y, (int)(sizef.Width + 1), (int)(sizef.Height + 1)), text, dp, c, back, backscale, frmt);
                return sizef;
            }
        }

        // draw into bitmap into rectangle
        // If back colour is set, back fill is the whole rectangle
        public static void DrawTextIntoBitmap(Bitmap img, Rectangle area, string text, Font dp, Color c, Color? b,
                                                float backscale = 1.0F, StringFormat frmt = null, int angleback = 90)
        {
            using (Graphics dgr = Graphics.FromImage(img))
            {
                if (b != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Draw back into {area}");

                    if (b.Value.IsFullyTransparent())       // if transparent colour to paint in, need to fill clear it completely
                    {
                        if (area.Size != img.Size || area.Left != 0 || area.Top != 0 )      // if not the whole bitmap
                        {
                            dgr.SetClip(area);
                            dgr.Clear(Color.Transparent);       // seems to be the only way to set transparent pixels
                            dgr.ResetClip();
                        }
                        else
                            dgr.Clear(Color.Transparent);       // clear the lot
                    }
                    else
                    {
                        using (Brush bb = new System.Drawing.Drawing2D.LinearGradientBrush(area, b.Value, b.Value.Multiply(backscale), angleback))
                            dgr.FillRectangle(bb, area);
                    }

                    dgr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; // only if filled
                }

                using (Brush textb = new SolidBrush(c))
                {
                    if (frmt != null)
                        dgr.DrawString(text, dp, textb, area, frmt);
                    else
                        dgr.DrawString(text, dp, textb, area);
                }

                dgr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            }
        }


        public static void FillBitmap(Bitmap img, Color c, float backscale = 1.0F)
        {
            using (Graphics dgr = Graphics.FromImage(img))
            {
                Rectangle backarea = new Rectangle(0, 0, img.Width, img.Height);
                using (Brush bb = new System.Drawing.Drawing2D.LinearGradientBrush(backarea, c, c.Multiply(backscale), 90))
                    dgr.FillRectangle(bb, backarea);
            }
        }

        public static void ClearBitmapArea(Bitmap img, Rectangle backarea, Color c)     // allows transparent to be restored to areas..
        {
            using (Graphics dgr = Graphics.FromImage(img))
            {
                dgr.SetClip(backarea);   // set graphics to the clip area so we can clear a specific area
                dgr.Clear(c);
                dgr.ResetClip();
            }
        }

        // convert BMP to another format and return the bytes of that format

        public static byte[] ConvertTo(this Bitmap bmp, System.Drawing.Imaging.ImageFormat fmt)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            bmp.Save(ms, fmt);
            Byte[] f = ms.ToArray();
            return f;
        }

        // from https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp
        public static Bitmap ResizeImage(this Image image, int width, int height,
                                         System.Drawing.Drawing2D.InterpolationMode interm = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
                                        )
        {
            if (width <= 0)
                width = image.Width;
            if (height <= 0)
                height = image.Height;

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = interm;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static Bitmap CropImage(this Bitmap image, Rectangle croparea)
        {
            if ((croparea.Width <= 0) || (croparea.Width > image.Width))
            {
                croparea.X = 0;
                croparea.Width = image.Width;
            }
            else if (croparea.Left + croparea.Width > image.Width)
            {
                croparea.X = image.Width - croparea.Width;
            }

            if ((croparea.Height <= 0) || (croparea.Height > image.Height))
            {
                croparea.Y = 0;
                croparea.Height = image.Height;
            }
            else if (croparea.Top + croparea.Height > image.Height)
            {
                croparea.Y = image.Height - croparea.Height;
            }

            return image.Clone(croparea, System.Drawing.Imaging.PixelFormat.DontCare);
        }

        /// <summary>
        /// Crop bitmap by area - percentages (0-100)
        /// </summary>
        /// <param name="image">Bitmap</param>
        /// <param name="croparea">crop area in percentages</param>
        /// <returns>new Bitmap</returns>
        public static Bitmap CropImage(this Bitmap image, RectangleF croparea)
        {
            int left = (int)(image.Width * croparea.X / 100.0F);
            int top = (int)(image.Height * croparea.Y / 100.0F);
            int width = (int)(image.Width * croparea.Width / 100.0F);
            int height = (int)(image.Height * croparea.Height / 100.0F);
            left = Math.Max(0, left);
            top = Math.Max(0, top);
            width = Math.Min(width, image.Width - left);
            height = Math.Min(height, image.Height - top);

            return image.Clone(new Rectangle(left, top, width, height), System.Drawing.Imaging.PixelFormat.DontCare);
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

    #endregion
}
