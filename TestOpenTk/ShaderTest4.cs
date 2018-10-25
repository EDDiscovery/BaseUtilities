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
        GLUniformBlockList rUniformBlocks = new GLUniformBlockList();

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


            rShaders.Add("COS", new GLColourObjectShaderNoTranslation());       // shader simple with translation
            rShaders.Add("COST", new GLColourObjectShaderTranslation());       // shader simple with translation
            rShaders.Add("TEX", new GLTexturedObjectShaderSimple());
            rShaders.Add("CROT", new GLTexturedObjectShaderTransformWithCommonTransform());

            var translationvertex = new GLVertexShaderTransform();
            var simplefragment = new GLFragmentShaderPassThru();
            rShaders.Add("PIPE1", new GLProgramShaderPipeline(translationvertex, simplefragment));

            #region Uniform

            rUniformBlocks.Add(new GLUniformBlock(5));
            rUniformBlocks[5].Write(20);
            //rUniformBlocks[5].Write(new float[] { 1.0f, 0, 0, 0,  0, 1.0f, 0,  0, 0, 0, 1.0f } );

            Vector3[] cvalues = new Vector3[100];
            for (int i = 0; i < 100; i++)
                cvalues[i] = new Vector3(0, 0, 0);
            cvalues[0] = new Vector3(1, 0, 0);
            cvalues[10] = new Vector3(0, 1, 0);
            cvalues[20] = new Vector3(0, 0, 1);

            rUniformBlocks[5].Write(cvalues );
            //            rUniformBlocks[5].Write(new Vector3(0.0f, 0.9f, 0.0f));
            rUniformBlocks[5].Complete();

            //rUniformBlocks.Add(new GLUniformBlock(6));
            //rUniformBlocks[6].Write(new Vector3(0.9f, 0.0f, 0.0f));
            //rUniformBlocks[6].Complete();


            rShaders.Add("UT-1", new GLProgramShaderPipeline(new GLVertexShaderTransform(), new GLFragmentShaderUniformTest(5)));
           // rShaders.Add("UT-2", new GLProgramShaderPipeline(new GLVertexShaderTransform(), new GLFragmentShaderUniformTest(6)));

            rObjects.Add(rShaders["UT-1"], "UT1", new GLColouredTriangles(
                    GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f),
                    new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange },
                    new GLObjectDataTranslationRotation(new Vector3(0, 0, -13))));

            //rObjects.Add(rShaders["UT-2"], "UT2", new GLColouredTriangles(
            //        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f),
            //        new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange },
            //        new GLObjectDataTranslationRotation(new Vector3(2, 0, -13))));

            #endregion


            #region MipMaps

            rTexture.Add("mipmap1", new GLTexture2D(Properties.Resources.mipmap, 9));
            //rTexture.Add("mipmap2", new GLTexture2D(Properties.Resources.mipmap2, 9));

            rObjects.Add(rShaders["TEX"], "mipmap1", new GLTexturedTriangles(
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotation(new Vector3(3, 0, -5)),
                        rTexture["mipmap1"]));

            #endregion

            #region 2dArrays
            rShaders.Add("TEX2DA", new GLTexturedObjectShader2DBlend());

            rTexture.Add("2DArray2", new GLTexture2DArray(new Bitmap[] { Properties.Resources.mipmap, Properties.Resources.mipmap2 }, 9));

            //rTexture.Add("2DArray1", new GLTexture2DArray(new Bitmap[] { Properties.Resources.dotted, Properties.Resources.dotted2 }, 1));

            rObjects.Add(rShaders["TEX2DA"], "2DA",
                new GLTexturedQuads(
                        GLShapeObjectFactory.CreateQuad(2.0f),
                        GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotation(new Vector3(-10, 0, -10)),
                        rTexture["2DArray2"]));

            rTexture.Add("2DArray2-1", new GLTexture2DArray(new Bitmap[] { Properties.Resources.dotted, Properties.Resources.dotted2 }));

            //rTexture.Add("2DArray1", new GLTexture2DArray(new Bitmap[] { Properties.Resources.dotted, Properties.Resources.dotted2 }, 1));

            rObjects.Add(rShaders["TEX2DA"], "2DA-1",
                new GLTexturedQuads(
                        GLShapeObjectFactory.CreateQuad(2.0f),
                        GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotation(new Vector3(-10, 0, -12)),
                        rTexture["2DArray2-1"]));
            #endregion

