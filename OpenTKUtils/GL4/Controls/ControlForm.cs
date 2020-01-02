/*
 * Copyright © 2019 Robbyxp1 @ github.com
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

using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public class GLForm : GLBaseControl
    {
        public GLForm( OpenTK.GLControl gcp)
        {
            gc = gcp;
            window = new Rectangle(0,0,gcp.Width, gcp.Height);

            vertexes = new GLBuffer();

            vertexarray = new GLVertexArray();
            vertexes.Bind(0, 0, cperv * sizeof(float));             // bind to 0, from 0, 2xfloats. Must bind after vertexarray is made as its bound during construction
            
            vertexarray.Attribute(0, 0, cperv, OpenTK.Graphics.OpenGL4.VertexAttribType.Float); // bind 0 on attr 0, 2 components per vertex

            ri = new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.TriangleStrip, 0, vertexarray);     // create a renderable item
            ri.CreateRectangleRestartIndexByte(255 / 5);

            shader = new GLControlShader();

            textures = new Dictionary<GLBaseControl, GLTexture2D>();
            texturebinds = new GLBindlessTextureHandleBlock(10);
        }

        const int cperv = 2;
        const int maxtoplevel = 16;

        GLBuffer vertexes;
        GLVertexArray vertexarray;
        Dictionary<GLBaseControl, GLTexture2D> textures;
        GLBindlessTextureHandleBlock texturebinds;
        GLRenderableItem ri;
        IGLProgramShader shader;

        public class GLControlShader : GLShaderPipeline
        {
            public GLControlShader()
            {
                AddVertexFragment(new GLPLVertexShaderTextureScreenCoordWithTriangleStripCoord(), new GLPLBindlessFragmentShaderTextureTriangleStrip());
                //AddVertexFragment(new GLPLVertexShaderTextureScreenCoordWithTriangleStripCoord(), new GLPLFragmentShaderFixedColour(Color.Red));
            }
        }

        public override void PerformLayout()
        {
            base.PerformLayout();

            vertexes.Allocate(children.Count * sizeof(float) * cperv * 4);
            IntPtr p = vertexes.Map(0, vertexes.BufferSize);

            List<IGLTexture> tlist = new List<IGLTexture>();

            foreach (var c in children)
            {
                float[] a = new float[] {
                                                c.ClientRectangle.Left, c.ClientRectangle.Top,
                                                c.ClientRectangle.Left, c.ClientRectangle.Bottom ,
                                                c.ClientRectangle.Right, c.ClientRectangle.Top,
                                                c.ClientRectangle.Right, c.ClientRectangle.Bottom , 
                                            };

                vertexes.MapWrite(ref p, a);

                if ( !textures.ContainsKey(c))      // if we don't have a texture for it..
                {
                    textures[c] = new GLTexture2D();
                    textures[c].CreateTexture(c.Width, c.Height);   // and make a texture
                }
                else if ( textures[c].Width != c.GetBitmap().Width || textures[c].Height != c.GetBitmap().Height )      // if layout changed bitmap
                {
                    textures[c].CreateTexture(c.Width, c.Height);   // and make a texture, this will dispose of the old one 
                }

                tlist.Add(textures[c]);     // need to have them in the same order as the client rectangles, and the dictionary does not guarantee this
            }

            ri.DrawCount = children.Count * 5 - 1;    // 4 vertexes per rectangle, 1 restart

            texturebinds.WriteHandles(tlist.ToArray()); // write texture handles to the buffer..

            vertexes.UnMap();

            float[] d = vertexes.ReadFloats(0, children.Count * 4 * cperv);
        }

        // tbd clean up textures[c] on removal of control?

        public void Render(Common.MatrixCalc mc)
        {
            foreach (var c in children)
            {
                if (c.NeedRedraw)
                {
                    c.Redraw(null, new Rectangle(0, 0, 0, 0));      // repaint
                    textures[c].LoadBitmap(c.GetBitmap());  // and update texture unit with new bitmap
                }
            }

            shader.Start();
            OpenTKUtils.GLStatics.Check();
            ri.Bind(shader, mc);                    // binds VA AND the element buffer
            GLStatics.PrimitiveRestart(true, 0xff);
            ri.Render();                            // draw using primitive restart on element index buffer with bindless textures
            shader.Finish();
            GL.UseProgram(0);           // final clean up
            GL.BindProgramPipeline(0);
            OpenTKUtils.GLStatics.Check();
        }


        private OpenTK.GLControl gc;

    }
}
