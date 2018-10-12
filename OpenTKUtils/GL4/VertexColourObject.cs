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

using OpenTK.Graphics.OpenGL4;
using System;

namespace OpenTKUtils.GL4
{
    public abstract class VertexColourObject : Renderable
    {
        public VertexColourObject(VertexColour[] vertices, Program program, PrimitiveType pt) : base(vertices.Length, program, pt)
        {
            // create first buffer: vertex
            GL.NamedBufferStorage(
                VertexBuffer,
                VertexColour.Size * vertices.Length,        // the size needed by this buffer
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
                4,                      // size of attribute, vec4
                VertexAttribType.Float, // contains floats
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                16);                     // relative offset after a vec4

            // link the vertex array and buffer and provide the stride as size of Vertex
            GL.VertexArrayVertexBuffer(VertexArray, 0, VertexBuffer, IntPtr.Zero, VertexColour.Size);
        }
    }

    // Triangles, so vertex's come in 3's

    public class Triangles : VertexColourObject
    {
        public Triangles(VertexColour[] vertices, Program program) : base(vertices, program, PrimitiveType.Triangles)
        {
        }
    }

    // Triangle strip, first/second/third = T1, second/third/fourth = T2, etc

    public class TriangleStrip : VertexColourObject
    {
        public TriangleStrip(VertexColour[] vertices, Program program) : base(vertices, program, PrimitiveType.TriangleStrip)
        {
        }
    }

    // Triangle fan, first = fixed pos, each pair then defines a triangle from that vertex

    public class TriangleFan : VertexColourObject
    {
        public TriangleFan(VertexColour[] vertices, Program program) : base(vertices, program, PrimitiveType.TriangleFan)
        {
        }
    }


    // each vertex pair defines an individual line
    public class Lines : VertexColourObject
    {
        private float pointsize;

        public Lines(VertexColour[] vertices, Program program, float ps) : base(vertices, program, PrimitiveType.Lines)
        {
            pointsize = ps;
        }

        public override void Bind()
        {
            base.Bind();
            GL.PointSize(pointsize);
        }
    }

    public class LineStrip : VertexColourObject
    {
        private float pointsize;

        public LineStrip(VertexColour[] vertices, Program program, float ps) : base(vertices, program, PrimitiveType.LineStrip)
        {
            pointsize = ps;
        }

        public override void Bind()
        {
            base.Bind();
            GL.PointSize(pointsize);
        }
    }

    // line strips, plus vertex 0 and vertex n-1 are linked
    public class LineLoop : VertexColourObject
    {
        private float pointsize;

        public LineLoop(VertexColour[] vertices, Program program, float ps) : base(vertices, program, PrimitiveType.LineLoop)
        {
            pointsize = ps;
        }

        public override void Bind()
        {
            base.Bind();
            GL.PointSize(pointsize);
        }
    }

    // single points with defined size
    public class Points : VertexColourObject
    {
        private float pointsize;

        public Points(VertexColour[] vertices, Program program, float ps) : base(vertices, program, PrimitiveType.Points)
        {
            pointsize = ps;
        }

        public override void Bind()
        {
            base.Bind();
            GL.PointSize(pointsize);
        }
    }

}