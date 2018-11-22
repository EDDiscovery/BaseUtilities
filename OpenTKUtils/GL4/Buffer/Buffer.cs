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
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    // local data block, writable in Vectors etc, supports std140 and std430

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

        public GLLayoutStandards( bool std430 = false)
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
                    if ( cachebufferdata != null )
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

        protected int Align(int align, int datasize )           // for use after allocation by sub classes, align and return pos
        {
            CurrentPos = (CurrentPos + align - 1) & (~(align - 1));
            int pos = CurrentPos;
            CurrentPos += datasize;
            Positions.Add(pos);
            System.Diagnostics.Debug.Assert(pos+datasize <= BufferSize);
            return pos;
        }

        #endregion

        protected abstract void WriteAreaToBuffer(int fillpos, int datasize);       // implement to write
    }

    [System.Diagnostics.DebuggerDisplay("Id {Id}")]
    public class GLBuffer : GLLayoutStandards, IDisposable
    {
        public int Id { get; private set; } = -1;

        public GLBuffer(bool std430 = false) : base(std430)
        {
            GL.CreateBuffers(1, out int id);     // this actually makes the buffer, GenBuffer does not - just gets a name
            Id = id;
        }

        #region Write to cache them complete

        public void Complete()
        {
            BufferSize = CurrentPos;       // what we have written now completes the size
            GL.NamedBufferData(Id, BufferSize, (IntPtr)0, BufferUsageHint.DynamicDraw);
            Update();
        }

        public void Update()        // rewrite the whole thing.. Complete must be called first.  Use after Writes without the immediate write buffer
        {
            System.Diagnostics.Debug.Assert(IsAllocated);
            IntPtr ptr = GL.MapNamedBufferRange(Id, (IntPtr)0, BufferSize, BufferAccessMask.MapWriteBit | BufferAccessMask.MapInvalidateBufferBit);
            System.Runtime.InteropServices.Marshal.Copy(cachebufferdata, 0, ptr, BufferSize);
            GL.UnmapNamedBuffer(Id);
            GLStatics.Check();
        }

        protected override void WriteAreaToBuffer(int fillpos, int datasize)        // update the buffer with an area of updated cache on a write..
        {
            System.Diagnostics.Debug.Assert(IsAllocated);
            IntPtr ptr = GL.MapNamedBufferRange(Id, (IntPtr)fillpos, datasize, BufferAccessMask.MapWriteBit | BufferAccessMask.MapInvalidateRangeBit);
            System.Runtime.InteropServices.Marshal.Copy(cachebufferdata, fillpos, ptr, datasize);
            GL.UnmapNamedBuffer(Id);
        }

        #endregion

        #region Allocate first, then Fill Direct - cache is not involved, so you can't use the cache write functions 
        
        public void Allocate(int size, BufferUsageHint uh = BufferUsageHint.StaticDraw)  // call first to set buffer size.. allow for alignment in your size
        {                                                                    // can call twice - get fresh buffer each time
            BufferSize = size;
            GL.NamedBufferData(Id, BufferSize, (IntPtr)0, uh);                  // set buffer size
            CurrentPos = 0;                                                  // reset back to zero as this clears the buffer
            GLStatics.Check();
        }

        public void Fill(Vector4[] vertices)
        {
            int datasize = vertices.Length * Vec4size;
            int posv = Align(Vec4size, datasize);
            GL.NamedBufferSubData(Id, (IntPtr)posv, datasize, vertices);
            GLStatics.Check();
        }

        public void Fill(Matrix4[] mats)
        {
            int datasize = mats.Length * Mat4size;
            int posv = Align(Vec4size, datasize);
            GL.NamedBufferSubData(Id, (IntPtr)posv, datasize, mats);
            GLStatics.Check();
        }

        public void Fill(Vector2[] vertices)
        {
            int datasize = vertices.Length * Vec2size;
            int posv = Align(Vec4size, datasize);
            GL.NamedBufferSubData(Id, (IntPtr)posv, datasize, vertices);
            GLStatics.Check();
        }

        public void Fill(OpenTK.Graphics.Color4[] colours, int entries = -1)        // entries can say repeat colours until filled to entries..
        {
            if (entries == -1)
                entries = colours.Length;

            int datasize = entries * Vec4size;
            int posc = Align(Vec4size, datasize);

            int colstogo = entries;
            int colp = posc;

            while (colstogo > 0)   // while more to fill in
            {
                int take = Math.Min(colstogo, colours.Length);      // max of colstogo and length of array
                GL.NamedBufferSubData(Id, (IntPtr)colp, 16 * take, colours);
                colstogo -= take;
                colp += take * 16;
            }
            GLStatics.Check();
        }

        public void Fill(uint[] data, BufferStorageFlags uh = BufferStorageFlags.MapWriteBit)
        {
            int datasize = data.Length * sizeof(uint);
            int pos = Align(sizeof(uint), datasize);
            GL.NamedBufferSubData(Id, (IntPtr)pos, datasize, data);
            GLStatics.Check();
        }

        public void Fill(Vector3[] vertices, Vector3 offsets, float mult)
        {
            int p = 0;                                                                  // probably change to write directly into buffer..
            uint[] packeddata = new uint[vertices.Length * 2];
            for (int i = 0; i < vertices.Length; i++)
            {
                uint z = (uint)((vertices[i].Z + offsets.Z) * mult);
                packeddata[p++] = (uint)((vertices[i].X + offsets.X) * mult) | ((z & 0x7ff) << 21);
                packeddata[p++] = (uint)((vertices[i].Y + offsets.Y) * mult) | (((z >> 11) & 0x7ff) << 21);
            }

            Fill(packeddata);
        }

        public void ZeroBuffer()
        {
            System.Diagnostics.Debug.Assert(BufferSize != 0);
            GL.ClearNamedBufferSubData(Id, PixelInternalFormat.R32ui, (IntPtr)0, BufferSize, PixelFormat.RedInteger, PixelType.UnsignedInt, (IntPtr)0);
            GLStatics.Check();
        }

        public void Set(Vector4[] v)        // quick helpers - set length and data
        {
            Allocate(v.Length * Vec4size);
            Fill(v);
        }

        #endregion

        #region Map then write..

        public IntPtr Map(int fillpos, int datasize)        // update the buffer with an area of updated cache on a write..
        {
            IntPtr p = GL.MapNamedBufferRange(Id, (IntPtr)fillpos, datasize, BufferAccessMask.MapWriteBit | BufferAccessMask.MapInvalidateRangeBit);
            GLStatics.Check();
            return p;
        }

        public void UnMap()
        { 
            GL.UnmapNamedBuffer(Id);
        }

        public void MapWrite(ref IntPtr pos, Matrix4 mat)
        {
            MapWrite(ref pos, mat.Row0);
            MapWrite(ref pos, mat.Row1);
            MapWrite(ref pos, mat.Row2);
            MapWrite(ref pos, mat.Row3);
        }

        public void MapWrite(ref IntPtr pos, Vector4 mat)
        {
            float[] a = new float[] { mat.X, mat.Y, mat.Z, mat.W };
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, 4);          // number of units, not byte length!
            pos += Vec4size;
        }

        public void MapWrite(ref IntPtr pos, Vector3 mat, float vec4other)      // write vec3 as vec4.
        {
            float[] a = new float[] { mat.X, mat.Y, mat.Z, vec4other };
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, 4);          // number of units, not byte length!
            pos += Vec4size;
        }

        public void MapWrite(ref IntPtr pos, float v)      
        {
            float[] a= new float[] { v };
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, 1);
            pos += sizeof(float);
        }

        public void MapWrite(ref IntPtr pos, float[] a)     
        {
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, a.Length);       // number of units, not byte length!
            pos += sizeof(float) * a.Length;
        }

        #endregion


        #region Reads

        public byte[] ReadBuffer(int offset, int size )         // read into a byte array
        {
            byte[] data = new byte[size];
            IntPtr ptr = GL.MapNamedBufferRange(Id, (IntPtr)offset, size, BufferAccessMask.MapReadBit);
            GLStatics.Check();
            System.Runtime.InteropServices.Marshal.Copy(ptr, data,0, size);
            GL.UnmapNamedBuffer(Id);
            return data;
        }

        public int ReadInt(int offset)                          // read a UINT
        {
            byte[] d = ReadBuffer(offset, sizeof(uint));
            return BitConverter.ToInt32(d, 0);
        }

        public float[] ReadFloats(int offset, int number)                      
        {
            float[] d = new float[number];
            byte[] bytes = ReadBuffer(offset, sizeof(float) * number);

            for (int i = 0; i < number; i++)
                d[i] = BitConverter.ToSingle(bytes, i * 4);

            return d;
        }

        public Vector4[] ReadVector4(int offset, int number)                   
        {
            byte[] bytes = ReadBuffer(offset, Vec4size * number);
            Vector4[] d = new Vector4[number];

            for (int i = 0; i < number; i++)
            {
                int p = i * 16;
                d[i] = new Vector4(BitConverter.ToSingle(bytes, p),
                    BitConverter.ToSingle(bytes, p + 4),
                    BitConverter.ToSingle(bytes, p + 8),
                    BitConverter.ToSingle(bytes, p + 12));
            }

            return d;
        }


        #endregion

        #region Binding a buffer to a binding index on the currently bound VA

        public void Bind(int bindingindex, int start, int stride, int divisor = 0)
        {
            GL.BindVertexBuffer(bindingindex, Id, (IntPtr)start, stride);      // this buffer to binding index
            GL.VertexBindingDivisor(bindingindex, divisor);
            GLStatics.Check();
            //System.Diagnostics.Debug.WriteLine("BUFBIND " + bindingindex + " To B" + Id + " pos " + start + " stride " + stride + " divisor " + divisor);
        }
        #endregion

        #region Implementation

        public void Dispose()           // you can double dispose.
        {
            if (Id != -1)
            {
                GL.DeleteBuffer(Id);
                Id = -1;
            }
        }

        #endregion
    }
}