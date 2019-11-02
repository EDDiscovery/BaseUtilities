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
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        public ShaderTestVolumetric6()
        {
            InitializeComponent();

            this.glControlContainer.SuspendLayout();
            gl3dcontroller.CreateGLControl();
            this.glControlContainer.Controls.Add(gl3dcontroller.glControl);
            gl3dcontroller.PaintObjects = ControllerDraw;
            this.glControlContainer.ResumeLayout();

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
                AddVertexFragment(new GLVertexShaderNoTranslation(), new GLFragmentShaderFixedColour(c));
            }
        }

        public class GLFixedProjectionShader : GLShaderPipeline
        {
            public GLFixedProjectionShader(Color c, Action<IGLProgramShader> action = null) : base(action)
            {
                AddVertexFragment(new GLVertexShaderProjection(), new GLFragmentShaderFixedColour(c));
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

            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 1f;
            gl3dcontroller.MatrixCalc.PerspectiveFarZDistance = 500000f;
            gl3dcontroller.ZoomDistance = 500F;
            gl3dcontroller.EliteMovement = true;


            gl3dcontroller.KeyboardTravelSpeed = (ms) =>
            {
                return (float)ms * 10.0f;
            };

            gl3dcontroller.MatrixCalc.InPerspectiveMode = true;
            gl3dcontroller.Start(new Vector3(0, 0, -35000), new Vector3(126.75f, 0, 0), 0.31622F);
            //gl3dcontroller.MatrixCalc.InPerspectiveMode = false;
            //gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(180f, 0, 0), 0.01F);

            items.Add("COS-1L", new GLColourObjectShaderNoTranslation((a) => { GLStatics.LineWidth(1); }));

            //for (float h = -2000; h <= 2000; h += 2000)
            float h = 0;
            {
                Color cr = Color.FromArgb(60, Color.Gray);
                rObjects.Add(items.Shader("COS-1L"),    // horizontal
                             GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(-35000, h, -35000), new Vector3(-35000, h, 35000), new Vector3(1000, 0, 0), 70),
                                                        new Color4[] { cr })
                                   );

                rObjects.Add(items.Shader("COS-1L"),  
                             GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(-35000, h, -35000), new Vector3(35000, h, -35000), new Vector3(0, 0,1000), 70),
                                                        new Color4[] { cr })
                                   );

            }


            // bounding box

            int hsize = 35000, vsize = 2000, zsize = 35000;
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

            items.Add("LINEYELLOW", new GLFixedShader(System.Drawing.Color.Yellow, (a) => { GLStatics.LineWidth(1); }));
            rObjects.Add(items.Shader("LINEYELLOW"),
                        GLRenderableItem.CreateVector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines, lines2));


            items.Add("gal", new GLTexture2D(Properties.Resources.galheightmap7));

            items.Add("V2", new ShaderV2(items.Tex("gal")));
            rObjects.Add(items.Shader("V2"), GLRenderableItem.CreateNullVertex(OpenTK.Graphics.OpenGL4.PrimitiveType.Points, ic: slices));

            items.Add("MCUB", new GLMatrixCalcUniformBlock());     // create a matrix uniform block 

            dataoutbuffer = items.NewStorageBlock(5);
            dataoutbuffer.Allocate(sizeof(float) * 4 * 32, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicRead);    // 32 vec4 back

            atomicbuffer = items.NewAtomicBlock(6);
            atomicbuffer.Allocate(sizeof(float) * 32, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicCopy);

            dataoutbuffer = items.NewStorageBlock(5);
            dataoutbuffer.Allocate(sizeof(float) * 4 * 256, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicRead);    // 32 vec4 back

            volumetricblock = new GLVolumetricUniformBlock();
            items.Add("VB",volumetricblock);



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
                numberpos2[i] *= Matrix4.CreateTranslation(new Vector3(v,0,-35500));
            }

            GLTexture2DArray array = new GLTexture2DArray(numbers, ownbitmaps: true);
            items.Add("Nums", array);
            items.Add("IC-2", new GLShaderPipeline(new GLVertexShaderTextureMatrixTranslation(), new GLFragmentShaderTexture2DIndexed()));
            items.Shader("IC-2").StartAction += (s) => { items.Tex("Nums").Bind(1); GL.Disable(EnableCap.CullFace); };
            items.Shader("IC-2").FinishAction += (s) => { GL.Enable(EnableCap.CullFace); };

            // investigate why its wrapping when we asked for it TexQUAD 1 which should interpolate over surface..

            rObjects.Add(items.Shader("IC-2"), "1-b",
                                    GLRenderableItem.CreateVector4Vector2Matrix4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                                            GLShapeObjectFactory.CreateQuad(500.0f), GLShapeObjectFactory.TexQuad, numberpos,
                                            null, numberpos.Length));

            rObjects.Add(items.Shader("IC-2"), "1-b2",
                                    GLRenderableItem.CreateVector4Vector2Matrix4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                                            GLShapeObjectFactory.CreateQuad(500.0f), GLShapeObjectFactory.TexQuad, numberpos2,
                                            null, numberpos.Length));






        }

        Vector4[] boundingbox;

        GLStorageBlock dataoutbuffer;
        GLVolumetricUniformBlock volumetricblock;
        GLAtomicBlock atomicbuffer;

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(MatrixCalc mc, long time)
        {
            ((GLMatrixCalcUniformBlock)items.UB("MCUB")).Set(gl3dcontroller.MatrixCalc);        // set the matrix unform block to the controller 3d matrix calc.

            volumetricblock.Set(gl3dcontroller.MatrixCalc, boundingbox, slices);        // set up the volumentric uniform

            dataoutbuffer.ZeroBuffer();
            atomicbuffer.ZeroBuffer();

            rObjects.Render(gl3dcontroller.MatrixCalc);

            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

            Vector4[] databack = dataoutbuffer.ReadVector4(0, 10);

            //System.Diagnostics.Debug.WriteLine("avg {0} txtavg {1}", databack[0].ToStringVec(), databack[1].ToStringVec());

            for (int i = 0; i < databack.Length; i += 1)
            {
                System.Diagnostics.Debug.WriteLine("db " + databack[i].ToStringVec());
            }

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " dir " + gl3dcontroller.Camera.Current + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance + " Zoom " + gl3dcontroller.Zoom.Current;
        }

        int slices = 1000;


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

            layout (binding=1) uniform sampler2D tex;

            void main(void)
            {
                float xc = abs(vs_texcoord.x-0.5)*2;    // so +/-1
                float zc = abs(vs_texcoord.z-0.5)*2;
                float h = abs(vs_texcoord.y-0.5)*2;       // from 0 to 1
                float ms = xc*xc+zc*zc;
                float m = sqrt(ms);        // 0 at centre, 0.707 at maximum

                float gd = gaussian(m,0,0.2) ;  // deviation around centre, 1 = centre

                bool colourit = false;
                
                gd = max(gd,0.1);

                if ( h < gd && m < 1.1)     // 0.5 sets the size of the disk, this is touching the bounding box
                    colourit = true;

                if ( colourit)
                {
                    float s = 1.25;
                    float edge = (1-1/s)/2;
                    vec4 c =texture(tex,vec2(vs_texcoord.x/s+edge,vs_texcoord.z/s+edge));                     
                    //vec4 c =texture(tex,vec2(vs_texcoord.x,vs_texcoord.z));                     
                    if ( c.x*c.x+c.y*c.y+c.z*c.z > 0.0)
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
            var cdmt = gl3dcontroller.HandleKeyboard(true, OtherKeys);
            if (cdmt.AnythingChanged)
                gl3dcontroller.Redraw();
        }

        private void OtherKeys(BaseUtils.KeyboardState kb)
        {
            if (kb.IsPressedRemove(Keys.F1, BaseUtils.KeyboardState.ShiftState.None))
            {
                int times = 1000;
                System.Diagnostics.Debug.WriteLine("Start test");
                long tickcount = gl3dcontroller.Redraw(times);
                System.Diagnostics.Debug.WriteLine("Redraw {0} ms per {1}", tickcount, (float)tickcount/(float)times);
            }
        }
    }
}

