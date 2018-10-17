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

        GLRenderProgramSortedList rObjects = new GLRenderProgramSortedList();
        GLTextureList rTexture = new GLTextureList();
        GLProgramShaderList rShaders = new GLProgramShaderList();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            gl3dcontroller.MatrixCalc.ZoomDistance = 20F;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(45f, 0, 0f), 1F);

            gl3dcontroller.TravelSpeed = (ms) =>
            {
                return (float)ms / 100.0f;
            };

            rShaders.Add("COS",new GLVertexColourObjectShaderSimple(false));       // shader simple
            rShaders.Add("COST",new GLVertexColourObjectShaderSimple(true));       // shader simple with translation
            rShaders.Add("TEX", new GLVertexTexturedObjectShaderSimple(true));       // texture simple with translation
            rShaders.Add("CROT", new GLVertexTexturedObjectShaderCommonTransform());       // texture simple with translation

            var translationvertex = new GLVertexShaderTranslation();
            var simplefragment = new GLFragmentShaderSimple();
            rShaders.Add("PIPE1", new GLProgramShaderPipeline(translationvertex, simplefragment));

            rTexture.Add("dotted", new GLTexture2D(Properties.Resources.dotted));
            rTexture.Add("dotted2", new GLTexture2D(Properties.Resources.dotted2));
            rTexture.Add("wooden", new GLTexture2D(Properties.Resources.wooden));
            rTexture.Add("logo8bpp", new GLTexture2D(Properties.Resources.Logo8bpp));
            rTexture.Add("shoppinglist", new GLTexture2D(Properties.Resources.shoppinglist));
            rTexture.Add("golden", new GLTexture2D(Properties.Resources.golden));

            rTexture.Add("mipmap1", new GLTexture2DMipMap(Properties.Resources.mipmap, 9));

            rObjects.Add("sc", new GLColouredTriangles(GLColouredObjectFactory.CreateSolidCubeFromTriangles(1f, new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange }), rShaders["COS"], null));

            rObjects.Add("scopen", new GLColouredTriangles(
                    GLColouredObjectFactory.CreateSolidCubeFromTriangles(1f, new GLCubeObjectFactory.Sides[] { GLCubeObjectFactory.Sides.Bottom , GLCubeObjectFactory.Sides.Top, GLCubeObjectFactory.Sides.Left, GLCubeObjectFactory.Sides.Right },
                    new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange }), rShaders["COST"], new GLObjectDataTranslationRotation(new Vector3(2, 0, 0))));

            rObjects.Add("pc", new GLColouredPoints(
                         GLColouredObjectFactory.CreateVertexPointCube(1f, new Color4[] { Color4.Yellow }),
                         rShaders["COST"],
                         new GLObjectDataTranslationRotation(new Vector3(10, 0, 0)),
                         10f));

            rObjects.Add("pc2", new GLColouredPoints(
                         GLColouredObjectFactory.CreateVertexPointCube(3f, new Color4[] { Color4.Green, Color4.White }),
                         rShaders["PIPE1"],
                         new GLObjectDataTranslationRotation(new Vector3(15, 0, 0)),
                         10f));

            rObjects.Add("cp", new GLColouredPoints(
                         GLColouredObjectFactory.CreateVertexPointCube(1f, new Color4[] { Color4.Red }),
                         rShaders["COST"],
                         new GLObjectDataTranslationRotation(new Vector3(-10, 0, 0)),
                         10f));

            rObjects.Add("dot2-1", new GLColouredPoints(
                         GLColouredObjectFactory.CreateVertexPointCube(1f, new Color4[] { Color.Red, Color.Green, Color.Blue, Color.Cyan, Color.Yellow, Color.Yellow, Color.Yellow, Color.Yellow }),
                         rShaders["COST"],
                         new GLObjectDataTranslationRotation(new Vector3(-14, 0, 0)),
                         10f));

            rObjects.Add(new GLColouredLines(
                        GLColouredObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(-100, 0, 100), new Vector3(10, 0, 0), 21, new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green }),
                        rShaders["COS"],
                        null,       // lines are positioned directly.. no need for object data binding
                        1f));

            rObjects.Add(new GLColouredLines(
                        GLColouredObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(100, 0, -100), new Vector3(0, 0, 10), 21, new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green }),
                        rShaders["COS"],
                        null,       // lines are positioned directly.. no need for object data binding
                        1f));

            rObjects.Add(new GLColouredLines(
                        GLColouredObjectFactory.CreateLines(new Vector3(-100, 10, -100), new Vector3(-100, 10, 100), new Vector3(10, 0, 0), 21, new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green }),
                        rShaders["COS"],
                        null,       // lines are positioned directly.. no need for object data binding
                        1f));

            rObjects.Add(new GLColouredLines(
                        GLColouredObjectFactory.CreateLines(new Vector3(-100, 10, -100), new Vector3(100, 10, -100), new Vector3(0, 0, 10), 21, new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green }),
                        rShaders["COS"],
                        null,       // lines are positioned directly.. no need for object data binding
                        1f));

            rObjects.Add(new GLTexturedTriangles(
                        GLTexturedObjectFactory.CreateTexturedCubeFromTriangles(1f),
                        rShaders["TEX"],
                        new GLObjectDataTranslationRotation(new Vector3(10, 0, -5)),
                        rTexture["dotted2"]));

            rObjects.Add("EDDCube", new GLTexturedTriangles(
                        GLTexturedObjectFactory.CreateTexturedCubeFromTriangles(3f),
                        rShaders["TEX"],
                        new GLObjectDataTranslationRotation(new Vector3(15, 0, -8)),
                        rTexture["logo8bpp"]));

            rObjects.Add("woodbox", new GLTexturedTriangles(
                        GLTexturedObjectFactory.CreateTexturedCubeFromTriangles(1f),
                        rShaders["TEX"],
                        new GLObjectDataTranslationRotation(new Vector3(5, 0, -5)),
                        rTexture["wooden"]));

            rObjects.Add("mipmap1", new GLTexturedTriangles(
                        GLTexturedObjectFactory.CreateTexturedCubeFromTriangles(1f),
                        rShaders["TEX"],
                        new GLObjectDataTranslationRotation(new Vector3(15, 0, -5)),
                        rTexture["mipmap1"]));


            rObjects.Add(new GLTexturedQuads(
                        GLTexturedObjectFactory.CreateTexturedQuad(1.0f, 1.0f, new Vector3(0, 0, 0)),
                        rShaders["TEX"],
                        new GLObjectDataTranslationRotation(new Vector3(0, 0, -5)),
                        rTexture["dotted2"]));

            rObjects.Add(new GLTexturedQuads(
                        GLTexturedObjectFactory.CreateTexturedQuad(1.0f, 1.0f, new Vector3(0, 0, 0)),
                        rShaders["TEX"],
                        new GLObjectDataTranslationRotation(new Vector3(-5, 0, 0)),
                        rTexture["dotted"]));

            rObjects.Add("EDDFlat", new GLTexturedQuads(
                        GLTexturedObjectFactory.CreateTexturedQuad(4.0f, rTexture["logo8bpp"].Width, rTexture["logo8bpp"].Height, new Vector3(-0, 0, 0)),
                        rShaders["TEX"],
                        new GLObjectDataTranslationRotation(new Vector3(-10, 0, -5)),
                        rTexture["logo8bpp"]));

            rObjects.Add(new GLTexturedQuads(
                        GLTexturedObjectFactory.CreateTexturedQuad(4.0f, new Vector3(-90, 0, 0)),
                        rShaders["TEX"],
                        new GLObjectDataTranslationRotation(new Vector3(-15, 0, -5)),
                        rTexture["shoppinglist"]));

            rObjects.Add("woodboxc1", new GLTexturedTriangles(
                        GLTexturedObjectFactory.CreateTexturedCubeFromTriangles(1.0f),
                        rShaders["CROT"],
                        new GLObjectDataTranslationRotation(new Vector3(5, 0, -10)),
                        rTexture["wooden"]));

            rObjects.Add("woodboxc2", new GLTexturedTriangles(
                        GLTexturedObjectFactory.CreateTexturedCubeFromTriangles(1.0f),
                        rShaders["CROT"],
                        new GLObjectDataTranslationRotation(new Vector3(7, 0, -10)),
                        rTexture["wooden"]));

            rObjects.Add("woodboxc3", new GLTexturedTriangles(
                        GLTexturedObjectFactory.CreateTexturedCubeFromTriangles(1.0f),
                        rShaders["CROT"],
                        new GLObjectDataTranslationRotation(new Vector3(9, 0, -10)),
                        rTexture["wooden"]));


            rObjects.Add("sphere1", new GLTexturedTriangles(
                        GLTexturedObjectFactory.CreateTexturedSphereFromTriangles(3, 1.0f),
                        rShaders["TEX"],
                        new GLObjectDataTranslationRotation(new Vector3(0, 0, -10)),
                        rTexture["wooden"]));

            rObjects.Add("woodboxce", new GLTexturedTriangles(
                        GLTexturedObjectFactory.CreateTexturedCubeFromTriangles(1.0f),
                        rShaders["COST"],
                        new GLObjectDataTranslationRotation(new Vector3(0, 0, -11)),
                        rTexture["wooden"]));

            rObjects.Add("sphere2", new GLTexturedTriangles(
                        GLTexturedObjectFactory.CreateTexturedSphereFromTriangles(3, 1.0f),
                        rShaders["CROT"],
                        new GLObjectDataTranslationRotation(new Vector3(-4, 0, -4)),
                        rTexture["golden"]));


            rObjects.Add("sphere3", new GLColouredPoints(
                        GLColouredObjectFactory.CreateSphere(3, 2.0f, new Color4[] { Color4.Red, Color4.Green, Color4.Blue, }),
                        rShaders["COST"],
                        new GLObjectDataTranslationRotation(new Vector3(-4, 0, -6)), 5.0f));

            rObjects.Add("sphere4", new GLColouredPoints(
                        GLColouredObjectFactory.CreateSphere(1, 1.0f, new Color4[] { Color4.Red, Color4.Green, Color4.Blue, }),
                        rShaders["COST"],
                        new GLObjectDataTranslationRotation(new Vector3(-4, 0, -8)), 5.0f));

            rObjects.Add("sphere5", new GLColouredPoints(
                        GLColouredObjectFactory.CreateSphere(2, 1.0f, new Color4[] { Color4.Red, Color4.Green, Color4.Blue, }),
                        rShaders["COST"],
                        new GLObjectDataTranslationRotation(new Vector3(-4, 0, -10)), 5.0f));


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

            ((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).XRotDegrees = degrees;
            ((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).XRotDegrees = degrees;
            ((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).Translate(new Vector3(0.01f, 0.01f, 0));
            ((GLObjectDataTranslationRotation)(rObjects["sphere3"].InstanceData)).XRotDegrees = -degrees;
            ((GLObjectDataTranslationRotation)(rObjects["sphere3"].InstanceData)).YRotDegrees = degrees;
            ((GLObjectDataTranslationRotation)(rObjects["EDDCube"].InstanceData)).YRotDegrees = degrees;
            ((GLObjectDataTranslationRotation)(rObjects["EDDCube"].InstanceData)).ZRotDegrees = degreesd2;

            ((GLVertexTexturedObjectShaderCommonTransform)rShaders["CROT"]).Transform.YRotDegrees = degrees;

            rObjects.Render(model, projection);
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


