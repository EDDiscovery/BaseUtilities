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
using System.Collections.Generic;

namespace OpenTKUtils.GL4
{
    // Pipeline of shaders - inherit from this and install your shared shaders..

    public class GLShaderPipelineBase : IGLProgramShader
    {
        public int Id { get { return pipelineid + 100000; } }            // to avoid clash with standard ProgramIDs, use an offset for pipeline IDs
        public Action<IGLProgramShader> StartAction { get; set; }

        public IGLShader Get(ShaderType t) { return programs[t]; }

        private int pipelineid;
        private Dictionary<ShaderType, IGLShader> programs;

        public GLShaderPipelineBase()
        {
            pipelineid = GL.GenProgramPipeline();
            programs = new Dictionary<ShaderType, IGLShader>();
        }

        public GLShaderPipelineBase(Action<IGLProgramShader> sa) : this()
        {
            StartAction = sa;
        }

        public GLShaderPipelineBase(IGLShader vertex, IGLShader fragment) : this()
        {
            AddVertex(vertex);
            AddFragment(fragment);
        }

        public void Add(IGLShader p, ShaderType m)
        {
            programs[m] = p;
            GL.UseProgramStages(pipelineid, convmask[m], p.Id);
        }
        public void AddVertex(IGLShader p)
        {
            Add(p, ShaderType.VertexShader);
        }
        public void AddFragment(IGLShader p)
        {
            Add(p, ShaderType.FragmentShader);
        }
        public void AddVertexFragment(IGLShader vertex, IGLShader fragment)
        {
            AddVertex(vertex);
            AddFragment(fragment);
        }

        public virtual void Start(Common.MatrixCalc c)
        {
            GL.UseProgram(0);           // ensure no active program - otherwise the stupid thing picks it
            GL.BindProgramPipeline(pipelineid);
        
            //System.Diagnostics.Debug.WriteLine("Pipeline " + pipelineid);

            foreach (var x in programs)                             // let any programs do any special set up
                x.Value.Start(c);

            StartAction?.Invoke(this);                           // any shader hooks get a chance.
        }

        public virtual void Finish()                                        // and clean up afterwards
        {
            foreach (var x in programs)
                x.Value.Finish();

            GL.BindProgramPipeline(0);
            //System.Diagnostics.Debug.WriteLine("Pipeline " + pipelineid + " Released");
        }

        public void Dispose()
        {
            foreach (var x in programs)
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
