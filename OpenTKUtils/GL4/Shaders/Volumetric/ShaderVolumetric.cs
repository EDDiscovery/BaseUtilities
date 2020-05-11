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
    // vertex and geo shaders for volumetric shading.  You supply the fragment shader yourself, as its different for each use case.
    // join up using a shader pipelineL:
    // public Shader() {
    //          Add(new GLVertexShaderVolumetric(), OpenTK.Graphics.OpenGL4.ShaderType.VertexShader);
    //          Add(new GLGeometricShaderVolumetric(), OpenTK.Graphics.OpenGL4.ShaderType.GeometryShader);
    //          Add(new FragmentPipeline(), OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader); }

    public class GLPLVertexShaderVolumetric : GLShaderPipelineShadersBase
    {
        string vcode =
        @"
#version 450 core

out int instance;

out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

void main(void)
{
    instance = gl_InstanceID;
}
            ";

        public GLPLVertexShaderVolumetric()
        {
            CompileLink(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, vcode);
        }
    }

    public class GLPLGeometricShaderVolumetric : GLShaderPipelineShadersBase
    {
        public GLPLGeometricShaderVolumetric(int bufferbindingpoint)
        {
            CompileLink(ShaderType.GeometryShader, "#include Shaders.Volumetric.volumetricgeoshader.glsl", new object[] { "bufferbp", bufferbindingpoint } );
        }
    }
}

