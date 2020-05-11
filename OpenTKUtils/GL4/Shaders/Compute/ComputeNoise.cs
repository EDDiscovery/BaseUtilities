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
    
    public class ComputeShaderNoise3D : GLShaderCompute
    {
        static int Localgroupsize = 8;

        private string gencode(int w, int h, int d, int wb, int hb, int db, int binding)
        {
            return
@"
#version 450 core
#include Shaders.Functions.noise3.glsl
#include Shaders.Functions.random.glsl

layout (local_size_x = 8, local_size_y = 8, local_size_z = 8) in;

layout (binding=" + binding.ToStringInvariant() + @", r32f ) uniform image3D img;

void main(void)
{
    ivec3 p = ivec3(gl_GlobalInvocationID.xyz);

    float w = " + w.ToStringInvariant() + @";       // grab the constants from caller
    float h = " + h.ToStringInvariant() + @";
    float d = " + d.ToStringInvariant() + @";
    float wb = " + wb.ToStringInvariant() + @";     // these set the granularity of the image..
    float hb = " + hb.ToStringInvariant() + @";
    float db = " + db.ToStringInvariant() + @";

    vec3 np = vec3( float(gl_GlobalInvocationID.x)/w*wb, float(gl_GlobalInvocationID.y)/h*hb,float(gl_GlobalInvocationID.z)/d*db);

    float f = gradientnoiseT1(np);
    vec4 color = vec4(f*0.5+0.5,0,0,1);             // red only

    imageStore( img, p, color);                     // store back the computed noise
}
";
        }

        // width/height/depth determine points, with wb/hb/db the granularity of the noise

        public ComputeShaderNoise3D(int width, int height, int depth, int wb, int hb, int db, int binding = 3) : base(width / Localgroupsize, height / Localgroupsize, depth / Localgroupsize)
        {
            System.Diagnostics.Debug.Assert(width % 8 == 0);
            System.Diagnostics.Debug.Assert(height % 8 == 0);
            System.Diagnostics.Debug.Assert(depth % 8 == 0);
            CompileLink(gencode(width, height, depth, wb, hb, db, binding));
        }
    }
}
