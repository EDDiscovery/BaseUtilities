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


using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;

namespace OpenTKUtils.GL4
{
    // Factory created Vector4 shapes..

    static public class GLCubeObjectFactory
    {
        public enum Sides { Left,Right,Front,Back,Bottom,Top, All};

        public static Vector4[] CreateSolidCubeFromTriangles(float side, Vector3? pos = null)
        {
            return CreateSolidCubeFromTriangles(side, new Sides[] { Sides.All }, pos);
        }

        public static Vector4[] CreateSolidCubeFromTriangles(float side, Sides[] sides, Vector3? pos = null)
        {
            side = side / 2f; // halv side - and other half +
            List<Vector4> vert = new List<Vector4>();

            bool all = Array.IndexOf(sides, Sides.All) >= 0;

            if (all || Array.IndexOf(sides, Sides.Left) >= 0)
            {
                vert.AddRange(new Vector4[] {
                                            new Vector4(new Vector4(-side, -side, -side, 1.0f)),        // left side, lower right triangle
                                            new Vector4(new Vector4(-side, side, -side, 1.0f)),
                                            new Vector4(new Vector4(-side, -side, side, 1.0f)),
                                            new Vector4(new Vector4(-side, -side, side, 1.0f)),         // left side, upper left triangle
                                            new Vector4(new Vector4(-side, side, -side, 1.0f)),
                                            new Vector4(new Vector4(-side, side, side, 1.0f)),
                });
            }
            if (all || Array.IndexOf(sides, Sides.Right) >= 0)
            {
                vert.AddRange(new Vector4[] {
                new Vector4(new Vector4(side, -side, side, 1.0f)),         // right side, lower right triangle
                new Vector4(new Vector4(side, side, side, 1.0f)),
                new Vector4(new Vector4(side, -side, -side, 1.0f)),
                new Vector4(new Vector4(side, -side, -side, 1.0f)),         // right side, upper left triangle
                new Vector4(new Vector4(side, side, side, 1.0f)),
                new Vector4(new Vector4(side, side, -side, 1.0f)),
                });
            }
            if (all || Array.IndexOf(sides, Sides.Bottom) >= 0)
            {
                vert.AddRange(new Vector4[] {
                new Vector4(new Vector4(side, -side, side, 1.0f)),        // bottom face, lower right
                new Vector4(new Vector4(side, -side, -side, 1.0f)),
                new Vector4(new Vector4(-side, -side, side, 1.0f)),
                new Vector4(new Vector4(-side, -side, side, 1.0f)),         //bottom face, upper left
                new Vector4(new Vector4(side, -side, -side, 1.0f)),
                new Vector4(new Vector4(-side, -side, -side, 1.0f)),
                });
            }
            if (all || Array.IndexOf(sides, Sides.Top) >= 0)
            {
                vert.AddRange(new Vector4[] {
                new Vector4(new Vector4(side, side, -side, 1.0f)),         // top face
                new Vector4(new Vector4(side, side, side, 1.0f)),
                new Vector4(new Vector4(-side, side, -side, 1.0f)),
                new Vector4(new Vector4(-side, side, -side, 1.0f)),
                new Vector4(new Vector4(side, side, side, 1.0f)),
                new Vector4(new Vector4(-side, side, side, 1.0f)),
                });
            }
            if (all || Array.IndexOf(sides, Sides.Front) >= 0)
            {
                vert.AddRange(new Vector4[] {
                new Vector4(new Vector4(side, -side, -side, 1.0f)),         // front face, lower right
                new Vector4(new Vector4(side, side, -side, 1.0f)),
                new Vector4(new Vector4(-side, -side, -side, 1.0f)),
                new Vector4(new Vector4(-side, -side, -side, 1.0f)),        // front face, upper left
                new Vector4(new Vector4(side, side, -side, 1.0f)),
                new Vector4(new Vector4(-side, side, -side, 1.0f)),
                });
            }
            if (all || Array.IndexOf(sides, Sides.Back) >= 0)
            {
                vert.AddRange(new Vector4[] {
                new Vector4(new Vector4(-side, -side, side, 1.0f)),         // back face, lower right
                new Vector4(new Vector4(-side, side, side, 1.0f)),
                new Vector4(new Vector4(side, -side, side, 1.0f)),
                new Vector4(new Vector4(side, -side, side, 1.0f)),          // back face, upper left
                new Vector4(new Vector4(-side, side, side, 1.0f)),
                new Vector4(new Vector4(side, side, side, 1.0f)),
                });
            }

            var array = vert.ToArray();
            if (pos != null)
                array.Translate(pos.Value);

            return array;
        }

        static Vector2[] tricoords = new Vector2[]
        {
                new Vector2(1.0f, 1.0f),      // lower right triangle
                new Vector2(1.0f, 0),
                new Vector2(0, 1.0f),
                new Vector2(0, 1.0f),      // upper left triangle
                new Vector2(1.0f, 0),
                new Vector2(0, 0),
        };

        public static Vector2[] CreateTexTriangles(int number)
        {
            Vector2[] t = new Vector2[number * 6];
            for (int i = 0; i < number * 6; i++)
                t[i] = tricoords[i % 6];

            return t;
        }

        public static Vector2[] CreateCubeTexTriangles()
        {
            return CreateTexTriangles(6);
        }

        public static Vector4[] CreateVertexPointCube(float side, Vector3? pos = null)
        {
            side = side / 2f; // halv side - and other half +
            Vector4[] vertices =
            {
                new Vector4(new Vector4(-side, side, side, 1.0f)),       // arranged as wound clockwise around top, then around bottom
                new Vector4(new Vector4(side, side, side, 1.0f)),
                new Vector4(new Vector4(side, side, -side, 1.0f)),
                new Vector4(new Vector4(-side, side, -side, 1.0f)),
                new Vector4(new Vector4(-side, -side, side, 1.0f)),
                new Vector4(new Vector4(side, -side, side, 1.0f)),
                new Vector4(new Vector4(side, -side, -side, 1.0f)),
                new Vector4(new Vector4(-side, -side, -side, 1.0f)),
            };

            if (pos != null)
                vertices.Translate(pos.Value);

            return vertices;
        }

    }
}