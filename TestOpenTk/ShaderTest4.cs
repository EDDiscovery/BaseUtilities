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
    public partial class ShaderTest4 : Form
    {
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        OpenTKUtils.GL4.Program solidprogram;
        OpenTKUtils.GL4.Program textureprogram;

        public ShaderTest4()
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

        List<Renderable> rObjects = new List<Renderable>();
        List<Texture> rTexture = new List<Texture>();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            string vertexsolid =
@"
#version 450 core
layout (location = 0) in vec4 position;
layout(location = 1) in vec4 color;

out vec4 vs_color;

layout (location = 20) uniform  mat4 projection;
layout (location = 21) uniform  mat4 modelView;

void main(void)
{
	gl_Position = projection * modelView * position;        // order important
	vs_color = color;
}
";

            string fragmentsolid =
@"
#version 450 core
in vec4 vs_color;
out vec4 color;

void main(void)
{
	color = vs_color;
}
";

            string vertexttexture =
@"
#version 450 core
layout (location = 0) in vec4 position;
layout (location = 1) in vec2 textureCoordinate;

out vec2 vs_textureCoordinate;

layout(location = 20) uniform  mat4 projection;
layout (location = 21) uniform  mat4 modelView;

void main(void)
{
	vs_textureCoordinate = textureCoordinate;
	gl_Position = projection * modelView * position;
}
";

            string fragmenttexture =
@"
#version 450 core
in vec2 vs_textureCoordinate;
uniform sampler2D textureObject;
out vec4 color;

void main(void)
{
	color = texelFetch(textureObject, ivec2(vs_textureCoordinate.x, vs_textureCoordinate.y), 0);
}
";


            //GL.Viewport(0, 0, gltracker.glControl.Width, gltracker.glControl.Height);

            gl3dcontroller.MatrixCalc.ZoomDistance = 20F;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(45f, 0, 0f), 1F);

            gl3dcontroller.TravelSpeed = (ms) =>
            {
                return (float)ms / 100.0f;
            };

            solidprogram = new OpenTKUtils.GL4.Program();
            solidprogram.Compile(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, vertexsolid);
            solidprogram.Compile(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, fragmentsolid);
            string ret = solidprogram.Link();
            if (ret != null)
                System.Diagnostics.Debug.WriteLine("Link error " + ret);

            textureprogram = new OpenTKUtils.GL4.Program();
            textureprogram.Compile(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, vertexttexture);
            textureprogram.Compile(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, fragmenttexture);
            ret = textureprogram.Link();
            if (ret != null)
                System.Diagnostics.Debug.WriteLine("Link error " + ret);

            rTexture.Add(new Texture(Properties.Resources.dotted));
            rTexture.Add(new Texture(Properties.Resources.dotted2));
            rTexture.Add(new Texture(Properties.Resources.wooden));

            rObjects.Add(new Triangles(CubeObjectFactory.CreateSolidCube(Vector3.Zero, 1f, Color4.HotPink), solidprogram));
            rObjects.Add(new Points(CubeObjectFactory.CreateVertexPointCube(new Vector3(-10, 0, 0), 1f, new Color4[] { Color4.Yellow }), solidprogram, 10f));

            rObjects.Add(new VertexTexturedObject(TexturedObjectFactory.CreateTexturedCube(new Vector3(10, 0, -10), 1f, rTexture[0].Width, rTexture[0].Height), textureprogram, rTexture[0]));

            rObjects.Add(new Points(CubeObjectFactory.CreateVertexPointCube(new Vector3(-5, 0, 0), 1f, new Color4[] { Color.Red, Color.Green, Color.Blue, Color.Cyan, Color.Yellow, Color.Yellow, Color.Yellow, Color.Yellow }), solidprogram, 10f));
            rObjects.Add(new Lines(LineObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(-100, 0, 100), new Vector3(10, 0, 0), 21, new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green }), solidprogram, 1f));
            rObjects.Add(new Lines(LineObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(100, 0, -100), new Vector3(0, 0, 10), 21, new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green }), solidprogram, 1f));

            rObjects.Add(new VertexTexturedObject(TexturedObjectFactory.CreateTexturedCube(new Vector3(10, 0, 10), 1f, rTexture[1].Width, rTexture[1].Height), textureprogram, rTexture[1]));

            rObjects.Add(new Lines(LineObjectFactory.CreateLines(new Vector3(-100, 10, -100), new Vector3(-100, 10, 100), new Vector3(10, 0, 0), 21, new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green }), solidprogram, 1f));
            rObjects.Add(new Lines(LineObjectFactory.CreateLines(new Vector3(-100, 10, -100), new Vector3(100, 10, -100), new Vector3(0, 0, 10), 21, new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green }), solidprogram, 1f));

            rObjects.Add(new VertexTexturedObject(TexturedObjectFactory.CreateTexturedCube(new Vector3(0, 0, -10), 1f, rTexture[2].Width, rTexture[2].Height), textureprogram, rTexture[2]));
            rObjects.Add(new VertexTexturedQuadObject(TexturedObjectFactory.CreateFlatTexturedQuadImage(new Vector3(0, 0, -8), 1f, rTexture[2].Width, rTexture[2].Height), textureprogram, rTexture[2]));
            rObjects.Add(new VertexTexturedQuadObject(TexturedObjectFactory.CreateVerticalTexturedQuadImage(new Vector3(0, 0, -5), 1f, rTexture[2].Width, rTexture[2].Height), textureprogram, rTexture[2]));

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.PatchParameter(PatchParameterInt.PatchVertices, 3);
            GL.PointSize(3);
            GL.Enable(EnableCap.DepthTest);

            Closed += ShaderTest_Closed;
        }

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            foreach (var obj in rObjects)
                obj.Dispose();
            foreach (var obj in rTexture)
                obj.Dispose();

            solidprogram.Dispose();
            textureprogram.Dispose();
        }

        private void ControllerDraw(Matrix4 model, Matrix4 projection, long time)
        {
            //System.Diagnostics.Debug.WriteLine("Draw");

            foreach (var renderObject in rObjects)
            {
                GL.UniformMatrix4(20, false, ref projection);   // pass in uniform var the proj matrix
                GL.UniformMatrix4(21, false, ref model);        // pass in uniform var the model matrix
                renderObject.BindRender();
            }
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


