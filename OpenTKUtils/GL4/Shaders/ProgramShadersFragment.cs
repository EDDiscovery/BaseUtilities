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
    // Fragment, requires vs_color
    public class GLFragmentShaderSimple : IGLPipelineShaders
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
        public GLFragmentShaderSimple()                 // seperable note - you need a pipeline
        {
            program = new OpenTKUtils.GL4.GLProgram();
            string ret = program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, fragmentsolid);
            System.Diagnostics.Debug.Assert(ret == null, ret);
            ret = program.Link(separable: true);
            System.Diagnostics.Debug.Assert(ret == null, ret);
        }

        public void Start(Matrix4 model, Matrix4 projection)
        {
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
