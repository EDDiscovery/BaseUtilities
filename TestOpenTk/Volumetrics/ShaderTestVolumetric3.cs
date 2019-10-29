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
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        public ShaderTestVolumetric3()
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

        public class ShaderV2 : GLShaderStandard
        {
            string vcode =
@"
#version 450 core
" + GLMatrixCalcUniformBlock.GLSL + @"

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
                CompileLink(vertex: vcode, frag: fcode, geo: "TestOpenTk.Volumetrics.volumetricgeo3.glsl");
                StartAction += (s) => { GLStatics.PointSize(10); };
            }
        }

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
            string s = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message);
            System.Diagnostics.Debug.WriteLine("{0} {1} {2} {3} {4} {5}", source, type, id, severity, length, s);
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Closed += ShaderTest_Closed;

            GLStatics.EnableDebug(DebugProc);

            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.MatrixCalc.ZoomDistance = 40F;
            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(90, 0, 0), 1F);

            gl3dcontroller.TravelSpeed = (ms) =>
            {
                return (float)ms / 100.0f;
            };

            items.Add("COS-1L", new GLColourObjectShaderNoTranslation((a) => { GLStatics.LineWidth(1); }));
            items.Add("LINEYELLOW", new GLFixedShader(System.Drawing.Color.Yellow, (a) => { GLStatics.LineWidth(1); }));
            items.Add("LINEPURPLE", new GLFixedShader(System.Drawing.Color.Purple, (a) => { GLStatics.LineWidth(1); }));
            items.Add("DOTYELLOW", new GLFixedProjectionShader(System.Drawing.Color.Yellow, (a) => { GLStatics.PointSize(10); }));
            items.Add("SURFACEBLUE", new GLFixedProjectionShader(System.Drawing.Color.Blue, (a) => { }));

            items.Add("V2", new ShaderV2());
            //           items.Add("wooden", new GLTexture2D(Properties.Resources.wooden));

            rObjects.Add(items.Shader("COS-1L"), "L1",   // horizontal
                         GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                                                    GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(-100, 0, 100), new Vector3(10, 0, 0), 21),
                                                    new Color4[] { Color.Gray })
                               );


            rObjects.Add(items.Shader("COS-1L"),    // vertical
                         GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                               GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(100, 0, -100), new Vector3(0, 0, 10), 21),
                                                         new Color4[] { Color.Gray })
                               );

            // Number markers using instancing and 2d arrays, each with its own transform

            Bitmap[] numbers = new Bitmap[20];
            Matrix4[] numberpos = new Matrix4[20];

            Font fnt = new Font("Arial", 44);

            for (int i = 0; i < numbers.Length; i++)
            {
                int v = -100 + i * 10;
                numbers[i] = new Bitmap(100, 100);
                BaseUtils.BitMapHelpers.DrawTextCentreIntoBitmap(ref numbers[i], v.ToString(), fnt, Color.Red, Color.AliceBlue);
                numberpos[i] = Matrix4.CreateScale(1);
                numberpos[i] *= Matrix4.CreateRotationX(-80f.Radians());
                numberpos[i] *= Matrix4.CreateTranslation(new Vector3(20, 0, v));
            }

            GLTexture2DArray array = new GLTexture2DArray(numbers, ownbitmaps: true);
            items.Add("Nums", array);
            items.Add("IC-2", new GLShaderPipeline(new GLVertexShaderTextureMatrixTranslation(), new GLFragmentShaderTexture2DIndexed()));
            items.Shader("IC-2").StartAction += (s) => { items.Tex("Nums").Bind(1); GL.Disable(EnableCap.CullFace); };
            items.Shader("IC-2").FinishAction += (s) => { GL.Enable(EnableCap.CullFace); };

            // investigate why its wrapping when we asked for it TexQUAD 1 which should interpolate over surface..

            rObjects.Add(items.Shader("IC-2"), "1-b",
                                    GLRenderableItem.CreateVector4Vector2Matrix4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                                            GLShapeObjectFactory.CreateQuad(1.0f), GLShapeObjectFactory.TexQuad, numberpos,
                                            null, numberpos.Length));






            items.Add("MCUB", new GLMatrixCalcUniformBlock());     // create a matrix uniform block 

            // New geo shader

            Vector4[] points = new Vector4[]
            {
                new Vector4(-40,-20,-40,1),
                new Vector4(+40,+20,+40,1),
            };

            rObjects.Add(items.Shader("V2"), GLRenderableItem.CreateVector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines, points, ic: 1));

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

            rObjects.Add(items.Shader("LINEYELLOW"),
                        GLRenderableItem.CreateVector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines, lines2));

            dataoutbuffer = items.NewStorageBlock(5);
            dataoutbuffer.Allocate(sizeof(float) * 4 * 32, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicRead);    // 32 vec4 back

            atomicbuffer = items.NewAtomicBlock(6);
            atomicbuffer.Allocate(sizeof(float) * 32, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicCopy);

        }

        GLStorageBlock dataoutbuffer;
        GLAtomicBlock atomicbuffer;

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(MatrixCalc mc, long time)
        {
            ((GLMatrixCalcUniformBlock)items.UB("MCUB")).Set(gl3dcontroller.MatrixCalc);        // set the matrix unform block to the controller 3d matrix calc.

            dataoutbuffer.ZeroBuffer();
            atomicbuffer.ZeroBuffer();

            rObjects.Render(gl3dcontroller.MatrixCalc);

            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

            Vector4[] databack = dataoutbuffer.ReadVector4(0, 12);

            System.Diagnostics.Debug.WriteLine("avg {0} txtavg {1}", databack[0].ToStringVec(), databack[1].ToStringVec());

            for (int i = 2; i < databack.Length; i += 2)
            {
                System.Diagnostics.Debug.WriteLine("{0} v{1} a{2:0.0} :  {3}", databack[i].X, databack[i].Z, databack[i].Y, databack[i + 1].ToStringVec());
            }

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " dir " + gl3dcontroller.Camera.Current + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance;
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

