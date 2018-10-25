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
    // All renderables inherit from this class..
    // and override bind if required.

    public abstract class SingleBufferRenderable : IGLRenderable
    {
        public IGLObjectInstanceData InstanceData { get { return instancedata; } }

        protected int VertexArray;                  // the vertex GL Array 
        protected int VertexBuffer;                 // its buffer data
        protected int VertexCount;
        protected PrimitiveType primitivetype;        // Draw type
        protected IGLObjectInstanceData instancedata;   // any instance data

        protected SingleBufferRenderable(int vertexCount, IGLObjectInstanceData id, PrimitiveType pt)
        {
            instancedata = id;
            VertexCount = vertexCount;
            primitivetype = pt;

            VertexArray = GL.GenVertexArray();
            VertexBuffer = GL.GenBuffer();

            GL.BindVertexArray(VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
        }

        public virtual void Bind(IGLProgramShaders shader)
        {
            GL.BindVertexArray(VertexArray);        // Bind vertex
            instancedata?.Bind(shader);                   // offer any instance data bind opportunity
        }

        public virtual void Render()
        {
            //System.Diagnostics.Debug.WriteLine("Draw " + primitivetype + " using " + primitivetype);
            GL.DrawArrays(primitivetype, 0, VertexCount);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GL.DeleteVertexArray(VertexArray);
                GL.DeleteBuffer(VertexBuffer);
            }
        }
    }
}
