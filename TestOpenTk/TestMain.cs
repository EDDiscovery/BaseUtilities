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
using OpenTKUtils;
using OpenTKUtils.Common;
using OpenTKUtils.GL4;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TestOpenTk
{
    public partial class TestMain : Form
    {
        private OpenTKUtils.WinForm.GLWinFormControl glwfc;
        private Controller3D gl3dcontroller;

        private Timer systemtimer = new Timer();

        GLRenderProgramSortedList rObjects = new GLRenderProgramSortedList();
        GLRenderProgramSortedList rObjectscw = new GLRenderProgramSortedList();
        GLItemsList items = new GLItemsList();
        GLStorageBlock dataoutbuffer;

        public TestMain()
        {
            InitializeComponent();

            glwfc = new OpenTKUtils.WinForm.GLWinFormControl(glControlContainer);

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
            systemtimer.Start();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Closed += ShaderTest_Closed;

            gl3dcontroller = new Controller3D();
            gl3dcontroller.PaintObjects = ControllerDraw;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 1f;
            gl3dcontroller.MatrixCalc.PerspectiveFarZDistance= 1000f;
            gl3dcontroller.ZoomDistance = 20F;
            gl3dcontroller.Start(glwfc, new Vector3(0, 0, 0), new Vector3(110f, 0, 0f), 1F);

            gl3dcontroller.KeyboardTravelSpeed = (ms,eyedist) =>
            {
                return (float)ms / 100.0f;
            };

            items.Add( new GLTexturedShaderWithObjectTranslation(),"TEXOT");
            items.Add(new GLTexturedShaderWithObjectTranslation(), "TEXOTNoRot");
            items.Add(new GLColourShaderWithWorldCoord(), "COSW");
            items.Add(new GLColourShaderWithObjectTranslation(), "COSOT");
            items.Add(new GLFixedColourShaderWithObjectTranslation(Color.Goldenrod), "FCOSOT");
            items.Add(new GLTexturedShaderWithObjectCommonTranslation(), "TEXOCT");

            items.Add( new GLTexture2D(Properties.Resources.dotted)  ,           "dotted"    );
            items.Add(new GLTexture2D(Properties.Resources.Logo8bpp), "logo8bpp");
            items.Add(new GLTexture2D(Properties.Resources.dotted2), "dotted2");
            items.Add(new GLTexture2D(Properties.Resources.wooden), "wooden");
            items.Add(new GLTexture2D(Properties.Resources.shoppinglist), "shoppinglist");
            items.Add(new GLTexture2D(Properties.Resources.golden), "golden");
            items.Add(new GLTexture2D(Properties.Resources.smile5300_256x256x8), "smile");
            items.Add(new GLTexture2D(Properties.Resources.moonmap1k), "moon");    

            #region Sphere mapping 

            {
                GLRenderControl rc1 = GLRenderControl.Tri();
                rObjects.Add(items.Shader("TEXOT"), "sphere7",
                    GLRenderableItem.CreateVector4Vector2(items, rc1,
                            GLSphereObjectFactory.CreateTexturedSphereFromTriangles(4, 4.0f),
                            new GLRenderDataTranslationRotationTexture(items.Tex("moon"), new Vector3(4, 0, 0))
                            ));

            }



            #endregion

            #region coloured lines

            if (true)
            {
                GLRenderControl lines = GLRenderControl.Lines(1);

                rObjects.Add(items.Shader("COSW"),
                             GLRenderableItem.CreateVector4Color4(items, lines,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(-100, 0, 100), new Vector3(10, 0, 0), 21),
                                                        new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                                   );


                rObjects.Add(items.Shader("COSW"),
                             GLRenderableItem.CreateVector4Color4(items, lines,
                                   GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(100, 0, -100), new Vector3(0, 0, 10), 21),
                                                             new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green }));
            }
            if ( true )
            {
                GLRenderControl lines = GLRenderControl.Lines(1);

                rObjects.Add(items.Shader("COSW"),
                             GLRenderableItem.CreateVector4Color4(items, lines,
                                   GLShapeObjectFactory.CreateLines(new Vector3(-100, 10, -100), new Vector3(-100, 10, 100), new Vector3(10, 0, 0), 21),
                                                             new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                                   );
                rObjects.Add(items.Shader("COSW"),
                             GLRenderableItem.CreateVector4Color4(items, lines,
                                   GLShapeObjectFactory.CreateLines(new Vector3(-100, 10, -100), new Vector3(100, 10, -100), new Vector3(0, 0, 10), 21),
                                                             new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                                   );
            }

            #endregion

            #region Coloured triangles
            if (true)
            {
                GLRenderControl rc = GLRenderControl.Tri();
                rc.CullFace = false;

                rObjects.Add(items.Shader("COSOT"), "scopen",
                            GLRenderableItem.CreateVector4Color4(items, rc,
                                            GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f, new GLCubeObjectFactory.Sides[] { GLCubeObjectFactory.Sides.Bottom, GLCubeObjectFactory.Sides.Top, GLCubeObjectFactory.Sides.Left, GLCubeObjectFactory.Sides.Right }),
                                            new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange },
                                            new GLRenderDataTranslationRotation(new Vector3(-6, 0, 0))
                            ));


                rObjects.Add(items.Shader("COSOT"), "scopen-op",
                            GLRenderableItem.CreateVector4Color4(items, rc,
                                            GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f, new GLCubeObjectFactory.Sides[] { GLCubeObjectFactory.Sides.Bottom, GLCubeObjectFactory.Sides.Top, GLCubeObjectFactory.Sides.Left, GLCubeObjectFactory.Sides.Right }),
                                            new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange },
                                            new GLRenderDataTranslationRotation(new Vector3(-6, 0, -2))
                            ));

                rObjects.Add(items.Shader("COSOT"), "sphere1",
                            GLRenderableItem.CreateVector4Color4(items, rc,
                                        GLSphereObjectFactory.CreateSphereFromTriangles(3, 2.0f),
                                        new Color4[] { Color4.Red, Color4.Green, Color4.Blue, },
                                        new GLRenderDataTranslationRotation(new Vector3(-6, 0, -4))
                            ));
            }

            #endregion


            #region view marker

            if (true)
            {
                GLRenderControl rc = GLRenderControl.Points(10);

                rObjects.Add(items.Shader("COSOT"), "viewpoint",
                        GLRenderableItem.CreateVector4Color4(items, rc,
                                       GLCubeObjectFactory.CreateVertexPointCube(1f), new Color4[] { Color.Purple },
                                 new GLRenderDataTranslationRotation(new Vector3(0,10,0))
                                 ));
            }

            #endregion


            #region coloured points
            if (true)
            {
                GLRenderControl rc2 = GLRenderControl.Points(2);

                rObjects.Add(items.Shader("COSOT"), "pc",
                            GLRenderableItem.CreateVector4Color4(items, rc2,
                                   GLCubeObjectFactory.CreateVertexPointCube(1f), new Color4[] { Color4.Yellow },
                             new GLRenderDataTranslationRotation(new Vector3(-4, 0, 0))
                             ));
                rObjects.Add(items.Shader("COSOT"), "pc2",
                    GLRenderableItem.CreateVector4Color4(items, rc2,
                                   GLCubeObjectFactory.CreateVertexPointCube(1f), new Color4[] { Color4.Green, Color4.White, Color4.Purple, Color4.Blue },
                             new GLRenderDataTranslationRotation(new Vector3(-4, 0, -2))
                             ));
                rObjects.Add(items.Shader("COSOT"), "cp",
                    GLRenderableItem.CreateVector4Color4(items, rc2,
                                   GLCubeObjectFactory.CreateVertexPointCube(1f), new Color4[] { Color4.Red },
                             new GLRenderDataTranslationRotation(new Vector3(-4, 0, -4))
                             ));
                rObjects.Add(items.Shader("COSOT"), "dot2-1",
                    GLRenderableItem.CreateVector4Color4(items, rc2,
                                   GLCubeObjectFactory.CreateVertexPointCube(1f), new Color4[] { Color.Red, Color.Green, Color.Blue, Color.Cyan, Color.Yellow, Color.Yellow, Color.Yellow, Color.Yellow },
                             new GLRenderDataTranslationRotation(new Vector3(-4, 0, -6))
                             ));
                rObjects.Add(items.Shader("COSOT"), "sphere2",
                    GLRenderableItem.CreateVector4Color4(items, rc2,
                                   GLSphereObjectFactory.CreateSphereFromTriangles(3, 1.0f), new Color4[] { Color4.Red, Color4.Green, Color4.Blue, },
                            new GLRenderDataTranslationRotation(new Vector3(-4, 0, -8))));

                rObjects.Add(items.Shader("COSOT"), "sphere4",
                            GLRenderableItem.CreateVector4Color4(items, rc2,
                                   GLSphereObjectFactory.CreateSphereFromTriangles(2, 1.0f), new Color4[] { Color4.Red, Color4.Green, Color4.Blue, },
                                new GLRenderDataTranslationRotation(new Vector3(-4, 0, -12))));

                GLRenderControl rc10 = GLRenderControl.Points(10);

                rObjects.Add(items.Shader("COSOT"), "sphere3",
                    GLRenderableItem.CreateVector4Color4(items, rc10,
                                   GLSphereObjectFactory.CreateSphereFromTriangles(1, 1.0f), new Color4[] { Color4.Red, Color4.Green, Color4.Blue, },
                            new GLRenderDataTranslationRotation(new Vector3(-4, 0, -10))));

            }

            #endregion


            #region textures
            if (true)
            {
                GLRenderControl rt = GLRenderControl.Tri();

                rObjects.Add(items.Shader("TEXOT"),
                        GLRenderableItem.CreateVector4Vector2(items, rt,
                                GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                                new GLRenderDataTranslationRotationTexture(items.Tex("dotted2"), new Vector3(-2, 0, 0))
                                ));


                rObjects.Add(items.Shader("TEXOT"), "EDDCube",
                            GLRenderableItem.CreateVector4Vector2(items, rt,
                            GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                            new GLRenderDataTranslationRotationTexture(items.Tex("logo8bpp"), new Vector3(-2, 1, -2))
                            ));

                rObjects.Add(items.Shader("TEXOT"), "woodbox",
                            GLRenderableItem.CreateVector4Vector2(items, rt,
                            GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                            new GLRenderDataTranslationRotationTexture(items.Tex("wooden"), new Vector3(-2, 2, -4))
                            ));

                GLRenderControl rq = GLRenderControl.Quads();

                rObjects.Add(items.Shader("TEXOT"),
                            GLRenderableItem.CreateVector4Vector2(items, rq,
                            GLShapeObjectFactory.CreateQuad(1.0f, 1.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                            new GLRenderDataTranslationRotationTexture(items.Tex("dotted2"), new Vector3(-2, 3, -6))
                            ));

                rObjects.Add(items.Shader("TEXOT"),
                        GLRenderableItem.CreateVector4Vector2(items, rq,
                            GLShapeObjectFactory.CreateQuad(1.0f, 1.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                            new GLRenderDataTranslationRotationTexture(items.Tex("dotted2"), new Vector3(-2, 4, -8))
                            ));

                rObjects.Add(items.Shader("TEXOT"),
                    GLRenderableItem.CreateVector4Vector2(items, rq,
                            GLShapeObjectFactory.CreateQuad(1.0f, 1.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                            new GLRenderDataTranslationRotationTexture(items.Tex("dotted"), new Vector3(-2, 5, -10))
                            ));

                GLRenderControl rqnc = GLRenderControl.Quads(cullface: false);

                rObjects.Add(items.Shader("TEXOT"), "EDDFlat",
                    GLRenderableItem.CreateVector4Vector2(items, rqnc,
                    GLShapeObjectFactory.CreateQuad(2.0f, items.Tex("logo8bpp").Width, items.Tex("logo8bpp").Height, new Vector3(-0, 0, 0)), GLShapeObjectFactory.TexQuad,
                            new GLRenderDataTranslationRotationTexture(items.Tex("logo8bpp"), new Vector3(0, 0, 0))
                            ));

                rObjects.Add(items.Shader("TEXOT"),
                    GLRenderableItem.CreateVector4Vector2(items, rqnc,
                            GLShapeObjectFactory.CreateQuad(1.5f, new Vector3( -90f.Radians(), 0, 0)), GLShapeObjectFactory.TexQuad,
                            new GLRenderDataTranslationRotationTexture(items.Tex("smile"), new Vector3(0, 0, -2))
                           ));

                rObjects.Add(items.Shader("TEXOCT"), "woodboxc1",
                        GLRenderableItem.CreateVector4Vector2(items, rt,
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                            new GLRenderDataTranslationRotationTexture(items.Tex("wooden"), new Vector3(0, 0, -4))
                            ));

                rObjects.Add(items.Shader("TEXOCT"), "woodboxc2",
                        GLRenderableItem.CreateVector4Vector2(items, rt,
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                            new GLRenderDataTranslationRotationTexture(items.Tex("wooden"), new Vector3(0, 0, -6))
                           ));

                rObjects.Add(items.Shader("TEXOCT"), "woodboxc3",
                        GLRenderableItem.CreateVector4Vector2(items, rt,
                        GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                            new GLRenderDataTranslationRotationTexture(items.Tex("wooden"), new Vector3(0, 0, -8))
                            ));

                rObjects.Add(items.Shader("TEXOCT"), "sphere5",
                        GLRenderableItem.CreateVector4Vector2(items, rt,
                        GLSphereObjectFactory.CreateTexturedSphereFromTriangles(3, 1.0f),
                            new GLRenderDataTranslationRotationTexture(items.Tex("wooden"), new Vector3(0, 0, -10))
                            ));

                rObjects.Add(items.Shader("TEXOCT"), "sphere6",
                        GLRenderableItem.CreateVector4Vector2(items, rt,
                        GLSphereObjectFactory.CreateTexturedSphereFromTriangles(3, 1.5f),
                            new GLRenderDataTranslationRotationTexture(items.Tex("golden"), new Vector3(0, 0, -12))
                            ));

                var cyl = GLCylinderObjectFactory.CreateCylinderFromTriangles(3, 20, 20, 2, caps:true);
                GLRenderControl rt2 = GLRenderControl.Tri();
                rObjects.Add(items.Shader("TEXOTNoRot"), "cylinder1",
                GLRenderableItem.CreateVector4Vector2(items, rt2, cyl,
                            new GLRenderDataTranslationRotationTexture(items.Tex("logo8bpp"), new Vector3(30, 0, 10))
                            ));

                // for this one, demo indexes and draw with CW to show it works

                var cyl2 = GLCylinderObjectFactory.CreateCylinderFromTrianglesIndexes(3, 10, 20, 2, caps: true, ccw:false);

                rObjectscw.Add(items.Shader("TEXOTNoRot"), "cylinder2",
                        GLRenderableItem.CreateVector4Vector2Indexed(items, rt2, cyl2,
                            new GLRenderDataTranslationRotationTexture(items.Tex("logo8bpp"), new Vector3(40, 0, 10))
                            ));


            }

            #endregion

            #region 2dArrays
            if (true)
            {
                items.Add( new GLTexturedShader2DBlendWithWorldCoord(), "TEX2DA");
                items.Add(new GLTexture2DArray(new Bitmap[] { Properties.Resources.mipmap2, Properties.Resources.mipmap3 }, 9), "2DArray2");

                GLRenderControl rq = GLRenderControl.Quads();

                rObjects.Add(items.Shader("TEX2DA"), "2DA",
                    GLRenderableItem.CreateVector4Vector2(items, rq,
                            GLShapeObjectFactory.CreateQuad(2.0f), GLShapeObjectFactory.TexQuad,
                            new GLRenderDataTranslationRotationTexture(items.Tex("2DArray2"), new Vector3(-8, 0, 2))
                        ));


                items.Add( new GLTexture2DArray(new Bitmap[] { Properties.Resources.dotted, Properties.Resources.dotted2 }), "2DArray2-1");

                rObjects.Add(items.Shader("TEX2DA"), "2DA-1",
                    GLRenderableItem.CreateVector4Vector2(items, rq,
                                    GLShapeObjectFactory.CreateQuad(2.0f), GLShapeObjectFactory.TexQuad,
                                new GLRenderDataTranslationRotationTexture(items.Tex("2DArray2-1"), new Vector3(-8, 0, -2))
                        ));
            }

            #endregion

            #region Instancing
            if (true)
            {
                items.Add(new GLShaderPipeline(new GLPLVertexShaderModelCoordWithMatrixTranslation(), new GLPLFragmentShaderVSColour()),"IC-1");

                Matrix4[] pos1 = new Matrix4[3];
                pos1[0] = Matrix4.CreateTranslation(new Vector3(10, 0, 10));
                pos1[1] = Matrix4.CreateTranslation(new Vector3(10, 5, 10));
                pos1[2] = Matrix4.CreateRotationX(45f.Radians());
                pos1[2] *= Matrix4.CreateTranslation(new Vector3(10, 10, 10));

                GLRenderControl rp = GLRenderControl.Points(10);

                rObjects.Add(items.Shader("IC-1"), "1-a",
                                        GLRenderableItem.CreateVector4Matrix4(items, rp,
                                                GLShapeObjectFactory.CreateQuad(2.0f), pos1,
                                                null, pos1.Length));


                Matrix4[] pos2 = new Matrix4[3];
                pos2[0] = Matrix4.CreateRotationX(-80f.Radians());
                pos2[0] *= Matrix4.CreateTranslation(new Vector3(20, 0, 10));
                pos2[1] = Matrix4.CreateRotationX(-70f.Radians());
                pos2[1] *= Matrix4.CreateTranslation(new Vector3(20, 5, 10));
                pos2[2] = Matrix4.CreateRotationZ(-60f.Radians());
                pos2[2] *= Matrix4.CreateTranslation(new Vector3(20, 10, 10));

                items.Add( new GLShaderPipeline(new GLPLVertexShaderTextureModelCoordWithMatrixTranslation(), new GLPLFragmentShaderTexture()),"IC-2");

                GLRenderControl rq = GLRenderControl.Quads();
                rq.CullFace = false;

                GLRenderDataTexture rdt = new GLRenderDataTexture(items.Tex("wooden"));

                rObjects.Add(items.Shader("IC-2"), "1-b",
                                        GLRenderableItem.CreateVector4Vector2Matrix4(items, rq,
                                                GLShapeObjectFactory.CreateQuad(2.0f), GLShapeObjectFactory.TexQuad, pos2, 
                                                rdt, pos2.Length));
            }
            #endregion


            #region Tesselation
            if (true)
            {
                var shdrtesssine = new GLTesselationShaderSinewave(20, 0.5f, 2f);
                items.Add(shdrtesssine, "TESx1");

                GLRenderControl rp = GLRenderControl.Patches(4);

                rObjects.Add(items.Shader("TESx1"), "O-TES1",
                    GLRenderableItem.CreateVector4(items, rp,
                                        GLShapeObjectFactory.CreateQuad2(6.0f, 6.0f),
                                        new GLRenderDataTranslationRotationTexture(items.Tex("logo8bpp"), new Vector3(12, 0, 0), new Vector3( -90f.Radians(), 0, 0))
                                        ));
            }

            #endregion


            #region MipMaps
            if (true)
            {
                items.Add( new GLTexture2D(Properties.Resources.mipmap2, 9), "mipmap1");

                rObjects.Add(items.Shader("TEXOT"), "mipmap1",
                    GLRenderableItem.CreateVector4Vector2(items, GLRenderControl.Tri(),
                                    GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                                    new GLRenderDataTranslationRotationTexture(items.Tex("mipmap1"), new Vector3(-10, 0, 0))
                            ));
            }

            #endregion

            #region Tape

            if (true)
            {
                var p = GLTapeObjectFactory.CreateTape(new Vector3(0, 5, 10), new Vector3(100, 50, 100), 4, 20, 80F.Radians(), ensureintegersamples: true);

                items.Add( new GLTexture2D(Properties.Resources.Logo8bpp), "tapelogo");

                items.Tex("tapelogo").SetSamplerMode(OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat, OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat);

                items.Add(new GLTexturedShaderTriangleStripWithWorldCoord(true), "tapeshader" );

                GLRenderControl rts = GLRenderControl.TriStrip();
                rts.CullFace = false;

                rObjects.Add(items.Shader("tapeshader"), "tape1", GLRenderableItem.CreateVector4(items, rts, p , new GLRenderDataTexture(items.Tex("tapelogo"))));
            }

            if (true)
            {
                var p = GLTapeObjectFactory.CreateTape(new Vector3(-0, 5, 10), new Vector3(-100, 50, 100), 4, 20, 80F.Radians(), ensureintegersamples: true);

                items.Add(new GLTexture2D(Properties.Resources.Logo8bpp), "tapelogo2");

                items.Tex("tapelogo2").SetSamplerMode(OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat, OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat);

                items.Add(new GLTexturedShaderTriangleStripWithWorldCoord(true), "tapeshader2");

                GLRenderControl rts = GLRenderControl.TriStrip();
                rts.CullFace = false;

                rObjects.Add(items.Shader("tapeshader2"), "tape2", GLRenderableItem.CreateVector4(items, rts, p, new GLRenderDataTexture(items.Tex("tapelogo2"))));
            }

            if (true)
            {
                Vector4[] points = new Vector4[] { new Vector4(100, 5, 40, 0), new Vector4(0, 5, 100, 0), new Vector4(-50, 5, 80, 0), new Vector4(-60, 5, 40, 0) };

                var p = GLTapeObjectFactory.CreateTape(points.ToArray(), 4, 20, 90F.Radians(), ensureintegersamples: true, margin:0.5f);

                items.Add( new GLTexture2D(Properties.Resources.chevron), "tapelogo3");
                items.Tex("tapelogo3").SetSamplerMode(OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat, OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat);
                items.Add( new GLTexturedShaderTriangleStripWithWorldCoord(true), "tapeshader3");

                GLRenderControl rts = GLRenderControl.TriStrip(p.Item3);
                rts.CullFace = false;

                GLRenderableItem ri = GLRenderableItem.CreateVector4(items, rts, p.Item1.ToArray(), new GLRenderDataTexture(items.Tex("tapelogo3")));
                ri.CreateElementIndex(items.NewBuffer(), p.Item2.ToArray(), p.Item3);

                rObjects.Add(items.Shader("tapeshader3"), "tape3", ri);
            }

            #endregion

            #region Screen coords
            // fixed point on screen
            if (true)
            {
                Vector4[] p = new Vector4[4];

                p[0] = new Vector4(10, 10, 0, 1);       // topleft - correct winding for our system. For dotted, red/blue at top as dots
                p[1] = new Vector4(10, 100, 0, 1);      // bottomleft
                p[2] = new Vector4(50, 10, 0, 1);       // topright
                p[3] = new Vector4(50, 100, 0, 1);      // botright

                items.Add( new GLDirect(), "ds1");

                GLRenderControl rts = GLRenderControl.TriStrip();
                GLRenderDataTexture rdt = new GLRenderDataTexture(items.Tex("dotted2"));

                rObjects.Add(items.Shader("ds1"), "ds1", GLRenderableItem.CreateVector4(items, rts, p , rdt));
            }

            #endregion

            #region Index/instance draw

            // multi element index draw
            if (true)
            {
                float CS = 2, O = -20, OY = 0;
                float[] v = new float[]
                {
                    0,0,0,      // basevertex=1, pad with empties at the start to demo
                    -CS+O, -CS+OY, -CS,
                    -CS+O,  CS+OY, -CS,
                     CS+O, -CS+OY, -CS,
                     CS+O,  CS+OY, -CS,
                     CS+O, -CS+OY,  CS,
                     CS+O,  CS+OY,  CS,
                    -CS+O, -CS+OY,  CS,
                    -CS+O,  CS+OY,  CS,
                };

                byte[] vertex_indices = new byte[]
                {
                    2, 1, 0,
                    3, 1, 2,
                    4, 3, 2,
                    5, 3, 4,
                    6, 5, 4,
                    7, 5, 6,
                    0, 7, 6,
                    1, 7, 0,
                    2, 0, 6,
                    6, 4, 2,
                    3, 5, 7,
                    1, 3, 7
                };

                GLRenderControl rt = GLRenderControl.Tri();
                GLRenderableItem ri = GLRenderableItem.CreateFloats(items, rt, v, 3);
                ri.CreateElementIndexByte(items.NewBuffer(), vertex_indices);
                ri.BaseVertex = 1;      // first vertex not used

                items.Add(new GLColourShaderWithWorldCoordXX(), "es1");
                rObjects.Add(items.Shader("es1"), "es1", ri);
            }

            // multi element index draw with primitive restart, draw a triangle strip
            if (true)
            {
                float X = -10, Z = -10;
                float X2 = -8, Z2 = -10;
                float[] v = new float[]
                {
                    1+X,0,1+Z,
                    1+X,0,0+Z,
                    0+X,0,1+Z,
                    0+X,0,0+Z,
                    1+X2,0,1+Z2,
                    1+X2,0,0+Z2,
                    0+X2,0,1+Z2,
                    0+X2,0,0+Z2,
                };

                GLRenderControl rts = GLRenderControl.TriStrip(0xff);
                rts.DepthTest = false;
                rts.CullFace = false;

                GLRenderableItem ri = GLRenderableItem.CreateFloats(items, rts, v, 3);
                ri.CreateRectangleElementIndexByte(items.NewBuffer(), 2,0xff);

                items.Add(new GLColourShaderWithWorldCoordXX(), "es2");

                rObjects.Add(items.Shader("es2"), "es2", ri);
            }

            // indirect multi draw with element index - two red squares in foreground.
            if (true)
            {
                float X = -10, Z = -12;
                float X2 = -8, Z2 = -12;
                float[] v = new float[]
                {
                    1+X,0,1+Z,
                    1+X,0,0+Z,
                    0+X,0,1+Z,
                    0+X,0,0+Z,
                    1+X2,0,1+Z2,
                    1+X2,0,0+Z2,
                    0+X2,0,1+Z2,
                    0+X2,0,0+Z2,
                };

                GLRenderControl rts = GLRenderControl.TriStrip(0xff);
                rts.DepthTest = false;
                rts.CullFace = false;

                GLRenderableItem ri = GLRenderableItem.CreateFloats(items, rts, v, 3);
                ri.CreateRectangleElementIndexByte(items.NewBuffer(), 2);  // put the primitive restart markers in, but we won't use them

                ri.IndirectBuffer = new GLBuffer(std430:true);  // disable alignment to vec4 for arrays for this buffer.
                ri.MultiDrawCount = 2;
                ri.IndirectBuffer.AllocateBytes(ri.MultiDrawCountStride * ri.MultiDrawCount + 4);
                ri.IndirectBuffer.StartWrite(0, ri.IndirectBuffer.Length);
                ri.IndirectBuffer.Write(1.0f);        // dummy float to demo index offset
                ri.BaseIndex = 4;       // and indicate that the base command index is 4
                ri.IndirectBuffer.WriteIndirectElements(4, 1, 0, 0, 0);       // draw indexes 0-3
                ri.IndirectBuffer.WriteIndirectElements(4, 1, 5, 0, 0);       // and 5-8
                ri.IndirectBuffer.StopReadWrite();
                var data = ri.IndirectBuffer.ReadInts(0,10);                            // notice both are red due to primitive ID=1

                items.Add(new GLColourShaderWithWorldCoordXX(), "es3");

                rObjects.Add(items.Shader("es3"), "es3", ri);
            }

            #endregion

            #region Bindless texture
            if (true)
            {
                IGLTexture[] btextures = new IGLTexture[3];
                btextures[0] = items.Add(new GLTexture2D(Properties.Resources.Logo8bpp), "bl1");
                btextures[1] = items.Add(new GLTexture2D(Properties.Resources.dotted2), "bl2");
                btextures[2] = items.Add(new GLTexture2D(Properties.Resources.golden), "bl3");

                GLBindlessTextureHandleBlock bl = new GLBindlessTextureHandleBlock(11,btextures);

                GLStatics.Check();

                float X = -10, Z = -14;
                float X2 = -9, Z2 = -15;
                float X3 = -8, Z3 = -16;
                float[] v = new float[]
                {
                    0+X,0,1+Z,
                    0+X,0,0+Z,
                    1+X,0,1+Z,
                    1+X,0,0+Z,

                    0+X2,0,1+Z2,
                    0+X2,0,0+Z2,
                    1+X2,0,1+Z2,
                    1+X2,0,0+Z2,

                    0+X3,0,1+Z3,
                    0+X3,0,0+Z3,
                    1+X3,0,1+Z3,
                    1+X3,0,0+Z3,
                };

                GLRenderControl rts = GLRenderControl.TriStrip(0xff);

                GLRenderableItem ri = GLRenderableItem.CreateFloats(items, rts, v, 3);
                ri.CreateRectangleElementIndexByte(items.NewBuffer(), 3);

                items.Add(new GLBindlessTextureShaderWithWorldCoord(11), "bt1");

                rObjects.Add(items.Shader("bt1"), "bt1-1", ri);
            }


            #endregion

            #region Objects

            if (true)
            {
                GLWaveformObjReader read = new GLWaveformObjReader();
                string s = System.Text.Encoding.UTF8.GetString(Properties.Resources.cubeobj);

                var objlist = read.ReadOBJData(s);

                if (objlist.Count > 0)
                {
                    GLBuffer vert = new GLBuffer();
                    vert.AllocateFill(objlist[0].Vertices.Vertices.ToArray());

                    var shader = new GLUniformColourShaderWithObjectTranslation();

                    GLRenderControl rts = GLRenderControl.Tri();

                    foreach (var obj in objlist)
                    {
                        if (obj.Indices.VertexIndices.Count > 0)
                        {
                            obj.Indices.RefactorVertexIndiciesIntoTriangles();

                            var ri = GLRenderableItem.CreateVector4(items, rts, vert, 0, 0, new GLRenderDataTranslationRotationColour(Color.FromName(obj.Material), new Vector3(20, 0, -20), scale: 2f));           // renderable item pointing to vert for vertexes
                            ri.CreateElementIndex(items.NewBuffer(), obj.Indices.VertexIndices.ToArray(), 0);       // using the refactored indexes, create an index table and use

                            rObjects.Add(shader, ri);
                        }
                    }
                }
            }

            if (true)       // waveform object
            {
                GLWaveformObjReader read = new GLWaveformObjReader();
                string s = System.Text.Encoding.UTF8.GetString(Properties.Resources.textobj1);

                var objlist = read.ReadOBJData(s);

                if (objlist.Count > 0)
                {
                    GLBuffer vert = new GLBuffer();
                    vert.AllocateFill(objlist[0].Vertices.Vertices.ToArray(), objlist[0].Vertices.TextureVertices2.ToArray());

                    var shader = new GLTexturedShaderWithObjectTranslation();
                    items.Add(new GLTexture2D(Properties.Resources.wooden), "wood");

                    GLRenderControl rts = GLRenderControl.Tri();
                    //rts.CullFace = false;

                    foreach (var obj in objlist)
                    {
                        obj.Indices.RefactorVertexIndiciesIntoTriangles();

                        IGLTexture tex = items.Tex(obj.Material);

                        var ri = GLRenderableItem.CreateVector4Vector2(items, rts, vert, vert.Positions[0], vert.Positions[1], 0, 
                                new GLRenderDataTranslationRotationTexture(tex, new Vector3(15, 0, -20), scale: 2f));           // renderable item pointing to vert for vertexes
                        ri.CreateElementIndex(items.NewBuffer(), obj.Indices.VertexIndices.ToArray(), 0);       // using the refactored indexes, create an index table and use

                        rObjects.Add(shader, ri);
                    }
                }
            }

            if ( true )     // obj creator on those
            {
                GLWaveformObjReader read = new GLWaveformObjReader();
                var objlist = read.ReadOBJData(System.Text.Encoding.UTF8.GetString(Properties.Resources.textobj1));

                GLWavefrontObjCreator oc = new GLWavefrontObjCreator(items, rObjects);

                oc.Create(objlist, new Vector3(10, 0, -20), new Vector3(0, 0, 0), 2.0f);

                objlist = read.ReadOBJData(System.Text.Encoding.UTF8.GetString(Properties.Resources.cubeobj));

                oc.Create(objlist, new Vector3(5, 0, -20), new Vector3(0, 0, 0), 2.0f);
            }

            if ( true ) // another waveform object
            {
                GLWaveformObjReader read = new GLWaveformObjReader();
                var objlist = read.ReadOBJData(System.Text.Encoding.UTF8.GetString(Properties.Resources.Koltuk));

                GLWavefrontObjCreator oc = new GLWavefrontObjCreator(items, rObjects);
                oc.DefaultColour = Color.Red;

                bool v = oc.Create(objlist, new Vector3(-20, 0, -20), new Vector3(0, 0, 0), 8.0f);
                System.Diagnostics.Debug.Assert(v == true);
            }

            if (true)   // instanced sinewive
            {
                var shdrtesssine = new GLTesselationShaderSinewaveAutoscaleLookatInstanced(20, 0.4f, 1f, rotate:true, rotateelevation:false);
                items.Add(shdrtesssine, "TESIx1");

                Vector4[] pos = new Vector4[]       //w = image index
                {   
                    new Vector4(40,0,-30,0),
                    new Vector4(39,0,-20,1),
                    new Vector4(38,0,-10,2),
                };

                var texarray = new GLTexture2DArray(new Bitmap[] { Properties.Resources.beacon, Properties.Resources.planetaryNebula, Properties.Resources.wooden });
                items.Add(texarray, "Sinewavetex");

                GLRenderControl rp = GLRenderControl.Patches(4);
                var dt = GLRenderableItem.CreateVector4Vector4(items, rp,
                                        GLShapeObjectFactory.CreateQuad2(10.0f, 10.0f, new Vector3(-0f.Radians(),0,0)), pos,
                                        new GLRenderDataTexture(texarray),
                                        ic:3, seconddivisor:1);

                rObjects.Add(shdrtesssine, "O-TES2", dt);

            }

            #endregion


            #region Instancing with matrix and lookat
            if (true)
            {
                var texarray = new GLTexture2DArray(new Bitmap[] { Properties.Resources.dotted2, Properties.Resources.planetaryNebula, Properties.Resources.wooden });
                items.Add(texarray);

                var shader = new GLShaderPipeline(new GLPLVertexShaderQuadTextureWithMatrixTranslation(), new GLPLFragmentShaderTexture2DIndexed(0));
                items.Add(shader);

                shader.StartAction += (s) => { texarray.Bind(1); };

                Matrix4[] pos = new Matrix4[3];
                pos[0] = Matrix4.CreateTranslation(new Vector3(-20, 0, -10));

                pos[1] = Matrix4.CreateTranslation(new Vector3(-20, 5, -10));
                pos[1][0, 3] = 1;   // image no
                pos[1][1, 3] = 1;   // lookat control

                pos[2] = Matrix4.CreateRotationX(-45f.Radians());
                pos[2] *= Matrix4.CreateTranslation(new Vector3(-20, 10, -10));
                pos[2][0, 3] = 2;
                pos[2][1, 3] = 2;   // lookat control
                GLRenderControl rp = GLRenderControl.Quads();

                rObjects.Add(shader, "1-atex2",
                                        GLRenderableItem.CreateMatrix4(items, rp, pos, 4, ic: 3, matrixdivisor: 1));

            }

            #endregion

            #region Matrix Calc Uniform

            items.Add(new GLMatrixCalcUniformBlock(),"MCUB");     // def binding of 0

            #endregion

            dataoutbuffer = items.NewStorageBlock(5);
            dataoutbuffer.AllocateBytes(sizeof(float) * 4 * 32, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicRead);    // 32 vec4 back

        }

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(GLMatrixCalc mc, long time)
        {
            //System.Diagnostics.Debug.WriteLine("Draw");

            float degrees = ((float)time / 5000.0f * 360.0f) % 360f;
            float degreesd2 = ((float)time / 10000.0f * 360.0f) % 360f;
            float degreesd4 = ((float)time / 20000.0f * 360.0f) % 360f;
            float zeroone = (degrees >= 180) ? (1.0f - (degrees - 180.0f) / 180.0f) : (degrees / 180f);

            if (rObjects.Contains("woodbox"))
            {
                ((GLRenderDataTranslationRotation)(rObjects["woodbox"].RenderData)).XRotDegrees = degrees;
                ((GLRenderDataTranslationRotation)(rObjects["woodbox"].RenderData)).ZRotDegrees = degrees;

                ((GLRenderDataTranslationRotation)(rObjects["EDDCube"].RenderData)).YRotDegrees = degrees;
                ((GLRenderDataTranslationRotation)(rObjects["EDDCube"].RenderData)).ZRotDegrees = degreesd2;

                ((GLRenderDataTranslationRotation)(rObjects["sphere3"].RenderData)).XRotDegrees = -degrees;
                ((GLRenderDataTranslationRotation)(rObjects["sphere3"].RenderData)).YRotDegrees = degrees;
                ((GLRenderDataTranslationRotation)(rObjects["sphere4"].RenderData)).YRotDegrees = degrees;
                ((GLRenderDataTranslationRotation)(rObjects["sphere4"].RenderData)).ZRotDegrees = -degreesd2;
                ((GLRenderDataTranslationRotation)(rObjects["sphere7"].RenderData)).YRotDegrees = degreesd4;
            }

            if (items.Contains("TEXOCT"))
                ((GLPLVertexShaderTextureModelCoordsWithObjectCommonTranslation)items.Shader("TEXOCT").Get(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader)).Transform.YRotDegrees = degrees;

            if (items.Contains("TEX2DA"))
                ((GLPLFragmentShaderTexture2DBlend)items.Shader("TEX2DA").Get(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader)).Blend = zeroone;

            if (items.Contains("tapeshader"))
                ((GLTexturedShaderTriangleStripWithWorldCoord)items.Shader("tapeshader")).TexOffset = new Vector2(degrees / 360f, 0.0f);
            if (items.Contains("tapeshader2"))
                ((GLTexturedShaderTriangleStripWithWorldCoord)items.Shader("tapeshader2")).TexOffset = new Vector2(-degrees / 360f, 0.0f);
            if (items.Contains("tapeshader3"))
                ((GLTexturedShaderTriangleStripWithWorldCoord)items.Shader("tapeshader3")).TexOffset = new Vector2(-degrees / 360f, 0.0f);

            if (items.Contains("TESx1"))
                ((GLTesselationShaderSinewave)items.Shader("TESx1")).Phase = degrees / 360.0f;
            if (items.Contains("TESIx1"))
                ((GLTesselationShaderSinewaveAutoscaleLookatInstanced)items.Shader("TESIx1")).Phase = degrees / 360.0f;

            GLStatics.Check();
            GLMatrixCalcUniformBlock mcub = (GLMatrixCalcUniformBlock)items.UB("MCUB");
            mcub.SetFull(gl3dcontroller.MatrixCalc);

            rObjects.Render(glwfc.RenderState, gl3dcontroller.MatrixCalc);

            GL.FrontFace(FrontFaceDirection.Cw);
            rObjectscw.Render(glwfc.RenderState, gl3dcontroller.MatrixCalc);
            GL.FrontFace(FrontFaceDirection.Ccw);

            var azel = gl3dcontroller.PosCamera.Lookat.AzEl(gl3dcontroller.PosCamera.EyePosition, true);


            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " from " + gl3dcontroller.MatrixCalc.EyePosition + " cdir " + gl3dcontroller.PosCamera.CameraDirection + " azel " + azel + " zoom " + gl3dcontroller.PosCamera.ZoomFactor + " dist " + gl3dcontroller.MatrixCalc.EyeDistance + " FOV " + gl3dcontroller.MatrixCalc.FovDeg;

            //GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            //Vector4[] databack = dataoutbuffer.ReadVector4(0, 4);
            //for (int i = 0; i < databack.Length; i += 1)
            //{
            //   // databack[i] = databack[i] / databack[i].W;
            //   // databack[i].X = databack[i].X * gl3dcontroller.glControl.Width / 2 + gl3dcontroller.glControl.Width/2;
            //   // databack[i].Y = gl3dcontroller.glControl.Height - databack[i].Y * gl3dcontroller.glControl.Height;
            //    System.Diagnostics.Debug.WriteLine("{0}={1}", i, databack[i].ToStringVec(true));
            //}
            //GLStatics.Check();

        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboardSlewsInvalidate(true, OtherKeys);
            gl3dcontroller.Redraw();
        }

        private void OtherKeys( OpenTKUtils.Common.KeyboardMonitor kb )
        {
            if (kb.HasBeenPressed(Keys.F5, OpenTKUtils.Common.KeyboardMonitor.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(0, 0, 0), 1, 2);
            }

            if (kb.HasBeenPressed(Keys.F6, OpenTKUtils.Common.KeyboardMonitor.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(4, 0, 0), 1, 2);
            }

            if (kb.HasBeenPressed(Keys.F7, OpenTKUtils.Common.KeyboardMonitor.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(10, 0, -10), 1, 2);
            }

            if (kb.HasBeenPressed(Keys.F8, OpenTKUtils.Common.KeyboardMonitor.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(50, 0, 50), 1, 2);
            }

            if (kb.HasBeenPressed(Keys.F4, OpenTKUtils.Common.KeyboardMonitor.ShiftState.None))
            {
                gl3dcontroller.ChangePerspectiveMode(!gl3dcontroller.MatrixCalc.InPerspectiveMode);
            }


        }

    }

    public class GLDirect : GLShaderPipeline
    {
        public GLDirect(Action<IGLProgramShader> start = null, Action<IGLProgramShader> finish = null) : base(start, finish)
        {
            AddVertexFragment(new GLPLVertexShaderTextureScreenCoordWithTriangleStripCoord(), new GLPLFragmentShaderTextureTriangleStrip(false));
        }
    }

    public class GLColourShaderWithWorldCoordXX : GLShaderPipeline
    {
        public GLColourShaderWithWorldCoordXX(Action<IGLProgramShader> start = null, Action<IGLProgramShader> finish = null) : base(start, finish)
        {
            AddVertexFragment(new GLPLVertexShaderWorldCoord(), new GLPLFragmentIDShaderColour(2));
        }
    }

    public class GLBindlessTextureShaderWithWorldCoord : GLShaderPipeline
    {
        public GLBindlessTextureShaderWithWorldCoord(int arbbindingpoint, Action<IGLProgramShader> start = null, Action<IGLProgramShader> finish = null) : base(start, finish)
        {
            AddVertexFragment(new GLPLVertexShaderTextureWorldCoordWithTriangleStripCoord(), new GLPLBindlessFragmentShaderTextureTriangleStrip(arbbindingpoint));
        }
    }



}


