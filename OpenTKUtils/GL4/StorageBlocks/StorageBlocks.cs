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
        public GLDataBlock(int bindingindex, bool std430, int startsize, BufferTarget target, BufferRangeTarget tgr) : base(std430,startsize)
        {
            BindingIndex = bindingindex;

            GL.BindBuffer(target, Id);
            GL.BindBufferBase(tgr, BindingIndex, Id);            // binding point
            GL.BindBuffer(target, 0);
        }

        // must call at least one.. from then on you can update using writes..
        public void Complete()
        {
            GL.NamedBufferData(Id, Size, (IntPtr)0, BufferUsageHint.DynamicDraw);
            // want to write, and the previous contents may be thrown away https://www.khronos.org/registry/OpenGL-Refpages/gl4/
            WriteCacheToBuffer();
            GL4Statics.Check();
        }

        // rewrite the whole thing.. Complete must be called first.  Use after Writes without the immediate write buffer
        public void Update()
        {
            WriteCacheToBuffer();
        }
    }

    // uniform blocks - std140 only
    public class GLUniformBlock : GLDataBlock
    {
        public GLUniformBlock(int bindingindex, int defaultsize = 64) : base(bindingindex, false, defaultsize, BufferTarget.UniformBuffer, BufferRangeTarget.UniformBuffer)
        {

        }

    }

    // storage blocks - std140 and 430
    public class GLStorageBlock : GLDataBlock
    {
        public GLStorageBlock(int bindingindex, bool std430 = false, int defaultsize = 64) : base(bindingindex, std430, defaultsize, BufferTarget.ShaderStorageBuffer, BufferRangeTarget.ShaderStorageBuffer)
        {
        }
    }
}

