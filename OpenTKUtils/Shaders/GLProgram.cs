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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils
{
    public class GLProgram : IDisposable
    {
        public int Id { get; private set; }
        public bool Created { get { return Id != -1; } }

        private List<GLShader> shaders;

        public GLProgram()
        {
            Id = GL.CreateProgram();
            shaders = new List<GLShader>();
        }

        public void Add( GLShader s)
        {
            System.Diagnostics.Debug.Assert(s.Compiled);
            shaders.Add(s);
        }

        public string Link(params GLShader[] sh)
        {
            foreach( var s in sh )
            {
                if (s.Compiled)
                    shaders.Add(s);
                else
                    return "Shader not compiled";
            }
            return Link();
        }

        public string Link()
        {
            if (shaders.Count == 0)
                return "No shaders attached";

            foreach (GLShader s in shaders)
                GL.AttachShader(Id, s.Id);

            GL.LinkProgram(Id);
            var info = GL.GetProgramInfoLog(Id);

            foreach (GLShader s in shaders)
                GL.DetachShader(Id, s.Id);

            return info.HasChars() ? info : null;
        }

        public void Use()
        {
            GL.UseProgram(Id);
        }

        public void Dispose()
        {
            if ( Id != -1 )
                GL.DeleteProgram(Id);
        }

    }
}
