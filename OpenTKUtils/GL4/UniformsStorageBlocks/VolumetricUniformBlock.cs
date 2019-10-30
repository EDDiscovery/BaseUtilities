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

        public static string GLSL =                         // code to include in your shader..
        @"
                layout (std140, binding = 1) uniform PointBlock
                {
	                vec4 p[8];      // model positions
	                float minz;
	                float maxz;
	                vec4 eyeposition; // model positions
	                float slicestart;
	                float slicedist;
                } pb;
        ";

        public void Set(MatrixCalc c, Vector4[] boundingbox, int slices)
        {
            if (NotAllocated)
                Allocate(Vec4size * 9 + 4 * sizeof(float) + 32, BufferUsageHint.DynamicCopy);   // extra for alignment

            IntPtr pb = Map(0, BufferSize);        // the whole schebang

            float minzv = float.MaxValue, maxzv = float.MinValue;
            for (int i = 0; i < 8; i++)
            {
                Vector4 p = Vector4.Transform(boundingbox[i], c.ModelMatrix);
                if (p.Z < minzv)
                    minzv = p.Z;
                if (p.Z > maxzv)
                    maxzv = p.Z;
                MapWrite(ref pb, p);
            }

            MapWrite(ref pb, minzv);
            MapWrite(ref pb, maxzv);
            MapWrite(ref pb, Vector4.Transform(new Vector4(c.EyePosition, 0), c.ModelMatrix));
            float slicedist = (maxzv - minzv) / (float)slices;
            float slicestart = (maxzv - minzv) / ((float)slices * 2);
            MapWrite(ref pb, slicestart);
            MapWrite(ref pb, slicedist); 
            UnMap();
        }
    }
}

