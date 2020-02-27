/*
 * Copyright © 2016-2020 EDDiscovery development team
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
using System.Drawing;
using System.Drawing.Imaging;

// from BaseUtils abridged

public static class ControlHelpersStaticFunc
{
    static public StringFormat StringFormatFromContentAlignment(ContentAlignment c)
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

    public static int ScalePixels(this Font f, int nominalat12)      //given a font, and size at normal 12 point, what size should i make it now
    {
        return (int)(f.GetHeight() / 18 * nominalat12);
    }

    // used to compute ImageAttributes, given a disabled scaling, a remap table, and a optional color matrix
    static public void ComputeDrawnPanel(out ImageAttributes enabled,
                    out ImageAttributes disabled, float disabledscaling, 
                    System.Drawing.Imaging.ColorMap[] remap, float[][] colormatrix = null)
    {
        enabled = new ImageAttributes();
        enabled.SetRemapTable(remap, ColorAdjustType.Bitmap);
        if (colormatrix != null)
            enabled.SetColorMatrix(new ColorMatrix(colormatrix), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

        disabled = new ImageAttributes();
        disabled.SetRemapTable(remap, ColorAdjustType.Bitmap);

        if (colormatrix != null)
        {
            colormatrix[0][0] *= disabledscaling;     // the identity positions are scaled by BDS
            colormatrix[1][1] *= disabledscaling;
            colormatrix[2][2] *= disabledscaling;
            disabled.SetColorMatrix(new ColorMatrix(colormatrix), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        }
        else
        {
            float[][] disabledMatrix = {
                        new float[] {disabledscaling,  0,  0,  0, 0},        // red scaling factor of BDS
                        new float[] {0,  disabledscaling,  0,  0, 0},        // green scaling factor of BDS
                        new float[] {0,  0,  disabledscaling,  0, 0},        // blue scaling factor of BDS
                        new float[] {0,  0,  0,  1, 0},        // alpha scaling factor of 1
                        new float[] {0,0,0, 0, 1}};    // three translations of 0

            disabled.SetColorMatrix(new ColorMatrix(disabledMatrix), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        }
    }

    public static int XCenter(this Rectangle r)
    {
        return (r.Right + r.Left) / 2;
    }

    public static int YCenter(this Rectangle r)
    {
        return (r.Top + r.Bottom) / 2;
    }

    public static Font GetFontToFitRectangle(this Graphics g, string text, Font fnt, Rectangle textarea, StringFormat fmt)
    {
        bool ownfont = false;
        while (true)
        {
            SizeF drawnsize = g.MeasureString(text, fnt, new Point(0, 0), fmt);

            if ((int)(drawnsize.Width + 0.99f) <= textarea.Width && (int)(drawnsize.Height + 0.99f) <= textarea.Height)
                return fnt;

            if (ownfont)
                fnt.Dispose();

            fnt = new Font(fnt.FontFamily.Name, fnt.Size - 0.5f, fnt.Style);
            ownfont = true;
        }
    }

    static public Rectangle ImagePositionFromContentAlignment(this ContentAlignment c, Rectangle client, Size image, bool cliplefttop = false, bool stayinrectangle = false)
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

        if (cliplefttop)        // ensure we start in rectangle..
        {
            left = Math.Max(Math.Min(client.Right, left), client.Left);
            top = Math.Max(Math.Min(client.Bottom, top), client.Top);
        }

        int ih = image.Height;
        int iw = image.Width;

        if (stayinrectangle)
        {
            ih = Math.Min(client.Height, ih);
            iw = Math.Min(client.Width, iw);
        }

        return new Rectangle(left, top, iw, ih);
    }


}
