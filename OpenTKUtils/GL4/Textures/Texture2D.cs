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
    public class GLTexture2D : GLTextureBase          // load a texture into open gl
    {
        public GLTexture2D()
        {
        }

        public GLTexture2D(Bitmap bmp, int bitmipmaplevel = 1, SizedInternalFormat internalformat = SizedInternalFormat.Rgba32f, 
                            int genmipmaplevel = 1, bool ownbitmaps = false)
        {
            LoadBitmap(bmp, bitmipmaplevel, internalformat, genmipmaplevel, ownbitmaps);
        }

        // You can call as many times to create textures. Only creates one if required

        public void CreateOrUpdateTexture(int width, int height, int mipmaplevels = 1, SizedInternalFormat internalformat = SizedInternalFormat.Rgba32f)
        {
            if (Id == -1 || Width != width || Height != height)    // if not there, or changed, we can't just replace it, size is fixed. Delete it
            {
                System.Diagnostics.Debug.WriteLine("Remake texture");
                if ( Id != -1 )
                { 
                    Dispose();
                }

                InternalFormat = internalformat;
                Width = width;
                Height = height;

                GL.CreateTextures(TextureTarget.Texture2D, 1, out int id);
                Id = id;

                GL.TextureStorage2D(
                                Id,
                                mipmaplevels,                    // levels of mipmapping
                                InternalFormat,                 // format of texture - 4 floats is the normal, and is given in the constructor
                                Width,                          // width and height of mipmap level 0
                                Height);

                SetMinMagFilter();

                OpenTKUtils.GLStatics.Check();
            }
        }

        // You can reload the bitmap, it will create a new Texture if required

        public void LoadBitmap(Bitmap bmp, int bitmipmaplevels = 1, SizedInternalFormat internalformat = SizedInternalFormat.Rgba32f, int genmipmaplevel = 1, bool ownbitmaps = false)
        {
            int h = MipMapHeight(bmp, bitmipmaplevels);
            int texmipmaps = Math.Max(bitmipmaplevels, genmipmaplevel);

            CreateOrUpdateTexture(bmp.Width, h, texmipmaps, internalformat);

            BitMaps = new Bitmap[1];
            BitMaps[0] = bmp;
            OwnBitmaps = ownbitmaps;

            GLTextureBase.LoadBitmap(Id, bmp, bitmipmaplevels, -1);    // use common load into bitmap, indicating its a 2D texture so use texturesubimage2d

            if (bitmipmaplevels == 1 && genmipmaplevel > 1)     // single level mipmaps with genmipmap levels > 1 get auto gen
                GL.GenerateTextureMipmap(Id);
            
            OpenTKUtils.GLStatics.Check();

           // float[] tex = GetTextureImageAsFloats(end:100);

        }


    }
}

