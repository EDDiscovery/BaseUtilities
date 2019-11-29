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
using System.Drawing;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    public class GLTexture3D : GLTextureBase         // load a texture into open gl
    {
        public int Depth { get; private set; }      // z

        public GLTexture3D( int width, int height, int depth, SizedInternalFormat internalformat = SizedInternalFormat.Rgba32f, int mipmaplevels = 1)
        {
            InternalFormat = internalformat;
            Width = width; Height = height; Depth = depth;

            GL.CreateTextures(TextureTarget.Texture3D, 1, out int id);
            Id = id;

            GL.TextureStorage3D(Id, mipmaplevels, InternalFormat, Width, Height, Depth);
        }

        // Write to a Z plane the X/Y info.

        // you can use PixelFormat = Red just to store a single float, and then use texture(tex,vec3(x,y,z)) to pick it up - only .x is applicable
        // if you have an rgba, and you store to a single plane using PixelFormat, the other planes are wiped! beware.

        public void StoreZPlane(int zcoord, int xoffset, int yoffset, int width, int height, PixelFormat px, PixelType ty, IntPtr ptr)
        {
            GL.TextureSubImage3D(Id, 0, xoffset, yoffset, zcoord, width, height, 1, px, ty, ptr);
        }

        public void StoreZPlane(int zcoord, int xoffset, int yoffset, int width, int height, Byte[] array, PixelFormat px = PixelFormat.Bgra)    
        {
            GL.TextureSubImage3D(Id, 0, xoffset, yoffset, zcoord, width, height, 1, px, PixelType.UnsignedByte, array);
        }

        public void StoreZPlane(int zcoord, int xoffset, int yoffset, int width, int height, float[] array, PixelFormat px = PixelFormat.Bgra)      
        {
            GL.TextureSubImage3D(Id, 0, xoffset, yoffset, zcoord, width, height, 1, px, PixelType.Float, array);
        }

        // only if internal format is RGBA data.
        // either make bitmapmipmaplevels = set up mip map levels, or use GenMipMapTextures() after all is loaded
        public void StoreBitmapZPlane(Bitmap map, int zoffset, int bitmapmipmaplevels = 1)      
        {
            LoadBitmap(Id, map, bitmapmipmaplevels, zoffset);
        }

        public void GenMipMapTextures()     // only call if mipmaplevels > 1
        {
            GL.GenerateTextureMipmap(Id);
        }

    }

}

