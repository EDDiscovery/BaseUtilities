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
    // Pipeline shader, Translation, Modelpos, transform
    // Requires:
    //      location 0 : position: vec4 vertex array of positions
    //      location 1 : vec3 model position
    //      uniform 0 : GL MatrixCalc
    //      uniform 22 : objecttransform: mat4 array of transforms
    // Out:
    //      gl_Position
    //      modelpos

    public class GLVertexShaderObjectTransform : GLShaderPipelineShadersBase
    {
        public string Code()       // with transform, object needs to pass in uniform 22 the transform
        {
            return
@"
#version 450 core
#include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl
out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

layout (location = 0) in vec4 position;
layout (location = 1) out vec3 modelpos;

layout (location = 22) uniform  mat4 transform;

void main(void)
{
    modelpos = position.xyz;
	gl_Position = mc.ProjectionModelMatrix * transform * position;        // order important
}
";
        }

        public GLVertexShaderObjectTransform()
        {
            Program = GLProgram.CompileLink(ShaderType.VertexShader, Code(), GetType().Name);
        }
    }


    // Pipeline shader, Translation, Colour, Modelpos, transform
    // Requires:
    //      location 0 : position: vec4 vertex array of positions
    //      location 1 : vec4 colour
    //      uniform 0 : GL MatrixCalc
    //      uniform 22 : objecttransform: mat4 array of transforms
    // Out:
    //      gl_Position
    //      vs_color
    //      modelpos

    public class GLVertexShaderColourObjectTransform : GLShaderPipelineShadersBase
    {
        public string Code()       // with transform, object needs to pass in uniform 22 the transform
        {
            return

@"
#version 450 core
#include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl
out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

layout (location = 0) in vec4 position;
layout (location = 1) in vec4 color;

layout (location = 22) uniform  mat4 transform;

out vec4 vs_color;
layout (location = 1) out vec3 modelpos;

void main(void)
{
    modelpos = position.xyz;
	gl_Position = mc.ProjectionModelMatrix * transform * position;        // order important
	vs_color = color;                                                   // pass to fragment shader
}
";
        }

        public GLVertexShaderColourObjectTransform()
        {
            Program = GLProgram.CompileLink(ShaderType.VertexShader, Code(), GetType().Name);
        }
    }

    // Pipeline shader, Translation, Texture, Modelpos, transform
    // Requires:
    //      location 0 : position: vec4 vertex array of positions
    //      location 1 : vec2 texture co-ords
    //      uniform 0 : GL MatrixCalc
    //      uniform 22 : objecttransform: mat4 array of transforms
    // Out:
    //      gl_Position
    //      vs_textureCoordinate
    //      modelpos


    public class GLVertexShaderTextureObjectTransform : GLShaderPipelineShadersBase
    {
        public string Code()       // with transform, object needs to pass in uniform 22 the transform
        {
            return

@"
#version 450 core
#include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl
out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

layout (location = 0) in vec4 position;
layout(location = 1) in vec2 texco;

layout(location = 0) out vec2 vs_textureCoordinate;
layout(location = 1) out vec3 modelpos;

layout (location = 22) uniform  mat4 transform;

void main(void)
{
    modelpos = position.xyz;
	gl_Position = mc.ProjectionModelMatrix * transform * position;        // order important
    vs_textureCoordinate = texco;
}
";
        }

        public GLVertexShaderTextureObjectTransform()
        {
            Program = GLProgram.CompileLink(ShaderType.VertexShader, Code(), GetType().Name);
        }
    }
}
