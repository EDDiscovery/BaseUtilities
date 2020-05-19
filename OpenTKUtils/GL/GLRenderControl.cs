/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
 * 
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

using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils
{
    // Wraps the openGL main state variables in a class so they get selected correctly for each render.

    public class GLRenderControl 
    {
        static public GLRenderControl Tri(FrontFaceDirection frontface = FrontFaceDirection.Ccw, bool cullface = true, PolygonMode polygonmode = PolygonMode.Fill)
        { return new GLRenderControl(PrimitiveType.Triangles) { FrontFace = frontface, CullFace = cullface, PolygonModeFrontAndBack = polygonmode }; }

        static public GLRenderControl TriStrip(FrontFaceDirection frontface = FrontFaceDirection.Ccw, bool cullface = true, PolygonMode polygonmode = PolygonMode.Fill)
        { return new GLRenderControl(PrimitiveType.TriangleStrip) { FrontFace = frontface, CullFace = cullface, PolygonModeFrontAndBack = polygonmode }; }

        static public GLRenderControl TriStrip(uint primitiverestart, FrontFaceDirection frontface = FrontFaceDirection.Ccw, bool cullface = true, PolygonMode polygonmode = PolygonMode.Fill)
        { return new GLRenderControl(PrimitiveType.TriangleStrip) { PrimitiveRestart = primitiverestart, FrontFace = frontface, CullFace = cullface, PolygonModeFrontAndBack = polygonmode }; }

        static public GLRenderControl TriStrip(DrawElementsType primitiverestarttype, FrontFaceDirection frontface = FrontFaceDirection.Ccw, bool cullface = true, PolygonMode polygonmode = PolygonMode.Fill)
        { return new GLRenderControl(PrimitiveType.TriangleStrip) { PrimitiveRestart = GL4Statics.DrawElementsRestartValue(primitiverestarttype), FrontFace = frontface, CullFace = cullface, PolygonModeFrontAndBack = polygonmode }; }

        static public GLRenderControl TriFan(FrontFaceDirection frontface = FrontFaceDirection.Ccw, bool cullface = true, PolygonMode polygonmode = PolygonMode.Fill)
        { return new GLRenderControl(PrimitiveType.TriangleFan) { FrontFace = frontface, CullFace = cullface, PolygonModeFrontAndBack = polygonmode }; }

        static public GLRenderControl TriFan(DrawElementsType primitiverestarttype, FrontFaceDirection frontface = FrontFaceDirection.Ccw, bool cullface = true, PolygonMode polygonmode = PolygonMode.Fill)
        { return new GLRenderControl(PrimitiveType.TriangleFan) { PrimitiveRestart = GL4Statics.DrawElementsRestartValue(primitiverestarttype), FrontFace = frontface, CullFace = cullface, PolygonModeFrontAndBack = polygonmode }; }

        static public GLRenderControl Quads(FrontFaceDirection frontface = FrontFaceDirection.Ccw, bool cullface = true, PolygonMode polygonmode = PolygonMode.Fill)
        { return new GLRenderControl(PrimitiveType.Quads) { FrontFace = frontface, CullFace = cullface, PolygonModeFrontAndBack = polygonmode }; }

        static public GLRenderControl Points(float pointsize = 1, bool smooth = true)
        { return new GLRenderControl(PrimitiveType.Points) { PointSize = pointsize, PointSprite = false , PointSmooth = smooth }; }

        static public GLRenderControl PointsByProgram(bool pointsprite = false, bool smooth = true)
        { return new GLRenderControl(PrimitiveType.Points) { PointSize = 0, PointSprite = false, PointSmooth = smooth }; }

        static public GLRenderControl PointSprites( bool? depthtest = null)
        { return new GLRenderControl(PrimitiveType.Points) { PointSize = 0, PointSprite = true, DepthTest = depthtest }; }

        static public GLRenderControl Patches(int patchsize = 4 , FrontFaceDirection frontface = FrontFaceDirection.Ccw, bool cullface = true, PolygonMode polygonmode = PolygonMode.Fill)
        { return new GLRenderControl(PrimitiveType.Patches) { PatchSize = patchsize , FrontFace = frontface, CullFace = cullface, PolygonModeFrontAndBack = polygonmode }; }

        static public GLRenderControl Lines(float linewidth = 1, bool smooth = true)        // vertex 0/1 line, 2/3 line
        { return new GLRenderControl(PrimitiveType.Lines) { LineWidth = linewidth, LineSmooth = smooth }; }

        static public GLRenderControl LineLoop(float linewidth = 1, bool smooth = true)     // vertex 0->1->2->0
        { return new GLRenderControl(PrimitiveType.LineLoop) { LineWidth = linewidth, LineSmooth = smooth }; }

        static public GLRenderControl LineStrip(float linewidth = 1, bool smooth = true)    // vertex 0->1->2
        { return new GLRenderControl(PrimitiveType.LineStrip) { LineWidth = linewidth, LineSmooth = smooth };        }

        // geoshaders which change the primitive type need the values for the output, but a different input type
        // good for any triangle type at the geoshader output

        static public GLRenderControl ToTri(PrimitiveType t, FrontFaceDirection frontface = FrontFaceDirection.Ccw, bool cullface = true, PolygonMode polygonmode = PolygonMode.Fill)
        { return new GLRenderControl(t) { FrontFace = frontface, CullFace = cullface, PolygonModeFrontAndBack = polygonmode }; }


        static public GLRenderControl Default()
        {
            return new GLRenderControl(PrimitiveType.Points)        // we only need to update the ones which are null by default.
            {
                FrontFace = FrontFaceDirection.Ccw,
                CullFace = true,
                PolygonModeFrontAndBack = PolygonMode.Fill,
                PatchSize = 1,
                PointSize = 1,
                PointSprite = false,
                PointSmooth = true,
                LineWidth = 1,
                LineSmooth = true,
                PrimitiveRestart = null,
            };
        }

        static public GLRenderControl AtGLStart()       // used at startup, everything needs to be null so we apply all
        {
            GL.Disable(EnableCap.PrimitiveRestart);     // disable by default, since PrimitiveRestart null means disabled, we must explicitly disable

            return new GLRenderControl(PrimitiveType.Points)        
            {
                DepthTest = null,
                DepthClamp = null,
                BlendEnable = null,
                BlendSource = null,
                BlendDest = null,
            };
        }

        public void ApplyState( GLRenderControl newstate )      // apply deltas to GL
        {
            // general

            if (newstate.DepthTest.HasValue && DepthTest != newstate.DepthTest)
            {
                DepthTest = newstate.DepthTest;
                GLStatics.SetEnable(OpenTK.Graphics.OpenGL.EnableCap.DepthTest, DepthTest.Value);
             //   System.Diagnostics.Debug.WriteLine("Depth Test " + DepthTest.Value);
            }

            if (newstate.DepthClamp.HasValue && DepthClamp != newstate.DepthClamp)
            {
                DepthClamp = newstate.DepthClamp;
                GLStatics.SetEnable(OpenTK.Graphics.OpenGL.EnableCap.DepthClamp, DepthClamp.Value);
                // System.Diagnostics.Debug.WriteLine("Depth Clamp" + DepthClamp.Value);
            }

            if (newstate.PrimitiveRestart != PrimitiveRestart)
            {
                //System.Diagnostics.Debug.WriteLine("Set PR to {0}", newstate.PrimitiveRestart);
                if (newstate.PrimitiveRestart.HasValue)         // is new state has value
                {
                    if (PrimitiveRestart == null )              // if last was off, turn it on
                        GL.Enable(EnableCap.PrimitiveRestart);

                    GL.PrimitiveRestartIndex(newstate.PrimitiveRestart.Value);  // set
                }
                else
                    GL.Disable(EnableCap.PrimitiveRestart);     // else disable

                PrimitiveRestart = newstate.PrimitiveRestart;
            }

            if (newstate.BlendEnable.HasValue && BlendEnable != newstate.BlendEnable)
            {
                BlendEnable = newstate.BlendEnable;
                GLStatics.SetEnable(OpenTK.Graphics.OpenGL.EnableCap.Blend, BlendEnable.Value);
            }

            if (BlendEnable == true && newstate.BlendSource.HasValue && newstate.BlendDest.HasValue &&  // blend on, we have new values for blend equation
                    (BlendSource != newstate.BlendSource || BlendDest != newstate.BlendDest))
            {
                BlendSource = newstate.BlendSource;
                BlendDest = newstate.BlendDest;
                GL.BlendFunc(BlendSource.Value, BlendDest.Value);
            }

            if ( ClipDistanceEnable != newstate.ClipDistanceEnable )        // if changed
            {
                if ( newstate.ClipDistanceEnable>ClipDistanceEnable)
                {
                    for (int i = ClipDistanceEnable; i < newstate.ClipDistanceEnable; i++)
                        GL.Enable(EnableCap.ClipDistance0 + i);
                }
                else if ( newstate.ClipDistanceEnable < ClipDistanceEnable )
                {
                    for (int i = ClipDistanceEnable - 1; i >= newstate.ClipDistanceEnable; i--)
                        GL.Disable(EnableCap.ClipDistance0 + i);
                }

                ClipDistanceEnable = newstate.ClipDistanceEnable;
            }

            // patches

            if (newstate.PatchSize.HasValue && PatchSize != newstate.PatchSize )
            {
                PatchSize = newstate.PatchSize;
                GL.PatchParameter(PatchParameterInt.PatchVertices, PatchSize.Value);
            }

            // points

            if (newstate.PointSize.HasValue && PointSize != newstate.PointSize )
            {
                if ( newstate.PointSize>0 )     // if fixed point size
                {
                    if ( PointSize == null || newstate.PointSize != PointSize.Value )
                        GL.PointSize(newstate.PointSize.Value); // set if different

                    if ( PointSize == null || PointSize == 0 )       // if previous was off
                        GL.Disable(EnableCap.ProgramPointSize);
                }
                else
                    GL.Enable(EnableCap.ProgramPointSize);

                PointSize = newstate.PointSize;
            }

            if (newstate.PointSprite.HasValue && PointSprite != newstate.PointSprite )
            {
                PointSprite = newstate.PointSprite;
                GLStatics.SetEnable(OpenTK.Graphics.OpenGL.EnableCap.PointSprite, PointSprite.Value);
            }

            if (newstate.PointSmooth.HasValue && PointSmooth != newstate.PointSmooth)
            {
                PointSmooth = newstate.PointSmooth;
                GLStatics.SetEnable(OpenTK.Graphics.OpenGL.EnableCap.PointSmooth, PointSmooth.Value);
            }

            // lines

            if (newstate.LineWidth.HasValue && LineWidth != newstate.LineWidth)
            {
                LineWidth = newstate.LineWidth;
                GL.LineWidth(LineWidth.Value);
            }

            if (newstate.LineSmooth.HasValue && LineSmooth != newstate.LineSmooth)
            {
                LineSmooth = newstate.LineSmooth;
                GLStatics.SetEnable(OpenTK.Graphics.OpenGL.EnableCap.LineSmooth, LineSmooth.Value);
            }

            // triangles

            if ( newstate.PolygonModeFrontAndBack.HasValue && PolygonModeFrontAndBack != newstate.PolygonModeFrontAndBack)
            {
                PolygonModeFrontAndBack = newstate.PolygonModeFrontAndBack;
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonModeFrontAndBack.Value);
            }

            if (newstate.CullFace.HasValue && CullFace != newstate.CullFace)
            {
                CullFace = newstate.CullFace;
                GLStatics.SetEnable(OpenTK.Graphics.OpenGL.EnableCap.CullFace, CullFace.Value);
                // System.Diagnostics.Debug.WriteLine("Cull mode " + CullFace.Value);
            }

            if (newstate.FrontFace.HasValue && FrontFace != newstate.FrontFace)
            {
                FrontFace = newstate.FrontFace;
                GL.FrontFace(FrontFace.Value);
            }
        }


        private GLRenderControl(PrimitiveType p)
        {
            PrimitiveType = p;
        }

        public PrimitiveType PrimitiveType { get; set; }             // Draw type for front end - may not be draw type after geo shader note

        // these are only set for particular primitive types - so the default construction is don't care.

        public FrontFaceDirection? FrontFace { get;  set;} = null;   // triangles/quads
        public bool? CullFace { get;  set;} = null;                  // triangles/quads
        public PolygonMode? PolygonModeFrontAndBack { get; set; } = null;        // triangles/quads
        public int? PatchSize { get; set; } = null;                  // patches
        public float? PointSize { get; set; } = null;                // points
        public bool? PointSprite { get; set; } = null;               // points
        public bool? PointSmooth { get; set; } = null;               // points
        public float? LineWidth { get;  set;} = null;                // lines
        public bool? LineSmooth { get; set; } = null;                // lines

        // these affect all types so are configured to their defaults for normal drawing

        public uint? PrimitiveRestart { get; set; } = null;          // its either null (disabled) or value (enabled). null does not mean don't care.
        public int ClipDistanceEnable { get; set; } = 0;           // set for number of clip/cull distances to enable. 0 means none. Null means not configured

        // for the following, null is used for not configured (applicable at start up)

        public bool? DepthTest { get; set; } = true;                
        public bool? DepthClamp { get; set; } = false;              
        public bool? BlendEnable { get;  set; } = true;             
        public BlendingFactor? BlendSource { get;  set;} = BlendingFactor.SrcAlpha;     
        public BlendingFactor? BlendDest { get;  set;} = BlendingFactor.OneMinusSrcAlpha;
    }

}

