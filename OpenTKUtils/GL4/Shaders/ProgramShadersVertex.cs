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

    public class GLVertexShaderTranslation: IGLPipelineShaders
    {
        private GLProgram program;
        public int Id { get { return program.Id; } }
        static string vertextx =
@"
#version 450 core
layout (location = 0) in vec4 position;
layout(location = 1) in vec4 color;

out vec4 vs_color;

layout (location = 20) uniform  mat4 projection;
layout (location = 21) uniform  mat4 modelView;
layout (location = 22) uniform  mat4 transform;

out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

void main(void)
{
	gl_Position = projection * modelView * transform * position;        // order important
	vs_color = color;                                                   // pass to fragment shader
}
";
        // with transform, object needs to pass in uniform 22 the transform

        public GLVertexShaderTranslation()       // seperable note - you need a pipeline
        {
            program = new OpenTKUtils.GL4.GLProgram();
            string ret = program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, vertextx);
            System.Diagnostics.Debug.Assert(ret == null, ret);
            ret = program.Link(separable: true);
            System.Diagnostics.Debug.Assert(ret == null, ret);
        }

        public void Start(Matrix4 model, Matrix4 projection ) // seperable do not use a program - that is for the pipeline to hook up
        {
            GL.UniformMatrix4(20, false, ref projection);
            GL.UniformMatrix4(21, false, ref model);        // pass in uniform var the model matrix
        }

        public void Finish()
        {
        }

        public void Dispose()
        {
            program.Dispose();
        }
    }

    // Simple rendered with optional rot/translation

    public class GLVertexShaderStars : IGLPipelineShaders
    {
        private GLProgram program;
        public int Id { get { return program.Id; } }
        static string vertextx =
@"
#version 450 core
layout (location = 0) in vec4 position;

out vec4 vs_color;

layout (location = 20) uniform  mat4 projection;
layout (location = 21) uniform  mat4 modelView;

out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

void main(void)
{
	gl_Position = projection * modelView *  position;        // order important
    gl_PointSize = (gl_VertexID+5)%10;
	vs_color = vec4((gl_VertexID%5)*0.1+0.5,(gl_VertexID%5)*0.1+0.5,0.5,1.0);                                   // pass to fragment shader
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
            GL.UniformMatrix4(20, false, ref projection);
            GL.UniformMatrix4(21, false, ref model);        // pass in uniform var the model matrix
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

}

