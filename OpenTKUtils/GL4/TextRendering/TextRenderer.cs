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


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace OpenTKUtils.GL4
{
    public class GLTextRenderer : IDisposable
    {
        public GLRenderableItem RenderableItem { get; private set; }
        public GLShaderPipeline Shader { get; private set; }

        public int Count { get { return entries.Count; } }
        public int Max { get; private set; }
        public int Deleted { get; private set; }
        public bool AutoResize { get; set; } = false;
        public int MipMapLevel { get; set; } = 3;
        public bool TextureDirty { get; set; } = false;

        private GLTexture2DArray textures;
        private Size bitmapsize;
        private GLBuffer matrixbuffer;

        private struct EntryInfo
        {
            public Bitmap bitmap;
            public Object tag;
            public string tagstring;
        }

        private List<EntryInfo> entries = new List<EntryInfo>();

        private GLItemsList items = new GLItemsList();      // we have our own item list, which is disposed when we dispose

        public GLTextRenderer(Size bitmapp, int max, bool cullface)
        {
            bitmapsize = bitmapp;
            Max = max;
            Deleted = 0;

            Shader = new GLShaderPipeline(new GLPLVertexShaderQuadTextureWithMatrixTranslation(), new GLPLFragmentShaderTexture2DIndexed(0));
            items.Add(Shader);
            Shader.StartAction += (s) => { if (TextureDirty) CreateTexture(); textures.Bind(1); };

            textures = new GLTexture2DArray();
            items.Add(textures);

            matrixbuffer = new GLBuffer();
            items.Add(matrixbuffer);

            matrixbuffer.AllocateBytes(Max * GLLayoutStandards.Mat4size);
            matrixbuffer.AddPosition(0);        // CreateMatrix4 needs to have a position

            var rc = GLRenderControl.Quads();
            rc.CullFace = cullface;
            rc.ClipDistanceEnable = 1;
            RenderableItem = GLRenderableItem.CreateMatrix4(items, rc, matrixbuffer, 4);
        }

        public Bitmap Add(object tag,
                                    string text, Font f, Color fore, Color back,
                                    Vector3 worldpos,
                                    Vector3 size,       // Note if Y and Z are zero, then Z is set to same ratio to width as bitmap
                                    Vector3 rotationradians,
                                    StringFormat fmt = null, float backscale = 1.0f,
                                    bool rotatetoviewer = false, bool rotateelevation = false   // if set, rotationradians not used
                                ) 
        {
            if (size.Z == 0 && size.Y == 0)
            {
                size.Z = size.X * (float)bitmapsize.Height / (float)bitmapsize.Width;       // autoscale to bitmap ratio
            }

            int pos = entries.Count;

            if ( pos >= Max)
            {
                if (AutoResize)
                    Resize(Max * 2);
                else
                    System.Diagnostics.Debug.Assert(false, "Exceeded size of TextRender Max");
            }

            Bitmap bmp = BitMapHelpers.DrawTextIntoFixedSizeBitmapC(text, bitmapsize, f, fore, back, backscale, false, fmt);
            entries.Add(new EntryInfo() { bitmap = bmp, tag = tag, tagstring = tag is string ? (string)tag : null });

            Matrix4 mat = Matrix4.Identity;
            mat = Matrix4.Mult(mat, Matrix4.CreateScale(size));
            if (rotatetoviewer == false)
            {
                mat = Matrix4.Mult(mat, Matrix4.CreateRotationX(rotationradians.X));
                mat = Matrix4.Mult(mat, Matrix4.CreateRotationY(rotationradians.Y));
                mat = Matrix4.Mult(mat, Matrix4.CreateRotationZ(rotationradians.Z));
            }
            mat = Matrix4.Mult(mat, Matrix4.CreateTranslation(worldpos));
            mat[0, 3] = pos;     // store pos of image in stack
            mat[1, 3] = rotatetoviewer ? (rotateelevation ? 2 : 1) : 0;  // and rotation selection

            System.Diagnostics.Debug.WriteLine("Pos {0} Matrix {1}", pos, mat);
            matrixbuffer.StartWrite(GLLayoutStandards.Mat4size * pos);
            matrixbuffer.Write(mat);
            matrixbuffer.StopReadWrite();

            TextureDirty = true;

            return bmp;
        }

        public bool Remove(Object tag)
        {
            int indexof = Array.IndexOf(entries.Select(x => tag is string ? x.tagstring : x.tag).ToArray(), tag);
            return RemoveAt(indexof);
        }

        public bool RemoveAt(int indexof)
        {
            if (indexof >= 0 && indexof < entries.Count)
            {
                entries[indexof].bitmap.Dispose();
                entries[indexof] = new EntryInfo(); // all will be null

                Matrix4 zero = Matrix4.Identity;      // set ctrl 1,3 to -1 to indicate cull matrix
                zero[1, 3] = -1;                        // if it did not work, it would appear at (0,0,0)
                matrixbuffer.StartWrite(GLLayoutStandards.Mat4size * indexof, GLLayoutStandards.Mat4size);
                matrixbuffer.Write(zero);
                matrixbuffer.StopReadWrite();
                Deleted++;
                return true;
            }
            else
                return false;
        }

        public void Resize(int newmax = 0)                  // if newmax>0, set new max limit.
        {
            if ( Deleted>0 || (newmax>0 && newmax!=Max) )
            {
                if (newmax > 0)
                    Max = newmax;
                    
                GLBuffer newbuffer = new GLBuffer(Max * GLLayoutStandards.Mat4size);
                List<EntryInfo> newentries = new List<EntryInfo>();

                newbuffer.StartWrite(0);
                matrixbuffer.StartRead(0);

                foreach( var e in entries)
                {
                    Matrix4 m = matrixbuffer.ReadMatrix4();

                    if ( e.bitmap != null )
                    {
                        newentries.Add(new EntryInfo() { bitmap = e.bitmap, tag = e.tag, tagstring = e.tagstring });
                        newbuffer.Write(m);
                    }
                }

                newbuffer.StopReadWrite();
                matrixbuffer.StopReadWrite();

                items.Dispose(matrixbuffer);
                matrixbuffer = newbuffer;
                entries = newentries;

                matrixbuffer.AddPosition(0);        // CreateMatrix4 needs to have a position

                RenderableItem.CreateMatrix4(items, matrixbuffer);
                Deleted = 0;

                TextureDirty = true;

                GLStatics.Check();
            }
        }

        public void Dispose()           // you can double dispose.
        {
            items.Dispose();
        }

        private void CreateTexture()       // called when texture needs updating due to change
        {
            textures.OwnBitmaps = false;        // we are reloading the bitmaps, so we set this false so the dispose will not delete them during LoadBitmaps
            var barray = entries.Select(x => x.bitmap).ToArray();
            textures.LoadBitmaps(barray, genmipmaplevel: MipMapLevel, ownbitmaps: true);   // then we tell it that it owns the bitmaps.
            RenderableItem.InstanceCount = entries.Count;
            TextureDirty = false;
        }

    }

}

