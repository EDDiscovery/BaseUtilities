/*yes.
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
using OpenTK;

namespace OpenTKUtils.GL4
{
    // implements open GL standards on writing data to a GLBuffer.
    // this class operates a local cache.
    // Normal operation involves making a GLBuffer, use these to write to the cache, then call GLBuffer.Complete() to upload it to GL
    
    public abstract class GLLayoutStandards
    {
        public int CurrentPos { get; protected set; } = 0;
        public int BufferSize { get; protected set; } = 0;      // 0 means not complete and allocated, otherwise allocated to this size.
        public bool IsAllocated { get { return BufferSize != 0; } }
        public bool NotAllocated { get { return BufferSize == 0; } }
        public List<int> Positions = new List<int>();           // at each alignment, a position is stored

        public const int Vec4size = 4 * sizeof(float);
        public const int Vec2size = 2 * sizeof(float);
        public const int Mat4size = 4 * 4 * sizeof(float);

        protected byte[] cachebufferdata = null;

        // std140 alignment: scalars are N, 2Vec = 2N, 3Vec = 4N, 4Vec = 4N. Array alignment is vec4, stride vec4
        // std430 alignment: scalars are N, 2Vec = 2N, 3Vec = 4N, 4Vec = 4N. Array alignment is same as left, stride is as per left.

        private int arraystridealignment;

        public GLLayoutStandards(bool std430 = false)
        {
            CurrentPos = 0;
            BufferSize = 0;
            cachebufferdata = null;
            arraystridealignment = std430 ? sizeof(float) : Vec4size;               // if in std130, arrays have vec4 alignment and strides are vec4
        }

        #region Write to cache area..  then call Complete() to store to GL buffer.

        // Write to the local copy - pos = -1 place at end, else place at position (for update)
        // Set writebuf to write to buffer as well - only use for updating

        public int Write(float f, int pos = -1, bool writebuffer = false)
        {
            float[] fa = new float[] { f };
            int fillpos = WriteCacheStart(sizeof(float), pos, sizeof(float));                 // this is purposely done on another statement as writebuffer may change during this function
            return WriteCacheFinish(fa, fillpos, sizeof(float), writebuffer);
        }

        //TBD not sure they are not separ by vec4 in strides

        public int Write(float[] v, int pos = -1, bool writebuffer = false)
        {
            int sz = sizeof(float) * v.Length;
            int fillpos = WriteCacheStart(Math.Max(arraystridealignment, sizeof(float)), pos, sz);
            return WriteCacheFinish(v, fillpos, sz, writebuffer);
        }

        public int Write(int i, int pos = -1, bool writebuffer = false)
        {
            int sz = sizeof(int);
            int[] ia = new int[] { i };
            int fillpos = WriteCacheStart(sz, pos, sz);
            return WriteCacheFinish(ia, fillpos, sz, writebuffer);
        }

        public int Write(Vector2 v, int pos = -1, bool writebuffer = false)
        {
            float[] fa = new float[] { v.X, v.Y };
            int fillpos = WriteCacheStart(Vec2size, pos, Vec2size);
            return WriteCacheFinish(fa, fillpos, Vec2size, writebuffer);
        }

        public int Write(Vector2[] vec, int pos = -1, bool writebuffer = false)
        {
            float[] array = new float[vec.Length * 2];
            int fill = 0;
            foreach (var v in vec)
            {
                array[fill++] = v.X;
                array[fill++] = v.Y;
                if (arraystridealignment == Vec4size)
                    fill += 2;                     // std130 has vec2 padded to vec4 length - page 123 last paragraph
            }

            int sz = sizeof(float) * array.Length;
            int fillpos = WriteCacheStart(Math.Max(arraystridealignment, Vec2size), pos, sz);
            return WriteCacheFinish(array, fillpos, sz, writebuffer);
        }

        public int Write(Vector3 v, int pos = -1, bool writebuffer = false)
        {
            int sz = sizeof(float) * 3;
            float[] fa = new float[] { v.X, v.Y, v.Z };
            int fillpos = WriteCacheStart(Vec4size, pos, sz);
            return WriteCacheFinish(fa, fillpos, sz, writebuffer);
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
            int fillpos = WriteCacheStart(Math.Max(arraystridealignment, Vec4size), pos, sz);
            return WriteCacheFinish(array, fillpos, sz, writebuffer);
        }

        public int Write(Vector4 v, int pos = -1, bool writebuffer = false)
        {
            float[] fa = new float[] { v.X, v.Y, v.Z, v.W };
            int fillpos = WriteCacheStart(Vec4size, pos, Vec4size);
            return WriteCacheFinish(fa, fillpos, Vec4size, writebuffer);
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

            int sz = sizeof(float) * array.Length;
            int fillpos = WriteCacheStart(Math.Max(arraystridealignment, Vec4size), pos, sz);
            return WriteCacheFinish(array, fillpos, sz, writebuffer);
        }

        private void Write(float[] array, int pos, Matrix4 mat)
        {
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    array[pos++] = mat[r, c];
        }

        public int Write(Matrix4 mat, int pos = -1, bool writebuffer = false)
        {
            float[] array = new float[4 * 4];
            Write(array, 0, mat);
            int sz = sizeof(float) * array.Length;
            int fillpos = WriteCacheStart(Math.Max(arraystridealignment, Vec4size), pos, sz);
            return WriteCacheFinish(array, fillpos, sz, writebuffer);
        }

        public int WriteTranslationMatrix(Vector3 trans, int pos = -1, bool writebuffer = false)
        {
            Matrix4 mat = Matrix4.CreateTranslation(trans);
            return Write(mat, pos, writebuffer);
        }

        public int WriteTranslationRotationMatrix(Vector3 trans, Vector3 rot, int pos = -1, bool writebuffer = false)   // IN RADIANS
        {
            Matrix4 mat = Matrix4.CreateRotationX(rot.X);
            mat *= Matrix4.CreateRotationX(rot.Y);
            mat *= Matrix4.CreateRotationX(rot.Z);
            mat *= Matrix4.CreateTranslation(trans);
            return Write(mat, pos, writebuffer);
        }

        public int WriteTranslationRotationDegMatrix(Vector3 trans, Vector3 rot, int pos = -1, bool writebuffer = false)   // IN DEF
        {
            Matrix4 mat = Matrix4.CreateRotationX(rot.X.Radians());
            mat *= Matrix4.CreateRotationX(rot.Y.Radians());
            mat *= Matrix4.CreateRotationX(rot.Z.Radians());
            mat *= Matrix4.CreateTranslation(trans);
            System.Diagnostics.Debug.WriteLine("mat " + mat);
            return Write(mat, pos, writebuffer);
        }

        public int Write(Matrix4[] vec, int pos = -1, bool writebuffer = false)
        {
            float[] array = new float[vec.Length * 4 * 4];
            int fill = 0;
            foreach (var v in vec)
            {
                Write(array, fill, v);
                fill += 16;
            }

            int sz = sizeof(float) * array.Length;
            int fillpos = WriteCacheStart(Math.Max(arraystridealignment, Vec4size), pos, sz);
            return WriteCacheFinish(array, fillpos, sz, writebuffer);
        }

        #endregion

        #region Implementation

        private int WriteCacheStart(int align, int pos, int datasize)       // if pos == -1 move size to alignment of N, else just return pos
        {
            if (pos == -1)
            {
                CurrentPos = (CurrentPos + align - 1) & (~(align - 1));     // align..

                System.Diagnostics.Debug.Assert(BufferSize == 0 || CurrentPos + datasize <= BufferSize); // need either an uncommitted buffer, or within buffersize

                if (cachebufferdata == null || CurrentPos + datasize > cachebufferdata.Length)  // if need to make or grow cache
                {
                    int newsize = CurrentPos + datasize + 512;      // 512 is extra..
                    byte[] buf2 = new byte[newsize];
                    if (cachebufferdata != null)
                        Array.Copy(cachebufferdata, buf2, CurrentPos);
                    cachebufferdata = buf2;
                }

                pos = CurrentPos;                                   // move curpos on.
                CurrentPos += datasize;

                Positions.Add(pos);
            }
            else
            {
                System.Diagnostics.Debug.Assert(BufferSize == 0 || pos + datasize <= BufferSize); // need either an uncommitted buffer, or within buffersize
            }

            return pos;
        }

        private int WriteCacheFinish(Array data, int fillpos, int datasize, bool writebuffer)
        {
            System.Buffer.BlockCopy(data, 0, cachebufferdata, fillpos, datasize);

            if (writebuffer)
                WriteAreaToBuffer(fillpos, datasize);

            return fillpos;
        }

        protected int Align(int align, int datasize)           // for use after allocation by sub classes, align and return pos
        {
            CurrentPos = (CurrentPos + align - 1) & (~(align - 1));
            int pos = CurrentPos;
            CurrentPos += datasize;
            Positions.Add(pos);
            System.Diagnostics.Debug.Assert(pos + datasize <= BufferSize);
            return pos;
        }

        protected IntPtr Align(IntPtr p, int offset, int align)
        {
            int newoffset = (offset + align - 1) & (~(align - 1));     // align..
            p += (newoffset - offset);
            return p;
        }

        #endregion

        protected abstract void WriteAreaToBuffer(int fillpos, int datasize);       // implement to write
    }


}