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

namespace OpenTKUtils.GL4
{
    // Vertex's only, in vec4 form

    public class GLVertexObject : GLVertexArray
    {
        // Vertex shader must implement
        // layout(location = 0) in vec4 position;
        const int attribindex = 0;
        const int bindingindex = 0;

        GLBuffer buffer;

        public override int Count { get; set; }

        public GLVertexObject(Vector4[] vertices)
        {
            Count = vertices.Length;

            buffer = new GLBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.Id);

            int pos = buffer.Write(vertices);

            GL.VertexArrayVertexBuffer(Array, bindingindex, buffer.Id, IntPtr.Zero, 16);        // tell Array that binding index comes from this buffer.

            GL.VertexArrayAttribBinding(Array, attribindex, bindingindex);     // bind atrib index 0 to binding index 0

            GL.VertexArrayAttribFormat(
                Array,
                attribindex,            // attribute index, from the shader location = 0
                4,                      // size of attribute, vec4
                VertexAttribType.Float, // contains floats
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                pos);                     // relative offset, first item

            GL.EnableVertexArrayAttrib(Array, attribindex);         // enable attrib 0 - this is the layout number

            // link the vertex array and buffer and provide the stride as size of Vertex
            // removed GL.VertexArrayVertexBuffer(Array, bindingindex, buffer.Id, IntPtr.Zero, 16);        // link Vertextarry to buffer and set stride

            GLStatics.Check();
        }

        public override void Dispose()
        {
            base.Dispose();
            buffer.Dispose();
        }
    }
}