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
        public bool RequestRender { get; set; } = false;

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

            gc.MouseMove += Gc_MouseMove;
            gc.MouseClick += Gc_MouseClick;
            gc.MouseDown += Gc_MouseDown;
            gc.MouseUp += Gc_MouseUp;
            gc.MouseEnter += Gc_MouseEnter;
            gc.MouseLeave += Gc_MouseLeave;
        }

        const int cperv = 2;
        const int maxtoplevel = 16;

        GLBuffer vertexes;
        GLVertexArray vertexarray;
        Dictionary<GLBaseControl, GLTexture2D> textures;
        GLBindlessTextureHandleBlock texturebinds;
        GLRenderableItem ri;
        IGLProgramShader shader;
        private OpenTK.GLControl gc;

        public class GLControlShader : GLShaderPipeline
        {
            public GLControlShader()
            {
                AddVertexFragment(new GLPLVertexShaderTextureScreenCoordWithTriangleStripCoord(), new GLPLBindlessFragmentShaderTextureTriangleStrip());
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

            RequestRender = true;

            OpenTKUtils.GLStatics.Check();
            //float[] d = vertexes.ReadFloats(0, children.Count * 4 * cperv);
        }

        // tbd clean up textures[c] on removal of control?

        public void Render(Common.MatrixCalc mc)
        {
            foreach (var c in children)
            {
                bool redrawn = c.Redraw(null, new Rectangle(0, 0, 0, 0), null , false);      // see if redraw done

                if (redrawn)
                {
                    textures[c].LoadBitmap(c.GetBitmap());  // and update texture unit with new bitmap
                }
            }

            shader.Start();
            ri.Bind(shader, mc);                    // binds VA AND the element buffer
            GLStatics.PrimitiveRestart(true, 0xff);
            ri.Render();                            // draw using primitive restart on element index buffer with bindless textures
            shader.Finish();
            GL.UseProgram(0);           // final clean up
            GL.BindProgramPipeline(0);

            RequestRender = false;
        }

        GLBaseControl currentmouseover = null;
        Point currentmouseoverlocation;

        private void Gc_MouseLeave(object sender, EventArgs e)
        {
            if (currentmouseover != null)
            {
                currentmouseover.MouseButtonDown = currentmouseover.Hover = false;
                currentmouseover.OnMouseLeave(new MouseEventArgs(new Point(0, 0)));
                currentmouseover = null;
            }
        }

        private Point FindCursorFormCoords()
        {
            BaseUtils.Win32.UnsafeNativeMethods.GetCursorPos(out BaseUtils.Win32.UnsafeNativeMethods.POINT p);
            Point gcsp = gc.PointToScreen(new Point(0, 0));
            return new Point(p.X - gcsp.X, p.Y - gcsp.Y);
        }

        private void Gc_MouseEnter(object sender, EventArgs e)
        {
            if (currentmouseover != null)
            {
                currentmouseover.Hover = currentmouseover.MouseButtonDown = false;
                currentmouseover.OnMouseLeave(new MouseEventArgs(new Point(0, 0)));
                currentmouseover = null;
            }

            Point relcurpos = FindCursorFormCoords();

            currentmouseover = FindControlOver(relcurpos);

            if (currentmouseover != null)
            {
                currentmouseoverlocation = currentmouseover.FormCoords();
                System.Diagnostics.Debug.WriteLine("Set mouse over loc " + currentmouseoverlocation);
                currentmouseover.Hover = true;
                currentmouseover?.OnMouseEnter(new MouseEventArgs(new Point(relcurpos.X - currentmouseoverlocation.X, relcurpos.Y - currentmouseoverlocation.Y)));
            }
        }

        private void Gc_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            currentmouseover.MouseButtonDown = false;
            currentmouseover?.OnMouseUp(new MouseEventArgs((int)e.Button, new Point(e.X - currentmouseoverlocation.X, e.Y - currentmouseoverlocation.Y), e.Clicks));
        }

        private void Gc_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            currentmouseover.MouseButtonDown = true;
            currentmouseover?.OnMouseDown(new MouseEventArgs((int)e.Button, new Point(e.X - currentmouseoverlocation.X, e.Y - currentmouseoverlocation.Y), e.Clicks));
        }

        private void Gc_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            currentmouseover?.OnMouseClick(new MouseEventArgs((int)e.Button, new Point(e.X - currentmouseoverlocation.X, e.Y - currentmouseoverlocation.Y), e.Clicks));
        }

        private void Gc_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            GLBaseControl c = FindControlOver(e.Location);      // e.location are form co-ords

            if (c != currentmouseover )
            {
                if (currentmouseover != null)
                {
                    if ( currentmouseover.MouseButtonDown )   // click and drag, can't change control while mouse is down
                    {
                        currentmouseover.OnMouseMove(new MouseEventArgs(new Point(e.X - currentmouseoverlocation.X, e.Y - currentmouseoverlocation.Y)));
                        return;
                    }

                    currentmouseover.Hover = false;
                    currentmouseover.OnMouseLeave(new MouseEventArgs(new Point(0, 0)));
                }

                currentmouseover = c;

                if (currentmouseover != null)
                {
                    currentmouseover.Hover = true;
                    currentmouseoverlocation = currentmouseover.FormCoords();
                    System.Diagnostics.Debug.WriteLine("2Set mouse over loc " + currentmouseoverlocation);
                    currentmouseover?.OnMouseEnter(new MouseEventArgs(new Point(e.X - currentmouseoverlocation.X, e.Y - currentmouseoverlocation.Y)));
                }
            }

            if ( currentmouseover != null )
            {
                currentmouseover?.OnMouseMove(new MouseEventArgs(new Point(e.X - currentmouseoverlocation.X, e.Y - currentmouseoverlocation.Y)));
            }
        }

    }
}
