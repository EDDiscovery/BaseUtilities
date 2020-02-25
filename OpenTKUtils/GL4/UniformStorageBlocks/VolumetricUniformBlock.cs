/*
 * Copyright  2019 Robbyxp1 @ github.com
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

namespace OpenTKUtils.GL4
{
    public class GLVolumetricUniformBlock : GLUniformBlock 
    {
        public GLVolumetricUniformBlock() : base(1)         // binding block 1 fixed
        {
        }

        public int Set(GLMatrixCalc c, Vector4[] boundingbox, float slicesize) // return slices to show
        {
            if (NotAllocated)
                Allocate(Vec4size * 9 + 4 * sizeof(float) + 32, BufferUsageHint.DynamicCopy);   // extra for alignment, not important to get precise

            IntPtr pb = Map(0, BufferSize);        // the whole schebang

            float minzv = float.MaxValue, maxzv = float.MinValue;
            int minv = 0, maxv = 0;
            for (int i = 0; i < 8; i++)
            {
                Vector4 m = Vector4.Transform(boundingbox[i], c.ModelMatrix);
                Vector4 p = Vector4.Transform(m, c.ProjectionMatrix);
               // System.Diagnostics.Debug.WriteLine("{0} {1} -> {2} -> {3}", i, boundingbox[i].ToStringVec(), m.ToStringVec(), p.ToStringVec());
                if (m.Z < minzv)
                {
                    minzv = m.Z;
                    minv = i;
                }
                if (m.Z > maxzv)
                {
                    maxzv = m.Z;
                    maxv = i;
                }
                MapWrite(ref pb, m);
            }

            if (maxzv > 0)      // 0 is the eye plane in z, no point above it
                maxzv = 0;

            if (minzv > maxzv)
                minzv = -1;

            float zdist = maxzv - minzv;
            int slices = Math.Max(1, (int)(zdist / slicesize));

            MapWrite(ref pb, minzv);
            MapWrite(ref pb, (float)slicesize);
            UnMap();
            //System.Diagnostics.Debug.WriteLine("Z from {0}:{1} to {2}:{3} slices {4} dist {5}", minv, minzv, maxv, maxzv, slices, slicesize);

            return slices;
        }

    }
}

