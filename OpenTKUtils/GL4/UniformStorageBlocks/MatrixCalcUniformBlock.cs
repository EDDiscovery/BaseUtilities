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


using System;

using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTKUtils.Common;

namespace OpenTKUtils.GL4
{
    // Matrix calc UB block at 0 fixed

    public class GLMatrixCalcUniformBlock : GLUniformBlock 
    {
        public GLMatrixCalcUniformBlock() : base(0)         // 0 is the fixed binding block for matrixcalc
        {
        }

        public void SetMinimal(GLMatrixCalc c)
        {
            if (NotAllocated)
                Allocate(Mat4size * 1, BufferUsageHint.DynamicCopy);

            IntPtr ptr = Map(0, BufferSize);        // the whole schebang
            MapWrite(ref ptr, c.ProjectionModelMatrix);
            UnMap();                                // and complete..
        }

        public void Set(GLMatrixCalc c)
        {
            if (NotAllocated)
                Allocate(Mat4size * 3 + Vec4size * 2 + sizeof(float), BufferUsageHint.DynamicCopy);

            IntPtr ptr = Map(0, BufferSize);        // the whole schebang
            MapWrite(ref ptr, c.ProjectionModelMatrix);

            MapWrite(ref ptr, c.ProjectionMatrix);
            MapWrite(ref ptr, c.ModelMatrix);
            MapWrite(ref ptr, c.TargetPosition, 0);
            MapWrite(ref ptr, c.EyePosition, 0);
            MapWrite(ref ptr, c.EyeDistance);
            UnMap();                                // and complete..
        }

        public void Set(GLMatrixCalc c, int width, int height)
        {
            if (NotAllocated)
                Allocate(Mat4size * 3 + Vec4size * 2 + sizeof(float) + sizeof(int)*2, BufferUsageHint.DynamicCopy);

            IntPtr ptr = Map(0, BufferSize);        // the whole schebang
            MapWrite(ref ptr, c.ProjectionModelMatrix);

            MapWrite(ref ptr, c.ProjectionMatrix);
            MapWrite(ref ptr, c.ModelMatrix);
            MapWrite(ref ptr, c.TargetPosition, 0);
            MapWrite(ref ptr, c.EyePosition, 0);
            MapWrite(ref ptr, c.EyeDistance);
            MapWrite(ref ptr, width);
            MapWrite(ref ptr, height);
            UnMap();                                // and complete..
        }

    }

}

