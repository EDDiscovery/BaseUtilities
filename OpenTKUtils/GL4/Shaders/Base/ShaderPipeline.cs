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


using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

namespace OpenTKUtils.GL4
{
    // inherit from this to make a shader which uses a set of pipeline shaders (from ShaderPipelineShadersBase) to make up your whole shader

    // A pipeline shader has a Start(), called by RenderableList when the shader is started
    //  this calls all the Start() in each ShaderPipelineShaderBase
    // StartAction, an optional hook to supply more start functionality
    // A Finish() to clean up
    //  this calls all the Finish() in each ShaderPipelineShaderBase
    // FinishAction, an optional hook to supply more finish functionality

    public class GLShaderPipeline : IGLProgramShader
    {
        public int Id { get { return pipelineid + 100000; } }            // to avoid clash with standard ProgramIDs, use an offset for pipeline IDs
        public Action<IGLProgramShader> StartAction { get; set; }
        public Action<IGLProgramShader> FinishAction { get; set; }

        public IGLShader Get(ShaderType t) { return shaders[t]; }

        private int pipelineid;
        private Dictionary<ShaderType, IGLPipelineShader> shaders;

        public GLShaderPipeline()
        {
            pipelineid = GL.GenProgramPipeline();
            shaders = new Dictionary<ShaderType, IGLPipelineShader>();
        }

        public GLShaderPipeline(Action<IGLProgramShader> sa, Action<IGLProgramShader> fa = null) : this()
        {
            StartAction = sa;
            FinishAction = fa;
        }

        public GLShaderPipeline(IGLPipelineShader vertex, Action<IGLProgramShader> sa = null, Action<IGLProgramShader> fa = null) : this()
        {
            AddVertex(vertex);
            StartAction = sa;
            FinishAction = fa;
        }

        public GLShaderPipeline(IGLPipelineShader vertex, IGLPipelineShader fragment, Action<IGLProgramShader> sa = null, Action<IGLProgramShader> fa = null) : this()
        {
            AddVertexFragment(vertex, fragment);
            StartAction = sa;
            FinishAction = fa;
        }

        public void AddVertex(IGLPipelineShader p)
        {
            Add(p, ShaderType.VertexShader);
        }

        public void AddVertexFragment(IGLPipelineShader p, IGLPipelineShader f)
        {
            Add(p, ShaderType.VertexShader);
            Add(f, ShaderType.FragmentShader);
        }

        public void Add(IGLPipelineShader p, ShaderType m)
        {
            shaders[m] = p;
            GL.UseProgramStages(pipelineid, convmask[m], p.Id);
        }

        public virtual void Start()
        {
            GL.UseProgram(0);           // ensure no active program - otherwise the stupid thing picks it
            GL.BindProgramPipeline(pipelineid);

            foreach (var x in shaders)                             // let any programs do any special set up
                x.Value.Start();

            StartAction?.Invoke(this);                           // any shader hooks get a chance.
        }

        public virtual void Finish()                                        // and clean up afterwards
        {
            foreach (var x in shaders)
                x.Value.Finish();

            FinishAction?.Invoke(this);                           // any shader hooks get a chance.

            GL.BindProgramPipeline(0);
        }

        public void Dispose()
        {
            foreach (var x in shaders)
                x.Value.Dispose();

            GL.DeleteProgramPipeline(pipelineid);
        }

        static Dictionary<ShaderType, ProgramStageMask> convmask = new Dictionary<ShaderType, ProgramStageMask>()
        {
            { ShaderType.FragmentShader, ProgramStageMask.FragmentShaderBit },
            { ShaderType.VertexShader, ProgramStageMask.VertexShaderBit },
            { ShaderType.TessControlShader, ProgramStageMask.TessControlShaderBit },
            { ShaderType.TessEvaluationShader, ProgramStageMask.TessEvaluationShaderBit },
            { ShaderType.GeometryShader, ProgramStageMask.GeometryShaderBit},
            { ShaderType.ComputeShader, ProgramStageMask.ComputeShaderBit },
        };


    }
}
