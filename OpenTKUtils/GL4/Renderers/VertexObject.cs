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
    public abstract class GLVertexObject : GLRenderable
    {
        public GLVertexObject(Vector4[] vertices, IGLProgramShaders program, IGLObjectInstanceData data, PrimitiveType pt) : base(vertices.Length, program, data, pt)
        {
            // create first buffer: vertex
            GL.NamedBufferStorage(
                VertexBuffer,
                16 * vertices.Length,    // the size needed by this buffer
                vertices,                           // data to initialize with
                BufferStorageFlags.MapWriteBit);    // at this point we will only write to the buffer

            const int bindingindex = 0;
            const int attribindex = 0;

            GL.VertexArrayAttribBinding(VertexArray, attribindex, bindingindex);     // bind atrib index 0 to binding index 0
            GL.EnableVertexArrayAttrib(VertexArray, attribindex);         // enable attrib 0 - this is the layout number
            GL.VertexArrayAttribFormat(
                VertexArray,
                attribindex,            // attribute index, from the shader location = 0
                4,                      // size of attribute, vec4
                VertexAttribType.Float, // contains floats
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                0);                     // relative offset, first item

            // link the vertex array and buffer and provide the stride as size of Vertex
            GL.VertexArrayVertexBuffer(VertexArray, bindingindex, VertexBuffer, IntPtr.Zero, 16);        // link Vertextarry to buffer and set stride
        }
    }

    // single points with a single defined size
    public class GLVertexPoints : GLVertexObject
    {
        private float pointsize;

        public GLVertexPoints(Vector4[] vertices, IGLProgramShaders program, IGLObjectInstanceData data, float ps) : base(vertices, program, data, PrimitiveType.Points)
        {
            pointsize = ps;
        }

        public override void Bind()
        {
            GL.PointSize(pointsize);
            base.Bind();
        }
    }

}