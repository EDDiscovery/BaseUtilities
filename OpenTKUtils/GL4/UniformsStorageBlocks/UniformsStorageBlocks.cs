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

using System;

using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    // Class supports writing to local data then to the buffer object.

    public abstract class GLDataBlock : GLBuffer
    {
        public int BindingIndex { get; private set; }

        //public GLDataBlock(int bp, bool std430, int isz, BufferTarget tg = BufferTarget.UniformBuffer, BufferRangeTarget tgr = BufferRangeTarget.UniformBuffer) : base(isz)
        public GLDataBlock(int bindingindex, bool std430, BufferTarget target, BufferRangeTarget tgr) : base(std430)
        {
            BindingIndex = bindingindex;

            GL.BindBuffer(target, Id);
            GL.BindBufferBase(tgr, BindingIndex, Id);            // binding point
            GL.BindBuffer(target, 0);
        }
       
    }

    // uniform blocks - std140 only
    public class GLUniformBlock : GLDataBlock
    {
        public GLUniformBlock(int bindingindex) : base(bindingindex, false, BufferTarget.UniformBuffer, BufferRangeTarget.UniformBuffer)
        {

        }

    }

    // storage blocks - std140 and 430
    public class GLStorageBlock : GLDataBlock
    {
        public GLStorageBlock(int bindingindex, bool std430 = false): base(bindingindex, std430, BufferTarget.ShaderStorageBuffer, BufferRangeTarget.ShaderStorageBuffer)
        {
        }
    }
}

