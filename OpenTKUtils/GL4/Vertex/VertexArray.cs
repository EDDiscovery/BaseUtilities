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
    // Base Class for vertex data to vertex shader..

    public abstract class GLVertexArray : IGLRenderable
    {
        public IGLObjectInstanceData InstanceData { get { return instancedata; } }

        protected int Array;                            // the vertex GL Array 
        protected int Count;                            // num of vertexes
        protected PrimitiveType primitivetype;          // Draw type
        protected IGLObjectInstanceData instancedata;   // any instance data

        // tbd instance count

        protected GLVertexArray(int vertexCount, IGLObjectInstanceData id, PrimitiveType pt)
        {
            instancedata = id;
            Count = vertexCount;
            primitivetype = pt;

            Array = GL.GenVertexArray();        // get the handle
            GL.BindVertexArray(Array);          // creates the array
        }

        public virtual void Bind(IGLProgramShader shader)
        {
            GL.BindVertexArray(Array);                  // Bind vertex
            instancedata?.Bind(shader);                 // offer any instance data bind opportunity
        }

        public virtual void Render()
        {
            //System.Diagnostics.Debug.WriteLine("Draw " + primitivetype + " using " + primitivetype);
            GL.DrawArrays(primitivetype, 0, Count);
        }

        public virtual void Dispose()
        {
            GL.DeleteVertexArray(Array);
        }
    }
}
