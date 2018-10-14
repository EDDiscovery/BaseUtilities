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
using System;

namespace OpenTKUtils.GL4
{
    public class GLTexturedObjectFactory
    {
        public static GLVertexTextured[] CreateTexturedCubeFromTriangles(float side, GLTexture tex, Vector3? pos = null)
        {
            return CreateTexturedCubeFromTriangles(side, tex.Width, tex.Height, pos);
        }

        public static GLVertexTextured[] CreateTexturedCubeFromTriangles(float side, float textureWidth, float textureHeight, Vector3? pos = null)
        {
                float h = textureHeight;
            float w = textureWidth;
            side = side / 2f; // half side - and other half

            GLVertexTextured[] vertices =
            {
                new GLVertexTextured(new Vector4(-side, -side, -side, 1.0f),   new Vector2(0, h)),
                new GLVertexTextured(new Vector4(-side, -side, side, 1.0f),    new Vector2(w, h)),
                new GLVertexTextured(new Vector4(-side, side, -side, 1.0f),    new Vector2(0, 0)),
                new GLVertexTextured(new Vector4(-side, side, -side, 1.0f),    new Vector2(0, 0)),
                new GLVertexTextured(new Vector4(-side, -side, side, 1.0f),    new Vector2(w, h)),
                new GLVertexTextured(new Vector4(-side, side, side, 1.0f),     new Vector2(w, 0)),

                new GLVertexTextured(new Vector4(side, -side, -side, 1.0f),    new Vector2(w, 0)),
                new GLVertexTextured(new Vector4(side, side, -side, 1.0f),     new Vector2(0, 0)),
                new GLVertexTextured(new Vector4(side, -side, side, 1.0f),     new Vector2(w, h)),
                new GLVertexTextured(new Vector4(side, -side, side, 1.0f),     new Vector2(w, h)),
                new GLVertexTextured(new Vector4(side, side, -side, 1.0f),     new Vector2(0, 0)),
                new GLVertexTextured(new Vector4(side, side, side, 1.0f),      new Vector2(0, h)),

                new GLVertexTextured(new Vector4(-side, -side, -side, 1.0f),   new Vector2(w, 0)),
                new GLVertexTextured(new Vector4(side, -side, -side, 1.0f),    new Vector2(0, 0)),
                new GLVertexTextured(new Vector4(-side, -side, side, 1.0f),    new Vector2(w, h)),
                new GLVertexTextured(new Vector4(-side, -side, side, 1.0f),    new Vector2(w, h)),
                new GLVertexTextured(new Vector4(side, -side, -side, 1.0f),    new Vector2(0, 0)),
                new GLVertexTextured(new Vector4(side, -side, side, 1.0f),     new Vector2(0, h)),

                new GLVertexTextured(new Vector4(-side, side, -side, 1.0f),    new Vector2(w, 0)),
                new GLVertexTextured(new Vector4(-side, side, side, 1.0f),     new Vector2(0, 0)),
                new GLVertexTextured(new Vector4(side, side, -side, 1.0f),     new Vector2(w, h)),
                new GLVertexTextured(new Vector4(side, side, -side, 1.0f),     new Vector2(w, h)),
                new GLVertexTextured(new Vector4(-side, side, side, 1.0f),     new Vector2(0, 0)),
                new GLVertexTextured(new Vector4(side, side, side, 1.0f),      new Vector2(0, h)),

                new GLVertexTextured(new Vector4(-side, -side, -side, 1.0f),   new Vector2(0, h)),
                new GLVertexTextured(new Vector4(-side, side, -side, 1.0f),    new Vector2(w, h)),
                new GLVertexTextured(new Vector4(side, -side, -side, 1.0f),    new Vector2(0, 0)),
                new GLVertexTextured(new Vector4(side, -side, -side, 1.0f),    new Vector2(0, 0)),
                new GLVertexTextured(new Vector4(-side, side, -side, 1.0f),    new Vector2(w, h)),
                new GLVertexTextured(new Vector4(side, side, -side, 1.0f),     new Vector2(w, 0)),

                new GLVertexTextured(new Vector4(-side, -side, side, 1.0f),    new Vector2(0, h)),
                new GLVertexTextured(new Vector4(side, -side, side, 1.0f),     new Vector2(w, h)),
                new GLVertexTextured(new Vector4(-side, side, side, 1.0f),     new Vector2(0, 0)),
                new GLVertexTextured(new Vector4(-side, side, side, 1.0f),     new Vector2(0, 0)),
                new GLVertexTextured(new Vector4(side, -side, side, 1.0f),     new Vector2(w, h)),
                new GLVertexTextured(new Vector4(side, side, side, 1.0f),      new Vector2(w, 0)),
            };

            if (pos != null)
                GLVertexTextured.Translate(vertices, pos.Value);
            return vertices;
        }

        // rotation in degrees..
        public static GLVertexTextured[] CreateTexturedQuad(float scale, Vector3 rotation, GLTexture tex, float whratio = 1.0f, Vector3? pos = null)
        {
            return CreateTexturedQuad(scale, rotation, tex.Width, tex.Height, whratio , pos);
        }

        public static GLVertexTextured[] CreateTexturedQuad(float scale, Vector3 rotation, float textureWidth, float textureHeight, float whratio = 1.0f, Vector3? pos = null)
        {
            float xsize = scale * textureWidth / 2.0f / whratio;
            float ysize = scale * textureHeight / 2.0f;

            GLVertexTextured[] vertices =
            {
                new GLVertexTextured(new Vector4(-xsize, 0, -ysize, 1.0f),   new Vector2(0, textureHeight)),
                new GLVertexTextured(new Vector4(+xsize, 0, -ysize, 1.0f),    new Vector2(textureWidth, textureHeight)),
                new GLVertexTextured(new Vector4(+xsize, 0, +ysize, 1.0f),    new Vector2(textureWidth, 0)),
                new GLVertexTextured(new Vector4(-xsize, 0, +ysize, 1.0f),    new Vector2(0, 0)),
            };

            if (pos != null)
                GLVertexTextured.Translate(vertices, pos.Value);

            if (rotation.Length > 0)
            {
                Matrix4 transform = Matrix4.Identity;                   // identity nominal matrix, dir is in degrees
                transform *= Matrix4.CreateRotationX((float)(rotation.X * Math.PI / 180.0f));
                transform *= Matrix4.CreateRotationY((float)(rotation.Y * Math.PI / 180.0f));
                transform *= Matrix4.CreateRotationZ((float)(rotation.Z * Math.PI / 180.0f));
                GLVertexTextured.Transform(vertices, transform);
            }

            return vertices;
        }
    }
}
