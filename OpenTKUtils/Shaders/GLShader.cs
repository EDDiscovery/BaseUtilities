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
    public class GLShader : IDisposable
    {
        public int Id { get; private set; }
        public bool Compiled { get { return Id != -1; } }
        ShaderType type;

        public GLShader( ShaderType t , string source = null )
        {
            Id = -1;
            type = t;
            if (source != null)
                Compile(source);
        }

        public string Compile(string source)                // string return gives any errors
        {
            Id = GL.CreateShader(type);
            GL.ShaderSource(Id, source);
            GL.CompileShader(Id);
            var info = GL.GetShaderInfoLog(Id);

            if (info.HasChars())
            {
                GL.DeleteShader(Id);
                Id = -1;
                return info;
            }
            return null;
        }

        public void Dispose()
        {
            if (Id != -1)
                GL.DeleteShader(Id);
        }
    }
}
