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

using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace OpenTKUtils.GL4
{
    public class GLTexture : IDisposable           // load a texture into open gl
    {
        private int Id;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public GLTexture(Bitmap bmp)
        {
            Width = bmp.Width;
            Height = bmp.Height;

            GL.CreateTextures(TextureTarget.Texture2D, 1, out Id);

            GL.GenTextures(1, out Id);
            GL.BindTexture(TextureTarget.Texture2D, Id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            System.Drawing.Imaging.BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpdata.Width, bmpdata.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bmpdata.Scan0);
            bmp.UnlockBits(bmpdata);

            GL.Disable(EnableCap.Texture2D);
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

    public class GLTextureList : IDisposable
    {
        private Dictionary<string, GLTexture> textures;
        private int unnamed = 0;

        public GLTextureList()
        {
            textures = new Dictionary<string, GLTexture>();
        }

        public GLTexture Add(string name, GLTexture r)
        {
            textures.Add(name, r);
            return r;
        }

        public GLTexture Add(GLTexture r)
        {
            textures.Add("Unnamed_" + (unnamed++), r);
            return r;
        }

        public GLTexture this[string key] { get { return textures[key]; } }
        public bool Contains(string key) { return textures.ContainsKey(key); }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (GLTexture r in textures.Values)
                    r.Dispose();

                textures.Clear();
            }
        }
    }


}
