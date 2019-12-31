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


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;

namespace OpenTKUtils.GL4
{
    // Standard renderable item supporting Instancing and draw count, vertex arrays, instance data, element indexes, indirect command buffers
    // A Renderable Item must implement Bind, Render and have a Primitive Type
    // An item has a Draw count and Instance count
    // It has a primitive type
    // it is associated with an optional VertexArray which is bound using Bind()
    // it is associated with an optional InstanceData which is instanced using Bind()
    // it is associated with an optional ElementBuffer giving vertex indices
    // it is associated with an optional IndirectBuffer giving draw command groups

    public class GLRenderableItem : IGLRenderableItem
    {
        public PrimitiveType PrimitiveType { get; set; }                    // Draw type
        public IGLVertexArray VertexArray { get; set; }                     // may be null - if so no vertex data. Does not own

        // we can draw either arrays (A); element index (E); indirect arrays (IA); indirect element index (IE)

        public int DrawCount { get; set; } = 0;                             // A+E : Draw count (not used in indirect - this comes from the buffer)

        public int InstanceCount { get; set; } = 1;                         // A+E: Instances (not used in indirect - this comes from the buffer)
        public int BaseInstance { get; set; } = 0;                          // A+E: Base Instance, normally 0 (not used in indirect - this comes from the buffer)

        public GLBuffer ElementBuffer { get; set; }                         // E+IE: if non null, we doing a draw using a element buffer to control indexes
        public GLBuffer IndirectBuffer { get; set; }                        // IA+IE: if non null, we doing a draw using a indirect buffer to control draws

        public DrawElementsType DrawType { get; set; }                      // E+IE: for element draws, its index type (byte/short/uint)

        public int BaseIndex { get; set; }                                  // E: for element draws, first index element in this buffer to use, offset to use different groups. 
                                                                            // IE+IA: offset in buffer to first command entry in bytes

        public int BaseVertex { get; set; }                                 // E: for element draws (but not indirect) first vertex to use (not used in indirect - this comes from the buffer)

        public int MultiDrawCount { get; set; } = 1;                        // IE+IA: number of draw command buffers 
        public int MultiDrawCountStride { get; set; } = 20;                 // IE+IA: distance between each command buffer entry (default is we use the maximum of elements+array structures)

        public IGLInstanceControl InstanceControl { get; set; }             // may be null - no instance data. Does not own.

        public GLRenderableItem(PrimitiveType pt, int drawcount, IGLVertexArray va, IGLInstanceControl id = null, int ic = 1)
        {
            PrimitiveType = pt;
            DrawCount = drawcount;
            VertexArray = va;
            InstanceControl = id;
            InstanceCount = ic;
        }

        public void Bind(IGLProgramShader shader, Common.MatrixCalc c)      // called by Render() to bind data to GL, vertex and InstanceData
        {
            VertexArray?.Bind();
            InstanceControl?.Bind(shader,this,c);
            if (ElementBuffer != null)
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBuffer.Id);
            if (IndirectBuffer != null)
                GL.BindBuffer(BufferTarget.DrawIndirectBuffer, IndirectBuffer.Id);
        }

        public void Render()                                                // called by Render() to draw the item.
        {
            // System.Diagnostics.Debug.WriteLine("Render " + PrimitiveType + " " + DrawCount + " " + InstanceCount);

            if ( ElementBuffer != null )
            {
                if (IndirectBuffer != null)                         // IE
                {               
                    GL.MultiDrawElementsIndirect(PrimitiveType, DrawType, (IntPtr)BaseIndex, MultiDrawCount, MultiDrawCountStride);
                }
                else
                {                                                   // E
                    GL.DrawElementsInstancedBaseVertexBaseInstance(PrimitiveType, DrawCount, DrawType, (IntPtr)BaseIndex, InstanceCount, BaseVertex, BaseInstance);
                }
            }
            else
            {
                if (IndirectBuffer != null)                         // IA
                {
                    GL.MultiDrawArraysIndirect(PrimitiveType, (IntPtr)BaseIndex, MultiDrawCount, MultiDrawCountStride);
                }
                else
                {                                                   // A
                    GL.DrawArraysInstancedBaseInstance(PrimitiveType, 0, DrawCount, InstanceCount, BaseInstance);       // Type A
                }
            }
        }

        #region These create new vertext arrays into buffers of vertexs and colours

        // Vector4, Color4, optional instance data and count

