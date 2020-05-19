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
    // Autoscale to size on model if required
    //      location 0 : position: vec4 vertex array of positions model coords, w is ignored
    //      location 1 : worldpositions
    //      uniform block 0 : GL MatrixCalc
    //      uniform 22 : objecttransform: mat4 transform of model before world applied (for rotation/scaling)
    // Out:
    //      gl_Position
    //      location 1 : wordpos copied
    //      location 2 : instance id

    public class GLPLVertexScaleLookat : GLShaderPipelineShadersBase
    {
        string vert =
        @"
#version 450 core

#include UniformStorageBlocks.matrixcalc.glsl
#include Shaders.Functions.trig.glsl
#include Shaders.Functions.mat4.glsl
#include Shaders.Functions.vec4.glsl

layout (location = 0) in vec4 modelposition;
layout (location = 1) in vec4 worldposition;
layout (location = 22) uniform  mat4 transform;

layout( location = 1) out vec4 worldposinstance;
layout (location = 2) out int instance;

out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

const bool rotateelevation = true;
const bool rotate = true;
const bool usetransform = false;
const float autoscale = 0;
const float autoscalemax = 0;
const float autoscalemin = 0;

void main(void)
{
    vec4 pos = vec4(modelposition.xyz,1);       

    if ( autoscale>0)
        pos = Scale(pos,clamp(mc.EyeDistance/autoscale,autoscalemin,autoscalemax));

    if ( usetransform )
    {
        pos = transform * pos;      // use transform to adjust
    }

    if ( rotate )
    {
        vec2 dir = AzEl(worldposition.xyz,mc.EyePosition.xyz);      // y = azimuth

        mat4 tx;

        if (rotateelevation )
            tx = mat4rotateXthenY(dir.x,PI-dir.y);              // rotate the flat image to vertical using dir.x (0 = on top, 90 = straight on, etc) then rotate by 180-dir (0 if directly facing from neg z)
        else
            tx = mat4rotateXthenY(PI/2,PI-dir.y);

        pos = pos * tx;
    }

    gl_Position = pos;
    
    worldposinstance = worldposition;
    instance = gl_InstanceID;
}
";

        public GLPLVertexScaleLookat(bool rotate = false, bool rotateelevation = true, bool commontransform = false,
                                                    float autoscale = 0, float autoscalemin = 0.1f, float autoscalemax = 3f)
        {
            CompileLink(ShaderType.VertexShader, vert, new object[] { "rotate", rotate, "rotateelevation", rotateelevation, "usetransform", commontransform, "autoscale", autoscale, "autoscalemin", autoscalemin, "autoscalemax", autoscalemax });
        }
    }
}
