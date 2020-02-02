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
    public partial class ShaderTestMain : Form
    {
        private OpenTKUtils.WinForm.GLWinFormControl glwfc;
        private Controller3D gl3dcontroller;

        private Timer systemtimer = new Timer();

        GLRenderProgramSortedList rObjects = new GLRenderProgramSortedList();
        GLItemsList items = new GLItemsList();
        GLStorageBlock dataoutbuffer;

        public ShaderTestMain()
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
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.ZoomDistance = 20F;
            gl3dcontroller.Start(glwfc, new Vector3(0, 0, 0), new Vector3(110f, 0, 0f), 1F);

            gl3dcontroller.KeyboardTravelSpeed = (ms,eyedist) =>
            {
                return (float)ms / 100.0f;
            };

            items.Add("TEXOT", new GLTexturedShaderWithObjectTranslation());
            items.Add("COSW", new GLColourShaderWithWorldCoord());
            items.Add("COSOT", new GLColourShaderWithObjectTranslation());
            items.Add("TEXOCT", new GLTexturedShaderWithObjectCommonTranslation());

            items.Add("dotted", new GLTexture2D(Properties.Resources.dotted));
            items.Add("logo8bpp", new GLTexture2D(Properties.Resources.Logo8bpp));
            items.Add("dotted2", new GLTexture2D(Properties.Resources.dotted2));
            items.Add("wooden", new GLTexture2D(Properties.Resources.wooden));
            items.Add("shoppinglist", new GLTexture2D(Properties.Resources.shoppinglist));
            items.Add("golden", new GLTexture2D(Properties.Resources.golden));
            items.Add("smile", new GLTexture2D(Properties.Resources.smile5300_256x256x8));
            items.Add("moon", new GLTexture2D(Properties.Resources.moonmap1k));

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
                                                             new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                                   );
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
                            GLShapeObjectFactory.CreateQuad(1.5f, new Vector3(-90, 0, 0)), GLShapeObjectFactory.TexQuad,
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
            }

            #endregion

            #region 2dArrays
            {
                items.Add("TEX2DA", new GLTexturedShader2DBlendWithWorldCoord());
                items.Add("2DArray2", new GLTexture2DArray(new Bitmap[] { Properties.Resources.mipmap2, Properties.Resources.mipmap3 }, 9));

                GLRenderControl rq = GLRenderControl.Quads();

                rObjects.Add(items.Shader("TEX2DA"), "2DA",
                    GLRenderableItem.CreateVector4Vector2(items, rq,
                            GLShapeObjectFactory.CreateQuad(2.0f), GLShapeObjectFactory.TexQuad,
                            new GLRenderDataTranslationRotationTexture(items.Tex("2DArray2"), new Vector3(-8, 0, 2))
                        ));


                items.Add("2DArray2-1", new GLTexture2DArray(new Bitmap[] { Properties.Resources.dotted, Properties.Resources.dotted2 }));

                rObjects.Add(items.Shader("TEX2DA"), "2DA-1",
                    GLRenderableItem.CreateVector4Vector2(items, rq,
                                    GLShapeObjectFactory.CreateQuad(2.0f), GLShapeObjectFactory.TexQuad,
                                new GLRenderDataTranslationRotationTexture(items.Tex("2DArray2-1"), new Vector3(-8, 0, -2))
                        ));
            }

            #endregion

            #region Instancing
            {
                items.Add("IC-1", new GLShaderPipeline(new GLPLVertexShaderMatrixModelCoordWithMatrixTranslation(), new GLPLFragmentShaderColour()));

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

                items.Add("IC-2", new GLShaderPipeline(new GLPLVertexShaderTextureModelCoordWithMatrixTranslation(), new GLPLFragmentShaderTexture()));

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

            {
                var shdrtesssine = new GLTesselationShaderSinewave(20, 0.5f, 2f);
                items.Add("TESx1", shdrtesssine);

                GLRenderControl rp = GLRenderControl.Patches(4);

                rObjects.Add(items.Shader("TESx1"), "O-TES1",
                    GLRenderableItem.CreateVector4(items, rp,
                                        GLShapeObjectFactory.CreateQuad2(6.0f, 6.0f),
                                        new GLRenderDataTranslationRotationTexture(items.Tex("logo8bpp"), new Vector3(12, 0, 0), new Vector3(-90, 0, 0))
                                        ));
            }

            #endregion


            #region MipMaps
            {
                items.Add("mipmap1", new GLTexture2D(Properties.Resources.mipmap2, 9));

                rObjects.Add(items.Shader("TEXOT"), "mipmap1",
                    GLRenderableItem.CreateVector4Vector2(items, GLRenderControl.Tri(),
                                    GLCubeObjectFactory.CreateSolidCubeFromTriangles(1f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                                    new GLRenderDataTranslationRotationTexture(items.Tex("mipmap1"), new Vector3(-10, 0, 0))
                            ));
            }

            #endregion

            #region Tape

            {
                var p = GLTapeObjectFactory.CreateTape(new Vector3(0, 5, 10), new Vector3(100, 50, 100), 4, 20, 80F.Radians(), ensureintegersamples: true);

                items.Add("tapelogo", new GLTexture2D(Properties.Resources.Logo8bpp));

                items.Tex("tapelogo").SetSamplerMode(OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat, OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat);

                items.Add("tapeshader", new GLTexturedShaderTriangleStripWithWorldCoord(true));

                GLRenderControl rts = GLRenderControl.TriStrip();
                rts.CullFace = false;

                rObjects.Add(items.Shader("tapeshader"), "tape1", GLRenderableItem.CreateVector4(items, rts, p , new GLRenderDataTexture(items.Tex("tapelogo"))));
            }

            {
                var p = GLTapeObjectFactory.CreateTape(new Vector3(-0, 5, 10), new Vector3(-100, 50, 100), 4, 20, 80F.Radians(), ensureintegersamples: true);

                items.Add("tapelogo2", new GLTexture2D(Properties.Resources.Logo8bpp));

                items.Tex("tapelogo2").SetSamplerMode(OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat, OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat);

                items.Add("tapeshader2", new GLTexturedShaderTriangleStripWithWorldCoord(true));

                GLRenderControl rts = GLRenderControl.TriStrip();
                rts.CullFace = false;

                rObjects.Add(items.Shader("tapeshader2"), "tape2", GLRenderableItem.CreateVector4(items, rts, p, new GLRenderDataTexture(items.Tex("tapelogo"))));
            }

            #endregion
            
            #region Screen coords
            // fixed point on screen
            {
                Vector4[] p = new Vector4[4];

                p[0] = new Vector4(10, 10, 0, 1);       // topleft - correct winding for our system. For dotted, red/blue at top as dots
                p[1] = new Vector4(10, 100, 0, 1);      // bottomleft
                p[2] = new Vector4(50, 10, 0, 1);       // topright
                p[3] = new Vector4(50, 100, 0, 1);      // botright

                items.Add("ds1", new GLDirect());

                GLRenderControl rts = GLRenderControl.TriStrip();
                GLRenderDataTexture rdt = new GLRenderDataTexture(items.Tex("dotted2"));

                rObjects.Add(items.Shader("ds1"), "ds1", GLRenderableItem.CreateVector4(items, rts, p , rdt));
            }

            #endregion

            #region Index/instance draw

            // multi element index draw
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
                ri.CreateByteIndex(vertex_indices,1);

                items.Add("es1", new GLColourShaderWithWorldCoordXX());
                rObjects.Add(items.Shader("es1"), "es1", ri);
            }

            // multi element index draw with primitive restart, draw a triangle strip
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
                ri.CreateRectangleRestartIndexByte(2,0xff);

                items.Add("es2", new GLColourShaderWithWorldCoordXX());

                rObjects.Add(items.Shader("es2"), "es2", ri);
            }

            // indirect multi draw with element index
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
                ri.CreateRectangleRestartIndexByte(2);  // put the primitive restart markers in, but we won't use them

                ri.IndirectBuffer = new GLBuffer();
                ri.MultiDrawCount = 2;
                ri.IndirectBuffer.Allocate(ri.MultiDrawCountStride * ri.MultiDrawCount + 4);
                IntPtr p = ri.IndirectBuffer.Map(0, ri.IndirectBuffer.BufferSize);
                ri.IndirectBuffer.MapWrite(ref p, 1.0f);        // dummy float to demo index offset
                ri.BaseIndex = 4;       // and indicate that the base command index is 4
                ri.IndirectBuffer.MapWriteIndirectElements(ref p, 4, 1, 0, 0, 0);       // draw indexes 0-3
                ri.IndirectBuffer.MapWriteIndirectElements(ref p, 4, 1, 5, 0, 0);       // and 5-8
                ri.IndirectBuffer.UnMap();
                var data = ri.IndirectBuffer.ReadInts(0,10);                            // notice both are red due to primitive ID=1

                items.Add("es3", new GLColourShaderWithWorldCoordXX());

                rObjects.Add(items.Shader("es3"), "es3", ri);
            }

            #endregion

            #region Bindless texture

            {
                IGLTexture[] btextures = new IGLTexture[2];
                btextures[0] = items.Add("bl1", new GLTexture2D(Properties.Resources.Logo8bpp));
                btextures[1] = items.Add("bl2", new GLTexture2D(Properties.Resources.dotted2));

                GLBindlessTextureHandleBlock bl = new GLBindlessTextureHandleBlock(10,btextures);

                GLStatics.Check();

                float X = -10, Z = -14;
                float X2 = -9, Z2 = -15;
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
                };

                GLRenderControl rts = GLRenderControl.TriStrip(0xff);

                GLRenderableItem ri = GLRenderableItem.CreateFloats(items, rts, v, 3);
                ri.CreateRectangleRestartIndexByte(2);

                items.Add("bt1", new GLBindlessTextureShaderWithWorldCoord());

                rObjects.Add(items.Shader("bt1"), "bt1-1", ri);
            }


            #endregion

            #region Matrix Calc Uniform

            items.Add("MCUB", new GLMatrixCalcUniformBlock());     // def binding of 0

