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
    public class GLTexture2D : IGLTexture           // load a texture into open gl
    {
        private int Id;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public GLTexture2D(Bitmap bmp)
        {
            Width = bmp.Width;
            Height = bmp.Height;

            GL.CreateTextures(TextureTarget.Texture2D, 1, out Id);
            GL.BindTexture(TextureTarget.Texture2D, Id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            System.Drawing.Imaging.BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpdata.Width, bmpdata.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bmpdata.Scan0);
            bmp.UnlockBits(bmpdata);

            GL.Disable(EnableCap.Texture2D);
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, Id);
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

    // MIP MAP texture 

    public class GLTexture2DMipMap : IGLTexture          // load a texture into open gl
    {
        private int Id;
        public int Width { get; private set; }
        public int Height { get; private set; }
        private int maxMipmapLevel;

        public GLTexture2DMipMap(Bitmap bmp, int mipmaplevel)
        {
            Width = bmp.Width;
            Height = bmp.Height;
            maxMipmapLevel = mipmaplevel;

            GL.CreateTextures(TextureTarget.Texture2D, 1, out Id);
            GL.BindTexture(TextureTarget.Texture2D, Id);

            var data = LoadTexture(bmp);
            GL.CreateTextures(TextureTarget.Texture2D, 1, out Id);
            GL.BindTexture(TextureTarget.Texture2D, Id);
            GL.TextureStorage2D(
                Id,
                maxMipmapLevel,             // levels of mipmapping
                SizedInternalFormat.Rgba32f, // format of texture
                data.First().Width,
                data.First().Height);

            for (int m = 0; m < data.Count; m++)
            {
                var mipLevel = data[m];
                GL.TextureSubImage2D(Id,
                    m,                  // this is level m
                    0,                  // x offset
                    0,                  // y offset
                    mipLevel.Width,
                    mipLevel.Height,
                    PixelFormat.Rgba,
                    PixelType.Float,
                    mipLevel.Data);
            }

            var textureMinFilter = (int)All.LinearMipmapLinear;
            GL.TextureParameterI(Id, TextureParameterName.TextureMinFilter, ref textureMinFilter);
            var textureMagFilter = (int)All.Linear;
            GL.TextureParameterI(Id, TextureParameterName.TextureMagFilter, ref textureMagFilter);

            GL.Disable(EnableCap.Texture2D);
        }

        public struct MipLevel
        {
            public int Level;
            public int Width;
            public int Height;
            public float[] Data;
        }

        private List<MipLevel> LoadTexture(Bitmap bmp)
        {
            var mipmapLevels = new List<MipLevel>();
            int xOffset = 0;
            int width = bmp.Width;
            int height = (bmp.Height / 3) * 2;
            var originalHeight = height;
            for (int m = 0; m < maxMipmapLevel; m++)
            {
                xOffset += m == 0 || m == 1 ? 0 : width * 2;
                var yOffset = m == 0 ? 0 : originalHeight;

                MipLevel mipLevel;
                mipLevel.Level = m;
                mipLevel.Width = width;
                mipLevel.Height = height;
                mipLevel.Data = new float[mipLevel.Width * mipLevel.Height * 4];
                int index = 0;
                ExtractMipmapLevel(yOffset, mipLevel, xOffset, bmp, index);
                mipmapLevels.Add(mipLevel);

                if (width == 1 && height == 1)
                {
                    maxMipmapLevel = m;
                    break;
                }

                width /= 2;
                if (width < 1)
                    width = 1;
                height /= 2;
                if (height < 1)
                    height = 1;
            }

            return mipmapLevels;
        }


        private static void ExtractMipmapLevel(int yOffset, MipLevel mipLevel, int xOffset, Bitmap bmp, int index)
        {
            var width = xOffset + mipLevel.Width;
            var height = yOffset + mipLevel.Height;
            for (int y = yOffset; y < height; y++)
            {
                for (int x = xOffset; x < width; x++)
                {
                    var pixel = bmp.GetPixel(x, y);
                    mipLevel.Data[index++] = pixel.R / 255f;
                    mipLevel.Data[index++] = pixel.G / 255f;
                    mipLevel.Data[index++] = pixel.B / 255f;
                    mipLevel.Data[index++] = pixel.A / 255f;
                }
            }
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, Id);
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


    // List of textures

    public class GLTextureList : IDisposable
    {
        private Dictionary<string, IGLTexture> textures;
        private int unnamed = 0;

        public GLTextureList()
        {
            textures = new Dictionary<string, IGLTexture>();
        }

        public IGLTexture Add(string name, IGLTexture r)
        {
            textures.Add(name, r);
            return r;
        }

        public IGLTexture Add(IGLTexture r)
        {
            textures.Add("Unnamed_" + (unnamed++), r);
            return r;
        }

        public IGLTexture this[string key] { get { return textures[key]; } }
        public bool Contains(string key) { return textures.ContainsKey(key); }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (IGLTexture r in textures.Values)
                    r.Dispose();

                textures.Clear();
            }
        }
    }


}


// stuff does not work - keep for now until we decide the above method is good

//var data = LoadTexture(bmp);
//GL.TextureStorage2D(
//    Id,
//    1,                           // levels of mipmapping
//    SizedInternalFormat.Rgba32f, // format of texture
//    Width,
//    Height);
//GL.TextureSubImage2D(Id,
//    0,                  // this is level 0
//    0,                  // x offset
//    0,                  // y offset
//    Width,
//    Height,
//    OpenTK.Graphics.OpenGL4.PixelFormat.Rgba,
//    PixelType.Float,
//    data);
//private float[] UnusedLoadTexture(Bitmap bmp)
//{
//    float[] r;
//    r = new float[bmp.Width * bmp.Height * 4];
//    int index = 0;
//    BitmapData data = null;
//    try
//    {
//        data = bmp.LockBits(
//            new Rectangle(0, 0, bmp.Width, bmp.Height),
//            ImageLockMode.ReadOnly,
//            System.Drawing.Imaging.PixelFormat.Format24bppRgb);
//        unsafe
//        {

//            // fix this to load better

//            var ptr = (byte*)data.Scan0;
//            int remain = data.Stride - data.Width * 3;
//            for (int i = 0; i < data.Height; i++)
//            {
//                for (int j = 0; j < data.Width; j++)
//                {
//                    r[index++] = ptr[2] / 255f;
//                    r[index++] = ptr[1] / 255f;
//                    r[index++] = ptr[0] / 255f;
//                    r[index++] = 1f;
//                    ptr += 3;
//                }
//                ptr += remain;
//            }
//        }
//    }
//    finally
//    {
//        bmp.UnlockBits(data);
//    }

//    return r;
//}