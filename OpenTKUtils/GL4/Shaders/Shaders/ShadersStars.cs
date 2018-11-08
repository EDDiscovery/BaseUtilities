/*
 * Copyright © 2015 - 2018 EDDiscovery development team
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

namespace OpenTKUtils.GL4
{
    // Simple rendered with optional rot/translation

    public class GLShaderStars : GLShaderPipelineShadersBase
    {
        public string Code()
        {
            return
@"
#version 450 core
layout (location = 0) in uvec2 positionpacked;

out vec4 vs_color;

layout (location = 20) uniform  mat4 projectionmodel;
layout (location = 23) uniform  vec3 eyepos;

out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };


float rand1(float n)
{
return fract(sin(n) * 43758.5453123);
}

void main(void)
{
    uint xcoord = positionpacked.x & 0x1fffff;
    uint ycoord = positionpacked.y & 0x1fffff;
    float x = float(xcoord)/16.0-50000;
    float y = float(ycoord)/16.0-50000;
    uint zcoord = positionpacked.x >> 21;
    zcoord = zcoord | ( ((positionpacked.y >> 21) & 0x7ff) << 11);
    float z = float(zcoord)/16.0-50000;

    vec4 position = vec4( x, y, z, 1.0f);

	gl_Position = projectionmodel * position;        // order important

    float distance = 50-pow(distance(eyepos,vec3(x,y,z)),2)/20;

    gl_PointSize = clamp(distance,1.0,63.0);
    vs_color = vec4(rand1(gl_VertexID),0.5,0.5,1.0);
}
";
        }

        public GLShaderStars()
        {
            Program = GLProgram.CompileLink(ShaderType.VertexShader, Code(), GetType().Name);
            SetupProjMatrix = true;
        }

        public override void Start(Common.MatrixCalc c) // seperable do not use a program - that is for the pipeline to hook up
        {
            base.Start(c);
            GL.ProgramUniform3(Id, 23, c.EyePosition);
            GL.Enable(EnableCap.ProgramPointSize);
        }

        public override void Finish() // seperable do not use a program - that is for the pipeline to hook up
        {
            base.Finish();
            GL.Disable(EnableCap.ProgramPointSize);
        }

    }

}
