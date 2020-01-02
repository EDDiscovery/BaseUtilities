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
    // Texture, no translation
    // Requires:
    //      location 0 : position: vec4 vertex array of positions world coords
    //      location 1 : vec2 texture co-ords
    //      tex binding 1 : textureObject : 2D 
    //      uniform 0 : GL MatrixCalc

    public class GLTexturedShaderWithWorldCoord : GLShaderPipeline
    {
        public GLTexturedShaderWithWorldCoord(Action<IGLProgramShader> start = null, Action<IGLProgramShader> finish = null) : base(start, finish)
        {
            AddVertexFragment(new GLPLVertexShaderTextureWorldCoord(), new GLPLFragmentShaderTexture());
        }
    }

    // Texture, translation
    // Requires:
    //      location 0 : position: vec4 vertex array of positions
    //      location 1 : vec2 texture co-ords
    //      tex binding 1 : textureObject : 2D 
    //      uniform 0 : GL MatrixCalc
    //      uniform 22 : objecttransform: mat4 array of transforms

    public class GLTexturedShaderWithObjectTranslation : GLShaderPipeline
    {
        public GLTexturedShaderWithObjectTranslation(Action<IGLProgramShader> start = null, Action<IGLProgramShader> finish = null) : base(start, finish)
        {
            AddVertexFragment(new GLPLVertexShaderTextureModelCoordWithObjectTranslation(), new GLPLFragmentShaderTexture());
        }
    }

    // Texture, translation, common translation
    // Requires:
    //      location 0 : position: vec4 vertex array of positions
    //      location 1 : vec2 texture co-ords
    //      tex binding 1 : textureObject : 2D 
    //      uniform 0 : GL MatrixCalc
    //      uniform 22 : objecttransform: mat4 array of transforms
    //      uniform 23 : commontransform: mat4 array of transforms

    public class GLTexturedShaderWithObjectCommonTranslation : GLShaderPipeline
    {
        public GLTexturedShaderWithObjectCommonTranslation(Action<IGLProgramShader> start = null, Action<IGLProgramShader> finish = null) : base(start, finish)
        {
            AddVertexFragment(new GLPLVertexShaderTextureModelCoordsWithObjectCommonTranslation(), new GLPLFragmentShaderTexture());
        }
    }

    // Texture, translation, 2d blend
    // Requires:
    //      location 0 : position: vec4 vertex array of positions
    //      location 1 : vec2 texture co-ords
    //      tex binding 1 : textureObject : 2D array texture of two bitmaps, 0 and 1.
    //      uniform 0 : GL MatrixCalc
    //      uniform 22 : objecttransform: mat4 array of transforms
    //      location 30 : uniform float blend between the two texture

    public class GLTexturedShader2DBlendWithWorldCoord : GLShaderPipeline
    {
        public GLTexturedShader2DBlendWithWorldCoord(Action<IGLProgramShader> start = null, Action<IGLProgramShader> finish = null) : base(start, finish)
        {
            AddVertexFragment(new GLPLVertexShaderTextureModelCoordWithObjectTranslation(), new GLPLFragmentShaderTexture2DBlend());
        }
    }

    // Texture, triangle strip
    // Requires:
    //      location 0 : position: vec4 vertex array of positions world
    //      tex binding 1 : textureObject : texture of bitmap
    //      uniform 0 : GL MatrixCalc
    //      location 24 : uniform of texture offset (written by start automatically)

    public class GLTexturedShaderTriangleStripWithWorldCoord : GLShaderPipeline
    {
        GLPLFragmentShaderTextureTriangleStrip frag;

        public GLTexturedShaderTriangleStripWithWorldCoord(bool backtoback, Action<IGLProgramShader> start = null, Action<IGLProgramShader> finish = null) : base(start, finish)
        {
            frag = new GLPLFragmentShaderTextureTriangleStrip(backtoback);
            AddVertexFragment(new GLPLVertexShaderTextureWorldCoordWithTriangleStripCoord(), frag);
        }

        public Vector2 TexOffset { get { return frag.TexOffset; } set { frag.TexOffset = value; } }
    }




}
