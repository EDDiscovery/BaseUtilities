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
    public partial class ShaderTest1 : Form
    {
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        public ShaderTest1()
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

            public override void Start(MatrixCalc c)
            {
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.MatrixCalc.ZoomDistance = 20F;
            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(110f, 0, 0f), 1F);

            gl3dcontroller.TravelSpeed = (ms) =>
            {
                return (float)ms / 100.0f;
            };

            items.Add("COS-1L", new GLColourObjectShaderNoTranslation((a) => { GLStatics.LineWidth(1); }));
            items.Add("TEX", new GLTexturedObjectShaderSimple());
            items.Add("COST-FP", new GLColourObjectShaderTranslation((a) => { GL4Statics.PolygonMode(OpenTK.Graphics.OpenGL4.MaterialFace.FrontAndBack, OpenTK.Graphics.OpenGL4.PolygonMode.Fill); }));
            items.Add("COST-LP", new GLColourObjectShaderTranslation((a) => { GL4Statics.PolygonMode(OpenTK.Graphics.OpenGL4.MaterialFace.FrontAndBack, OpenTK.Graphics.OpenGL4.PolygonMode.Line); }));
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

            #region Instancing

            items.Add("IC-1", new GLShaderPipeline(new GLVertexShaderMatrixTranslation(), new GLFragmentShaderFixedColour(new Color4(0.5F, 0.5F, 0.0F, 1.0F))));

            Matrix4[] pos1 = new Matrix4[3];
            pos1[0] = Matrix4.CreateTranslation(new Vector3(10, 0, 10));
            pos1[1] = Matrix4.CreateTranslation(new Vector3(10, 5, 10));
            pos1[2] = Matrix4.CreateRotationX(45f.Radians());
            pos1[2] *= Matrix4.CreateTranslation(new Vector3(10, 10, 10));

            var tvi = new GLVertexInstancedTransformObject(GLShapeObjectFactory.CreateQuad(2.0f), pos1);
            rObjects.Add(items.Shader("IC-1"), "1-a",
                                    new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                                            tvi,
                                            null, pos1.Length));
            Matrix4[] pos2 = new Matrix4[3];
            pos2[0] = Matrix4.CreateRotationX(-80f.Radians());
            pos2[0] *= Matrix4.CreateTranslation(new Vector3(20, 0, 10));
            pos2[1] = Matrix4.CreateRotationX(-70f.Radians());
            pos2[1] *= Matrix4.CreateTranslation(new Vector3(20, 5, 10));
            pos2[2] = Matrix4.CreateRotationX(-60f.Radians());
            pos2[2] *= Matrix4.CreateTranslation(new Vector3(20, 10, 10));


            items.Add("IC-2", new GLShaderPipeline(new GLVertexShaderTextureMatrixTranslation(), new GLFragmentShaderTexture()));
            items.Shader("IC-2").StartAction += (s) => { items.Tex("wooden").Bind(1); GL.Disable(EnableCap.CullFace); };
            items.Shader("IC-2").FinishAction += (s) => { items.Tex("wooden").Bind(1); GL.Enable(EnableCap.CullFace); };

            var tvi2 = new GLVertexInstancedTexCoordsTransformObject(GLShapeObjectFactory.CreateQuad(2.0f), GLShapeObjectFactory.TexQuad, pos2);
            rObjects.Add(items.Shader("IC-2"), "1-b",
                                    new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                                            tvi2,
                                            null, pos2.Length));
            #endregion

            #region Tesselation
            items.Add("TESx1", new GLTesselationShaderSinewave(20,0.5f,2f,true));
            rObjects.Add(items.Shader("TESx1"), "O-TES1",
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Patches,
                                    new GLVertexVector4(GLShapeObjectFactory.CreateQuad2(10.0f, 10.0f)),
                                    new GLObjectDataTranslationRotationTexture(items.Tex("logo8bpp"), new Vector3(12, 0, 0), new Vector3(-90,0,0))
                                    ));

            #endregion
            
            #region coloured lines

            rObjects.Add(items.Shader("COS-1L"),
                         new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                               new GLVertexColourObject(  GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(-100, 0, 100), new Vector3(10, 0, 0), 21),
                                                            new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                               ));


            rObjects.Add(items.Shader("COS-1L"),
                         new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                               new GLVertexColourObject( GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(100, 0, -100), new Vector3(0, 0, 10), 21),
                                                         new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                               ));
            rObjects.Add(items.Shader("COS-1L"),
                         new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                               new GLVertexColourObject( GLShapeObjectFactory.CreateLines(new Vector3(-100, 10, -100), new Vector3(-100, 10, 100), new Vector3(10, 0, 0), 21),
                                                         new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                               ));
            rObjects.Add(items.Shader("COS-1L"),
                         new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                               new GLVertexColourObject( GLShapeObjectFactory.CreateLines(new Vector3(-100, 10, -100), new Vector3(100, 10, -100), new Vector3(0, 0, 10), 21),
                                                         new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
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
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                    new GLVertexColourObject(GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange }),
                    new GLObjectDataTranslationRotation(new Vector3(-12, 0, 0))
                    ));

            #endregion


            #region MipMaps

            items.Add("mipmap1", new GLTexture2D(Properties.Resources.mipmap, 9));

            rObjects.Add(items.Shader("TEX"), "mipmap1",
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                    new GLVertexCoordsObject(GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles()),
                        new GLObjectDataTranslationRotationTexture(items.Tex("mipmap1"), new Vector3(-10, 0, 0))
                        ));

            #endregion

            #region 2dArrays
            items.Add("TEX2DA", new GLTexturedObjectShader2DBlend());

            items.Add("2DArray2", new GLTexture2DArray(1,new Bitmap[] { Properties.Resources.mipmap, Properties.Resources.mipmap2 }, 9));

            rObjects.Add(items.Shader("TEX2DA"), "2DA",
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                    new GLVertexCoordsObject(GLShapeObjectFactory.CreateQuad(2.0f), GLShapeObjectFactory.TexQuad),
                        new GLObjectDataTranslationRotationTexture(items.Tex("2DArray2"), new Vector3(-8, 0, 0))
                        ));


            items.Add("2DArray2-1", new GLTexture2DArray(1,new Bitmap[] { Properties.Resources.dotted, Properties.Resources.dotted2 }));

            rObjects.Add(items.Shader("TEX2DA"), "2DA-1",
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                    new GLVertexCoordsObject(GLShapeObjectFactory.CreateQuad(2.0f), GLShapeObjectFactory.TexQuad),
                        new GLObjectDataTranslationRotationTexture(items.Tex("2DArray2-1"), new Vector3(-8, 0, -2))
                        ));

            #endregion

            #region Coloured triangles

            rObjects.Add(items.Shader("COST-FP"), "scopen",
                        new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                               new GLVertexColourObject(
                                       GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f, new GLCubeObjectFactory.Sides[] { GLCubeObjectFactory.Sides.Bottom, GLCubeObjectFactory.Sides.Top, GLCubeObjectFactory.Sides.Left, GLCubeObjectFactory.Sides.Right }),
                                        new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange }),
                            new GLObjectDataTranslationRotation(new Vector3(-6, 0, 0))
                        ));


            rObjects.Add(items.Shader("COST-LP"), "scopen-op",
                        new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                               new GLVertexColourObject(
                                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f, new GLCubeObjectFactory.Sides[] { GLCubeObjectFactory.Sides.Bottom, GLCubeObjectFactory.Sides.Top, GLCubeObjectFactory.Sides.Left, GLCubeObjectFactory.Sides.Right }), 
                                        new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange }),
                                new GLObjectDataTranslationRotation(new Vector3(-6, 0, -2))));

            rObjects.Add(items.Shader("COST-FP"), "sphere1",
                        new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                               new GLVertexColourObject(
                                    GLSphereObjectFactory.CreateSphereFromTriangles(3, 2.0f),
                                                new Color4[] { Color4.Red, Color4.Green, Color4.Blue, }),
                        new GLObjectDataTranslationRotation(new Vector3(-6, 0, -4))));

            #endregion

            #region coloured points

            rObjects.Add(items.Shader("COST-2P"), "pc",
                        new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                               new GLVertexColourObject( GLCubeObjectFactory.CreateVertexPointCube(1f) , new Color4[] { Color4.Yellow } ),
                         new GLObjectDataTranslationRotation(new Vector3(-4, 0, 0))
                         ));

            rObjects.Add(items.Shader("COST-2P"), "pc2",
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                               new GLVertexColourObject( GLCubeObjectFactory.CreateVertexPointCube(1f), new Color4[] { Color4.Green, Color4.White, Color4.Purple, Color4.Blue }),
                         new GLObjectDataTranslationRotation(new Vector3(-4, 0, -2))
                         ));

            rObjects.Add(items.Shader("COST-2P"), "cp",
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                               new GLVertexColourObject( GLCubeObjectFactory.CreateVertexPointCube(1f), new Color4[] { Color4.Red }),
                         new GLObjectDataTranslationRotation(new Vector3(-4, 0, -4))
                         ));

            rObjects.Add(items.Shader("COST-2P"), "dot2-1",
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                               new GLVertexColourObject( GLCubeObjectFactory.CreateVertexPointCube(1f), new Color4[] { Color.Red, Color.Green, Color.Blue, Color.Cyan, Color.Yellow, Color.Yellow, Color.Yellow, Color.Yellow }),
                         new GLObjectDataTranslationRotation(new Vector3(-4, 0, -6))
                         ));


            rObjects.Add(items.Shader("COST-2P"), "sphere2",
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                               new GLVertexColourObject( GLSphereObjectFactory.CreateSphereFromTriangles(3, 1.0f), new Color4[] { Color4.Red, Color4.Green, Color4.Blue, }),
                        new GLObjectDataTranslationRotation(new Vector3(-4, 0, -8))));

            rObjects.Add(items.Shader("COST-10P"), "sphere3",
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                               new GLVertexColourObject( GLSphereObjectFactory.CreateSphereFromTriangles(1, 1.0f), new Color4[] { Color4.Red, Color4.Green, Color4.Blue, }),
                        new GLObjectDataTranslationRotation(new Vector3(-4, 0, -10))));

            rObjects.Add(items.Shader("COST-2P"), "sphere4",
                        new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                               new GLVertexColourObject( GLSphereObjectFactory.CreateSphereFromTriangles(2, 1.0f), new Color4[] { Color4.Red, Color4.Green, Color4.Blue, }),
                            new GLObjectDataTranslationRotation(new Vector3(-4, 0, -12))));

            #endregion

            #region textures

            rObjects.Add(items.Shader("TEX"),
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 
                    new GLVertexCoordsObject( GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles()),
                            new GLObjectDataTranslationRotationTexture(items.Tex("dotted2"),new Vector3(-2, 0, 0))
                            ));

            rObjects.Add(items.Shader("TEX"), "EDDCube",
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 
                new GLVertexCoordsObject( GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles()),
                        new GLObjectDataTranslationRotationTexture(items.Tex("logo8bpp"),new Vector3(-2, 0, -2))
                        ));

            rObjects.Add(items.Shader("TEX"), "woodbox",
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 
                new GLVertexCoordsObject( GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles()),
                        new GLObjectDataTranslationRotationTexture(items.Tex("wooden"), new Vector3(-2, 0, -4))
                        ));

            rObjects.Add(items.Shader("TEX"),
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Quads, new GLVertexCoordsObject(
                        GLShapeObjectFactory.CreateQuad(1.0f, 1.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad),
                        new GLObjectDataTranslationRotationTexture(items.Tex("dotted2"),new Vector3(-2, 0, -6))
                        ));

            rObjects.Add(items.Shader("TEX"),
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Quads, 
                    new GLVertexCoordsObject( GLShapeObjectFactory.CreateQuad(1.0f, 1.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad),
                        new GLObjectDataTranslationRotationTexture(items.Tex("dotted2"),new Vector3(-2, 0, -8))
                        ));

            rObjects.Add(items.Shader("TEX"),
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Quads, 
                new GLVertexCoordsObject( GLShapeObjectFactory.CreateQuad(1.0f, 1.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad),
                        new GLObjectDataTranslationRotationTexture(items.Tex("dotted"),new Vector3(-2, 0, -10))
                        ));

            rObjects.Add(items.Shader("TEX"), "EDDFlat",
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Quads, 
                new GLVertexCoordsObject( GLShapeObjectFactory.CreateQuad(2.0f, items.Tex("logo8bpp").Width, items.Tex("logo8bpp").Height, new Vector3(-0, 0, 0)),  GLShapeObjectFactory.TexQuad),
                        new GLObjectDataTranslationRotationTexture(items.Tex("logo8bpp"), new Vector3(0, 0, 0))
                        ));

            rObjects.Add(items.Shader("TEX"),
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Quads, 
                new GLVertexCoordsObject( GLShapeObjectFactory.CreateQuad(1.5f, new Vector3(-90, 0, 0)), GLShapeObjectFactory.TexQuad),
                        new GLObjectDataTranslationRotationTexture(items.Tex("smile"),new Vector3(0, 0, -2))
                       ));

            rObjects.Add(items.Shader("CROT"), "woodboxc1",
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 
                    new GLVertexCoordsObject( GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles()),
                        new GLObjectDataTranslationRotationTexture(items.Tex("wooden"),new Vector3(0, 0, -4))
                        ));

            rObjects.Add(items.Shader("CROT"), "woodboxc2",
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 
                    new GLVertexCoordsObject( GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles()),
                        new GLObjectDataTranslationRotationTexture(items.Tex("wooden"),new Vector3(0, 0, -6))
                       ));

            rObjects.Add(items.Shader("CROT"), "woodboxc3",
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 
                    new GLVertexCoordsObject(  GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles()),
                        new GLObjectDataTranslationRotationTexture(items.Tex("wooden"),new Vector3(0, 0, -8))
                        ));

            rObjects.Add(items.Shader("TEX"), "sphere5",
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 
                    new GLVertexCoordsObject( GLSphereObjectFactory.CreateTexturedSphereFromTriangles(3, 1.0f)),
                        new GLObjectDataTranslationRotationTexture(items.Tex("wooden"), new Vector3(0, 0, -10))
                        ));

            rObjects.Add(items.Shader("CROT"), "sphere6",
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 
                    new GLVertexCoordsObject( GLSphereObjectFactory.CreateTexturedSphereFromTriangles(3, 1.5f)),
                        new GLObjectDataTranslationRotationTexture(items.Tex("golden"), new Vector3(0, 0, -12))
                        ));

            #endregion

            #region Sphere mapping 
            rObjects.Add(items.Shader("TEX"), "sphere7",
                new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 
                        new GLVertexCoordsObject( GLSphereObjectFactory.CreateTexturedSphereFromTriangles(4, 4.0f)),
                        new GLObjectDataTranslationRotationTexture(items.Tex("moon"),new Vector3(4, 0, 0))
                        ));

            #endregion

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

            ((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).XRotDegrees = degrees;
            ((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).ZRotDegrees = degrees;

            //((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).Translate(new Vector3(0.01f, 0.01f, 0));
            ((GLObjectDataTranslationRotation)(rObjects["EDDCube"].InstanceData)).YRotDegrees = degrees;
            ((GLObjectDataTranslationRotation)(rObjects["EDDCube"].InstanceData)).ZRotDegrees = degreesd2;

            ((GLObjectDataTranslationRotation)(rObjects["sphere3"].InstanceData)).XRotDegrees = -degrees;
            ((GLObjectDataTranslationRotation)(rObjects["sphere3"].InstanceData)).YRotDegrees = degrees;
            ((GLObjectDataTranslationRotation)(rObjects["sphere4"].InstanceData)).YRotDegrees = degrees;
            ((GLObjectDataTranslationRotation)(rObjects["sphere4"].InstanceData)).ZRotDegrees = -degreesd2;
            ((GLObjectDataTranslationRotation)(rObjects["sphere7"].InstanceData)).YRotDegrees = degreesd4;

            ((GLVertexShaderTextureTransformWithCommonTransform)items.Shader("CROT").Get(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader)).Transform.YRotDegrees = degrees;
            ((GLFragmentShader2DCommonBlend)items.Shader("TEX2DA").Get(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader)).Blend = zeroone;

           items.SB("SB6").Write(zeroone, 4, true);

            ((GLObjectDataTranslationRotation)(rObjects["woodbox"].InstanceData)).Position = gl3dcontroller.Pos.Current;

            ((GLTesselationShaderSinewave)items.Shader("TESx1")).Phase = degrees/360.0f;

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

        }

    }



}


