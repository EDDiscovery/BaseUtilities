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
using System.Linq;

namespace OpenTKUtils.GL4
{
    // Standard renderable item supporting Instancing and draw count, vertex arrays, instance data, element indexes, indirect command buffers
    // A Renderable Item must implement Bind, Render and have a Primitive Type
    // An item has a Draw count and Instance count
    // It has a primitive type
    // it is associated with an optional VertexArray which is bound using Bind()
    // it is associated with an optional InstanceData which is instanced using Bind()
    // it is associated with an optional ElementBuffer giving vertex indices for all vertex inputs
    // it is associated with an optional IndirectBuffer giving draw command groups

    // Renderable items are normally put into a GLRenderProgramSortedList by shader, but can be executed directly
    // using Execute.  This is normally only done for renders which do not produce output but compute to a buffer.

    public class GLRenderableItem : IGLRenderableItem
    {
        public bool Visible { get; set; } = true;                           // is visible?

        public GLRenderControl RenderControl { get; set; }                  // Draw type and other GL states needed for a correct render

        public IGLVertexArray VertexArray { get; set; }                     // may be null - if so no vertex data. Does not own

        // we can draw either arrays (A); element index (E); indirect arrays (IA); indirect element index (IE)
        // type is controlled by if ElementBuffer and/or IndirectBuffer is non null

        public int DrawCount { get; set; } = 0;                             // A+E : Draw count (not used in indirect - this comes from the buffer)

        public int InstanceCount { get; set; } = 1;                         // A+E: Instances (not used in indirect - this comes from the buffer)
        public int BaseInstance { get; set; } = 0;                          // A+E: Base Instance, normally 0 (not used in indirect - this comes from the buffer)

        public GLBuffer ElementBuffer { get; set; }                         // E+IE: if non null, we doing a draw using a element buffer to control indexes
        public GLBuffer IndirectBuffer { get; set; }                        // IA+IE: if non null, we doing a draw using a indirect buffer to control draws

        public DrawElementsType DrawType { get; set; }                      // E+IE: for element draws, its index type (byte/short/uint)

        public int BaseIndex { get; set; }                                  // E: for element draws, first index element in the element index buffer to use, offset to use different groups. 
                                                                            // IE+IA: offset in buffer to first command entry in bytes

        public int BaseVertex { get; set; }                                 // E: for element draws (but not indirect) first vertex to use in the buffer (not used in indirect - this comes from the buffer)

        public int MultiDrawCount { get; set; } = 1;                        // IE+IA: number of draw command buffers 
        public int MultiDrawCountStride { get; set; } = 20;                 // IE+IA: distance between each command buffer entry (default is we use the maximum of elements+array structures)

        public IGLRenderItemData RenderData { get; set; }                   // may be null - no specific render data. Does not own.  called at bind

        public GLRenderableItem(GLRenderControl rc, int drawcount, IGLVertexArray va, IGLRenderItemData id = null, int ic = 1)
        {
            RenderControl = rc;
            DrawCount = drawcount;
            VertexArray = va;
            RenderData = id;
            InstanceCount = ic;
        }

        public void Bind(GLRenderControl currentstate, IGLProgramShader shader, GLMatrixCalc c)      // called by Render() to bind data to GL, vertex and InstanceData
        {
            if (currentstate != null)
                currentstate.ApplyState(RenderControl);         // go to this state first

            VertexArray?.Bind();      
            RenderData?.Bind(this,shader,c);
            ElementBuffer?.BindElement();
            IndirectBuffer?.BindIndirect();
        }

