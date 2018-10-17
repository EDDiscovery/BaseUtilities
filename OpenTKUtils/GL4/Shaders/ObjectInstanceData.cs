/*
 * Copyright © 2015 - 2018 EDDiscovery development team
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
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    // Called per object, to bind any data needed to place/rotate the object etc

    // Translation and rotation of object, placed at 0,0,0, to position.

    public class GLObjectDataTranslationRotation : IGLObjectInstanceData
    {
        public const int TRUniformId = 22;      // Standard used to pass object data transform to shader

        public Vector3 Position { get { return pos; } set { pos = value; Calc(); } }
        public void Translate(Vector3 off) { pos += off; Calc(); }

        public Vector3 Rotation { get { return rot; } set { rot = value; Calc(); } }
        public float XRotDegrees { get { return rot.X; } set { rot.X = value; Calc(); } }
        public float YRotDegrees { get { return rot.Y; } set { rot.Y = value; Calc(); } }
        public float ZRotDegrees { get { return rot.Z; } set { rot.Z = value; Calc(); } }

        public Matrix4 Transform { get { return transform; } }

        private Vector3 pos;
        Vector3 rot;

        private Matrix4 transform;
        private int uniform;

        public GLObjectDataTranslationRotation(float rx = 0, float ry = 0, float rz = 0, int uniformid = TRUniformId)
        {
            pos = new Vector3(0, 0, 0);
            rot = new Vector3(rx, ry, rz);
            uniform = uniformid;
            Calc();
        }

        public GLObjectDataTranslationRotation(Vector3 p, float rx = 0, float ry = 0, float rz = 0, int uniformid = TRUniformId)
        {
            pos = p;
            rot = new Vector3(rx, ry, rz);
            uniform = uniformid;
            Calc();
        }

        public GLObjectDataTranslationRotation(Vector3 p, Vector3 rotp, int uniformid = TRUniformId)
        {
            pos = p;
            rot = rotp;
            uniform = uniformid;
            Calc();
        }

        void Calc()
        {
            transform = Matrix4.Identity;
            transform *= Matrix4.CreateRotationX((float)(rot.X * Math.PI / 180.0f));
            transform *= Matrix4.CreateRotationY((float)(rot.Y * Math.PI / 180.0f));
            transform *= Matrix4.CreateRotationZ((float)(rot.Z * Math.PI / 180.0f));
            transform *= Matrix4.CreateTranslation(pos);
        }

        public void Bind()
        {
           // System.Diagnostics.Debug.WriteLine("Object Bind " + transform);
            GL.UniformMatrix4(uniform, false, ref transform);
        }
    }

    // version of above, uses less memory by not storing anything but transform

    public class GLObjectDataTranslationRotationSetOnly : IGLObjectInstanceData
    {
        public const int TRUniformId = 22;      // Standard used to pass object data transform to shader

        public GLObjectDataTranslationRotationSetOnly(Vector3 pos, Vector3 rot)
        {
            Set(pos, rot);
        }

        void Set(Vector3 pos, Vector3 rot)
        {
            transform = Matrix4.Identity;
            transform *= Matrix4.CreateRotationX((float)(rot.X * Math.PI / 180.0f));
            transform *= Matrix4.CreateRotationY((float)(rot.Y * Math.PI / 180.0f));
            transform *= Matrix4.CreateRotationZ((float)(rot.Z * Math.PI / 180.0f));
            transform *= Matrix4.CreateTranslation(pos);
        }

        private Matrix4 transform;

        public void Bind()
        {
            //System.Diagnostics.Debug.WriteLine("Object Bind " + transform);
            GL.UniformMatrix4(TRUniformId, false, ref transform);
        }
    }
}