// keep, later idea

//foreach( var x in list)
//{
//    int deltas = x.Item2.RenderControl.Deltas(r.RenderControl);
//    System.Diagnostics.Debug.WriteLine("Render list " + x.Item1 + " delta to " + name + " = " + deltas);
//}




//public int Deltas(GLRenderControl newstate)
//{
//    // general
//    int count = (newstate.FrontFace.HasValue && FrontFace.HasValue && FrontFace != newstate.FrontFace) ? 1 : 0;       // keep synced with conditions below
//    count += (newstate.CullFace.HasValue && CullFace.HasValue && CullFace != newstate.CullFace) ? 1 : 0;
//    count += (newstate.DepthTest.HasValue && DepthTest.HasValue && DepthTest != newstate.DepthTest) ? 1 : 0;
//    count += (newstate.DepthClamp.HasValue && DepthClamp.HasValue && DepthClamp != newstate.DepthClamp) ? 1 : 0;
//    count += (newstate.BlendEnable.HasValue && BlendEnable.HasValue && BlendEnable != newstate.BlendEnable) ? 1 : 0;
//    count += (newstate.BlendSource.HasValue && newstate.BlendDest.HasValue && BlendSource.HasValue && BlendDest.HasValue &&
//            (BlendSource != newstate.BlendSource || BlendDest != newstate.BlendDest)) ? 1 : 0;
//    count += (newstate.PrimitiveRestart != newstate.PrimitiveRestart) ? 1 : 0;
//    count += (newstate.RasterizerDiscard != newstate.RasterizerDiscard) ? 1 : 0;

//    // patches
//    count += (newstate.PatchSize.HasValue && PatchSize.HasValue && PatchSize != newstate.PatchSize) ? 1 : 0;

//    // points
//    count += (newstate.PointSize.HasValue && PointSize.HasValue && PointSize != newstate.PointSize) ? 1 : 0;
//    count += (newstate.PointSprite.HasValue && PointSprite.HasValue && PointSprite != newstate.PointSprite) ? 1 : 0;

//    // lines
//    count += (newstate.LineWidth.HasValue && LineWidth.HasValue && LineWidth != newstate.LineWidth) ? 1 : 0;
//    count += (newstate.LineSmooth.HasValue && LineSmooth.HasValue && LineSmooth != newstate.LineSmooth) ? 1 : 0;

//    // triangles
//    count += (newstate.PolygonModeFrontAndBack.HasValue && PolygonModeFrontAndBack.HasValue && PolygonModeFrontAndBack != newstate.PolygonModeFrontAndBack) ? 1 : 0;
//    return count;
//}


