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
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OpenTKUtils
{
    public static class GLStatics
    {
        #region Static helpers

        static public Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Math.Abs(v.X), Math.Abs(v.Y), Math.Abs(v.Z));
        }

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

        static public void Transform(ref Vector4[] vertices, Matrix4 trans)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = Vector4.Transform(vertices[i], trans);
        }

        static public Vector4[] Transform(this Vector4[] vertices, Matrix4 trans)
        {
            Vector4[] res = new Vector4[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                res[i] = Vector4.Transform(vertices[i], trans);
            return res;
        }

        static public void MinMaxZ(this Vector4[] vertices, out int minzindex, out int maxzindex)
        {
            minzindex = maxzindex = -1;
            float minzv = float.MaxValue, maxzv = float.MinValue;
            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].Z > maxzv)
                {
                    maxzv = vertices[i].Z;
                    maxzindex = i;
                }
                if (vertices[i].Z < minzv)
                {
                    minzv = vertices[i].Z;
                    minzindex = i;
                }
            }
        }

        static public string ToStringVec(this Vector4 vertices)
        {
            return String.Format("{0},{1},{2},{3}", vertices.X, vertices.Y, vertices.Z, vertices.W);
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

        // give the % (<0 before, 0..1 inside, >1 after) of a point on vector from this->x1, the point being perpendicular to x2.
        public static float InterceptPercent(this Vector4 x0, Vector4 x1, Vector4 x2)
        {
            float dotp = (x1.X - x0.X) * (x2.X - x1.X) + (x1.Y - x0.Y) * (x2.Y - x1.Y) + (x1.Z - x0.Z) * (x2.Z - x1.Z);
            float mag2 = ((x2.X - x1.X) * (x2.X - x1.X) + (x2.Y - x1.Y) * (x2.Y - x1.Y) + (x2.Z - x1.Z) * (x2.Z - x1.Z));
            return -dotp / mag2;              // its -((x1-x0) dotp (x2-x1) / |x2-x1|^2)
        }

        // given a z position, and a vector from x0 to x1, is there a point on that vector which has the same z?  otherwise return NAN
        public static Vector4 FindVectorFromZ(this Vector4 x0, Vector4 x1, float z, float w = 1)
        {
            float zpercent = (z - x0.Z) / (x1.Z - x0.Z);        // distance from z to x0, divided by total distance.
            if (zpercent >= 0 && zpercent <= 1.0)
                return new Vector4(x0.X + (x1.X - x0.X) * zpercent, x0.Y + (x1.Y - x0.Y) * zpercent, z, w);
            else
                return new Vector4(float.NaN, float.NaN, float.NaN, float.NaN);
        }

        public static void FindVectorFromZ(this Vector4 x0, Vector4 x1, ref List<Vector4> veclist, float z, float w = 1)
        {
            float zpercent = (z - x0.Z) / (x1.Z - x0.Z);        // distance from z to x0, divided by total distance.
            if (zpercent >= 0 && zpercent <= 1.0)
                veclist.Add(new Vector4(x0.X + (x1.X - x0.X) * zpercent, x0.Y + (x1.Y - x0.Y) * zpercent, z, w));
        }

        public static void FindVectorFromZ(this Vector4 x0, Vector4 x1, ref Vector4[] vec, ref int count, float z, float w = 1)
        {
            float zpercent = (z - x0.Z) / (x1.Z - x0.Z);        // distance from z to x0, divided by total distance.
            if (zpercent >= 0 && zpercent <= 1.0)
                vec[count++] = new Vector4(x0.X + (x1.X - x0.X) * zpercent, x0.Y + (x1.Y - x0.Y) * zpercent, z, w);
        }

        public static Vector4 PointAlongPath(this Vector4 x0, Vector4 x1, float i) // i = 0 to 1.0, on the path. Negative before the path, >1 after the path
        {
            return new Vector4(x0.X + (x1.X - x0.X) * i, x0.Y + (x1.Y - x0.Y) * i, x0.Z + (x1.Z - x0.Z) * i,1);
        }

        public static float PMSquare(Vector4 p1, Vector4 p2, Vector4 p3)       // p1,p2 is the line, p3 is the test point. which side of the line is it on?
        {
            return (p3.X - p1.X) * (p2.Y - p1.Y) - (p2.X - p1.X) * (p3.Y - p1.Y);
        }

        public static Vector4 Average(this Vector4 [] array)
        {
            float x = 0, y = 0, z = 0;
            foreach( var v in array)
            {
                x += v.X;
                y += v.Y;
                z += v.Z;
            }
            return new Vector4(x / array.Length, y / array.Length, z / array.Length, 1);
        }


        #endregion

        [System.Diagnostics.Conditional("DEBUG")]
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

        static bool? LastCullface = null;

        public static void CullFace(bool on)
        {
            if (LastCullface == null || LastCullface.Value != on)
            {
                if (on)
                    GL.Enable(EnableCap.CullFace);
                else
                    GL.Disable(EnableCap.CullFace);
                LastCullface = on;
            }
        }


        public static void DefaultCullFace()
        {
            CullFace(true);
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

        public static void PointSizeByProgram()          // cache size for speed - 0 means use shader point size
        {
            PointSize(0);
        }

        public static void PointSize(float p)          // cache size for speed - 0 means use shader point size
        {
            if (LastPointSize == null || LastPointSize.Value != p)
            {
                if (p > 0)
                {
                    if ( LastPointSize == null || LastPointSize == 0 )  // if last was 0, turn off point size
                        GL.Disable(EnableCap.ProgramPointSize);

                    GL.PointSize(p);
                }
                else
                    GL.Enable(EnableCap.ProgramPointSize);

                LastPointSize = p;
            }
        }

        static bool? LastPointSpriteEnable = null;

        public static void DisablePointSprite()          // cache size for speed 
        {
            if (LastPointSpriteEnable == null || LastPointSpriteEnable == true)
            {
                GL.Disable(EnableCap.PointSprite);
                LastPointSpriteEnable = false;
            }
        }

        public static void EnablePointSprite()          // cache size for speed 
        {
            if (LastPointSpriteEnable == null || LastPointSpriteEnable == false)
            {
                GL.Enable(EnableCap.PointSprite);
                LastPointSpriteEnable = true;
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

        public static Vector3 AzEl(this Vector3 curpos, Vector3 target, bool returndegrees)     // az and elevation between curpos and target
        {
            Vector3 delta = Vector3.Subtract(target, curpos);
            //Console.WriteLine("{0}->{1} d {2}", curpos, target, delta);

            float radius = delta.Length;

            if (radius < 0.1)
                return new Vector3(180, 0, 0);     // point forward, level

            float inclination = (float)Math.Acos(delta.Y / radius);
            float azimuth = (float)Math.Atan(delta.Z / delta.X);

            if (delta.X >= 0)      // atan wraps -90 (south)->+90 (north), then -90 to +90 around the y axis, going anticlockwise
                azimuth = (float)(Math.PI / 2) - azimuth;     // adjust
            else
                azimuth = -(float)(Math.PI / 2) - azimuth;

            if (returndegrees)
            {
                inclination = inclination.Degrees();
                azimuth = azimuth.Degrees();
            }

            //System.Diagnostics.Debug.WriteLine("inc " + inclination + " az " + azimuth + " delta" + delta);

            //System.Diagnostics.Debug.WriteLine(" -> inc " + inclination + " az " + azimuth);
            return new Vector3(inclination, azimuth, 0);
        }

        public static Vector2 Floor(this Vector2 a)
        {
            return new Vector2((float)Math.Floor(a.X), (float)Math.Floor(a.Y));
        }

        public static Vector2 Fract(this Vector2 a)
        {
            float x = (float)(a.X - Math.Floor(a.X));
            float y = (float)(a.Y - Math.Floor(a.Y));
            return new Vector2(x, y);
        }

        public static Vector2 Mix(Vector2 a, Vector2 b, float mix)
        {
            float x = (float)(a.X + (b.X - a.X) * mix);
            float y = (float)(a.Y - (b.Y - a.Y) * mix);
            return new Vector2(x, y);
        }

        public static float randA(Vector2 n)
        {
            Vector2 i0 = new Vector2(12.9898f, 4.1414f);
            float i1 = Vector2.Dot(n, i0);
            float i2 = (float)Math.Sin(i1) * 43758.5453f;
            return i2.Fract();
        }

        public static float noiseA(Vector2 p)
        {
            Vector2 ip = p.Floor();
            Vector2 u = p.Fract();
            u = u * u * (new Vector2(3, 3) - 2.0f * u);

            float res =
                ObjectExtensionsNumbersBool.Mix(
                    ObjectExtensionsNumbersBool.Mix(GLStatics.randA(ip), GLStatics.randA(ip + new Vector2(1.0f, 0.0f)), u.X),
                    ObjectExtensionsNumbersBool.Mix(GLStatics.randA(ip + new Vector2(0.0f, 1.0f)), GLStatics.randA(ip + new Vector2(1.0f, 1.0f)), u.X),
                    u.Y);
            return res * res;
        }

        public static string[] Extensions()
        {
            return GL.GetString(StringName.Extensions).Split(' ');
        }

        public static bool HasExtensions(string s)
        {
            return Array.IndexOf(Extensions(), s)>=0;
        }

        // public delegate void DebugProc(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam);

        public static bool EnableDebug( DebugProc p )
        {
            if (HasExtensions("GL_KHR_debug"))
            {
                GL.Enable(EnableCap.DebugOutput);
                GL.Enable(EnableCap.DebugOutputSynchronous);

                GL.DebugMessageCallback(p, IntPtr.Zero);
                GL.DebugMessageControl(DebugSourceControl.DontCare, DebugTypeControl.DontCare, DebugSeverityControl.DontCare, 0, new int[0], true);

                GL.DebugMessageInsert(DebugSourceExternal.DebugSourceApplication, DebugType.DebugTypeMarker, 0, DebugSeverity.DebugSeverityNotification, -1, "Debug output enabled");

                return true;
            }
            else
                return false;
        }
    }
}
