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

        protected int VertexArray;                  // the vertex GL Array 
        protected int VertexBuffer;                 // its buffer data
        protected int VerticeCount;                 // and size...

        private PrimitiveType primitivetype;        // Draw type

        protected IGLProgramShaders iglprogramshader;        // program to use on this
        protected IGLObjectInstanceData instancedata;   // any instance data

        public int PId { get { return iglprogramshader.Id; } }   // the program ID

        protected GLRenderable(int vertexCount, IGLProgramShaders pn, IGLObjectInstanceData id, PrimitiveType pt)
        {
            iglprogramshader = pn;
            instancedata = id;
            VerticeCount = vertexCount;
            VertexArray = GL.GenVertexArray();
            VertexBuffer = GL.GenBuffer();
            primitivetype = pt;

            GL.BindVertexArray(VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
        }

        public virtual void Bind()                  // override if you need to bind other stuff
        {
            GL.BindVertexArray(VertexArray);        // Bind vertex
            instancedata?.Bind();                   // offer any instance data bind opportunity
        }

        public void StartProgram(Matrix4 model, Matrix4 proj)      // Called when the 
        {
            System.Diagnostics.Debug.WriteLine("Use program " + iglprogramshader.Id);
            iglprogramshader.Start(model, proj);
        }

        public void FinishProgram()      // override if you need to bind other stuff
        {
            System.Diagnostics.Debug.WriteLine("Finish program " + iglprogramshader.Id);
            iglprogramshader.Finish();
        }

        public void BindRender()      // override if you need to bind other stuff
        {
            System.Diagnostics.Debug.WriteLine("Draw " + VerticeCount + " using " + primitivetype + " under " + iglprogramshader.Id);
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
}

