/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
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

        public GLTexture2DArray()
        {
        }

        public GLTexture2DArray(Bitmap[] bmps, int mipmaplevel = 1, SizedInternalFormat internalformat = SizedInternalFormat.Rgba32f, int genmipmaplevel = 1, bool ownbitmaps = false)
        {
            LoadBitmaps(bmps, mipmaplevel, internalformat, genmipmaplevel, ownbitmaps);
        }

        public void CreateTexture( int width , int height, int depth , int mipmaplevels = 1, SizedInternalFormat internalformat = SizedInternalFormat.Rgba32f)
        {
            if (Id == -1 || Width != width || Height != height || Depth != depth)
            {
                InternalFormat = internalformat;
                Width = width;
                Height = height;
                Depth = depth;

                GL.CreateTextures(TextureTarget.Texture2DArray, 1, out int id);
                Id = id;

                GL.TextureStorage3D(Id,
                        mipmaplevels,        // miplevels.  Either given in the bitmap itself, or generated automatically
                        InternalFormat,         // format of texture - 4 floats is normal, given in constructor
                        Width,
                        Height,
                        Depth);       // depth = number of bitmaps depth

                SetMinMagFilter();

                OpenTKUtils.GLStatics.Check();
            }
        }

        // You can reload the bitmap, it will create a new Texture if required. Bitmaps array can be sparse will null entries if you don't want to use that level. Level 0 must be there

        public void LoadBitmaps(Bitmap[] bmps, int bitmapmipmaplevels = 1, SizedInternalFormat internalformat = SizedInternalFormat.Rgba32f, int genmipmaplevel = 1, bool ownbitmaps = false)
        {
            int h = MipMapHeight(bmps[0], bitmapmipmaplevels);        // if bitmap is mipped mapped, work out correct height.
            int texmipmaps = Math.Max(bitmapmipmaplevels, genmipmaplevel);

            CreateTexture(bmps[0].Width, h, bmps.Length, texmipmaps, internalformat);

            BitMaps = bmps;
            OwnBitmaps = ownbitmaps;

            for (int bitmapnumber = 0; bitmapnumber < bmps.Length; bitmapnumber++)      // for all bitmaps, we load the texture into zoffset of 2darray
            {
                if ( bmps[bitmapnumber] != null )       // it can be sparse
                    LoadBitmap(Id, bmps[bitmapnumber], bitmapmipmaplevels, bitmapnumber);   // load into bitmapnumber zoffset level
            }

            if (bitmapmipmaplevels == 1 && genmipmaplevel > 1)     // single level mipmaps with genmipmap levels > 1 get auto gen
                GL.GenerateTextureMipmap(Id);

            OpenTKUtils.GLStatics.Check();
        }

        // must have called CreateTexture before, allows bitmaps to be loaded individually
        // either make bitmapmipmaplevels>1 meaning the image is mipmapped, or use GenMipMapTextures() after all bitmaps in all z planes are loaded

        public void LoadBitmap(Bitmap map, int zoffset, int bitmapmipmaplevels = 1)
        {
            int h = MipMapHeight(map, bitmapmipmaplevels);        // if bitmap is mipped mapped, work out correct height.
            System.Diagnostics.Debug.Assert(map.Width == Width && map.Height == h && Id != -1);

            LoadBitmap(Id, map, bitmapmipmaplevels, zoffset);

            if (BitMaps == null)
                BitMaps = new Bitmap[Depth];

            BitMaps[zoffset] = map;
        }

    }
}
