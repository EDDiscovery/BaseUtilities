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
    // Vertex and texture co-ords

    abstract public class GLVertexTexturedObject : GLVertexArrayBuffer
    {
        // Vertex shader must implement
        // layout(location = 0) in vec4 position;
        // layout(location = 1) in vec2 textureCoordinate;

        const int attriblayoutindexposition = 0;
        const int attriblayouttexcoord = 1;

        int texturebindingpoint;

        private IGLTexture texture;

        public GLVertexTexturedObject(Vector4[] vertices, Vector2[] texcoords, IGLObjectInstanceData data, IGLTexture tx, int texbindingpoint, PrimitiveType pt) :
                    base(vertices.Length,data,pt)
        {
            System.Diagnostics.Debug.Assert(vertices.Length== texcoords.Length);

            int offset = 0;                                                     // buffer size is enough to hold vertices and tex coords
            GL.BufferData(BufferTarget.ArrayBuffer, 16 * vertices.Length + 8 * texcoords.Length, (IntPtr)offset, BufferUsageHint.StaticDraw);

            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)offset, 16 * vertices.Length, vertices);     // first plug in the vertices
            offset += 16 * vertices.Length;                 
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)offset, 8 * texcoords.Length, texcoords);    // then copy in the colours

            GL.VertexAttribPointer(attriblayoutindexposition, 4, VertexAttribPointerType.Float, false, 16, 0);  // attrib 0, vertices, 4 entries, float, 16 long, at 0 in buffer
            GL.VertexAttribPointer(attriblayouttexcoord, 2, VertexAttribPointerType.Float, false, 8, offset); // attrib 1, 2 entries, float, 8 long, at offset in buffer

            GL.EnableVertexArrayAttrib(array, attriblayoutindexposition);       // go for attrib launch!
            GL.EnableVertexArrayAttrib(array, attriblayouttexcoord);

            texture = tx;
            texturebindingpoint = texbindingpoint;
        }

        public override void Bind(IGLProgramShader shader)
        {
            base.Bind(shader);
            texture.Bind(texturebindingpoint);
        }
    }

    public class GLTexturedTriangles : GLVertexTexturedObject
    {
        public GLTexturedTriangles(Vector4[] vertices, Vector2[] tex, IGLObjectInstanceData data, IGLTexture tx, int texbindingpoint) : base(vertices, tex, data, tx, texbindingpoint, PrimitiveType.Triangles)
        {
        }

        public GLTexturedTriangles(Tuple<Vector4[], Vector2[]> verticestex, IGLObjectInstanceData data, IGLTexture tx, int texbindingpoint) :
                    base(verticestex.Item1, verticestex.Item2, data, tx, texbindingpoint, PrimitiveType.Triangles)
        {
        }
    }

    public class GLTexturedQuads : GLVertexTexturedObject
    {
        public GLTexturedQuads(Vector4[] vertices, Vector2[] tex, IGLObjectInstanceData data, IGLTexture tx, int texbindingpoint ) : base(vertices, tex, data, tx, texbindingpoint, PrimitiveType.Quads)
        {
        }
    }
}