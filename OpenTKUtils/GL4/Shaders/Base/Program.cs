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
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
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

        public string Compile( ShaderType t, string code )
        {
            GLShader shader = new GLShader(t);
            string ret = shader.Compile(code);

            if (ret == null)
            {
                Add(shader);
                return null;
            }
            else
                return ret;
        }

        public string Link(params GLShader[] sh)        // done together, not seperable
        {
            foreach( var s in sh )
            {
                if (s.Compiled)
                    shaders.Add(s);
                else
                    return "Shader not compiled";
            }

            return Link(false);
        }

        public string Link( bool separable = false )            // link, seperable or not.  Disposes of shaders
        {
            if (shaders.Count == 0)
                return "No shaders attached";

            foreach (GLShader s in shaders)
                GL.AttachShader(Id, s.Id);

            if (separable)
                GL.ProgramParameter(Id, ProgramParameterName.ProgramSeparable, 1);

            GL.LinkProgram(Id);
            var info = GL.GetProgramInfoLog(Id);

            foreach (GLShader s in shaders)
            {
                GL.DetachShader(Id, s.Id);
                s.Dispose();
            }

            return info.HasChars() ? info : null;
        }

        public void Use()
        {
            GL.UseProgram(Id);
        }

        public void Dispose()               // you can double dispose
        {
            if (Id != -1)
            {
                GL.DeleteProgram(Id);
                Id = -1;
            }
        }

        static public GLProgram CompileLink(OpenTK.Graphics.OpenGL4.ShaderType st, string code, string auxname = "")
        {
            GLProgram program = new OpenTKUtils.GL4.GLProgram();
            string ret = program.Compile(st, code);
            System.Diagnostics.Debug.Assert(ret == null, auxname, ret);
            ret = program.Link(separable: true);
            System.Diagnostics.Debug.Assert(ret == null, auxname, ret);
            return program;
        }


    }
}
