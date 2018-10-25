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
    public abstract class GLVertexPackedObject212122 : SingleBufferRenderable    // packs three values to 8 bytes
    {
        // Vertex shader must implement
        // layout(location = 0) in uvec2 position;
        const int attribindex = 0;

        public GLVertexPackedObject212122(Vector4[] vertices, IGLObjectInstanceData data, PrimitiveType pt, float off, float mult) : base(vertices.Length, data, pt)
        {
            int p = 0;
            uint[] packeddata = new uint[vertices.Length * 2];
            for (int i = 0; i < vertices.Length; i++)
            {
                uint z = (uint)((vertices[i].Z + off) * mult);
                packeddata[p++] = (uint)((vertices[i].X + off) * mult) | ((z & 0x7ff) << 21);
                packeddata[p++] = (uint)((vertices[i].Y + off) * mult) | (((z >> 11) & 0x7ff) << 21);
            }

            // create first buffer: vertex
            GL.NamedBufferStorage(
                VertexBuffer,
                8 * vertices.Length,                // the size needed by this buffer in bytes
                packeddata,                           // data to initialize with
                BufferStorageFlags.MapWriteBit);    // at this point we will only write to the buffer

            const int bindingindex = 0;
           
            GL.VertexArrayAttribBinding(VertexArray, attribindex, bindingindex);     // bind atrib index 0 to binding index 0
            GL.EnableVertexArrayAttrib(VertexArray, attribindex);         // enable attrib 0 - this is the layout number

            GL.VertexArrayAttribIFormat(VertexArray,                // IFormat!  Needed to prevent auto conversion to float
                attribindex,            // attribute index, from the shader location = 0
                2,
                VertexAttribType.UnsignedInt,  // contains unsigned ints
                0);

            // link the vertex array and buffer and provide the stride as size of Vertex
            GL.VertexArrayVertexBuffer(VertexArray, bindingindex, VertexBuffer, IntPtr.Zero, 8);        // link Vertextarry to buffer and set stride
        }
    }

    // single points with a single defined size
    public class GLVertexPackedPoints212122 : GLVertexPackedObject212122
    {
        public GLVertexPackedPoints212122(Vector4[] vertices, IGLObjectInstanceData data, float off, float mult) : base(vertices, data, PrimitiveType.Points, off, mult )
        {
        }
    }

}