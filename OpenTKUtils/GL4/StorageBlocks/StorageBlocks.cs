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
    // local data block, writable in Vectors etc, supports std140 and std430

    public abstract class GLLayoutStandards
    {
        protected int size;
        protected byte[] bufferdata;

        // std140 alignment: scalars are N, 2Vec = 2N, 3Vec = 4N, 4Vec = 4N. Array alignment is vec4, stride vec4
        // std430 alignment: scalars are N, 2Vec = 2N, 3Vec = 4N, 4Vec = 4N. Array alignment is same as left, stride is as per left.

        private int arraystridealignment;

        public GLLayoutStandards(int isz = 64, bool std430 = false)
        {
            size = 0;
            bufferdata = new byte[isz];
            arraystridealignment = std430 ? 4 : 16;               // if in std130, arrays have vec4 alignment and strides are vec4
        }

        public abstract void WriteAreaToBuffer(int fillpos, int datasize);

        // Write to the local copy - pos = -1 place at end, else place at position (for update)
        // Set writebuf to write to buffer as well - only use for updating

        public int Write(float f, int pos = -1, bool writebuffer = false)
        {
            float[] fa = new float[] { f };
            int fillpos = Start(sizeof(float), pos, 4);                 // this is purposely done on another statement as writebuffer may change during this function
            return Finish(fa, fillpos, sizeof(float), writebuffer);
        }

        //TBD not sure they are not separ by vec4 in strides

        public int Write(float[] v, int pos = -1, bool writebuffer = false)
        {
            int sz = sizeof(float) * v.Length;
            int fillpos = Start(Math.Max(arraystridealignment, sizeof(float)), pos, sz);
            return Finish(v, fillpos, sz, writebuffer);
        }

        public int Write(int i, int pos = -1, bool writebuffer = false)
        {
            int sz = sizeof(int);
            int[] ia = new int[] { i };
            int fillpos = Start(sz, pos, sz);
            return Finish(ia, fillpos, sz, writebuffer);
        }

        public int Write(Vector2 v, int pos = -1, bool writebuffer = false)
        {
            int sz = sizeof(float) * 2;
            float[] fa = new float[] { v.X, v.Y };
            int fillpos = Start(sz, pos, sz);
            return Finish(fa, fillpos, sz, writebuffer);
        }

        public int Write(Vector2[] vec, int pos = -1, bool writebuffer = false)
        {
            float[] array = new float[vec.Length * 2];
            int fill = 0;
            foreach (var v in vec)
            {
                array[fill++] = v.X;
                array[fill++] = v.Y;
                if (arraystridealignment == 16)
                    fill += 2;                     // std130 has vec2 padded to vec4 length - page 123 last paragraph
            }

            int sz = sizeof(float) * array.Length;
            int fillpos = Start(Math.Max(arraystridealignment, sizeof(float) * 2), pos, sz);
            return Finish(array, fillpos, sz, writebuffer);
        }

        public int Write(Vector3 v, int pos = -1, bool writebuffer = false)
        {
            float[] fa = new float[] { v.X, v.Y, v.Z };
            int fillpos = Start(sizeof(float) * 4, pos, sizeof(float) * 3);
            return Finish(fa, fillpos, sizeof(float) * 3, writebuffer);
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
                fill++;                     // vec3 padded to vec4 length always - page 123 last paragraph, opengl page 138
            }

            int sz = sizeof(float) * array.Length;
            int fillpos = Start(Math.Max(arraystridealignment, sizeof(float) * 4), pos, sz);
            return Finish(array, fillpos, sz, writebuffer);
        }

        public int Write(Vector4 v, int pos = -1, bool writebuffer = false)
        {
            int sz = sizeof(float) * 4;
            float[] fa = new float[] { v.X, v.Y, v.Z, v.W };
            int fillpos = Start(sz, pos, sz);
            return Finish(fa, fillpos, sz, writebuffer);
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
            int fillpos = Start(Math.Max(arraystridealignment, sizeof(float) * 4), pos, sz);
            return Finish(array, fillpos, sz, writebuffer);
        }

        private int Start(int align, int pos, int datasize)       // if pos == -1 move size to alignment of N, else just return pos
        {
            //initialsize size incr
            if (pos == -1)
            {
                size = (size + align - 1) & (~(align - 1));

                if (size + datasize > bufferdata.Length)
                {
                    int nextsize = size + datasize + 512;
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
                WriteAreaToBuffer(fillpos, datasize);

            return fillpos;
        }

    }

    // Class supports writing to local data then to the buffer object.

    public abstract class GLDataBlock : GLLayoutStandards, IDisposable
    {
        public int BindingIndex { get; private set; }

        private int Id;
        private BufferTarget target;

        //public GLDataBlock(int bp, bool std430, int isz, BufferTarget tg = BufferTarget.UniformBuffer, BufferRangeTarget tgr = BufferRangeTarget.UniformBuffer) : base(isz)
        public GLDataBlock(int bindingindex, bool std430, int startsize, BufferTarget tg, BufferRangeTarget tgr) : base(startsize, std430)
        {
            BindingIndex = bindingindex;
            target = tg;

            Id = GL.GenBuffer();
            GL.BindBuffer(target, Id);
            GL.BindBufferBase(tgr, BindingIndex, Id);            // binding point
            GL.BindBuffer(target, 0);
        }

        // must call at least one.. from then on you can update using writes..
        public void Complete()
        {
            GL.BindBuffer(target, Id);
            GL.BufferData(target, size, (IntPtr)0, BufferUsageHint.DynamicDraw);
            // want to write, and the previous contents may be thrown away https://www.khronos.org/registry/OpenGL-Refpages/gl4/
            IntPtr ptr = GL.MapBufferRange(target, (IntPtr)0, size, BufferAccessMask.MapWriteBit | BufferAccessMask.MapInvalidateBufferBit);
            System.Runtime.InteropServices.Marshal.Copy(bufferdata, 0, ptr, size);
            GL.UnmapBuffer(target);
            GL.BindBuffer(target, 0);
            GL4Statics.Check();
        }

        // rewrite the whole thing.. Complete must be called first.  Use after Writes without the immediate write buffer
        public void Update()
        {
            GL.BindBuffer(target, Id);
            // want to write, and the previous contents may be thrown away https://www.khronos.org/registry/OpenGL-Refpages/gl4/
            IntPtr ptr = GL.MapBufferRange(target, (IntPtr)0, size, BufferAccessMask.MapWriteBit | BufferAccessMask.MapInvalidateBufferBit);
            System.Runtime.InteropServices.Marshal.Copy(bufferdata, 0, ptr, size);
            GL.UnmapBuffer(target);
            GL.BindBuffer(target, 0);
        }

        public void Dispose()           // you can double dispose.
        {
            if (Id != -1)
            {
                GL.DeleteBuffer(Id);
                Id = -1;
            }
        }

        public override void WriteAreaToBuffer(int fillpos, int datasize)
        {
            IntPtr ptr = GL.MapNamedBufferRange(Id, (IntPtr)fillpos, datasize, BufferAccessMask.MapWriteBit | BufferAccessMask.MapInvalidateRangeBit);
            System.Runtime.InteropServices.Marshal.Copy(bufferdata, fillpos, ptr, datasize);
            GL.UnmapNamedBuffer(Id);
        }

    }

    // uniform blocks - std140 only
    public class GLUniformBlock : GLDataBlock
    {
        public GLUniformBlock(int bindingindex, int defaultsize = 64) : base(bindingindex, false, defaultsize, BufferTarget.UniformBuffer, BufferRangeTarget.UniformBuffer)
        {

        }

    }

    // storage blocks - std140 and 430
    public class GLStorageBlock : GLDataBlock
    {
        public GLStorageBlock(int bindingindex, bool std430 = false, int defaultsize = 64) : base(bindingindex, std430, defaultsize, BufferTarget.ShaderStorageBuffer, BufferRangeTarget.ShaderStorageBuffer)
        {
        }
    }
}

