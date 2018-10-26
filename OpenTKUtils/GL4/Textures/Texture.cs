/*
 * Copyright © 2015 - 2018 EDDiscovery development team
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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    public class GLTexture2D : IGLTexture          // load a texture into open gl
    {
        private int Id;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public GLTexture2D(Bitmap bmp, int mipmaplevel = 1, SizedInternalFormat internalformat = SizedInternalFormat.Rgba32f, int genmipmaplevel = 4)
        {
            GL.CreateTextures(TextureTarget.Texture2D, 1, out Id);

            Width = bmp.Width;
            Height = LoadMipMap(Id, bmp, mipmaplevel, genmipmaplevel, internalformat);
        }

        static int LoadMipMap(int Id, Bitmap bmp, int mipmaplevel, int genmipmaplevel, SizedInternalFormat internalformat = SizedInternalFormat.Rgba32f)
        { 
            System.Drawing.Imaging.BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                            System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);     // 32 bit words, ARGB format

            IntPtr ptr = bmpdata.Scan0;     // its a byte ptr, not an int ptr in the classic sense.


            int height = (mipmaplevel == 1) ? bmp.Height : (bmp.Height / 3) * 2;

            GL.TextureStorage2D(
                Id,
                mipmaplevel == 1 ? genmipmaplevel : mipmaplevel,    // levels of mipmapping.  If we supplied one, we use that, else we use genmipmaplevel
                internalformat,                       // format of texture - 4 floats is the normal, and is given in the constructor
                bmp.Width,                            // width and height of mipmap level 0
                height);
            GL4Statics.Check();

            int curwidth = bmp.Width;
            int curheight = height;

            GL.PixelStore(PixelStoreParameter.UnpackRowLength, bmp.Width);      // indicate the image width, if we take less, then GL will skip pixels to get to next row

            for (int m = 0; m < mipmaplevel;  m++)
            {
                GL.TextureSubImage2D(Id,
                    m,                  // this is level m
                    0,                  // x offset inside the target texture..
                    0,                  // y offset..
                    curwidth,           // width to load in the target texture
                    curheight,          // height..
                    PixelFormat.Bgra,
                    PixelType.UnsignedByte,     // and we asked above for Bgra data as unsigned bytes
                    ptr);
                GL4Statics.Check();

                if (m == 0)             // at 0, we jump down the whole image.  4 is the bytes/pixel
                    ptr += bmp.Width * height * 4;
                else
                    ptr += curwidth * 4;    // else we move across by curwidth.

                if (curwidth > 1)           // scale down size by 2
                    curwidth /= 2;
                if (curheight > 1)
                    curheight /= 2;
            }

            bmp.UnlockBits(bmpdata);


            var textureMinFilter = (int)All.LinearMipmapLinear;
            GL.TextureParameterI(Id, TextureParameterName.TextureMinFilter, ref textureMinFilter);
            var textureMagFilter = (int)All.Linear;
            GL.TextureParameterI(Id, TextureParameterName.TextureMagFilter, ref textureMagFilter);

            //var textureWrap = (int)All.ClampToBorder;     // for testing, keep for now.
            //GL.TextureParameterI(Id, TextureParameterName.TextureWrapS, ref textureWrap);     
            //GL.TextureParameterI(Id, TextureParameterName.TextureWrapT, ref textureWrap);


            GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);      // back to off for safety

            if (mipmaplevel == 1)                                       // single level mipmaps get auto gen
                GL.GenerateTextureMipmap(Id);

            return height;
        }

        public void Bind(int bindingpoint)
        {
            //GL.BindTexture(TextureTarget.Texture2D, Id);  // not needed
            GL.BindTextureUnit(bindingpoint, Id);
        }

        public void Dispose()           // you can double dispose.
        {
            if (Id != -1)
            {
                GL.DeleteTexture(Id);
                Id = -1;
            }
        }
    }

    public class GLTexture2DArray : IGLTexture          // load a 2D set of textures into open gl
    {
        private int Id;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public GLTexture2DArray(int bindingpoint, Bitmap[] bmps, int mipmaplevel = 1, SizedInternalFormat internalformat = SizedInternalFormat.Rgba32f)
        {
            GL.CreateTextures(TextureTarget.Texture2DArray, 1, out Id);
            GL.BindTexture(TextureTarget.Texture2DArray, Id);
            GL4Statics.Check();

            for (int bitmapnumber = 0; bitmapnumber < bmps.Length; bitmapnumber++)
            {
                if (bitmapnumber == 0)
                {
                    Width = bmps[0].Width;
                    Height = (mipmaplevel == 1) ? bmps[0].Height : (bmps[0].Height / 3) * 2;

                    GL.TexStorage3D(TextureTarget3d.Texture2DArray,
                            mipmaplevel, // miplevels
                            internalformat,         // format of texture - 4 floats is normal, given in constructor
                            Width,
                            Height,
                            bmps.Length);       // number of bitmaps depth
                    GL4Statics.Check();
                }

                System.Drawing.Imaging.BitmapData bmpdata = bmps[bitmapnumber].LockBits(new Rectangle(0, 0, bmps[bitmapnumber].Width, bmps[bitmapnumber].Height),
                                System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);     // 32 bit words, ARGB format

                IntPtr ptr = bmpdata.Scan0;     // its a byte ptr, not an int ptr in the classic sense.

                int curwidth = Width;
                int curheight = Height;

                GL.PixelStore(PixelStoreParameter.UnpackRowLength, bmps[bitmapnumber].Width);      // indicate the image width, if we take less, then GL will skip pixels to get to next row

                for (int m = 0; m < mipmaplevel; m++)
                {
                    GL.TexSubImage3D(TextureTarget.Texture2DArray,
                        m,  // miplevel
                        0,      // xoff into target
                        0,      // yoff into target
                        bitmapnumber,  // zoffset, which is the bitmap depth
                        curwidth,       // size of image
                        curheight,
                        1,      // depth of the texture, which is 1 pixel
                        PixelFormat.Bgra,
                        PixelType.UnsignedByte, // unsigned bytes in BGRA.  PixelStore above indicated the stride across 1 row
                        ptr);
                    GL4Statics.Check();

                    if (m == 0)             // at 0, we jump down the whole first image.  4 is the bytes/pixel
                        ptr += Width * Height * 4;
                    else
                        ptr += curwidth * 4;    // else we move across by curwidth.

                    if (curwidth > 1)           // scale down size by 2
                        curwidth /= 2;
                    if (curheight > 1)
                        curheight /= 2;
                }

                if (mipmaplevel == 1)                                       // single level mipmaps get auto gen
                    GL.GenerateTextureMipmap(Id);

                bmps[bitmapnumber].UnlockBits(bmpdata);
            }

            var textureMinFilter = (int)All.LinearMipmapLinear;
            GL.TextureParameterI(Id, TextureParameterName.TextureMinFilter, ref textureMinFilter);
            var textureMagFilter = (int)All.Linear;
            GL.TextureParameterI(Id, TextureParameterName.TextureMagFilter, ref textureMagFilter);
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);      // back to off for safety
            GL4Statics.Check();
        }

        public void Bind(int bindingpoint)
        {
            GL.BindTexture(TextureTarget.Texture2DArray, Id);
            GL.BindTextureUnit(bindingpoint, Id);
            GL4Statics.Check();
        }

        public void Dispose()           // you can double dispose.
        {
            if (Id != -1)
            {
                GL.DeleteTexture(Id);
                Id = -1;
            }
        }
    }



}

