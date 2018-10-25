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
    abstract public class GLVertexTexturedObject : SingleBufferRenderable
    {
        // Vertex shader must implement
        // layout(location = 0) in vec4 position;
        // layout(location = 1) in vec2 textureCoordinate;

        const int attriblayoutindexposition = 0;
        const int attriblayouttexcoord = 1;

        private IGLTexture texture;

        public GLVertexTexturedObject(Vector4[] vertices, Vector2[] texcoords, IGLObjectInstanceData data, IGLTexture tx, PrimitiveType pt) :
                    base(vertices.Length,data,pt)
        {
            System.Diagnostics.Debug.Assert(vertices.Length== texcoords.Length);
            int offset = 0;
            GL.BufferData(BufferTarget.ArrayBuffer, 16 * vertices.Length + 8 * texcoords.Length, (IntPtr)offset, BufferUsageHint.StaticDraw);

            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)offset, 16 * vertices.Length, vertices);
            offset += 16 * vertices.Length;
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)offset, 8 * texcoords.Length, texcoords);

            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 16, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8, offset);

            GL.EnableVertexArrayAttrib(VertexArray, attriblayoutindexposition);
            GL.EnableVertexArrayAttrib(VertexArray, attriblayouttexcoord);

            texture = tx;
        }

        public override void Bind(IGLProgramShaders shader)
        {
            base.Bind(shader);
            texture.Bind();
        }
    }

    public class GLTexturedTriangles : GLVertexTexturedObject
    {
        public GLTexturedTriangles(Vector4[] vertices, Vector2[] tex, IGLObjectInstanceData data, IGLTexture tx) : base(vertices, tex, data, tx, PrimitiveType.Triangles)
        {
        }

        public GLTexturedTriangles(Tuple<Vector4[], Vector2[]> verticestex, IGLObjectInstanceData data, IGLTexture tx) :
                    base(verticestex.Item1, verticestex.Item2, data, tx, PrimitiveType.Triangles)
        {
        }
    }

    public class GLTexturedQuads : GLVertexTexturedObject
    {
        public GLTexturedQuads(Vector4[] vertices, Vector2[] tex, IGLObjectInstanceData data, IGLTexture tx) : base(vertices, tex, data, tx, PrimitiveType.Quads)
        {
        }
    }
}