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

using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    public class GLUniformBlock : IDisposable
    {
        public int BindingIndex { get; private set;  }

        private int Id;
        private int size;
        private byte[] bufferdata;

        public GLUniformBlock(int bp, int isz= 64)
        {
            BindingIndex = bp;
            size = 0;
            bufferdata = new byte[isz];

            Id = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, Id);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, BindingIndex, Id);            // binding point
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        // Write to the local copy - pos = -1 place at end, else place at position (for update)
        // Set writebuf to write to buffer as well - only use for updating

        public int Write(float f, int pos = -1, bool writebuffer = false)
        {
            float[] fa = new float[] { f };
            int fillpos = Start(4, pos, 4);
            return Finish(fa, fillpos,4, writebuffer);
        }

        public int Write(float[] v, int pos = -1, bool writebuffer = false)
        {
            int sz = 4 * v.Length;
            int fillpos = Start(16, pos, sz);
            return Finish(v, fillpos, sz, writebuffer);
        }

        public int Write(int i, int pos = -1, bool writebuffer = false)
        {
            int[] ia = new int[] { i };
            int fillpos = Start(4, pos , 4);
            return Finish(ia, fillpos,4,writebuffer);
        }

        public int Write(Vector2 v, int pos = -1, bool writebuffer = false)
        {
            float[] fa = new float[] { v.X, v.Y };
            int fillpos = Start(8, pos, 8);
            return Finish(fa, fillpos, 8, writebuffer);
        }

        public int Write(Vector2[] vec, int pos = -1, bool writebuffer = false)
        {
            float[] array = new float[vec.Length * 4];
            int fill = 0;
            foreach (var v in vec)
            {
                array[fill++] = v.X;
                array[fill++] = v.Y;
                fill += 2;                     // vec2 padded to vec4 length - page 123 last paragraph
            }

            int sz = 4 * array.Length;
            int fillpos = Start(16, pos, sz);
            return Finish(array, fillpos, sz, writebuffer);
        }

        public int Write(Vector3 v, int pos = -1, bool writebuffer = false)
        {
            float[] fa = new float[] { v.X, v.Y, v.Z };
            int fillpos = Start(16, pos, 12);
            return Finish(fa, fillpos, 12, writebuffer);
        }

        public int Write(Vector3[] vec, int pos = -1, bool writebuffer = false)
        {
            float[] array = new float[vec.Length * 4];
            int fill = 0;
            foreach (var v in vec)
            {
                array[fill++] = v.X;
                array[fill++] = v.Y;
                array[fill++] = v.Z;
                fill++;                     // vec3 padded to vec4 length - page 123 last paragraph
            }

            int sz = 4 * array.Length;
            int fillpos = Start(16, pos, sz);
            return Finish(array, fillpos, sz, writebuffer);
        }

        public int Write(Vector4 v, int pos = -1, bool writebuffer = false)
        {
            float[] fa = new float[] { v.X, v.Y, v.Z, v.W };
            int fillpos = Start(16, pos, 16);
            return Finish(fa, fillpos, 16, writebuffer);
        }

        public int Write(Vector4[] vec, int pos = -1, bool writebuffer = false)
        {
            float[] array = new float[vec.Length * 4];
            int fill = 0;
            foreach (var v in vec)
            {
                array[fill++] = v.X;
                array[fill++] = v.Y;
                array[fill++] = v.Z;
                array[fill++] = v.W;
            }

            int sz = 4 * array.Length;
            int fillpos = Start(16, pos, sz);
            return Finish(array, fillpos, sz, writebuffer);
        }

        // must call at least one.. from then on you can update using writes..
        public void Complete()
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, Id);
            GL.BufferData(BufferTarget.UniformBuffer, size, (IntPtr)0, BufferUsageHint.DynamicDraw);
            IntPtr ptr = GL.MapBufferRange(BufferTarget.UniformBuffer, (IntPtr)0, size, BufferAccessMask.MapWriteBit | BufferAccessMask.MapInvalidateBufferBit);
            System.Runtime.InteropServices.Marshal.Copy(bufferdata, 0, ptr, size);
            GL.UnmapBuffer(BufferTarget.UniformBuffer);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
            GL4Statics.Check();
        }

        public void Dispose()           // you can double dispose.
        {
            if (Id != -1)
            {
                GL.DeleteBuffer(Id);
                Id = -1;
            }
        }

        private int Start(int align, int pos, int datasize)       // if pos == -1 move size to alignment of N, else just return pos
        {
            //initialsize size incr
            if (pos == -1)
            {
                size = (size + align - 1) & (~(align - 1));

                if (size + datasize > bufferdata.Length)
                {
                    int nextsize = bufferdata.Length + datasize + 512;
                    byte[] buf2 = new byte[nextsize];
                    Array.Copy(bufferdata, buf2, size);
                    bufferdata = buf2;
                }

                pos = size;
                size += datasize;
            }

            return pos;
        }

        private int Finish(Array data, int fillpos, int datasize, bool writebuffer)
        {
            System.Buffer.BlockCopy(data, 0, bufferdata, fillpos, datasize);

            if (writebuffer)
            {
                IntPtr ptr = GL.MapNamedBufferRange(Id, (IntPtr)fillpos, datasize, BufferAccessMask.MapWriteBit | BufferAccessMask.MapInvalidateBufferBit);
                System.Runtime.InteropServices.Marshal.Copy(bufferdata, fillpos, ptr, datasize);
                GL.UnmapNamedBuffer(Id);
            }

            return fillpos;
        }


    }

}

