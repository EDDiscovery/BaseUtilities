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
    public partial class ShaderTestStarDiscs : Form
    {
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        public ShaderTestStarDiscs()
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
        GLRenderProgramSortedList rObjects2 = new GLRenderProgramSortedList();
        GLItemsList items = new GLItemsList();

        // Demonstrate buffer feedback AND geo shader add vertex/dump vertex


        public class GLFragmentShaderStarTexture : GLShaderPipelineShadersBase
        {
            const int BindingPoint = 1;

            public string Fragment()
            {
                return
    @"
#version 450 core
layout (location = 1) in vec3 modelpos;
out vec4 color;

layout (binding = 1, std430) buffer Positions
{
    int count;
    float noisebuf[];
};

layout (location = 10) uniform float frequency;
layout (location = 11) uniform float unRadius;      // km
layout (location = 12) uniform float s;
layout (location = 13) uniform float blackdeepness;
layout (location = 14) uniform float concentrationequator;


" + GLShaderFunctionsNoise.NoiseFunctions3 +
    @"
void main(void)
{
    vec3 position = normalize(modelpos);        // normalise model vectors

    float theta = dot(vec3(0,1,0),position);    // dotp between cur pos and up -1 to +1, 0 at equator
    theta = abs(theta);                         // uniform around equator.

    float clip = s + (theta/concentrationequator);               // 

    vec3 sPosition = position * unRadius;
    float t1 = snoise(sPosition * frequency) -clip;
    float t2 = snoise((sPosition + unRadius) * frequency) -clip;
	float ss = (max(t1, 0.0) * max(t2, 0.0)) * blackdeepness;

    float n = (noise(position, 4, 40.0, 0.7) + 1.0) * 0.5;

    vec3 baseColor = vec3(0.9, 0.9 ,0.0);
    baseColor = baseColor - ss - n/4;
    color = vec4(baseColor, 1.0);

    if ( ss != 0 )
    {
        uint ipos = atomicAdd(count,1);
        if ( ipos < 1024 )
            noisebuf[ipos] = clip;
    }

}
";
            }

            public GLFragmentShaderStarTexture()
            {
                Program = GLProgram.CompileLink(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, Fragment(), GetType().Name);
            }

            public override void Start(MatrixCalc c)
            {
                GL4Statics.PolygonMode(OpenTK.Graphics.OpenGL4.MaterialFace.FrontAndBack, OpenTK.Graphics.OpenGL4.PolygonMode.Fill);        // need fill for fragment to work
                GLStatics.Check();
                System.Diagnostics.Debug.WriteLine("Star draw");

            }
        }



        public class GLShaderStarCorona : GLShaderProgramBase
        {
            const int BindingPoint = 1;

            public string Vertex()
            {
                return
    @"
#version 450 core

layout (location = 0) in vec4 position;

layout (location = 20) uniform  mat4 projectionmodel;
layout (location = 10) uniform  mat4 inveye;
layout (location = 22) uniform  mat4 transform;

layout (location =0) out vec3 fposition;

void main(void)
{
    fposition =vec3(position.xz,0);
    vec4 p1 = inveye * position;
	gl_Position = projectionmodel * transform * p1;        // order important
}
";
            }

            public string Fragment()
            {
                return
    @"
#version 450 core
" + GLShaderFunctionsNoise.NoiseFunctions4 + @"

layout (location =0 ) in vec3 fposition;
out vec4 color;

void main(void)
{
	const float brightnessMultiplier = 0.9;   // The higher the number, the brighter the corona will be.
	const float smootheningMultiplier = 0.15; // How smooth the irregular effect is, the higher the smoother.
	const float ringIntesityMultiplier = 2.8; // The higher the number, the smaller the solid ring inside
	const float coronaSizeMultiplier = 2.0;  // The higher the number, the smaller the corona. 2.0
	const float frequency = 1.5;              // The frequency of the irregularities.
	const float fDetail = 0.7;                // The higher the number, the more detail the corona will have. (Might be more GPU intensive when higher, 0.7 seems fine for the normal PC)
	const int iDetail = 10;                   // The higher the number, the more detail the corona will have.
	const float irregularityMultiplier = 4;   // The higher the number, the more irregularities and bigger ones. (Might be more GPU intensive when higher, 4 seems fine for the normal PC)
    float unDT = 0.0001;                      // time constant..

	/* Don't edit these */

    float t = unDT * 10.0 - length(fposition);

    // Offset normal with noise
    float ox = snoise(vec4(fposition, t) * frequency);
    float oy = snoise(vec4((fposition + (1000.0 * irregularityMultiplier)), t) * frequency);
    float oz = snoise(vec4((fposition + (2000.0 * irregularityMultiplier)), t) * frequency);
	float om = snoise(vec4((fposition + (4000.0 * irregularityMultiplier)), t) * frequency) * snoise(vec4((fposition + (250.0 * irregularityMultiplier)), t) * frequency);
    vec3 offsetVec = vec3(ox * om, oy * om, oz * om) * smootheningMultiplier;

    // Get the distance vector from the center
    vec3 nDistVec = normalize(fposition + offsetVec);

    // Get noise with normalized position to offset the original position
    vec3 position = fposition + noise(vec4(nDistVec, t), iDetail, 1.5, fDetail) * smootheningMultiplier;

    // Calculate brightness based on distance
    float dist = length(position + offsetVec) * coronaSizeMultiplier;
    float brightness = (1.0 / (dist * dist) - 0.1) * (brightnessMultiplier - 0.4);
	float brightness2 = (1.0 / (dist * dist)) * brightnessMultiplier;

    // Calculate color
    vec3 unColor = vec3(0.9,0.9,0);

    vec3 starcolor = unColor * brightness;
    float alpha = clamp(brightness, 0.0, 1.0) * (cos(clamp(brightness, 0.0, 0.5)) / (cos(clamp(brightness2 / ringIntesityMultiplier, 0.0, 1.5)) * 2));
    color = vec4(starcolor, alpha );

//if ( color.length < 1 )
  //  color = vec4(fposition.x/20+0.5,fposition.y/20+0.5,0,1.0);

}
";
            }

            public GLShaderStarCorona()
            {
                Compile(vertex: Vertex(), frag: Fragment());
            }

            public override void Start(MatrixCalc c)
            {
                base.Start();

                OpenTK.Matrix4 projmodel = c.ProjectionModelMatrix;
                GL.ProgramUniformMatrix4(Id, 20, false, ref projmodel);
                OpenTK.Matrix4 inveye = c.InvEyeRotate;
                GL.ProgramUniformMatrix4(Id, 10, false, ref inveye);
                //System.Diagnostics.Debug.WriteLine("DIR " + inveye);
                GL4Statics.PolygonMode(OpenTK.Graphics.OpenGL4.MaterialFace.FrontAndBack, OpenTK.Graphics.OpenGL4.PolygonMode.Fill);        // need fill for fragment to work
                GLStatics.Check();
                System.Diagnostics.Debug.WriteLine("Corona draw");
            }
        }


        // 0.001/200000/0.5 with *4 is good
        float frequency = 0.00005f;     // higher, more but small
        float unRadius = 200000;        // lower, more diffused
        float scutoff = 0.5f;           // bar to pass, lower more, higher lots 0.4 lots, 0.6 few
        float blackdeepness = 8;        // how dark is each spot
        float concentrationequator = 4; // how spread out


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Closed += ShaderTest_Closed;

            gl3dcontroller.MatrixCalc.ZoomDistance = 20F;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.BackColour = Color.FromArgb(0, 0, 60);
            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(120f, 0, 0f), 1F);

            gl3dcontroller.TravelSpeed = (ms) =>
            {
                return (float)ms / 20.0f;
            };

            items.Add("COS-1L", new GLColourObjectShaderNoTranslation((a) => { GLStatics.LineWidth(1); }));
            items.Add("TEX", new GLTexturedObjectShaderSimple());

            rObjects.Add(items.Shader("COS-1L"),
                         GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                                                    GLShapeObjectFactory.CreateLines(new Vector3(-40, 0, -40), new Vector3(-40, 0, 40), new Vector3(10, 0, 0), 9),
                                                    new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                               );


            rObjects.Add(items.Shader("COS-1L"),
                         GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                               GLShapeObjectFactory.CreateLines(new Vector3(-40, 0, -40), new Vector3(40, 0, -40), new Vector3(0, 0, 10), 9),
                                                         new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                               );


            items.Add("moon", new GLTexture2D(Properties.Resources.moonmap1k));

            rObjects.Add(items.Shader("TEX"), "sphere7",
                GLRenderableItem.CreateVector4Vector2(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                        GLSphereObjectFactory.CreateTexturedSphereFromTriangles(4, 20.0f),
                        new GLObjectDataTranslationRotationTexture(items.Tex("moon"), new Vector3(30, 0, 0))
                        ));


            items.Add("STAR", new GLShaderPipeline(new GLVertexShaderObjectTransform(),
                        new GLFragmentShaderStarTexture()));

            rObjects.Add(items.Shader("STAR"),
                       GLRenderableItem.CreateVector4(items,
                               OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                               GLSphereObjectFactory.CreateSphereFromTriangles(3, 20.0f),
                               new GLObjectDataTranslationRotation(new Vector3(0, 0, 0)),
                               ic: 1));

            items.Add("BCK", new GLShaderPipeline(new GLVertexShaderObjectTransform(), new GLFragmentShaderFixedColour(new Color4(0.5f,0,0,0.5f))));

            //rObjects.Add(items.Shader("BCK"), GLRenderableItem.CreateVector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
            //                            GLShapeObjectFactory.CreateQuad(20.0f),
            //                            new GLObjectDataTranslationRotation(new Vector3(0, 0, 0.1f))

            //                            ));

            items.Add("CORONA", new GLShaderStarCorona());

            rObjects.Add(items.Shader("CORONA"), GLRenderableItem.CreateVector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                                        GLShapeObjectFactory.CreateQuad(1f),
                                        new GLObjectDataTranslationRotation(new Vector3(0, 0, 0), new Vector3(0,0,0), 35f)));



            vecoutbuffer = new GLStorageBlock(1);           // new storage block on binding index
            vecoutbuffer.Allocate(sizeof(float) * 2048, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicCopy);       // set size of vec buffer

            OpenTKUtils.GLStatics.Check();
            //GL.Enable(EnableCap.DepthClamp);
        }

        GLStorageBlock vecoutbuffer;

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(Matrix4 model, Matrix4 projection, long time)
        {
            // System.Diagnostics.Debug.WriteLine("Draw eye " + gl3dcontroller.MatrixCalc.EyePosition + " to " + gl3dcontroller.Pos.Current);

            float zeroone10s = ((float)(time % 10000)) / 10000.0f;
            float zeroone5s = ((float)(time % 5000)) / 5000.0f;
            float zerotwo5s = ((float)(time % 5000)) / 2500.0f;
            float degrees = zeroone10s * 360;

            vecoutbuffer.ZeroBuffer();

            if (items.Contains("STAR"))
            {
                int vid = items.Shader("STAR").Get(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader).Id;

                GL.ProgramUniform1(vid, 10, frequency);
                GL.ProgramUniform1(vid, 11, unRadius);
                GL.ProgramUniform1(vid, 12, scutoff);
                GL.ProgramUniform1(vid, 13, blackdeepness);
                GL.ProgramUniform1(vid, 14, concentrationequator);
            }


            rObjects.Render(gl3dcontroller.MatrixCalc);
            rObjects2.Render(gl3dcontroller.MatrixCalc);
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

            int count = vecoutbuffer.ReadInt(0);
            if (count > 0)
            {
                float[] values = vecoutbuffer.ReadFloats(4, Math.Min(2000, count));
                System.Diagnostics.Debug.WriteLine("Count " + count + " min " + values.Min() + " max " + values.Max());
            }

            //this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " dir " + gl3dcontroller.Camera.Current + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance;
            this.Text = "Freq " + frequency.ToString("#.#########") + " unRadius " + unRadius + " scutoff" + scutoff + " BD " + blackdeepness + " CE " + concentrationequator
            + "    Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " dir " + gl3dcontroller.Camera.Current + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance;

        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboard(true, OtherKeys);
            gl3dcontroller.Redraw();
        }

        private void OtherKeys( BaseUtils.KeyboardState kb )
        {
            float fact = kb.Shift ? 10 : kb.Alt ? 100 : 1;
            if (kb.IsPressed(Keys.F1) != null)
                frequency -= 0.000001f * fact;
            if (kb.IsPressed(Keys.F2) != null)
                frequency += 0.000001f * fact;
            if (kb.IsPressed(Keys.F5) != null)
                unRadius -= 10 * fact;
            if (kb.IsPressed(Keys.F6) != null)
                unRadius += 10 * fact;
            if (kb.IsPressed(Keys.F7) != null)
                scutoff -= 0.001f * fact;
            if (kb.IsPressed(Keys.F8) != null)
                scutoff += 0.001f * fact;
            if (kb.IsPressed(Keys.F9) != null)
                blackdeepness -= 0.1f * fact;
            if (kb.IsPressed(Keys.F10) != null)
                blackdeepness += 0.1f * fact;
            if (kb.IsPressed(Keys.F11) != null)
                concentrationequator -= 0.1f * fact;
            if (kb.IsPressed(Keys.F12) != null)
                concentrationequator += 0.1f * fact;
        }
    }
}


