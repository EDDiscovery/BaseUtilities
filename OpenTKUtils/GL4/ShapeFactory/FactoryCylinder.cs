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
    // Factory created Cylinder Faces for use

    static public class GLCylinderObjectFactory
    {
        // Thanks to https://github.com/doukasd/Unity-Components/blob/master/ProceduralCylinder/Assets/Scripts/Procedural/ProceduralCylinder.cs for the inspiration

        public static Tuple<Vector4[], Vector2[], uint[]> CreateCylinderFromTrianglesIndexes(float radius, float height, uint radialSegments, uint heightSegments,
                                                    bool caps,
                                                    Vector3? pos = null, bool ccw = true)
        {
            if (!pos.HasValue)
                pos = new Vector3(0, 0, 0);

            uint numVertexColumns = radialSegments + 1;
            uint numVertexRows = heightSegments + 1;

            uint numVertices = numVertexColumns * numVertexRows;
            var Vertices = new Vector4[numVertices];
            Vector2[] UVs = new Vector2[numVertices];

            //precalculate increments to improve performance
            float heightStep = height / heightSegments;
            float angleStep = (float)(2 * Math.PI / radialSegments);
            if (ccw)
                angleStep = -angleStep;
            float uvStepH = 1.0f / radialSegments;
            float uvStepV = 1.0f / heightSegments;

            uint numSideTris = radialSegments * heightSegments * 2;      // 2 triangles per area
            uint numCapTris = (caps) ? (radialSegments - 2) : 0;         // if caps on, we have 2 triangles for 4 vertexes, etc, 

            var Indexes = new uint[numSideTris * 3 + numCapTris * 3 * 2];

            for (int j = 0; j < numVertexRows; j++)
            {
                for (int i = 0; i < numVertexColumns; i++)
                {
                    float angle = (i == numVertexColumns - 1) ? 0 : (i * angleStep);

                    Vertices[j * numVertexColumns + i] = new Vector4(pos.Value.X + radius * (float)Math.Cos(angle),
                                                                     pos.Value.Y + j * heightStep,
                                                                     pos.Value.Z + radius * (float)Math.Sin(angle), 0);

                    UVs[j * numVertexColumns + i] = new Vector2(ccw ? (1 - i * uvStepH) : (i * uvStepH), 1 - j * uvStepV);
                }
            }

            for (uint j = 1; j < numVertexRows; j++)
            {
                for (uint i = 0; i < numVertexColumns - 1; i++)
                {
                    uint baseIndex = (j - 1) * radialSegments * 6 + i * 6;

                    //1st tri - below and in front
                    Indexes[baseIndex + 0] = j * numVertexColumns + i;
                    Indexes[baseIndex + 1] = j * numVertexColumns + i + 1;
                    Indexes[baseIndex + 2] = (j - 1) * numVertexColumns + i;

                    Indexes[baseIndex + 3] = (j - 1) * numVertexColumns + i;
                    Indexes[baseIndex + 4] = j * numVertexColumns + i + 1;
                    Indexes[baseIndex + 5] = (j - 1) * numVertexColumns + i + 1;
                }
            }

            if (caps)
            {
                uint captri = 3 * numSideTris;
                uint botvert = numVertexColumns * (numVertexRows - 1);

                bool leftSided = true;
                uint leftIndex = 1;
                uint rightIndex = numVertexColumns - 2;
                uint middleIndex = 0;

                for (int i = 0; i < numCapTris; i++)
                {
                    if (i == 0)
                    {
                    }
                    else if (leftSided)
                    {
                        middleIndex = rightIndex;
                        rightIndex--;
                    }
                    else
                    {
                        middleIndex = leftIndex;
                        leftIndex++;
                    }
                    leftSided = !leftSided;

                    System.Diagnostics.Debug.WriteLine("{0} {1} {2}", leftIndex, middleIndex, rightIndex);

                    Indexes[captri + 0] = leftIndex;
                    Indexes[captri + 2] = middleIndex;
                    Indexes[captri + 1] = rightIndex;

                    Indexes[captri + 3] = botvert + leftIndex;
                    Indexes[captri + 4] = botvert + middleIndex;
                    Indexes[captri + 5] = botvert + rightIndex;
                    captri += 6;
                }
            }

            return new Tuple<Vector4[], Vector2[], uint[]>(Vertices, UVs, Indexes);
        }


        public static Tuple<Vector4[], Vector2[]> CreateCylinderFromTriangles(float radius, float height, int radialSegments, int heightSegments,
                                                    bool caps,
                                                    Vector3? pos = null, bool ccw = true)
        {
            if (!pos.HasValue)
                pos = new Vector3(0, 0, 0);

            int numVertexColumns = radialSegments + 1;
            int numVertexRows = heightSegments + 1;

            int numVertices = numVertexColumns * numVertexRows;
            var Vertices = new Vector4[numVertices];
            Vector2[] UVs = new Vector2[numVertices];

            //precalculate increments to improve performance
            float heightStep = height / heightSegments;
            float angleStep = (float)(2 * Math.PI / radialSegments);
            if (ccw)
                angleStep = -angleStep;
            float uvStepH = 1.0f / radialSegments;
            float uvStepV = 1.0f / heightSegments;
            
            int numSideTris = radialSegments * heightSegments * 2;      // 2 triangles per area
            int numCapTris = (caps) ? (radialSegments -2) : 0;         // if caps on, we have 2 triangles for 4 vertexes, etc, top and bot *2

            var Tris = new Vector4[numSideTris * 3 + numCapTris * 3 * 2];
            var UVOut = new Vector2[numSideTris * 3 + numCapTris * 3 * 2];

            for (int j = 0; j < numVertexRows; j++)
            {
                for (int i = 0; i < numVertexColumns; i++)
                {
                    float angle = (i == numVertexColumns - 1) ? 0 : (i * angleStep);

                    Vertices[j * numVertexColumns + i] = new Vector4(pos.Value.X + radius * (float)Math.Cos(angle),
                                                                     pos.Value.Y + j * heightStep,
                                                                     pos.Value.Z + radius * (float)Math.Sin(angle), 0);

                    UVs[j * numVertexColumns + i] = new Vector2(ccw ? (1 - i * uvStepH) : (i * uvStepH), 1 - j * uvStepV);
                }
            }

            for (int j = 1; j < numVertexRows; j++)
            {
                for (int i = 0; i < numVertexColumns - 1; i++)
                {
                    int baseIndex = (j - 1) * radialSegments * 6 + i * 6;

                    Tris[baseIndex + 0] = Vertices[j * numVertexColumns + i];
                    Tris[baseIndex + 1] = Vertices[j * numVertexColumns + i + 1];
                    Tris[baseIndex + 2] = Vertices[(j - 1) * numVertexColumns + i];

                    UVOut[baseIndex + 0] = UVs[j * numVertexColumns + i];
                    UVOut[baseIndex + 1] = UVs[j * numVertexColumns + i + 1];
                    UVOut[baseIndex + 2] = UVs[(j - 1) * numVertexColumns + i];

                    Tris[baseIndex + 3] = Vertices[(j - 1) * numVertexColumns + i];
                    Tris[baseIndex + 4] = Vertices[j * numVertexColumns + i + 1];
                    Tris[baseIndex + 5] = Vertices[(j - 1) * numVertexColumns + i + 1];

                    UVOut[baseIndex + 3] = UVs[(j - 1) * numVertexColumns + i];
                    UVOut[baseIndex + 4] = UVs[j * numVertexColumns + i + 1];
                    UVOut[baseIndex + 5] = UVs[(j - 1) * numVertexColumns + i + 1];

                }
            }

            if (caps)       // note not sure about CCW on these..
            {
                int captri = 3 * numSideTris;
                int botvert = numVertexColumns * (numVertexRows - 1);

                bool leftSided = true;
                int leftIndex = 1;
                int rightIndex = numVertexColumns-2;
                int middleIndex = 0;

                for (int i = 0; i < numCapTris ; i++)
                {
                    if (i == 0)
                    {
                    }
                    else if (leftSided)
                    {
                        middleIndex = rightIndex;
                        rightIndex--;
                    }
                    else
                    {
                        middleIndex = leftIndex;
                        leftIndex++;
                    }
                    leftSided = !leftSided;

                   // System.Diagnostics.Debug.WriteLine("{0} {1} {2}", leftIndex, middleIndex, rightIndex);

                    Tris[captri + 0] = Vertices[leftIndex];
                    Tris[captri + 2] = Vertices[middleIndex];
                    Tris[captri + 1] = Vertices[rightIndex];

                    UVOut[captri + 0] = UVs[leftIndex];
                    UVOut[captri + 2] = UVs[middleIndex];
                    UVOut[captri + 1] = UVs[rightIndex];

                    Tris[captri + 3] = Vertices[botvert+leftIndex];
                    Tris[captri + 4] = Vertices[botvert+middleIndex];
                    Tris[captri + 5] = Vertices[botvert+rightIndex];

                    UVOut[captri + 3] = UVs[botvert+leftIndex];
                    UVOut[captri + 4] = UVs[botvert+middleIndex];
                    UVOut[captri + 5] = UVs[botvert+rightIndex];

                    captri += 6;
                }
            }

            return new Tuple<Vector4[], Vector2[]>(Tris, UVOut);
        }



    }
}