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
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    // Simple rendered with optional rot/translation

    public class GLVertexShadersBase : IGLSharedProgramShaders
    {
        public int Id { get { return program.Id; } }
        public IGLProgramShaders GetVertex() { return this; }
        public IGLProgramShaders GetFragment() { throw new NotImplementedException(); }

        public virtual string Code() { return null; }

        private GLProgram program;

        public void CompileLink()     
        {
            program = new OpenTKUtils.GL4.GLProgram();
            string ret = program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, Code());
            System.Diagnostics.Debug.Assert(ret == null, ret);
            ret = program.Link(separable: true);
            System.Diagnostics.Debug.Assert(ret == null, ret);
        }

        public virtual void Start(Common.MatrixCalc c) // seperable do not use a program - that is for the pipeline to hook up
        {
            Matrix4 projection = c.ProjectionMatrix;
            GL.ProgramUniformMatrix4(program.Id, 20, false, ref projection);
            Matrix4 model = c.ModelMatrix;
            GL.ProgramUniformMatrix4(program.Id, 21, false, ref model);
        }

        public virtual void Finish()
        {
        }

        public void Dispose()
        {
            program.Dispose();
        }
    }


    public class GLVertexShaderNoTranslation : GLVertexShadersBase
    {
        public override string Code()
        {
            return
@"
#version 450 core
layout (location = 0) in vec4 position;
layout(location = 1) in vec4 color;

out vec4 vs_color;

layout (location = 20) uniform  mat4 projection;
layout (location = 21) uniform  mat4 modelView;

out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

void main(void)
{
	gl_Position = projection * modelView * position;        // order important
	vs_color = color;                                                   // pass to fragment shader
}
";
        }

        // with transform, object needs to pass in uniform 22 the transform

        public GLVertexShaderNoTranslation() 
        {
            CompileLink();
        }
    }

    public class GLVertexShaderTransform : GLVertexShadersBase
    {
        public override string Code()
        {
            return

@"
#version 450 core
layout (location = 0) in vec4 position;
layout(location = 1) in vec4 color;

out vec4 vs_color;

layout (location = 20) uniform  mat4 projection;
layout (location = 21) uniform  mat4 modelView;
layout (location = 22) uniform  mat4 transform;

out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

void main(void)
{
	gl_Position = projection * modelView * transform * position;        // order important
	vs_color = color;                                                   // pass to fragment shader
}
";
        }

        // with transform, object needs to pass in uniform 22 the transform

        public GLVertexShaderTransform()
        {
            CompileLink();
        }
    }


    public class GLVertexShaderTransformWithCommonTransform : GLVertexShadersBase
    {
        public GLObjectDataTranslationRotation Transform { get; set; }           // only use this for rotation - position set by object data

        public override string Code()
        {
            return

@"
#version 450 core
layout (location = 0) in vec4 position;
layout(location = 1) in vec4 color;

out vec4 vs_color;

layout (location = 20) uniform  mat4 projection;
layout (location = 21) uniform  mat4 modelView;
layout (location = 22) uniform  mat4 transform;
layout (location = 23) uniform  mat4 commontransform;


out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

void main(void)
{
	gl_Position = projection * modelView * transform *  commontransform * position;        // order important
	vs_color = color;                                                   // pass to fragment shader
}
";
        }

        // with transform, object needs to pass in uniform 22 the transform

        public GLVertexShaderTransformWithCommonTransform()
        {
            Transform = new GLObjectDataTranslationRotation();
            CompileLink();
        }

        public override void Start(Common.MatrixCalc c)
        {
            base.Start(c);
            Matrix4 t = Transform.Transform;
            GL.ProgramUniformMatrix4(Id, 23, false, ref t);
        }

    }

}

