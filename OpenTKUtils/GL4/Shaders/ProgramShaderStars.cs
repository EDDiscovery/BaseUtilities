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

    public class GLVertexShaderStars : IGLPipelineShaders
    {
        private GLProgram program;
        public int Id { get { return program.Id; } }
        static string vertextx =
@"
#version 450 core
layout (location = 0) in uvec2 positionpacked;

out vec4 vs_color;

layout (location = 20) uniform  mat4 projection;
layout (location = 21) uniform  mat4 modelView;
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

	gl_Position = projection * modelView * position;        // order important

    float distance = 50-pow(distance(eyepos,vec3(x,y,z)),2)/20;

    gl_PointSize = clamp(distance,1.0,63.0);
    vs_color = vec4(rand1(gl_VertexID),0.5,0.5,1.0);
}
";
        // with transform, object needs to pass in uniform 22 the transform

        public GLVertexShaderStars()       // seperable note - you need a pipeline
        {
            program = new OpenTKUtils.GL4.GLProgram();
            string ret = program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, vertextx);
            System.Diagnostics.Debug.Assert(ret == null, ret);
            ret = program.Link(separable: true);
            System.Diagnostics.Debug.Assert(ret == null, ret);
        }

        public void Start(Matrix4 model, Matrix4 projection) // seperable do not use a program - that is for the pipeline to hook up
        {
            System.Diagnostics.Debug.WriteLine("Shader model view");
            GL.ProgramUniformMatrix4(program.Id, 20, false, ref projection);
            GL.ProgramUniformMatrix4(program.Id, 21, false, ref model);
            GL.Enable(EnableCap.ProgramPointSize);
        }

        public void Finish()
        {
            GL.Disable(EnableCap.ProgramPointSize);
        }

        public void Dispose()
        {
            program.Dispose();
        }
    }

    public class GLObjectDataEyePosition : IGLObjectInstanceData
    {
        public const int TRUniformId = 23;      // Standard used to pass eye pos to shader

        public GLObjectDataEyePosition()
        {
            eyepos = Vector3.Zero;
        }

        public void Set(Vector3 pos)
        {
            eyepos = pos;
        }

        private Vector3 eyepos;

        public void Bind()
        {
            GL.Uniform3(TRUniformId, ref eyepos);
        }
    }

}
