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
using System.Collections.Generic;

namespace OpenTKUtils.GL4
{
    // Base Class for vertex data to vertex shader..

    public class GLVertexArray : IGLVertexArray
    {
        public int Array { get; private set; }                            // the vertex GL Array 

        public GLVertexArray()
        {
            Array = GL.GenVertexArray();        // get the handle
            GL.BindVertexArray(Array);          // creates the array
        }

        public int Id { get { return Array; } }

        public virtual void Bind()
        {
            GL.BindVertexArray(Array);                  // Bind vertex
        }

        public virtual void Dispose()
        {
            GL.DeleteVertexArray(Array);
        }

        public void BindVector4x2(GLBuffer buf, int pos1, int pos2, int bindingindex = 0, int attriblayoutindexposition = 0, int attriblayoutcolour = 1)
        {
            Bind();

            GL.BindBuffer(BufferTarget.ArrayBuffer, buf.Id);

            GL.VertexArrayVertexBuffer(Id, bindingindex, Id, IntPtr.Zero, 0);        // tell Array that binding index comes from this buffer.

            GL.VertexArrayAttribBinding(Id, attriblayoutindexposition, bindingindex);     // bind atrib index to binding index
            GL.VertexArrayAttribBinding(Id, attriblayoutcolour, bindingindex);     // bind atrib index to binding index

            GL.VertexAttribPointer(attriblayoutindexposition, 4, VertexAttribPointerType.Float, false, 16, pos1);  // set where in the buffer the attrib data comes from
            GL.VertexAttribPointer(attriblayoutcolour, 4, VertexAttribPointerType.Float, false, 16, pos2);

            GL.EnableVertexArrayAttrib(Id, attriblayoutindexposition);
            GL.EnableVertexArrayAttrib(Id, attriblayoutcolour);

            GLStatics.Check();
        }

        public void BindVector4(GLBuffer buf, int pos, int bindingindex = 0, int attribindex = 0)
        {
            Bind();

            GL.VertexArrayVertexBuffer(Id, bindingindex, buf.Id, IntPtr.Zero, 16);        // tell Array that binding index comes from this buffer.

            GL.VertexArrayAttribBinding(Id, attribindex, bindingindex);     // bind atrib index 0 to binding index 0

            GL.VertexArrayAttribFormat(
                Id,
                attribindex,            // attribute index, from the shader location = 0
                4,                      // size of attribute, vec4
                VertexAttribType.Float, // contains floats
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                pos);                   // relative offset, first item

            GL.EnableVertexArrayAttrib(Id, attribindex);         // enable attrib 0 - this is the layout number

            GLStatics.Check();
        }

        public void BindVector4Vector2(GLBuffer buf, int pos1, int pos2, int bindingindex = 0, int attriblayoutindexposition = 0, int attriblayouttexcoord = 1)
        {
            Bind();

            GL.BindBuffer(BufferTarget.ArrayBuffer, buf.Id);

            GL.VertexArrayVertexBuffer(Id, bindingindex, buf.Id, IntPtr.Zero, 0);        // tell Id that binding index comes from this buffer.

            GL.VertexArrayAttribBinding(Id, attriblayoutindexposition, bindingindex);     // bind atrib index to binding index
            GL.VertexArrayAttribBinding(Id, attriblayouttexcoord, bindingindex);     // bind atrib index to binding index

            GL.VertexAttribPointer(attriblayoutindexposition, 4, VertexAttribPointerType.Float, false, 16, pos1);  // attrib 0, vertices, 4 entries, float, 16 long, at 0 in buffer
            GL.VertexAttribPointer(attriblayouttexcoord, 2, VertexAttribPointerType.Float, false, 8, pos2); // attrib 1, 2 entries, float, 8 long, at offset in buffer

            GL.EnableVertexArrayAttrib(Id, attriblayoutindexposition);       // go for attrib launch!
            GL.EnableVertexArrayAttrib(Id, attriblayouttexcoord);
        }


        public void BindVector4Matrix4(GLBuffer buf, int pos1, int pos2, int bindingindex = 0, int attriblayoutindexposition = 0, int attriblayouttransforms = 4)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, buf.Id);

            GL.VertexArrayVertexBuffer(Id, bindingindex, buf.Id, IntPtr.Zero, 0);        // tell Array that binding index comes from this buffer.

            GL.VertexArrayAttribBinding(Id, attriblayoutindexposition, bindingindex);     // bind atrib index to binding index
            GL.VertexAttribPointer(attriblayoutindexposition, 4, VertexAttribPointerType.Float, false, 16, pos1);  // attrib 0, vertices, 4 entries, float, 16 long, at 0 in buffer

            GL.VertexArrayAttribBinding(Id, attriblayouttransforms, bindingindex);     // bind atrib index to binding index, for all four columns.
            GL.VertexArrayAttribBinding(Id, attriblayouttransforms + 1, bindingindex);
            GL.VertexArrayAttribBinding(Id, attriblayouttransforms + 2, bindingindex);
            GL.VertexArrayAttribBinding(Id, attriblayouttransforms + 3, bindingindex);

