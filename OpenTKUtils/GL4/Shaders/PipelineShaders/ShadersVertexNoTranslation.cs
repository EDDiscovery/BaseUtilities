/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
 * 
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

namespace OpenTKUtils.GL4
{
    // No extra translation, direct move
    // Requires:
    //      location 0 vec4 positions
    //      uniform 0 standard Matrix uniform block GLMatrixCalcUniformBlock

    public class GLPLVertexShaderWorldCoord : GLShaderPipelineShadersBase
    {
        public string Code()
        {
            return
@"
#version 450 core

#include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl

layout (location = 0) in vec4 position;
out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

void main(void)
{
	gl_Position = mc.ProjectionModelMatrix * position;        // order important
}
";
        }

        public GLPLVertexShaderWorldCoord()
        {
            CompileLink(ShaderType.VertexShader, Code(), auxname: GetType().Name);
        }
    }

    // No modelview, just project view. Co-ords are in model view values
    // Requires:
    //      location 0 vec4 positions
    //      uniform 0 standard Matrix uniform block GLMatrixCalcUniformBlock

    public class GLPLVertexShaderModelViewCoord: GLShaderPipelineShadersBase
    {
        public string Code()
        {
            return
@"
#version 450 core
#include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl
layout (location = 0) in vec4 position;
out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

void main(void)
{
	gl_Position = mc.ProjectionMatrix * position;        // order important
}
";
        }

        public GLPLVertexShaderModelViewCoord()
        {
            CompileLink(ShaderType.VertexShader, Code(), auxname: GetType().Name);
        }
    }



    // No extra translation, direct move, but with colour
    // Requires:
    //      location 0 vec4 positions in world space
    //      location 1 vec4 color components
    //      uniform 0 standard Matrix uniform block GLMatrixCalcUniformBlock

    public class GLPLVertexShaderColourWorldCoord : GLShaderPipelineShadersBase
    {
        public string Code()
        {
            return
@"
#version 450 core
#include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl
layout (location = 0) in vec4 position;
out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };


layout(location = 1) in vec4 color;
out vec4 vs_color;

void main(void)
{
	gl_Position = mc.ProjectionModelMatrix * position;        // order important
	vs_color = color;                                                   // pass to fragment shader
}
";
        }

        public GLPLVertexShaderColourWorldCoord()
        {
            CompileLink(ShaderType.VertexShader, Code(), auxname: GetType().Name);
        }
    }

    // Pipeline shader, Texture, Modelpos, transform
    // Requires:
    //      location 0 : position: vec4 vertex array of positions
    //      location 1 : vec2 texture co-ords
    //      uniform 0 : GL MatrixCalc
    // Out:
    //      gl_Position
    //      vs_textureCoordinate
    //      modelpos

    public class GLPLVertexShaderTextureWorldCoord : GLShaderPipelineShadersBase
    {
        public string Code()     
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

void main(void)
{
    modelpos = position.xyz;
	gl_Position = mc.ProjectionModelMatrix * position;        // order important
    vs_textureCoordinate = texco;
}
";
        }

        public GLPLVertexShaderTextureWorldCoord()
        {
            CompileLink(ShaderType.VertexShader, Code(), auxname: GetType().Name);
        }
    }


    // Pipeline shader, Texture, Modelpos, transform
    // Requires:
    //      location 0 : position: vec4 vertex array of positions
    //      uniform 0 : GL MatrixCalc
    // Out:
    //      gl_Position
    //      vs_textureCoordinate per triangle strip rules
    //      modelpos

    public class GLPLVertexShaderTextureWorldCoordWithTriangleStripCoord : GLShaderPipelineShadersBase
    {
        public string Code()       
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

layout(location = 0) out vec2 vs_textureCoordinate;
layout(location = 1) out vec3 modelpos;

void main(void)
{
    vec2 vcoords[4] = {{0,0},{0,1},{1,0},{1,1} };      // these give the coords for the 4 points making up 2 triangles.  Use with the right fragment shader which understands strip co-ords

    modelpos = position.xyz;
    vec4 p = position;
	gl_Position = mc.ProjectionModelMatrix * p;        // order important
    vs_textureCoordinate = vcoords[ gl_VertexID % 4];       // gl_vertextid is either an autocounter, or the actual element index given in an element draw
}
";
        }

        public GLPLVertexShaderTextureWorldCoordWithTriangleStripCoord()
        {
            CompileLink(ShaderType.VertexShader, Code(), auxname: GetType().Name);
        }
    }

    // Pipeline shader, Texture, real screen coords  (0-glcontrol.Width,0-glcontrol.height, 0,0 at top left)
    // Requires:
    //      location 0 : position: vec4 vertex array of real screen coords in the x/y/z slots.  w must be 1.
    //      uniform 0 : GL MatrixCalc with ScreenMatrix set up
    // Out:
    //      gl_Position
    //      vs_textureCoordinate per triangle strip rules
    //      z=0 placing it in foreground

    public class GLPLVertexShaderTextureScreenCoordWithTriangleStripCoord : GLShaderPipelineShadersBase
    {
        public string Code()
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

layout(location = 0) out vec2 vs_textureCoordinate;

void main(void)
{
	gl_Position = mc.ScreenMatrix * position;        // order important
    vec2 vcoords[4] = {{0,0},{0,1},{1,0},{1,1} };      // these give the coords for the 4 points making up 2 triangles.  Use with the right fragment shader which understands strip co-ords
    vs_textureCoordinate = vcoords[ gl_VertexID % 4];
}
";
        }

        public GLPLVertexShaderTextureScreenCoordWithTriangleStripCoord()
        {
            CompileLink(ShaderType.VertexShader, Code(), auxname: GetType().Name);
        }
    }

}
