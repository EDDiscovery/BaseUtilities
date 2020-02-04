/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
 * 
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
    // Compute shader, 3d noise, 8x8x8 multiple
    // Requires:
    //      3d texture to write to, bound on binding point
    
    public class ComputeShaderGaussian : GLShaderCompute
    {
        static int Localgroupsize = 8;

        private string gencode(int points, float centre, float width, float stddist, int binding)
        {
            return
@"
#version 450 core
#include OpenTKUtils.GL4.Shaders.Functions.distribution.glsl

layout (local_size_x = 8, local_size_y = 1, local_size_z = 1) in;

layout (binding=" + binding.ToStringInvariant() + @", r32f ) uniform image1D img;

void main(void)
{
    float points = " + points.ToStringInvariant() + @";       // grab the constants from caller
    float centre = " + centre.ToStringInvariant() + @";       // grab the constants from caller
    float stddist = " + stddist.ToStringInvariant() + @";       // grab the constants from caller
    float width = " + width.ToStringInvariant() + @";       // grab the constants from caller

    float x = (float(gl_GlobalInvocationID.x)/points-0.5)*2*width;      // normalise to -1 to +1, mult by width
    float g = gaussian(x,centre,stddist);
    vec4 color = vec4( g, 0,0,0);
    imageStore( img, int(gl_GlobalInvocationID.x), color);    // store back the computed dist
}
";
        }

        public ComputeShaderGaussian(int points,float centre, float width, float stddist, int binding = 4) : base(points / Localgroupsize, 1,1)
        {
            System.Diagnostics.Debug.Assert(points % Localgroupsize == 0);
            CompileLink(gencode(points, centre, width, stddist, binding));
        }
    }
}
