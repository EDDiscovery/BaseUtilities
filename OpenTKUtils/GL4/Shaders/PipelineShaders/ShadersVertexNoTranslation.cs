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
    public class GLVertexShaderNoTranslation : GLShaderPipelineShadersBase
    {
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

void main(void)
{
	gl_Position = mc.ProjectionModelMatrix * position;        // order important
}
";
        }

        public GLVertexShaderNoTranslation()
        {
            Program = GLProgram.CompileLink(ShaderType.VertexShader, Code(), GetType().Name);
        }
    }


    public class GLVertexShaderColourNoTranslation : GLShaderPipelineShadersBase
    {
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

void main(void)
{
	gl_Position = mc.ProjectionModelMatrix * position;        // order important
	vs_color = color;                                                   // pass to fragment shader
}
";
        }

        public GLVertexShaderColourNoTranslation()
        {
            Program = GLProgram.CompileLink(ShaderType.VertexShader, Code(), GetType().Name);
        }
    }


}
