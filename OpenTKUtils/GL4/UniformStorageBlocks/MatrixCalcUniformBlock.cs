/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
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

        const int maxmcubsize = Mat4size * 3 + Vec4size * 2 + sizeof(float) * 4 + Mat4size; // always use max so we can swap between.

        public void SetMinimal(GLMatrixCalc c)
        {
            if (NotAllocated)
                Allocate(maxmcubsize, BufferUsageHint.DynamicCopy);

            IntPtr ptr = Map(0, BufferSize);        // the whole schebang
            MapWrite(ref ptr, c.ProjectionModelMatrix);
            UnMap();                                // and complete..
        }

        public void Set(GLMatrixCalc c)
        {
            if (NotAllocated)
                Allocate(maxmcubsize, BufferUsageHint.DynamicCopy);

            IntPtr ptr = Map(0, BufferSize);        // the whole schebang
            MapWrite(ref ptr, c.ProjectionModelMatrix);
            MapWrite(ref ptr, c.ProjectionMatrix);
            MapWrite(ref ptr, c.ModelMatrix);
            MapWrite(ref ptr, c.TargetPosition, 0);
            MapWrite(ref ptr, c.EyePosition, 0);
            MapWrite(ref ptr, c.EyeDistance);
            UnMap();                                // and complete..
        }

        public void SetFull(GLMatrixCalc c) 
        {
            if (NotAllocated)
                Allocate(maxmcubsize, BufferUsageHint.DynamicCopy);

            Matrix4 screenmat = Matrix4.Zero;
            screenmat.Column0 = new Vector4(2.0f / c.ScreenSize.Width, 0, 0, -1);      // transform of x = x * 2 / width - 1
            screenmat.Column1 = new Vector4(0.0f, -2.0f / c.ScreenSize.Height, 0, 1);  // transform of y = y * -2 / height +1
            screenmat.Column2 = new Vector4(0, 0, 1, 0);                  // transform of z = none
            screenmat.Column3 = new Vector4(0, 0, 0, 1);                  // transform of w = none

            IntPtr ptr = Map(0, BufferSize);        // the whole schebang
            MapWrite(ref ptr, c.ProjectionModelMatrix);     //0, 64 long
            MapWrite(ref ptr, c.ProjectionMatrix);          //64, 64 long
            MapWrite(ref ptr, c.ModelMatrix);               //128, 64 long
            MapWrite(ref ptr, c.TargetPosition, 0);         //192, vec4, 16 long
            MapWrite(ref ptr, c.EyePosition, 0);            // 208, vec3, 16 long
            MapWrite(ref ptr, c.EyeDistance);               // 224, float, 4 long
            MapWrite(ref ptr, screenmat);                // 240, into the project model matrix slot
            UnMap();                                // and complete..
        }

    }

}

