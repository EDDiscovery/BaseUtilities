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

    public class GLProgramShaderPipeline : IGLProgramShaders
    {
        public int Id { get { return pipelineid + 100000; } }            // to avoid clash with standard ProgramIDs, use an offset for pipeline IDs

        public IGLProgramShaders GetVertex() { return programs[ProgramStageMask.VertexShaderBit]; }
        public IGLProgramShaders GetFragment() { return programs[ProgramStageMask.FragmentShaderBit]; }

        private int pipelineid;
        private Dictionary<ProgramStageMask, IGLProgramShaders> programs;

        public GLProgramShaderPipeline()
        {
            pipelineid = GL.GenProgramPipeline();
            programs = new Dictionary<ProgramStageMask, IGLProgramShaders>();
        }

        public GLProgramShaderPipeline(IGLProgramShaders vertex, IGLProgramShaders fragment) : this()
        {
            AddVertex(vertex);
            AddFragment(fragment);
        }

        public void Add(IGLProgramShaders p, ProgramStageMask m)
        {
            programs[m] = p;
            GL.UseProgramStages(pipelineid, m, p.Id);
        }
        public void AddVertex(IGLProgramShaders p)
        {
            Add(p, ProgramStageMask.VertexShaderBit);
        }
        public void AddFragment(IGLProgramShaders p)
        {
            Add(p, ProgramStageMask.FragmentShaderBit);
        }
        public void AddVertexFragment(IGLProgramShaders vertex, IGLProgramShaders fragment)
        {
            AddVertex(vertex);
            AddFragment(fragment);
        }

        public virtual void Start(Common.MatrixCalc c)
        {
            GL.UseProgram(0);           // ensure no active program - otherwise the stupid thing picks it
            GL.BindProgramPipeline(pipelineid);
        
            // remove for now - everyone targets uniforms at programs..
            //    GL.ActiveShaderProgram(pipelineid, GetVertex().Id);     // tell uniforms to target this one - otherwise uniforms don't go in.

            System.Diagnostics.Debug.WriteLine("Pipeline " + pipelineid);

            foreach (var x in programs)                             // let any programs do any special set up
                x.Value.Start(c);
        }

        public virtual void Finish()                                        // and clean up afterwards
        {
            foreach (var x in programs)
                x.Value.Finish();

            GL.BindProgramPipeline(0);
            System.Diagnostics.Debug.WriteLine("Pipeline " + pipelineid + " Released");
        }

        public void Dispose()
        {
            foreach (var x in programs)
                x.Value.Dispose();

            GL.DeleteProgramPipeline(pipelineid);
        }
    }

}
