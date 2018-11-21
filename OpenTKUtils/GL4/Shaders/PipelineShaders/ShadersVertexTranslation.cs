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

    public class GLVertexShaderMatrixTranslation : GLShaderPipelineShadersBase
    {
        public string Code()
        {
            return
@"
#version 450 core
" + GLMatrixCalcUniformBlock.GLSL + @"
layout (location = 0) in vec4 position;
layout (location = 4) in mat4 transform;

out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

out vec4 vs_color;

void main(void)
{
    vs_color = vec4(gl_InstanceID*0.2+0.2,gl_InstanceID*0.2+0.2,0.5+gl_VertexID*0.1,1.0);       // colour may be thrown away if required..
	gl_Position = mc.ProjectionModelMatrix * transform * position;        // order important
}
";
        }

        public GLVertexShaderMatrixTranslation()
        {
            Program = GLProgram.CompileLink(ShaderType.VertexShader, Code(), GetType().Name);
        }
    }


    public class GLVertexShaderTextureMatrixTranslation : GLShaderPipelineShadersBase
    {
        public string Code()
        {
            return
@"
#version 450 core
" + GLMatrixCalcUniformBlock.GLSL + @"
layout (location = 0) in vec4 position;
layout (location = 4) in mat4 transform;

out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

layout(location = 1) in vec2 texco;
out vec2 vs_textureCoordinate;

void main(void)
{
	gl_Position = mc.ProjectionModelMatrix * transform * position;        // order important
    vs_textureCoordinate = texco;
}
";
        }

        public GLVertexShaderTextureMatrixTranslation()
        {
            Program = GLProgram.CompileLink(ShaderType.VertexShader, Code(), GetType().Name);
        }
    }



    public class GLVertexShaderColorTransformWithCommonTransform : GLShaderPipelineShadersBase
    {
        public GLObjectDataTranslationRotation Transform { get; set; }           // only use this for rotation - position set by object data

        public string Code()
        {
            return

@"
#version 450 core
" + GLMatrixCalcUniformBlock.GLSL + @"
layout (location = 0) in vec4 position;
out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

layout(location = 1) in vec4 color;
out vec4 vs_color;

layout (location = 22) uniform  mat4 transform;
layout (location = 23) uniform  mat4 commontransform;

void main(void)
{
	gl_Position = mc.ProjectionModelMatrix * transform *  commontransform * position;        // order important
	vs_color = color;                                                   // pass to fragment shader
}
";
        }

        // with transform, object needs to pass in uniform 22 the transform

        public GLVertexShaderColorTransformWithCommonTransform()
        {
            Transform = new GLObjectDataTranslationRotation();
            Program = GLProgram.CompileLink(ShaderType.VertexShader, Code(), GetType().Name);
        }

        public override void Start()
        {
            base.Start();
            Matrix4 t = Transform.Transform;
            GL.ProgramUniformMatrix4(Id, 23, false, ref t);
            GLStatics.Check();
        }
    }


    public class GLVertexShaderTextureTransformWithCommonTransform : GLShaderPipelineShadersBase
    {
        public GLObjectDataTranslationRotation Transform { get; set; }           // only use this for rotation - position set by object data

        public string Code()
        {
            return

@"
#version 450 core
" + GLMatrixCalcUniformBlock.GLSL + @"
layout (location = 0) in vec4 position;
out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

layout(location = 1) in vec2 texco;
out vec2 vs_textureCoordinate;

layout (location = 22) uniform  mat4 transform;
layout (location = 23) uniform  mat4 commontransform;

void main(void)
{
	gl_Position = mc.ProjectionModelMatrix * transform *  commontransform * position;        // order important
    vs_textureCoordinate = texco;
}
";
        }

        // with transform, object needs to pass in uniform 22 the transform

        public GLVertexShaderTextureTransformWithCommonTransform()
        {
            Transform = new GLObjectDataTranslationRotation();
            Program = GLProgram.CompileLink(ShaderType.VertexShader, Code(), GetType().Name);
        }

        public override void Start()
        {
            base.Start();
            Matrix4 t = Transform.Transform;
            GL.ProgramUniformMatrix4(Id, 23, false, ref t);
            GLStatics.Check();
        }
    }



}

