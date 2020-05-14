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

 using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    // Pipeline shader, Tesselation Control, with sinewave
    // input gl_in
    // input (1) tcs_worldposinstance
    // input (2) tcs_instance
    // output gl_position
    // output (0) vs_textureCoordinate
    // output (1) imageno - from w in tcs_worldposinstance
    // input (2) instance number from first vertex of primitive

    public class GLPLTesselationEvaluateSinewave : GLShaderPipelineShadersBase
    {
        string TES()
        {
            return
@"
#version 450 core
#include UniformStorageBlocks.matrixcalc.glsl
#include Shaders.Functions.trig.glsl
layout (quads) in; 

layout (location = 26) uniform  float phase;

in gl_PerVertex
{
  vec4 gl_Position;
  float gl_PointSize;
  float gl_ClipDistance[];
} gl_in[];

layout( location = 1 ) in vec4 tcs_worldposinstance[];
layout( location = 2 ) in int tcs_instance[];

out gl_PerVertex {
  vec4 gl_Position;
  float gl_PointSize;
  float gl_ClipDistance[];
};

layout( location = 0 ) out vec2 vs_textureCoordinate;
layout( location = 1 ) flat out int imageno;
layout( location = 2 ) out int instance;
const float amplitude = 0;
const float repeats = 0;

void main(void)
{
    vs_textureCoordinate = vec2(gl_TessCoord.x,1.0-gl_TessCoord.y);         //1.0-y is due to the project model turning Y upside down so Y points upwards on screen

    vec4 p1 = mix(gl_in[0].gl_Position, gl_in[1].gl_Position, gl_TessCoord.x);  
    vec4 p2 = mix(gl_in[2].gl_Position, gl_in[3].gl_Position, gl_TessCoord.x); 
    vec4 pos = mix(p1, p2, gl_TessCoord.y);                                     

    pos.y += amplitude*sin((phase+gl_TessCoord.x)*PI*2*repeats);           // .x goes 0-1, phase goes 0-1, convert to radians

    imageno = int(tcs_worldposinstance[0].w);

    pos += vec4(tcs_worldposinstance[0].xyz,0);     // shift by instance pos
    
    gl_Position = mc.ProjectionModelMatrix * pos;

    instance = tcs_instance[0];
}";
        }

        public float Phase { get; set; } = 0;                   // set to animate.  Phase is 0-1

        public GLPLTesselationEvaluateSinewave(float amplitude, float repeats)
        {
            CompileLink(ShaderType.TessEvaluationShader, TES(), constvalues: new object[] { "amplitude", amplitude, "repeats", repeats });
        }

        public override void Start()
        {
            base.Start();
            GL.ProgramUniform1(Id, 26, Phase);
            OpenTKUtils.GLStatics.Check();
        }

    }

}
