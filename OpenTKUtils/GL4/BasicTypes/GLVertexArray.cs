/*
 * Copyright © 2019 Robbyxp1 @ github.com
 * Part of the EDDiscovery Project
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
            if (Id != -1)
            {
                GL.DeleteVertexArray(Id);
                Id = -1;
            }
        }

        public void Attribute(int bindingindex, int attribindex, int components, VertexAttribType vat, int reloffset = 0, int divisor = -1)
        {
            GL.VertexArrayAttribFormat(
                Id,
                attribindex,            // attribute index
                components,             // no of components per attribute, 1-4
                vat,                    // type
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                reloffset);             // relative offset, first item

            // ORDER Important, .. found that out

            if (divisor >= 0)            // normally use binding divisor..
                GL.VertexAttribDivisor(attribindex, divisor);

            GL.VertexArrayAttribBinding(Id, attribindex, bindingindex);     // bind atrib to binding    - do this after attrib format
            GL.EnableVertexArrayAttrib(Id, attribindex);

            OpenTKUtils.GLStatics.Check();
           // System.Diagnostics.Debug.WriteLine("ATTR " + attribindex + " to " + bindingindex + " Components " + components + " +" + reloffset + " divisor " + divisor);
        }

        public void AttributeI(int bindingindex, int attribindex, int components, VertexAttribType vat, int reloffset = 0, int divisor = -1)
        {
            GL.VertexArrayAttribIFormat(
                Id,
                attribindex,            // attribute index
                components,             // no of attribs
                vat,                    // type
                reloffset);             // relative offset, first item

            if (divisor >= 0)            // normally use binding divisor..
                GL.VertexAttribDivisor(attribindex, divisor);               // set up attribute divisor - doing this after doing the binding divisor screws things up

            GL.VertexArrayAttribBinding(Id, attribindex, bindingindex);     // bind atrib to binding 
            GL.EnableVertexArrayAttrib(Id, attribindex);                    // enable attrib

            OpenTKUtils.GLStatics.Check();
           // System.Diagnostics.Debug.WriteLine("ATTRI " + attribindex + " to " + bindingindex + " Components " + components + " +" + reloffset + " divisor " + divisor);
        }
        
        public void MatrixAttribute(int bindingindex, int attribstart, int divisor = 0)      // bind a matrix..
        {
            for (int i = 0; i < 4; i++)
                Attribute(bindingindex, attribstart + i, 4, VertexAttribType.Float, 16*i, divisor);
        }
    }
}
