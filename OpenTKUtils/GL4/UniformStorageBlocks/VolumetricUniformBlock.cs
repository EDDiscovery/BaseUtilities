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
        public GLVolumetricUniformBlock(int bindingpoint = 1 ) : base(bindingpoint)         // binding block 1 is default
        {
        }

        // if slicesize=0, we take 1 slice in the middle of the z volume
        // otherwise we slice up min/max z by slicesize

        public int Set(GLMatrixCalc c, Vector4[] boundingbox, float slicesize) // return slices to show
        {
            if (NotAllocated)
                AllocateBytes(Vec4size * 9 + 4 * sizeof(float) + 32, BufferUsageHint.DynamicCopy);   // extra for alignment, not important to get precise

            StartMapWrite(0);        // the whole schebang

            float minzv = float.MaxValue, maxzv = float.MinValue;
            for (int i = 0; i < boundingbox.Length; i++)
            {
                Vector4 m = Vector4.Transform(boundingbox[i], c.ModelMatrix);
                MapWrite(m);
                if (m.Z < minzv)
                {
                    minzv = m.Z;
                }
                if (m.Z > maxzv)
                {
                    maxzv = m.Z;
                }

                //c.WorldToScreen(boundingbox[i], i.ToString() + " " );
            }

            //if (maxzv > 0)      // 0 is the eye plane in z, no point above it tbd
            //maxzv = 0;

            //if (minzv > maxzv)
            //  minzv = -1;

            int slices = 1;
            if (slicesize == 0 )
            {
                minzv = (minzv + maxzv) / 2;
            }
            else
            {
                slices = Math.Max(1, (int)((maxzv-minzv) / slicesize));
            }

           // System.Diagnostics.Debug.WriteLine("..Z Calc {0} {1} slices {2} slicesize {3}", minzv, maxzv, slices, slicesize);

            MapWrite(minzv);
            MapWrite((float)slicesize);
            UnMap();

            //Vector4 t0 = new Vector4(0, 12000, 25666, 1);
            //c.WorldToScreen(t0, "Sv");
            //Vector4 t01 = new Vector4(0, -6000, 25666, 1);
            //c.WorldToScreen(t01, "Sm");
            //Vector4 t1 = new Vector4(0, 2000, 25666, 1);
            //c.WorldToScreen(t1, "Sh");
            //Vector4 t2 = new Vector4(0, 0, 25666, 1);
            //c.WorldToScreen(t2, "sa");
            //Vector4 t3 = new Vector4(0, 0, 0, 1);
            //c.WorldToScreen(t3, "so");

            ////   TestZ(c, boundingbox, minzv);

            return slices;
        }

        // code from the volumetric testers to help debug stuff

        void Test(GLMatrixCalc c, Vector4[] boundingbox, float percent)
        {
            Vector4[] modelboundingbox = boundingbox.Transform(c.ModelMatrix);

            float minzv = float.MaxValue, maxzv = float.MinValue;
            foreach (var m in modelboundingbox)
            {
                if (m.Z < minzv)
                {
                    minzv = m.Z;
                }
                if (m.Z > maxzv)
                {
                    maxzv = m.Z;
                }

                Vector4[] proj = modelboundingbox.Transform(c.ProjectionMatrix);
                System.Diagnostics.Debug.WriteLine("Model {0} -> {1}", m, proj);
            }

            System.Diagnostics.Debug.WriteLine("Min {0} Max {1}", minzv, maxzv);

            float zpoint = minzv + (maxzv - minzv) * percent;
            TestZ(c, boundingbox, zpoint);
        }

        void TestZ(GLMatrixCalc c, Vector4[] boundingbox, float zpoint)
        {
            Vector4[] modelboundingbox = boundingbox.Transform(c.ModelMatrix);

            Vector4[] intercepts = new Vector4[6];
            Vector3[] texpoints = new Vector3[6];
            int count = 0;
            modelboundingbox[0].FindVectorFromZ(modelboundingbox[1], ref intercepts, ref texpoints, new Vector3(0, 9, 0), ref count, zpoint);  
            modelboundingbox[1].FindVectorFromZ(modelboundingbox[2], ref intercepts, ref texpoints, new Vector3(9, 1, 0), ref count, zpoint);
            modelboundingbox[3].FindVectorFromZ(modelboundingbox[2], ref intercepts, ref texpoints, new Vector3(1, 9, 0), ref count, zpoint);
            modelboundingbox[0].FindVectorFromZ(modelboundingbox[3], ref intercepts, ref texpoints, new Vector3(9, 0, 0), ref count, zpoint);

            modelboundingbox[4].FindVectorFromZ(modelboundingbox[5], ref intercepts, ref texpoints, new Vector3(0, 9, 1), ref count, zpoint);
            modelboundingbox[5].FindVectorFromZ(modelboundingbox[6], ref intercepts, ref texpoints, new Vector3(9, 1, 1), ref count, zpoint);
            modelboundingbox[7].FindVectorFromZ(modelboundingbox[6], ref intercepts, ref texpoints, new Vector3(1, 9, 1), ref count, zpoint);
            modelboundingbox[4].FindVectorFromZ(modelboundingbox[7], ref intercepts, ref texpoints, new Vector3(9, 0, 1), ref count, zpoint);

            modelboundingbox[0].FindVectorFromZ(modelboundingbox[4], ref intercepts, ref texpoints, new Vector3(0, 0, 9), ref count, zpoint);
            modelboundingbox[1].FindVectorFromZ(modelboundingbox[5], ref intercepts, ref texpoints, new Vector3(0, 1, 9), ref count, zpoint);
            modelboundingbox[2].FindVectorFromZ(modelboundingbox[6], ref intercepts, ref texpoints, new Vector3(1, 1, 9), ref count, zpoint);
            modelboundingbox[3].FindVectorFromZ(modelboundingbox[7], ref intercepts, ref texpoints, new Vector3(1, 0, 9), ref count, zpoint);   

            if (count >= 3)
            {
                Vector4 avg = intercepts.Average();
                float[] angles = new float[6];
                for (int i = 0; i < count; i++)
                {
                    angles[i] = -(float)Math.Atan2(intercepts[i].Y - avg.Y, intercepts[i].X - avg.X);        // all on the same z plane, so x/y only need be considered
                    System.Diagnostics.Debug.WriteLine("C" + intercepts[i].ToStringVec() + " ang " + angles[i].Degrees() + " t " + texpoints[i]);
                }

            }
        }
    }
}

