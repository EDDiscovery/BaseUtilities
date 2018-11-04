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

    public abstract class GLShaderPipelineVertexBase : IGLShader
    {
        public int Id { get { return program.Id; } }

        public abstract string Code();

        private GLProgram program;

        public void CompileLink()     
        {
            program = new OpenTKUtils.GL4.GLProgram();
            string ret = program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, Code());
            System.Diagnostics.Debug.Assert(ret == null, ret);
            ret = program.Link(separable: true);
            System.Diagnostics.Debug.Assert(ret == null, ret);
        }

        public virtual void Start(Common.MatrixCalc c)
        {
            Matrix4 projmodel = c.ProjectionModelMatrix;
            GL.ProgramUniformMatrix4(Id, 20, false, ref projmodel);
        }

        public virtual void Finish() { }

        public void Dispose()
        {
            program.Dispose();
        }
    }

}

