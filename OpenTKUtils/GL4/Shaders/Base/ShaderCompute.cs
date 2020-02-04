/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
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
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

namespace OpenTKUtils.GL4
{
    // inherit from this to make a compute shader 
    // you can either run it directly, or you can add it to a RenderableList to mix it with renderable items

    public abstract class GLShaderCompute : GLShaderStandard
    {
        int XWorkgroupSize { get; set; } = 1;
        int YWorkgroupSize { get; set; } = 1;
        int ZWorkgroupSize { get; set; } = 1;

        public GLShaderCompute()
        {
        }

        public GLShaderCompute(Action<IGLProgramShader> sa = null) : this()
        {
            StartAction = sa;
        }

        public GLShaderCompute(int x, int y, int z, Action<IGLProgramShader> sa = null) : this()
        {
            XWorkgroupSize = x; YWorkgroupSize = y; ZWorkgroupSize = z;
            StartAction = sa;
        }

        // completeoutfile is output of file for debugging
        public void CompileLink(string code, Object[] constvalues = null, string completeoutfile = null )
        {
            program = new OpenTKUtils.GL4.GLProgram();
            string ret = program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.ComputeShader, code, constvalues, completeoutfile);
            System.Diagnostics.Debug.Assert(ret == null, "Compute Shader", ret);
            ret = program.Link();
            System.Diagnostics.Debug.Assert(ret == null, "Link", ret);
            OpenTKUtils.GLStatics.Check();
        }

        public override void Start()                 // override.. but call back.  Executes compute.
        {
            base.Start();
            GL.DispatchCompute(XWorkgroupSize, YWorkgroupSize, ZWorkgroupSize);
        }

        public void Run()                           // for compute shaders, we can just run them.  
        {
            Start();
            Finish();
        }
    }
}
