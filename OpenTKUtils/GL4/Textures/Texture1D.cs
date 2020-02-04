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
    public class GLTexture1D : GLTextureBase          
    {
        public GLTexture1D()
        {
        }

        public GLTexture1D( int width, SizedInternalFormat internalformat = SizedInternalFormat.Rgba32f, int mipmaplevel = 1)
        {
            CreateTexture(width, internalformat, mipmaplevel);
        }

        public void CreateTexture(int width, SizedInternalFormat internalformat = SizedInternalFormat.Rgba32f, int mipmaplevel = 1)
        {
            InternalFormat = internalformat;
            Width = width; 

            GL.CreateTextures(TextureTarget.Texture1D, 1, out int id);
            Id = id;

            GL.TextureStorage1D(
                Id,
                mipmaplevel,    // levels of mipmapping.  If we supplied one, we use that, else we use genmipmaplevel
                InternalFormat,                       // format of texture - 4 floats is the normal, and is given in the constructor
                Width);
        }

        public void Store(int xoffset, int width, PixelFormat px, PixelType ty, IntPtr ptr)
        {
            GL.TextureSubImage1D(Id, 0, xoffset, width, px, ty, ptr);
        }

        public void Store(int xoffset, int width, Byte[] array, PixelFormat px = PixelFormat.Bgra)
        {
            GL.TextureSubImage1D(Id, 0, xoffset, width, px, PixelType.UnsignedByte, array);
        }

        public void Store(int xoffset, int width, float[] array, PixelFormat px = PixelFormat.Bgra)
        {
            GL.TextureSubImage1D(Id, 0, xoffset, width, px, PixelType.Float, array);
        }

    }
}

