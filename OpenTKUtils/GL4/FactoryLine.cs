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
    static public class LineObjectFactory
    {
        public static VertexColour[] CreateLines(Vector3 startpos, Vector3 endpos, Vector3 offset, int lines, Color4[] colors)
        {
            VertexColour[] vertices = new VertexColour[lines * 2];

            for (int i = 0; i < lines; i++)
            {
                vertices[i * 2] = new VertexColour(new Vector4(startpos.X, startpos.Y, startpos.Z, 1.0f), VertexColour.ColorFrom(colors, i * 2));
                vertices[i * 2 + 1] = new VertexColour(new Vector4(endpos.X, endpos.Y, endpos.Z, 1.0f), VertexColour.ColorFrom(colors, i * 2+1));
                startpos += offset;
                endpos += offset;
            }

            return vertices;
        }


    }
}