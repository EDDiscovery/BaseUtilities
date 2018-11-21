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
    // Fragment, requires vs_color

    public class GLFragmentShaderFixedColour : GLShaderPipelineShadersBase
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

        public GLFragmentShaderFixedColour(OpenTK.Graphics.Color4 c)
        {
            col = c;
            Program = GLProgram.CompileLink(ShaderType.FragmentShader, Code(), GetType().Name);
        }
    }

    public class GLFragmentShaderColour : GLShaderPipelineShadersBase
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

        public GLFragmentShaderColour()
        {
            Program = GLProgram.CompileLink(ShaderType.FragmentShader, Code(), GetType().Name);
        }
    }

    // Requires a 2D texture bound

    public class GLFragmentShaderTexture : GLShaderPipelineShadersBase
    {
        const int BindingPoint = 1;

        public string Code()
        {
            return
@"
#version 450 core
layout (location=0) in vec2 vs_textureCoordinate2;
layout (binding=1) uniform sampler2D textureObject;
out vec4 color;

void main(void)
{
    color = texture(textureObject, vs_textureCoordinate2);       // vs_texture coords normalised 0 to 1.0f
}
";
        }

        public GLFragmentShaderTexture()
        {
            Program = GLProgram.CompileLink(ShaderType.FragmentShader, Code(), GetType().Name);
        }

        public override void Start()
        {
            GL4Statics.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GLStatics.Check();
        }
    }

    // Requires a 2D texture array bound

    public class GLFragmentShader2DCommonBlend : GLShaderPipelineShadersBase
    {
        const int BindingPoint = 1;

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

        public GLFragmentShader2DCommonBlend()
        {
            Program = GLProgram.CompileLink(ShaderType.FragmentShader, Code(), GetType().Name);
        }

        public override void Start()
        {
            GL4Statics.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);        // need fill for fragment to work
            GL.ProgramUniform1(Id, 30, Blend);
            GLStatics.Check();
        }
    }


}