        public void Render()                                               // called by Render() to draw the item.
        {
            //System.Diagnostics.Debug.WriteLine("Draw " + RenderControl.PrimitiveType + " " + DrawCount + " " + InstanceCount);

            if ( ElementBuffer != null )
            {
                if (IndirectBuffer != null)                         // IE
                {               
                    GL.MultiDrawElementsIndirect(RenderControl.PrimitiveType, DrawType, (IntPtr)BaseIndex, MultiDrawCount, MultiDrawCountStride);
                }
                else
                {                                                   // E
                    GL.DrawElementsInstancedBaseVertexBaseInstance(RenderControl.PrimitiveType, DrawCount, DrawType, (IntPtr)BaseIndex, 
                                                                    InstanceCount, BaseVertex, BaseInstance);
                }
            }
            else
            {
                if (IndirectBuffer != null)                         // IA
                {
                    GL.MultiDrawArraysIndirect(RenderControl.PrimitiveType, (IntPtr)BaseIndex, MultiDrawCount, MultiDrawCountStride);
                }
                else
                {                                                   // A
                    GL.DrawArraysInstancedBaseInstance(RenderControl.PrimitiveType, 0, DrawCount, InstanceCount, BaseInstance);       // Type A
                }
            }
        }

        #region These create a new RI with vertex arrays into buffers, lots of them 

        // Vector4, Color4, optional instance data and count