#if true
            #region Coloured triangles

            rObjects.Add(rShaders["COST"], "scopen", new GLColouredTriangles(
                    GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f, new GLCubeObjectFactory.Sides[] { GLCubeObjectFactory.Sides.Bottom, GLCubeObjectFactory.Sides.Top, GLCubeObjectFactory.Sides.Left, GLCubeObjectFactory.Sides.Right }),
                    new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange },
                    new GLObjectDataTranslationRotation(new Vector3(2, 0, 0))));

            rObjects.Add(rShaders["COST"], "scopen-op", new GLOutlineTriangles(
                    GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f, new GLCubeObjectFactory.Sides[] { GLCubeObjectFactory.Sides.Bottom, GLCubeObjectFactory.Sides.Top, GLCubeObjectFactory.Sides.Left, GLCubeObjectFactory.Sides.Right }), 
                    new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange }, 
                    new GLObjectDataTranslationRotation(new Vector3(3, 0, 0))));

            rObjects.Add(rShaders["COST"], "sphere3-tri", new GLColouredTriangles(
                        GLSphereObjectFactory.CreateSphereFromTriangles(3, 2.0f),
                        new Color4[] { Color4.Red, Color4.Green, Color4.Blue, },
                        new GLObjectDataTranslationRotation(new Vector3(-4, 0, -12))));

            #endregion

            #region coloured points

            rObjects.Add(rShaders["COST"], "pc", new GLColouredPoints(
                         GLCubeObjectFactory.CreateVertexPointCube(1f)
                         , new Color4[] { Color4.Yellow },
                         new GLObjectDataTranslationRotation(new Vector3(10, 0, 0)),
                         10f));

            rObjects.Add(rShaders["PIPE1"], "pc2", new GLColouredPoints(
                         GLCubeObjectFactory.CreateVertexPointCube(2f),
                         new Color4[] { Color4.Green, Color4.White, Color4.Purple, Color4.Blue },
                         new GLObjectDataTranslationRotation(new Vector3(15, 0, 0)),
                         10f));

            rObjects.Add(rShaders["COST"], "cp", new GLColouredPoints(
                         GLCubeObjectFactory.CreateVertexPointCube(1f),
                         new Color4[] { Color4.Red },
                         new GLObjectDataTranslationRotation(new Vector3(-10, 0, 0)),
                         10f));

            rObjects.Add(rShaders["COST"], "dot2-1", new GLColouredPoints(
                         GLCubeObjectFactory.CreateVertexPointCube(1f),
                         new Color4[] { Color.Red, Color.Green, Color.Blue, Color.Cyan, Color.Yellow, Color.Yellow, Color.Yellow, Color.Yellow },
                         new GLObjectDataTranslationRotation(new Vector3(-14, 0, 0)),
                         10f));


            rObjects.Add(rShaders["COST"], "sphere3", new GLColouredPoints(
                        GLSphereObjectFactory.CreateSphereFromTriangles(3, 2.0f),
                        new Color4[] { Color4.Red, Color4.Green, Color4.Blue, },
                        new GLObjectDataTranslationRotation(new Vector3(-4, 0, -6)), 5.0f));

            rObjects.Add(rShaders["COST"], "sphere4", new GLColouredPoints(
                        GLSphereObjectFactory.CreateSphereFromTriangles(1, 1.0f),
                        new Color4[] { Color4.Red, Color4.Green, Color4.Blue, },
                        new GLObjectDataTranslationRotation(new Vector3(-4, 0, -8)), 5.0f));

            rObjects.Add(rShaders["COST"], "sphere5", new GLColouredPoints(
                        GLSphereObjectFactory.CreateSphereFromTriangles(2, 1.0f),
                        new Color4[] { Color4.Red, Color4.Green, Color4.Blue, },
                        new GLObjectDataTranslationRotation(new Vector3(-4, 0, -10)), 5.0f));

            #endregion

            #region coloured lines

            rObjects.Add(rShaders["COS"], new GLColouredLines(
                        GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(-100, 0, 100), new Vector3(10, 0, 0), 21),
                        new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green },
                        null,       // lines are positioned directly.. no need for object data binding
                        1f));

            rObjects.Add(rShaders["COS"], new GLColouredLines(
                        GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(100, 0, -100), new Vector3(0, 0, 10), 21),
                        new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green },
                        null,       // lines are positioned directly.. no need for object data binding
                        1f));

            rObjects.Add(rShaders["COS"], new GLColouredLines(
                        GLShapeObjectFactory.CreateLines(new Vector3(-100, 10, -100), new Vector3(-100, 10, 100), new Vector3(10, 0, 0), 21),
                        new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green },
                        null,       // lines are positioned directly.. no need for object data binding
                        1f));

            rObjects.Add(rShaders["COS"], new GLColouredLines(
                        GLShapeObjectFactory.CreateLines(new Vector3(-100, 10, -100), new Vector3(100, 10, -100), new Vector3(0, 0, 10), 21),
                        new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green },
                        null,       // lines are positioned directly.. no need for object data binding
                        1f));

            #endregion

            #region textures

            rTexture.Add("dotted", new GLTexture2D(Properties.Resources.dotted));
            rTexture.Add("logo8bpp", new GLTexture2D(Properties.Resources.Logo8bpp));
            rTexture.Add("dotted2", new GLTexture2D(Properties.Resources.dotted2));
            rTexture.Add("wooden", new GLTexture2D(Properties.Resources.wooden));
            rTexture.Add("shoppinglist", new GLTexture2D(Properties.Resources.shoppinglist));
            rTexture.Add("golden", new GLTexture2D(Properties.Resources.golden));

            rObjects.Add(rShaders["TEX"], new GLTexturedTriangles(
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotation(new Vector3(10, 0, -5)),
                        rTexture["dotted2"]));

            rObjects.Add(rShaders["TEX"], "EDDCube", new GLTexturedTriangles(
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(3f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotation(new Vector3(15, 0, -8)),
                        rTexture["logo8bpp"]));

            rObjects.Add(rShaders["TEX"], "woodbox", new GLTexturedTriangles(
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotation(new Vector3(5, 0, -5)),
                        rTexture["wooden"]));

            rObjects.Add(rShaders["TEX"], new GLTexturedQuads(
                        GLShapeObjectFactory.CreateQuad(1.0f, 1.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotation(new Vector3(0, 0, -5)),
                        rTexture["dotted2"]));

            rObjects.Add(rShaders["TEX"], new GLTexturedQuads(
                        GLShapeObjectFactory.CreateQuad(1.0f, 1.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotation(new Vector3(1, 0, -7)),
                        rTexture["dotted2"]));

            rObjects.Add(rShaders["TEX"], new GLTexturedQuads(
                        GLShapeObjectFactory.CreateQuad(1.0f, 1.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotation(new Vector3(-5, 0, 0)),
                        rTexture["dotted"]));

            rObjects.Add(rShaders["TEX"], "EDDFlat", new GLTexturedQuads(
                        GLShapeObjectFactory.CreateQuad(4.0f, rTexture["logo8bpp"].Width, rTexture["logo8bpp"].Height, new Vector3(-0, 0, 0)),
                        GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotation(new Vector3(-10, 0, -5)),
                        rTexture["logo8bpp"]));

            rObjects.Add(rShaders["TEX"], new GLTexturedQuads(
                        GLShapeObjectFactory.CreateQuad(4.0f, new Vector3(-90, 0, 0)),
                        GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotation(new Vector3(-15, 0, -5)),
                        rTexture["shoppinglist"]));

            rObjects.Add(rShaders["CROT"], "woodboxc1", new GLTexturedTriangles(
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotation(new Vector3(5, 0, -10)),
                        rTexture["wooden"]));

            rObjects.Add(rShaders["CROT"], "woodboxc2", new GLTexturedTriangles(
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotation(new Vector3(7, 0, -10)),
                        rTexture["wooden"]));

            rObjects.Add(rShaders["CROT"], "woodboxc3", new GLTexturedTriangles(
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotation(new Vector3(9, 0, -10)),
                        rTexture["wooden"]));

            rObjects.Add(rShaders["TEX"], "sphere1", new GLTexturedTriangles(
                        GLSphereObjectFactory.CreateTexturedSphereFromTriangles(3, 1.0f),
                        new GLObjectDataTranslationRotation(new Vector3(0, 0, -10)),
                        rTexture["wooden"]));

            rObjects.Add(rShaders["CROT"], "sphere2", new GLTexturedTriangles(
                        GLSphereObjectFactory.CreateTexturedSphereFromTriangles(3, 1.0f),
                        new GLObjectDataTranslationRotation(new Vector3(-4, 0, -4)),
                        rTexture["golden"]));

            #endregion
#endif

            GL.PatchParameter(PatchParameterInt.PatchVertices, 3);
            GL.Enable(EnableCap.DepthTest);

            Closed += ShaderTest_Closed;
        }

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            rObjects.Dispose();
            rTexture.Dispose();
            rShaders.Dispose();
            rUniformBlocks.Dispose();
        }

        private void ControllerDraw(Matrix4 model, Matrix4 projection, long time)
        {
            //System.Diagnostics.Debug.WriteLine("Draw");

            float degrees = ((float)time / 5000.0f * 360.0f) % 360f;
            float degreesd2 = ((float)time / 10000.0f * 360.0f) % 360f;

            ((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).XRotDegrees = degrees;
            ((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).ZRotDegrees = degrees;

            ((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).Translate(new Vector3(0.01f, 0.01f, 0));
            ((GLObjectDataTranslationRotation)(rObjects["sphere3"].InstanceData)).XRotDegrees = -degrees;
            ((GLObjectDataTranslationRotation)(rObjects["sphere3"].InstanceData)).YRotDegrees = degrees;
            ((GLObjectDataTranslationRotation)(rObjects["sphere3-tri"].InstanceData)).YRotDegrees = degrees;
            ((GLObjectDataTranslationRotation)(rObjects["sphere3-tri"].InstanceData)).ZRotDegrees = -degreesd2;
            ((GLObjectDataTranslationRotation)(rObjects["EDDCube"].InstanceData)).YRotDegrees = degrees;
            ((GLObjectDataTranslationRotation)(rObjects["EDDCube"].InstanceData)).ZRotDegrees = degreesd2;

            ((GLVertexShaderTransformWithCommonTransform)rShaders["CROT"].GetVertex()).Transform.YRotDegrees = degrees;
            ((GLFragmentShader2DCommonBlend)rShaders["TEX2DA"].GetFragment()).Blend = (degrees>=180) ? (1.0f-(degrees-180.0f) / 180.0f) : (degrees/180f);

            //rUniformBlocks[6].Write(new Vector3(degrees/360.0f, 0.4f, 0.0f),0,true);

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


