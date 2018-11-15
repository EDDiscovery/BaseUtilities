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
in vec4 vs_color;
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
//color = vec4(1.0,1.0,0,1.0);
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
in vec2 vs_textureCoordinate;
layout (binding=1) uniform sampler2D textureObject;
out vec4 color;

void main(void)
{
    color = texture(textureObject, vs_textureCoordinate);       // vs_texture coords normalised 0 to 1.0f
}
";      }

        public GLFragmentShaderTexture()
        {
            Program = GLProgram.CompileLink(ShaderType.FragmentShader, Code(), GetType().Name);
        }

        public override void Start(Common.MatrixCalc c) 
        {
            GL4Statics.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GLStatics.Check();
        }
    }

    // Requires a 2D texture array bound

    public class GLFragmentShader2DCommonBlend : GLShaderPipelineShadersBase
    {
        const int BindingPoint = 1;

        public float Blend { get; set; } = 0.0f;
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

        public GLFragmentShader2DCommonBlend()
        {
            Program = GLProgram.CompileLink(ShaderType.FragmentShader, Code(), GetType().Name);
        }

        public override void Start(Common.MatrixCalc c)
        {
            GL4Statics.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);        // need fill for fragment to work
            GL.ProgramUniform1(Id, 30, Blend);
            GLStatics.Check();
        }
    }

    //  

    public class GLFragmentShaderStarTexture : GLShaderPipelineShadersBase
    {
        const int BindingPoint = 1;

        public string Code()
        {
            return
@"
#version 450 core
in vec2 vs_textureCoordinate;
in vec3 modelpos;
out vec4 pColor;

" + GLShaderFunctionsNoise.NoiseFunctions3 +
@"
void main(void)
{
    vec3 position = modelpos/20;
    float n = (noise(position, 4, 40.0, 0.7) + 1.0) * 0.5;

    //float unRadius = 1000.0;

    // Get worldspace position
    //vec3 sPosition = position * unRadius;
    
    // Sunspots
    //float s = 0.36;
    //float frequency = 0.00001;
    //float t1 = snoise(sPosition * frequency) - s;
    //float t2 = snoise((sPosition + unRadius) * frequency) - s;
	//float ss = (max(t1, 0.0) * max(t2, 0.0)) * 2.0;
    // Accumulate total noise

    //float total = n - ss;
    float total = n;

    vec3 unCenterDir = vec3(1,0,0);
	float theta = 0.0;//- dot(unCenterDir, modelpos);

    float mag = 0.3;
	
    vec3 unColor = vec3(0.5, 0.5 ,0.0);
    pColor = vec4(unColor + (total - 0.0)*mag - theta, 1.0);
}
";
        }

        public GLFragmentShaderStarTexture()
        {
            Program = GLProgram.CompileLink(ShaderType.FragmentShader, Code(), GetType().Name);
        }

        public override void Start(Common.MatrixCalc c)
        {
            GL4Statics.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);        // need fill for fragment to work
            GLStatics.Check();
        }
    }
}
