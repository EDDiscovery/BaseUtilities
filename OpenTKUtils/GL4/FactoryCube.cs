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
    static public class GLCubeObjectFactory
    {
        public static GLVertexColour[] CreateSolidCubeFromTriangles(float side, Color4 color, Vector3? pos = null )
        {
            side = side / 2f; // halv side - and other half +
            GLVertexColour[] vertices =
            {
                new GLVertexColour(new Vector4(-side, -side, -side, 1.0f), color),
                new GLVertexColour(new Vector4(-side, side, -side, 1.0f), color),
                new GLVertexColour(new Vector4(-side, -side, side, 1.0f), color),
                new GLVertexColour(new Vector4(-side, -side, side, 1.0f), color),
                new GLVertexColour(new Vector4(-side, side, -side, 1.0f), color),
                new GLVertexColour(new Vector4(-side, side, side, 1.0f), color),

                new GLVertexColour(new Vector4(side, -side, -side, 1.0f), color),
                new GLVertexColour(new Vector4(side, side, -side, 1.0f), color),
                new GLVertexColour(new Vector4(side, -side, side, 1.0f), color),
                new GLVertexColour(new Vector4(side, -side, side, 1.0f), color),
                new GLVertexColour(new Vector4(side, side, -side, 1.0f), color),
                new GLVertexColour(new Vector4(side, side, side, 1.0f), color),

                new GLVertexColour(new Vector4(-side, -side, -side, 1.0f), color),
                new GLVertexColour(new Vector4(side, -side, -side, 1.0f), color),
                new GLVertexColour(new Vector4(-side, -side, side, 1.0f), color),
                new GLVertexColour(new Vector4(-side, -side, side, 1.0f), color),
                new GLVertexColour(new Vector4(side, -side, -side, 1.0f), color),
                new GLVertexColour(new Vector4(side, -side, side, 1.0f), color),

                new GLVertexColour(new Vector4(-side, side, -side, 1.0f), color),
                new GLVertexColour(new Vector4(side, side, -side, 1.0f), color),
                new GLVertexColour(new Vector4(-side, side, side, 1.0f), color),
                new GLVertexColour(new Vector4(-side, side, side, 1.0f), color),
                new GLVertexColour(new Vector4(side, side, -side, 1.0f), color),
                new GLVertexColour(new Vector4(side, side, side, 1.0f), color),

                new GLVertexColour(new Vector4(-side, -side, -side, 1.0f), color),
                new GLVertexColour(new Vector4(side, -side, -side, 1.0f), color),
                new GLVertexColour(new Vector4(-side, side, -side, 1.0f), color),
                new GLVertexColour(new Vector4(-side, side, -side, 1.0f), color),
                new GLVertexColour(new Vector4(side, -side, -side, 1.0f), color),
                new GLVertexColour(new Vector4(side, side, -side, 1.0f), color),

                new GLVertexColour(new Vector4(-side, -side, side, 1.0f), color),
                new GLVertexColour(new Vector4(side, -side, side, 1.0f), color),
                new GLVertexColour(new Vector4(-side, side, side, 1.0f), color),
                new GLVertexColour(new Vector4(-side, side, side, 1.0f), color),
                new GLVertexColour(new Vector4(side, -side, side, 1.0f), color),
                new GLVertexColour(new Vector4(side, side, side, 1.0f), color),
            };

            if (pos != null)
                GLVertexColour.Translate(vertices, pos.Value);

            return vertices;
        }

        public static GLVertexColour[] CreateVertexPointCube(float side, Color4[] color, Vector3? pos = null)
        {
            side = side / 2f; // halv side - and other half +
            GLVertexColour[] vertices =
            {
                new GLVertexColour(new Vector4(-side, side, side, 1.0f), color.ColorFrom(0 )),       // arranged as wound clockwise around top, then around bottom
                new GLVertexColour(new Vector4(side, side, side, 1.0f), color.ColorFrom(1 )),
                new GLVertexColour(new Vector4(side, side, -side, 1.0f),  color.ColorFrom(2 )),
                new GLVertexColour(new Vector4(-side, side, -side, 1.0f),  color.ColorFrom(3 )),
                new GLVertexColour(new Vector4(-side, -side, side, 1.0f), color.ColorFrom(4 )),
                new GLVertexColour(new Vector4(side, -side, side, 1.0f), color.ColorFrom(5 )),
                new GLVertexColour(new Vector4(side, -side, -side, 1.0f),  color.ColorFrom(6 )),
                new GLVertexColour(new Vector4(-side, -side, -side, 1.0f),  color.ColorFrom(7 )),
            };

            if (pos != null)
                GLVertexColour.Translate(vertices, pos.Value);

            return vertices;
        }

    }
}