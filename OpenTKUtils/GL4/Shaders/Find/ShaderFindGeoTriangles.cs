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

using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    // Geo shader, find triangle under cursor
    // combine with your chosen vertex shader feeding in ProjectionModelMatrix values
    // using a RenderableItem
    // call SetScreenCoords before render executes 
    // call GetResult after Executing the shader/RI combo
    // Inputs gl_in positions
    // Inputs (=2) instance[] instance number. 

    public class GLPLGeoShaderFindTriangles : GLShaderPipelineShadersBase
    {
        public string Code(bool passthru)
        {
            return
@"
#version 450 core
#include UniformStorageBlocks.matrixcalc.glsl

layout (triangles) in;               // triangles come in
layout (triangle_strip) out;        // norm op is not to sent them on
layout (max_vertices=3) out;	    // 1 triangle max

layout (location = 10) uniform vec4 screencoords;
layout (location = 11) uniform float pointdist;

in gl_PerVertex
{
    vec4 gl_Position;
    float gl_PointSize;
    float gl_ClipDistance[];
   
} gl_in[];

out gl_PerVertex 
{
    vec4 gl_Position;
    float gl_PointSize;
    float gl_ClipDistance[];
};

// shows how to pass thru modelpos..
layout (location = 1) in vec3[] modelpos;
out MP
{
    layout (location = 1) out vec3 modelpos;
} MPOUT;

layout (location = 2) in int instance[];

float PMSquareS(vec4 l1, vec4 l2, vec4 p)
{
    return sign((p.x - l1.x) * (l2.y - l1.y) - (l2.x - l1.x) * (p.y - l1.y));
}

const int bindingoutdata = 20;
layout (binding = bindingoutdata, std430) buffer Positions      // StorageBlock note - buffer
{
    uint count;
    vec4 values[];
};

const int maximumresults = 1;       // is overriden by compiler feed in const
const bool forwardsfacing = true;   // compiler overriden

void main(void)
{
" + ((passthru) ? // pass thru is for testing purposes only
@"
        gl_Position = gl_in[0].gl_Position;
        MPOUT.modelpos =modelpos[0];
        EmitVertex();

        gl_Position = gl_in[1].gl_Position;
        MPOUT.modelpos =modelpos[1];
        EmitVertex();

        gl_Position = gl_in[2].gl_Position;
        MPOUT.modelpos =modelpos[2];
        EmitVertex();
        EndPrimitive();
" : "") +
@"
        vec4 p0 = gl_in[0].gl_Position / gl_in[0].gl_Position.w;        // normalise w to produce screen pos in x/y, +/- 1
        vec4 p1 = gl_in[1].gl_Position / gl_in[1].gl_Position.w;        
        vec4 p2 = gl_in[2].gl_Position / gl_in[2].gl_Position.w;

        if ( !forwardsfacing || PMSquareS(p0,p1,p2) < 0 )     // if wound okay, so its forward facing (p0->p1 vector, p2 is on the right)
        {
            // only check for approximate cursor position on first triangle of primitive (if small, all would respond)

            if ( gl_PrimitiveIDIn == 0 && abs(p0.x-screencoords.x) < pointdist && abs(p0.y-screencoords.y) < pointdist )
            {
                uint ipos = atomicAdd(count,1);
                if ( ipos < maximumresults )
                {
                    float avgz = (p0.z+p1.z+p2.z)/3;
                    values[ipos] = vec4(gl_PrimitiveIDIn,instance[0],avgz,1000+ipos);
                }
            }
            else 
            {
                if ( p0.z > 0 && p1.z > 0 && p2.z > 0 && p0.z <1 && p1.z < 1 && p2.z < 1)       // all must be on screen
                {
                    float p0s = PMSquareS(p0,p1,screencoords);      // perform point to line detection on all three lines
                    float p1s = PMSquareS(p1,p2,screencoords);
                    float p2s = PMSquareS(p2,p0,screencoords);

                    if ( p0s == p1s && p0s == p2s)      // all signs agree, its within the triangle
                    {
                        uint ipos = atomicAdd(count,1);     // this keeps on going even if we exceed max results, the results are just not stored
                        if ( ipos < maximumresults )
                        {
                            float avgz = (p0.z+p1.z+p2.z)/3;
                            values[ipos] = vec4(gl_PrimitiveIDIn,instance[0],avgz,ipos);
                        }
                    }
                }   
            }
        }
}
";
        }

        public GLPLGeoShaderFindTriangles(int resultoutbufferbinding,int maximumresultsp, bool forwardfacing = true)
        {
            maximumresults = maximumresultsp;
            vecoutbuffer = new GLStorageBlock(resultoutbufferbinding);      // buffer is disposed by overriden dispose below.
            vecoutbuffer.AllocateBytes(16 + sizeof(float) * 4 * maximumresults );
            CompileLink(ShaderType.GeometryShader, Code(false), auxname: GetType().Name,
                                constvalues:new object[] { "bindingoutdata", resultoutbufferbinding, "maximumresults", maximumresults, "forwardfacing", forwardfacing});
        }

        public void SetScreenCoords(Point p, Size s, int margin = 10)
        {
            Vector4 v = new Vector4(((float)p.X) / (s.Width / 2) - 1.0f, (1.0f - (float)p.Y / (s.Height / 2)), 0, 0);
            GL.ProgramUniform4(Id, 10, v);
            float pixd = (float)(margin / (float)(s.Width+s.Height/2/2));
            System.Diagnostics.Debug.WriteLine("Set CP {0} Pixd {1}", v , pixd);
            GL.ProgramUniform1(Id, 11, pixd);
        }

        public override void Start()
        {
            base.Start();
            vecoutbuffer.ZeroBuffer();
        }

        // returns null or vec4 ( PrimitiveID, InstanceID, average Z of triangle points, result index)
        public Vector4[] GetResult()
        {
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            vecoutbuffer.StartRead(0);
            int count = Math.Min(vecoutbuffer.ReadInt(),maximumresults);       // atomic counter keeps on going if it exceeds max results, so limit to it

            Vector4[] d = null;

            if (count > 0)
            {
                d = vecoutbuffer.ReadVector4s(count);      // align 16 for vec4
                Array.Sort(d, delegate (Vector4 left, Vector4 right) { return left.Z.CompareTo(right.Z); });
            }

            vecoutbuffer.StopReadWrite();
            return d;
        }

        public override void Dispose()
        {
            vecoutbuffer.Dispose();
            base.Dispose();
        }

        private GLStorageBlock vecoutbuffer;
        private int maximumresults;
    }

}