        public static GLRenderableItem CreateVector4Color4(GLItemsList items, PrimitiveType pt, Vector4[] vectors, Color4[] colours, IGLInstanceControl id = null, int ic = 1)
        {
            var vb = items.NewBuffer();
            vb.Allocate(GLBuffer.Vec4size * vectors.Length * 2);
            vb.Fill(vectors);
            vb.Fill(colours,vectors.Length);

            var va = items.NewArray();
            vb.Bind(0, vb.Positions[0], 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);
            vb.Bind(1, vb.Positions[1], 16);
            va.Attribute(1, 1, 4, VertexAttribType.Float);
            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 0 set up
        public static GLRenderableItem CreateVector4(GLItemsList items, PrimitiveType pt, Vector4[] vectors, IGLInstanceControl id = null, int ic = 1)
        {
            var vb = items.NewBuffer();
            vb.Allocate(GLBuffer.Vec4size * vectors.Length);
            vb.Fill(vectors);

            var va = items.NewArray();
            vb.Bind(0, vb.Positions[0], 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);
            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 0 set up. Use a buffer setup, Must set up drawcount, or set to 0 and reset it later..
        public static GLRenderableItem CreateVector4(GLItemsList items, PrimitiveType pt, GLBuffer vb, int drawcount, int pos = 0, IGLInstanceControl id = null, int ic = 1)
        {
            var va = items.NewArray();
            vb.Bind(0, pos, 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);
            return new GLRenderableItem(pt, drawcount, va, id, ic);
        }

        // in 0,1 set up
        public static GLRenderableItem CreateVector4Vector2(GLItemsList items, PrimitiveType pt, Vector4[] vectors, Vector2[] coords, IGLInstanceControl id = null, int ic = 1)
        {
            var vb = items.NewBuffer();
            vb.Allocate(GLBuffer.Vec4size * vectors.Length + GLBuffer.Vec2size * coords.Length);
            vb.Fill(vectors);
            vb.Fill(coords);

            var va = items.NewArray();
            vb.Bind(0, vb.Positions[0], 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);
            vb.Bind(1, vb.Positions[1], 8);
            va.Attribute(1, 1, 2, VertexAttribType.Float);
            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        public static GLRenderableItem CreateVector4Vector2(GLItemsList items, PrimitiveType pt, Tuple<Vector4[], Vector2[]> vectors, IGLInstanceControl id = null, int ic = 1)
        {
            return CreateVector4Vector2(items, pt, vectors.Item1, vectors.Item2, id, ic);
        }

        public static GLRenderableItem CreateVector4Vector2Vector4(GLItemsList items, PrimitiveType pt,
                                                                   Tuple<Vector4[],Vector2[]> pos, 
                                                                    Vector4[] instanceposition,
                                                                   IGLInstanceControl id = null, int ic = 1,
                                                                   bool separbuf = false, int divisorinstance = 1)
        {
            return CreateVector4Vector2Vector4(items, pt, pos.Item1, pos.Item2, instanceposition, id, ic, separbuf, divisorinstance);
        }

        // in 0,1,4 set up.  if separbuffer = true and instanceposition is null, it makes a buffer for you to fill up externally.
        public static GLRenderableItem CreateVector4Vector2Vector4(GLItemsList items, PrimitiveType pt,
                                                                   Vector4[] vectors, Vector2[] coords, Vector4[] instanceposition,
                                                                   IGLInstanceControl id = null, int ic = 1,
                                                                   bool separbuf = false, int divisorinstance = 1)
        {
            var va = items.NewArray();
            var vbuf1 = items.NewBuffer();
            GLBuffer vbuf2 = vbuf1;
            int posi = 2;

            if (separbuf)
            {
                vbuf1.Allocate(GLBuffer.Vec4size * vectors.Length + GLBuffer.Vec2size * coords.Length);
                vbuf2 = items.NewBuffer();

                if ( instanceposition != null )
                    vbuf2.Allocate(GLBuffer.Vec4size * instanceposition.Length);

                posi = 0;
            }
            else
            {
                vbuf1.Allocate(GLBuffer.Vec4size * vectors.Length + GLBuffer.Vec2size * coords.Length + GLBuffer.Vec4size * instanceposition.Length + GLBuffer.Vec4size);    // due to alignment, add on a little
            }

            vbuf1.Fill(vectors);
            vbuf1.Fill(coords);
            vbuf2.Fill(instanceposition);

            vbuf1.Bind(0, vbuf1.Positions[0], 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);

            vbuf1.Bind(1, vbuf1.Positions[1], 8);
            va.Attribute(1, 1, 2, VertexAttribType.Float);

            vbuf2.Bind(2, instanceposition!=null ? vbuf2.Positions[posi] : 0, 16, divisorinstance);
            va.Attribute(2, 2, 4, VertexAttribType.Float);

            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 0,1,4-7 set up.  if separbuffer = true and instancematrix is null, it makes a buffer for you to fill up externally.
        public static GLRenderableItem CreateVector4Vector2Matrix4(GLItemsList items, PrimitiveType pt, 
                                                                    Vector4[] vectors, Vector2[] coords, Matrix4[] instancematrix, 
                                                                    IGLInstanceControl id = null, int ic = 1, 
                                                                    bool separbuf = false, int matrixdivisor = 1)
        {
            var va = items.NewArray();
            GLBuffer vbuf1 = items.NewBuffer();
            GLBuffer vbuf2 = vbuf1;
            int posi = 2;

            if (separbuf)
            {
                vbuf1.Allocate(GLBuffer.Vec4size * vectors.Length + GLBuffer.Vec2size * coords.Length);

                vbuf2 = items.NewBuffer();

                if (instancematrix != null)
                    vbuf2.Allocate(GLBuffer.Mat4size * instancematrix.Length);

                posi = 0;
            }
            else
            { 
                vbuf1.Allocate(GLBuffer.Vec4size * vectors.Length + GLBuffer.Vec2size * coords.Length + GLBuffer.Vec4size + GLBuffer.Mat4size * instancematrix.Length);    // due to alignment, add on a little
            }

            vbuf1.Fill(vectors);
            vbuf1.Fill(coords);
            if ( instancematrix != null )
                vbuf2.Fill(instancematrix);

            vbuf1.Bind(0, vbuf1.Positions[0], 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);      // bp 0 at 0 

            vbuf1.Bind(1, vbuf1.Positions[1], 8);
            va.Attribute(1, 1, 2, VertexAttribType.Float);      // bp 1 at 1

            va.MatrixAttribute(2, 4);                           // bp 2 at 4-7
            vbuf2.Bind(2, instancematrix != null ? vbuf2.Positions[posi]: 0, 64, matrixdivisor);     // use a binding 

            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 0,4-7 set up
        public static GLRenderableItem CreateVector4Matrix4(GLItemsList items, PrimitiveType pt, Vector4[] vectors, Matrix4[] matrix, IGLInstanceControl id = null, int ic = 1, int matrixdivisor=1)
        {
            var vb = items.NewBuffer();

            vb.Allocate(GLBuffer.Vec4size * vectors.Length + GLBuffer.Mat4size * matrix.Length);    
            vb.Fill(vectors);
            vb.Fill(matrix);

            var va = items.NewArray();

            vb.Bind(0, vb.Positions[0], 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);      // bp 0 at attrib 0

            vb.Bind(2, vb.Positions[1], 64, matrixdivisor);     // use a binding 
            va.MatrixAttribute(2, 4);                           // bp 2 at attribs 4-7

            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 0 set up
        public static GLRenderableItem CreateVector3Packed2(GLItemsList items, PrimitiveType pt, Vector3[] vectors, Vector3 offsets, float mult, IGLInstanceControl id = null, int ic = 1)
        {
            var vb = items.NewBuffer();
            vb.Allocate(sizeof(uint) * 2 * vectors.Length);
            vb.FillPacked2vec(vectors, offsets, mult);
            var va = items.NewArray();
            vb.Bind(0, vb.Positions[0], 8);
            va.AttributeI(0, 0, 2, VertexAttribType.UnsignedInt);

            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 0 set up floats with configurable components numbers
        public static GLRenderableItem CreateFloats(GLItemsList items, PrimitiveType pt, float[] floats, int components, IGLInstanceControl id = null, int ic = 1)
        {
            var vb = items.NewBuffer();
            vb.AllocateFill(floats);
            //float[] ouwt = vb.ReadFloats(0, floats.Length); // test read back

            var va = items.NewArray();
            vb.Bind(0, vb.Positions[0], sizeof(float)*components);     //tbd
            va.Attribute(0, 0, components, VertexAttribType.Float);

            return new GLRenderableItem(pt, floats.Length / components, va, id, ic);
        }


        public static GLRenderableItem CreateNullVertex(PrimitiveType pt, IGLInstanceControl id = null, int dc =1,  int ic = 1)
        {
            return new GLRenderableItem(pt, dc, null, id, ic);  // no vertex data.
        }

        #endregion

        #region Create element indexs for this RI

        public void CreateByteIndex(byte[] indexes, int base_vertex = 0, int base_index = 0)
        {
            ElementBuffer = new GLBuffer();
            ElementBuffer.AllocateFill(indexes);
            BaseVertex = base_vertex;
            DrawType = DrawElementsType.UnsignedByte;
            BaseIndex = base_index;
            DrawCount = indexes.Length;
        }

        public void CreateRectangleRestartIndexByte(int reccount)
        {
            ElementBuffer = new GLBuffer();
            ElementBuffer.FillRectangularIndices(reccount);
            DrawType = DrawElementsType.UnsignedByte;
            DrawCount = ElementBuffer.BufferSize - 1;

            //byte[] b = ReadBuffer(0, buf.BufferSize); // test read back
        }

        #endregion

        #region Indirect draw setup

        #endregion

    }

}