        // in attribute 0 and 1 setup vector4 and vector4 colours
        public static GLRenderableItem CreateVector4Color4(GLItemsList items, GLRenderControl pt, Vector4[] vectors, Color4[] colours, IGLRenderItemData id = null, int ic = 1)
        {
            var vb = items.NewBuffer();
            vb.AllocateBytes(GLBuffer.Vec4size * vectors.Length * 2);
            vb.Fill(vectors);
            vb.Fill(colours, vectors.Length);

            var va = items.NewArray();
            vb.Bind(va, 0, vb.Positions[0], 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);
            vb.Bind(va, 1, vb.Positions[1], 16);
            va.Attribute(1, 1, 4, VertexAttribType.Float);
            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 0 set up
        public static GLRenderableItem CreateVector4(GLItemsList items, GLRenderControl pt, Vector4[] vectors, IGLRenderItemData id = null, int ic = 1)
        {
            var vb = items.NewBuffer();
            vb.AllocateFill(vectors);

            var va = items.NewArray();
            vb.Bind(va, 0, vb.Positions[0], 16);        // bind buffer to binding point 0
            va.Attribute(0, 0, 4, VertexAttribType.Float);  // bind binding point 0 to attribute point 0 with 4 float components
            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 0 set up. Use a buffer, Must set up drawcount, or set to 0 and reset it later..
        public static GLRenderableItem CreateVector4(GLItemsList items, GLRenderControl pt, GLBuffer vb, int drawcount, int pos = 0, IGLRenderItemData id = null, int ic = 1)
        {
            var va = items.NewArray();
            vb.Bind(va, 0, pos, 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);
            return new GLRenderableItem(pt, drawcount, va, id, ic);
        }

        // in 0,1 set up.  Second vector can be instance divided
        public static GLRenderableItem CreateVector4Vector4(GLItemsList items, GLRenderControl pt, Vector4[] vectors, Vector4[] secondvector, IGLRenderItemData id = null, int ic = 1, int seconddivisor = 0)
        {
            var vb = items.NewBuffer();
            vb.AllocateBytes(GLBuffer.Vec4size * vectors.Length + GLBuffer.Vec4size * secondvector.Length);
            vb.Fill(vectors);
            vb.Fill(secondvector);

            var va = items.NewArray();
            vb.Bind(va, 0, vb.Positions[0], 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);

            vb.Bind(va, 1, vb.Positions[1], 16, seconddivisor);
            va.Attribute(1, 1, 4, VertexAttribType.Float);
            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 0,1 set up. Two seperate buffers.  Second vector can be instance divided
        public static GLRenderableItem CreateVector4Vector4Buf2(GLItemsList items, GLRenderControl pt, Vector4[] vectors, Vector4[] secondvector, IGLRenderItemData id = null, int ic = 1, int seconddivisor = 0)
        {
            var v1 = items.NewBuffer();
            v1.AllocateBytes(GLBuffer.Vec4size * vectors.Length );
            v1.Fill(vectors);

            var v2 = items.NewBuffer();
            v2.AllocateBytes(GLBuffer.Vec4size * secondvector.Length);
            v2.Fill(secondvector);

            var va = items.NewArray();
            v1.Bind(va, 0, v1.Positions[0], 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);

            v2.Bind(va, 1, v2.Positions[0], 16, seconddivisor);
            va.Attribute(1, 1, 4, VertexAttribType.Float);
            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 0,1 set up. With second buffer given to us. Second vector can be instance divided
        public static GLRenderableItem CreateVector4Vector4(GLItemsList items, GLRenderControl pt, Vector4[] vectors, GLBuffer buf2, int bufoff = 0, IGLRenderItemData id = null, int ic = 1, int seconddivisor = 0)
        {
            var vb = items.NewBuffer();
            vb.AllocateBytes(GLBuffer.Vec4size * vectors.Length);
            vb.Fill(vectors);

            var va = items.NewArray();
            vb.Bind(va, 0, vb.Positions[0], 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);

            buf2.Bind(va, 1, bufoff, 16, seconddivisor);
            va.Attribute(1, 1, 4, VertexAttribType.Float);
            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 0,1 set up. With two given buffers and positions, Second buffer can be instance divided
        public static GLRenderableItem CreateVector4Vector4(GLItemsList items, GLRenderControl pt, GLBuffer buf1, int buf1off, int drawcount, 
                                    GLBuffer buf2, int buf2off = 0, IGLRenderItemData id = null, int ic = 1, int seconddivisor = 0)
        {
            var va = items.NewArray();
            buf1.Bind(va, 0, buf1off, 16);          // binding index 0
            va.Attribute(0, 0, 4, VertexAttribType.Float);  // attribute 0

            buf2.Bind(va, 1, buf2off, 16, seconddivisor);   // binding index 1
            va.Attribute(1, 1, 4, VertexAttribType.Float);  // attribute 1
            return new GLRenderableItem(pt,drawcount, va, id, ic);
        }

        // in 0,1 set up. Second vector can be instance divided
        public static GLRenderableItem CreateVector4Vector2(GLItemsList items, GLRenderControl pt, Vector4[] vectors, Vector2[] coords, IGLRenderItemData id = null, int ic = 1, int seconddivisor = 0)
        {
            var vb = items.NewBuffer();
            vb.AllocateBytes(GLBuffer.Vec4size * vectors.Length + GLBuffer.Vec2size * coords.Length);
            vb.Fill(vectors);
            vb.Fill(coords);

            var va = items.NewArray();
            vb.Bind(va, 0, vb.Positions[0], 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);
            vb.Bind(va, 1, vb.Positions[1], 8, seconddivisor);
            va.Attribute(1, 1, 2, VertexAttribType.Float);
            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 0,1 set up. Second vector can be instance divided
        public static GLRenderableItem CreateVector4Vector2(GLItemsList items, GLRenderControl pt, GLBuffer vb, int pos1, int pos2, int drawcount, IGLRenderItemData id = null, int ic = 1, int seconddivisor = 0)
        {
            var va = items.NewArray();
            vb.Bind(va, 0, vb.Positions[0], 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);
            vb.Bind(va, 1, vb.Positions[1], 8, seconddivisor);
            va.Attribute(1, 1, 2, VertexAttribType.Float);
            return new GLRenderableItem(pt, drawcount, va, id, ic);
        }

        // using output of some shape generators
        public static GLRenderableItem CreateVector4Vector2(GLItemsList items, GLRenderControl pt, Tuple<Vector4[], Vector2[]> vectors, IGLRenderItemData id = null, int ic = 1, int seconddivisor = 0)
        {
            return CreateVector4Vector2(items, pt, vectors.Item1, vectors.Item2, id, ic, seconddivisor);
        }

        // using output of some shape generators, with element buffer indices. Normally Vertex, UVs, element indexes
        public static GLRenderableItem CreateVector4Vector2Indexed(GLItemsList items, GLRenderControl pt, Tuple<Vector4[], Vector2[], uint[]> vectors, IGLRenderItemData id = null, int ic = 1, int seconddivisor = 0)
        {
            var dt = GL4Statics.DrawElementsTypeFromMaxEID((uint)vectors.Item1.Length - 1);
            var ri = CreateVector4Vector2(items, pt, vectors.Item1, vectors.Item2, id, ic, seconddivisor);
            ri.CreateElementIndex(items.NewBuffer(), vectors.Item3, dt);
            return ri;
        }

        public static GLRenderableItem CreateVector4Vector2Vector4(GLItemsList items, GLRenderControl pt,
                                                                   Tuple<Vector4[],Vector2[]> pos, 
                                                                    Vector4[] instanceposition,
                                                                   IGLRenderItemData id = null, int ic = 1,
                                                                   bool separbuf = false, int divisorinstance = 1)
        {
            return CreateVector4Vector2Vector4(items, pt, pos.Item1, pos.Item2, instanceposition, id, ic, separbuf, divisorinstance);
        }

        // in 0,1,4 set up.  
        // if separbuffer = true it makes as separate buffer for instanceposition
        // if separbuffer = true and instanceposition is null, you fill up the separbuffer yourself outside of this (maybe auto generated).  
        // You can get this buffer using items.LastBuffer()
        public static GLRenderableItem CreateVector4Vector2Vector4(GLItemsList items, GLRenderControl pt,
                                                                   Vector4[] vectors, Vector2[] coords, Vector4[] instanceposition,
                                                                   IGLRenderItemData id = null, int ic = 1,
                                                                   bool separbuf = false, int divisorinstance = 1)
        {
            var va = items.NewArray();
            var vbuf1 = items.NewBuffer();
            var vbuf2 = vbuf1;
            int posi = 2;

            if (separbuf)
            {
                vbuf1.AllocateBytes(GLBuffer.Vec4size * vectors.Length + GLBuffer.Vec2size * coords.Length);
                vbuf2 = items.NewBuffer();

                if (instanceposition != null)
                    vbuf2.AllocateBytes(GLBuffer.Vec4size * instanceposition.Length);

                posi = 0;
            }
            else
            {
                vbuf1.AllocateBytes(GLBuffer.Vec4size * vectors.Length + GLBuffer.Vec2size * coords.Length + GLBuffer.Vec4size * instanceposition.Length + GLBuffer.Vec4size);    // due to alignment, add on a little
            }

            vbuf1.Fill(vectors);
            vbuf1.Fill(coords);
            if (instanceposition != null)
                vbuf2.Fill(instanceposition);

            vbuf1.Bind(va, 0, vbuf1.Positions[0], 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);

            vbuf1.Bind(va, 1, vbuf1.Positions[1], 8);
            va.Attribute(1, 1, 2, VertexAttribType.Float);

            vbuf2.Bind(va, 2, instanceposition!=null ? vbuf2.Positions[posi] : 0, 16, divisorinstance);
            va.Attribute(2, 2, 4, VertexAttribType.Float);

            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 0,1,4-7 set up.  if separbuffer = true and instancematrix is null, it makes a buffer for you to fill up externally.
        // if separbuffer = true it makes as separate buffer for instancematrix
        // if separbuffer = true and instancematrix is null, you fill up the separbuffer yourself outside of this (maybe auto generated).  
        // You can get this buffer using items.LastBuffer()

        public static GLRenderableItem CreateVector4Vector2Matrix4(GLItemsList items, GLRenderControl pt, 
                                                                    Vector4[] vectors, Vector2[] coords, Matrix4[] instancematrix, 
                                                                    IGLRenderItemData id = null, int ic = 1, 
                                                                    bool separbuf = false, int matrixdivisor = 1)
        {
            var va = items.NewArray();
            GLBuffer vbuf1 = items.NewBuffer();
            GLBuffer vbuf2 = vbuf1;
            int posi = 2;

            if (separbuf)
            {
                vbuf1.AllocateBytes(GLBuffer.Vec4size * vectors.Length + GLBuffer.Vec2size * coords.Length);

                vbuf2 = items.NewBuffer();

                if (instancematrix != null)
                    vbuf2.AllocateBytes(GLBuffer.Mat4size * instancematrix.Length);

                posi = 0;
            }
            else
            { 
                vbuf1.AllocateBytes(GLBuffer.Vec4size * vectors.Length + GLBuffer.Vec2size * coords.Length + GLBuffer.Vec4size + GLBuffer.Mat4size * instancematrix.Length);    // due to alignment, add on a little
            }

            vbuf1.Fill(vectors);
            vbuf1.Fill(coords);
            if ( instancematrix != null )
                vbuf2.Fill(instancematrix);

            vbuf1.Bind(va, 0, vbuf1.Positions[0], 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);      // bp 0 at 0 

            vbuf1.Bind(va, 1, vbuf1.Positions[1], 8);
            va.Attribute(1, 1, 2, VertexAttribType.Float);      // bp 1 at 1

            va.MatrixAttribute(2, 4);                           // bp 2 at 4-7
            vbuf2.Bind(va, 2, instancematrix != null ? vbuf2.Positions[posi]: 0, 64, matrixdivisor);     // use a binding 

            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 0,4-7 set up
        public static GLRenderableItem CreateVector4Matrix4(GLItemsList items, GLRenderControl pt, Vector4[] vectors, Matrix4[] matrix, IGLRenderItemData id = null, int ic = 1, int matrixdivisor = 1)
        {
            var vb = items.NewBuffer();

            vb.AllocateBytes(GLBuffer.Vec4size * vectors.Length + GLBuffer.Mat4size * matrix.Length);
            vb.Fill(vectors);
            vb.Fill(matrix);

            var va = items.NewArray();

            vb.Bind(va, 0, vb.Positions[0], 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);      // bp 0 at attrib 0

            vb.Bind(va, 2, vb.Positions[1], 64, matrixdivisor);     // use a binding 
            va.MatrixAttribute(2, 4);                           // bp 2 at attribs 4-7

            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 4-7 set up
        public static GLRenderableItem CreateMatrix4(GLItemsList items, GLRenderControl pt, Matrix4[] matrix, int drawcount, IGLRenderItemData id = null, int ic = 1, int matrixdivisor = 1)
        {
            var vb = items.NewBuffer();

            vb.AllocateBytes(GLBuffer.Mat4size * matrix.Length);
            vb.Fill(matrix);

            var va = items.NewArray();

            vb.Bind(va, 2, vb.Positions[0], 64, matrixdivisor);     // use a binding 
            va.MatrixAttribute(2, 4);                           // bp 2 at attribs 4-7

            return new GLRenderableItem(pt, drawcount, va, id, ic);
        }

        // in 4-7 set up
        public static GLRenderableItem CreateMatrix4(GLItemsList items, GLRenderControl pt, GLBuffer vb, int drawcount, IGLRenderItemData id = null, int ic = 1, int matrixdivisor = 1)
        {
            var ri = new GLRenderableItem(pt, drawcount, null, id, ic);
            ri.CreateMatrix4(items, vb, matrixdivisor);
            return ri;
        }

        // in 4-7 set up - create over the top of an existing RI
        public void CreateMatrix4(GLItemsList items, GLBuffer vb, int matrixdivisor = 1)
        {
            VertexArray?.Dispose();
            var va = items.NewArray();
            VertexArray = va;

            vb.Bind(va, 2, vb.Positions[0], 64, matrixdivisor);     // use a binding 
            va.MatrixAttribute(2, 4);                           // bp 2 at attribs 4-7
        }

        // in 0 set up
        public static GLRenderableItem CreateVector3Packed2(GLItemsList items, GLRenderControl pt, Vector3[] vectors, Vector3 offsets, float mult, IGLRenderItemData id = null, int ic = 1)
        {
            var vb = items.NewBuffer();
            vb.AllocateBytes(sizeof(uint) * 2 * vectors.Length);
            vb.FillPacked2vec(vectors, offsets, mult);

            var va = items.NewArray();
            vb.Bind(va, 0, vb.Positions[0], 8);
            va.AttributeI(0, 0, 2, VertexAttribType.UnsignedInt);

            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 0 set up floats with configurable components numbers
        public static GLRenderableItem CreateFloats(GLItemsList items, GLRenderControl pt, float[] floats, int components, IGLRenderItemData id = null, int ic = 1)
        {
            var vb = items.NewBuffer();
            vb.AllocateFill(floats);
            //float[] ouwt = vb.ReadFloats(0, floats.Length); // test read back

            var va = items.NewArray();
            vb.Bind(va, 0, vb.Positions[0], sizeof(float)*components);    
            va.Attribute(0, 0, components, VertexAttribType.Float);

            return new GLRenderableItem(pt, floats.Length / components, va, id, ic);
        }

        // no data into GL pipeline.  Used when a uniform buffer gives info for the vertex shader to create vertices

        public static GLRenderableItem CreateNullVertex(GLRenderControl pt, IGLRenderItemData id = null, int dc =1,  int ic = 1)
        {
            return new GLRenderableItem(pt, dc, null, id, ic);  // no vertex data.
        }

        #endregion

        #region Create element indexs for this RI. Normally called after Create..

        public void CreateRectangleElementIndexByte(GLBuffer elementbuf, int reccount, int restartindex = 0xff)
        {
            ElementBuffer = elementbuf;
            ElementBuffer.FillRectangularIndicesBytes(reccount, restartindex);
            DrawType = DrawElementsType.UnsignedByte;
            DrawCount = ElementBuffer.Length - 1;

            //byte[] b = elementbuf.ReadBuffer(0, elementbuf.BufferSize); // test read back
        }

        public void CreateRectangleElementIndexUShort(GLBuffer elementbuf, int reccount, int restartindex = 0xffff)
        {
            ElementBuffer = elementbuf;
            ElementBuffer.FillRectangularIndicesShort(reccount, restartindex);
            DrawType = DrawElementsType.UnsignedShort;
            DrawCount = ElementBuffer.Length - 1;
        }

        public void CreateElementIndexByte(GLBuffer elementbuf, byte[] indexes, int base_index = 0)
        {
            ElementBuffer = elementbuf;
            ElementBuffer.AllocateFill(indexes);
            DrawType = DrawElementsType.UnsignedByte;
            BaseIndex = base_index;
            DrawCount = indexes.Length;
        }

        // create an index, to the drawtype size
        public void CreateElementIndex(GLBuffer elementbuf, uint[] eids, int base_index = 0)
        {
            CreateElementIndex(elementbuf, eids, GL4Statics.DrawElementsTypeFromMaxEID(eids.Max()), base_index);
        }

        // create an index, to the drawtype size
        public void CreateElementIndex(GLBuffer elementbuf, uint[] eids, DrawElementsType drawtype, int base_index = 0)
        {
            ElementBuffer = elementbuf;

            if (drawtype == DrawElementsType.UnsignedByte)
            {
                ElementBuffer.AllocateFill(eids.Select(x => (byte)x).ToArray());
            }
            else if (drawtype == DrawElementsType.UnsignedShort)
            {
                ElementBuffer.AllocateFill(eids.Select(x => (ushort)x).ToArray());
                DrawType = DrawElementsType.UnsignedShort;
            }
            else
            {
                ElementBuffer.AllocateFill(eids.ToArray());
                DrawType = DrawElementsType.UnsignedInt;
            }

            DrawType = drawtype;
            BaseIndex = base_index;
            DrawCount = eids.Length;
        }

        #endregion

        #region Execute without a List.. usually used if shader is not rendering to screen, but is computing, and that would normally have discard=true

        public void Execute( IGLProgramShader shader , GLRenderControl state, GLMatrixCalc c = null, bool discard = true )
        {
            if (discard)
                GL.Enable(EnableCap.RasterizerDiscard);

            shader.Start();
            Bind(state, shader, c);
            Render();
            shader.Finish();

            if (discard)
                GL.Disable(EnableCap.RasterizerDiscard);
        }

        #endregion

    }

}

