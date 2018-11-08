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

        public void Bind(IGLProgramShader shader)
        {
            VertexArray?.Bind();
            InstanceData?.Bind(shader);
        }

        public void Render()
        {
            GL.DrawArraysInstanced(PrimitiveType, 0, DrawCount,InstanceCount);
        }

        public static GLRenderableItem CreateVector4Color4(GLItemsList items, PrimitiveType pt, Vector4[] vectors, Color4[] colours, IGLInstanceData id = null, int ic = 1)
        {
            var vb = items.NewBuffer();
            vb.Set(vectors, colours);
            var va = items.NewArray();
            va.BindVector4x2(vb, vb.Positions[0], vb.Positions[1]);
            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        public static GLRenderableItem CreateVector4(GLItemsList items, PrimitiveType pt, Vector4[] vectors, IGLInstanceData id = null, int ic = 1)
        {
            var vb = items.NewBuffer();
            vb.Set(vectors);
            var va = items.NewArray();
            va.BindVector4(vb, vb.Positions[0]);
            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        public static GLRenderableItem CreateVector4(GLItemsList items, PrimitiveType pt, GLBuffer vb, int pos = 0, IGLInstanceData id = null, int ic = 1)
        {
            var va = items.NewArray();
            va.BindVector4(vb, pos);
            return new GLRenderableItem(pt, 0, va, id, ic);
        }

        public static GLRenderableItem CreateVector4Vector2(GLItemsList items, PrimitiveType pt, Vector4[] vectors, Vector2[] coords, IGLInstanceData id = null, int ic = 1)
        {
            var vb = items.NewBuffer();
            vb.Set(vectors, coords);
            var va = items.NewArray();
            va.BindVector4Vector2(vb, vb.Positions[0], vb.Positions[1]);
            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        public static GLRenderableItem CreateVector4Vector2(GLItemsList items, PrimitiveType pt, Tuple<Vector4[], Vector2[]> vectors, IGLInstanceData id = null, int ic = 1)
        {
            return CreateVector4Vector2(items, pt, vectors.Item1, vectors.Item2, id, ic);
        }

        public static GLRenderableItem CreateVector4Matrix4(GLItemsList items, PrimitiveType pt, Vector4[] vectors, Matrix4[] matrix, IGLInstanceData id = null, int ic = 1)
        {
            var vb = items.NewBuffer();
            vb.Set(vectors, matrix);
            var va = items.NewArray();
            va.BindVector4Matrix4(vb, vb.Positions[0], vb.Positions[1]);
            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        public static GLRenderableItem CreateVector4Vector2Matrix4(GLItemsList items, PrimitiveType pt, Vector4[] vectors, Vector2[] coords, Matrix4[] matrix, IGLInstanceData id = null, int ic = 1)
        {
            var vb = items.NewBuffer();
            vb.Set(vectors, coords, matrix);
            var va = items.NewArray();
            va.BindVector4Vector2Matrix4(vb, vb.Positions[0], vb.Positions[1], vb.Positions[2]);
            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }

        public static GLRenderableItem CreateVector3Packed(GLItemsList items, PrimitiveType pt, Vector3[] vectors, Vector3 offsets, float mult, IGLInstanceData id = null, int ic = 1)
        {
            var vb = items.NewBuffer();
            vb.Set(vectors, offsets, mult);
            var va = items.NewArray();
            va.BindUINT(vb, vb.Positions[0], 2);                // 2 uints per entry
            return new GLRenderableItem(pt, vectors.Length, va, id, ic);
        }
    }

    // List to hold named renderables against programs, and a Render function to send the lot to GL - issued in Program ID order, then Add order

}

