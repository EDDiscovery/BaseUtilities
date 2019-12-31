/*
 * Copyright © 2019 Robbyxp1 @ github.com
 * Part of the EDDiscovery Project
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
using System.Linq;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    public class GLTexture2DArray : GLTextureBase          // load a 2D set of textures into open gl
    {
        // bitmap 0 gives the common width/height of the image.
        // 2d arrays do not interpolate between z pixels, unlike 3d textures

        public GLTexture2DArray(Bitmap[] bmps, int mipmaplevel = 1, SizedInternalFormat internalformat = SizedInternalFormat.Rgba32f, int genmipmaplevel = 1, bool ownbitmaps = false)
        {
            InternalFormat = internalformat;
            BitMaps = bmps;
            OwnBitmaps = ownbitmaps;

            GL.CreateTextures(TextureTarget.Texture2DArray, 1, out int id);
            Id = id;
            Depth = bmps.Length;

            for (int bitmapnumber = 0; bitmapnumber < bmps.Length; bitmapnumber++)      // for all bitmaps, we load the texture into zoffset of 2darray
            {
                if (bitmapnumber == 0)
                {
                    Width = bmps[0].Width;
                    Height = (mipmaplevel == 1) ? bmps[0].Height : (bmps[0].Height / 3) * 2;

                    GL.TextureStorage3D(Id,
                            mipmaplevel == 1 ? genmipmaplevel : mipmaplevel,        // miplevels.  Either given in the bitmap itself, or generated automatically
                            InternalFormat,         // format of texture - 4 floats is normal, given in constructor
                            Width,
                            Height,
                            bmps.Length);       // depth = number of bitmaps depth
                }

                LoadBitmap(Id, bmps[bitmapnumber], mipmaplevel, bitmapnumber);   // load into bitmapnumber zoffset level
            }

            if (mipmaplevel == 1 && genmipmaplevel > 1)     // single level mipmaps with genmipmap levels > 1 get auto gen
                GL.GenerateTextureMipmap(Id);

            SetMinMagFilter();

            OpenTKUtils.GLStatics.Check();
        }

        public GLTexture2DArray(int nobitmaps, int width, int height, int mipmaplevel = 1, SizedInternalFormat internalformat = SizedInternalFormat.Rgba32f)
        {
            InternalFormat = internalformat;

            GL.CreateTextures(TextureTarget.Texture2DArray, 1, out int id);
            Id = id;
            Depth = nobitmaps;
            Width = width;
            Height = height;

            GL.TextureStorage3D(Id,
                    mipmaplevel,            // miplevels.
                    InternalFormat,         // format of texture - 4 floats is normal, given in constructor
                    Width,
                    Height,
                    nobitmaps);             // depth = number of bitmaps depth

            OpenTKUtils.GLStatics.Check();
        }

        public void StoreBitmap(Bitmap map, int bmpnumber, int bitmapmipmaplevels = 1)
        {
            LoadBitmap(Id, map, bitmapmipmaplevels, bmpnumber);
        }
    }
}
