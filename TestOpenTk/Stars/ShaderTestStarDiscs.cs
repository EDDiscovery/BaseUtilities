/*
 * Copyright 2019 Robbyxp1 @ github.com
 * Part of the EDDiscovery Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */

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
        private OpenTKUtils.WinForm.GLWinFormControl glwfc;
        private Controller3D gl3dcontroller;

        private Timer systemtimer = new Timer();

        public ShaderTestStarDiscs()
        {
            InitializeComponent();

            glwfc = new OpenTKUtils.WinForm.GLWinFormControl(glControlContainer);

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
            systemtimer.Start();
        }

        GLRenderProgramSortedList rObjects = new GLRenderProgramSortedList();
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

layout (location = 10) uniform float frequency;
layout (location = 11) uniform float unRadius;      // km
layout (location = 12) uniform float s;
layout (location = 13) uniform float blackdeepness;
layout (location = 14) uniform float concentrationequator;
layout (location = 15) uniform float unDTsurface;
layout (location = 16) uniform float unDTspots;

#include OpenTKUtils.GL4.Shaders.Functions.snoise3.glsl

void main(void)
{
    vec3 position = normalize(modelpos);        // normalise model vectors

    float theta = dot(vec3(0,1,0),position);    // dotp between cur pos and up -1 to +1, 0 at equator
    theta = abs(theta);                         // uniform around equator.

    float clip = s + (theta/concentrationequator);               // clip sets the pass criteria to do the sunspots
    vec3 sPosition = (position + unDTspots) * unRadius;
    float t1 = simplexnoise(sPosition * frequency) -clip;
    float t2 = simplexnoise((sPosition + unRadius) * frequency) -clip;
	float ss = (max(t1, 0.0) * max(t2, 0.0)) * blackdeepness;

    vec3 p1 = vec3(position.x+unDTsurface,position.y,position.z);   // moving the noise across x produces a more realistic look
    float n = (simplexnoise(p1, 4, 40.0, 0.7) + 1.0) * 0.5;      // noise of surface..

    vec3 baseColor = vec3(0.9, 0.9 ,0.0);
    baseColor = baseColor - ss - n/4;
    color = vec4(baseColor, 1.0);
}
";
            }

            public float TimeDeltaSurface { get; set; } = 0;
            public float TimeDeltaSpots { get; set; } = 0;

            public GLFragmentShaderStarTexture()
            {
                CompileLink(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, Fragment(), GetType().Name);
            }

            public override void Start()
            {
                GL.ProgramUniform1(Id, 15, TimeDeltaSurface);
                GL.ProgramUniform1(Id, 16, TimeDeltaSpots);
                OpenTKUtils.GLStatics.Check();
                System.Diagnostics.Debug.WriteLine("Star draw");

            }
        }



        public class GLShaderStarCorona : GLShaderStandard
        {
            const int BindingPoint = 1;

            public string Vertex()
            {
                return
    @"
#version 450 core
#include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl

layout (location = 0) in vec4 position;

layout (location = 21) uniform  mat4 rotate;
layout (location = 22) uniform  mat4 transform;

layout (location =0) out vec3 fposition;

void main(void)
{
    fposition =vec3(position.xz,0);
    vec4 p1 = rotate * position;
	gl_Position = mc.ProjectionModelMatrix * transform * p1;        // order important
}
";
            }

            public string Fragment()
            {
                return
    @"
#version 450 core

#include OpenTKUtils.GL4.Shaders.Functions.snoise4.glsl

layout (location =0 ) in vec3 fposition;
out vec4 color;

layout (location = 15) uniform float unDT;

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

	/* Don't edit these */

    float t = unDT - length(fposition);

    // Offset normal with noise
    float ox = simplexnoise(vec4(fposition, t) * frequency);
    float oy = simplexnoise(vec4((fposition + (1000.0 * irregularityMultiplier)), t) * frequency);
    float oz = simplexnoise(vec4((fposition + (2000.0 * irregularityMultiplier)), t) * frequency);
	float om = simplexnoise(vec4((fposition + (4000.0 * irregularityMultiplier)), t) * frequency) * simplexnoise(vec4((fposition + (250.0 * irregularityMultiplier)), t) * frequency);
    vec3 offsetVec = vec3(ox * om, oy * om, oz * om) * smootheningMultiplier;

    // Get the distance vector from the center
    vec3 nDistVec = normalize(fposition + offsetVec);

    // Get noise with normalized position to offset the original position
    vec3 position = fposition + simplexnoise(vec4(nDistVec, t), iDetail, 1.5, fDetail) * smootheningMultiplier;

    // Calculate brightness based on distance
    float dist = length(position + offsetVec) * coronaSizeMultiplier;
    float brightness = (1.0 / (dist * dist) - 0.1) * (brightnessMultiplier - 0.4);
	float brightness2 = (1.0 / (dist * dist)) * brightnessMultiplier;

    // Calculate color
    vec3 unColor = vec3(0.9,0.9,0);

    float alpha = clamp(brightness, 0.0, 1.0) * (cos(clamp(brightness, 0.0, 0.5)) / (cos(clamp(brightness2 / ringIntesityMultiplier, 0.0, 1.5)) * 2));
    vec3 starcolor = unColor * brightness;

    alpha = pow(alpha,1.8);             // exp roll of of alpha so it does go to 0, and therefore it does not show box
    color = vec4(starcolor, alpha );
}
";
            }

            public GLShaderStarCorona()
            {
                CompileLink(vertex: Vertex(), frag: Fragment());
            }

            public float TimeDelta{ get; set; } = 0.00001f*10;

            public override void Start()
            {
                base.Start();

                GL.ProgramUniform1(Id, 15, TimeDelta);
                OpenTKUtils.GLStatics.Check();
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

            gl3dcontroller = new Controller3D();
            gl3dcontroller.PaintObjects = ControllerDraw;
            gl3dcontroller.ZoomDistance = 20F;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            glwfc.BackColour = Color.FromArgb(0, 0, 60);
            gl3dcontroller.Start(glwfc,new Vector3(0, 0, 0), new Vector3(120f, 0, 0f), 1F);

            gl3dcontroller.KeyboardTravelSpeed = (ms,eyedist) =>
            {
                return (float)ms / 20.0f;
            };

            items.Add("MCUB", new GLMatrixCalcUniformBlock());     // def binding of 0

            {
                items.Add("COS", new GLColourShaderWithWorldCoord());
                GLRenderControl rl = GLRenderControl.Lines(1);

                rObjects.Add(items.Shader("COS"),
                             GLRenderableItem.CreateVector4Color4(items, rl,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(-40, 0, -40), new Vector3(-40, 0, 40), new Vector3(10, 0, 0), 9),
                                                        new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                                   );


                rObjects.Add(items.Shader("COS"),
                             GLRenderableItem.CreateVector4Color4(items, rl,
                                   GLShapeObjectFactory.CreateLines(new Vector3(-40, 0, -40), new Vector3(40, 0, -40), new Vector3(0, 0, 10), 9),
                                                             new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                                   );
            }

            {
                items.Add("TEX", new GLTexturedShaderWithObjectTranslation());
                items.Add("moon", new GLTexture2D(Properties.Resources.moonmap1k));

                GLRenderControl rt = GLRenderControl.Tri();

                rObjects.Add(items.Shader("TEX"), "sphere7",
                        GLRenderableItem.CreateVector4Vector2(items, rt,
                            GLSphereObjectFactory.CreateTexturedSphereFromTriangles(4, 20.0f),
                            new GLRenderDataTranslationRotationTexture(items.Tex("moon"), new Vector3(30, 0, 0))
                        ));
            }

            {
                items.Add("STAR", new GLShaderPipeline(new GLPLVertexShaderModelCoordWithObjectTranslation(), new GLFragmentShaderStarTexture()));

                GLRenderControl rt = GLRenderControl.Tri();

                rObjects.Add(items.Shader("STAR"), "sun",
                       GLRenderableItem.CreateVector4(items,
                               rt,
                               GLSphereObjectFactory.CreateSphereFromTriangles(3, 20.0f),
                               new GLRenderDataTranslationRotation(new Vector3(1, 1, 1)),
                               ic: 1));

                items.Add("CORONA", new GLShaderStarCorona());

                GLRenderControl rq = GLRenderControl.Quads();

                rObjects.Add(items.Shader("CORONA"), GLRenderableItem.CreateVector4(items,
                                        rq,
                                        GLShapeObjectFactory.CreateQuad(1f),
                                        new GLRenderDataTranslationRotation(new Vector3(1, 1, 1), new Vector3(0, 0, 0), 40f, calclookat: true)));
            }



            //items.Add("BCK", new GLShaderPipeline(new GLVertexShaderObjectTransform(), new GLFragmentShaderFixedColour(new Color4(0.5f,0,0,0.5f))));

            //rObjects.Add(items.Shader("BCK"), GLRenderableItem.CreateVector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
            //                            GLShapeObjectFactory.CreateQuad(20.0f),
            //                            new GLObjectDataTranslationRotation(new Vector3(0, 0, 0.1f))

            //                            ));

            OpenTKUtils.GLStatics.Check();

            GL.Enable(EnableCap.DepthClamp);
        }


        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(GLMatrixCalc mc, long time)
        {
            // System.Diagnostics.Debug.WriteLine("Draw eye " + gl3dcontroller.MatrixCalc.EyePosition + " to " + gl3dcontroller.Pos.Current);

            float zeroone10000s = ((float)(time % 10000000)) / 10000000.0f;
            float zeroone5000s = ((float)(time % 5000000)) / 5000000.0f;
            float zeroone1000s = ((float)(time % 1000000)) / 1000000.0f;
            float zeroone500s = ((float)(time % 500000)) / 500000.0f;
            float zeroone100s = ((float)(time % 100000)) / 100000.0f;
            float zeroone10s = ((float)(time % 10000)) / 10000.0f;
            float zeroone5s = ((float)(time % 5000)) / 5000.0f;
            float zerotwo5s = ((float)(time % 5000)) / 2500.0f;
            float timediv10s = (float)time / 10000.0f;
            float timediv100s = (float)time / 100000.0f;


            if (items.Contains("STAR"))
            {
                int vid = items.Shader("STAR").Get(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader).Id;

                GL.ProgramUniform1(vid, 10, frequency);
                GL.ProgramUniform1(vid, 11, unRadius);
                GL.ProgramUniform1(vid, 12, scutoff);
                GL.ProgramUniform1(vid, 13, blackdeepness);
                GL.ProgramUniform1(vid, 14, concentrationequator);

                ((GLRenderDataTranslationRotation)(rObjects["sun"].RenderData)).Rotation = new Vector3(0, -zeroone100s * 360, 0);

                var stellarsurfaceshader = (GLFragmentShaderStarTexture)items.Shader("STAR").Get(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader);
                stellarsurfaceshader.TimeDeltaSpots = zeroone500s;
                stellarsurfaceshader.TimeDeltaSurface = timediv100s;
            }

            if (items.Contains("CORONA"))
            {
                ((GLShaderStarCorona)items.Shader("CORONA")).TimeDelta = (float)time / 100000f;
            }

            GLMatrixCalcUniformBlock mcub = (GLMatrixCalcUniformBlock)items.UB("MCUB");
            mcub.Set(gl3dcontroller.MatrixCalc);

            rObjects.Render(glwfc.RenderState,gl3dcontroller.MatrixCalc);
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);


            //this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " dir " + gl3dcontroller.Camera.Current + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance;
            this.Text = "Freq " + frequency.ToString("#.#########") + " unRadius " + unRadius + " scutoff" + scutoff + " BD " + blackdeepness + " CE " + concentrationequator
            + "    Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " dir " + gl3dcontroller.Camera.Current + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance;

        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboardSlews(true, OtherKeys);
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


