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
    // point sprite shader based on eye position vs sprite position.  Needs point sprite on and program point size

    public class GLPLStarSurfaceFragmentShader : GLShaderPipelineShadersBase
    {
        public string Fragment()
        {
            return
@"
#version 450 core
layout (location = 1) in vec3 modelpos;
out vec4 color;

layout (location = 10) uniform float frequency;
layout (location = 11) uniform float unRadius;      // km
layout (location = 12) uniform float s;
layout (location = 13) uniform float blackdeepness;
layout (location = 14) uniform float concentrationequator;
layout (location = 15) uniform float unDTsurface;
layout (location = 16) uniform float unDTspots;

#include Shaders.Functions.snoise3.glsl

void main(void)
{
    vec3 position = normalize(modelpos);        // normalise model vectors

    float theta = dot(vec3(0,1,0),position);    // dotp between cur pos and up -1 to +1, 0 at equator
    theta = abs(theta);                         // uniform around equator.

    float clip = s + (theta/concentrationequator);               // clip sets the pass criteria to do the sunspots
    vec3 sPosition = (position + unDTspots) * unRadius;
    float t1 = simplexnoise(sPosition * frequency) -clip;
    float t2 = simplexnoise((sPosition + unRadius) * frequency) -clip;
	float ss = (max(t1, 0.0) * max(t2, 0.0)) * blackdeepness;

    vec3 p1 = vec3(position.x+unDTsurface,position.y,position.z);   // moving the noise across x produces a more realistic look
    float n = (simplexnoise(p1, 4, 40.0, 0.7) + 1.0) * 0.5;      // noise of surface..

    vec3 baseColor = vec3(0.9, 0.9 ,0.0);
    baseColor = baseColor - ss - n/4;
    color = vec4(baseColor, 1.0);
}
";
        }

        // applied each start, change to make spots move
        public float TimeDeltaSurface { get; set; } = 0;
        public float TimeDeltaSpots { get; set; } = 0;

        // applied once unless UpdateControls set
        public bool UpdateControls { get; set; } = true;        // set to update below
        public float Frequency { get; set; } = 0.00005f;     // higher, more but small
        public float UnRadius { get; set; } = 200000;        // lower, more diffused
        public float Scutoff { get; set; } = 0.5f;           // bar to pass, lower more, higher lots 0.4 lots, 0.6 few
        public float Blackdeepness { get; set; } = 8;        // how dark is each spot
        public float Concentrationequator { get; set; } = 4; // how spread out

        public GLPLStarSurfaceFragmentShader()
        {
            CompileLink(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, Fragment(), auxname: GetType().Name);
        }

        public override void Start()
        {
            if ( UpdateControls )
            {
                GL.ProgramUniform1(Id, 10, Frequency);
                GL.ProgramUniform1(Id, 11, UnRadius);
                GL.ProgramUniform1(Id, 12, Scutoff);
                GL.ProgramUniform1(Id, 13, Blackdeepness);
                GL.ProgramUniform1(Id, 14, Concentrationequator);
                UpdateControls = false;
            }

            GL.ProgramUniform1(Id, 15, TimeDeltaSurface);
            GL.ProgramUniform1(Id, 16, TimeDeltaSpots);

            OpenTKUtils.GLStatics.Check();
        }
    }
}

