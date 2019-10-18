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

using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace OpenTKUtils.GL4
{
    // this is a render list, holding a list of Shader programs
    // each shader program is associated with one or more RenderableItems 
    // This Start() each program, goes thru the render list Binding and Rendering each item
    // then it Finish() the program

    public class GLRenderProgramSortedList 
    {
        private BaseUtils.DictionaryOfDictionaries<IGLProgramShader, string, IGLRenderableItem> renderables;
        private int unnamed = 0;

        public GLRenderProgramSortedList()
        {
            renderables = new BaseUtils.DictionaryOfDictionaries<IGLProgramShader, string, IGLRenderableItem>();
        }

        public void Add(IGLProgramShader prog, string name, IGLRenderableItem r)
        {
            renderables.Add(prog, name, r);
        }

        public void Add(IGLProgramShader prog, IGLRenderableItem r)
        {
            Add(prog, "Unnamed_" + (unnamed++), r);
        }

        public IGLRenderableItem this[string key] {  get { return renderables[key]; } }

        public bool Contains(string key) { return renderables.ContainsKey(key); }

        public void Render(Common.MatrixCalc c)
        {
            foreach (var d in renderables)
            {
                d.Key.Start();       // start the program

                foreach (IGLRenderableItem g in d.Value.Values)
                {
                    g.Bind(d.Key,c);
                    g.Render();
                }

                d.Key.Finish();
            }

            GL.UseProgram(0);           // final clean up
            GL.BindProgramPipeline(0);
        }
    }
}

