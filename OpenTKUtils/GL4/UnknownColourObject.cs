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
    public abstract class GLColourObject            // colour on its own
    {
        // layout(location = 1) in vec4 color;

        const int attriblayoutindexposition = 1;

        protected int ColourArray;                  // the vertex GL Array 
        protected int ColourBuffer;                 // its buffer data
        protected int ColourCount;                 // and size...

        public GLColourObject(Color4 [] colours) 
        {
            ColourCount = colours.Length;
            ColourArray = GL.GenVertexArray();
            ColourBuffer = GL.GenBuffer();

            // one buffer holding position and colour data interspersed.
            GL.NamedBufferStorage(
                ColourBuffer,
                4 * 4 * colours.Length,        // the size needed by this buffer
                colours,                           // data to initialize with
                BufferStorageFlags.MapWriteBit);    // at this point we will only write to the buffer

            const int bindingindex = 0;

            // first position vectors, to attrib 0
            GL.VertexArrayAttribBinding(ColourArray, attriblayoutindexposition, bindingindex);         
            GL.EnableVertexArrayAttrib(ColourArray, attriblayoutindexposition);
            GL.VertexArrayAttribFormat(
                ColourArray,           // for colour buffer
                attriblayoutindexposition,  // attribute index, from the shader location 
                4,                      // size of attribute, vec4, 4 floats
                VertexAttribType.Float, // contains floats
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                0);                     // relative offset, first item

            // link the vertex array and buffer and provide the stride as size of VertexColour
            GL.VertexArrayVertexBuffer(ColourArray, bindingindex, ColourBuffer, IntPtr.Zero, 16);
        }
    }

    
}