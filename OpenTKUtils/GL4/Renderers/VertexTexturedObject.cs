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
    public struct GLVertexTextured
    {
        // Vertex shader must implement
        // layout(location = 0) in vec4 position;
        // layout(location = 1) in vec2 textureCoordinate;

        public const int Size = (4 + 2) * 4; // size of struct in bytes

        public Vector4 Vertex;               // GL position on model of this vertex
        public Vector2 TextureCoordinate;      // and its correponding point on the texture

        public GLVertexTextured(Vector4 v, Vector2 textureCoordinate)
        {
            Vertex = v;
            TextureCoordinate = textureCoordinate;
        }
        public GLVertexTextured(Vector4 v)
        {
            Vertex = v;
            TextureCoordinate = Vector2.Zero;
        }
    }

    abstract public class GLVertexTexturedObject : GLRenderable
    {
        private IGLTexture texture;

        public GLVertexTexturedObject(GLVertexTextured[] vertices, IGLProgramShaders program, IGLObjectInstanceData data , IGLTexture tx, PrimitiveType pt) : base(vertices.Length, program, data, pt)
        {
            // create first buffer: vertex
            GL.NamedBufferStorage(
                VertexBuffer,
                GLVertexTextured.Size * vertices.Length,        // the size needed by this buffer
                vertices,                           // data to initialize with
                BufferStorageFlags.MapWriteBit);    // at this point we will only write to the buffer

            const int bindingindex = 0;

            // first position vectors, to attrib 0
            const int attriblayoutindexposition = 0;
            GL.VertexArrayAttribBinding(VertexArray, attriblayoutindexposition, bindingindex);
            GL.EnableVertexArrayAttrib(VertexArray, attriblayoutindexposition);
            GL.VertexArrayAttribFormat(
                VertexArray,
                attriblayoutindexposition, // attribute index, from the shader location = 0
                4,                      // size of attribute, vec4
                VertexAttribType.Float, // contains floats
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                0);                     // relative offset, first item

            // second texcoord, to attrib 1

            const int attriblayouttexcoord = 1;
            GL.VertexArrayAttribBinding(VertexArray, attriblayouttexcoord, bindingindex);
            GL.EnableVertexArrayAttrib(VertexArray, attriblayouttexcoord);
            GL.VertexArrayAttribFormat(
                VertexArray,
                attriblayouttexcoord,   // attribute index, from the shader location = 1
                2,                      // size of attribute, vec2
                VertexAttribType.Float, // contains floats
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                16);                    // relative offset after a vec4

            // link the vertex array and buffer and provide the stride as size of VertexTextured
            GL.VertexArrayVertexBuffer(VertexArray, 0, VertexBuffer, IntPtr.Zero, GLVertexTextured.Size);

            texture = tx;
        }

        public override void Bind()
        {
            base.Bind();
            GL.BindVertexArray(VertexArray);
            texture.Bind();
        }
    }

    public class GLTexturedTriangles : GLVertexTexturedObject
    {
        public GLTexturedTriangles(GLVertexTextured[] vertices, IGLProgramShaders program, IGLObjectInstanceData data, IGLTexture tx) : base(vertices, program, data, tx, PrimitiveType.Triangles)
        {
        }
    }

    public class GLTexturedQuads : GLVertexTexturedObject
    {
        public GLTexturedQuads(GLVertexTextured[] vertices, IGLProgramShaders program, IGLObjectInstanceData data, IGLTexture tx) : base(vertices, program, data, tx, PrimitiveType.Quads)
        {
        }
    }
}