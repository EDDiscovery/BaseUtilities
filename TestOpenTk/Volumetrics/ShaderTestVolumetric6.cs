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
    public partial class ShaderTestVolumetric6 : Form
    {
        private OpenTKUtils.WinForm.GLWinFormControl glwfc;
        private Controller3D gl3dcontroller;

        private Timer systemtimer = new Timer();

        public ShaderTestVolumetric6()
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
            gl3dcontroller.EliteMovement = true;


            gl3dcontroller.KeyboardTravelSpeed = (ms,eyedist) =>
            {
                return (float)ms * 10.0f;
            };

            gl3dcontroller.MatrixCalc.InPerspectiveMode = true;
            gl3dcontroller.Start(glwfc,new Vector3(0, 0, -35000), new Vector3(126.75f, 0, 0), 0.31622F);

            items.Add("COSW", new GLColourShaderWithWorldCoord());
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

                items.Add("LINEYELLOW", new GLFixedShader(System.Drawing.Color.Yellow));
                rObjects.Add(items.Shader("LINEYELLOW"),
                            GLRenderableItem.CreateVector4(items, rl1, lines2));
            }

            {
                Bitmap[] numbers = new Bitmap[70];
                Matrix4[] numberpos = new Matrix4[numbers.Length];
                Matrix4[] numberpos2 = new Matrix4[numbers.Length];

                Font fnt = new Font("Arial", 20);

                for (int i = 0; i < numbers.Length; i++)
                {
                    int v = -35000 + i * 1000;
                    numbers[i] = new Bitmap(100, 100);
                    BaseUtils.BitMapHelpers.DrawTextCentreIntoBitmap(ref numbers[i], v.ToString(), fnt, Color.Red, Color.AliceBlue);

                    numberpos[i] = Matrix4.CreateScale(1);
                    numberpos[i] *= Matrix4.CreateRotationX(-25f.Radians());
                    numberpos[i] *= Matrix4.CreateTranslation(new Vector3(35500, 0, v));
                    numberpos2[i] = Matrix4.CreateScale(1);
                    numberpos2[i] *= Matrix4.CreateRotationX(-25f.Radians());
                    numberpos2[i] *= Matrix4.CreateTranslation(new Vector3(v, 0, -35500));
                }

                GLTexture2DArray array = new GLTexture2DArray(numbers, ownbitmaps: true);
                items.Add("Nums", array);
                items.Add("IC-2", new GLShaderPipeline(new GLPLVertexShaderTextureModelCoordWithMatrixTranslation(), new GLPLFragmentShaderTexture2DIndexed(0)));

                GLRenderControl rq = GLRenderControl.Quads(cullface: false);
                GLRenderDataTexture rt = new GLRenderDataTexture(items.Tex("Nums"));

                rObjects.Add(items.Shader("IC-2"), "1-b",
                                        GLRenderableItem.CreateVector4Vector2Matrix4(items, rq,
                                                GLShapeObjectFactory.CreateQuad(500.0f), GLShapeObjectFactory.TexQuad, numberpos,
                                                rt, numberpos.Length));

                rObjects.Add(items.Shader("IC-2"), "1-b2",
                                        GLRenderableItem.CreateVector4Vector2Matrix4(items, rq,
                                                GLShapeObjectFactory.CreateQuad(500.0f), GLShapeObjectFactory.TexQuad, numberpos2,
                                                rt, numberpos.Length));
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

            items.Add("gal", new GLTexture2D(Properties.Resources.Galaxy_L));

            items.Add("V2", new ShaderV2(items.Tex("gal")));
            GLRenderControl rv = GLRenderControl.ToTri(OpenTK.Graphics.OpenGL4.PrimitiveType.Points);
            galaxy = GLRenderableItem.CreateNullVertex(rv);   // no vertexes, all data from bound volumetric uniform, no instances as yet
            rObjects.Add(items.Shader("V2"), galaxy);

            dataoutbuffer = items.NewStorageBlock(5);
            dataoutbuffer.Allocate(sizeof(float) * 4 * 32, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicRead);    // 32 vec4 back

            atomicbuffer = items.NewAtomicBlock(6);
            atomicbuffer.Allocate(sizeof(float) * 32, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicCopy);

            dataoutbuffer = items.NewStorageBlock(5);
            dataoutbuffer.Allocate(sizeof(float) * 4 * 256, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicRead);    // 32 vec4 back

            volumetricblock = new GLVolumetricUniformBlock();
            items.Add("VB",volumetricblock);

            items.Add("MCUB", new GLMatrixCalcUniformBlock());     // create a matrix uniform block 
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

            galaxy.InstanceCount = volumetricblock.Set(gl3dcontroller.MatrixCalc, boundingbox, 10.0f);        // set up the volumentric uniform

            dataoutbuffer.ZeroBuffer();
            atomicbuffer.ZeroBuffer();

            rObjects.Render(glwfc.RenderState, gl3dcontroller.MatrixCalc);

            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

            Vector4[] databack = dataoutbuffer.ReadVector4(0, 10);

            //System.Diagnostics.Debug.WriteLine("avg {0} txtavg {1}", databack[0].ToStringVec(), databack[1].ToStringVec());

            for (int i = 0; i < databack.Length; i += 1)
            {
         //       System.Diagnostics.Debug.WriteLine("db " + databack[i].ToStringVec());
            }

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " dir " + gl3dcontroller.Camera.Current + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance + " Zoom " + gl3dcontroller.Zoom.Current;
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

            layout (binding=1) uniform sampler2D tex;

            void main(void)
            {
                float xc = abs(vs_texcoord.x-0.5)*2;    // so +/-1
                float zc = abs(vs_texcoord.z-0.5)*2;
                float h = abs(vs_texcoord.y-0.5)*2;       // from 0 to 1
                float ms = xc*xc+zc*zc;
                float m = sqrt(ms);        

                float gd = gaussian(m,0,0.2) ;  // deviation around centre, 1 = centre
                gd = max(gd,0.1);   // limit height of galaxy

                bool colourit = false;

                if ( h < gd && m < 1.1)     // 0.5 sets the size of the disk, this is touching the bounding box
                    colourit = true;

                if ( colourit)
                {
                    float s = 1.25;
                    float edge = (1-1/s)/2;
                    vec4 c =texture(tex,vec2(vs_texcoord.x/s+edge,vs_texcoord.z/s+edge));   // pick up colour scaled out
                    float brightness = c.x*c.x+c.y*c.y+c.z*c.z;

                    if ( brightness > 0.01) // min brightness
                    {
                        float alpha = min(max(m,0.7),0.04); // beware the 8 bit alpha (0.0039 per bit).
                        color = vec4(c.x,c.y,c.z,alpha);
                    }
                    else
                        discard;
                }
                else
                    discard;
            }
            ";

            public ShaderV2( IGLTexture t)
            {
                CompileLink(vertex: vcode, frag: fcode, geo: "#include TestOpenTk.Volumetrics.volumetricgeo6.glsl");
                StartAction = (a) => { t.Bind(1); };
            }
        }

        
        private void SystemTick(object sender, EventArgs e)
        {
            var cdmt = gl3dcontroller.HandleKeyboardSlews(true, OtherKeys);
            if (cdmt.AnythingChanged)
                gl3dcontroller.Redraw();
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


//next..
//    fade when close
//    reject vertex if all behind users - two ways.. all mv.z of z's behind eyez.  
//        if we went
//                back edge - > Model view->projection, /w and looked at the x/y size
//                front edge -> Model view->projection, /w and looked at the x/y size
//                maybe you could optimise the z range based on the rec sizes interpolated.
//        experiment with this.

