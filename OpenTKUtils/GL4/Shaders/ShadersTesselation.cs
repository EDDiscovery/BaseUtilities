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
    // Simple rendered with optional rot/translation

    public class GLTesselationShadersExample : GLShaderProgramBase
    {
        string vert =
        @"
#version 450 core
layout (location = 0) in vec4 position;

void main(void)
{
	gl_Position =position;        // order important
}
";

        string tcs =

        @"
#version 450 core

layout (vertices = 4) out;

void main(void)
{
    if ( gl_InvocationID == 0 )
    {
        // gl_TessLevelInner[0] = 5.0;      
        // gl_TessLevelOuter[0] = 8.0;      
        //gl_TessLevelOuter[1] = 8.0;      
        //gl_TessLevelOuter[2] = 8.0;      

        gl_TessLevelInner[0] =  9.0;
        gl_TessLevelInner[1] =  7.0;
        gl_TessLevelOuter[0] =  3.0;
        gl_TessLevelOuter[1] =  5.0;
        gl_TessLevelOuter[2] =  3.0;
        gl_TessLevelOuter[3] =  5.0;
    }

    gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;
}
";

        string tes =

@"
#version 450 core

layout (quads) in; 

layout (location = 20) uniform  mat4 projectionmodel;

void main(void)
{
    vec4 p1 = mix(gl_in[0].gl_Position, gl_in[1].gl_Position, gl_TessCoord.x);  
    vec4 p2 = mix(gl_in[2].gl_Position, gl_in[3].gl_Position, gl_TessCoord.x); 

    //vec4 p1 = mix(gl_in[0].gl_Position, gl_in[1].gl_Position, gl_TessCoord.x);       
    //vec4 p2 = mix(gl_in[2].gl_Position, gl_in[3].gl_Position, gl_TessCoord.x);      
    vec4 pos = mix(p1, p2, gl_TessCoord.y);                                     
    //gl_Position = projectionmodel * gl_Position;
    
    //gl_Position = projectionmodel * gl_in[0].gl_Position;

    //vec4 pos = (gl_TessCoord.x * gl_in[0].gl_Position) +   
    //          (gl_TessCoord.y * gl_in[1].gl_Position) +   
    //          (gl_TessCoord.z * gl_in[2].gl_Position);    

    gl_Position = projectionmodel * pos;

}
";

        string frag =

@"
#version 450 core
out vec4 color;

void main(void)
{
    color = vec4(0.9,0.9,0.9,1.0);
}
";



        public GLTesselationShadersExample()
        {
            Compile(vertex: vert, tcs: tcs, tes: tes, frag: frag);
        }

        public override void Start(Common.MatrixCalc c)
        {
            GL.UseProgram(Id);           // use this
            Matrix4 projmodel = c.ProjectionModelMatrix;
            GL.ProgramUniformMatrix4(Id, 20, false, ref projmodel);
            GL4Statics.PatchSize(4);
            GL4Statics.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        }
    }
}
