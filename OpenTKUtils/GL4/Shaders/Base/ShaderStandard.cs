/*
 * Copyright © 2019 Robbyxp1 @ github.com
 * Part of the EDDiscovery Project
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
using OpenTK.Graphics.OpenGL4;
using OpenTKUtils.Common;

namespace OpenTKUtils.GL4
{
    // inherit from this is you have a shader which items makes its own set of vertext/fragment shaders all in one go, non pipelined
    // A program shader has a Start(), called by RenderableList when the shader is started
    // StartAction, an optional hook to supply more start functionality
    // A Finish() to clean up
    // FinishAction, an optional hook to supply more finish functionality

    // you can use the CompileAndLink() function to quickly compile and link multiple shaders

    public abstract class GLShaderStandard : IGLProgramShader
    {
        public int Id { get { return program.Id; } }
        public IGLShader Get(ShaderType t) { return this; }
        public Action<IGLProgramShader> StartAction { get; set; }
        public Action<IGLProgramShader> FinishAction { get; set; }

        protected GLProgram program;

        public GLShaderStandard()
        {
        }

        public GLShaderStandard(Action<IGLProgramShader> sa) : this()
        {
            StartAction = sa;
        }

        public GLShaderStandard(Action<IGLProgramShader> sa, Action<IGLProgramShader> fa) : this()
        {
            StartAction = sa;
            FinishAction = fa;
        }

        // Compile/link various shaders
        // for varyings, you must set up a start action of Gl.BindBuffer(GL.TRANSFORM_FEEDBACK_BUFFER,bufid) AND BeingTransformFeedback.

        public void CompileLink( string vertex=null, string tcs=null, string tes=null, string geo=null, string frag=null, string[] varyings = null , 
                                    TransformFeedbackMode varymode = TransformFeedbackMode.InterleavedAttribs )
        {
            program = new OpenTKUtils.GL4.GLProgram();
            string ret;

            if (vertex != null)
            {
                ret = program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, vertex);
                System.Diagnostics.Debug.Assert(ret == null, "Vertex Shader", ret);
            }

            if (tcs != null)
            {
                ret = program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.TessControlShader, tcs);
                System.Diagnostics.Debug.Assert(ret == null, "Tesselation Control Shader", ret);
            }

            if (tes != null)
            {
                ret = program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.TessEvaluationShader, tes);
                System.Diagnostics.Debug.Assert(ret == null, "Tesselation Evaluation Shader", ret);
            }

            if (geo != null)
            {
                ret = program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.GeometryShader, geo);
                System.Diagnostics.Debug.Assert(ret == null, "Geometry shader", ret);
            }

            if (frag != null)
            {
                ret = program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, frag);
                System.Diagnostics.Debug.Assert(ret == null, "Fragment Shader", ret);
            }

            if ( varyings != null )
            {
                GL.TransformFeedbackVaryings(program.Id, varyings.Length, varyings, varymode);      // this indicate varyings.
            }

            ret = program.Link();
            System.Diagnostics.Debug.Assert(ret == null, "Link", ret);

            OpenTKUtils.GLStatics.Check();
        }

        public virtual void Start()     
        {
            GL.UseProgram(Id);
            StartAction?.Invoke(this);
        }

        public virtual void Finish()                 
        {
            FinishAction?.Invoke(this);                           // any shader hooks get a chance.
        }

        public virtual void Dispose()
        {
            program.Dispose();
        }

    }
}
