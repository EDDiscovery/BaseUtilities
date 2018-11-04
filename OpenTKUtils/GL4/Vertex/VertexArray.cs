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

    public abstract class GLVertexArray : IGLVertexArray
    {
        public abstract int Count { get; set; }

        protected int Array;                            // the vertex GL Array 
        
        protected GLVertexArray()
        {
            Array = GL.GenVertexArray();        // get the handle
            GL.BindVertexArray(Array);          // creates the array
        }

        public virtual void Bind(IGLProgramShader shader)
        {
            GL.BindVertexArray(Array);                  // Bind vertex
        }

        public virtual void Dispose()
        {
            GL.DeleteVertexArray(Array);
        }

    }
}
