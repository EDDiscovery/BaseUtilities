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
    public partial class ShaderTest5 : Form
    {
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        public ShaderTest5()
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
        GLTextureList rTexture = new GLTextureList();
        GLProgramShaderList rShaders = new GLProgramShaderList();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            gl3dcontroller.MatrixCalc.ZoomDistance = 20F;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(80f, 0, 0f), 1F);

            gl3dcontroller.TravelSpeed = (ms) =>
            {
                return (float)ms / 10.0f;
            };

            //rShaders.Add("COST", new GLVertexColourObjectShaderSimple(true));       // shader simple with translation

            //var translationvertex = new GLVertexShaderTranslation();      // expects a star list in attr 0
            //var simplefragment = new GLFragmentShaderSimple();      // vs_color shader
            //rShaders.Add("PIPE1", new GLProgramShaderPipeline(translationvertex, simplefragment));


            ////rObjects.Add("cp", new GLColouredPoints(
            ////             GLColouredObjectFactory.CreateVertexColour(stars, new Color4[] { Color.Yellow, Color.Red }),
            ////             rShaders["PIPE1"],
            ////             new GLObjectDataTranslationRotation(new Vector3(-10, 0, 0)),
            ////             2f));

            //rObjects.Add("pc2", new GLColouredPoints(
            //             GLColouredObjectFactory.CreateVertexPointCube(1f, new Color4[] { Color4.Green, Color4.White }),
            //             rShaders["PIPE1"],
            //             new GLObjectDataTranslationRotation(new Vector3(12, 0, 0)),
            //             10f));

            var translationvertex = new GLVertexShaderStars();
            var simplefragment = new GLFragmentShaderSimple();
            rShaders.Add("PIPE1", new GLProgramShaderPipeline(translationvertex, simplefragment));

            rShaders.Add("COST", new GLVertexColourObjectShaderSimple(true));       // shader simple with translation

            rObjects.Add("scopen", new GLColouredTriangles(
                    GLColouredObjectFactory.CreateSolidCubeFromTriangles(1f, new GLCubeObjectFactory.Sides[] { GLCubeObjectFactory.Sides.Bottom, GLCubeObjectFactory.Sides.Top, GLCubeObjectFactory.Sides.Left, GLCubeObjectFactory.Sides.Right },
                    new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange }), rShaders["COST"], new GLObjectDataTranslationRotation(new Vector3(4, 0, 0))));

            rObjects.Add("scopen2", new GLColouredTriangles(
                    GLColouredObjectFactory.CreateSolidCubeFromTriangles(1f, new GLCubeObjectFactory.Sides[] { GLCubeObjectFactory.Sides.Bottom, GLCubeObjectFactory.Sides.Top, GLCubeObjectFactory.Sides.Left, GLCubeObjectFactory.Sides.Right },
                    new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange }), rShaders["COST"], new GLObjectDataTranslationRotation(new Vector3(-4, 0, 0))));

            rObjects.Add("pc2", new GLColouredPoints(
                         GLColouredObjectFactory.CreateVertexPointCube(1f, new Color4[] { Color4.Green, Color4.White }),
                         rShaders["PIPE1"],
                         new GLObjectDataTranslationRotation(new Vector3(12, 0, 0)),
                         10f));

            Vector4[] stars = GLPointsFactory.RandomStars(1000, 12322, -200, 200, -100, 100, 20, -20);
            GLVertexPoints sobj = new GLVertexPoints(stars, rShaders["PIPE1"], new GLObjectDataTranslationRotation(new Vector3(0, 0, 0), new Vector3(0, 0, 0)), 1.0f);
            rObjects.Add(sobj);


            rObjects.Add("sphere3", new GLColouredPoints(
                        GLColouredObjectFactory.CreateSphere(3, 1.0f, new Color4[] { Color4.Red, Color4.Green, Color4.Blue, }),
                        rShaders["COST"],
                        new GLObjectDataTranslationRotation(new Vector3(-4, 0, -6)), 5.0f));



            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.PatchParameter(PatchParameterInt.PatchVertices, 3);
            GL.PointSize(3);
            GL.Enable(EnableCap.DepthTest);

            Closed += ShaderTest_Closed;
        }

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            rObjects.Dispose();
            rTexture.Dispose();
            rShaders.Dispose();
        }

        private void ControllerDraw(Matrix4 model, Matrix4 projection, long time)
        {
            //System.Diagnostics.Debug.WriteLine("Draw");

            float degrees = ((float)time / 5000.0f * 360.0f) % 360f;
            float degreesd2 = ((float)time / 10000.0f * 360.0f) % 360f;

            //((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).XRotDegrees = degrees;
            //((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).XRotDegrees = degrees;
            //((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).Translate(new Vector3(0.01f, 0.01f, 0));
            //((GLObjectDataTranslationRotation)(rObjects["sphere3"].InstanceData)).XRotDegrees = -degrees;
            //((GLObjectDataTranslationRotation)(rObjects["sphere3"].InstanceData)).YRotDegrees = degrees;
            //((GLObjectDataTranslationRotation)(rObjects["EDDCube"].InstanceData)).YRotDegrees = degrees;
            //((GLObjectDataTranslationRotation)(rObjects["EDDCube"].InstanceData)).ZRotDegrees = degreesd2;

            //((GLVertexTexturedObjectShaderCommonTransform)rShaders["CROT"]).Transform.YRotDegrees = degrees;

            rObjects.Render(model, projection);
        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboard(true, OtherKeys);
         //   gl3dcontroller.Redraw();
        }

        private void OtherKeys( BaseUtils.KeyboardState kb )
        {
        }

    }
}


