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

    public abstract class GLVertexObject : GLVertexArrayBuffer
    {
        // Vertex shader must implement
        // layout(location = 0) in vec4 position;
        const int attribindex = 0;

        public GLVertexObject(Vector4[] vertices, IGLObjectInstanceData data, PrimitiveType pt) : base(vertices.Length, data, pt)
        {
            // create first buffer: vertex
            GL.NamedBufferStorage(
                buffer,
                16 * vertices.Length,    // the size needed by this buffer
                vertices,                           // data to initialize with
                BufferStorageFlags.MapWriteBit);    // at this point we will only write to the buffer

            const int bindingindex = 0;

            GL.VertexArrayAttribBinding(array, attribindex, bindingindex);     // bind atrib index 0 to binding index 0
            GL.EnableVertexArrayAttrib(array, attribindex);         // enable attrib 0 - this is the layout number
            GL.VertexArrayAttribFormat(
                array,
                attribindex,            // attribute index, from the shader location = 0
                4,                      // size of attribute, vec4
                VertexAttribType.Float, // contains floats
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                0);                     // relative offset, first item

            // link the vertex array and buffer and provide the stride as size of Vertex
            GL.VertexArrayVertexBuffer(array, bindingindex, buffer, IntPtr.Zero, 16);        // link Vertextarry to buffer and set stride

            GL4Statics.Check();
        }
    }

    // single points with a single defined size
    public class GLVertexPoints : GLVertexObject
    {
        public GLVertexPoints(Vector4[] vertices, IGLObjectInstanceData data) : base(vertices, data, PrimitiveType.Points)
        {
        }
    }

    public class GLVertexQuad : GLVertexObject
    {
        public GLVertexQuad(Vector4[] vertices, IGLObjectInstanceData data): base(vertices, data, PrimitiveType.Quads)
        {
        }
    }

    public class GLVertexPatches : GLVertexObject
    {
        public GLVertexPatches(Vector4[] vertices, IGLObjectInstanceData data) : base(vertices, data, PrimitiveType.Patches)
        {
        }
    }

}