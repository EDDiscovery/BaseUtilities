/*
 * Copyright © 2019 Robbyxp1 @ github.com
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
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{

    // Pipeline shader for a 2D texture bound with 2D vertexes
    // Requires:
    //      location 0 : vs_texturecoordinate : vec2 of texture co-ord
    //      tex binding 1 : textureObject : 2D texture

    public class GLPLFragmentShaderTexture : GLShaderPipelineShadersBase
    {
        public string Code()
        {
            return
@"
#version 450 core
layout (location=0) in vec2 vs_textureCoordinate;
layout (binding=1) uniform sampler2D textureObject;
out vec4 color;

void main(void)
{
    color = texture(textureObject, vs_textureCoordinate);       // vs_texture coords normalised 0 to 1.0f
}
";
        }

        public GLPLFragmentShaderTexture()
        {
            CompileLink(ShaderType.FragmentShader, Code(), GetType().Name);
        }

        public override void Start()
        {
            GLStatics4.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            OpenTKUtils.GLStatics.Check();
        }
    }

    // Pipeline shader for a 2D Array texture bound using instance to pick between them. Use with GLVertexShaderTextureMatrixTransform
    // Requires:
    //      location 0 : vs_texturecoordinate : vec2 of texture co-ord
    //      tex binding 1 : textureObject : 2D texture
    //      vs_in.vs_instance - instance id

    public class GLPLFragmentShaderTexture2DIndexed : GLShaderPipelineShadersBase
    {
        public string Code(int offset)
        {
            return
@"
#version 450 core
layout (location=0) in vec2 vs_textureCoordinate;
layout (binding=1) uniform sampler2DArray textureObject2D;
out vec4 color;

in VS_IN
{
    flat int vs_instanced;      // not sure why structuring is needed..
} vs;

void main(void)
{
    color = texture(textureObject2D, vec3(vs_textureCoordinate,vs.vs_instanced+ " + offset.ToStringInvariant() + @"));
}
";
        }

        public GLPLFragmentShaderTexture2DIndexed(int offset)
        {
            CompileLink(ShaderType.FragmentShader, Code(offset), GetType().Name);
        }

        public override void Start()
        {
            GLStatics4.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            OpenTKUtils.GLStatics.Check();
        }
    }

    // Pipeline shader, 2d texture array (0,1), 2d o-ords, with blend between them via a uniform
    // Requires:
    //      location 0 : vs_texturecoordinate : vec2 of texture co-ord
    //      tex binding 1 : textureObject : 2D array texture of two bitmaps, 0 and 1.
    //      location 30 : uniform float blend between the two texture

    public class GLPLFragmentShaderTexture2DBlend : GLShaderPipelineShadersBase
    {
        public string Code()
        {
            return
@"
#version 450 core
in vec2 vs_textureCoordinate;
layout (binding=1) uniform sampler2DArray textureObject;
out vec4 color;
layout (location = 30) uniform float blend;

void main(void)
{
    vec4 col1 = texture(textureObject, vec3(vs_textureCoordinate,0));
    vec4 col2 = texture(textureObject, vec3(vs_textureCoordinate,1));
    color = mix(col1,col2,blend);
}
";
        }

        public float Blend { get; set; } = 0.0f;

        public GLPLFragmentShaderTexture2DBlend()
        {
            CompileLink(ShaderType.FragmentShader, Code(), GetType().Name);
        }

        public override void Start()
        {
            GLStatics4.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);        // need fill for fragment to work
            GL.ProgramUniform1(Id, 30, Blend);
            OpenTKUtils.GLStatics.Check();
        }
    }

    // Pipeline shader, Co-ords are from a triangle strip, so we need to invert x for each other set of triangles
    // Requires:
    //      location 0 : vs_texturecoordinate : vec2 of texture co-ord - as per triangle strip
    //      tex binding 1 : textureObject : 2D array texture of two bitmaps, 0 and 1.
    //      location 24 : uniform of texture offset (written by start automatically)

    public class GLPLFragmentShaderTextureTriangleStrip : GLShaderPipelineShadersBase
    {
        public string Code()
        {
            return
@"
#version 450 core
layout (location=0) in vec2 vs_textureCoordinate;
layout (binding=1) uniform sampler2D textureObject;
layout (location = 24) uniform  vec2 offset;
out vec4 color;

void main(void)
{
    if ( gl_PrimitiveID % 4 < 2 )   // first two primitives have coords okay
    {
        color = texture(textureObject, vs_textureCoordinate+offset);       // vs_texture coords normalised 0 to 1.0f
    }
    else    // next two have them inverted in x due to re-using the previous triangles vertexes
    {
        color = texture(textureObject, vec2(1.0-vs_textureCoordinate.x,vs_textureCoordinate.y)+offset);       // vs_texture coords normalised 0 to 1.0f
    }

}
";
        }

        public Vector2 TexOffset { get; set; } = Vector2.Zero;                   // set to animate.

        public GLPLFragmentShaderTextureTriangleStrip()
        {
            CompileLink(ShaderType.FragmentShader, Code(), GetType().Name);
        }

        public override void Start()
        {
            GLStatics4.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.ProgramUniform2(Id, 24, TexOffset);
            OpenTKUtils.GLStatics.Check();
        }
    }

    // Pipeline shader, Co-ords are from a triangle strip, so we need to invert x for each other set of triangles
    // Requires:
    //      location 0 : vs_texturecoordinate : vec2 of texture co-ord - as per triangle strip
    //      uniform binding 10: ARB bindless texture handles, int 64s
    //      location 24 : uniform of texture offset (written by start automatically)

    public class GLPLBindlessFragmentShaderTextureTriangleStrip : GLShaderPipelineShadersBase
    {
        public string Code()
        {
            return
@"
#version 450 core
#extension GL_ARB_bindless_texture : require

layout (location=0) in vec2 vs_textureCoordinate;

layout (binding = 10, std140) uniform TEXTURE_BLOCK
{
    sampler2D tex[2];
};

layout (location = 24) uniform  vec2 offset;
out vec4 color;

void main(void)
{
    int objno = gl_PrimitiveID/2;

    if ( (objno % 2) == 0 )   // first two primitives have coords okay
    {
        color = texture(tex[objno], vs_textureCoordinate+offset);       // vs_texture coords normalised 0 to 1.0f
    }
    else    // next two have them inverted in x due to re-using the previous triangles vertexes
    {
        color = texture(tex[objno], vec2(1.0-vs_textureCoordinate.x,vs_textureCoordinate.y)+offset);       // vs_texture coords normalised 0 to 1.0f
    }

    //color = texture(tex[1], vs_textureCoordinate+offset);       // vs_texture coords normalised 0 to 1.0f
    //vec4 sc[] = { vec4(1,0,0,1),vec4(0,1,0,1),vec4(0,0,1,1),vec4(1,1,0,1),vec4(0,1,1,1),vec4(1,1,1,1)};
    //if ( objno == 1)
    //    color = sc[objno+1];

}
";
        }

        public Vector2 TexOffset { get; set; } = Vector2.Zero;                   // set to animate.

        public GLPLBindlessFragmentShaderTextureTriangleStrip()
        {
            CompileLink(ShaderType.FragmentShader, Code(), GetType().Name);
        }

        public override void Start()
        {
            GLStatics4.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.ProgramUniform2(Id, 24, TexOffset);
            OpenTKUtils.GLStatics.Check();
        }
    }




}
