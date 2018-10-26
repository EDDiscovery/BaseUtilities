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
        GLItemsList items = new GLItemsList();

        public class GLFragmentShaderUniformTest : GLFragmentShadersBase
        {
            private int bindingpoint;

            public override string Code()
            {
                return
    @"
#version 450 core
out vec4 color;

layout( std140, binding =" + bindingpoint.ToString() + @") uniform DataInBlock
{
    int index;
    vec3[100] c;
} datainblock;

in vec4 vs_color;

layout( binding = 6, std140) buffer storagebuffer
{
float red;
float green;
float blue;
};
    

void main(void)
{
	color = vs_color;
    int id = datainblock.index;
    float r = datainblock.c[id].x;
    float g = datainblock.c[id].y;
    float b = datainblock.c[id].z;

    color = vec4(r,g,b,1.0f);

    //color = vec4(storagebuffer.red,storagebuffer.green,storagebuffer.blue,1.0f);
   color = vec4(red,green,blue,1f);
}
";
            }

            public GLFragmentShaderUniformTest(int bp)
            {
                bindingpoint = bp;
                CompileLink();
            }

            public override void Start(MatrixCalc c)
            {
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.MatrixCalc.ZoomDistance = 20F;
            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(45f, 0, 0f), 1F);
           // gl3dcontroller.MatrixCalc.ZoomDistance = 5F;
           // gl3dcontroller.Start(new Vector3(10, 0, 0), new Vector3(90f, 0, 0f), 1F);

            gl3dcontroller.TravelSpeed = (ms) =>
            {
                return (float)ms / 100.0f;
            };


            items.Add("COS", new GLColourObjectShaderNoTranslation());
            items.Add("COST", new GLColourObjectShaderTranslation());
            items.Add("TEX", new GLTexturedObjectShaderSimple());
            items.Add("CROT", new GLTexturedObjectShaderTransformWithCommonTransform());
            items.Add("PIPE1", new GLProgramShaderPipeline(new GLVertexShaderColourObjectTransform(), new GLFragmentShaderColour()));
            items.Add("TES1", new GLTesselationShadersExample());

            items.Add("dotted", new GLTexture2D(Properties.Resources.dotted));
            items.Add("logo8bpp", new GLTexture2D(Properties.Resources.Logo8bpp));
            items.Add("dotted2", new GLTexture2D(Properties.Resources.dotted2));
            items.Add("wooden", new GLTexture2D(Properties.Resources.wooden));
            items.Add("shoppinglist", new GLTexture2D(Properties.Resources.shoppinglist));
            items.Add("golden", new GLTexture2D(Properties.Resources.golden));
            items.Add("smile", new GLTexture2D(Properties.Resources.smile5300_256x256x8));
            items.Add("moon", new GLTexture2D(Properties.Resources.moonmap1k));

            rObjects.Add(items.Shader("COS"), "O-TES1",  new GLVertexPoints(   GLShapeObjectFactory.CreateQuad(2.0f), null, 2.0f));



#if false
            #region Uniform Block test
            //GLUniformBlock b5 = new GLUniformBlock(5);        // keep test
            //items.Add("UB5", b5);
            //b5.Write(0);

            //Vector3[] cvalues = new Vector3[100];
            //for (int i = 0; i < 100; i++)
            //    cvalues[i] = new Vector3(((float)i)/100.0f, 0.2f, ((float)i)/100.0f);

            //b5.Write(cvalues );
            //b5.Complete();

            GLStorageBlock b6 = new GLStorageBlock(6);
            items.Add("SB6", b6);
            b6.Write(0.9f);
            b6.Write(0.0f);
            b6.Write(0.0f);
            b6.Complete();

            items.Add("UT-1", new GLProgramShaderPipeline(new GLVertexShaderColourObjectTransform(), new GLFragmentShaderUniformTest(5)));

            rObjects.Add(items.Shader("UT-1"), "UT1", new GLColouredTriangles(
                    GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f),
                    new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange },
                    new GLObjectDataTranslationRotation(new Vector3(-12, 0, 0))));

            #endregion


            #region MipMaps

            items.Add("mipmap1", new GLTexture2D(Properties.Resources.mipmap, 9));
            //items.Add("mipmap2", new GLTexture2D(Properties.Resources.mipmap2, 9));

            rObjects.Add(items.Shader("TEX"), "mipmap1", new GLTexturedTriangles(
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotation(new Vector3(-10, 0, 0)),
                        items.Tex("mipmap1"),1));

            #endregion

            #region 2dArrays
            items.Add("TEX2DA", new GLTexturedObjectShader2DBlend());

            items.Add("2DArray2", new GLTexture2DArray(1,new Bitmap[] { Properties.Resources.mipmap, Properties.Resources.mipmap2 }, 9));

            //items.Add("2DArray1", new GLTexture2DArray(new Bitmap[] { Properties.Resources.dotted, Properties.Resources.dotted2 }, 1));

            rObjects.Add(items.Shader("TEX2DA"), "2DA",
                new GLTexturedQuads(
                        GLShapeObjectFactory.CreateQuad(2.0f),
                        GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotation(new Vector3(-8, 0, 0)),
                        items.Tex("2DArray2"),1));

            items.Add("2DArray2-1", new GLTexture2DArray(1,new Bitmap[] { Properties.Resources.dotted, Properties.Resources.dotted2 }));

            //items.Add("2DArray1", new GLTexture2DArray(new Bitmap[] { Properties.Resources.dotted, Properties.Resources.dotted2 }, 1));

            rObjects.Add(items.Shader("TEX2DA"), "2DA-1",
                new GLTexturedQuads(
                        GLShapeObjectFactory.CreateQuad(2.0f),
                        GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotation(new Vector3(-8, 0, -2)),
                        items.Tex("2DArray2-1"),1));
            #endregion

            #region Coloured triangles

            rObjects.Add(items.Shader("COST"), "scopen", new GLColouredTriangles(
                    GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f, new GLCubeObjectFactory.Sides[] { GLCubeObjectFactory.Sides.Bottom, GLCubeObjectFactory.Sides.Top, GLCubeObjectFactory.Sides.Left, GLCubeObjectFactory.Sides.Right }),
                    new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange },
                    new GLObjectDataTranslationRotation(new Vector3(-6, 0, 0))));

            rObjects.Add(items.Shader("COST"), "scopen-op", new GLOutlineTriangles(
                    GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f, new GLCubeObjectFactory.Sides[] { GLCubeObjectFactory.Sides.Bottom, GLCubeObjectFactory.Sides.Top, GLCubeObjectFactory.Sides.Left, GLCubeObjectFactory.Sides.Right }), 
                    new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange }, 
                    new GLObjectDataTranslationRotation(new Vector3(-6, 0, -2))));

            rObjects.Add(items.Shader("COST"), "sphere1", new GLColouredTriangles(
                        GLSphereObjectFactory.CreateSphereFromTriangles(3, 2.0f),
                        new Color4[] { Color4.Red, Color4.Green, Color4.Blue, },
                        new GLObjectDataTranslationRotation(new Vector3(-6, 0, -4))));

            #endregion

            #region coloured points

            rObjects.Add(items.Shader("COST"), "pc", new GLColouredPoints(
                         GLCubeObjectFactory.CreateVertexPointCube(1f)
                         , new Color4[] { Color4.Yellow },
                         new GLObjectDataTranslationRotation(new Vector3(-4, 0, 0)),
                         10f));

            rObjects.Add(items.Shader("PIPE1"), "pc2", new GLColouredPoints(
                         GLCubeObjectFactory.CreateVertexPointCube(1f),
                         new Color4[] { Color4.Green, Color4.White, Color4.Purple, Color4.Blue },
                         new GLObjectDataTranslationRotation(new Vector3(-4, 0, -2)),
                         10f));

            rObjects.Add(items.Shader("COST"), "cp", new GLColouredPoints(
                         GLCubeObjectFactory.CreateVertexPointCube(1f),
                         new Color4[] { Color4.Red },
                         new GLObjectDataTranslationRotation(new Vector3(-4, 0, -4)),
                         10f));

            rObjects.Add(items.Shader("COST"), "dot2-1", new GLColouredPoints(
                         GLCubeObjectFactory.CreateVertexPointCube(1f),
                         new Color4[] { Color.Red, Color.Green, Color.Blue, Color.Cyan, Color.Yellow, Color.Yellow, Color.Yellow, Color.Yellow },
                         new GLObjectDataTranslationRotation(new Vector3(-4, 0, -6)),
                         10f));


            rObjects.Add(items.Shader("COST"), "sphere2", new GLColouredPoints(
                        GLSphereObjectFactory.CreateSphereFromTriangles(3, 1.0f),
                        new Color4[] { Color4.Red, Color4.Green, Color4.Blue, },
                        new GLObjectDataTranslationRotation(new Vector3(-4, 0, -8)), 5.0f));

            rObjects.Add(items.Shader("COST"), "sphere3", new GLColouredPoints(
                        GLSphereObjectFactory.CreateSphereFromTriangles(1, 1.0f),
                        new Color4[] { Color4.Red, Color4.Green, Color4.Blue, },
                        new GLObjectDataTranslationRotation(new Vector3(-4, 0, -10)), 5.0f));

            rObjects.Add(items.Shader("COST"), "sphere4", new GLColouredPoints(
                        GLSphereObjectFactory.CreateSphereFromTriangles(2, 1.0f),
                        new Color4[] { Color4.Red, Color4.Green, Color4.Blue, },
                        new GLObjectDataTranslationRotation(new Vector3(-4, 0, -12)), 5.0f));

            #endregion

            #region coloured lines

            rObjects.Add(items.Shader("COS"), new GLColouredLines(
                        GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(-100, 0, 100), new Vector3(10, 0, 0), 21),
                        new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green },
                        null,       // lines are positioned directly.. no need for object data binding
                        1f));

            rObjects.Add(items.Shader("COS"), new GLColouredLines(
                        GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(100, 0, -100), new Vector3(0, 0, 10), 21),
                        new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green },
                        null,       // lines are positioned directly.. no need for object data binding
                        1f));

            rObjects.Add(items.Shader("COS"), new GLColouredLines(
                        GLShapeObjectFactory.CreateLines(new Vector3(-100, 10, -100), new Vector3(-100, 10, 100), new Vector3(10, 0, 0), 21),
                        new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green },
                        null,       // lines are positioned directly.. no need for object data binding
                        1f));

            rObjects.Add(items.Shader("COS"), new GLColouredLines(
                        GLShapeObjectFactory.CreateLines(new Vector3(-100, 10, -100), new Vector3(100, 10, -100), new Vector3(0, 0, 10), 21),
                        new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green },
                        null,       // lines are positioned directly.. no need for object data binding
                        1f));

            #endregion

            #region textures

            rObjects.Add(items.Shader("TEX"), new GLTexturedTriangles(
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotation(new Vector3(-2, 0, 0)),
                        items.Tex("dotted2"),1));

            rObjects.Add(items.Shader("TEX"), "EDDCube", new GLTexturedTriangles(
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotation(new Vector3(-2, 0, -2)),
                        items.Tex("logo8bpp"),1));

            rObjects.Add(items.Shader("TEX"), "woodbox", new GLTexturedTriangles(
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotation(new Vector3(-2, 0, -4)),
                        items.Tex("wooden"),1));

            rObjects.Add(items.Shader("TEX"), new GLTexturedQuads(
                        GLShapeObjectFactory.CreateQuad(1.0f, 1.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotation(new Vector3(-2, 0, -6)),
                        items.Tex("dotted2"),1));

            rObjects.Add(items.Shader("TEX"), new GLTexturedQuads(
                        GLShapeObjectFactory.CreateQuad(1.0f, 1.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotation(new Vector3(-2, 0, -8)),
                        items.Tex("dotted2"),1));

            rObjects.Add(items.Shader("TEX"), new GLTexturedQuads(
                        GLShapeObjectFactory.CreateQuad(1.0f, 1.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotation(new Vector3(-2, 0, -10)),
                        items.Tex("dotted"),1));

            rObjects.Add(items.Shader("TEX"), "EDDFlat", new GLTexturedQuads(
                        GLShapeObjectFactory.CreateQuad(2.0f, items.Tex("logo8bpp").Width, items.Tex("logo8bpp").Height, new Vector3(-0, 0, 0)),
                        GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotation(new Vector3(0, 0, 0)),
                        items.Tex("logo8bpp"),1));



            rObjects.Add(items.Shader("TEX"), new GLTexturedQuads(
                        GLShapeObjectFactory.CreateQuad(1.5f, new Vector3(-90, 0, 0)),
                        GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotation(new Vector3(0, 0, -2)),
                        items.Tex("smile"),1));

            rObjects.Add(items.Shader("CROT"), "woodboxc1", new GLTexturedTriangles(
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotation(new Vector3(0, 0, -4)),
                        items.Tex("wooden"),1));

            rObjects.Add(items.Shader("CROT"), "woodboxc2", new GLTexturedTriangles(
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotation(new Vector3(0, 0, -6)),
                        items.Tex("wooden"),1));

            rObjects.Add(items.Shader("CROT"), "woodboxc3", new GLTexturedTriangles(
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotation(new Vector3(0, 0, -8)),
                        items.Tex("wooden"),1));

            rObjects.Add(items.Shader("TEX"), "sphere5", new GLTexturedTriangles(
                        GLSphereObjectFactory.CreateTexturedSphereFromTriangles(3, 1.0f),
                        new GLObjectDataTranslationRotation(new Vector3(0, 0, -10)),
                        items.Tex("wooden"),1));

            rObjects.Add(items.Shader("CROT"), "sphere6", new GLTexturedTriangles(
                        GLSphereObjectFactory.CreateTexturedSphereFromTriangles(3, 1.0f),
                        new GLObjectDataTranslationRotation(new Vector3(0, 0, -12)),
                        items.Tex("golden"),1));

            #endregion

            #region Sphere mapping 
            rObjects.Add(items.Shader("TEX"), "sphere7", new GLTexturedTriangles(
                        GLSphereObjectFactory.CreateTexturedSphereFromTriangles(4, 4.0f),
                        new GLObjectDataTranslationRotation(new Vector3(4, 0, 0)),
                        items.Tex("moon"), 1));

            #endregion
#endif
            GL.PatchParameter(PatchParameterInt.PatchVertices, 3);
            GL.Enable(EnableCap.DepthTest);
          //  GL.FrontFace(FrontFaceDirection.Ccw);
            //GL.Enable(EnableCap.CullFace);

            Closed += ShaderTest_Closed;
        }

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            rObjects.Dispose();
            items.Dispose();
        }

        private void ControllerDraw(Matrix4 model, Matrix4 projection, long time)
        {
            //System.Diagnostics.Debug.WriteLine("Draw");

            float degrees = ((float)time / 5000.0f * 360.0f) % 360f;
            float degreesd2 = ((float)time / 10000.0f * 360.0f) % 360f;
            float degreesd4 = ((float)time / 20000.0f * 360.0f) % 360f;
            float zeroone = (degrees >= 180) ? (1.0f - (degrees - 180.0f) / 180.0f) : (degrees / 180f);

            //((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).XRotDegrees = degrees;
            //((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).ZRotDegrees = degrees;

            //((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).Translate(new Vector3(0.01f, 0.01f, 0));
            //((GLObjectDataTranslationRotation)(rObjects["EDDCube"].InstanceData)).YRotDegrees = degrees;
            //((GLObjectDataTranslationRotation)(rObjects["EDDCube"].InstanceData)).ZRotDegrees = degreesd2;

            //((GLObjectDataTranslationRotation)(rObjects["sphere3"].InstanceData)).XRotDegrees = -degrees;
            //((GLObjectDataTranslationRotation)(rObjects["sphere3"].InstanceData)).YRotDegrees = degrees;
            //((GLObjectDataTranslationRotation)(rObjects["sphere4"].InstanceData)).YRotDegrees = degrees;
            //((GLObjectDataTranslationRotation)(rObjects["sphere4"].InstanceData)).ZRotDegrees = -degreesd2;
            //((GLObjectDataTranslationRotation)(rObjects["sphere7"].InstanceData)).YRotDegrees = degreesd4;

            //((GLVertexShaderTextureTransformWithCommonTransform)items.Shader("CROT").Get(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader)).Transform.YRotDegrees = degrees;
            //((GLFragmentShader2DCommonBlend)items.Shader("TEX2DA").Get(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader)).Blend = zeroone;

            ////items.UB("UB5").Write((int)(zeroone * 99f), 0, true);
            //items.SB("SB6").Write(zeroone, 4, true);

            rObjects.Render(gl3dcontroller.MatrixCalc);

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " dir " + gl3dcontroller.Camera.Current + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance;
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


