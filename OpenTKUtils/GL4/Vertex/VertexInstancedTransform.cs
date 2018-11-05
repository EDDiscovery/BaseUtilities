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
    // Vertex and transforms

    public class GLVertexInstancedTransformObject : GLVertexArray
    {
        // Vertex shader must implement
        // layout(location = 0) in vec4 position;
        // layout(location = 4) in mat4 transforms; (4-7 are used)

        const int attriblayoutindexposition = 0;
        const int attriblayouttransforms = 4;
        const int bindingindex = 0;

        GLBuffer buffer;

        public override int Count { get; set; }

        public GLVertexInstancedTransformObject(Vector4[] vertices, Matrix4[] transform)
        {
            Count = vertices.Length;

            buffer = new GLBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.Id);

            var pos = buffer.Write(vertices, transform);

            GL.VertexArrayVertexBuffer(Array, bindingindex, buffer.Id, IntPtr.Zero, 0);        // tell Array that binding index comes from this buffer.

            GL.VertexArrayAttribBinding(Array, attriblayoutindexposition, bindingindex);     // bind atrib index to binding index
            GL.VertexAttribPointer(attriblayoutindexposition, 4, VertexAttribPointerType.Float, false, 16, pos.Item1);  // attrib 0, vertices, 4 entries, float, 16 long, at 0 in buffer

            GL.VertexArrayAttribBinding(Array, attriblayouttransforms, bindingindex);     // bind atrib index to binding index, for all four columns.
            GL.VertexArrayAttribBinding(Array, attriblayouttransforms + 1, bindingindex);
            GL.VertexArrayAttribBinding(Array, attriblayouttransforms + 2, bindingindex);
            GL.VertexArrayAttribBinding(Array, attriblayouttransforms + 3, bindingindex);

            GL.VertexAttribPointer(attriblayouttransforms, 4, VertexAttribPointerType.Float, false, 64, pos.Item2 + 0); // attrib t, 4entries, float, at offset in buffer, stride is 16x4
            GL.VertexAttribPointer(attriblayouttransforms + 1, 4, VertexAttribPointerType.Float, false, 64, pos.Item2 + 4 * 4); // attrib t+1
            GL.VertexAttribPointer(attriblayouttransforms + 2, 4, VertexAttribPointerType.Float, false, 64, pos.Item2 + 8 * 4); // attrib t+2
            GL.VertexAttribPointer(attriblayouttransforms + 3, 4, VertexAttribPointerType.Float, false, 64, pos.Item2 + 12 * 4); // attrib t+3

            GL.VertexAttribDivisor(attriblayouttransforms, 1);      // 1 transform per instance..
            GL.VertexAttribDivisor(attriblayouttransforms + 1, 1);
            GL.VertexAttribDivisor(attriblayouttransforms + 2, 1);
            GL.VertexAttribDivisor(attriblayouttransforms + 3, 1);

            GL.EnableVertexArrayAttrib(Array, attriblayoutindexposition);       // go for attrib launch!
            GL.EnableVertexArrayAttrib(Array, attriblayouttransforms);
            GL.EnableVertexArrayAttrib(Array, attriblayouttransforms + 1);
            GL.EnableVertexArrayAttrib(Array, attriblayouttransforms + 2);
            GL.EnableVertexArrayAttrib(Array, attriblayouttransforms + 3);

            GLStatics.Check();
        }

        public override void Dispose()
        {
            base.Dispose();
            buffer.Dispose();
        }
    }

    public class GLVertexInstancedTexCoordsTransformObject : GLVertexArray
    {
        // Vertex shader must implement
        // layout(location = 0) in vec4 position;
        // layout(location = 4) in mat4 transforms; (4-7 are used)

        const int attriblayoutindexposition = 0;
        const int attriblayouttexcoord = 1;
        const int attriblayouttransforms = 4;
        const int bindingindex = 0;

        GLBuffer buffer;

        public override int Count { get; set; }

        public GLVertexInstancedTexCoordsTransformObject(Vector4[] vertices, Vector2[] texcoords, Matrix4[] transform)
        {
            Count = vertices.Length;

            buffer = new GLBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.Id);

            var pos = buffer.Write(vertices, texcoords, transform);

            GL.VertexArrayVertexBuffer(Array, bindingindex, buffer.Id, IntPtr.Zero, 0);        // tell Array that binding index comes from this buffer.

            GL.VertexArrayAttribBinding(Array, attriblayoutindexposition, bindingindex);     // bind atrib index to binding index
            GL.VertexAttribPointer(attriblayoutindexposition, 4, VertexAttribPointerType.Float, false, 16, pos.Item1);  // attrib 0, vertices, 4 entries, float, 16 long, at 0 in buffer

            GL.VertexArrayAttribBinding(Array, attriblayouttexcoord, bindingindex);     // bind atrib index to binding index
            GL.VertexAttribPointer(attriblayouttexcoord, 2, VertexAttribPointerType.Float, false, 8, pos.Item2);  

            GL.VertexArrayAttribBinding(Array, attriblayouttransforms, bindingindex);     // bind atrib index to binding index, for all four columns.
            GL.VertexArrayAttribBinding(Array, attriblayouttransforms + 1, bindingindex);
            GL.VertexArrayAttribBinding(Array, attriblayouttransforms + 2, bindingindex);
            GL.VertexArrayAttribBinding(Array, attriblayouttransforms + 3, bindingindex);

            GL.VertexAttribPointer(attriblayouttransforms, 4, VertexAttribPointerType.Float, false, 64, pos.Item3 + 0); // attrib t, 4entries, float, at offset in buffer, stride is 16x4
            GL.VertexAttribPointer(attriblayouttransforms + 1, 4, VertexAttribPointerType.Float, false, 64, pos.Item3 + 4 * 4); // attrib t+1
            GL.VertexAttribPointer(attriblayouttransforms + 2, 4, VertexAttribPointerType.Float, false, 64, pos.Item3 + 8 * 4); // attrib t+2
            GL.VertexAttribPointer(attriblayouttransforms + 3, 4, VertexAttribPointerType.Float, false, 64, pos.Item3 + 12 * 4); // attrib t+3

            GL.VertexAttribDivisor(attriblayouttransforms, 1);      // 1 transform per instance..
            GL.VertexAttribDivisor(attriblayouttransforms + 1, 1);
            GL.VertexAttribDivisor(attriblayouttransforms + 2, 1);
            GL.VertexAttribDivisor(attriblayouttransforms + 3, 1);

            GL.EnableVertexArrayAttrib(Array, attriblayoutindexposition);       // go for attrib launch!
            GL.EnableVertexArrayAttrib(Array, attriblayouttexcoord);  
            GL.EnableVertexArrayAttrib(Array, attriblayouttransforms);
            GL.EnableVertexArrayAttrib(Array, attriblayouttransforms + 1);
            GL.EnableVertexArrayAttrib(Array, attriblayouttransforms + 2);
            GL.EnableVertexArrayAttrib(Array, attriblayouttransforms + 3);

            GLStatics.Check();
        }

        public override void Dispose()
        {
            base.Dispose();
            buffer.Dispose();
        }
    }

}