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
    public class GLVertexShaderObjectTransform : GLShaderPipelineShadersBase
    {
        public string Code()       // with transform, object needs to pass in uniform 22 the transform
        {
            return
@"
#version 450 core
layout (location = 0) in vec4 position;
out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

layout (location = 1) out vec3 modelpos;

layout (location = 20) uniform  mat4 projectionmodel;
layout (location = 22) uniform  mat4 transform;

void main(void)
{
    modelpos = position.xyz;
	gl_Position = projectionmodel * transform * position;        // order important
}
";
        }

        public GLVertexShaderObjectTransform()
        {
            Program = GLProgram.CompileLink(ShaderType.VertexShader, Code(), GetType().Name);
            SetupProjMatrix = true;
        }
    }




    public class GLVertexShaderColourObjectTransform : GLShaderPipelineShadersBase
    {
        public string Code()       // with transform, object needs to pass in uniform 22 the transform
        {
            return

@"
#version 450 core
layout (location = 0) in vec4 position;
out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

layout(location = 1) in vec4 color;
out vec4 vs_color;

layout (location = 20) uniform  mat4 projectionmodel;
layout (location = 22) uniform  mat4 transform;

void main(void)
{
	gl_Position = projectionmodel * transform * position;        // order important
	vs_color = color;                                                   // pass to fragment shader
}
";
        }

        public GLVertexShaderColourObjectTransform()
        {
            Program = GLProgram.CompileLink(ShaderType.VertexShader, Code(), GetType().Name);
            SetupProjMatrix = true;
        }
    }


    public class GLVertexShaderTextureObjectTransform : GLShaderPipelineShadersBase
    {
        public string Code()       // with transform, object needs to pass in uniform 22 the transform
        {
            return

@"
#version 450 core
layout (location = 0) in vec4 position;
out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

layout(location = 1) in vec2 texco;

layout(location = 0) out vec2 vs_textureCoordinate;
layout(location = 1) out vec3 modelpos;

layout (location = 20) uniform  mat4 projectionmodel;
layout (location = 22) uniform  mat4 transform;

void main(void)
{
    modelpos = position.xyz;
	gl_Position = projectionmodel * transform * position;        // order important
    vs_textureCoordinate = texco;
}
";
        }

        public GLVertexShaderTextureObjectTransform()
        {
            Program = GLProgram.CompileLink(ShaderType.VertexShader, Code(), GetType().Name);
            SetupProjMatrix = true;
        }
    }


}
