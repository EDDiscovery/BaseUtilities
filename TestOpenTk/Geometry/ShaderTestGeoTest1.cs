/*
 * Copyright 2019 Robbyxp1 @ github.com
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

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTKUtils.GL4;
using OpenTKUtils.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTKUtils;

namespace TestOpenTk
{
    public partial class ShaderTestGeoTest1 : Form
    {
        private OpenTKUtils.WinForm.GLWinFormControl glwfc;
        private Controller3D gl3dcontroller;

        private Timer systemtimer = new Timer();

        public ShaderTestGeoTest1()
        {
            InitializeComponent();
            glwfc = new OpenTKUtils.WinForm.GLWinFormControl(glControlContainer);

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
            systemtimer.Start();
        }

        GLRenderProgramSortedList rObjects = new GLRenderProgramSortedList();
        GLRenderProgramSortedList rObjects2 = new GLRenderProgramSortedList();
        GLItemsList items = new GLItemsList();

        public class ShaderT3 : GLShaderStandard
        {
            string vcode = @"

#version 450 core

layout( std140, binding=5) buffer storagebuffer
{
    vec4 vertex[];
};

void main(void)
{
    vec4 p = vertex[gl_VertexID];
    gl_Position = p;
}
";

            string gcode = @"
#version 450 core
#include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl
layout (points) in;
layout (points) out;
layout (max_vertices=2) out;
out vec4 vs_color;

layout (binding = 1, std430) buffer Positions
{
    vec4 rejectedpos[];
};

layout (binding = 2, std430) buffer Count
{
    uint count;
};

void main(void)
{
    int i;
    for( i = 0 ; i < gl_in.length() ; i++)
    {
        if ( (gl_PrimitiveIDIn & 1) != 0)
            gl_PointSize = 5;
        else
            gl_PointSize = gl_PrimitiveIDIn+5;
        gl_Position = mc.ProjectionModelMatrix * gl_in[i].gl_Position;
        vs_color = vec4(gl_PrimitiveIDIn*0.05,0.4,gl_PrimitiveIDIn*0.05,1.0);

        if (gl_PrimitiveIDIn != 9 && gl_PrimitiveIDIn != 13 )
        {
            EmitVertex();
        }
        else
        {
            uint ipos = atomicAdd(count,1);

            if ( ipos < 128 )
                rejectedpos[ipos] = gl_in[i].gl_Position;
        }

        if ( gl_PrimitiveIDIn ==8  )        // on 8, emit an extra
        {
            gl_PointSize = 50;
            vec4 p = gl_in[i].gl_Position;
            gl_Position = mc.ProjectionModelMatrix *  vec4(p.x-4,p.y,p.z,1.0);
            vs_color = vec4(1.0-gl_PrimitiveIDIn*0.05,0.4,gl_PrimitiveIDIn*0.05,1.0);
            EmitVertex();
        }

    }

    EndPrimitive();
}
";

            string fcode = @"
#version 450 core
out vec4 color;
in vec4 vs_color;

void main(void)
{
    color = vs_color;
}
";


            public ShaderT3()
            {
                CompileLink(vertex: vcode, frag:fcode, geo:gcode);
            }
        }

        // Demonstrate buffer feedback AND geo shader add vertex/dump vertex

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            gl3dcontroller = new Controller3D();
            gl3dcontroller.PaintObjects = ControllerDraw;
            gl3dcontroller.ZoomDistance = 20F;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.Start(glwfc,new Vector3(0, 0, 0), new Vector3(170f, 0, 0f), 1F);

            gl3dcontroller.KeyboardTravelSpeed = (ms,eyedist) =>
            {
                return (float)ms / 20.0f;
            };

            GLStorageBlock storagebuffer = new GLStorageBlock(5);           // new storage block on binding index 5 to provide vertexes
            Vector4[] vertexes = new Vector4[16];
            for (int v = 0; v < vertexes.Length; v++)
                vertexes[v] = new Vector4(v % 4, 0, v / 4, 1);
            storagebuffer.AllocateFill(vertexes);

            vecoutbuffer = new GLStorageBlock(1);           // new storage block on binding index 1 for vector out
            vecoutbuffer.AllocateBytes(sizeof(float) * 4 * 128, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicCopy);       // set size of vec buffer

            countbuffer = new GLStorageBlock(2);           // new storage block on binding index 2 for count out
            countbuffer.AllocateBytes(sizeof(int), OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicRead);       // set size to a int.

            items.Add(new ShaderT3(), "Shader");            // geo shader
            GLRenderControl ri = GLRenderControl.PointsByProgram();
            rObjects.Add(items.Shader("Shader"), "T1", new GLRenderableItem(ri, vertexes.Length, null, null, 1));


            items.Add(new GLShaderPipeline(new GLPLVertexShaderWorldCoord(), new GLPLFragmentShaderFixedColour(new Color4(0.9f, 0.0f, 0.0f, 1.0f))), "ResultShader");
            GLRenderControl rs = GLRenderControl.Points(30);
            redraw = GLRenderableItem.CreateVector4(items, rs, vecoutbuffer, 0);
            rObjects2.Add(items.Shader("ResultShader"), redraw);

            items.Add( new GLMatrixCalcUniformBlock(), "MCUB");     // def binding of 0

            Closed += ShaderTest_Closed;
        }

        GLRenderableItem redraw;
        GLStorageBlock countbuffer;
        GLStorageBlock vecoutbuffer;

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(OpenTKUtils.GLMatrixCalc mc, long time)
        {
            // System.Diagnostics.Debug.WriteLine("Draw eye " + gl3dcontroller.MatrixCalc.EyePosition + " to " + gl3dcontroller.Pos.Current);

            GLMatrixCalcUniformBlock mcub = (GLMatrixCalcUniformBlock)items.UB("MCUB");
            mcub.Set(gl3dcontroller.MatrixCalc);

            countbuffer.ZeroBuffer();

            rObjects.Render(glwfc.RenderState, gl3dcontroller.MatrixCalc);
            GL.MemoryBarrier(MemoryBarrierFlags.VertexAttribArrayBarrierBit);

            int count = countbuffer.ReadInt(0);
            Vector4[] d = vecoutbuffer.ReadVector4(0, count);
            for (int i = 0; i < count; i++)
            {
                System.Diagnostics.Debug.WriteLine(i + " = " + d[i]);
            }

            redraw.DrawCount = count;                                               // render passed back ones using red from vecoutbuffer
            rObjects2.Render(glwfc.RenderState, gl3dcontroller.MatrixCalc);
        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboardSlews(true, OtherKeys);
            gl3dcontroller.Redraw();
        }

        private void OtherKeys( OpenTKUtils.Common.KeyboardMonitor kb )
        {
        }
    }
}


