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
    public partial class ShaderTestVolumetric5 : Form
    {
        private OpenTKUtils.WinForm.GLWinFormControl glwfc;
        private Controller3D gl3dcontroller;

        private Timer systemtimer = new Timer();

        public ShaderTestVolumetric5()
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
            //string s = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message);
            //System.Diagnostics.Debug.WriteLine("{0} {1} {2} {3} {4} {5}", source, type, id, severity, length, s);
           //  s = null;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Closed += ShaderTest_Closed;

            //GLStatics.EnableDebug(DebugProc);

            gl3dcontroller = new Controller3D();
            gl3dcontroller.PaintObjects = ControllerDraw;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 1f;
            gl3dcontroller.MatrixCalc.PerspectiveFarZDistance = 500000f;
            gl3dcontroller.ZoomDistance = 500F;

            gl3dcontroller.KeyboardTravelSpeed = (ms,eyedist) =>
            {
                return (float)ms * 20.0f;
            };

            //gl3dcontroller.MatrixCalc.InPerspectiveMode = false;
            gl3dcontroller.Start(glwfc,new Vector3(0, 0, 0), new Vector3(135f, 0, 0), 0.01F);

            items.Add( new GLColourShaderWithWorldCoord(), "COSW");
            GLRenderControl rl1 = GLRenderControl.Lines(1);

            float h = 0;
            {
                rObjects.Add(items.Shader("COSW"),    // horizontal
                             GLRenderableItem.CreateVector4Color4(items, rl1,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(-35000, h, -35000), new Vector3(-35000, h, 35000), new Vector3(1000, 0, 0), 70),
                                                        new Color4[] { Color.Gray })
                                   );

                rObjects.Add(items.Shader("COSW"),
                             GLRenderableItem.CreateVector4Color4(items, rl1,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(-35000, h, -35000), new Vector3(35000, h, -35000), new Vector3(0, 0, 1000), 70),
                                                        new Color4[] { Color.Gray })
                                   );
            }

            int hsize = 35000, vsize = 2000, zsize = 35000;
            {
                int left = -hsize, right = hsize, bottom = -vsize, top = +vsize, front = -zsize, back = zsize;
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

            {
                Bitmap[] numbers = new Bitmap[70];
                Matrix4[] numberpos = new Matrix4[numbers.Length];

                Font fnt = new Font("Arial", 20);

                for (int i = 0; i < numbers.Length; i++)
                {
                    int v = -35000 + i * 1000;
                    numbers[i] = new Bitmap(100, 100);
                    BitMapHelpers.DrawTextCentreIntoBitmap(ref numbers[i], v.ToString(), fnt, Color.Red, Color.AliceBlue);
                    numberpos[i] = Matrix4.CreateScale(1);
                    numberpos[i] *= Matrix4.CreateRotationX(-25f.Radians());
                    numberpos[i] *= Matrix4.CreateTranslation(new Vector3(35500, 0, v));
                }

                GLTexture2DArray array = new GLTexture2DArray(numbers, ownbitmaps: true);
                items.Add( array, "Nums");
                items.Add(new GLShaderPipeline(new GLPLVertexShaderTextureModelCoordWithMatrixTranslation(), new GLPLFragmentShaderTexture2DIndexed(0)), "IC-2");
                items.Shader("IC-2").StartAction += (s) => { items.Tex("Nums").Bind(1); GL.Disable(EnableCap.CullFace); };
                items.Shader("IC-2").FinishAction += (s) => { GL.Enable(EnableCap.CullFace); };

                // investigate why its wrapping when we asked for it TexQUAD 1 which should interpolate over surface..

                GLRenderControl rq = GLRenderControl.Quads(cullface: false);
                GLRenderDataTexture rt = new GLRenderDataTexture(items.Tex("Nums"));

                rObjects.Add(items.Shader("IC-2"), "1-b",
                                        GLRenderableItem.CreateVector4Vector2Matrix4(items, rq,
                                                GLShapeObjectFactory.CreateQuad(500.0f), GLShapeObjectFactory.TexQuad, numberpos, rt,
                                                numberpos.Length));
            }

            // bounding box

            boundingbox = new Vector4[]
            {
                new Vector4(-hsize,-vsize,-zsize,1),
                new Vector4(-hsize,vsize,-zsize,1),
                new Vector4(hsize,vsize,-zsize,1),
                new Vector4(hsize,-vsize,-zsize,1),

                new Vector4(-hsize,-vsize,zsize,1),
                new Vector4(-hsize,vsize,zsize,1),
                new Vector4(hsize,vsize,zsize,1),
                new Vector4(hsize,-vsize,zsize,1),
            };

            items.Add( new ShaderV2(), "V2");
            GLRenderControl rv = GLRenderControl.ToTri(OpenTK.Graphics.OpenGL4.PrimitiveType.Points);
            galaxy = GLRenderableItem.CreateNullVertex(rv);
            rObjects.Add(items.Shader("V2"), galaxy);

            dataoutbuffer = items.NewStorageBlock(5);
            dataoutbuffer.AllocateBytes(sizeof(float) * 4 * 32, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicRead);    // 32 vec4 back

            atomicbuffer = items.NewAtomicBlock(6);
            atomicbuffer.AllocateBytes(sizeof(float) * 32, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicCopy);

            dataoutbuffer = items.NewStorageBlock(5);
            dataoutbuffer.AllocateBytes(sizeof(float) * 4 * 256, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicRead);    // 32 vec4 back

            volumetricblock = new GLVolumetricUniformBlock();
            items.Add(volumetricblock, "VB");

            items.Add( new GLMatrixCalcUniformBlock(), "MCUB");     // create a matrix uniform block 






        }

        Vector4[] boundingbox;

        GLStorageBlock dataoutbuffer;
        GLVolumetricUniformBlock volumetricblock;
        GLAtomicBlock atomicbuffer;
        GLRenderableItem galaxy;

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(GLMatrixCalc mc, long time)
        {
            ((GLMatrixCalcUniformBlock)items.UB("MCUB")).Set(gl3dcontroller.MatrixCalc);        // set the matrix unform block to the controller 3d matrix calc.

            galaxy.InstanceCount = volumetricblock.Set(gl3dcontroller.MatrixCalc, boundingbox, 100);        // set up the volumentric uniform

            dataoutbuffer.ZeroBuffer();
            atomicbuffer.ZeroBuffer();

            rObjects.Render(glwfc.RenderState, gl3dcontroller.MatrixCalc);

            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

            Vector4[] databack = dataoutbuffer.ReadVector4(0, 10);

            //System.Diagnostics.Debug.WriteLine("avg {0} txtavg {1}", databack[0].ToStringVec(), databack[1].ToStringVec());

            for (int i = 0; i < databack.Length; i += 1)
            {
                System.Diagnostics.Debug.WriteLine("db " + databack[i].ToStringVec());
            }

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " dir " + gl3dcontroller.PosCamera.CameraDirection + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance + " Zoom " + gl3dcontroller.PosCamera.ZoomFactor;
        }


        public class ShaderV2 : GLShaderStandard
        {
            string vcode =
            @"
            #version 450 core

            out int instance;

            void main(void)
            {
                instance = gl_InstanceID;
            }
            ";

            string fcode = @"
            #version 450 core
            #include OpenTKUtils.GL4.Shaders.Functions.distribution.glsl
            out vec4 color;

            in vec3 vs_texcoord;
            in vec4 vs_color;

            void main(void)
            {
                float xc = abs(vs_texcoord.x-0.5)*2;    // so +/-1
                float zc = abs(vs_texcoord.z-0.5)*2;
                float h = abs(vs_texcoord.y-0.5)*2;       // from 0 to 1
                float m = sqrt(xc*xc+zc*zc);        // 0 at centre, 0.707 at maximum

                float gd = gaussian(m,0,0.2) ;  // deviation around centre, 1 = centre

                bool colourit = false;
                
                gd = max(gd,0.1);

                if ( h < gd && m < 1)     // 0.5 sets the size of the disk, this is touching the bounding box
                    colourit = true;

                if ( colourit)
                {
                    color = vec4(vs_texcoord,0.05);
                }
                else
                    discard;
            }
            ";

            public ShaderV2()
            {
                CompileLink(vertex: vcode, frag: fcode, geo: "#include TestOpenTk.Volumetrics.volumetricgeo5.glsl");
            }
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

