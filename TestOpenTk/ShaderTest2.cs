using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTKUtils;
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
    public partial class ShaderTest2 : Form
    {
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        GLProgram program;

        public ShaderTest2()
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

        List<RenderObject> rObjects = new List<RenderObject>();

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
layout (location = 21) uniform  mat4 projection;

void main(void)
{
	gl_Position = projection * modelView * position;        // order important

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
            //GL.Viewport(0, 0, gltracker.glControl.Width, gltracker.glControl.Height);

            gl3dcontroller.MatrixCalc.ZoomDistance = 5F;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(45f, 45f, 45f), 1F);

            gl3dcontroller.TravelSpeed = (ms) =>
            {
                return (float)ms / 100.0f;
            };


            Vertex[] vertices = Vertex.CreateSolidCube(1f, Color4.Yellow);
            rObjects.Add(new RenderObject(vertices));

            program = new GLProgram();

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
        }

        private void ControllerDraw(Matrix4 model, Matrix4 projection, long time)
        {
            //System.Diagnostics.Debug.WriteLine("Draw");

            program.Use();
            GL.UniformMatrix4(20, false, ref model);        // pass in uniform var 20 the model matrix
            GL.UniformMatrix4(21, false, ref projection);   // pass in uniform var 21 the proj matrix

            foreach (var renderObject in rObjects)
                renderObject.Render();
        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboard(true, OtherKeys);
        }

        private void OtherKeys( BaseUtils.KeyboardState kb )
        {
            if (kb.IsPressed(Keys.M))
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
            }
            if (kb.IsPressed(Keys.N))
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            }
            if (kb.IsPressed(Keys.B))
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }

            gl3dcontroller.Redraw();

        }

    }
}


