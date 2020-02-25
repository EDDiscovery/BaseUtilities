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

        public void Set(GLMatrixCalc c, int width, int height)  // set ProjectionModelMatrix to transform (x,y,0,1) screen coords to display coords (-1..+1, 1 to -1)
        {
            if (NotAllocated)
                Allocate(maxmcubsize, BufferUsageHint.DynamicCopy);

            //Matrix4 pm = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI / 2.0f), (float)width / height, 1, 100000);

            //pm.M11 *= 2.0f / width;
            //pm.M14 += -1;
            //pm.M22 *= -2.0f / height;
            //pm.M24 += 1;

            Matrix4 mat = Matrix4.Zero;
            mat.Column0 = new Vector4(2.0f / width, 0, 0, -1);      // transform of x
            mat.Column1 = new Vector4(0.0f, -2.0f / height, 0, 1);     // transform of y
            mat.Column2 = new Vector4(0, 0, 1, 0);                  // transform of z
            mat.Column3 = new Vector4(0, 0, 0, 1);                  // transform of w

            IntPtr ptr = Map(0, BufferSize);        // the whole schebang
            MapWrite(ref ptr, c.ProjectionModelMatrix);     //0, 64 long
            MapWrite(ref ptr, c.ProjectionMatrix);          //64, 64 long
            MapWrite(ref ptr, c.ModelMatrix);               //128, 64 long
            MapWrite(ref ptr, c.TargetPosition, 0);         //192, vec4, 16 long
            MapWrite(ref ptr, c.EyePosition, 0);            // 208, vec3, 16 long
            MapWrite(ref ptr, c.EyeDistance);               // 224, float, 4 long
            MapWrite(ref ptr, mat);                // 240, into the project model matrix slot
            UnMap();                                // and complete..
        }

    }

}

