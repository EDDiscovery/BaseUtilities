/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
 * 
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
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    // local data block, writable in Vectors etc, supports std140 and std430
    // inherits functionality from layout standards to operate a chash write then Complete() functionality
    // or you can use Allocate then Fill direct
    // or you can use the GL Mapping function which maps the buffer into memory

    [System.Diagnostics.DebuggerDisplay("Id {Id}")]
    public class GLBuffer : GLLayoutStandards, IDisposable
    {
        public int Id { get; private set; } = -1;

        public GLBuffer(bool std430 = false) : base(std430)
        {
            GL.CreateBuffers(1, out int id);     // this actually makes the buffer, GenBuffer does not - just gets a name
            Id = id;
        }

        public GLBuffer(int allocatesize, bool std430 = false, BufferUsageHint bh = BufferUsageHint.StaticDraw) : this(std430)
        {
            Allocate(allocatesize, bh);
        }

        #region Cache function. Cache functions are in LayoutStandards. Write to cache them complete

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
            OpenTKUtils.GLStatics.Check();
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
            OpenTKUtils.GLStatics.Check();
        }

        public void Fill(float[] floats)
        {
            int datasize = floats.Length * sizeof(float);
            int posv = Align(sizeof(float), datasize);
            GL.NamedBufferSubData(Id, (IntPtr)posv, datasize, floats);
            OpenTKUtils.GLStatics.Check();
        }

        public void AllocateFill(float[] vertices)
        {
            Allocate(sizeof(float) * vertices.Length);
            Fill(vertices);
        }

        public void Fill(Vector2[] vertices)
        {
            int datasize = vertices.Length * Vec2size;
            int posv = Align(Vec4size, datasize);
            GL.NamedBufferSubData(Id, (IntPtr)posv, datasize, vertices);
            OpenTKUtils.GLStatics.Check();
        }

        public void AllocateFill(Vector2[] vertices)
        {
            Allocate(Vec2size * vertices.Length);
            Fill(vertices);
        }

        public void Fill(Vector4[] vertices)
        {
            int datasize = vertices.Length * Vec4size;
            int posv = Align(Vec4size, datasize);
            GL.NamedBufferSubData(Id, (IntPtr)posv, datasize, vertices);
            OpenTKUtils.GLStatics.Check();
        }

        public void AllocateFill(Vector4[] vertices)
        {
            Allocate(Vec4size * vertices.Length);
            Fill(vertices);
        }

        public void Fill(Matrix4[] mats)
        {
            int datasize = mats.Length * Mat4size;
            int posv = Align(Vec4size, datasize);
            GL.NamedBufferSubData(Id, (IntPtr)posv, datasize, mats);
            OpenTKUtils.GLStatics.Check();
        }

        public void AllocateFill(Matrix4[] mats)
        {
            Allocate(Mat4size * mats.Length);
            Fill(mats);
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
            OpenTKUtils.GLStatics.Check();
        }

        public void Fill(uint[] data, BufferStorageFlags uh = BufferStorageFlags.MapWriteBit)
        {
            int datasize = data.Length * sizeof(uint);
            int pos = Align(sizeof(uint), datasize);
            GL.NamedBufferSubData(Id, (IntPtr)pos, datasize, data);
            OpenTKUtils.GLStatics.Check();
        }

        public void AllocateFill(uint[] data, BufferStorageFlags uh = BufferStorageFlags.MapWriteBit)
        {
            Allocate(sizeof(uint) * data.Length);
            Fill(data,uh);
        }

        public void Fill(byte[] data, BufferStorageFlags uh = BufferStorageFlags.MapWriteBit)
        {
            int datasize = data.Length;
            int pos = Align(sizeof(byte), datasize);        //tbd
            GL.NamedBufferSubData(Id, (IntPtr)pos, datasize, data);
            OpenTKUtils.GLStatics.Check();
        }

        public void AllocateFill(byte[] data, BufferStorageFlags uh = BufferStorageFlags.MapWriteBit)
        {
            Allocate(data.Length);
            Fill(data, uh);
        }

        public void FillPacked2vec(Vector3[] vertices, Vector3 offsets, float mult)
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

        public void ZeroBuffer()    // after allocated
        {
            System.Diagnostics.Debug.Assert(BufferSize != 0);
            GL.ClearNamedBufferSubData(Id, PixelInternalFormat.R32ui, (IntPtr)0, BufferSize, PixelFormat.RedInteger, PixelType.UnsignedInt, (IntPtr)0);
            OpenTKUtils.GLStatics.Check();
        }

        public void ZeroBuffer(int size)
        {
            Allocate(size);
            GL.ClearNamedBufferSubData(Id, PixelInternalFormat.R32ui, (IntPtr)0, BufferSize, PixelFormat.RedInteger, PixelType.UnsignedInt, (IntPtr)0);
            OpenTKUtils.GLStatics.Check();
        }

        public void FillRectangularIndices(int reccount, int restartindex = 0xff)        // rectangular indicies with restart of 0xff
        {
            Allocate(reccount * 5);
            IntPtr ip = Map(0, BufferSize);
            for (int r = 0; r < reccount; r++)
            {
                byte[] ar = new byte[] { (byte)(r * 4), (byte)(r * 4 + 1), (byte)(r * 4 + 2), (byte)(r * 4 + 3), (byte)restartindex};
                MapWrite(ref ip, ar);
            }

            UnMap();
        }

        #endregion

        #region GL MAP into memory so you can use a IntPtr

        int mapoffset;

        public IntPtr Map(int fillpos, int datasize)        // update the buffer with an area of updated cache on a write..
        {
            IntPtr p = GL.MapNamedBufferRange(Id, (IntPtr)fillpos, datasize, BufferAccessMask.MapWriteBit | BufferAccessMask.MapInvalidateRangeBit);
            mapoffset = fillpos;
            OpenTKUtils.GLStatics.Check();
            return p;
        }

        public void UnMap()
        { 
            GL.UnmapNamedBuffer(Id);
            OpenTKUtils.GLStatics.Check();
        }

        public void MapWrite(ref IntPtr pos, Matrix4 mat)
        {
            pos = Align(pos, mapoffset, Vec4size);
            float[] r = new float[] {   mat.Row0.X, mat.Row0.Y, mat.Row0.Z, mat.Row0.W ,
                                        mat.Row1.X, mat.Row1.Y, mat.Row1.Z, mat.Row1.W ,
                                        mat.Row2.X, mat.Row2.Y, mat.Row2.Z, mat.Row2.W ,
                                        mat.Row3.X, mat.Row3.Y, mat.Row3.Z, mat.Row3.W };

            System.Runtime.InteropServices.Marshal.Copy(r, 0, pos, 4*4);          // number of units, not byte length!
            pos += Vec4size*4;
            mapoffset += Vec4size*4;
        }

        public void MapWrite(ref IntPtr pos, Vector4 v4)
        {
            pos = Align(pos, mapoffset, Vec4size);
            float[] a = new float[] { v4.X, v4.Y, v4.Z, v4.W };
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, 4);          // number of units, not byte length!
            pos += Vec4size;
            mapoffset += Vec4size;
        }

        public void MapWrite(ref IntPtr pos, System.Drawing.Rectangle r)
        {
            pos = Align(pos, mapoffset, Vec4size);
            float[] a = new float[] { r.Left, r.Top, r.Right, r.Bottom };
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, 4);          // number of units, not byte length!
            pos += Vec4size;
            mapoffset += Vec4size;
        }

        public void MapWrite(ref IntPtr pos, Vector3 mat, float vec4other)      // write vec3 as vec4.
        {
            pos = Align(pos, mapoffset, Vec4size);
            float[] a = new float[] { mat.X, mat.Y, mat.Z, vec4other };
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, 4);          // number of units, not byte length!
            pos += Vec4size;
            mapoffset += Vec4size;
        }

        public void MapWrite(ref IntPtr pos, float v)
        {
            float[] a = new float[] { v };
            pos = Align(pos, mapoffset, sizeof(float));
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, 1);
            pos += sizeof(float);
            mapoffset += sizeof(float);
        }

        public void MapWrite(ref IntPtr pos, float[] a)
        {
            pos = Align(pos, mapoffset, sizeof(float));
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, a.Length);       // number of units, not byte length!
            pos += sizeof(float) * a.Length;
            mapoffset += sizeof(float) * a.Length;
        }

        public void MapWrite(ref IntPtr pos, int v)
        {
            int[] a = new int[] { v };
            pos = Align(pos, mapoffset, sizeof(int));
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, 1);
            pos += sizeof(int);
            mapoffset += sizeof(int);
        }

        public void MapWrite(ref IntPtr pos, int[] a)
        {
            pos = Align(pos, mapoffset, sizeof(int));
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, a.Length);
            pos += sizeof(int) * a.Length;
            mapoffset += sizeof(int) * a.Length;
        }

        public void MapWrite(ref IntPtr pos, long v)
        {
            pos = Align(pos, mapoffset, sizeof(long));
            long[] a = new long[] { v };
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, 1);
            pos += sizeof(long);
            mapoffset += sizeof(long);
        }

        public void MapWrite(ref IntPtr pos, long[] a)
        {
            pos = Align(pos, mapoffset, sizeof(long));
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, a.Length);
            pos += sizeof(long) * a.Length;
            mapoffset += sizeof(long) * a.Length;
        }

        public void MapWrite(ref IntPtr pos, byte[] a)
        {
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, a.Length);
            pos += a.Length;
            mapoffset += a.Length;
        }

        public void MapWrite(ref IntPtr pos, byte v)
        {
            byte[] a = new byte[] { v };
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, 1);
            pos += 1;
            mapoffset++;
        }

        // write an Indirect Array draw command to the buffer
        // if you use it, MultiDrawCountStride = 16
        public void MapWriteIndirectArray(ref IntPtr pos, int vertexcount, int instancecount = 1, int firstvertex = 0, int baseinstance = 0)
        {
            int[] i = new int[] { vertexcount, instancecount, firstvertex, baseinstance };       
            MapWrite(ref pos, i);
        }

        // write an Element draw command to the buffer
        // if you use it, MultiDrawCountStride = 20
        public void MapWriteIndirectElements(ref IntPtr pos, int vertexcount, int instancecount = 1, int firstindex = 0, int basevertex = 0,int baseinstance = 0)
        {
            int[] i = new int[] { vertexcount, instancecount, firstindex, basevertex, baseinstance };
            MapWrite(ref pos, i);
        }


        #endregion

        #region DrawElements indexs


        #endregion

        #region Reads - Map into memory and then read

        public byte[] ReadBytes(int offset, int size )         // read into a byte array
        {
            byte[] data = new byte[size];
            IntPtr ptr = GL.MapNamedBufferRange(Id, (IntPtr)offset, size, BufferAccessMask.MapReadBit);
            OpenTKUtils.GLStatics.Check();
            System.Runtime.InteropServices.Marshal.Copy(ptr, data,0, size);
            GL.UnmapNamedBuffer(Id);
            return data;
        }

        public int ReadInt(int offset)
        {
            byte[] d = ReadBytes(offset, sizeof(uint));
            return BitConverter.ToInt32(d, 0);
        }

        public int[] ReadInts(int offset,  int number)                    
        {
            int[] ints = new int[number];
            byte[] bytes = ReadBytes(offset, sizeof(int)*number);

            for (int i = 0; i < number; i++)
                ints[i] = BitConverter.ToInt32(bytes, i * 4);

            return ints;
        }

        public long ReadLong(int offset)
        {
            byte[] d = ReadBytes(offset, sizeof(long));
            return BitConverter.ToInt64(d, 0);
        }

        public long[] ReadLongs(int offset, int number)
        {
            long[] longs = new long[number];
            byte[] bytes = ReadBytes(offset, sizeof(long) * number);
            for (int i = 0; i < number; i++)
                longs[i] = BitConverter.ToInt64(bytes, i * 8);
            return longs;
        }

        public float[] ReadFloats(int offset, int number)
        {
            float[] d = new float[number];
            byte[] bytes = ReadBytes(offset, sizeof(float) * number);

            for (int i = 0; i < number; i++)
                d[i] = BitConverter.ToSingle(bytes, i * 4);

            return d;
        }

        public Vector4[] ReadVector4(int offset, int number)                   
        {
            byte[] bytes = ReadBytes(offset, Vec4size * number);
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
            OpenTKUtils.GLStatics.Check();
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