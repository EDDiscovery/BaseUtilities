/*
 * Copyright 2019 Robbyxp1 @ github.com
 * Part of the EDDiscovery Project
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
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using BaseUtils;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    // This is the base for all programs
    // programs hold GLShaders which are compiled and linked into the Program
    // Once linked, the program can execute the shaders
    // it holds one or more GLShaders (Vertex/ Geo/ Fragment etc)

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

        // completeoutfile is output of file for debugging
        public string Compile( ShaderType st, string codelisting, string completeoutfile = null )        // code listing - with added #includes
        {
            GLShader shader = new GLShader(st);

            LineReader lr = new LineReader();
            lr.OpenString(codelisting);

            string code = "", line;
            bool doneversion = false;

            while( (line = lr.ReadLine())!=null)
            {
                line = line.Trim();
                if (line.StartsWith("#include", StringComparison.InvariantCultureIgnoreCase) || line.StartsWith("//Include", StringComparison.InvariantCultureIgnoreCase))
                {
                    line = line.Mid(line[0]=='#' ? 8 : 9).Trim();
                    string include = BaseUtils.ResourceHelpers.GetResourceAsString(line);
                    System.Diagnostics.Debug.Assert(include != null, "Cannot include " + line);
                    lr.OpenString(include);     // include it
                }
                else if ( line.StartsWith("#version", StringComparison.InvariantCultureIgnoreCase))        // one and only one #version
                {
                    if ( !doneversion)
                    {
                        code += line + Environment.NewLine;
                        doneversion = true;
                    }
                }
                else
                    code += line + Environment.NewLine;
            }

            if ( completeoutfile != null )
                System.IO.File.WriteAllText(completeoutfile, code);
            string ret = shader.Compile(code);

            if (ret == null)
            {
                Add(shader);
                return null;
            }
            else
                return ret;
        }

        public string Link( bool separable = false )            // link, seperable or not.  Disposes of shaders. null if okay
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
    }
}
