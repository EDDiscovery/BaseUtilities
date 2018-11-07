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
    public partial class ShaderTest2 : Form
    {
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

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

        GLRenderProgramSortedList rObjects = new GLRenderProgramSortedList();
        GLItemsList items = new GLItemsList();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            gl3dcontroller.MatrixCalc.ZoomDistance = 20F;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(0f, 0, 0f), 1F);

            gl3dcontroller.TravelSpeed = (ms) =>
            {
                return (float)ms / 20.0f;
            };

            items.Add("STARS", new GLShaderPipeline(new GLShaderStars(), new GLFragmentShaderColour()));

            items.Add("COS", new GLColourObjectShaderNoTranslation());
            items.Add("COST", new GLColourObjectShaderTranslation());
            items.Add("TEX", new GLTexturedObjectShaderSimple());

            //rObjects.Add(items.Shader("COS"), new GLColouredLines(
            //            GLShapeObjectFactory.CreateBox(400, 200, 40, new Vector3(0, 0, 0), new Vector3(0, 0, 0)),
            //            new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green },
            //            null      // lines are positioned directly.. no need for object data binding
            //            ));


            //rObjects.Add(items.Shader("COST"), "dot2-1", new GLColouredPoints(
            //             GLCubeObjectFactory.CreateVertexPointCube(1f),
            //             new Color4[] { Color.Red, Color.Green, Color.Blue, Color.Cyan, Color.Yellow, Color.Yellow, Color.Yellow, Color.Yellow },
            //             new GLObjectDataTranslationRotation(new Vector3(0, 0, -100))));

            //rObjects.Add(items.Shader("COST"), "dot2-2", new GLColouredPoints(
            //             GLCubeObjectFactory.CreateVertexPointCube(1f),
            //             new Color4[] { Color.Red, Color.Green, Color.Blue, Color.Cyan, Color.Yellow, Color.Yellow, Color.Yellow, Color.Yellow },
            //             new GLObjectDataTranslationRotation(new Vector3(0, 20, 0))));

            //Vector3[] stars = GLPointsFactory.RandomStars(10000, 12322, -200, 200, -100, 100, 20, -20);

            //// makes no sense, eye pos is static for each draw..

            //GLVertexPackedPoints212122 sobj = new GLVertexPackedPoints212122(stars,
            //            null,
            //            new Vector3(50000, 50000, 50000),
            //            16);

            //rObjects.Add(items.Shader("STARS"), "Stars", sobj);

            using (var bmp = BaseUtils.BitMapHelpers.DrawTextIntoAutoSizedBitmap("200,100", new Size(200, 100), new Font("Arial", 10.0f), Color.Yellow, Color.Blue))
            {
                items.Add("200,100", new GLTexture2D(bmp));
            }

            using (var bmp = BaseUtils.BitMapHelpers.DrawTextIntoAutoSizedBitmap("-200,-100", new Size(200, 100), new Font("Arial", 10.0f), Color.Yellow, Color.Blue))
            {
                items.Add("-200,-100", new GLTexture2D(bmp));
            }

            //rObjects.Add(items.Shader("TEX"), new GLTexturedQuads(
            //            GLShapeObjectFactory.CreateQuad(20.0f, 20.0f, new Vector3(-90, 0, 0)), GLShapeObjectFactory.TexQuad,
            //            new GLObjectDataTranslationRotation(new Vector3(200, 0, 100)),
            //            items.Tex("200,100"),1));

            //rObjects.Add(items.Shader("TEX"), new GLTexturedQuads(
            //            GLShapeObjectFactory.CreateQuad(20.0f, 20.0f, new Vector3(-90, 0, 0)), GLShapeObjectFactory.TexQuad,
            //            new GLObjectDataTranslationRotation(new Vector3(-200, 0, -100)),
            //            items.Tex("-200,-100"),1));

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.PatchParameter(PatchParameterInt.PatchVertices, 3);

            Closed += ShaderTest_Closed;
        }

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            rObjects.Dispose();
            items.Dispose();
        }

        private void ControllerDraw(Matrix4 model, Matrix4 projection, long time)
        {
            float degrees = ((float)time / 5000.0f * 360.0f) % 360f;
            float degreesd2 = ((float)time / 10000.0f * 360.0f) % 360f;

            System.Diagnostics.Debug.WriteLine("Draw eye " + gl3dcontroller.MatrixCalc.EyePosition + " to " + gl3dcontroller.Pos.Current);
            rObjects.Render(gl3dcontroller.MatrixCalc);
        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboard(true, OtherKeys);
            gl3dcontroller.Redraw();
        }

        private void OtherKeys( BaseUtils.KeyboardState kb )
        {
        }

    }
}


