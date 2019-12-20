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
    public class GLTexture2D : GLTextureBase          // load a texture into open gl
    {
        public GLTexture2D(Bitmap bmp, int mipmaplevel = 1, SizedInternalFormat internalformat = SizedInternalFormat.Rgba32f, int genmipmaplevel = 1, bool ownbitmaps = false)
        {
            InternalFormat = internalformat;
            BitMaps = new Bitmap[1];
            BitMaps[0] = bmp;
            OwnBitmaps = ownbitmaps;

            GL.CreateTextures(TextureTarget.Texture2D, 1, out int id);
            Id = id;

            Width = bmp.Width;
            Height = (mipmaplevel == 1) ? bmp.Height : (bmp.Height / 3) * 2;        // if bitmap is mipped mapped, work out correct height.

            GL.TextureStorage2D(
                Id,
                mipmaplevel == 1 ? genmipmaplevel : mipmaplevel,    // levels of mipmapping.  If we supplied one, we use that, else we use genmipmaplevel
                InternalFormat,                       // format of texture - 4 floats is the normal, and is given in the constructor
                bmp.Width,                            // width and height of mipmap level 0
                Height);

            LoadBitmap(Id, bmp, mipmaplevel, -1);    // use common load into bitmap, indicating its a 2D texture so use texturesubimage2d

            if (mipmaplevel == 1 && genmipmaplevel > 1)     // single level mipmaps with genmipmap levels > 1 get auto gen
                GL.GenerateTextureMipmap(Id);

            var textureMinFilter = (int)All.LinearMipmapLinear;
            GL.TextureParameterI(Id, TextureParameterName.TextureMinFilter, ref textureMinFilter);
            var textureMagFilter = (int)All.Linear;
            GL.TextureParameterI(Id, TextureParameterName.TextureMagFilter, ref textureMagFilter);
        }
    }
}

