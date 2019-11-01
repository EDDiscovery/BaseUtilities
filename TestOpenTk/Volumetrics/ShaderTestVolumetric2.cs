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
using System.Reflection;

// Demonstrate the volumetric calculations needed to compute a plane facing the user inside a bounding box done inside a geo shader

namespace TestOpenTk
{
    public partial class ShaderTestVolumetric2 : Form
    {
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        public ShaderTestVolumetric2()
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

void main(void)
{
    color = vs_color;
}
";

            public ShaderV2()
            {
                CompileLink(vertex: vcode, frag: fcode, geo: "TestOpenTk.Volumetrics.volumetricgeo2.glsl");
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
            gl3dcontroller.ZoomDistance = 40F;
            gl3dcontroller.Start(new Vector3(30, 0, 0), new Vector3(90, 0, 0), 1F);

            gl3dcontroller.KeyboardTravelSpeed = (ms) =>
            {
                return (float)ms / 100.0f;
            };

            items.Add("COS-1L", new GLColourObjectShaderNoTranslation((a) => { GLStatics.LineWidth(1); }));
            items.Add("LINEYELLOW", new GLFixedShader(System.Drawing.Color.Yellow, (a) => { GLStatics.LineWidth(1); }));
            items.Add("LINEPURPLE", new GLFixedShader(System.Drawing.Color.Purple, (a) => { GLStatics.LineWidth(1); }));
            items.Add("DOTYELLOW", new GLFixedProjectionShader(System.Drawing.Color.Yellow, (a) => { GLStatics.PointSize(10); }));
            items.Add("SURFACEBLUE", new GLFixedProjectionShader(System.Drawing.Color.Blue, (a) => {  }));

            items.Add("V2", new ShaderV2());

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


            items.Add("MCUB", new GLMatrixCalcUniformBlock());     // create a matrix uniform block 

            // New geo shader

            Vector4[] points = new Vector4[]
            {
                new Vector4(20,-5,-10,1),
                new Vector4(40,+5,+10,1),
            };

            rObjects.Add(items.Shader("V2"), GLRenderableItem.CreateVector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines, points,ic:9));

            int left = 20, right = 40, bottom = -5, top = +5, front = -10, back = 10;
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
            dataoutbuffer.Allocate(sizeof(float) * 4 * 32, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicRead);

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

            Vector4[] databack = dataoutbuffer.ReadVector4(0, 2);
            for (int i = 0; i < databack.Length; i++)
            {
                System.Diagnostics.Debug.WriteLine(i + " = " + databack[i].ToString());
            }

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " dir " + gl3dcontroller.Camera.Current + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance;
        }

        private void SystemTick(object sender, EventArgs e )
        {
            var cdmt = gl3dcontroller.HandleKeyboard(true);
            if ( cdmt.AnythingChanged )
                gl3dcontroller.Redraw();
        }

    }
}

