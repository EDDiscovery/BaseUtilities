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

    // Pipeline shader, Matrix Translation
    // Requires:
    //      location 0 : position: vec4 vertex array of positions of model
    //      location 4 : transform: mat4 array of transforms.. 
    //      uniform block 0 : GL MatrixCalc
    // Out:
    //      gl_Position
    //      location 0 : vs_color is based on instance ID mostly for debugging
    //      location 1 : modelpos

    public class GLPLVertexShaderModelCoordWithMatrixTranslation : GLShaderPipelineShadersBase
    {
        public string Code()
        {
            return
@"
#version 450 core
#include UniformStorageBlocks.matrixcalc.glsl
layout (location = 0) in vec4 position;
layout (location = 4) in mat4 transform;

out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

layout (location = 0) out vec4 vs_color;
layout (location = 1) out vec3 modelpos;

void main(void)
{
    modelpos = position.xyz;
    vs_color = vec4(gl_InstanceID*0.2+0.2,gl_InstanceID*0.2+0.2,0.5+gl_VertexID*0.1,1.0);       // colour may be thrown away if required..
	gl_Position = mc.ProjectionModelMatrix * transform * position;        // order important
}
";
        }

        public GLPLVertexShaderModelCoordWithMatrixTranslation()
        {
            CompileLink(ShaderType.VertexShader, Code(), auxname: GetType().Name);
        }
    }


    // Pipeline shader, Matrix Translation, Tex out, image selection out, Quads in, no vertex input, no tex input, Lookat control
    // Requires:
    //      no vertex input, its auto gen to y=0, x=+/-1, z = +/-1
    //      uniform block 0 : GL MatrixCalc
    //      uniform 4 : transform: mat4 array of transforms, one per instance 
    //              [col=3,row=0] is the image index, 
    //              [col=3,row=1] 0 rotate as per matrix, 1 means look at in azimuth, 2 look at in elevation and azimuth, -1 means cull primitive
    // Out:
    //      location 0 : vs_textureCoordinate
    //      location 2 : image index to use
    //      gL_Position

    public class GLPLVertexShaderQuadTextureWithMatrixTranslation : GLShaderPipelineShadersBase
    {
        public string Code()
        {
            return
@"
    #version 450 core
    #include UniformStorageBlocks.matrixcalc.glsl
    #include Shaders.Functions.trig.glsl
    #include Shaders.Functions.mat4.glsl

    layout (location = 4) in mat4 transform;

    out gl_PerVertex {
            vec4 gl_Position;
            float gl_PointSize;
            float gl_ClipDistance[];
            float gl_CullDistance[];
        };

    layout( location = 0) out vec2 vs_textureCoordinate;
    layout (location = 2) out VS_OUT
    {
        flat int vs_index;      // not sure why structuring is needed..
    } vs;

    vec4 vertex[] = { vec4(-1,0,1,1), vec4(-1,0,-1,1), vec4(1,0,-1,1), vec4(1,0,1,1)};
    vec2 tex[] = { vec2(0,0), vec2(0,1), vec2(1,1), vec2(1,0)};

    void main(void)
    {
        mat4 tx = transform;
        vs.vs_index = int(tx[0][3]);
        tx[0][3] = 0;

        float ctrl = tx[1][3];

        if ( ctrl >= 1 )    // very confused over this, this is row/column order, but the info says its col/row order
        {
            vec3 worldposition = vec3(tx[3][0],tx[3][1],tx[3][2]);      // extract world position
            vec3 scale = vec3(tx[0][0],tx[1][1],tx[2][2]);              // extrace scale only

            vec2 dir = AzEl(worldposition,mc.EyePosition.xyz);      // y = azimuth

            if ( ctrl == 2 )
            {
                tx = mat4rotateXthenY(dir.x,PI-dir.y);              // rotate the flat image to vertical using dir.x (0 = on top, 90 = straight on, etc) then rotate by 180-dir (0 if directly facing from neg z)
            }
            else
            {
                tx = mat4rotateXthenY(PI/2,PI-dir.y);               // rotate 90 up and by azimuth
            }

            tx = mat4translationscale(tx,worldposition,scale);      // apply stored world pos and scaling

            gl_Position = mc.ProjectionModelMatrix * tx * vertex[gl_VertexID];        // order important
        }
        else if ( ctrl < 0 )
        {
            gl_CullDistance[0] = -1;        // all vertex culled
        }
        else
            gl_Position = mc.ProjectionModelMatrix * tx * vertex[gl_VertexID];        // order important

        vs_textureCoordinate = tex[gl_VertexID];
    }
    ";
        }

        public GLPLVertexShaderQuadTextureWithMatrixTranslation()
        {
            CompileLink(ShaderType.VertexShader, Code(), auxname: GetType().Name);
        }
    }


          
    // Pipeline shader, Translation, Texture
    // Requires:
    //      location 0 : position: vec4 vertex array of positions model coords
    //      location 1 : vec2 texture co-ordinates
    //      location 4 : transform: mat4 array of transforms
    //      uniform block 0 : GL MatrixCalc
    // Out:
    //      location 0 : vs_textureCoordinate
    //      location 1 : modelpos
    //      location 2 : instance count
    //      gl_Position

    public class GLPLVertexShaderTextureModelCoordWithMatrixTranslation : GLShaderPipelineShadersBase
    {
        public string Code()
        {
            return
@"
#version 450 core

#include UniformStorageBlocks.matrixcalc.glsl

layout (location = 0) in vec4 position;
layout (location = 1) in vec2 texco;
layout (location = 4) in mat4 transform;

out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

layout(location = 0) out vec2 vs_textureCoordinate;
layout (location = 1) out vec3 modelpos;
layout (location = 2) out VS_OUT
{
    flat int vs_instanced;
} vs_out;


void main(void)
{
    modelpos = position.xyz;
	gl_Position = mc.ProjectionModelMatrix * transform * position;        // order important
    vs_textureCoordinate = texco;
    vs_out.vs_instanced = gl_InstanceID;
}
";
        }

        public GLPLVertexShaderTextureModelCoordWithMatrixTranslation()
        {
            CompileLink(ShaderType.VertexShader, Code(), auxname: GetType().Name);
        }
    }

    // Pipeline shader, Translation, Color, Common transform, Object transform
    // Requires:
    //      location 0 : position: vec4 vertex array of positions
    //      location 1 : vec4 colours of vertexs
    //      uniform 0 : GL MatrixCalc
    //      uniform 22 : objecttransform: mat4 array of transforms
    //      uniform 23 : commontransform: mat4 array of transforms
    // Out:
    //      location 0 : vs_textureCoordinate
    //      gl_Position

    public class GLPLVertexShaderColorModelCoordWithObjectCommonTranslation : GLShaderPipelineShadersBase
    {
        public GLRenderDataTranslationRotation Transform { get; set; }           // only use this for rotation - position set by object data

        public string Code()
        {
            return

@"
#version 450 core
#include UniformStorageBlocks.matrixcalc.glsl

layout (location = 0) in vec4 position;
layout(location = 1) in vec4 color;

layout (location = 22) uniform  mat4 objecttransform;
layout (location = 23) uniform  mat4 commontransform;

out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

layout (location= 0) out vec4 vs_color;

void main(void)
{
	gl_Position = mc.ProjectionModelMatrix * objecttransform *  commontransform * position;        // order important
	vs_color = color;                                                   // pass to fragment shader
}
";
        }

        public GLPLVertexShaderColorModelCoordWithObjectCommonTranslation()
        {
            Transform = new GLRenderDataTranslationRotation();
            CompileLink(ShaderType.VertexShader, Code(), auxname: GetType().Name);
        }

        public override void Start()
        {
            base.Start();
            Matrix4 t = Transform.Transform;
            GL.ProgramUniformMatrix4(Id, 23, false, ref t);
            OpenTKUtils.GLStatics.Check();
        }
    }

    // Pipeline shader, Translation, Texture, Common transform, Object transform
    // Requires:
    //      location 0 : position: vec4 vertex array of positions
    //      location 1 : vec2 texture co-ords
    //      uniform 0 : GL MatrixCalc
    //      uniform 22 : objecttransform: mat4 array of transforms
    //      uniform 23 : commontransform: mat4 array of transforms
    // Out:
    //      location 0: vs_textureCoordinate
    //      gl_Position

    public class GLPLVertexShaderTextureModelCoordsWithObjectCommonTranslation : GLShaderPipelineShadersBase
    {
        public GLRenderDataTranslationRotation Transform { get; set; }           // only use this for rotation - position set by object data

        public string Code()
        {
            return

@"
#version 450 core
#include UniformStorageBlocks.matrixcalc.glsl

layout (location = 0) in vec4 position;
layout(location = 1) in vec2 texco;
layout (location = 22) uniform  mat4 objecttransform;
layout (location = 23) uniform  mat4 commontransform;

out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

layout(location = 0) out vec2 vs_textureCoordinate;

void main(void)
{
	gl_Position = mc.ProjectionModelMatrix * objecttransform *  commontransform * position;        // order important
    vs_textureCoordinate = texco;
}
";
        }

        public GLPLVertexShaderTextureModelCoordsWithObjectCommonTranslation()
        {
            Transform = new GLRenderDataTranslationRotation();
            CompileLink(ShaderType.VertexShader, Code(), auxname: GetType().Name);
        }

        public override void Start()
        {
            base.Start();
            Matrix4 t = Transform.Transform;
            GL.ProgramUniformMatrix4(Id, 23, false, ref t);
            OpenTKUtils.GLStatics.Check();
        }
    }
}

