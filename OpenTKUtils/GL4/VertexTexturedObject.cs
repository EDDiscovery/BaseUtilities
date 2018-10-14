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

        private Vector4 Vertex;               // GL position on model of this vertex
        private Vector2 TextureCoordinate;      // and its correponding point on the texture

        public GLVertexTextured(Vector4 v, Vector2 textureCoordinate)
        {
            Vertex = v;
            TextureCoordinate = textureCoordinate;
        }

        static public void Translate(GLVertexTextured[] vertices, Vector3 pos)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Vertex.Translate(pos);
        }
        static public void Transform(GLVertexTextured[] vertices, Matrix4 trans)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Vertex = Vector4.Transform(vertices[i].Vertex, trans);
            }
        }
    }

    abstract public class GLVertexTexturedObject : GLRenderable
    {
        private GLTexture texture;

        public GLVertexTexturedObject(GLVertexTextured[] vertices, IGLProgramShaders program, IGLObjectInstanceData data , GLTexture tx, PrimitiveType pt) : base(vertices.Length, program, data, pt)
        {
            // create first buffer: vertex
            GL.NamedBufferStorage(
                VertexBuffer,
                GLVertexTextured.Size * vertices.Length,        // the size needed by this buffer
                vertices,                           // data to initialize with
                BufferStorageFlags.MapWriteBit);    // at this point we will only write to the buffer

            GL.VertexArrayAttribBinding(VertexArray, 0, 0);
            GL.EnableVertexArrayAttrib(VertexArray, 0);
            GL.VertexArrayAttribFormat(
                VertexArray,
                0,                      // attribute index, from the shader location = 0
                4,                      // size of attribute, vec4
                VertexAttribType.Float, // contains floats
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                0);                     // relative offset, first item

            GL.VertexArrayAttribBinding(VertexArray, 1, 0);
            GL.EnableVertexArrayAttrib(VertexArray, 1);
            GL.VertexArrayAttribFormat(
                VertexArray,
                1,                      // attribute index, from the shader location = 1
                2,                      // size of attribute, vec2
                VertexAttribType.Float, // contains floats
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                16);                     // relative offset after a vec4

            // link the vertex array and buffer and provide the stride as size of Vertex
            GL.VertexArrayVertexBuffer(VertexArray, 0, VertexBuffer, IntPtr.Zero, GLVertexTextured.Size);

            texture = tx;
        }

        public override void Bind()
        {
            System.Diagnostics.Debug.WriteLine("Binding texture ");
            base.Bind();
            GL.BindVertexArray(VertexArray);
            texture.Bind();
        }
    }

    public class GLTexturedTriangles : GLVertexTexturedObject
    {
        public GLTexturedTriangles(GLVertexTextured[] vertices, IGLProgramShaders program, IGLObjectInstanceData data, GLTexture tx) : base(vertices, program, data, tx, PrimitiveType.Triangles)
        {
        }
    }

    public class GLTexturedQuads : GLVertexTexturedObject
    {
        public GLTexturedQuads(GLVertexTextured[] vertices, IGLProgramShaders program, IGLObjectInstanceData data, GLTexture tx) : base(vertices, program, data, tx, PrimitiveType.Quads)
        {
        }
    }
}