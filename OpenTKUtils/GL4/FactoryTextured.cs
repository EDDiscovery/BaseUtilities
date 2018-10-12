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
    public class TexturedObjectFactory
    {
        public static VertexTextured[] CreateTexturedCube(float side, float textureWidth, float textureHeight)
        {
            float h = textureHeight;
            float w = textureWidth;
            side = side / 2f; // half side - and other half

            VertexTextured[] vertices =
            {
                new VertexTextured(new Vector4(-side, -side, -side, 1.0f),   new Vector2(0, h)),
                new VertexTextured(new Vector4(-side, -side, side, 1.0f),    new Vector2(w, h)),
                new VertexTextured(new Vector4(-side, side, -side, 1.0f),    new Vector2(0, 0)),
                new VertexTextured(new Vector4(-side, side, -side, 1.0f),    new Vector2(0, 0)),
                new VertexTextured(new Vector4(-side, -side, side, 1.0f),    new Vector2(w, h)),
                new VertexTextured(new Vector4(-side, side, side, 1.0f),     new Vector2(w, 0)),

                new VertexTextured(new Vector4(side, -side, -side, 1.0f),    new Vector2(w, 0)),
                new VertexTextured(new Vector4(side, side, -side, 1.0f),     new Vector2(0, 0)),
                new VertexTextured(new Vector4(side, -side, side, 1.0f),     new Vector2(w, h)),
                new VertexTextured(new Vector4(side, -side, side, 1.0f),     new Vector2(w, h)),
                new VertexTextured(new Vector4(side, side, -side, 1.0f),     new Vector2(0, 0)),
                new VertexTextured(new Vector4(side, side, side, 1.0f),      new Vector2(0, h)),

                new VertexTextured(new Vector4(-side, -side, -side, 1.0f),   new Vector2(w, 0)),
                new VertexTextured(new Vector4(side, -side, -side, 1.0f),    new Vector2(0, 0)),
                new VertexTextured(new Vector4(-side, -side, side, 1.0f),    new Vector2(w, h)),
                new VertexTextured(new Vector4(-side, -side, side, 1.0f),    new Vector2(w, h)),
                new VertexTextured(new Vector4(side, -side, -side, 1.0f),    new Vector2(0, 0)),
                new VertexTextured(new Vector4(side, -side, side, 1.0f),     new Vector2(0, h)),

                new VertexTextured(new Vector4(-side, side, -side, 1.0f),    new Vector2(w, 0)),
                new VertexTextured(new Vector4(-side, side, side, 1.0f),     new Vector2(0, 0)),
                new VertexTextured(new Vector4(side, side, -side, 1.0f),     new Vector2(w, h)),
                new VertexTextured(new Vector4(side, side, -side, 1.0f),     new Vector2(w, h)),
                new VertexTextured(new Vector4(-side, side, side, 1.0f),     new Vector2(0, 0)),
                new VertexTextured(new Vector4(side, side, side, 1.0f),      new Vector2(0, h)),

                new VertexTextured(new Vector4(-side, -side, -side, 1.0f),   new Vector2(0, h)),
                new VertexTextured(new Vector4(-side, side, -side, 1.0f),    new Vector2(w, h)),
                new VertexTextured(new Vector4(side, -side, -side, 1.0f),    new Vector2(0, 0)),
                new VertexTextured(new Vector4(side, -side, -side, 1.0f),    new Vector2(0, 0)),
                new VertexTextured(new Vector4(-side, side, -side, 1.0f),    new Vector2(w, h)),
                new VertexTextured(new Vector4(side, side, -side, 1.0f),     new Vector2(w, 0)),

                new VertexTextured(new Vector4(-side, -side, side, 1.0f),    new Vector2(0, h)),
                new VertexTextured(new Vector4(side, -side, side, 1.0f),     new Vector2(w, h)),
                new VertexTextured(new Vector4(-side, side, side, 1.0f),     new Vector2(0, 0)),
                new VertexTextured(new Vector4(-side, side, side, 1.0f),     new Vector2(0, 0)),
                new VertexTextured(new Vector4(side, -side, side, 1.0f),     new Vector2(w, h)),
                new VertexTextured(new Vector4(side, side, side, 1.0f),      new Vector2(w, 0)),
            };
            return vertices;
        }

        public static VertexTextured[] CreateTexturedCube(Vector3 pos, float side, float textureWidth, float textureHeight)
        {
            var v = CreateTexturedCube(side, textureWidth, textureHeight);
            VertexTextured.Translate(v, pos);
            return v;
        }

        public static VertexTextured[] CreateFlatTexturedQuadImage(Vector3 pos, float side, float textureWidth, float textureHeight)
        {
            float h = textureHeight;
            float w = textureWidth;
            side = side / 2f; // half side - and other half

            VertexTextured[] vertices =
            {
                new VertexTextured(new Vector4(pos.X-side, pos.Y, pos.Z-side, 1.0f),   new Vector2(0, h)),
                new VertexTextured(new Vector4(pos.X+side, pos.Y, pos.Z-side, 1.0f),    new Vector2(w, h)),
                new VertexTextured(new Vector4(pos.X+side, pos.Y, pos.Z+side, 1.0f),    new Vector2(w, 0)),
                new VertexTextured(new Vector4(pos.X-side, pos.Y, pos.Z+side, 1.0f),    new Vector2(0, 0)),
            };
            return vertices;
        }

        public static VertexTextured[] CreateVerticalTexturedQuadImage(Vector3 pos, float side, float textureWidth, float textureHeight)
        {
            float h = textureHeight;
            float w = textureWidth;
            side = side / 2f; // half side - and other half

            VertexTextured[] vertices =
            {
                new VertexTextured(new Vector4(pos.X-side, pos.Y-side, pos.Z, 1.0f),   new Vector2(0, h)),
                new VertexTextured(new Vector4(pos.X+side, pos.Y-side, pos.Z, 1.0f),    new Vector2(w, h)),
                new VertexTextured(new Vector4(pos.X+side, pos.Y+side, pos.Z, 1.0f),    new Vector2(w, 0)),
                new VertexTextured(new Vector4(pos.X-side, pos.Y+side, pos.Z, 1.0f),    new Vector2(0, 0)),
            };
            return vertices;
        }


    }
}
