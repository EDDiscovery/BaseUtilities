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
    // you can use Allocate then Fill direct
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
            AllocateBytes(allocatesize, bh);
        }

        #region Allocate first, then Fill Direct - cache is not involved, so you can't use the cache write functions 
        
        public void AllocateBytes(int bytessize, BufferUsageHint uh = BufferUsageHint.StaticDraw)  // call first to set buffer size.. allow for alignment in your size
        {                                                                    // can call twice - get fresh buffer each time
            BufferSize = bytessize;
            GL.NamedBufferData(Id, BufferSize, (IntPtr)0, uh);               // set buffer size
            CurrentPos = 0;                                                  // reset back to zero as this clears the buffer
            Positions.Clear();
            OpenTKUtils.GLStatics.Check();
        }

        public void Fill(float[] floats)
        {
            int datasize = floats.Length * sizeof(float);
            int posv = AlignArray(sizeof(float), datasize);
            GL.NamedBufferSubData(Id, (IntPtr)posv, datasize, floats);
            OpenTKUtils.GLStatics.Check();
        }

        public void AllocateFill(float[] vertices)
        {
            AllocateBytes(sizeof(float) * vertices.Length);
            Fill(vertices);
        }

        public void Fill(Vector2[] vertices)
        {
            int datasize = vertices.Length * Vec2size;
            int posv = AlignArray(Vec2size, datasize);
            GL.NamedBufferSubData(Id, (IntPtr)posv, datasize, vertices);
            OpenTKUtils.GLStatics.Check();
        }

        public void AllocateFill(Vector2[] vertices)
        {
            AllocateBytes(Vec2size * vertices.Length);
            Fill(vertices);
        }

        // no Vector3 on purpose, they don't work well with opengl

        public void Fill(Vector4[] vertices)
        {
            int datasize = vertices.Length * Vec4size;
            int posv = AlignArray(Vec4size, datasize);
            GL.NamedBufferSubData(Id, (IntPtr)posv, datasize, vertices);
            OpenTKUtils.GLStatics.Check();
        }

        public void AllocateFill(Vector4[] vertices)
        {
            AllocateBytes(Vec4size * vertices.Length);
            Fill(vertices);
        }

        public void Fill(Matrix4[] mats)
        {
            int datasize = mats.Length * Mat4size;
            int posv = AlignArray(Vec4size, datasize);
            GL.NamedBufferSubData(Id, (IntPtr)posv, datasize, mats);
            OpenTKUtils.GLStatics.Check();
        }

        public void AllocateFill(Matrix4[] mats)
        {
            AllocateBytes(Mat4size * mats.Length);
            Fill(mats);
        }

        public void Fill(OpenTK.Graphics.Color4[] colours, int entries = -1)        // entries can say repeat colours until filled to entries..
        {
            if (entries == -1)
                entries = colours.Length;

            int datasize = entries * Vec4size;
            int posc = AlignArray(Vec4size, datasize);

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

        public void Fill(ushort[] words)
        {
            int datasize = words.Length * sizeof(ushort);
            int posv = AlignArray(sizeof(ushort), datasize);
            GL.NamedBufferSubData(Id, (IntPtr)posv, datasize, words);
            OpenTKUtils.GLStatics.Check();
        }

        public void AllocateFill(ushort[] words)
        {
            AllocateBytes(sizeof(ushort) * words.Length);
            Fill(words);
        }

        public void Fill(uint[] data)
        {
            int datasize = data.Length * sizeof(uint);
            int pos = AlignArray(sizeof(uint), datasize);
            GL.NamedBufferSubData(Id, (IntPtr)pos, datasize, data);
            OpenTKUtils.GLStatics.Check();
        }

        public void AllocateFill(uint[] data)
        {
            AllocateBytes(sizeof(uint) * data.Length);
            Fill(data);
        }

        public void Fill(byte[] data)      
        {
            int datasize = data.Length;
            int pos = AlignArray(sizeof(byte), datasize);        //tbd
            GL.NamedBufferSubData(Id, (IntPtr)pos, datasize, data);
            OpenTKUtils.GLStatics.Check();
        }

        public void AllocateFill(byte[] data)
        {
            AllocateBytes(data.Length);
            Fill(data);
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
            AllocateBytes(size);
            GL.ClearNamedBufferSubData(Id, PixelInternalFormat.R32ui, (IntPtr)0, BufferSize, PixelFormat.RedInteger, PixelType.UnsignedInt, (IntPtr)0);
            OpenTKUtils.GLStatics.Check();
        }

        public void FillRectangularIndicesBytes(int reccount, int restartindex = 0xff)        // rectangular indicies with restart of 0xff
        {
            AllocateBytes(reccount * 5);
            StartWrite(0, BufferSize);
            for (int r = 0; r < reccount; r++)
            {
                byte[] ar = new byte[] { (byte)(r * 4), (byte)(r * 4 + 1), (byte)(r * 4 + 2), (byte)(r * 4 + 3), (byte)restartindex };
                Write(ar);
            }

            StopReadWrite();
        }

        public void FillRectangularIndicesShort(int reccount, int restartindex = 0xffff)        // rectangular indicies with restart of 0xff
        {
            AllocateBytes(reccount * 5 * sizeof(short));     // lets use short because we don't have a marshall copy ushort.. ignore the overflow
            StartWrite(0, BufferSize);
            for (int r = 0; r < reccount; r++)
            {
                short[] ar = new short[] { (short)(r * 4), (short)(r * 4 + 1), (short)(r * 4 + 2), (short)(r * 4 + 3), (short)restartindex };
                Write(ar);
            }

            StopReadWrite();
        }

        #endregion

        #region Map Read/Write Common

        enum MapMode { None, Write, Read};
        MapMode mapmode = MapMode.None;

        public void AllocateStartWrite(int datasize)        // update the buffer with an area of updated cache on a write.. (0=all buffer)
        {
            AllocateBytes(datasize);
            StartWrite(0);
        }

        public void StartWrite(int fillpos, int datasize = 0)        // update the buffer with an area of updated cache on a write.. (0=all buffer)
        {
            if (datasize == 0)
                datasize = BufferSize - fillpos;

            System.Diagnostics.Debug.Assert(mapmode == MapMode.None && fillpos + datasize <= BufferSize); // catch double maps

            CurrentPtr = GL.MapNamedBufferRange(Id, (IntPtr)fillpos, datasize, BufferAccessMask.MapWriteBit | BufferAccessMask.MapInvalidateRangeBit);
            CurrentPos = fillpos;
            mapmode = MapMode.Write;
            OpenTKUtils.GLStatics.Check();
        }

        public void StartRead(int fillpos, int datasize = 0)        // update the buffer with an area of updated cache on a write (0=all buffer)
        {
            if (datasize == 0)
                datasize = BufferSize - fillpos;

            System.Diagnostics.Debug.Assert(mapmode == MapMode.None && fillpos + datasize <= BufferSize); // catch double maps

            CurrentPtr = GL.MapNamedBufferRange(Id, (IntPtr)fillpos, datasize, BufferAccessMask.MapReadBit);
            CurrentPos = fillpos;
            mapmode = MapMode.Read;
            OpenTKUtils.GLStatics.Check();
        }

        public void StopReadWrite()
        {
            GL.UnmapNamedBuffer(Id);
            mapmode = MapMode.None;
            OpenTKUtils.GLStatics.Check();
        }

        public void Skip(int p)
        {
            System.Diagnostics.Debug.Assert(mapmode != MapMode.None);
            CurrentPtr += p;
            CurrentPos += p;
            System.Diagnostics.Debug.Assert(CurrentPos <= BufferSize);
            OpenTKUtils.GLStatics.Check();
        }

        public void AlignArray(int size)         // align to array boundary without writing
        {
            AlignArrayPtr(size, 0);
        }

        #endregion

        #region Write to map

        public void Write(Matrix4 mat)
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Write);
            var p = AlignArrayPtr(Vec4size, 4);
            float[] r = new float[] {   mat.Row0.X, mat.Row0.Y, mat.Row0.Z, mat.Row0.W ,
                                        mat.Row1.X, mat.Row1.Y, mat.Row1.Z, mat.Row1.W ,
                                        mat.Row2.X, mat.Row2.Y, mat.Row2.Z, mat.Row2.W ,
                                        mat.Row3.X, mat.Row3.Y, mat.Row3.Z, mat.Row3.W };
            System.Runtime.InteropServices.Marshal.Copy(r, 0, p.Item1, r.Length);          // number of units, not byte length!
        }

        public void Write(Vector4 v4)
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Write);
            var p = AlignArrayPtr(Vec4size, 1);
            float[] a = new float[] { v4.X, v4.Y, v4.Z, v4.W };
            System.Runtime.InteropServices.Marshal.Copy(a, 0, p.Item1, a.Length);   
        }

        public void Write(System.Drawing.Rectangle r)
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Write);
            var p = AlignArrayPtr(Vec4size, 1);
            float[] a = new float[] { r.Left, r.Top, r.Right, r.Bottom };
            System.Runtime.InteropServices.Marshal.Copy(a, 0, p.Item1, a.Length);          
        }

        public void Write(Vector3 mat, float vec4other)      // write vec3 as vec4.
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Write);
            var p = AlignArrayPtr(Vec4size, 1);
            float[] a = new float[] { mat.X, mat.Y, mat.Z, vec4other };
            System.Runtime.InteropServices.Marshal.Copy(a, 0, p.Item1, a.Length);          
        }

        public void Write(float v)
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Write);
            IntPtr pos = AlignScalarPtr(sizeof(float));
            float[] a = new float[] { v };
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, a.Length);
        }

        public void Write(float[] a)
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Write);
            int size = sizeof(float);
            var p = AlignArrayPtr(size, a.Length);
            if (p.Item2 == size)
                System.Runtime.InteropServices.Marshal.Copy(a, 0, p.Item1, a.Length);       
            else
            {
                var fa = new float[a.Length * 4];
                for (int i = 0; i < a.Length; i++)      // std140 'orrible
                    fa[i * 4] = a[i];
                System.Runtime.InteropServices.Marshal.Copy(fa, 0, p.Item1, fa.Length);       // number of units, not byte length!
            }
        }

        public void WriteCont(float[] a)     // without checking for alignment/stride
        {
            System.Runtime.InteropServices.Marshal.Copy(a, 0, CurrentPtr, a.Length);       // number of units, not byte length!
            CurrentPtr += sizeof(float) * a.Length;
            CurrentPos += sizeof(float) * a.Length;
        }

        public void Write(int v)
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Write);
            int[] a = new int[] { v };
            IntPtr pos = AlignScalarPtr(sizeof(int));
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, a.Length);
        }

        public void Write(int[] a)
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Write);
            int size = sizeof(int);
            var p = AlignArrayPtr(size, a.Length);
            if (p.Item2 == size)
                System.Runtime.InteropServices.Marshal.Copy(a, 0, p.Item1, a.Length);       // number of units, not byte length!
            else
            {
                var fa = new int[a.Length * 4];
                for (int i = 0; i < a.Length; i++)      // std140 'orrible
                    fa[i * 4] = a[i];
                System.Runtime.InteropServices.Marshal.Copy(fa, 0, p.Item1, fa.Length);       // number of units, not byte length!
            }
        }

        public void Write(short v)
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Write);
            short[] a = new short[] { v };
            IntPtr pos = AlignScalarPtr(sizeof(short));
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, a.Length);
        }

        public void Write(short[] a)
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Write);
            int size = sizeof(short);
            var p = AlignArrayPtr(size, a.Length);
            if (p.Item2 == size)
                System.Runtime.InteropServices.Marshal.Copy(a, 0, p.Item1, a.Length);       // number of units, not byte length!
            else
            {
                var fa = new short[a.Length * 4];
                for (int i = 0; i < a.Length; i++)      // std140 'orrible
                    fa[i * 4] = a[i];
                System.Runtime.InteropServices.Marshal.Copy(fa, 0, p.Item1, fa.Length);       // number of units, not byte length!
            }
        }

        public void Write(long v)
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Write);
            long[] a = new long[] { v };
            IntPtr pos = AlignScalarPtr(sizeof(long));
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, a.Length);
        }

        public void Write(long[] a)
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Write);
            int size = sizeof(long);
            var p = AlignArrayPtr(size, a.Length);
            if (p.Item2 == size)
                System.Runtime.InteropServices.Marshal.Copy(a, 0, p.Item1, a.Length);       // number of units, not byte length!
            else
            {
                var fa = new long[a.Length * 4];
                for (int i = 0; i < a.Length; i++)      // std140 'orrible
                    fa[i * 4] = a[i];
                System.Runtime.InteropServices.Marshal.Copy(fa, 0, p.Item1, fa.Length);       // number of units, not byte length!
            }
        }

        public void Write(byte v)
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Write);
            byte[] a = new byte[] { v };
            IntPtr pos = AlignScalarPtr(sizeof(byte));
            System.Runtime.InteropServices.Marshal.Copy(a, 0, pos, a.Length);
        }

        public void Write(byte[] a)     // special , not aligned, as not normal glsl type.  Used mostly for element indexes
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Write);
            var p = AlignArrayPtr(sizeof(byte), a.Length);
            System.Runtime.InteropServices.Marshal.Copy(a, 0, p.Item1, a.Length);
        }

        // write an Indirect Array draw command to the buffer
        // if you use it, MultiDrawCountStride = 16
        public void WriteIndirectArray(int vertexcount, int instancecount = 1, int firstvertex = 0, int baseinstance = 0)
        {
            int[] i = new int[] { vertexcount, instancecount, firstvertex, baseinstance };       
            Write(i);
        }

        // write an Element draw command to the buffer
        // if you use it, MultiDrawCountStride = 20
        public void WriteIndirectElements(int vertexcount, int instancecount = 1, int firstindex = 0, int basevertex = 0,int baseinstance = 0)
        {
            int[] i = new int[] { vertexcount, instancecount, firstindex, basevertex, baseinstance };
            Write(i);
        }


        #endregion

        #region Reads - Map into memory and then read

        public byte[] ReadBytes(int size)   // read into a byte array. not aligned 
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Read);
            byte[] data = new byte[size];
            var p = AlignArrayPtr(sizeof(byte), size);
            System.Runtime.InteropServices.Marshal.Copy(p.Item1, data, 0, size);
            return data;
        }

        public int[] ReadInts(int count)                    // read into a byte array
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Read);
            int size = sizeof(int);
            var data = new int[count];
            var p = AlignArrayPtr(size, count);
            if (p.Item2 == size)
                System.Runtime.InteropServices.Marshal.Copy(p.Item1, data, 0, data.Length);
            else
            {
                var temp = new int[count * 4];
                System.Runtime.InteropServices.Marshal.Copy(p.Item1, data, 0, temp.Length);
                for (int i = 0; i < data.Length; i++)
                    data[i] = temp[i * 4];
            }
            return data;
        }

        public int ReadInt()
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Read);
            int[] data = new int[1];
            IntPtr pos = AlignScalarPtr(sizeof(int));
            System.Runtime.InteropServices.Marshal.Copy(pos, data, 0, 1);
            return data[0];
        }

        public long[] ReadLongs(int count)                    // read into a byte array
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Read);
            int size = sizeof(long);
            var data = new long[count];
            var p = AlignArrayPtr(size, count);
            if (p.Item2 == size)
                System.Runtime.InteropServices.Marshal.Copy(p.Item1, data, 0, data.Length);
            else
            {
                var temp = new long[count * 4];
                System.Runtime.InteropServices.Marshal.Copy(p.Item1, data, 0, temp.Length);
                for (int i = 0; i < data.Length; i++)
                    data[i] = temp[i * 4];
            }
            return data;
        }

        public long ReadLong()
        {
            var data = new long[1];
            IntPtr pos = AlignScalarPtr(sizeof(long));
            System.Runtime.InteropServices.Marshal.Copy(pos, data, 0, 1);
            return data[0];
        }

        public float[] ReadFloats(int count)                    // read into a byte array
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Read);
            int size = sizeof(float);
            var data = new float[count];
            var p = AlignArrayPtr(size, count);
            if (p.Item2 == size)
                System.Runtime.InteropServices.Marshal.Copy(p.Item1, data, 0, data.Length);
            else
            {
                var temp = new float[count * 4];
                System.Runtime.InteropServices.Marshal.Copy(p.Item1, data, 0, temp.Length);
                for (int i = 0; i < data.Length; i++)
                    data[i] = temp[i * 4];
            }
            return data;
        }

        public float ReadFloat()
        {
            var data = new float[1];
            IntPtr pos = AlignScalarPtr(sizeof(float));
            System.Runtime.InteropServices.Marshal.Copy(pos, data, 0, 1);
            return data[0];
        }

        public Vector4[] ReadVector4s(int count)                    // read into a byte array
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.Read);
            var p = AlignArrayPtr(Vec4size,count);
            float[] fdata = new float[count*4];
            System.Runtime.InteropServices.Marshal.Copy(p.Item1, fdata, 0, count*4);
            Vector4[] data = new Vector4[count];
            for (int i = 0; i < count; i++)
                data[i] = new Vector4(fdata[i * 4], fdata[i * 4+1], fdata[i * 4 + 2], fdata[i * 4 + 3]);
            return data;
        }

        public Vector4 ReadVector4()
        {
            var data = new float[4];
            IntPtr pos = AlignScalarPtr(Vec4size);
            System.Runtime.InteropServices.Marshal.Copy(pos, data, 0, 4);
            return new Vector4(data[0], data[1], data[2], data[3]);
        }

        #endregion

        #region Fast Read functions

        public byte[] ReadBuffer(int offset, int len)
        {
            StartRead(offset,len);
            var v = ReadBytes(len);
            StopReadWrite();
            return v;
        }

        public int ReadInt(int offset)
        {
            StartRead(offset);
            int v = ReadInt();
            StopReadWrite();
            return v;
        }

        public int[] ReadInts(int offset, int number)
        {
            StartRead(offset);
            var v = ReadInts(number);
            StopReadWrite();
            return v;
        }

        public float[] ReadFloats(int offset, int number)
        {
            StartRead(offset);
            var v = ReadFloats(number);
            StopReadWrite();
            return v;
        }

        public Vector4[] ReadVector4(int offset, int number)
        {
            StartRead(offset);
            var v = ReadVector4s(number);
            StopReadWrite();
            return v;
        }

        #endregion

        #region Binding a buffer to target 

        public void Bind(GLVertexArray va, int bindingindex, int start, int stride, int divisor = 0)      // set buffer binding to a VA
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.None);     // catch unmap missing. Since binding to VA can be done before buffer is full, then don't check BufferSize
            va.Bind();
            GL.BindVertexBuffer(bindingindex, Id, (IntPtr)start, stride);      // this buffer to binding index
            GL.VertexBindingDivisor(bindingindex, divisor);
            OpenTKUtils.GLStatics.Check();
            //System.Diagnostics.Debug.WriteLine("BUFBIND " + bindingindex + " To B" + Id + " pos " + start + " stride " + stride + " divisor " + divisor);
        }

        public void BindElement()
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.None && BufferSize > 0);     // catch unmap missing or nothing in buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Id);
            OpenTKUtils.GLStatics.Check();
        }

        public void BindIndirect()
        {
            System.Diagnostics.Debug.Assert(mapmode == MapMode.None && BufferSize > 0);     // catch unmap missing or nothing in buffer
            GL.BindBuffer(BufferTarget.DrawIndirectBuffer, Id);
            OpenTKUtils.GLStatics.Check();
        }

        public void Bind(int bindingindex,  BufferRangeTarget tgr)                           // Bind to a arbitary buffer target
        {
            GL.BindBufferBase(tgr, bindingindex, Id);       // binding point set to tgr
        }

        // tbd remove when we are sure
        //public void Bind(int bindingindex, BufferTarget target, BufferRangeTarget tgr) // Bind to a arbitary buffer target
        //{
        //    //  GL.BindBuffer(target, Id);          // bind buffer ID to target type
        //    GL.BindBufferBase(tgr, bindingindex, Id);       // binding point set.
        //    GL.BindBuffer(target, 0);           // unbind
        //                                                    //    GL.BindBuffer(target, 0);           // unbind
        //}

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