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
    public partial class ShaderTestVolumetric4 : Form
    {
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        public ShaderTestVolumetric4()
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

//layout (location = 0) in vec4 position;

out int instance;

void main(void)
{
	//gl_Position = position;       
	//gl_Position = vec4(0,0,0,0);

    instance = gl_InstanceID;
}
";

            string fcode = @"
#version 450 core
out vec4 color;
in vec3 vs_texcoord;

void main(void)
{
    color = vec4(vs_texcoord,0.5);
}
";

            public ShaderV2()
            {
                CompileLink(vertex: vcode, frag: fcode, geo: "TestOpenTk.Volumetrics.volumetricgeo4.glsl");
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
            //string s = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message);
            //System.Diagnostics.Debug.WriteLine("{0} {1} {2} {3} {4} {5}", source, type, id, severity, length, s);
           //  s = null;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Closed += ShaderTest_Closed;

            //GLStatics.EnableDebug(DebugProc);

            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.ZoomDistance = 80F;
            gl3dcontroller.MovementTracker.MinimumCameraDirChange = 0.01f;
            gl3dcontroller.MouseRotateAmountPerPixel = 0.1f;

            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(90, 0, 0), 1F);
        
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

            rObjects.Add(items.Shader("V2"), GLRenderableItem.CreateNullVertex(OpenTK.Graphics.OpenGL4.PrimitiveType.Points, ic: slices));

            // bounding box

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

            dataoutbuffer = items.NewStorageBlock(5);
            dataoutbuffer.Allocate(sizeof(float) * 4 * 32, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicRead);    // 32 vec4 back

            pointblock = items.NewUniformBlock(1);
            pointblock.Allocate(sizeof(float) * 4 * 8 + sizeof(float) * 30);        // plenty of space

            int hsize = 40, vsize = 20, zsize = 40;
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

        }

        int slices =10;

        Vector4[] boundingbox;

        GLStorageBlock dataoutbuffer;
        GLUniformBlock pointblock;
        GLAtomicBlock atomicbuffer;

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(MatrixCalc mc, long time)
        {
            ((GLMatrixCalcUniformBlock)items.UB("MCUB")).Set(gl3dcontroller.MatrixCalc);        // set the matrix unform block to the controller 3d matrix calc.

            IntPtr pb = pointblock.Map(0, pointblock.BufferSize);
            float minzv = float.MaxValue, maxzv = float.MinValue;
            for (int i = 0; i < 8; i++)
            {
                Vector4 p = Vector4.Transform(boundingbox[i], mc.ModelMatrix);
                if (p.Z < minzv)
                    minzv = p.Z;
                if (p.Z > maxzv)
                    maxzv = p.Z;
                pointblock.MapWrite(ref pb, p);
            }

            pointblock.MapWrite(ref pb, minzv);
            pointblock.MapWrite(ref pb, maxzv);
            pointblock.MapWrite(ref pb, Vector4.Transform(new Vector4(mc.EyePosition, 0), mc.ModelMatrix));
            float slicedist = (maxzv - minzv) / (float)slices;
            float slicestart = (maxzv - minzv) / ((float)slices*2);
            pointblock.MapWrite(ref pb, slicestart); //slicestart
            pointblock.MapWrite(ref pb, slicedist); //slicedist

       //     System.Diagnostics.Debug.WriteLine("slice start {0} dist {1}", slicestart, slicedist);
           // for (int ic = 0; ic < slices; ic++)
            //    System.Diagnostics.Debug.WriteLine("slice {0} {1} {2}", minzv, maxzv, minzv + slicestart + slicedist * ic);
            pointblock.UnMap();

            dataoutbuffer.ZeroBuffer();
            atomicbuffer.ZeroBuffer();

            rObjects.Render(gl3dcontroller.MatrixCalc);

            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

            Vector4[] databack = dataoutbuffer.ReadVector4(0, 5);

          //  System.Diagnostics.Debug.WriteLine("avg {0} txtavg {1}", databack[0].ToStringVec(), databack[1].ToStringVec());

            for (int i = 0; i < databack.Length; i += 1)
            {
         //       System.Diagnostics.Debug.WriteLine("db "+databack[i].ToStringVec());
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

