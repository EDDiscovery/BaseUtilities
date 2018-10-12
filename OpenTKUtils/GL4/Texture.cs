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
using System.Drawing;
using System.Drawing.Imaging;

namespace OpenTKUtils.GL4
{
    public class Texture : IDisposable           // load a texture into open gl
    {
        private int Id;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Texture(Bitmap bmp)
        {
            Width = bmp.Width;
            Height = bmp.Height;

            var data = LoadTexture(bmp);

            GL.CreateTextures(TextureTarget.Texture2D, 1, out Id);
            GL.TextureStorage2D(
                Id,
                1,                           // levels of mipmapping
                SizedInternalFormat.Rgba32f, // format of texture
                Width,
                Height);

            GL.BindTexture(TextureTarget.Texture2D, Id);
            GL.TextureSubImage2D(Id,
                0,                  // this is level 0
                0,                  // x offset
                0,                  // y offset
                Width,
                Height,
                OpenTK.Graphics.OpenGL4.PixelFormat.Rgba,
                PixelType.Float,
                data);

            GL.Disable(EnableCap.Texture2D);
        }

        private float[] LoadTexture(Bitmap bmp)
        {
            float[] r;
            r = new float[bmp.Width * bmp.Height * 4];
            int index = 0;
            BitmapData data = null;
            try
            {
                data = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                unsafe
                {
                    var ptr = (byte*)data.Scan0;
                    int remain = data.Stride - data.Width * 3;
                    for (int i = 0; i < data.Height; i++)
                    {
                        for (int j = 0; j < data.Width; j++)
                        {
                            r[index++] = ptr[2] / 255f;
                            r[index++] = ptr[1] / 255f;
                            r[index++] = ptr[0] / 255f;
                            r[index++] = 1f;
                            ptr += 3;
                        }
                        ptr += remain;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(data);
            }

            return r;
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

}
