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
using OpenTKUtils.GL4;
using OpenTK.Graphics.OpenGL4;
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
    public partial class ShaderTestUniforms : Form
    {
        private OpenTKUtils.WinForm.GLWinFormControl glwfc;
        private Controller3D gl3dcontroller;

        private Timer systemtimer = new Timer();

        public ShaderTestUniforms()
        {
            InitializeComponent();
            glwfc = new OpenTKUtils.WinForm.GLWinFormControl(glControlContainer);

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
            systemtimer.Start();
        }

        GLRenderProgramSortedList rObjects = new GLRenderProgramSortedList();
        GLItemsList items = new GLItemsList();

        //************************************
        // demos std 140 and 430 effect on uniforms

        public class GLPLVertexShaderModelCoordWithWorldTranslationCommonModelTranslation2 : GLShaderPipelineShadersBase
        {
            public string Code()       // with transform, object needs to pass in uniform 22 the transform
            {
                return
    @"
#version 450 core
#include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl
out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

layout (location = 0) in vec4 modelposition;
layout (location = 1) in vec4 worldposition;            // instanced
layout (location = 22) uniform  mat4 transform;

layout (location = 1) out vec3 modelpos;
layout (location = 2) out int instance;

out vec4 color;

const int bindingoutdata = 20;
layout (binding = bindingoutdata,std140) uniform Positions
{
    int count;      // index 0
    float values[200];  // index 16,32,48 etc
    //int values[200];
};

const int bindingoutdata2 = 21;
layout (binding = bindingoutdata2,std430) buffer Positions2
{
    int count2;      // index 0
    float values2[200];  
};


void main(void)
{
    modelpos = modelposition.xyz;
    vec4 modelrot = transform * modelposition;
    vec4 wp = modelrot + worldposition;
	gl_Position = mc.ProjectionModelMatrix * wp;        // order important
    instance = gl_InstanceID;

    //color = values[gl_InstanceID];         //vec4(v[gl_InstanceID],0,0.6,1);
    //color = vec4(values[gl_InstanceID]/255.0,0,0,1);
    //color = vec4(values[gl_InstanceID],0,0,1);
    color = vec4(values[gl_InstanceID],values2[gl_InstanceID],0,1);
}
";
            }

            public Matrix4 ModelTranslation { get; set; } = Matrix4.Identity;

            public GLPLVertexShaderModelCoordWithWorldTranslationCommonModelTranslation2()
            {
                CompileLink(ShaderType.VertexShader, Code(), auxname: GetType().Name);
            }

            public override void Start()
            {
                Matrix4 a = ModelTranslation;
                GL.ProgramUniformMatrix4(Id, 22, false, ref a);
                OpenTKUtils.GLStatics.Check();
            }
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            gl3dcontroller = new Controller3D();
            gl3dcontroller.PaintObjects = ControllerDraw;
            gl3dcontroller.ZoomDistance = 20F;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.Start(glwfc, new Vector3(0, 0, 0), new Vector3(180f, 0, 0f), 1F);

            gl3dcontroller.KeyboardTravelSpeed = (ms, eyedist) =>
            {
                return (float)ms / 50.0f;
            };


            items.Add( new GLMatrixCalcUniformBlock(), "MCUB");     // def binding of 0

            if (true)
            {
                GLRenderControl lines = GLRenderControl.Lines(1);

                items.Add(new GLColourShaderWithWorldCoord(), "COSW");

                rObjects.Add(items.Shader("COSW"),
                             GLRenderableItem.CreateVector4Color4(items, lines,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(-100, 0, 100), new Vector3(10, 0, 0), 21),
                                                        new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                                   );


                rObjects.Add(items.Shader("COSW"),
                             GLRenderableItem.CreateVector4Color4(items, lines,
                                   GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(100, 0, -100), new Vector3(0, 0, 10), 21),
                                                             new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green }));
            }

            var vert = new GLPLVertexShaderModelCoordWithWorldTranslationCommonModelTranslation2();
            var frag = new GLPLFragmentShaderColour();
            var shader = new GLShaderPipeline(vert, frag);
            items.Add(shader, "TRI");

            var vecp4 = new Vector4[] { new Vector4(0, 0, 0, 1), new Vector4(10, 0, 0, 1), new Vector4(10, 0, 10, 1) ,
                                    new Vector4(-20, 0, 0, 1), new Vector4(-10, 0, 0, 1), new Vector4(-10, 0, 10, 1)
            };

            var wpp4 = new Vector4[] { new Vector4(0, 0, 0, 0), new Vector4(0, 0, 12, 0) };

            GLRenderControl rc = GLRenderControl.Tri();

            rObjects.Add(items.Shader("TRI"), "scopen", GLRenderableItem.CreateVector4Vector4Buf2(items, rc, vecp4, wpp4, ic:2, seconddivisor:1));

            var uniformbuf = new GLUniformBlock(20);      
            uniformbuf.AllocateBytes(1024);
            uniformbuf.StartWrite(0);
            uniformbuf.Write(0);
            uniformbuf.Write(new float[2] { 0.5f, 0.9f });      // demo vec4 alignment and stride
            uniformbuf.StopReadWrite();

            var storagebuf = new GLStorageBlock(21,true);      
            storagebuf.AllocateBytes(1024);
            storagebuf.StartWrite(0);
            storagebuf.Write(0);
            storagebuf.Write(new float[2] { 0.2f, 0.9f });
            storagebuf.StopReadWrite();

            Closed += ShaderTest_Closed;
        }

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(OpenTKUtils.GLMatrixCalc mc, long time)
        {

            GLMatrixCalcUniformBlock mcub = (GLMatrixCalcUniformBlock)items.UB("MCUB");
            mcub.Set(gl3dcontroller.MatrixCalc);

            rObjects.Render(glwfc.RenderState, gl3dcontroller.MatrixCalc);
            GL.MemoryBarrier(MemoryBarrierFlags.VertexAttribArrayBarrierBit);

        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboardSlews(true, OtherKeys);
        }

        private void OtherKeys( OpenTKUtils.Common.KeyboardMonitor kb )
        {
        }
    }
}


