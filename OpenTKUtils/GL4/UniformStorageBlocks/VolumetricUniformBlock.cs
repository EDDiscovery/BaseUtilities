/*
 * Copyright © 2015 - 2019 EDDiscovery development team
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
using OpenTKUtils.Common;

namespace OpenTKUtils.GL4
{
    public class GLVolumetricUniformBlock : GLUniformBlock 
    {
        public GLVolumetricUniformBlock() : base(1)         // binding block 1 fixed
        {
        }

        public void Set(MatrixCalc c, Vector4[] boundingbox, int slices)
        {
            if (NotAllocated)
                Allocate(Vec4size * 9 + 4 * sizeof(float) + 32, BufferUsageHint.DynamicCopy);   // extra for alignment

            IntPtr pb = Map(0, BufferSize);        // the whole schebang

            float minzv = float.MaxValue, maxzv = float.MinValue;
            int minv=0, maxv=0;
            for (int i = 0; i < 8; i++)
            {
                Vector4 m = Vector4.Transform(boundingbox[i], c.ModelMatrix);
             //   Vector4 p = Vector4.Transform(m, c.ProjectionMatrix);
             //   System.Diagnostics.Debug.WriteLine("{0} {1} -> {2} -> {3}", i, boundingbox[i].ToStringVec(), m.ToStringVec() , p.ToStringVec());
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

            MapWrite(ref pb, minzv);
            MapWrite(ref pb, maxzv);
            Vector4 eyemodel = Vector4.Transform(new Vector4(c.EyePosition, 0), c.ModelMatrix);
            MapWrite(ref pb, eyemodel);

            float slicedist = (maxzv - minzv) / (float)slices;
            float slicestart = minzv + (maxzv - minzv) / ((float)slices * 2);
            float sliceend = slicestart + slicedist * slices;

            //slicestart = maxzv - (maxzv - minzv) / ((float)slices * 2);
            //slicestart += slicedist;

            //float slicedist = 1f;
            //float slicestart = maxzv - slices * slicedist;

            MapWrite(ref pb, slicestart);
            MapWrite(ref pb, slicedist); 
            UnMap();
            System.Diagnostics.Debug.WriteLine("Z from {0}:{1} to {2}:{3} eyemodel {4} : {5}..{6} delta {7}", minv, minzv, maxv, maxzv, eyemodel.Z, slicestart, sliceend, slicedist);
        }
    }
}

