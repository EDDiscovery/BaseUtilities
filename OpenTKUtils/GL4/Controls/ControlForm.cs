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
    // GL form is a base control, implementing the functions of GLWindowControl 

    public class GLForm : GLBaseControl, GLWindowControl
    {
        public bool RequestRender { get; set; } = false;

        public override bool Focused { get { return glwin.Focused; } }          // override focused to report if whole window is focused.

        // from Control, override the Mouse* and Key* events

        public new Action<Object> Paint { get; set; } = null;                   //override to get a paint event

        public GLForm(GLWindowControl win)
        {
            glwin = win;
            window = new Rectangle(0, 0, glwin.Width, glwin.Height);

            vertexes = new GLBuffer();

            vertexarray = new GLVertexArray();
            vertexes.Bind(0, 0, vertexesperentry * sizeof(float));             // bind to 0, from 0, 2xfloats. Must bind after vertexarray is made as its bound during construction

            vertexarray.Attribute(0, 0, vertexesperentry, OpenTK.Graphics.OpenGL4.VertexAttribType.Float); // bind 0 on attr 0, 2 components per vertex

            ri = new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.TriangleStrip, 0, vertexarray);     // create a renderable item
            ri.CreateRectangleRestartIndexByte(255 / 5);

            shader = new GLControlShader();

            textures = new Dictionary<GLBaseControl, GLTexture2D>();
            texturebinds = new GLBindlessTextureHandleBlock(10);

            glwin.MouseMove += Gc_MouseMove;
            glwin.MouseClick += Gc_MouseClick;
            glwin.MouseDown += Gc_MouseDown;
            glwin.MouseUp += Gc_MouseUp;
            glwin.MouseEnter += Gc_MouseEnter;
            glwin.MouseLeave += Gc_MouseLeave;
            glwin.MouseWheel += Gc_MouseWheel;
            glwin.KeyDown += Gc_KeyDown;
            glwin.KeyUp += Gc_KeyUp;
            glwin.KeyPress += Gc_KeyPress;
            glwin.Resize += Gc_Resize;
            glwin.Paint += Gc_Paint;

            Font = new Font("Microsoft Sans Serif", 8.25f);
        }

        public override void Add(GLBaseControl other)           // we need to override, since we want controls added to the scroll panel not us
        {
            textures[other] = new GLTexture2D();                // we make a texture per top level control to render with
            base.Add(other);
        }

        public override void Remove(GLBaseControl other)
        {
            base.Remove(other);
            textures[other].Dispose();
            textures.Remove(other);
        }

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

            vertexes.Allocate(children.Count * sizeof(float) * vertexesperentry * 4);
            IntPtr p = vertexes.Map(0, vertexes.BufferSize);

            List<IGLTexture> tlist = new List<IGLTexture>();

            foreach (var c in children)
            {
                float z = 0f;
                float[] a = new float[] {
                                                c.ClientRectangle.Left, c.ClientRectangle.Top, z, 1,
                                                c.ClientRectangle.Left, c.ClientRectangle.Bottom , z, 1,
                                                c.ClientRectangle.Right, c.ClientRectangle.Top, z, 1,
                                                c.ClientRectangle.Right, c.ClientRectangle.Bottom , z, 1,
                                            };

                vertexes.MapWrite(ref p, a);

                if (textures[c].Id == -1 || textures[c].Width != c.GetLevelBitmap.Width || textures[c].Height != c.GetLevelBitmap.Height)      // if layout changed bitmap
                {
                    textures[c].CreateOrUpdateTexture(c.Width, c.Height);   // and make a texture, this will dispose of the old one 
                }

                tlist.Add(textures[c]);     // need to have them in the same order as the client rectangles, and the dictionary does not guarantee this
            }

            vertexes.UnMap();
            OpenTKUtils.GLStatics.Check();

            ri.DrawCount = children.Count * 5 - 1;    // 4 vertexes per rectangle, 1 restart

            texturebinds.WriteHandles(tlist.ToArray()); // write texture handles to the buffer..

            RequestRender = true;

            //float[] d = vertexes.ReadFloats(0, children.Count * 4 * cperv);
        }

        // call this during your Paint to render

        public void Render()
        {
            //System.Diagnostics.Debug.WriteLine("Form redraw start");
            //DebugWhoWantsRedraw();

            LinkedListNode<GLBaseControl> pos = children.Last;      // render in order from last z to first z.
            while (pos != null)
            {
                var c = pos.Value;

                if (c.Visible)
                {
                    bool redrawn = c.Redraw(null, new Rectangle(0, 0, 0, 0), new Rectangle(0, 0, 0, 0), null, false);      // see if redraw done

                    if (redrawn)
                    {
                        textures[c].LoadBitmap(c.GetLevelBitmap);  // and update texture unit with new bitmap
                        //float[] p = textures[c].GetTextureImageAsFloats(end:100);
                    }
                }

                pos = pos.Previous;
            }

            NeedRedraw = false;

            shader.Start();
            ri.Bind(shader, null);                    // binds VA AND the element buffer
            GLStatics.PrimitiveRestart(true, 0xff);
            ri.Render();                            // draw using primitive restart on element index buffer with bindless textures
            shader.Finish();
            GL.UseProgram(0);           // final clean up
            GL.BindProgramPipeline(0);

            RequestRender = false;

            //System.Diagnostics.Debug.WriteLine("Form redraw end");
        }

        // tbd clean up textures[c] on removal of control

        private void Gc_MouseLeave(object sender, MouseEventArgs e)
        {
            if (currentmouseover != null)
            {
                currentmouseover.MouseButtonsDown = MouseEventArgs.MouseButtons.None;
                currentmouseover.Hover = false;
                currentmouseover.OnMouseLeave(new MouseEventArgs(e.Location));
                currentmouseover = null;
            }
        }

        private void Gc_MouseEnter(object sender, MouseEventArgs e)
        {
            if (currentmouseover != null)
            {
                currentmouseover.MouseButtonsDown = MouseEventArgs.MouseButtons.None;
                currentmouseover.Hover = false;
                if (currentmouseover.Enabled)
                    currentmouseover.OnMouseLeave(new MouseEventArgs(e.Location));
                currentmouseover = null;
            }

            currentmouseover = FindControlOver(e.Location);

            if (currentmouseover != null)
            {
                currentmouseoverlocation = currentmouseover.FormCoords();
                //System.Diagnostics.Debug.WriteLine("Set mouse over loc " + currentmouseoverlocation);
                currentmouseover.Hover = true;
                if (currentmouseover.Enabled)
                {
                    e.Location = new Point(e.Location.X - currentmouseoverlocation.X, e.Location.Y - currentmouseoverlocation.Y);
                    currentmouseover.OnMouseEnter(e);
                }
            }
        }

        private void Gc_MouseUp(object sender, MouseEventArgs e)
        {
            currentmouseover.MouseButtonsDown = MouseEventArgs.MouseButtons.None;

            if (currentmouseover.Enabled)
            {
                e.Location = new Point(e.Location.X - currentmouseoverlocation.X, e.Location.Y - currentmouseoverlocation.Y);
                currentmouseover.OnMouseUp(e);
            }
        }

        private void Gc_MouseDown(object sender, MouseEventArgs e)
        {
            var x = e.Button;
            if (currentmouseover.Enabled)
            {
                currentmouseover.MouseButtonsDown = e.Button;
                e.Location = new Point(e.Location.X - currentmouseoverlocation.X, e.Location.Y - currentmouseoverlocation.Y);
                currentmouseover.OnMouseDown(e);
            }
        }

        private void Gc_MouseClick(object sender, MouseEventArgs e)
        {
            if (currentmouseover.Enabled)
            {
                if ( currentfocus != currentmouseover)
                {
                    if (currentfocus != null)
                    {
                        currentfocus.Focused = false;
                        currentfocus.OnFocusChanged(false);
                        if (currentfocus.InvalidateOnFocusChange)
                            currentfocus.Invalidate();
                        currentfocus = null;
                    }

                    if (currentmouseover.Focusable)
                    {
                        currentfocus = currentmouseover;
                        currentfocus.Focused = true;
                        currentfocus.OnFocusChanged(true);
                        if (currentfocus.InvalidateOnFocusChange)
                            currentfocus.Invalidate();
                    }
                }

                e.Location = new Point(e.Location.X - currentmouseoverlocation.X, e.Location.Y - currentmouseoverlocation.Y);
                currentmouseover.OnMouseClick(e);
            }
            else if ( currentfocus != null )
            {
                currentfocus.Focused = false;
                currentfocus.OnFocusChanged(false);
                if (currentfocus.InvalidateOnFocusChange)
                    currentfocus.Invalidate();
                currentfocus = null;
            }
        }

        private void Gc_MouseWheel(object sender, MouseEventArgs e)
        {
            if (currentmouseover.Enabled)
            {
                e.Location = new Point(e.Location.X - currentmouseoverlocation.X, e.Location.Y - currentmouseoverlocation.Y);
                currentmouseover.OnMouseWheel(e);
            }
        }

        private void Gc_MouseMove(object sender, MouseEventArgs e)
        {
            GLBaseControl c = FindControlOver(e.Location);      // e.location are form co-ords
            System.Diagnostics.Debug.Assert(c != null);     // always returns something, even if its the form

            if (c != currentmouseover)
            {
                if (currentmouseover != null)
                {
                    if (currentmouseover.MouseButtonsDown != MouseEventArgs.MouseButtons.None)   // click and drag, can't change control while mouse is down
                    {
                        if (currentmouseover.Enabled)
                        {
                            e.Location = new Point(e.Location.X - currentmouseoverlocation.X, e.Location.Y - currentmouseoverlocation.Y);
                            currentmouseover.OnMouseMove(e);
                        }
                        return;
                    }

                    currentmouseover.Hover = false;

                    if (currentmouseover.Enabled)
                    {
                        currentmouseover.OnMouseLeave(e);
                    }
                }

                currentmouseover = c;

                if (currentmouseover != null)
                {
                    currentmouseover.Hover = true;
                    currentmouseoverlocation = currentmouseover.FormCoords();
                    //System.Diagnostics.Debug.WriteLine("2Set mouse over loc " + currentmouseoverlocation);

                    if (currentmouseover.Enabled)
                    {
                        e.Location = new Point(e.Location.X - currentmouseoverlocation.X, e.Location.Y - currentmouseoverlocation.Y);
                        currentmouseover.OnMouseEnter(e);
                    }
                }
            }
            else
            {
                if (currentmouseover.Enabled)
                {
                    e.Location = new Point(e.Location.X - currentmouseoverlocation.X, e.Location.Y - currentmouseoverlocation.Y);
                    //System.Diagnostics.Debug.WriteLine("Mouse " + e.Location);
                    currentmouseover.OnMouseMove(e);
                }
            }
        }

        private void Gc_KeyUp(object sender, KeyEventArgs e)
        {
            if (currentfocus != null && currentfocus.Enabled)
            {
                currentfocus.OnKeyUp(e);
            }
        }

        private void Gc_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentfocus != null && currentfocus.Enabled)
            {
                currentfocus.OnKeyDown(e);
            }
        }

        private void Gc_KeyPress(object sender, KeyEventArgs e)
        {
            if (currentfocus != null && currentfocus.Enabled)
            {
                currentfocus.OnKeyPress(e);
            }
        }

        private void Gc_Resize(object sender)
        {
            Resize?.Invoke(sender);
        }

        private void Gc_Paint(object sender)
        {
            Paint?.Invoke(sender);
        }

        const int vertexesperentry = 4;
        private GLWindowControl glwin;
        private GLBuffer vertexes;
        private GLVertexArray vertexarray;
        private Dictionary<GLBaseControl, GLTexture2D> textures;
        private GLBindlessTextureHandleBlock texturebinds;
        private GLRenderableItem ri;
        private IGLProgramShader shader;
        private GLBaseControl currentmouseover = null;
        private Point currentmouseoverlocation;
        private GLBaseControl currentfocus = null;


    }
}
