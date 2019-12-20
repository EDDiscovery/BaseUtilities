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
    // Translation shader with vertex colours
    // Requires:
    //      location 0 vec4 positions of model
    //      location 1 vec4 colours of each vertex
    //      uniform 0 standard Matrix uniform block GLMatrixCalcUniformBlock
    //      uniform 22 matrix4 transform of model->world positions, supply using per object binding

    public class GLColourShaderWithObjectTranslation : GLShaderPipeline
    {
        public GLColourShaderWithObjectTranslation(Action<IGLProgramShader> start = null, Action<IGLProgramShader> finish = null) : base(start, finish)
        {
            AddVertexFragment(new GLPLVertexShaderColourModelCoordWithObjectTranslation(), new GLPLFragmentShaderColour());
        }
    }

    // Fixed position shader with vertex colours
    // Requires:
    //      location 0 vec4 positions of world positions
    //      location 1 vec4 colours of each vertex
    //      uniform 0 standard Matrix uniform block GLMatrixCalcUniformBlock
    
    public class GLColourShaderWithWorldCoord : GLShaderPipeline
    {
        public GLColourShaderWithWorldCoord(Action<IGLProgramShader> start = null, Action<IGLProgramShader> finish = null) : base(start, finish)
        {
            AddVertexFragment(new GLPLVertexShaderColourWorldCoord(), new GLPLFragmentShaderColour());
        }
    }

    // Fixed position shader with fixed colour
    // Requires:
    //      location 0 vec4 positions of world positions
    //      uniform 0 standard Matrix uniform block GLMatrixCalcUniformBlock

    public class GLFixedColourShaderWithWorldCoord : GLShaderPipeline
    {
        public GLFixedColourShaderWithWorldCoord(System.Drawing.Color c, Action<IGLProgramShader> start = null, Action<IGLProgramShader> finish = null) : base(start, finish)
        {
            AddVertexFragment(new GLPLVertexShaderWorldCoord(), new GLPLFragmentShaderFixedColour(c));
        }
    }

}
