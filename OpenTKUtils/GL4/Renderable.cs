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
    // All renderables inherit from this class..
    // and override bind if required.

    public abstract class GLRenderable : IDisposable
    {
        public IGLObjectInstanceData InstanceData { get { return instancedata; } }

        protected int VertexArray;
        protected int VertexBuffer;
        protected int VerticeCount;

        private PrimitiveType primitivetype;

        protected IGLProgramShaders program;
        protected IGLObjectInstanceData instancedata;

        public int PId { get { return program.Id; } }

        protected GLRenderable(int vertexCount, IGLProgramShaders pn, IGLObjectInstanceData id, PrimitiveType pt)
        {
            program = pn;
            instancedata = id;
            VerticeCount = vertexCount;
            VertexArray = GL.GenVertexArray();
            VertexBuffer = GL.GenBuffer();
            primitivetype = pt;

            GL.BindVertexArray(VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
        }

        public virtual void Bind()      // override if you need to bind other stuff
        {
            GL.BindVertexArray(VertexArray);        // Bind vertex
            instancedata?.Bind();                   // offer any instance data bind opportunity
        }

        public void UseProgram(Matrix4 model, Matrix4 proj)      // override if you need to bind other stuff
        {
            program.Use(model, proj);
        }

        public void BindRender()      // override if you need to bind other stuff
        {
            System.Diagnostics.Debug.WriteLine("Draw " + VerticeCount + " using " + primitivetype + " under " + program.Id);
            Bind();
            GL.DrawArrays(primitivetype, 0, VerticeCount);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GL.DeleteVertexArray(VertexArray);
                GL.DeleteBuffer(VertexBuffer);
            }
        }
    }

    // List to hold named renderables and a Render function to send the lot to GL

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
        public bool Contains(string key ) { return renderables.ContainsKey(key); }

        public void Render(Matrix4 model, Matrix4 proj)
        {
            int pid = -1;

            foreach (GLRenderable r in renderables.Values)
            {
                if (pid == -1 || pid != r.PId)
                {
                    r.UseProgram(model, proj);
                    pid = r.PId;
                }

                r.BindRender();
            }

            GL.UseProgram(0);
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
}

