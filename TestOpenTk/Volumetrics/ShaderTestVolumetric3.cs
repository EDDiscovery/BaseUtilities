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
using OpenTKUtils;
using OpenTKUtils.Common;
using OpenTKUtils.GL4;
using System;
using System.Drawing;
using System.Windows.Forms;

// Demonstrate the volumetric calculations needed to compute a plane facing the user inside a bounding box done inside a geo shader
// this one add on tex coord calculation and using a single tight quad shows its working

namespace TestOpenTk
{
    public partial class ShaderTestVolumetric3 : Form
    {
        private OpenTKUtils.WinForm.GLWinFormControl glwfc;
        private Controller3D gl3dcontroller;

        private Timer systemtimer = new Timer();

        public ShaderTestVolumetric3()
        {
            InitializeComponent();

            glwfc = new OpenTKUtils.WinForm.GLWinFormControl(glControlContainer);

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
            systemtimer.Start();
        }

        GLRenderProgramSortedList rObjects = new GLRenderProgramSortedList();
        GLItemsList items = new GLItemsList();

        /// ////////////////////////////////////////////////////////////////////////////////////////////////////

        public class ShaderV2 : GLShaderStandard
        {
            string vcode =
@"
#version 450 core
#include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl

layout (location = 0) in vec4 position;

out int instance;

void main(void)
{
	gl_Position = position;       
    instance = gl_InstanceID;
}
";

            string fcode = @"
#version 450 core
out vec4 color;
in vec4 vs_color;
in vec3 vs_texcoord;

void main(void)
{
    //color = vs_color;
    color = vec4(vs_texcoord,0.8);
}
";

            public ShaderV2()
            {
                CompileLink(vertex: vcode, frag: fcode, geo: "#include TestOpenTk.Volumetrics.volumetricgeo3.glsl");
            }
        }

        public class GLFixedShader : GLShaderPipeline
        {
            public GLFixedShader(Color c, Action<IGLProgramShader> action = null) : base(action)
            {
                AddVertexFragment(new GLPLVertexShaderWorldCoord(), new GLPLFragmentShaderFixedColour(c));
            }
        }

        public class GLFixedProjectionShader : GLShaderPipeline
        {
            public GLFixedProjectionShader(Color c, Action<IGLProgramShader> action = null) : base(action)
            {
                AddVertexFragment(new GLPLVertexShaderModelViewCoord(), new GLPLFragmentShaderFixedColour(c));
            }
        }

        void DebugProc(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            string s = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message);
            System.Diagnostics.Debug.WriteLine("{0} {1} {2} {3} {4} {5}", source, type, id, severity, length, s);
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Closed += ShaderTest_Closed;

            GLStatics.EnableDebug(DebugProc);

            gl3dcontroller = new Controller3D();
            gl3dcontroller.PaintObjects = ControllerDraw;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.ZoomDistance = 40F;
            gl3dcontroller.MouseRotateAmountPerPixel = 0.05f;
            gl3dcontroller.Start(glwfc,new Vector3(0, 0, 0), new Vector3(90, 0, 0), 1F);

            gl3dcontroller.KeyboardTravelSpeed = (ms,eyedist) =>
            {
                return (float)ms / 100.0f;
            };

            items.Add(new GLColourShaderWithWorldCoord(), "COSW");
            GLRenderControl rl1 = GLRenderControl.Lines(1);

            {

                rObjects.Add(items.Shader("COSW"), "L1",   // horizontal
                             GLRenderableItem.CreateVector4Color4(items, rl1,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(-100, 0, 100), new Vector3(10, 0, 0), 21),
                                                        new Color4[] { Color.Gray })
                                   );


