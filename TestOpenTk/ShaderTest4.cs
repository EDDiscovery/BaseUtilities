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

        GLRenderList rObjects = new GLRenderList();
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

            rTexture.Add("dotted", new GLTexture(Properties.Resources.dotted));
            rTexture.Add("dotted2", new GLTexture(Properties.Resources.dotted2));
            rTexture.Add("wooden", new GLTexture(Properties.Resources.wooden));
            rTexture.Add("logo8bpp", new GLTexture(Properties.Resources.Logo8bpp));
            rTexture.Add("shoppinglist", new GLTexture(Properties.Resources.shoppinglist));

            rObjects.Add("sc", new GLColouredTriangles(GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f, Color4.HotPink), rShaders["COS"], null));

            rObjects.Add("pc", new GLColouredPoints(
                         GLCubeObjectFactory.CreateVertexPointCube(1f, new Color4[] { Color4.Yellow }),
                         rShaders["COST"],
                         new GLObjectDataTranslationRotation(new Vector3(10, 0, 0)),
                         10f));

            rObjects.Add("cp", new GLColouredPoints(
                         GLCubeObjectFactory.CreateVertexPointCube(1f, new Color4[] { Color4.Red }),
                         rShaders["COST"],
                         new GLObjectDataTranslationRotation(new Vector3(-10, 0, 0)),
                         10f));

            rObjects.Add("dot2-1", new GLColouredPoints(
                         GLCubeObjectFactory.CreateVertexPointCube(1f, new Color4[] { Color.Red, Color.Green, Color.Blue, Color.Cyan, Color.Yellow, Color.Yellow, Color.Yellow, Color.Yellow }),
                         rShaders["COST"],
                         new GLObjectDataTranslationRotation(new Vector3(-14, 0, 0)),
                         10f));

            rObjects.Add(new GLColouredLines(
                        GLLineObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(-100, 0, 100), new Vector3(10, 0, 0), 21, new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green }),
                        rShaders["COS"],
                        null,       // lines are positioned directly.. no need for object data binding
                        1f));

            rObjects.Add(new GLColouredLines(
                        GLLineObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(100, 0, -100), new Vector3(0, 0, 10), 21, new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green }),
                        rShaders["COS"],
                        null,       // lines are positioned directly.. no need for object data binding
                        1f));

            rObjects.Add(new GLColouredLines(
                        GLLineObjectFactory.CreateLines(new Vector3(-100, 10, -100), new Vector3(-100, 10, 100), new Vector3(10, 0, 0), 21, new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green }),
                        rShaders["COS"],
                        null,       // lines are positioned directly.. no need for object data binding
                        1f));

            rObjects.Add(new GLColouredLines(
                        GLLineObjectFactory.CreateLines(new Vector3(-100, 10, -100), new Vector3(100, 10, -100), new Vector3(0, 0, 10), 21, new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green }),
                        rShaders["COS"],
                        null,       // lines are positioned directly.. no need for object data binding
                        1f));

            rObjects.Add(new GLTexturedTriangles(
                        GLTexturedObjectFactory.CreateTexturedCubeFromTriangles(1f, rTexture["dotted2"]),
                        rShaders["TEX"],
                        new GLObjectDataTranslationRotation(new Vector3(10, 0, -5)),
                        rTexture["dotted2"]));

            rObjects.Add("woodbox", new GLTexturedTriangles(
                        GLTexturedObjectFactory.CreateTexturedCubeFromTriangles(1f, rTexture["wooden"]),
                        rShaders["TEX"],
                        new GLObjectDataTranslationRotation(new Vector3(5, 0, -5)),
                        rTexture["wooden"]));


            rObjects.Add(new GLTexturedQuads(
                        GLTexturedObjectFactory.CreateTexturedQuad(1.0f/ (float)rTexture["dotted2"].Width, new Vector3(0, 0, 0),  rTexture["dotted2"]),
                        rShaders["TEX"],
                        new GLObjectDataTranslationRotation(new Vector3(0, 0, -5)),
                        rTexture["dotted2"]));

            rObjects.Add(new GLTexturedQuads(
                        GLTexturedObjectFactory.CreateTexturedQuad(1.0f/ (float)rTexture["dotted"].Width, new Vector3(45, 45, 45), rTexture["dotted"]),
                        rShaders["TEX"],
                        new GLObjectDataTranslationRotation(new Vector3(-5, 0, -5)),
                        rTexture["dotted"]));

            rObjects.Add(new GLTexturedQuads(
                        GLTexturedObjectFactory.CreateTexturedQuad(4.0f / (float)rTexture["logo8bpp"].Width, new Vector3(0, 0, 0), rTexture["logo8bpp"]),
                        rShaders["TEX"],
                        new GLObjectDataTranslationRotation(new Vector3(-10, 0, -5)),
                        rTexture["logo8bpp"]));

            rObjects.Add(new GLTexturedQuads(
                        GLTexturedObjectFactory.CreateTexturedQuad(4.0f / (float)rTexture["shoppinglist"].Width, new Vector3(-90, 0, 0), rTexture["shoppinglist"]),
                        rShaders["TEX"],
                        new GLObjectDataTranslationRotation(new Vector3(-15, 0, -5)),
                        rTexture["shoppinglist"]));

            rObjects.Add("woodboxc1", new GLTexturedTriangles(
                        GLTexturedObjectFactory.CreateTexturedCubeFromTriangles(1f, rTexture["wooden"]),
                        rShaders["CROT"],
                        new GLObjectDataTranslationRotation(new Vector3(5, 0, -10)),
                        rTexture["wooden"]));

            rObjects.Add("woodboxc2", new GLTexturedTriangles(
                        GLTexturedObjectFactory.CreateTexturedCubeFromTriangles(1f, rTexture["wooden"]),
                        rShaders["CROT"],
                        new GLObjectDataTranslationRotation(new Vector3(7, 0, -10)),
                        rTexture["wooden"]));

            rObjects.Add("woodboxc3", new GLTexturedTriangles(
                        GLTexturedObjectFactory.CreateTexturedCubeFromTriangles(1f, rTexture["wooden"]),
                        rShaders["CROT"],
                        new GLObjectDataTranslationRotation(new Vector3(9, 0, -10)),
                        rTexture["wooden"]));


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

            ((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).XRotDegrees = degrees;
            ((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).XRotDegrees = degrees;
            ((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).Translate(new Vector3(0.01f, 0, 0));

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


