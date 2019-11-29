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

namespace TestOpenTk
{
    public partial class ShaderTestMain : Form
    {
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        public ShaderTestMain()
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

        public class GLFragmentShaderUniformTest : GLShaderPipelineShadersBase
        {
            private int bindingpoint;

            public string Code()
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
                Program = GLProgram.CompileLink(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, Code(), GetType().Name);
            }

            public override void Start()
            {
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Closed += ShaderTest_Closed;

            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.ZoomDistance = 20F;
            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(110f, 0, 0f), 1F);

            gl3dcontroller.KeyboardTravelSpeed = (ms) =>
            {
                return (float)ms / 100.0f;
            };

            items.Add("COS-1L", new GLColourObjectShaderNoTranslation((a) => { GLStatics.LineWidth(1); }));
            items.Add("TEX", new GLTexturedObjectShaderSimple());
            items.Add("COST-FP", new GLColourObjectShaderTranslation((a) => { GLStatics4.PolygonMode(OpenTK.Graphics.OpenGL4.MaterialFace.FrontAndBack, OpenTK.Graphics.OpenGL4.PolygonMode.Fill); }));
            items.Add("COST-LP", new GLColourObjectShaderTranslation((a) => { GLStatics4.PolygonMode(OpenTK.Graphics.OpenGL4.MaterialFace.FrontAndBack, OpenTK.Graphics.OpenGL4.PolygonMode.Line); }));
            items.Add("COST-1P", new GLColourObjectShaderTranslation((a) => { GLStatics.PointSize(1.0F); }));
            items.Add("COST-2P", new GLColourObjectShaderTranslation((a) => { GLStatics.PointSize(2.0F); }));
            items.Add("COST-10P", new GLColourObjectShaderTranslation((a) => { GLStatics.PointSize(10.0F); }));
            items.Add("CROT", new GLTexturedObjectShaderTransformWithCommonTransform());

            items.Add("dotted", new GLTexture2D(Properties.Resources.dotted));
            items.Add("logo8bpp", new GLTexture2D(Properties.Resources.Logo8bpp));
            items.Add("dotted2", new GLTexture2D(Properties.Resources.dotted2));
            items.Add("wooden", new GLTexture2D(Properties.Resources.wooden));
            items.Add("shoppinglist", new GLTexture2D(Properties.Resources.shoppinglist));
            items.Add("golden", new GLTexture2D(Properties.Resources.golden));
            items.Add("smile", new GLTexture2D(Properties.Resources.smile5300_256x256x8));
            items.Add("moon", new GLTexture2D(Properties.Resources.moonmap1k));


            #region Sphere mapping 
            rObjects.Add(items.Shader("TEX"), "sphere7",
                GLRenderableItem.CreateVector4Vector2(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                        GLSphereObjectFactory.CreateTexturedSphereFromTriangles(4, 4.0f),
                        new GLObjectDataTranslationRotationTexture(items.Tex("moon"), new Vector3(4, 0, 0))
                        ));

            #endregion

            #region coloured lines

            rObjects.Add(items.Shader("COS-1L"),
                         GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                                                    GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(-100, 0, 100), new Vector3(10, 0, 0), 21),
                                                    new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                               );


