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
    // List to hold named renderables and a Render function to send the lot to GL - issued in Add order

    public class GLRenderList : IDisposable
    {
        private Dictionary<string, GLRenderable> renderables;
        private int unnamed = 0;

        public GLRenderList()
        {
            renderables = new Dictionary<string, GLRenderable>();
        }

        public void Add(string name, GLRenderable r)
        {
            renderables.Add(name, r);
        }

        public void Add(GLRenderable r)
        {
            renderables.Add("Unnamed_" + (unnamed++), r);
        }

        public GLRenderable this[string key] { get { return renderables[key]; } }
        public bool Contains(string key) { return renderables.ContainsKey(key); }

        public void Render(Matrix4 model, Matrix4 proj)
        {
            GLRenderable last = null;

            foreach (GLRenderable r in renderables.Values)
            {
                if (last == null || last.PId != r.PId)
                {
                    if (last != null)
                        r.FinishProgram();

                    r.StartProgram(model, proj);
                    last = r;
                }

                r.BindRender();
            }

            if (last != null)
                last.FinishProgram();

            GL.UseProgram(0);
            GL.BindProgramPipeline(0);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (GLRenderable r in renderables.Values)
                    r.Dispose();

                renderables.Clear();
            }
        }
    }


    // List to hold named renderables and a Render function to send the lot to GL - issued in Program ID order, then Add order

    public class GLRenderProgramSortedList : IDisposable
    {
        private BaseUtils.DictionaryOfDictionaries<int, string, GLRenderable> renderables;
        private int unnamed = 0;

        public GLRenderProgramSortedList()
        {
            renderables = new BaseUtils.DictionaryOfDictionaries<int, string, GLRenderable>();
        }

        public void Add(string name, GLRenderable r)
        {
            renderables.Add(r.PId, name, r);
        }

        public void Add(GLRenderable r)
        {
            Add("Unnamed_" + (unnamed++), r);
        }

        public GLRenderable this[string key] {  get { return renderables[key]; } }

        public bool Contains(string key) { return renderables.ContainsKey(key); }

        public void Render(Matrix4 model, Matrix4 proj)
        {
            foreach (var d in renderables)
            {
                GLRenderable last = null;

                foreach (GLRenderable g in d.Value.Values)
                {
                    if (last == null)
                    {
                        last = g;
                        g.StartProgram(model, proj);
                    }

                    g.BindRender();
                }

                if (last != null)
                    last.FinishProgram();
            }

            GL.UseProgram(0);           // final clean up
            GL.BindProgramPipeline(0);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var d in renderables)
                {
                    foreach (GLRenderable g in d.Value.Values)
                        g.Dispose();
                }

                renderables.Clear();
            }
        }
    }
}

