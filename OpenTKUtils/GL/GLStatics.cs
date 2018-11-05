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

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.CompilerServices;

namespace OpenTKUtils
{
    public static class GLStatics
    {
        static public Vector4 Translate(this Vector4 Vertex, Vector3 offset)
        {
            Vertex.X += offset.X;
            Vertex.Y += offset.Y;
            Vertex.Z += offset.Z;
            return Vertex;
        }

        static public void Translate(this Vector4[] vertices, Vector3 pos)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = vertices[i].Translate(pos);
        }

        static public void Transform(this Vector4[] vertices, Matrix4 trans)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = Vector4.Transform(vertices[i], trans);
        }

        static public Color4 ColorFrom(this Color4[] array, int index)      // helper for color arrays
        {
            index = index % array.Length;
            return array[index];
        }

        public static void RotPos(this Vector4[] vertices, Vector3? rotation = null, Vector3? pos = null)
        {
            if (pos != null)
                vertices.Translate(pos.Value);

            if (rotation != null && rotation.Value.Length > 0)
            {
                Matrix4 transform = Matrix4.Identity;                   // identity nominal matrix, dir is in degrees
                transform *= Matrix4.CreateRotationX((float)(rotation.Value.X * Math.PI / 180.0f));
                transform *= Matrix4.CreateRotationY((float)(rotation.Value.Y * Math.PI / 180.0f));
                transform *= Matrix4.CreateRotationZ((float)(rotation.Value.Z * Math.PI / 180.0f));
                vertices.Transform(transform);
            }
        }

        public static void Check([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            string errmsg = "";

            OpenTK.Graphics.OpenGL4.ErrorCode ec;

            while ( (ec = OpenTK.Graphics.OpenGL4.GL.GetError()) != OpenTK.Graphics.OpenGL4.ErrorCode.NoError )     // call until no error
            {
                if (errmsg.IsEmpty())
                    errmsg += sourceFilePath + ":" + sourceLineNumber + Environment.NewLine;

                errmsg += "Error : " + ec.ToString() + Environment.NewLine;
            }

            if ( errmsg.HasChars() )
                System.Diagnostics.Debug.Assert(false, errmsg );
        }

        static int? LastPatchSize = null;

        public static void PatchSize(int p)          // cache size for speed
        {
            if (LastPatchSize == null || LastPatchSize.Value != p)
            {
                GL.PatchParameter(PatchParameterInt.PatchVertices, p);
                LastPatchSize = p;
            }
        }

        static float? LastPointSize = null;

        public static void PointSize(float p)          // cache size for speed
        {
            if (LastPointSize == null || LastPointSize.Value != p)
            {
                GL.PointSize(p);
                LastPointSize = p;
            }
        }

        static float? LastLineWidth = null;

        public static void LineWidth(float p)          // cache size for speed
        {
            if (LastLineWidth == null || LastLineWidth.Value != p)
            {
                GL.LineWidth(p);
                LastLineWidth = p;
            }
        }

        public static Vector3 AzEl(this Vector3 curpos, Vector3 target, bool degrees)     // az and elevation between curpos and target
        {
            Vector3 delta = Vector3.Subtract(target, curpos);
            //Console.WriteLine("{0}->{1} d {2}", curpos, target, delta);

            float radius = delta.Length;

            if (radius < 0.1)
                return new Vector3(180, 0, 0);     // point forward, level

            float inclination = (float)Math.Acos(delta.Y / radius);
            float azimuth = (float)Math.Atan(delta.Z / delta.X);

            if (degrees)
            {
                inclination = inclination.Degrees();
                azimuth = azimuth.Degrees();
            }

            //System.Diagnostics.Debug.WriteLine("inc " + inclination + " az " + azimuth + " delta" + delta);

            if (delta.X >= 0)      // atan wraps -90 (south)->+90 (north), then -90 to +90 around the y axis, going anticlockwise
                azimuth = 90 - azimuth;     // adjust
            else
                azimuth = -90 - azimuth;

            //System.Diagnostics.Debug.WriteLine(" -> inc " + inclination + " az " + azimuth);
            return new Vector3(inclination, azimuth, 0);
        }


    }
}
