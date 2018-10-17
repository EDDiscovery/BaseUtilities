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
    // Pipeline of shaders

    public class GLProgramShaderPipeline : IGLProgramShaders
    {
        private List<IGLPipelineShaders> programs;                      // Program 0 is always vertex
        public int Id { get { return pipelineid + 100000; } }            // to avoid clash with standard ProgramIDs, use an offset for pipeline IDs

        private int pipelineid;

        public GLProgramShaderPipeline()
        {
            pipelineid = GL.GenProgramPipeline();
            programs = new List<IGLPipelineShaders>();
        }

        public GLProgramShaderPipeline(IGLPipelineShaders vertex, IGLPipelineShaders fragment) : this()
        {
            AddVertex(vertex);
            AddFragment(fragment);
        }

        public void Add(IGLPipelineShaders p, ProgramStageMask m)
        {
            if (m == ProgramStageMask.VertexShaderBit)          // vertex goes to entry 0 so we know which active program to set
                programs.Insert(0, p);
            else
                programs.Add(p);

            GL.UseProgramStages(pipelineid, m, p.Id);
        }
        public void AddVertex(IGLPipelineShaders p)
        {
            Add(p, ProgramStageMask.VertexShaderBit);
        }
        public void AddFragment(IGLPipelineShaders p)
        {
            Add(p, ProgramStageMask.FragmentShaderBit);
        }
        public void AddVertexFragment(IGLPipelineShaders vertex, IGLPipelineShaders fragment)
        {
            AddVertex(vertex);
            AddFragment(fragment);
        }

        public void Start(Matrix4 model, Matrix4 projection)
        {
            GL.UseProgram(0);           // ensure no active program - otherwise the stupid thing picks it
            GL.BindProgramPipeline(pipelineid);     
            GL.ActiveShaderProgram(pipelineid, programs[0].Id);     // tell uniforms to target this one - otherwise uniforms don't go in.
                                                                    // we always target the vertex program (took a while to find this bit)
            System.Diagnostics.Debug.WriteLine("Pipeline " + pipelineid);

            foreach (var x in programs)                             // let any programs do any special set up
                x.Start(model, projection);
        }

        public void Finish()                                        // and clean up afterwards
        {
            foreach (var x in programs)
                x.Finish();

            GL.BindProgramPipeline(0);
            System.Diagnostics.Debug.WriteLine("Pipeline " + pipelineid + " Released");
        }

        public void Dispose()
        {
            foreach (var x in programs)
                x.Dispose();

            GL.DeleteProgramPipeline(pipelineid);
        }
    }

}
