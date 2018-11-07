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
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace OpenTKUtils.GL4
{
    // Standard renderable item supporting Instancing and draw count, vertex arrays, instance data.

    public class GLRenderableItem : IGLRenderableItem
    { 
        public int DrawCount { get; set; }                                  // Draw count
        public int InstanceCount { get; set; }                              // Instances
        public PrimitiveType PrimitiveType { get; set; }                    // Draw type
        public IGLVertexArray VertexArray { get; set; }                     // may be null - if so no vertex data
        public IGLInstanceData InstanceData { get; set; }                   // may be null - no instance data

        public GLRenderableItem(PrimitiveType pt, int drawcount, IGLVertexArray va = null, IGLInstanceData id = null, int ic = 1)
        {
            PrimitiveType = pt;
            DrawCount = drawcount;
            VertexArray = va;
            InstanceData = id;
            InstanceCount = ic;
        }

        public GLRenderableItem(PrimitiveType pt, IGLVertexArray va, IGLInstanceData id = null, int ic = 1)
        {
            PrimitiveType = pt;
            DrawCount = va.Count;
            VertexArray = va;
            InstanceData = id;
            InstanceCount = ic;
        }

        public void Bind(IGLProgramShader shader)
        {
            VertexArray?.Bind(shader);
            InstanceData?.Bind(shader);
        }

        public void Render()
        {
            GL.DrawArraysInstanced(PrimitiveType, 0, DrawCount,InstanceCount);
        }

        public void Dispose()
        {
            VertexArray?.Dispose();
            InstanceData?.Dispose();
        }
    }

    // List to hold named renderables against programs, and a Render function to send the lot to GL - issued in Program ID order, then Add order

}

