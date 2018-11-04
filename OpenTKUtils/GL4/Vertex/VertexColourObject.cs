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
    // Vertex and colour data

    public abstract class GLVertexColourObject : GLVertexArray
    {
        // Vertex shader must implement
        // layout(location = 0) in vec4 position;
        // layout(location = 1) in vec4 color;
        const int attriblayoutindexposition = 0;
        const int attriblayoutcolour = 1;
        const int bindingindex = 0;

        GLBuffer buffer;

        // colour data can be shorted than vertices, and will be repeated.
        public GLVertexColourObject(Vector4[] vertices, Color4[] colours , IGLObjectInstanceData data, PrimitiveType pt) : base(vertices.Length,data,pt)
        {
            buffer = new GLBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.Id);

            var pos = buffer.Write(vertices, colours);

            GL.VertexArrayVertexBuffer(Array, bindingindex, buffer.Id, IntPtr.Zero, 0);        // tell Array that binding index comes from this buffer.

            GL.VertexArrayAttribBinding(Array, attriblayoutindexposition, bindingindex);     // bind atrib index to binding index
            GL.VertexArrayAttribBinding(Array, attriblayoutcolour, bindingindex);     // bind atrib index to binding index

            GL.VertexAttribPointer(attriblayoutindexposition, 4, VertexAttribPointerType.Float, false, 16, pos.Item1);  // set where in the buffer the attrib data comes from
            GL.VertexAttribPointer(attriblayoutcolour, 4, VertexAttribPointerType.Float, false, 16, pos.Item2);

            GL.EnableVertexArrayAttrib(Array, attriblayoutindexposition);
            GL.EnableVertexArrayAttrib(Array, attriblayoutcolour);

            GL4Statics.Check();
        }

        public override void Dispose()
        {
            base.Dispose();
            buffer.Dispose();
        }

    }

    // Triangles, so vertex's come in 3's

    public class GLColouredTriangles : GLVertexColourObject
    {
        public GLColouredTriangles(Vector4[] vertices, Color4[] colours, IGLObjectInstanceData data ) : base(vertices, colours, data, PrimitiveType.Triangles)
        {
        }
    }

    // Triangle strip, first/second/third = T1, second/third/fourth = T2, etc

    public class GLColouredTriangleStrip: GLVertexColourObject
    {
        public GLColouredTriangleStrip(Vector4[] vertices, Color4[] colours,  IGLObjectInstanceData data) : base(vertices, colours, data, PrimitiveType.TriangleStrip)
        {
        }
    }

    // Triangle fan, first = fixed pos, each pair then defines a triangle from that vertex

    public class GLColouredTriangleFan : GLVertexColourObject
    {
        public GLColouredTriangleFan(Vector4[] vertices, Color4[] colours, IGLObjectInstanceData data) : base(vertices, colours, data, PrimitiveType.TriangleFan)
        {
        }
    }


    // each vertex pair defines an individual line
    public class GLColouredLines : GLVertexColourObject
    {
        public GLColouredLines(Vector4[] vertices, Color4[] colours, IGLObjectInstanceData data) : base(vertices, colours, data, PrimitiveType.Lines)
        {
        }

    }

    public class GLColouredLineStrip : GLVertexColourObject
    {
        public GLColouredLineStrip(Vector4[] vertices, Color4[] colours, IGLObjectInstanceData data) : base(vertices, colours, data, PrimitiveType.LineStrip)
        {
        }
    }

    // line strips, plus vertex 0 and vertex n-1 are linked
    public class GLColouredLineLoop : GLVertexColourObject
    {
        public GLColouredLineLoop(Vector4[] vertices, Color4[] colours,  IGLObjectInstanceData data) : base(vertices, colours, data, PrimitiveType.LineLoop)
        {
        }
    }

    // single points with a single defined size
    public class GLColouredPoints : GLVertexColourObject
    {
        public GLColouredPoints(Vector4[] vertices, Color4[] colours, IGLObjectInstanceData data) : base(vertices, colours, data, PrimitiveType.Points)
        {
        }

    }

}