#endregion

            dataoutbuffer = items.NewStorageBlock(5);
            dataoutbuffer.Allocate(sizeof(float) * 4 * 32, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicRead);    // 32 vec4 back

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

            ((GLRenderDataTranslationRotation)(rObjects["woodbox"].RenderData)).XRotDegrees = degrees;
            ((GLRenderDataTranslationRotation)(rObjects["woodbox"].RenderData)).ZRotDegrees = degrees;

            ((GLRenderDataTranslationRotation)(rObjects["EDDCube"].RenderData)).YRotDegrees = degrees;
            ((GLRenderDataTranslationRotation)(rObjects["EDDCube"].RenderData)).ZRotDegrees = degreesd2;

            ((GLRenderDataTranslationRotation)(rObjects["sphere3"].RenderData)).XRotDegrees = -degrees;
            ((GLRenderDataTranslationRotation)(rObjects["sphere3"].RenderData)).YRotDegrees = degrees;
            ((GLRenderDataTranslationRotation)(rObjects["sphere4"].RenderData)).YRotDegrees = degrees;
            ((GLRenderDataTranslationRotation)(rObjects["sphere4"].RenderData)).ZRotDegrees = -degreesd2;
            ((GLRenderDataTranslationRotation)(rObjects["sphere7"].RenderData)).YRotDegrees = degreesd4;

            ((GLPLVertexShaderTextureModelCoordsWithObjectCommonTranslation)items.Shader("TEXOCT").Get(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader)).Transform.YRotDegrees = degrees;
            ((GLPLFragmentShaderTexture2DBlend)items.Shader("TEX2DA").Get(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader)).Blend = zeroone;

            ((GLTexturedShaderTriangleStripWithWorldCoord)items.Shader("tapeshader")).TexOffset = new Vector2(degrees / 360f, 0.0f);
            ((GLTexturedShaderTriangleStripWithWorldCoord)items.Shader("tapeshader2")).TexOffset = new Vector2(-degrees / 360f, 0.0f);

            ((GLTesselationShaderSinewave)items.Shader("TESx1")).Phase = degrees / 360.0f;

            GLStatics.Check();
            GLMatrixCalcUniformBlock mcub = (GLMatrixCalcUniformBlock)items.UB("MCUB");
            mcub.Set(gl3dcontroller.MatrixCalc, glwfc.Width, glwfc.Height);

            rObjects.Render(glwfc.RenderState, gl3dcontroller.MatrixCalc);

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " dir " + gl3dcontroller.Camera.Current + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance;

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
            if ( gl3dcontroller.HandleKeyboardSlews(true, OtherKeys).AnythingChanged)
                gl3dcontroller.Redraw();
            else
            {
                gl3dcontroller.Redraw();
            }
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
        public GLBindlessTextureShaderWithWorldCoord(Action<IGLProgramShader> start = null, Action<IGLProgramShader> finish = null) : base(start, finish)
        {
            AddVertexFragment(new GLPLVertexShaderTextureWorldCoordWithTriangleStripCoord(), new GLPLBindlessFragmentShaderTextureTriangleStrip());
        }
    }



}


