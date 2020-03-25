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
            CompileLink(ShaderType.FragmentShader, Code(), auxname: GetType().Name);
        }
    }

    // Pipeline shader, uniform decides colour, use GLRenderDataTranslationRotationColour or similar to set the uniform on a per draw basis
    // Requires:
    //      uniform : vec4 of colour

    public class GLPLFragmentShaderUniformColour : GLShaderPipelineShadersBase
    {
        public string Code()
        {
            return
@"
#version 450 core
out vec4 color;

const int bindingpoint = 25;
layout (location=bindingpoint) uniform vec4 ucol;

void main(void)
{
    color = ucol;
}
";
        }

        public GLPLFragmentShaderUniformColour(int uniform = 25)
        {
            CompileLink(ShaderType.FragmentShader, Code(), constvalues:new object[] { "bindingpoint", uniform }, auxname: GetType().Name);
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
            CompileLink(ShaderType.FragmentShader, Code(), auxname: GetType().Name);
        }
    }

    // Pipeline shader, one of six colour based on primitive ID, selectable divisor, mostly for testing

    public class GLPLFragmentIDShaderColour : GLShaderPipelineShadersBase
    {
        public string Code(int divisor)
        {
            return
@"
#version 450 core
out vec4 color;

void main(void)
{
    int side = gl_PrimitiveID/" + divisor.ToStringInvariant() + @";
    vec4 sc[] = { vec4(1,0,0,1),vec4(0,1,0,1),vec4(0,0,1,1),vec4(1,1,0,1),vec4(0,1,1,1),vec4(1,1,1,1)};
	color = sc[side % 6];
}
";
        }

        public GLPLFragmentIDShaderColour(int divisor)
        {
            CompileLink(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, Code(divisor), auxname: GetType().Name);
        }
    }


}