            rObjects.Add(items.Shader("COS-1L"),
                         GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                               GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(100, 0, -100), new Vector3(0, 0, 10), 21),
                                                         new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                               );
            rObjects.Add(items.Shader("COS-1L"),
                         GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                               GLShapeObjectFactory.CreateLines(new Vector3(-100, 10, -100), new Vector3(-100, 10, 100), new Vector3(10, 0, 0), 21),
                                                         new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                               );
            rObjects.Add(items.Shader("COS-1L"),
                         GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                               GLShapeObjectFactory.CreateLines(new Vector3(-100, 10, -100), new Vector3(100, 10, -100), new Vector3(0, 0, 10), 21),
                                                         new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                               );

            #endregion


            #region Coloured triangles

            rObjects.Add(items.Shader("COST-FP"), "scopen",
                        GLRenderableItem.CreateVector4Color4(items,
                                        OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f, new GLCubeObjectFactory.Sides[] { GLCubeObjectFactory.Sides.Bottom, GLCubeObjectFactory.Sides.Top, GLCubeObjectFactory.Sides.Left, GLCubeObjectFactory.Sides.Right }),
                                        new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange },
                                        new GLObjectDataTranslationRotation(new Vector3(-6, 0, 0))
                        ));


            rObjects.Add(items.Shader("COST-LP"), "scopen-op",
                        GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f, new GLCubeObjectFactory.Sides[] { GLCubeObjectFactory.Sides.Bottom, GLCubeObjectFactory.Sides.Top, GLCubeObjectFactory.Sides.Left, GLCubeObjectFactory.Sides.Right }),
                                        new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange },
                                        new GLObjectDataTranslationRotation(new Vector3(-6, 0, -2))
                        ));

            rObjects.Add(items.Shader("COST-FP"), "sphere1",
                        GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                                    GLSphereObjectFactory.CreateSphereFromTriangles(3, 2.0f),
                                    new Color4[] { Color4.Red, Color4.Green, Color4.Blue, },
                                    new GLObjectDataTranslationRotation(new Vector3(-6, 0, -4))
                        ));

            #endregion

            #region view marker

            rObjects.Add(items.Shader("COST-2P"), "viewpoint",
                GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                               GLCubeObjectFactory.CreateVertexPointCube(0.25f), new Color4[] { Color.Yellow },
                         new GLObjectDataTranslationRotation(new Vector3(-100,-100,-100))
                         ));

            #endregion


            #region coloured points

            rObjects.Add(items.Shader("COST-2P"), "pc",
                        GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                               GLCubeObjectFactory.CreateVertexPointCube(1f), new Color4[] { Color4.Yellow },
                         new GLObjectDataTranslationRotation(new Vector3(-4, 0, 0))
                         ));

            rObjects.Add(items.Shader("COST-2P"), "pc2",
                GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                               GLCubeObjectFactory.CreateVertexPointCube(1f), new Color4[] { Color4.Green, Color4.White, Color4.Purple, Color4.Blue },
                         new GLObjectDataTranslationRotation(new Vector3(-4, 0, -2))
                         ));

            rObjects.Add(items.Shader("COST-2P"), "cp",
                GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                               GLCubeObjectFactory.CreateVertexPointCube(1f), new Color4[] { Color4.Red },
                         new GLObjectDataTranslationRotation(new Vector3(-4, 0, -4))
                         ));

            rObjects.Add(items.Shader("COST-2P"), "dot2-1",
                GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                               GLCubeObjectFactory.CreateVertexPointCube(1f), new Color4[] { Color.Red, Color.Green, Color.Blue, Color.Cyan, Color.Yellow, Color.Yellow, Color.Yellow, Color.Yellow },
                         new GLObjectDataTranslationRotation(new Vector3(-4, 0, -6))
                         ));


            rObjects.Add(items.Shader("COST-2P"), "sphere2",
                GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                               GLSphereObjectFactory.CreateSphereFromTriangles(3, 1.0f), new Color4[] { Color4.Red, Color4.Green, Color4.Blue, },
                        new GLObjectDataTranslationRotation(new Vector3(-4, 0, -8))));

            rObjects.Add(items.Shader("COST-10P"), "sphere3",
                GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                               GLSphereObjectFactory.CreateSphereFromTriangles(1, 1.0f), new Color4[] { Color4.Red, Color4.Green, Color4.Blue, },
                        new GLObjectDataTranslationRotation(new Vector3(-4, 0, -10))));

            rObjects.Add(items.Shader("COST-2P"), "sphere4",
                        GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                               GLSphereObjectFactory.CreateSphereFromTriangles(2, 1.0f), new Color4[] { Color4.Red, Color4.Green, Color4.Blue, },
                            new GLObjectDataTranslationRotation(new Vector3(-4, 0, -12))));

            #endregion


            #region textures

            rObjects.Add(items.Shader("TEX"),
                    GLRenderableItem.CreateVector4Vector2( items, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                            GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                            new GLObjectDataTranslationRotationTexture(items.Tex("dotted2"), new Vector3(-2, 0, 0))
                            ));

            rObjects.Add(items.Shader("TEX"), "EDDCube",
                        GLRenderableItem.CreateVector4Vector2( items, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotationTexture(items.Tex("logo8bpp"), new Vector3(-2, 1, -2))
                        ));

            rObjects.Add(items.Shader("TEX"), "woodbox",
                        GLRenderableItem.CreateVector4Vector2(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotationTexture(items.Tex("wooden"), new Vector3(-2, 2, -4))
                        ));

            rObjects.Add(items.Shader("TEX"),
                        GLRenderableItem.CreateVector4Vector2( items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                        GLShapeObjectFactory.CreateQuad(1.0f, 1.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotationTexture(items.Tex("dotted2"), new Vector3(-2, 3, -6))
                        ));

            rObjects.Add(items.Shader("TEX"),
                    GLRenderableItem.CreateVector4Vector2( items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                        GLShapeObjectFactory.CreateQuad(1.0f, 1.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotationTexture(items.Tex("dotted2"), new Vector3(-2, 4, -8))
                        ));

            rObjects.Add(items.Shader("TEX"),
                GLRenderableItem.CreateVector4Vector2( items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                        GLShapeObjectFactory.CreateQuad(1.0f, 1.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotationTexture(items.Tex("dotted"), new Vector3(-2, 5, -10))
                        ));

            rObjects.Add(items.Shader("TEX"), "EDDFlat",
                GLRenderableItem.CreateVector4Vector2( items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                GLShapeObjectFactory.CreateQuad(2.0f, items.Tex("logo8bpp").Width, items.Tex("logo8bpp").Height, new Vector3(-0, 0, 0)), GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotationTexture(items.Tex("logo8bpp"), new Vector3(0, 0, 0))
                        ));

            rObjects.Add(items.Shader("TEX"),
                GLRenderableItem.CreateVector4Vector2( items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                        GLShapeObjectFactory.CreateQuad(1.5f, new Vector3(-90, 0, 0)), GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotationTexture(items.Tex("smile"), new Vector3(0, 0, -2))
                       ));

            rObjects.Add(items.Shader("CROT"), "woodboxc1",
                    GLRenderableItem.CreateVector4Vector2( items, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                    GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotationTexture(items.Tex("wooden"), new Vector3(0, 0, -4))
                        ));

            rObjects.Add(items.Shader("CROT"), "woodboxc2",
                    GLRenderableItem.CreateVector4Vector2( items, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                    GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotationTexture(items.Tex("wooden"), new Vector3(0, 0, -6))
                       ));

            rObjects.Add(items.Shader("CROT"), "woodboxc3",
                    GLRenderableItem.CreateVector4Vector2( items, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                    GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                        new GLObjectDataTranslationRotationTexture(items.Tex("wooden"), new Vector3(0, 0, -8))
                        ));

            rObjects.Add(items.Shader("TEX"), "sphere5",
                    GLRenderableItem.CreateVector4Vector2( items, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                    GLSphereObjectFactory.CreateTexturedSphereFromTriangles(3, 1.0f),
                        new GLObjectDataTranslationRotationTexture(items.Tex("wooden"), new Vector3(0, 0, -10))
                        ));

            rObjects.Add(items.Shader("CROT"), "sphere6",
                    GLRenderableItem.CreateVector4Vector2( items, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                    GLSphereObjectFactory.CreateTexturedSphereFromTriangles(3, 1.5f),
                        new GLObjectDataTranslationRotationTexture(items.Tex("golden"), new Vector3(0, 0, -12))
                        ));

            #endregion

            #region 2dArrays
            items.Add("TEX2DA", new GLTexturedObjectShader2DBlend());
            items.Add("2DArray2", new GLTexture2DArray(new Bitmap[] { Properties.Resources.mipmap2, Properties.Resources.mipmap2 }, 9));

            rObjects.Add(items.Shader("TEX2DA"), "2DA",
                GLRenderableItem.CreateVector4Vector2(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                            GLShapeObjectFactory.CreateQuad(2.0f), GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotationTexture(items.Tex("2DArray2"), new Vector3(-8, 0, 0))
                        ));


            items.Add("2DArray2-1", new GLTexture2DArray(new Bitmap[] { Properties.Resources.dotted, Properties.Resources.dotted2 }));

            rObjects.Add(items.Shader("TEX2DA"), "2DA-1",
                GLRenderableItem.CreateVector4Vector2(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                                    GLShapeObjectFactory.CreateQuad(2.0f), GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotationTexture(items.Tex("2DArray2-1"), new Vector3(-8, 0, -2))
                        ));

            #endregion


            #region Instancing

            items.Add("IC-1", new GLShaderPipeline(new GLVertexShaderMatrixTranslation(), new GLFragmentShaderColour()));

            GLStatics.PointSize(10);
            Matrix4[] pos1 = new Matrix4[3];
            pos1[0] = Matrix4.CreateTranslation(new Vector3(10, 0, 10));
            pos1[1] = Matrix4.CreateTranslation(new Vector3(10, 5, 10));
            pos1[2] = Matrix4.CreateRotationX(45f.Radians());
            pos1[2] *= Matrix4.CreateTranslation(new Vector3(10, 10, 10));

            rObjects.Add(items.Shader("IC-1"), "1-a",
                                    GLRenderableItem.CreateVector4Matrix4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                                            GLShapeObjectFactory.CreateQuad(2.0f), pos1,
                                            null, pos1.Length));


            Matrix4[] pos2 = new Matrix4[3];
            pos2[0] = Matrix4.CreateRotationX(-80f.Radians());
            pos2[0] *= Matrix4.CreateTranslation(new Vector3(20, 0, 10));
            pos2[1] = Matrix4.CreateRotationX(-70f.Radians());
            pos2[1] *= Matrix4.CreateTranslation(new Vector3(20, 5, 10));
            pos2[2] = Matrix4.CreateRotationZ(-60f.Radians());
            pos2[2] *= Matrix4.CreateTranslation(new Vector3(20, 10, 10));

            items.Add("IC-2", new GLShaderPipeline(new GLVertexShaderTextureMatrixTranslation(), new GLFragmentShaderTexture()));
            items.Shader("IC-2").StartAction += (s) => { items.Tex("wooden").Bind(1); GL.Disable(EnableCap.CullFace); };
            items.Shader("IC-2").FinishAction += (s) => {  GL.Enable(EnableCap.CullFace); };

            rObjects.Add(items.Shader("IC-2"), "1-b",
                                    GLRenderableItem.CreateVector4Vector2Matrix4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                                            GLShapeObjectFactory.CreateQuad(2.0f), GLShapeObjectFactory.TexQuad, pos2,
                                            null, pos2.Length));
            #endregion


            #region Tesselation

            var shdrtesssine = new GLTesselationShaderSinewave(20, 0.5f, 2f);

            shdrtesssine.StartAction += (a) => { items.Tex("logo8bpp").Bind(1); };
            items.Add("TESx1", shdrtesssine);
            rObjects.Add(items.Shader("TESx1"), "O-TES1",
                GLRenderableItem.CreateVector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Patches,
                                    GLShapeObjectFactory.CreateQuad2(10.0f, 10.0f),
//                                    new GLObjectDataTranslationRotationTexture(items.Tex("logo8bpp"), new Vector3(12, 0, 0), new Vector3(-90,0,0))
                                    new GLObjectDataTranslationRotation(new Vector3(12, 0, 0), new Vector3(-90,0,0))
                                    ));

            #endregion


            #region Uniform Block test

            GLStorageBlock b6 = new GLStorageBlock(6);
            items.Add("SB6", b6);
            b6.Write(0.9f);
            b6.Write(0.0f);
            b6.Write(0.0f);
            b6.Complete();

            items.Add("UT-1", new GLShaderPipeline(new GLVertexShaderColourObjectTransform(), new GLFragmentShaderUniformTest(5)));

            rObjects.Add(items.Shader("UT-1"), "UT1",
                    GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                    GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange },
                    new GLObjectDataTranslationRotation(new Vector3(-12, 0, 0))
                    ));

            #endregion


            #region MipMaps

            items.Add("mipmap1", new GLTexture2D(Properties.Resources.mipmap2, 9));

            rObjects.Add(items.Shader("TEX"), "mipmap1",
                GLRenderableItem.CreateVector4Vector2( items, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                                GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                                new GLObjectDataTranslationRotationTexture(items.Tex("mipmap1"), new Vector3(-10, 0, 0))
                        ));

            #endregion

            #region Matrix Calc Uniform

            items.Add("MCUB", new GLMatrixCalcUniformBlock());     // def binding of 0

            #endregion

        }

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(MatrixCalc mc, long time)
        {
            //System.Diagnostics.Debug.WriteLine("Draw");

            float degrees = ((float)time / 5000.0f * 360.0f) % 360f;
            float degreesd2 = ((float)time / 10000.0f * 360.0f) % 360f;
            float degreesd4 = ((float)time / 20000.0f * 360.0f) % 360f;
            float zeroone = (degrees >= 180) ? (1.0f - (degrees - 180.0f) / 180.0f) : (degrees / 180f);

            ((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceControl)).XRotDegrees = degrees;
            ((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceControl)).ZRotDegrees = degrees;

            //((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).Translate(new Vector3(0.01f, 0.01f, 0));
            ((GLObjectDataTranslationRotation)(rObjects["EDDCube"].InstanceControl)).YRotDegrees = degrees;
            ((GLObjectDataTranslationRotation)(rObjects["EDDCube"].InstanceControl)).ZRotDegrees = degreesd2;

            ((GLObjectDataTranslationRotation)(rObjects["sphere3"].InstanceControl)).XRotDegrees = -degrees;
            ((GLObjectDataTranslationRotation)(rObjects["sphere3"].InstanceControl)).YRotDegrees = degrees;
            ((GLObjectDataTranslationRotation)(rObjects["sphere4"].InstanceControl)).YRotDegrees = degrees;
            ((GLObjectDataTranslationRotation)(rObjects["sphere4"].InstanceControl)).ZRotDegrees = -degreesd2;
            ((GLObjectDataTranslationRotation)(rObjects["sphere7"].InstanceControl)).YRotDegrees = degreesd4;

            ((GLVertexShaderTextureTransformWithCommonTransform)items.Shader("CROT").Get(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader)).Transform.YRotDegrees = degrees;
            ((GLFragmentShaderTexture2DBlend)items.Shader("TEX2DA").Get(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader)).Blend = zeroone;

            items.SB("SB6").Write(zeroone, 4, true);

            ((GLObjectDataTranslationRotation)(rObjects["viewpoint"].InstanceControl)).Position = gl3dcontroller.Pos.Lookat;

            ((GLTesselationShaderSinewave)items.Shader("TESx1")).Phase = degrees / 360.0f;

            GLMatrixCalcUniformBlock mcub = (GLMatrixCalcUniformBlock)items.UB("MCUB");
            mcub.Set(gl3dcontroller.MatrixCalc);

            rObjects.Render(gl3dcontroller.MatrixCalc);

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " dir " + gl3dcontroller.Camera.Current + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance;
        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboard(true, OtherKeys);
            gl3dcontroller.Redraw();
        }

        private void OtherKeys( BaseUtils.KeyboardState kb )
        {
            if (kb.IsPressedRemove(Keys.F1, BaseUtils.KeyboardState.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(0, 0, 0), 1, 2);
            }

            if (kb.IsPressedRemove(Keys.F2, BaseUtils.KeyboardState.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(4, 0, 0), 1, 2);
            }

            if (kb.IsPressedRemove(Keys.F3, BaseUtils.KeyboardState.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(10, 0, -10), 1, 2);
            }

            if (kb.IsPressedRemove(Keys.F4, BaseUtils.KeyboardState.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(50, 0, 50), 1, 2);
            }

        }

    }



}


