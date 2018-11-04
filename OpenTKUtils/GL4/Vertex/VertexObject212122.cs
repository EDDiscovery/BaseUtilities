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
    // Vertex only, in Packed form
    // packs three values to 8 bytes

    public class GLVertexPackedObject212122 : GLVertexArray    
    {
        // Vertex shader must implement
        // layout(location = 0) in uvec2 position;
        const int attribindex = 0;
        const int bindingindex = 0;

        GLBuffer buffer;

        public override int Count { get; set; }

        public GLVertexPackedObject212122(Vector3[] vertices, Vector3 offsets, float mult) 
        {
            Count = vertices.Length;

            int p = 0;
            uint[] packeddata = new uint[vertices.Length * 2];
            for (int i = 0; i < vertices.Length; i++)
            {
                uint z = (uint)((vertices[i].Z + offsets.Z) * mult);
                packeddata[p++] = (uint)((vertices[i].X + offsets.X) * mult) | ((z & 0x7ff) << 21);
                packeddata[p++] = (uint)((vertices[i].Y + offsets.Y) * mult) | (((z >> 11) & 0x7ff) << 21);
            }

            buffer = new GLBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.Id);

            int pos = buffer.Write(packeddata);

            GL.VertexArrayVertexBuffer(Array, bindingindex, buffer.Id, IntPtr.Zero, 8);        // link Vertextarry to buffer and set stride

            GL.VertexArrayAttribBinding(Array, attribindex, bindingindex);     // bind atrib index 0 to binding index 0
            GL.EnableVertexArrayAttrib(Array, attribindex);         // enable attrib 0 - this is the layout number

            GL.VertexArrayAttribIFormat(Array,                // IFormat!  Needed to prevent auto conversion to float
                attribindex,            // attribute index, from the shader location = 0
                2,                      // 2 entries per vertex
                VertexAttribType.UnsignedInt,  // contains unsigned ints
                pos);                     // 0 offset into item data

            // link the vertex array and buffer and provide the stride as size of Vertex
        }

        public override void Dispose()
        {
            base.Dispose();
            buffer.Dispose();
        }
    }
}