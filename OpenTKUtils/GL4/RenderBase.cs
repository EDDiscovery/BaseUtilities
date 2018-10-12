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
using System;

namespace OpenTKUtils.GL4
{
    public abstract class Renderable : IDisposable
    {
        protected int VertexArray;
        protected int VertexBuffer;
        protected int VerticeCount;

        protected Program program;
        private PrimitiveType primitivetype;

        protected Renderable(int vertexCount, Program pn, PrimitiveType pt)
        {
            program = pn;
            VerticeCount = vertexCount;
            VertexArray = GL.GenVertexArray();
            VertexBuffer = GL.GenBuffer();
            primitivetype = pt;

            GL.BindVertexArray(VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
        }

        public void BindRender()
        {
            Bind();             // purposely call Bind so if its overwritten..
            Render();       // purposely call Render so if its overwritten..
        }

        public virtual void Bind()
        {
            program.Use();
            GL.BindVertexArray(VertexArray);
        }

        public virtual void Render()
        {
            System.Diagnostics.Debug.WriteLine("Draw " + VerticeCount + " using " + primitivetype + " under " + program.Id);
            GL.DrawArrays(primitivetype, 0, VerticeCount);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
