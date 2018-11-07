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
        public int Size { get; protected set; } = 0;
        protected byte[] cachebufferdata = null;

        // std140 alignment: scalars are N, 2Vec = 2N, 3Vec = 4N, 4Vec = 4N. Array alignment is vec4, stride vec4
        // std430 alignment: scalars are N, 2Vec = 2N, 3Vec = 4N, 4Vec = 4N. Array alignment is same as left, stride is as per left.

        private int arraystridealignment;

        public GLLayoutStandards(int isz = 64, bool std430 = false)
        {
            Size = 0;
            if ( isz>0)
                cachebufferdata = new byte[isz];
            arraystridealignment = std430 ? 4 : 16;               // if in std130, arrays have vec4 alignment and strides are vec4
        }

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
                Size = (Size + align - 1) & (~(align - 1));

                if (cachebufferdata == null || Size + datasize > cachebufferdata.Length)
                {
                    int nextsize = Size + datasize + 512;
                    byte[] buf2 = new byte[nextsize];
                    if ( cachebufferdata != null )
                        Array.Copy(cachebufferdata, buf2, Size);
                    cachebufferdata = buf2;
                }

                pos = Size;
                Size += datasize;
            }

            return pos;
        }

        private int Finish(Array data, int fillpos, int datasize, bool writebuffer)
        {
            System.Buffer.BlockCopy(data, 0, cachebufferdata, fillpos, datasize);

            if (writebuffer)
                WriteAreaToBuffer(fillpos, datasize);

            return fillpos;
        }

        protected int Align(int align, int datasize )
        {
            Size = (Size + align - 1) & (~(align - 1));
            int pos = Size;
            Size += datasize;
            return pos;
        }

        protected abstract void WriteAreaToBuffer(int fillpos, int datasize);       // implement to write
    }

    public class GLBuffer : GLLayoutStandards, IDisposable
    {
        public int Id { get; private set; } = -1;

        public GLBuffer(bool std430 = false, int isz = 0) : base(isz, std430)
        {
            GL.CreateBuffers(1, out int id);     // this actually makes the buffer, GenBuffer does not - just gets a name
            Id = id;
        }

        #region Set Direct - cache is not involved, so you can't use the other write functions 

        public void SetSize(int size, BufferUsageHint uh = BufferUsageHint.StaticDraw)
        {
            Size = size;
            GL.NamedBufferData(Id,Size, (IntPtr)0, uh);              // set size no data
        }

        public void ZeroBuffer()
        {
            GL.ClearNamedBufferSubData(Id, PixelInternalFormat.R32ui, (IntPtr)0, Size, PixelFormat.RedInteger, PixelType.UnsignedInt, (IntPtr)0);
        }

        public Tuple<int,int> Set(Vector4[] vertices, OpenTK.Graphics.Color4[] colours, BufferUsageHint uh = BufferUsageHint.StaticDraw)
        {
            int datasize = vertices.Length * sizeof(float) * 4;
            int posv = Align(sizeof(float) * 4, datasize);
            int posc = Align(sizeof(float) * 4, datasize);

            GL.NamedBufferData(Id, Size, (IntPtr)0, uh);              // set size
            GL.NamedBufferSubData(Id, (IntPtr)posv, datasize, vertices);

            int colstogo = vertices.Length;
            int colp = posc;

            while (colstogo > 0)   // while more to fill in
            {
                int take = Math.Min(colstogo, colours.Length);      // max of colstogo and length of array
                GL.NamedBufferSubData(Id, (IntPtr)colp, 16 * take, colours);
                GLStatics.Check();
                colstogo -= take;
                colp += take * 16;
            }

            GLStatics.Check();
            return new Tuple<int, int>(posv, posc);
        }


        public Tuple<int, int> Set(Vector4[] vertices,Vector2[] coords, BufferUsageHint uh = BufferUsageHint.StaticDraw)
        {
            int datasizeV = vertices.Length * sizeof(float) * 4;
            int datasizeC = coords.Length * sizeof(float) * 2;
            int posv = Align(sizeof(float) * 4, datasizeV);
            int posc = Align(sizeof(float) * 4, datasizeC);

            GL.NamedBufferData(Id, Size, (IntPtr)0, uh);              // set size
            GL.NamedBufferSubData(Id, (IntPtr)posv, datasizeV, vertices);
            GL.NamedBufferSubData(Id, (IntPtr)posc, datasizeC, coords);
            GLStatics.Check();
            return new Tuple<int, int>(posv, posc);
        }

        public int Set(Vector4[] vertices, BufferStorageFlags uh = BufferStorageFlags.MapWriteBit)
        {
            int datasize = vertices.Length * sizeof(float) * 4;
            int pos = Align(sizeof(float) * 4, datasize);

            GL.NamedBufferStorage(Id, Size, vertices, uh);
            GLStatics.Check();
            return pos;
        }

        public int Set(uint[] data, BufferStorageFlags uh = BufferStorageFlags.MapWriteBit)
        {
            int datasize = data.Length * sizeof(uint);
            int pos = Align(sizeof(uint), datasize);

            GL.NamedBufferStorage(Id, Size, data, uh);
            GLStatics.Check();
            return pos;
        }

        public Tuple<int, int> Set(Vector4[] vertices, Matrix4[] transforms, BufferUsageHint uh = BufferUsageHint.StaticDraw)
        {
            int vertsize = vertices.Length * sizeof(float) * 4;
            int matsize = transforms.Length * sizeof(float) * 16;
            int posv = Align(sizeof(float) * 4, vertsize);
            int posc = Align(sizeof(float) * 4, matsize);

            GL.NamedBufferData(Id, Size, (IntPtr)0, uh);              // set size
            GL.NamedBufferSubData(Id, (IntPtr)posv, vertsize, vertices);
            GL.NamedBufferSubData(Id, (IntPtr)posc, matsize, transforms);

            GLStatics.Check();
            return new Tuple<int, int>(posv, posc);
        }

        public Tuple<int, int, int> Set(Vector4[] vertices, Vector2[] texcoords, Matrix4[] transforms, BufferUsageHint uh = BufferUsageHint.StaticDraw)
        {
            int vertsize = vertices.Length * sizeof(float) * 4;
            int texsize = texcoords.Length * sizeof(float) * 2;
            int matsize = transforms.Length * sizeof(float) * 16;
            int posv = Align(sizeof(float) * 4, vertsize);
            int post = Align(sizeof(float) * 2, texsize);
            int posc = Align(sizeof(float) * 4, matsize);

            GL.NamedBufferData(Id, Size, (IntPtr)0, uh);              // set size
            GL.NamedBufferSubData(Id, (IntPtr)posv, vertsize, vertices);
            GL.NamedBufferSubData(Id, (IntPtr)post, texsize, texcoords);
            GL.NamedBufferSubData(Id, (IntPtr)posc, matsize, transforms);

            GLStatics.Check();
            return new Tuple<int, int, int>(posv, post, posc);
        }

        public byte[] ReadBuffer(int offset, int size )         // read into a byte array
        {
            byte[] data = new byte[size];
            IntPtr ptr = GL.MapNamedBufferRange(Id, (IntPtr)0, Size, BufferAccessMask.MapReadBit);
            System.Runtime.InteropServices.Marshal.Copy(ptr,data,offset, size);
            GL.UnmapNamedBuffer(Id);
            return data;
        }

        public int ReadInt(int offset)                          // read a UINT
        {
            byte[] d = ReadBuffer(offset, sizeof(uint));
            return BitConverter.ToInt32(d, 0);

        }

        public float[] ReadFloats(int offset, int number)                          // read a UINT
        {
            float[] d = new float[number];
            byte[] bytes = ReadBuffer(offset, sizeof(float) * number);

            for (int i = 0; i < number; i++)
                d[i] = BitConverter.ToSingle(bytes, offset + i * 4);

            return d;
        }

        public Vector4[] ReadVector4(int offset, int number)                          // read a UINT
        {
            byte[] bytes = ReadBuffer(offset, sizeof(float) * 4 * number);
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

        #region Implementation

        protected void WriteCacheToBuffer()     // update the buffer with the cache.
        {
            IntPtr ptr = GL.MapNamedBufferRange(Id, (IntPtr)0, Size, BufferAccessMask.MapWriteBit | BufferAccessMask.MapInvalidateBufferBit);
            System.Runtime.InteropServices.Marshal.Copy(cachebufferdata, 0, ptr, Size);
            GL.UnmapNamedBuffer(Id);
        }

        protected override void WriteAreaToBuffer(int fillpos, int datasize)        // update the buffer with an area of updated cache
        {
            IntPtr ptr = GL.MapNamedBufferRange(Id, (IntPtr)fillpos, datasize, BufferAccessMask.MapWriteBit | BufferAccessMask.MapInvalidateRangeBit);
            System.Runtime.InteropServices.Marshal.Copy(cachebufferdata, fillpos, ptr, datasize);
            GL.UnmapNamedBuffer(Id);
        }

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