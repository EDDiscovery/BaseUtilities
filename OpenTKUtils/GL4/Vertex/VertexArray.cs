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

    [System.Diagnostics.DebuggerDisplay("Id {Id}")]
    public class GLVertexArray : IGLVertexArray
    {
        public int Id { get; private set; } 

        public GLVertexArray()
        {
            Id = GL.GenVertexArray();        // get the handle
            GL.BindVertexArray(Id);          // creates the array
        }


        public virtual void Bind()
        {
            GL.BindVertexArray(Id);                  // Bind vertex
        }

        public virtual void Dispose()
        {
            GL.DeleteVertexArray(Id);
        }

        public void BindAttribute(int bindingindex, int attribindex, int components, VertexAttribType vat, int reloffset = 0, int divisor = 0)
        {
            GL.VertexArrayAttribFormat(
                Id,
                attribindex,            // attribute index
                components,             // no of components per attribute, 1-4
                vat,                    // type
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                reloffset);             // relative offset, first item

            GL.VertexAttribDivisor(attribindex, divisor);                   // ORDER Important, after format .. found that out
            GL.VertexArrayAttribBinding(Id, attribindex, bindingindex);     // bind atrib to binding 
            GL.EnableVertexArrayAttrib(Id, attribindex);

            GLStatics.Check();
            //System.Diagnostics.Debug.WriteLine("ATTR " + attribindex + " to " + bindingindex + " Components " + components + " at " + reloffset + " divisor " + divisor);
        }

        public void BindAttributeI(int bindingindex, int attribindex, int components, VertexAttribType vat, int reloffset = 0, int divisor = 0)
        {
            GL.VertexArrayAttribIFormat(
                Id,
                attribindex,            // attribute index
                components,             // no of attribs
                vat,                    // type
                reloffset);             // relative offset, first item

            GL.VertexAttribDivisor(attribindex, divisor);                   // set up attribute divisor
            GL.VertexArrayAttribBinding(Id, attribindex, bindingindex);     // bind atrib to binding 
            GL.EnableVertexArrayAttrib(Id, attribindex);                    // enable attrib

            GLStatics.Check();
            //System.Diagnostics.Debug.WriteLine("ATTR " + attribindex + " to " + bindingindex + " Components " + components + " at " + reloffset + " divisor " + divisor);
        }
        
        public void BindMatrix(int bindingindex, int attribstart, int divisor = 0)      // bind a matrix..
        {
            for (int i = 0; i < 4; i++)
                BindAttribute(bindingindex, attribstart + i, 4, VertexAttribType.Float, 16*i, divisor);
        }

#if false
        public void BindBuffer(GLBuffer buf, int bindingindex, int pos, int stride)
        {
            Bind();
            GL.BindBuffer(BufferTarget.ArrayBuffer, buf.Id);
            GL.VertexArrayVertexBuffer(Id, bindingindex, buf.Id, (IntPtr)pos, stride);        // tell Array that binding index comes from this buffer.
        }


        public void BindVector4Matrix4(GLBuffer buf, int pos1, int pos2, int bindingindex = 0, int attribvec = 0, int attribtransform = 4, int dividor1 = 0, int dividor2 = 1)
        {
            BindBuffer(buf, bindingindex, 0, 0);

            //GL.VertexArrayAttribBinding(Id, attribvec, bindingindex);     // bind atrib index to binding index
            //GL.VertexAttribPointer(attribvec, 4, VertexAttribPointerType.Float, false, 16, pos1);  // attrib 0, vertices, 4 entries, float, 16 long, at 0 in buffer

            //GL.EnableVertexArrayAttrib(Id, attribvec);       // go for attrib launch!
            //GL.VertexAttribDivisor(attribvec, dividor1);      // 1 transform per instance..

            GL.VertexArrayAttribBinding(Id, attribtransform, bindingindex);     // bind atrib index to binding index, for all four columns.
            GL.VertexAttribPointer(attribtransform, 4, VertexAttribPointerType.Float, false, 64, pos2 + 0); // attrib t, 4entries, float, at offset in buffer, stride is 16x4
            GL.VertexAttribDivisor(attribtransform, dividor2);      // 1 transform per instance..
            GL.EnableVertexArrayAttrib(Id, attribtransform);

            GL.VertexArrayAttribBinding(Id, attribtransform + 1, bindingindex);
            GL.VertexAttribPointer(attribtransform + 1, 4, VertexAttribPointerType.Float, false, 64, pos2 + 4 * 4); // attrib t+1
            GL.VertexAttribDivisor(attribtransform + 1, dividor2);
            GL.EnableVertexArrayAttrib(Id, attribtransform + 1);

            GL.VertexArrayAttribBinding(Id, attribtransform + 2, bindingindex);
            GL.VertexAttribPointer(attribtransform + 2, 4, VertexAttribPointerType.Float, false, 64, pos2 + 8 * 4); // attrib t+2
            GL.VertexAttribDivisor(attribtransform + 2, dividor2);
            GL.EnableVertexArrayAttrib(Id, attribtransform + 2);

            GL.VertexArrayAttribBinding(Id, attribtransform + 3, bindingindex);
            GL.VertexAttribPointer(attribtransform + 3, 4, VertexAttribPointerType.Float, false, 64, pos2 + 12 * 4); // attrib t+3
            GL.VertexAttribDivisor(attribtransform + 3, dividor2);
            GL.EnableVertexArrayAttrib(Id, attribtransform + 3);

            GLStatics.Check();
        }


        public void BindAttribute3(int bindingindex, int attribindex, int components, VertexAttribPointerType vat, int pos, int stride, int divisor = 0)
        {
            //buf.Bind(bindingindex, posstart, stride);
            //GL.BindVertexBuffer(bindingindex, buf.Id, (IntPtr)posstart, stride);      // data at posstart/size

            GL.VertexArrayAttribBinding(Id, attribindex, bindingindex);     // bind atrib to binding 


            GL.VertexAttribPointer(attribindex, components, vat, false, stride, pos);  // set where in the buffer the attrib data comes from
            GLStatics.Check();

            GL.VertexAttribDivisor(attribindex, divisor);
            GL.EnableVertexArrayAttrib(Id, attribindex);    // enable attrib 0 - this is the layout number
            GLStatics.Check();
        }



        // pos2 = -1 means interlaced
        public void BindVector4Vector4(GLBuffer buf, int pos1, int pos2, int bindingindex = 0, int attrib1 = 0, int attrib2 = 1, int dividor1 = 0 , int dividor2 = 0)
        {
            BindBuffer(buf, bindingindex, 0, 0);

      //      BindAttribute(bindingindex, attrib1, 4, VertexAttribType.Float, pos1, dividor1);

            GL.VertexArrayAttribBinding(Id, attrib1, bindingindex);     // bind atrib index to binding index
            GL.VertexArrayAttribBinding(Id, attrib2, bindingindex);     // bind atrib index to binding index

            int stride = (pos2 == -1) ? 32 : 16;

            GL.VertexAttribPointer(attrib1, 4, VertexAttribPointerType.Float, false, stride, pos1);  // set where in the buffer the attrib data comes from
            GL.VertexAttribPointer(attrib2, 4, VertexAttribPointerType.Float, false, stride, (pos2 == -1) ? (pos1 + 16) : pos2);

            GL.VertexAttribDivisor(attrib1, dividor1);
            GL.VertexAttribDivisor(attrib2, dividor2);

            GL.EnableVertexArrayAttrib(Id, attrib1);
            GL.EnableVertexArrayAttrib(Id, attrib2);

            GLStatics.Check();
        }

        public void BindVector4Vector2(GLBuffer buf, int pos1, int pos2, int bindingindex = 0, int attrib1 = 0, int attrib2 = 1)
        {
            BindBuffer(buf, bindingindex, 0, 0);

            GL.VertexArrayAttribBinding(Id, attrib1, bindingindex);     // bind atrib index to binding index
            GL.VertexArrayAttribBinding(Id, attrib2, bindingindex);     // bind atrib index to binding index

            GL.VertexAttribPointer(attrib1, 4, VertexAttribPointerType.Float, false, 16, pos1);  // attrib 0, vertices, 4 entries, float, 16 long, at 0 in buffer
            GL.VertexAttribPointer(attrib2, 2, VertexAttribPointerType.Float, false, 8, pos2); // attrib 1, 2 entries, float, 8 long, at offset in buffer

            GL.EnableVertexArrayAttrib(Id, attrib1);       // go for attrib launch!
            GL.EnableVertexArrayAttrib(Id, attrib2);
            GLStatics.Check();
        }



        public void BindMatrix4(GLBuffer buf, int pos, int bindingindex, int attribtransform = 4, int dividor = 0)
        {
            BindBuffer(buf, bindingindex, 0, 0);

            GL.VertexArrayAttribBinding(Id, attribtransform, bindingindex);     // bind atrib index to binding index, for all four columns.
            GL.VertexArrayAttribBinding(Id, attribtransform + 1, bindingindex);
            GL.VertexArrayAttribBinding(Id, attribtransform + 2, bindingindex);
            GL.VertexArrayAttribBinding(Id, attribtransform + 3, bindingindex);

            GL.VertexAttribPointer(attribtransform, 4, VertexAttribPointerType.Float, false, 64, pos + 0); // attrib t, 4entries, float, at offset in buffer, stride is 16x4
            GL.VertexAttribPointer(attribtransform + 1, 4, VertexAttribPointerType.Float, false, 64, pos + 4 * 4); // attrib t+1
            GL.VertexAttribPointer(attribtransform + 2, 4, VertexAttribPointerType.Float, false, 64, pos + 8 * 4); // attrib t+2
            GL.VertexAttribPointer(attribtransform + 3, 4, VertexAttribPointerType.Float, false, 64, pos + 12 * 4); // attrib t+3

            GL.EnableVertexArrayAttrib(Id, attribtransform);
            GL.EnableVertexArrayAttrib(Id, attribtransform + 1);
            GL.EnableVertexArrayAttrib(Id, attribtransform + 2);
            GL.EnableVertexArrayAttrib(Id, attribtransform + 3);

            GL.VertexAttribDivisor(attribtransform, dividor);
            GL.VertexAttribDivisor(attribtransform + 1, dividor);
            GL.VertexAttribDivisor(attribtransform + 2, dividor);
            GL.VertexAttribDivisor(attribtransform + 3, dividor);

            GLStatics.Check();
        }

        public void BindVector4Vector2Matrix4(GLBuffer buf, int pos1, int pos2, int pos3, int bindingindex = 0, int attrib1 = 0, int attrib2 = 1, 
                                                                                                                int attribmatrix = 4)
        {
            BindBuffer(buf, bindingindex, 0, 0);

            GL.VertexArrayAttribBinding(Id, attrib1, bindingindex);     // bind atrib index to binding index
            GL.VertexAttribPointer(attrib1, 4, VertexAttribPointerType.Float, false, 16, pos1);  // attrib 0, vertices, 4 entries, float, 16 long, at 0 in buffer

            GL.VertexArrayAttribBinding(Id, attrib2, bindingindex);     // bind atrib index to binding index
            GL.VertexAttribPointer(attrib2, 2, VertexAttribPointerType.Float, false, 8, pos2);

            GL.VertexArrayAttribBinding(Id, attribmatrix, bindingindex);     // bind atrib index to binding index, for all four columns.
            GL.VertexArrayAttribBinding(Id, attribmatrix + 1, bindingindex);
            GL.VertexArrayAttribBinding(Id, attribmatrix + 2, bindingindex);
            GL.VertexArrayAttribBinding(Id, attribmatrix + 3, bindingindex);

            GL.VertexAttribPointer(attribmatrix, 4, VertexAttribPointerType.Float, false, 64, pos3 + 0); // attrib t, 4entries, float, at offset in buffer, stride is 16x4
            GL.VertexAttribPointer(attribmatrix + 1, 4, VertexAttribPointerType.Float, false, 64, pos3 + 4 * 4); // attrib t+1
            GL.VertexAttribPointer(attribmatrix + 2, 4, VertexAttribPointerType.Float, false, 64, pos3 + 8 * 4); // attrib t+2
            GL.VertexAttribPointer(attribmatrix + 3, 4, VertexAttribPointerType.Float, false, 64, pos3 + 12 * 4); // attrib t+3

            GL.VertexAttribDivisor(attribmatrix, 1);      // 1 transform per instance..
            GL.VertexAttribDivisor(attribmatrix + 1, 1);
            GL.VertexAttribDivisor(attribmatrix + 2, 1);
            GL.VertexAttribDivisor(attribmatrix + 3, 1);

            GL.EnableVertexArrayAttrib(Id, attrib1);       // go for attrib launch!
            GL.EnableVertexArrayAttrib(Id, attrib2);
            GL.EnableVertexArrayAttrib(Id, attribmatrix);
            GL.EnableVertexArrayAttrib(Id, attribmatrix + 1);
            GL.EnableVertexArrayAttrib(Id, attribmatrix + 2);
            GL.EnableVertexArrayAttrib(Id, attribmatrix + 3);

            GLStatics.Check();

        }

        public void BindUINT(GLBuffer buf, int pos, int uints = 1, int bindingindex = 0, int attribindex = 0)
        {
            BindBuffer(buf, bindingindex, pos, 4 * uints);

            //            GL.BindBuffer(BufferTarget.ArrayBuffer, buf.Id);
            //          GL.VertexArrayVertexBuffer(Id, bindingindex, buf.Id, IntPtr.Zero, 4 * uints);        // link Vertextarry to buffer and set stride

            GL.VertexArrayAttribBinding(Id, attribindex, bindingindex);     // bind atrib index 0 to binding index 0
            GL.EnableVertexArrayAttrib(Id, attribindex);         // enable attrib 0 - this is the layout number

            GL.VertexArrayAttribIFormat(Id,                // IFormat!  Needed to prevent auto conversion to float
                attribindex,            // attribute index, from the shader location = 0
                uints,                      // 2 entries per vertex
                VertexAttribType.UnsignedInt,  // contains unsigned ints
                pos);                     // 0 offset into item data
        }
#endif
    }
}
