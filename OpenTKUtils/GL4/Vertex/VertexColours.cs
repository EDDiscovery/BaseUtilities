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
    // Vertex and colour data

    public class GLVertexColourObject : GLVertexArray
    {
        // Vertex shader must implement
        // layout(location = 0) in vec4 position;
        // layout(location = 1) in vec4 color;
        const int attriblayoutindexposition = 0;
        const int attriblayoutcolour = 1;
        const int bindingindex = 0;

        public override int Count { get; set; }

        GLBuffer buffer;

        // colour data can be shorted than vertices, and will be repeated.
        public GLVertexColourObject(Vector4[] vertices, Color4[] colours) 
        {
            Count = vertices.Length;

            buffer = new GLBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.Id);

            var pos = buffer.Write(vertices, colours);

            GL.VertexArrayVertexBuffer(Array, bindingindex, buffer.Id, IntPtr.Zero, 0);        // tell Array that binding index comes from this buffer.

            GL.VertexArrayAttribBinding(Array, attriblayoutindexposition, bindingindex);     // bind atrib index to binding index
            GL.VertexArrayAttribBinding(Array, attriblayoutcolour, bindingindex);     // bind atrib index to binding index

            GL.VertexAttribPointer(attriblayoutindexposition, 4, VertexAttribPointerType.Float, false, 16, pos.Item1);  // set where in the buffer the attrib data comes from
            GL.VertexAttribPointer(attriblayoutcolour, 4, VertexAttribPointerType.Float, false, 16, pos.Item2);

            GL.EnableVertexArrayAttrib(Array, attriblayoutindexposition);
            GL.EnableVertexArrayAttrib(Array, attriblayoutcolour);

            GLStatics.Check();
        }

        public override void Dispose()
        {
            base.Dispose();
            buffer.Dispose();
        }
    }
}