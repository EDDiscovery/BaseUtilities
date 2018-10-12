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

namespace OpenTKUtils.GL4
{
    static public class CubeObjectFactory
    {
        public static VertexColour[] CreateSolidCube(float side, Color4 color)
        {
            side = side / 2f; // halv side - and other half +
            VertexColour[] vertices =
            {
                new VertexColour(new Vector4(-side, -side, -side, 1.0f), color),
                new VertexColour(new Vector4(-side, side, -side, 1.0f), color),
                new VertexColour(new Vector4(-side, -side, side, 1.0f), color),
                new VertexColour(new Vector4(-side, -side, side, 1.0f), color),
                new VertexColour(new Vector4(-side, side, -side, 1.0f), color),
                new VertexColour(new Vector4(-side, side, side, 1.0f), color),

                new VertexColour(new Vector4(side, -side, -side, 1.0f), color),
                new VertexColour(new Vector4(side, side, -side, 1.0f), color),
                new VertexColour(new Vector4(side, -side, side, 1.0f), color),
                new VertexColour(new Vector4(side, -side, side, 1.0f), color),
                new VertexColour(new Vector4(side, side, -side, 1.0f), color),
                new VertexColour(new Vector4(side, side, side, 1.0f), color),

                new VertexColour(new Vector4(-side, -side, -side, 1.0f), color),
                new VertexColour(new Vector4(side, -side, -side, 1.0f), color),
                new VertexColour(new Vector4(-side, -side, side, 1.0f), color),
                new VertexColour(new Vector4(-side, -side, side, 1.0f), color),
                new VertexColour(new Vector4(side, -side, -side, 1.0f), color),
                new VertexColour(new Vector4(side, -side, side, 1.0f), color),

                new VertexColour(new Vector4(-side, side, -side, 1.0f), color),
                new VertexColour(new Vector4(side, side, -side, 1.0f), color),
                new VertexColour(new Vector4(-side, side, side, 1.0f), color),
                new VertexColour(new Vector4(-side, side, side, 1.0f), color),
                new VertexColour(new Vector4(side, side, -side, 1.0f), color),
                new VertexColour(new Vector4(side, side, side, 1.0f), color),

                new VertexColour(new Vector4(-side, -side, -side, 1.0f), color),
                new VertexColour(new Vector4(side, -side, -side, 1.0f), color),
                new VertexColour(new Vector4(-side, side, -side, 1.0f), color),
                new VertexColour(new Vector4(-side, side, -side, 1.0f), color),
                new VertexColour(new Vector4(side, -side, -side, 1.0f), color),
                new VertexColour(new Vector4(side, side, -side, 1.0f), color),

                new VertexColour(new Vector4(-side, -side, side, 1.0f), color),
                new VertexColour(new Vector4(side, -side, side, 1.0f), color),
                new VertexColour(new Vector4(-side, side, side, 1.0f), color),
                new VertexColour(new Vector4(-side, side, side, 1.0f), color),
                new VertexColour(new Vector4(side, -side, side, 1.0f), color),
                new VertexColour(new Vector4(side, side, side, 1.0f), color),
            };

            return vertices;
        }

        public static VertexColour[] CreateSolidCube(Vector3 pos, float side, Color4 color)
        {
            var v = CreateSolidCube(side,color);
            VertexColour.Translate(v, pos);
            return v;
        }

        public static VertexColour[] CreateVertexPointCube(Vector3 pos, float side, Color4[] color)
        {
            side = side / 2f; // halv side - and other half +
            VertexColour[] vertices =
            {
                new VertexColour(new Vector4(-side, side, side, 1.0f), VertexColour.ColorFrom(color,0 )),       // arranged as wound clockwise around top, then around bottom
                new VertexColour(new Vector4(side, side, side, 1.0f), VertexColour.ColorFrom(color,1 )),
                new VertexColour(new Vector4(side, side, -side, 1.0f),  VertexColour.ColorFrom(color,2 )),
                new VertexColour(new Vector4(-side, side, -side, 1.0f),  VertexColour.ColorFrom(color,3 )),
                new VertexColour(new Vector4(-side, -side, side, 1.0f), VertexColour.ColorFrom(color,4 )),
                new VertexColour(new Vector4(side, -side, side, 1.0f), VertexColour.ColorFrom(color,5 )),
                new VertexColour(new Vector4(side, -side, -side, 1.0f),  VertexColour.ColorFrom(color,6 )),
                new VertexColour(new Vector4(-side, -side, -side, 1.0f),  VertexColour.ColorFrom(color,7 )),
            };                                                          

            VertexColour.Translate(vertices, pos);

            return vertices;
        }

    }
}