                rObjects.Add(items.Shader("COSW"),    // vertical
                             GLRenderableItem.CreateVector4Color4(items, rl1,
                                   GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(100, 0, -100), new Vector3(0, 0, 10), 21),
                                                             new Color4[] { Color.Gray })
                                   );

            }

            // Number markers using instancing and 2d arrays, each with its own transform
            {
                Bitmap[] numbers = new Bitmap[20];
                Matrix4[] numberpos = new Matrix4[20];

                Font fnt = new Font("Arial", 44);

                for (int i = 0; i < numbers.Length; i++)
                {
                    int v = -100 + i * 10;
                    numbers[i] = new Bitmap(100, 100);
                    BitMapHelpers.DrawTextCentreIntoBitmap(ref numbers[i], v.ToString(), fnt, Color.Red, Color.AliceBlue);
                    numberpos[i] = Matrix4.CreateScale(1);
                    numberpos[i] *= Matrix4.CreateRotationX(-80f.Radians());
                    numberpos[i] *= Matrix4.CreateTranslation(new Vector3(20, 0, v));
                }

                GLTexture2DArray array = new GLTexture2DArray(numbers, ownbitmaps: true);
                items.Add(array, "Nums");
                items.Add(new GLShaderPipeline(new GLPLVertexShaderTextureModelCoordWithMatrixTranslation(), new GLPLFragmentShaderTexture2DIndexed(0)), "IC-2");

                GLRenderControl rq = GLRenderControl.Quads(cullface:false);
                GLRenderDataTexture rt = new GLRenderDataTexture(items.Tex("Nums"));

                rObjects.Add(items.Shader("IC-2"), "1-b",
                                        GLRenderableItem.CreateVector4Vector2Matrix4(items, rq,
                                                GLShapeObjectFactory.CreateQuad(1.0f), GLShapeObjectFactory.TexQuad, numberpos, rt, 
                                                numberpos.Length));
            }

            {
                int left = -40, right = 40, bottom = -20, top = +20, front = -40, back = 40;
                Vector4[] lines2 = new Vector4[]
                {
                new Vector4(left,bottom,front,1),   new Vector4(left,top,front,1),
                new Vector4(left,top,front,1),      new Vector4(right,top,front,1),
                new Vector4(right,top,front,1),     new Vector4(right,bottom,front,1),
                new Vector4(right,bottom,front,1),  new Vector4(left,bottom,front,1),

                new Vector4(left,bottom,back,1),    new Vector4(left,top,back,1),
                new Vector4(left,top,back,1),       new Vector4(right,top,back,1),
                new Vector4(right,top,back,1),      new Vector4(right,bottom,back,1),
                new Vector4(right,bottom,back,1),   new Vector4(left,bottom,back,1),

                new Vector4(left,bottom,front,1),   new Vector4(left,bottom,back,1),
                new Vector4(left,top,front,1),      new Vector4(left,top,back,1),
                new Vector4(right,bottom,front,1),  new Vector4(right,bottom,back,1),
                new Vector4(right,top,front,1),     new Vector4(right,top,back,1),

                };

                items.Add(new GLFixedShader(System.Drawing.Color.Yellow), "LINEYELLOW");
                rObjects.Add(items.Shader("LINEYELLOW"),
                            GLRenderableItem.CreateVector4(items, rl1, lines2));
            }

            items.Add(new ShaderV2(), "V2");

            Vector4[] points = new Vector4[]
            {
                new Vector4(-40,-20,-40,1),     //P0
                new Vector4(+40,+20,+40,1),     //P6
            };

            GLRenderControl rltot = GLRenderControl.ToTri(OpenTK.Graphics.OpenGL4.PrimitiveType.Lines);

            rObjects.Add(items.Shader("V2"), GLRenderableItem.CreateVector4(items, rltot, points, ic: 1));

            dataoutbuffer = items.NewStorageBlock(5);
            dataoutbuffer.AllocateBytes(sizeof(float) * 4 * 32, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicRead);    // 32 vec4 back

            atomicbuffer = items.NewAtomicBlock(6);
            atomicbuffer.AllocateBytes(sizeof(float) * 32, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicCopy);

            items.Add( new GLMatrixCalcUniformBlock(), "MCUB");     // create a matrix uniform block 
        }

        GLStorageBlock dataoutbuffer;
        GLAtomicBlock atomicbuffer;

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(GLMatrixCalc mc, long time)
        {
            ((GLMatrixCalcUniformBlock)items.UB("MCUB")).Set(gl3dcontroller.MatrixCalc);        // set the matrix unform block to the controller 3d matrix calc.

            dataoutbuffer.ZeroBuffer();
            atomicbuffer.ZeroBuffer();

            rObjects.Render(glwfc.RenderState, gl3dcontroller.MatrixCalc);

            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

            Vector4[] databack = dataoutbuffer.ReadVector4(0, 12);

            System.Diagnostics.Debug.WriteLine("avg {0} txtavg {1}", databack[0].ToStringVec(), databack[1].ToStringVec());

            for (int i = 2; i < databack.Length; i += 2)
            {
                System.Diagnostics.Debug.WriteLine("{0} v{1} a{2:0.0} :  {3}", databack[i].X, databack[i].Z, databack[i].Y, databack[i + 1].ToStringVec());
            }

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " dir " + gl3dcontroller.Pos.CameraDirection + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance;
        }

        private void SystemTick(object sender, EventArgs e)
        {
            gl3dcontroller.HandleKeyboardSlewsInvalidate(true, OtherKeys);
        }

        private void OtherKeys(OpenTKUtils.Common.KeyboardMonitor kb)
        {
            if (kb.HasBeenPressed(Keys.F1, OpenTKUtils.Common.KeyboardMonitor.ShiftState.None))
            {
                int times = 1000;
                System.Diagnostics.Debug.WriteLine("Start test");
                long tickcount = gl3dcontroller.Redraw(times);
                System.Diagnostics.Debug.WriteLine("Redraw {0} ms per {1}", tickcount, (float)tickcount/(float)times);
            }
        }
    }
}

