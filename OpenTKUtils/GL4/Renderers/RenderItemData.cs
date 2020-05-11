/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
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
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    // Called per object, by the renderableitem, to bind any data needed to place/rotate the object etc

    // Translation,scaling and rotation of object, placed at 0,0,0, to position.
    // optional Lookat to look at viewer
    // optional texture bind

    public class GLRenderDataTranslationRotation : IGLRenderItemData
    {
        public int LookAtUniform { get; set; } = 21;    
        public int TransformUniform { get; set; } = 22;

        public Vector3 Position { get { return pos; } set { pos = value; Calc(); } }
        public float Scale { get { return scale; } set { scale = value; Calc(); } }
        public void Translate(Vector3 off) { pos += off; Calc(); }

        public Vector3 RotationRadians { get { return rot; } set { rot = value; Calc(); } }
        public Vector3 RotationDegrees { get { return new Vector3(rot.X.Degrees(),rot.Y.Degrees(),rot.Z.Degrees()); } set { rot = new Vector3(value.X.Radians(),value.Y.Radians(),value.Z.Radians()); Calc(); } }
        public float XRotDegrees { get { return rot.X.Degrees(); } set { rot.X = value.Radians(); Calc(); } }
        public float YRotDegrees { get { return rot.Y.Degrees(); } set { rot.Y = value.Radians(); Calc(); } }
        public float ZRotDegrees { get { return rot.Z.Degrees(); } set { rot.Z = value.Radians(); Calc(); } }
        public float XRotRadians { get { return rot.X; } set { rot.X = value; Calc(); } }
        public float YRotRadians { get { return rot.Y; } set { rot.Y = value; Calc(); } }
        public float ZRotRadians { get { return rot.Z; } set { rot.Z = value; Calc(); } }

        public Matrix4 Transform { get { return transform; } }

        private Vector3 pos;
        private Vector3 rot;
        private float scale = 1.0f;

        private Matrix4 transform;

        bool lookatangle = false;

        public GLRenderDataTranslationRotation(float rxradians = 0, float ryradians = 0, float rzradians = 0, float scale = 1.0f, bool calclookat = false)
        {
            pos = new Vector3(0, 0, 0);
            rot = new Vector3(rxradians, ryradians, rzradians);
            this.scale = scale;
            lookatangle = calclookat;
            Calc();
        }

        public GLRenderDataTranslationRotation(Vector3 p, float rxradians = 0, float ryradians = 0, float rzradians = 0, float scale = 1.0f, bool calclookat = false)
        {
            pos = p;
            rot = new Vector3(rxradians, ryradians, rzradians);
            this.scale = scale;
            lookatangle = calclookat;
            Calc();
        }

        public GLRenderDataTranslationRotation(Vector3 p, Vector3 rotpradians, float sc = 1.0f , bool calclookat = false)
        {
            pos = p;
            rot = rotpradians;
            scale = sc;
            lookatangle = calclookat;
            Calc();
        }

        void Calc()
        {
            transform = Matrix4.Identity;
            transform *= Matrix4.CreateScale(scale);
            transform *= Matrix4.CreateRotationX(rot.X);
            transform *= Matrix4.CreateRotationY(rot.Y);
            transform *= Matrix4.CreateRotationZ(rot.Z);
            transform *= Matrix4.CreateTranslation(pos);

          //  System.Diagnostics.Debug.WriteLine("Transform " + transform);
        }

        public virtual void Bind(IGLRenderableItem ri, IGLProgramShader shader, GLMatrixCalc c)
        {
            int sid = shader.Get(ShaderType.VertexShader).Id;
            GL.ProgramUniformMatrix4(sid, TransformUniform, false, ref transform);

            if (lookatangle)
            {
                Vector3 res = pos.AzEl(c.EyePosition, false);
                System.Diagnostics.Debug.WriteLine("Object Bind eye " + c.EyePosition + " to " + pos + " az " + res.Y.Degrees() + " inc " + res.X.Degrees());
                Matrix4 tx2 = Matrix4.Identity;
                tx2 *= Matrix4.CreateRotationX((-res.X));
                tx2 *= Matrix4.CreateRotationY(((float)Math.PI+res.Y));
                GL.ProgramUniformMatrix4(sid, LookAtUniform, false, ref tx2);
            }

            GLStatics.Check();
        }
    }

    // class to use above easily with textures

    public class GLRenderDataTranslationRotationTexture : GLRenderDataTranslationRotation
    {
        public int TextureBind { get; set; } = 1;

        public GLRenderDataTranslationRotationTexture(IGLTexture tex, Vector3 p, float rx = 0, float ry = 0, float rz = 0, float scale = 1.0f) : base(p, rx, ry, rx, scale)
        {
            Texture = tex;
        }

        public GLRenderDataTranslationRotationTexture(IGLTexture tex, Vector3 p, Vector3 rotp, float scale = 1.0f) : base(p, rotp, scale)
        {
            Texture = tex;
        }

        public override void Bind(IGLRenderableItem ri, IGLProgramShader shader, GLMatrixCalc c)
        {
            base.Bind(ri, shader, c);
            Texture.Bind(TextureBind);
            GLStatics.Check();
        }

        private IGLTexture Texture;                      // set to bind texture.
    }

    public class GLRenderDataTranslationRotationColour : GLRenderDataTranslationRotation
    {
        public int ColorBind { get; set; }

        public GLRenderDataTranslationRotationColour(System.Drawing.Color c, Vector3 p, float rx = 0, float ry = 0, float rz = 0, float scale = 1.0f, int uniformbinding = 25) : base(p, rx, ry, rx, scale)
        {
            col = c;
            ColorBind = uniformbinding;
        }

        public GLRenderDataTranslationRotationColour(System.Drawing.Color c, Vector3 p, Vector3 rotp, float scale = 1.0f, int uniformbinding = 25) : base(p, rotp, scale)
        {
            col = c;
            ColorBind = uniformbinding;
        }

        public override void Bind(IGLRenderableItem ri, IGLProgramShader shader, GLMatrixCalc c)
        {
            base.Bind(ri, shader, c);
            GL.ProgramUniform4(shader.Get(ShaderType.FragmentShader).Id,ColorBind, new Vector4((float)col.R / 255.0f, (float)col.G / 255.0f, (float)col.B / 255.0f, (float)col.A / 255.0f));
            GLStatics.Check();
        }

        private System.Drawing.Color col;
    }

    // texture only

    public class GLRenderDataTexture : IGLRenderItemData
    {
        public int TextureBind { get; set; } = 1;

        public GLRenderDataTexture(IGLTexture tex, int bind = 1)
        {
            Texture = tex;
            TextureBind = bind;
        }

        public virtual void Bind(IGLRenderableItem ri, IGLProgramShader shader, GLMatrixCalc c)
        {
            Texture?.Bind(TextureBind);
        }

        private IGLTexture Texture;
    }

    // colour only

    public class GLRenderDataColour : IGLRenderItemData
    {
        public int ColorBind { get; set; }

        public GLRenderDataColour(System.Drawing.Color c, int uniformbinding = 25) 
        {
            col = c;
            ColorBind = uniformbinding;
        }

        public virtual void Bind(IGLRenderableItem ri, IGLProgramShader shader, GLMatrixCalc c)
        {
            GL.ProgramUniform4(shader.Get(ShaderType.FragmentShader).Id, ColorBind, new Vector4((float)col.R / 255.0f, (float)col.G / 255.0f, (float)col.B / 255.0f, (float)col.A / 255.0f));
            GLStatics.Check();
        }

        private System.Drawing.Color col;
    }



}
