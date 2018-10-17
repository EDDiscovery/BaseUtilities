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

    public class GLVertexColourObjectShaderSimple : IGLProgramShaders
    {
        private GLProgram program;
        public int Id { get { return program.Id; } }

        static string fragmentsolid =
@"
#version 450 core
in vec4 vs_color;
out vec4 color;

void main(void)
{
	color = vs_color;
}
";

static string vertexnotx =
@"
#version 450 core
layout (location = 0) in vec4 position;
layout(location = 1) in vec4 color;

out vec4 vs_color;

layout (location = 20) uniform  mat4 projection;
layout (location = 21) uniform  mat4 modelView;

void main(void)
{
	gl_Position = projection * modelView * position;        // order important
	vs_color = color;
}
";

static string vertextx =
@"
#version 450 core
layout (location = 0) in vec4 position;
layout(location = 1) in vec4 color;

out vec4 vs_color;

layout (location = 20) uniform  mat4 projection;
layout (location = 21) uniform  mat4 modelView;
layout (location = 22) uniform  mat4 transform;

void main(void)
{
	gl_Position = projection * modelView * transform * position;        // order important
	vs_color = color;
}
";

        public GLVertexColourObjectShaderSimple(bool withtransform)     // if with transform, object needs to pass in uniform 22 the transform
        {
            program = new OpenTKUtils.GL4.GLProgram();
            program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, withtransform ? vertextx : vertexnotx);
            program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, fragmentsolid);
            string ret = program.Link();
            System.Diagnostics.Debug.Assert(ret == null);
        }

        public void Start(Matrix4 model, Matrix4 projection)
        {
            program.Use();
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
}
