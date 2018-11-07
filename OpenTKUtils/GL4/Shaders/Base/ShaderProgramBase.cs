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
using OpenTK.Graphics.OpenGL4;
using OpenTKUtils.Common;

namespace OpenTKUtils.GL4
{
    // Single program base.. inherit and use. Provides compile func.

    public abstract class GLShaderProgramBase : IGLProgramShader
    {
        public int Id { get { return program.Id; } }
        public IGLShader Get(ShaderType t) { return this; }
        public Action<IGLProgramShader> StartAction { get; set; }
        public Action<IGLProgramShader> FinishAction { get; set; }

        private GLProgram program;

        public GLShaderProgramBase()
        {
        }

        public GLShaderProgramBase(Action<IGLProgramShader> sa) : this()
        {
            StartAction = sa;
        }

        public void Compile( string vertex=null, string tcs=null, string tes=null, string geo=null, string frag=null )
        {
            program = new OpenTKUtils.GL4.GLProgram();
            string ret;

            if (vertex != null)
            {
                ret = program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, vertex);
                System.Diagnostics.Debug.Assert(ret == null, "Vertex", ret);
            }

            if (tcs != null)
            {
                ret = program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.TessControlShader, tcs);
                System.Diagnostics.Debug.Assert(ret == null, "TCS", ret);
            }

            if (tes != null)
            {
                ret = program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.TessEvaluationShader, tes);
                System.Diagnostics.Debug.Assert(ret == null, "TES", ret);
            }

            if (geo != null)
            {
                ret = program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.GeometryShader, geo);
                System.Diagnostics.Debug.Assert(ret == null, "GEO", ret);
            }

            if (frag != null)
            {
                ret = program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, frag);
                System.Diagnostics.Debug.Assert(ret == null, "Frag", ret);
            }

            ret = program.Link();
            System.Diagnostics.Debug.Assert(ret == null, "Link", ret);

            GLStatics.Check();
        }

        public virtual void Start(MatrixCalc c)     // override, but you must call these two
        {
            GL.UseProgram(Id);
            OpenTK.Matrix4 projmodel = c.ProjectionModelMatrix;
            GL.ProgramUniformMatrix4(Id, 20, false, ref projmodel);
            StartAction?.Invoke(this);
        }

        public virtual void Finish()                // override if required
        {
            FinishAction?.Invoke(this);                           // any shader hooks get a chance.
        }

        public virtual void Dispose()
        {
            program.Dispose();
        }

    }
}
