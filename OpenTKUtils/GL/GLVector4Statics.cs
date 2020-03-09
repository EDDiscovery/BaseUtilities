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

using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;

namespace OpenTKUtils
{
    public static class GLStaticsVector4
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

        static public string ToStringVec(this Vector4 vertices, string wformat = null)
        {
            if ( wformat != null )
                return String.Format("{0,10:0.00},{1,10:0.00},{2,10:0.00},{3," + wformat +"}", vertices.X, vertices.Y, vertices.Z, vertices.W);
            else
                return String.Format("{0,10:0.00},{1,10:0.00},{2,10:0.00}", vertices.X, vertices.Y, vertices.Z);
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

        public static float FindVectorFromZ(this Vector4 x0, Vector4 x1, ref Vector4[] vec, ref int count, float z, float w = 1)
        {
            float zpercent = (z - x0.Z) / (x1.Z - x0.Z);        // distance from z to x0, divided by total distance.
            if (zpercent >= 0 && zpercent <= 1.0)
                vec[count++] = new Vector4(x0.X + (x1.X - x0.X) * zpercent, x0.Y + (x1.Y - x0.Y) * zpercent, z, w);

            return zpercent;
        }

        public static float FindVectorFromZ(this Vector4 x0, Vector4 x1, ref Vector4[] vec, ref Vector3[] tex, Vector3 template, ref int count, float z, float w = 1)
        {
            float zpercent = (z - x0.Z) / (x1.Z - x0.Z);        // distance from z to x0, divided by total distance.
            if (zpercent >= 0 && zpercent <= 1.0)
            {
                if (template.X == 9)
                    tex[count] = new Vector3(zpercent, template.Y, template.Z);
                else if (template.Y == 9)
                    tex[count] = new Vector3(template.X, zpercent, template.Z);
                else 
                    tex[count] = new Vector3(template.X, template.Y, zpercent );

                vec[count++] = new Vector4(x0.X + (x1.X - x0.X) * zpercent, x0.Y + (x1.Y - x0.Y) * zpercent, z, w);
            }

            return zpercent;
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

        public static Vector3 ToVector3(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }


    }
}
