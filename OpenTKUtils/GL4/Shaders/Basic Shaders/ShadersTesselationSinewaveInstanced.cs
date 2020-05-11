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
    // Shader, with tesselation, and Y change in amp using sin
    // worldposition = instanced, xyz = position, w = image selection
    // Need to feed uniform 22 with common transform

    public class GLTesselationShaderSinewaveInstanced : GLShaderStandard
    {
        string vert =
        @"
#version 450 core
layout (location = 0) in vec4 modelposition;
layout (location = 1) in vec4 worldposition;

out vec4 worldposinstance;

void main(void)
{
	gl_Position = vec4(modelposition.xyz,1);       
    worldposinstance = worldposition;
}
";

        string TCS(float tesselation)
        {
            return
        @"
#version 450 core

layout (vertices = 4) out;

in vec4 worldposinstance[];         // pass thru this array. TCS is run one per vertex
out vec4 tcs_worldposinstance[];

void main(void)
{
    float tess = " + tesselation.ToString() + @";

    if ( gl_InvocationID == 0 )
    {
        gl_TessLevelInner[0] =  tess;
        gl_TessLevelInner[1] =  tess;
        gl_TessLevelOuter[0] =  tess;
        gl_TessLevelOuter[1] =  tess;
        gl_TessLevelOuter[2] =  tess;
        gl_TessLevelOuter[3] =  tess;
    }

    gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;
    tcs_worldposinstance[gl_InvocationID] = worldposinstance[gl_InvocationID];
}
";
        }

        string TES(float amplitude,float repeats)       // run once per patch
        {
            return

@"
#version 450 core
#include UniformStorageBlocks.matrixcalc.glsl
#include Shaders.Functions.rotations.glsl
layout (quads) in; 

layout (location = 22) uniform  mat4 transform;
layout (location = 26) uniform  float phase;

in vec4 tcs_worldposinstance[];

out vec2 vs_textureCoordinate;
flat out int imageno;

void main(void)
{
    float amp = " + amplitude + @";
    vs_textureCoordinate = vec2(gl_TessCoord.x,1.0-gl_TessCoord.y);         //1.0-y is due to the project model turning Y upside down so Y points upwards on screen

    vec4 p1 = mix(gl_in[0].gl_Position, gl_in[1].gl_Position, gl_TessCoord.x);  
    vec4 p2 = mix(gl_in[2].gl_Position, gl_in[3].gl_Position, gl_TessCoord.x); 
    vec4 pos = mix(p1, p2, gl_TessCoord.y);                                     

    pos.y += amp*sin((phase+gl_TessCoord.x)*3.142*2*" + repeats + @");           // .x goes 0-1, phase goes 0-1, convert to radians

    imageno = int(tcs_worldposinstance[0].w);

    mat4 trans = mat4translation(tcs_worldposinstance[0].xyz);
    mat4 rot = mat4rotateZ(radians(22));

    gl_Position = mc.ProjectionModelMatrix * transform * trans * rot * pos;
//    pos += vec4(tcs_worldposinstance[0].xyz,0);     // shift by instance pos
    //gl_Position = mc.ProjectionModelMatrix * transform * pos;
}
";
        }

        string frag =

@"
#version 450 core
in vec2 vs_textureCoordinate;
out vec4 color;
layout (binding=1) uniform sampler2DArray textureObject2D;

flat in int imageno;      

void main(void)
{
    color = texture(textureObject2D, vec3(vs_textureCoordinate,imageno));       // vs_texture coords normalised 0 to 1.0f
}
";

        public float Phase { get; set; } = 0;                   // set to animate.

        public GLTesselationShaderSinewaveInstanced(float tesselation,float amplitude, float repeats)
        {
            CompileLink(vertex: vert, tcs: TCS(tesselation), tes: TES(amplitude,repeats), frag: frag);
        }

        public override void Start()
        {
            base.Start();
            GL.ProgramUniform1(Id, 26, Phase);
            OpenTKUtils.GLStatics.Check();
        }

        public override void Finish()
        {
            base.Finish();
        }
    }
}

