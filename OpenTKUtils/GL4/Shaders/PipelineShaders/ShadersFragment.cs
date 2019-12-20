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
    // Pipeline shader, Fixed Colour fragment shader
    // Requires:
    //      no inputs

    public class GLPLFragmentShaderFixedColour : GLShaderPipelineShadersBase
    {
        OpenTK.Graphics.Color4 col;

        public string Code()
        {
            return
@"
#version 450 core
out vec4 color;

void main(void)
{
    color = vec4(" + col.R + "," + col.G + "," + col.B + "," + col.A + @");
}
";
        }

        public GLPLFragmentShaderFixedColour(OpenTK.Graphics.Color4 c)
        {
            col = c;
            Program = GLProgram.CompileLink(ShaderType.FragmentShader, Code(), GetType().Name);
        }
    }

    // Pipeline shader, Vertex shader colour pass to it
    // Requires:
    //      vs_color : vec4 of colour

    public class GLPLFragmentShaderColour : GLShaderPipelineShadersBase
    {
        public string Code()
        {
            return
@"
#version 450 core
in vec4 vs_color;
out vec4 color;

void main(void)
{
	color = vs_color;
}
";
        }

        public GLPLFragmentShaderColour()
        {
            Program = GLProgram.CompileLink(ShaderType.FragmentShader, Code(), GetType().Name);
        }
    }

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
            Program = GLProgram.CompileLink(ShaderType.FragmentShader, Code(), GetType().Name);
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
            Program = GLProgram.CompileLink(ShaderType.FragmentShader, Code(offset), GetType().Name);
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
            Program = GLProgram.CompileLink(ShaderType.FragmentShader, Code(), GetType().Name);
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
    //      location 30 : uniform float blend between the two texture

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
            Program = GLProgram.CompileLink(ShaderType.FragmentShader, Code(), GetType().Name);
        }

        public override void Start()
        {
            GLStatics4.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.ProgramUniform2(Id, 24, TexOffset);
            OpenTKUtils.GLStatics.Check();
        }
    }


}
