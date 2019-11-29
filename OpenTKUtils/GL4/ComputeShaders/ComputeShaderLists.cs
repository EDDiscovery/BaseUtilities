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

using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

namespace OpenTKUtils.GL4
{

    // this is a compute list, holding a list of Shader programs which are compute shaders only
    // This Start() each program, goes thru the render list Binding and Rendering each item
    // then it Finish() the program

    public class GLComputeShaderList
    {
        private List<GLShaderCompute> computeshaders;

        public GLComputeShaderList()
        {
            computeshaders = new List<GLShaderCompute>();
        }

        public void Add(GLShaderCompute prog)
        {
            computeshaders.Add(prog);
        }

        public void Run()
        {
            foreach (var d in computeshaders)
                d.Run();    // start the program and dispatch it, finish it

            GL.UseProgram(0);           // final clean up
            GL.BindProgramPipeline(0);
        }
    }
}