            GL.VertexAttribPointer(attriblayouttransforms, 4, VertexAttribPointerType.Float, false, 64, pos2 + 0); // attrib t, 4entries, float, at offset in buffer, stride is 16x4
            GL.VertexAttribPointer(attriblayouttransforms + 1, 4, VertexAttribPointerType.Float, false, 64, pos2 + 4 * 4); // attrib t+1
            GL.VertexAttribPointer(attriblayouttransforms + 2, 4, VertexAttribPointerType.Float, false, 64, pos2 + 8 * 4); // attrib t+2
            GL.VertexAttribPointer(attriblayouttransforms + 3, 4, VertexAttribPointerType.Float, false, 64, pos2 + 12 * 4); // attrib t+3

            GL.VertexAttribDivisor(attriblayouttransforms, 1);      // 1 transform per instance..
            GL.VertexAttribDivisor(attriblayouttransforms + 1, 1);
            GL.VertexAttribDivisor(attriblayouttransforms + 2, 1);
            GL.VertexAttribDivisor(attriblayouttransforms + 3, 1);

            GL.EnableVertexArrayAttrib(Id, attriblayoutindexposition);       // go for attrib launch!
            GL.EnableVertexArrayAttrib(Id, attriblayouttransforms);
            GL.EnableVertexArrayAttrib(Id, attriblayouttransforms + 1);
            GL.EnableVertexArrayAttrib(Id, attriblayouttransforms + 2);
            GL.EnableVertexArrayAttrib(Id, attriblayouttransforms + 3);

            GLStatics.Check();
        }

        public void BindVector4Vector2Matrix4(GLBuffer buf, int pos1, int pos2, int pos3, int bindingindex = 0, int attriblayoutindexposition = 0, int attriblayouttexcoord = 1, 
                                                                                                                int attriblayouttransforms = 4)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, buf.Id);

            GL.VertexArrayVertexBuffer(Array, bindingindex, buf.Id, IntPtr.Zero, 0);        // tell Array that binding index comes from this buffer.

            GL.VertexArrayAttribBinding(Array, attriblayoutindexposition, bindingindex);     // bind atrib index to binding index
            GL.VertexAttribPointer(attriblayoutindexposition, 4, VertexAttribPointerType.Float, false, 16, pos1);  // attrib 0, vertices, 4 entries, float, 16 long, at 0 in buffer

            GL.VertexArrayAttribBinding(Array, attriblayouttexcoord, bindingindex);     // bind atrib index to binding index
            GL.VertexAttribPointer(attriblayouttexcoord, 2, VertexAttribPointerType.Float, false, 8, pos2);

            GL.VertexArrayAttribBinding(Array, attriblayouttransforms, bindingindex);     // bind atrib index to binding index, for all four columns.
            GL.VertexArrayAttribBinding(Array, attriblayouttransforms + 1, bindingindex);
            GL.VertexArrayAttribBinding(Array, attriblayouttransforms + 2, bindingindex);
            GL.VertexArrayAttribBinding(Array, attriblayouttransforms + 3, bindingindex);

            GL.VertexAttribPointer(attriblayouttransforms, 4, VertexAttribPointerType.Float, false, 64, pos3 + 0); // attrib t, 4entries, float, at offset in buffer, stride is 16x4
            GL.VertexAttribPointer(attriblayouttransforms + 1, 4, VertexAttribPointerType.Float, false, 64, pos3 + 4 * 4); // attrib t+1
            GL.VertexAttribPointer(attriblayouttransforms + 2, 4, VertexAttribPointerType.Float, false, 64, pos3 + 8 * 4); // attrib t+2
            GL.VertexAttribPointer(attriblayouttransforms + 3, 4, VertexAttribPointerType.Float, false, 64, pos3 + 12 * 4); // attrib t+3

            GL.VertexAttribDivisor(attriblayouttransforms, 1);      // 1 transform per instance..
            GL.VertexAttribDivisor(attriblayouttransforms + 1, 1);
            GL.VertexAttribDivisor(attriblayouttransforms + 2, 1);
            GL.VertexAttribDivisor(attriblayouttransforms + 3, 1);

            GL.EnableVertexArrayAttrib(Array, attriblayoutindexposition);       // go for attrib launch!
            GL.EnableVertexArrayAttrib(Array, attriblayouttexcoord);
            GL.EnableVertexArrayAttrib(Array, attriblayouttransforms);
            GL.EnableVertexArrayAttrib(Array, attriblayouttransforms + 1);
            GL.EnableVertexArrayAttrib(Array, attriblayouttransforms + 2);
            GL.EnableVertexArrayAttrib(Array, attriblayouttransforms + 3);

            GLStatics.Check();

        }

        public void BindUINT(GLBuffer buf, int pos, int uints = 1, int bindingindex = 0, int attribindex = 0)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, buf.Id);

            GL.VertexArrayVertexBuffer(Array, bindingindex, buf.Id, IntPtr.Zero, 4 * uints);        // link Vertextarry to buffer and set stride

            GL.VertexArrayAttribBinding(Array, attribindex, bindingindex);     // bind atrib index 0 to binding index 0
            GL.EnableVertexArrayAttrib(Array, attribindex);         // enable attrib 0 - this is the layout number

            GL.VertexArrayAttribIFormat(Array,                // IFormat!  Needed to prevent auto conversion to float
                attribindex,            // attribute index, from the shader location = 0
                uints,                      // 2 entries per vertex
                VertexAttribType.UnsignedInt,  // contains unsigned ints
                pos);                     // 0 offset into item data
        }

    }
}
