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
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    public class Shader : IDisposable
    {
        public int Id { get; private set; }
        public bool Compiled { get { return Id != -1; } }
        public string CompileReport { get; private set; }       // null if okay, else error
        ShaderType type;

        public Shader( ShaderType t , string source = null )
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
            CompileReport = GL.GetShaderInfoLog(Id);

            if (CompileReport.HasChars())
            {
                GL.DeleteShader(Id);
                Id = -1;
                return CompileReport;
            }

            CompileReport = null;
            return null;
        }

        public void Dispose()           // you can double dispose.
        {
            if (Id != -1)
            {
                GL.DeleteShader(Id);
                Id = -1;
            }
        }
    }
}
