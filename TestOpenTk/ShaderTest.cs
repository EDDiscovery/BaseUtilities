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

namespace TestOpenTk
{
    public partial class ShaderTest : Form
    {
        private Controller3D gltracker = new Controller3D();

        private Timer systemtimer = new Timer();

        OpenTKUtils.GL4.GLProgram program;

        public ShaderTest()
        {
            InitializeComponent();

            this.glControlContainer.SuspendLayout();
            gltracker.CreateGLControl();
            this.glControlContainer.Controls.Add(gltracker.glControl);
            //gltracker.PaintObjects = ControllerDraw;
            this.glControlContainer.ResumeLayout();

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
            systemtimer.Start();
        }

        List<BasicRenderObject> rObjects = new List<BasicRenderObject>();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            string vertextshadercode =
@"
#version 450 core
layout (location = 0) in vec4 position;
layout(location = 1) in vec4 color;

out vec4 vs_color;

layout (location = 20) uniform  mat4 modelView;

void main(void)
{
	gl_Position = modelView * position;
	vs_color = color;
}
";

            string fragmentshadercode =
@"
#version 450 core
in vec4 vs_color;
out vec4 color;

void main(void)
{
	color = vs_color;
}
";
            GL.Viewport(0, 0, gltracker.glControl.Width, gltracker.glControl.Height);

            //gltracker.Start(true, new Vector3((float)0, (float)0, (float)0), Vector3.Zero, 1F, 10F, 0, 100000);

            GLVertexColour[] vertices = GLCubeObjectFactory.CreateSolidCubeFromTriangles(0.2f, Color4.HotPink);
            rObjects.Add(new BasicRenderObject(vertices));

            program = new OpenTKUtils.GL4.GLProgram();

            using (GLShader vertexshader = new GLShader(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, vertextshadercode))
            {
                using (GLShader fragmentshader = new GLShader(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, fragmentshadercode))
                {
                    string ret = program.Link( vertexshader, fragmentshader);
                    if (ret != null)
                        System.Diagnostics.Debug.WriteLine("Link error " + ret);
                }
            }

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.PatchParameter(PatchParameterInt.PatchVertices, 3);
            GL.Enable(EnableCap.DepthTest);

            Closed += ShaderTest_Closed;
        }

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            foreach (var obj in rObjects)
                obj.Dispose();

            program.Dispose();
         //   GL.DeleteVertexArrays(1, ref _vertexArray);
        }

        private void ControllerDraw(long time)
        {
            System.Diagnostics.Debug.WriteLine("Draw");
        }

        private void SystemTick(object sender, EventArgs e )
        {
            // works as per demo outside of our controller modelview..

            Matrix4 modelview;

            var k = (float)Environment.TickCount * 0.00005F;
            var t1 = Matrix4.CreateTranslation(
                (float)(Math.Sin(k * 5f) * 0.5f),
                (float)(Math.Cos(k * 5f) * 0.5f),
                0f);
            var r1 = Matrix4.CreateRotationX(k * 13.0f);
            var r2 = Matrix4.CreateRotationY(k * 13.0f);
            var r3 = Matrix4.CreateRotationZ(k * 3.0f);
            modelview = r1* r2 * r3* t1;

            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            program.Use();
            GL.UniformMatrix4(20, false, ref modelview);
            foreach (var renderObject in rObjects)
            {
                renderObject.Render();
            }
            // GL.PointSize(10);

             gltracker.glControl.SwapBuffers();


        }

    }



}


