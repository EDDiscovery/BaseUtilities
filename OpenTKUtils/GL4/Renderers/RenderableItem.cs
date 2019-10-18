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

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;

namespace OpenTKUtils.GL4
{
    // Standard renderable item supporting Instancing and draw count, vertex arrays, instance data.
    // A Renderable Item must implement Bind, Render and have a Primitive Type
    // An item has a Draw count and Instance count
    // It has a primitive type
    // it is associated with an optional VertexArray which is bound using Bind()
    // it is associated with an optional InstanceData which is instanced using Bind()
    // It is rendered by render, giving the instance count

    public class GLRenderableItem : IGLRenderableItem
    { 
        public int DrawCount { get; set; }                                  // Draw count
        public int InstanceCount { get; set; }                              // Instances
        public PrimitiveType PrimitiveType { get; set; }                    // Draw type
        public IGLVertexArray VertexArray { get; set; }                     // may be null - if so no vertex data. Does not own
        public IGLInstanceData InstanceData { get; set; }                   // may be null - no instance data. Does not own.

        public GLRenderableItem(PrimitiveType pt, int drawcount, IGLVertexArray va, IGLInstanceData id = null, int ic = 1)
        {
            PrimitiveType = pt;
            DrawCount = drawcount;
            VertexArray = va;
            InstanceData = id;
            InstanceCount = ic;
        }

        public void Bind(IGLProgramShader shader, Common.MatrixCalc c)      // called by Render() to bind data to GL, vertex and InstanceData
        {
            VertexArray?.Bind();
            InstanceData?.Bind(shader,c);
        }

        public void Render()                                                // called by Render() to draw the item.  Note DrawArrayInstanced
        {
            GL.DrawArraysInstanced(PrimitiveType, 0, DrawCount,InstanceCount);
        }

        #region These create new vertext arrays into buffers of vertexs and colours

        // Vector4, Color4, optional instance data and count

        public static GLRenderableItem CreateVector4Color4(GLItemsList items, PrimitiveType pt, Vector4[] vectors, Color4[] colours, IGLInstanceData id = null, int ic = 1)
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
        public static GLRenderableItem CreateVector4(GLItemsList items, PrimitiveType pt, Vector4[] vectors, IGLInstanceData id = null, int ic = 1)
        {
            var vb = items.NewBuffer();
            vb.Allocate(GLBuffer.Vec4size * vectors.Length);
            vb.Fill(vectors);

            var va = items.NewArray();
            vb.Bind(0, vb.Positions[0], 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);
            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 0 set up
        public static GLRenderableItem CreateVector4(GLItemsList items, PrimitiveType pt, GLBuffer vb, int pos = 0, IGLInstanceData id = null, int ic = 1)
        {
            var va = items.NewArray();
            vb.Bind(0, pos, 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);
            return new GLRenderableItem(pt, 0, va, id, ic);
        }

        // in 0,1 set up
        public static GLRenderableItem CreateVector4Vector2(GLItemsList items, PrimitiveType pt, Vector4[] vectors, Vector2[] coords, IGLInstanceData id = null, int ic = 1)
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

        public static GLRenderableItem CreateVector4Vector2(GLItemsList items, PrimitiveType pt, Tuple<Vector4[], Vector2[]> vectors, IGLInstanceData id = null, int ic = 1)
        {
            return CreateVector4Vector2(items, pt, vectors.Item1, vectors.Item2, id, ic);
        }

        public static GLRenderableItem CreateVector4Vector2Vector4(GLItemsList items, PrimitiveType pt,
                                                                   Tuple<Vector4[],Vector2[]> pos, 
                                                                    Vector4[] instanceposition,
                                                                   IGLInstanceData id = null, int ic = 1,
                                                                   bool separbuf = false, int divisorinstance = 1)
        {
            return CreateVector4Vector2Vector4(items, pt, pos.Item1, pos.Item2, instanceposition, id, ic, separbuf, divisorinstance);
        }

        // in 0,1,4 set up.  if separbuffer = true and instanceposition is null, it makes a buffer for you to fill up externally.
        public static GLRenderableItem CreateVector4Vector2Vector4(GLItemsList items, PrimitiveType pt,
                                                                   Vector4[] vectors, Vector2[] coords, Vector4[] instanceposition,
                                                                   IGLInstanceData id = null, int ic = 1,
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
                                                                    IGLInstanceData id = null, int ic = 1, 
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

            va.MatrixAttribute(2, 4);                                    // bp 2 at 4-7
            vbuf2.Bind(2, instancematrix != null ? vbuf2.Positions[posi]: 0, 64, matrixdivisor);     // use a binding 

            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 0,4-7 set up
        public static GLRenderableItem CreateVector4Matrix4(GLItemsList items, PrimitiveType pt, Vector4[] vectors, Matrix4[] matrix, IGLInstanceData id = null, int ic = 1, int matrixdivisor=1)
        {
            var vb = items.NewBuffer();

            vb.Allocate(GLBuffer.Vec4size * vectors.Length + GLBuffer.Mat4size * matrix.Length);    
            vb.Fill(vectors);
            vb.Fill(matrix);

            var va = items.NewArray();

            vb.Bind(0, vb.Positions[0], 16);
            va.Attribute(0, 0, 4, VertexAttribType.Float);      // bp 0 at 0/1 

            vb.Bind(2, vb.Positions[1], 64, matrixdivisor);     // use a binding 
            va.MatrixAttribute(2, 4);                    // bp 2 at 4-7

            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        // in 0 set up
        public static GLRenderableItem CreateVector3Packed2(GLItemsList items, PrimitiveType pt, Vector3[] vectors, Vector3 offsets, float mult, IGLInstanceData id = null, int ic = 1)
        {
            var vb = items.NewBuffer();
            vb.Allocate(sizeof(uint) * 2 * vectors.Length);
            vb.FillPacked2vec(vectors, offsets, mult);
            var va = items.NewArray();
            vb.Bind(0, vb.Positions[0], 8);
            va.AttributeI(0, 0, 2, VertexAttribType.UnsignedInt);

            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        #endregion
    }

}

