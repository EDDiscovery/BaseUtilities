/*
 * Copyright © 2022 - 2022 EDDiscovery development team
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

public static partial class DrawingHelpersStaticFunc
{
    #region Content Align

    static public StringFormat StringFormatFromContentAlignment(this ContentAlignment c)
    {
        StringFormat f = new StringFormat();
        if (c == ContentAlignment.BottomCenter || c == ContentAlignment.MiddleCenter || c == ContentAlignment.TopCenter)
            f.Alignment = StringAlignment.Center;
        else if (c == ContentAlignment.BottomLeft || c == ContentAlignment.MiddleLeft || c == ContentAlignment.TopLeft)
            f.Alignment = StringAlignment.Near;
        else
            f.Alignment = StringAlignment.Far;

        if (c == ContentAlignment.BottomCenter || c == ContentAlignment.BottomLeft || c == ContentAlignment.BottomRight)
            f.LineAlignment = StringAlignment.Far;
        else if (c == ContentAlignment.MiddleLeft || c == ContentAlignment.MiddleCenter || c == ContentAlignment.MiddleRight)
            f.LineAlignment = StringAlignment.Center;
        else
            f.LineAlignment = StringAlignment.Near;

        return f;
    }

    static public Rectangle ImagePositionFromContentAlignment(this ContentAlignment c, Rectangle client, Size image, bool cliptorectangle = false)
    {
        int left = client.Left;

        if (c == ContentAlignment.BottomCenter || c == ContentAlignment.MiddleCenter || c == ContentAlignment.TopCenter)
            left += Math.Max((client.Width - image.Width) / 2, 0);
        else if (c == ContentAlignment.BottomLeft || c == ContentAlignment.MiddleLeft || c == ContentAlignment.TopLeft)
            left += 0;
        else
            left += Math.Max(client.Width - image.Width, 0);

        int top = client.Top;

        if (c == ContentAlignment.BottomCenter || c == ContentAlignment.BottomLeft || c == ContentAlignment.BottomRight)
            top += Math.Max(client.Height - image.Height, 0);
        else if (c == ContentAlignment.MiddleLeft || c == ContentAlignment.MiddleCenter || c == ContentAlignment.MiddleRight)
            top += Math.Max((client.Height - image.Height) / 2, 0);
        else
            top += 0;

        if (cliptorectangle)        // ensure we start in rectangle..
        {
            left = Math.Max(0, left);
            top = Math.Max(0, top);
        }

        return new Rectangle(left, top, image.Width, image.Height);
    }

    #endregion

    #region Rectangles

    public static int XCenter(this Rectangle r)
    {
        return (r.Right + r.Left) / 2;
    }

    public static int YCenter(this Rectangle r)
    {
        return (r.Top + r.Bottom) / 2;
    }
    public static int Area(this Rectangle r)
    {
        return r.Width * r.Height;
    }

    public static Point ClipTo(this Point p, Rectangle r,int offsetrightbottom = -1)
    {
        return new Point(p.X.Range(r.Left, r.Right + offsetrightbottom), p.Y.Range(r.Top, r.Bottom + offsetrightbottom));
    }

    static public GraphicsPath RectCutCorners(int x, int y, int width, int height, int roundnessleft, int roundnessright)
    {
        GraphicsPath gr = new GraphicsPath();

        gr.AddLine(x + roundnessleft, y, x + width - 1 - roundnessright, y);
        gr.AddLine(x + width - 1, y + roundnessright, x + width - 1, y + height - 1 - roundnessright);
        gr.AddLine(x + width - 1 - roundnessright, y + height - 1, x + roundnessleft, y + height - 1);
        gr.AddLine(x, y + height - 1 - roundnessleft, x, y + roundnessleft);
        gr.AddLine(x, y + roundnessleft, x + roundnessleft, y);         // close figure manually, closing it with a break does not seem to work
        return gr;
    }

    // produce a rounded rectangle with a cut out at the top..

    static public GraphicsPath RectCutCorners(int x, int y, int width, int height, int roundnessleft, int roundnessright, int topcutpos, int topcutlength)
    {
        GraphicsPath gr = new GraphicsPath();

        if (topcutlength > 0)
        {
            gr.AddLine(x + roundnessleft, y, x + topcutpos, y);
            gr.StartFigure();
            gr.AddLine(x + topcutpos + topcutlength, y, x + width - 1 - roundnessright, y);
        }
        else
            gr.AddLine(x + roundnessleft, y, x + width - 1 - roundnessright, y);

        gr.AddLine(x + width - 1, y + roundnessright, x + width - 1, y + height - 1 - roundnessright);
        gr.AddLine(x + width - 1 - roundnessright, y + height - 1, x + roundnessleft, y + height - 1);
        gr.AddLine(x, y + height - 1 - roundnessleft, x, y + roundnessleft);
        gr.AddLine(x, y + roundnessleft, x + roundnessleft, y);         // close figure manually, closing it with a break does not seem to work
        return gr;
    }

    static public Rectangle Add(this Rectangle r, Rectangle next)
    {
        if (r.Width == 0 || r.Height == 0)      // if no size on r, then its next
            return next;
        else
        {
            var left = Math.Min(r.Left, next.Left);
            var top = Math.Min(r.Top, next.Top);
            return new Rectangle(left, top, Math.Max(r.Right, next.Right) - left, Math.Max(r.Bottom, next.Bottom) - top);
        }
    }

    #endregion

    #region Misc

    // this scales the font down only to fit into textarea given a graphic and text.  Used in Paint
    // fnt itself is not deallocated.
    public static Font GetFontToFit(this Graphics g, string text, Font fnt, Size textarea, StringFormat fmt)
    {
        if (!text.HasChars())       // can't tell
            return fnt;

        bool ownfont = false;
        while (true)
        {
            SizeF drawnsize = g.MeasureString(text, fnt, new Point(0, 0), fmt);

            if (fnt.Size < 2 || ((int)(drawnsize.Width + 0.99f) <= textarea.Width && (int)(drawnsize.Height + 0.99f) <= textarea.Height))
                return fnt;

            if (ownfont)
                fnt.Dispose();

            fnt = BaseUtils.FontLoader.GetFont(fnt.FontFamily.Name, fnt.Size - 0.5f, fnt.Style);
            ownfont = true;
        }
    }

    // this scales the font up or down to fit width and height.  Text is not allowed to wrap, its unformatted
    // fnt itself is not deallocated.
    public static Font GetFontToFit(string text, Font fnt, Size areasize)
    {
        if (!text.HasChars())       // can't tell
            return fnt;

        SizeF drawnsize = BaseUtils.BitMapHelpers.MeasureStringUnformattedLengthInBitmap(text, fnt);

        bool smallerthanbox = Math.Ceiling(drawnsize.Width) <= areasize.Width && Math.Ceiling(drawnsize.Height) < areasize.Height;
        float dir = smallerthanbox ? 0.5f : -0.5f;
        float fontsize = fnt.Size;
        System.Diagnostics.Debug.WriteLine($"Autofont {fnt.Name} {fnt.Size} fit {areasize} = {drawnsize} {smallerthanbox} dir {dir}");

        bool ownfont = false;

        while (true)
        {
            fontsize += dir;

            Font fnt2 = BaseUtils.FontLoader.GetFont(fnt.FontFamily.Name, fontsize, fnt.Style);

            drawnsize = BaseUtils.BitMapHelpers.MeasureStringUnformattedLengthInBitmap(text, fnt2);
            smallerthanbox = Math.Ceiling(drawnsize.Width) <= areasize.Width && Math.Ceiling(drawnsize.Height) < areasize.Height;

            System.Diagnostics.Debug.WriteLine($"Autofontnext  {fnt2.Name} {fnt2.Size} fit {areasize} = {drawnsize} {smallerthanbox} dir {dir}");

            // conditions to stop, betting too big, betting small enough, too small font
            if ((dir > 0 && !smallerthanbox) || (dir < 0 && smallerthanbox) || (dir < 0 && fnt.Size < 2))
            {
                fnt2.Dispose();
                return fnt;
            }
            else
            {
                if (ownfont)
                    fnt.Dispose();
                fnt = fnt2;
                ownfont = true;
            }
        }
    }

    public static StringFormat StringFormatFromName(this string textrep, StringFormat fmt = null)
    {
        if (fmt == null)
            fmt = new StringFormat();
        if (textrep.Contains("centre", StringComparison.InvariantCultureIgnoreCase) || textrep.Contains("center", StringComparison.InvariantCultureIgnoreCase))
            fmt.Alignment = StringAlignment.Center;
        if (textrep.Contains("right", StringComparison.InvariantCultureIgnoreCase))
            fmt.Alignment = StringAlignment.Far;
        if (textrep.Contains("left", StringComparison.InvariantCultureIgnoreCase))
            fmt.Alignment = StringAlignment.Near;
        if (textrep.Contains("top", StringComparison.InvariantCultureIgnoreCase))
            fmt.LineAlignment = StringAlignment.Near;
        if (textrep.Contains("bottom", StringComparison.InvariantCultureIgnoreCase))
            fmt.LineAlignment = StringAlignment.Near;
        if (textrep.Contains("middle", StringComparison.InvariantCultureIgnoreCase))
            fmt.LineAlignment = StringAlignment.Center;
        if (textrep.Contains("nowrap", StringComparison.InvariantCultureIgnoreCase))
            fmt.FormatFlags |= StringFormatFlags.NoWrap;
        return fmt;
    }

    public static Size MeasureItems(this Graphics g, Font fnt, string[] array, StringFormat fmt)
    {
        Size max = new Size(0, 0);
        foreach (string s in array)
        {
            SizeF f = g.MeasureString(s, fnt, new Point(0, 0), fmt);
            max = new Size(Math.Max(max.Width, (int)(f.Width + 0.99)), Math.Max(max.Height, (int)(f.Height + 0.99)));
        }

        return max;
    }

    public static int ScalePixels(this Font f, int nominalat12)      //given a font, and size at normal 12 point, what size should i make it now
    {
        return (int)(f.GetHeight() / 18 * nominalat12);
    }

    public static int ScaleScrollbar(this Font f)                    //given a font, scale the scroll bar
    {
        return (int)(f.GetHeight() / 18 * 28);
    }

    public static float ScaleSize(this Font f, float nominalat12)      //given a font, and size at normal 12 point, what size should i make it now
    {
        return f.GetHeight() / 18 * nominalat12;
    }

    // used to compute ImageAttributes, given a disabled scaling, a remap table, and a optional color matrix
    static public void ComputeDrawnPanel(out ImageAttributes Enabled,
                    out ImageAttributes Disabled,
                    float disabledscaling, System.Drawing.Imaging.ColorMap[] remap, float[][] colormatrix = null)
    {
        Enabled = new ImageAttributes();
        Enabled.SetRemapTable(remap, ColorAdjustType.Bitmap);
        if (colormatrix != null)
            Enabled.SetColorMatrix(new ColorMatrix(colormatrix), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

        Disabled = new ImageAttributes();
        Disabled.SetRemapTable(remap, ColorAdjustType.Bitmap);

        if (colormatrix != null)
        {
            colormatrix[0][0] *= disabledscaling;     // the identity positions are scaled by BDS 
            colormatrix[1][1] *= disabledscaling;
            colormatrix[2][2] *= disabledscaling;
            Disabled.SetColorMatrix(new ColorMatrix(colormatrix), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        }
        else
        {
            float[][] disabledMatrix = {
                        new float[] {disabledscaling,  0,  0,  0, 0},        // red scaling factor of BDS
                        new float[] {0,  disabledscaling,  0,  0, 0},        // green scaling factor of BDS
                        new float[] {0,  0,  disabledscaling,  0, 0},        // blue scaling factor of BDS
                        new float[] {0,  0,  0,  1, 0},        // alpha scaling factor of 1
                        new float[] {0,0,0, 0, 1}};    // three translations of 0

            Disabled.SetColorMatrix(new ColorMatrix(disabledMatrix), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        }
    }

    // Helper to paint multi coloured backgrounds
    static public void DrawMultiColouredRectangles(this Graphics gr, Rectangle area, Color[] themecolours, float direction)
    {
        if (area.Width > 0 && area.Height > 0)
        {
            using (LinearGradientBrush br = new LinearGradientBrush(area, themecolours[0], themecolours[1], direction))
            {
                if (themecolours.Length >= 4 && themecolours[3] != themecolours[2])
                {
                    br.InterpolationColors = new ColorBlend(4)
                    {
                        Colors = new Color[] { themecolours[0], themecolours[1], themecolours[2], themecolours[3] },
                        Positions = new float[] { 0f, 0.33f, 0.66f, 1f }
                    };
                    //System.Diagnostics.Debug.WriteLine($"Paint MultiColoured Rectangles {area} with {br.InterpolationColors.Colors.Length}");
                }
                else if (themecolours.Length >= 3 && themecolours[2] != themecolours[1])
                {
                    br.InterpolationColors = new ColorBlend(3)
                    {
                        Colors = new Color[] { themecolours[0], themecolours[1], themecolours[2] },
                        Positions = new float[] { 0f, 0.5f, 1f }
                    };
                    // System.Diagnostics.Debug.WriteLine($"Paint MultiColoured Rectangles {area} with {br.InterpolationColors.Colors.Length}");
                }
                else
                {
                    //   System.Diagnostics.Debug.WriteLine($"Paint MultiColoured Rectangles {area}");
                }

                gr.FillRectangle(br, area);
            }
        }
    }

    // Helper to paint a rectangle
    static public void DrawFilledRectangle(this Graphics gr, Rectangle area, Color colour)
    {
        if (area.Width > 0 && area.Height > 0)
        {
            using (SolidBrush br = new SolidBrush(colour))
            {
                gr.FillRectangle(br, area);
            }
        }
    }



    #endregion
}
