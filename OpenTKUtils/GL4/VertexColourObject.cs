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
    public struct GLVertexColour
    {
        // Vertex shader must implement
        // layout(location = 0) in vec4 position;
        // layout(location = 1) in vec4 color;

        public const int Size = (4 + 4) * 4; // size of struct in bytes

        private Vector4 Vertex;
        private Color4 Color;

        public GLVertexColour(Vector4 position, Color4 color)
        {
            Vertex = position;
            Color = color;
        }

        static public void Translate(GLVertexColour[] vertices, Vector3 pos)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Vertex.Translate(pos);
        }
        static public void Transform(GLVertexColour[] vertices, Matrix4 trans)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Vertex = Vector4.Transform(vertices[i].Vertex, trans);
            }
        }

    }

    public abstract class GLVertexColourObject : GLRenderable
    {
        public GLVertexColourObject(GLVertexColour[] vertices, IGLProgramShaders program, IGLObjectInstanceData data, PrimitiveType pt) : base(vertices.Length, program, data, pt)
        {
            // create first buffer: vertex
            GL.NamedBufferStorage(
                VertexBuffer,
                GLVertexColour.Size * vertices.Length,        // the size needed by this buffer
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


            GL.VertexArrayAttribBinding(VertexArray, 1, 0);         // implies 
            GL.EnableVertexArrayAttrib(VertexArray, 1);
            GL.VertexArrayAttribFormat(
                VertexArray,
                1,                      // attribute index, from the shader location = 1
                4,                      // size of attribute, vec4
                VertexAttribType.Float, // contains floats
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                16);                     // relative offset after a vec4

            // link the vertex array and buffer and provide the stride as size of Vertex
            GL.VertexArrayVertexBuffer(VertexArray, 0, VertexBuffer, IntPtr.Zero, GLVertexColour.Size);
        }
    }

    // Triangles, so vertex's come in 3's

    public class GLColouredTriangles : GLVertexColourObject
    {
        public GLColouredTriangles(GLVertexColour[] vertices, IGLProgramShaders program, IGLObjectInstanceData data) : base(vertices, program, data, PrimitiveType.Triangles)
        {
        }
    }

    // Triangle strip, first/second/third = T1, second/third/fourth = T2, etc

    public class GLColouredTriangleStrip : GLVertexColourObject
    {
        public GLColouredTriangleStrip(GLVertexColour[] vertices, IGLProgramShaders program, IGLObjectInstanceData data) : base(vertices, program, data, PrimitiveType.TriangleStrip)
        {
        }
    }

    // Triangle fan, first = fixed pos, each pair then defines a triangle from that vertex

    public class GLColouredTriangleFan : GLVertexColourObject
    {
        public GLColouredTriangleFan(GLVertexColour[] vertices, IGLProgramShaders program, IGLObjectInstanceData data) : base(vertices, program, data, PrimitiveType.TriangleFan)
        {
        }
    }


    // each vertex pair defines an individual line
    public class GLColouredLines : GLVertexColourObject
    {
        private float pointsize;

        public GLColouredLines(GLVertexColour[] vertices, IGLProgramShaders program, IGLObjectInstanceData data, float ps) : base(vertices, program, data, PrimitiveType.Lines)
        {
            pointsize = ps;
        }

        public override void Bind()
        {
            GL.PointSize(pointsize);
            base.Bind();
        }
    }

    public class GLColouredLineStrip : GLVertexColourObject
    {
        private float pointsize;

        public GLColouredLineStrip(GLVertexColour[] vertices, IGLProgramShaders program, IGLObjectInstanceData data, float ps) : base(vertices, program, data, PrimitiveType.LineStrip)
        {
            pointsize = ps;
        }

        public override void Bind()
        {
            GL.PointSize(pointsize);
            base.Bind();
        }
    }

    // line strips, plus vertex 0 and vertex n-1 are linked
    public class GLColouredLineLoop : GLVertexColourObject
    {
        private float pointsize;

        public GLColouredLineLoop(GLVertexColour[] vertices, IGLProgramShaders program, IGLObjectInstanceData data, float ps) : base(vertices, program, data, PrimitiveType.LineLoop)
        {
            pointsize = ps;
        }

        public override void Bind()
        {
            GL.PointSize(pointsize);
            base.Bind();
        }
    }

    // single points with a single defined size
    public class GLColouredPoints : GLVertexColourObject
    {
        private float pointsize;

        public GLColouredPoints(GLVertexColour[] vertices, IGLProgramShaders program, IGLObjectInstanceData data, float ps) : base(vertices, program, data, PrimitiveType.Points)
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