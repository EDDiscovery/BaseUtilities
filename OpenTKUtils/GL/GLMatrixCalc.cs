/*
 * Copyright © 2015 - 2019 EDDiscovery development team
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
using System;
using System.Diagnostics;

namespace OpenTKUtils
{
    public class GLMatrixCalc
    {
        public bool InPerspectiveMode { get; set; } = true;                     // perspective mode

        public float PerspectiveFarZDistance { get; set; } = 500000.0f;         // perspective, set Z's for clipping.
        public float PerspectiveNearZDistance { get; set; } = 1f;               // Don't set this too small othersize depth testing stars going wrong.
        public float OrthographicDistance { get; set; } = 5000.0f;              // Orthographic, give scale

        // after Calculate model matrix
        public Vector3 EyePosition { get; private set; }                       
        public Vector3 TargetPosition { get; private set; }                    
        public float EyeDistance { get; private set; }                         
        public Matrix4 ModelMatrix { get; private set; }
        public Matrix4 ProjectionMatrix { get; private set; }
        public Matrix4 ProjectionModelMatrix { get; private set; }

        // Calculate the model matrix, which is the model translated to world space then to view space..
        // Model matrix does not have any Y inversion(axis flip around x).
        // opengl normal model has +z towards viewer, y up, x to right.  We want +z away, y up, x to right
        // we use the normal in lookup to position the axis to +z away, y down, x to right (180 rot).  then use the projection model to flip y.

        public void CalculateModelMatrix(Vector3 position, Vector3 eyeposition, Vector3 normal, int w, int h)       // We compute the model matrix, not opengl, because we need it before we do a Paint for other computations
        {
            TargetPosition = position;      // record for shader use
            EyePosition = eyeposition;
            EyeDistance = (position - eyeposition).Length;

            if (InPerspectiveMode)
            {
                ModelMatrix = Matrix4.LookAt(EyePosition, position, normal);   // from eye, look at target, with up giving the rotation of the look
            }
            else
            {
                float orthoheight = (OrthographicDistance / 5.0f) * h / w;  // this comes from the projection calculation, and allows us to work out the scale factor the eye vs lookat has

                Matrix4 scale = Matrix4.CreateScale(orthoheight/EyeDistance);    // create a scale based on eyedistance compensated for by the orth projection scaling

                Matrix4 offset = Matrix4.CreateTranslation(-position.X, 0, -position.Z);        // we offset by the negative of the position to give the central look
                offset = Matrix4.Mult(offset, scale);          // scale the translation

                Matrix4 rotcam = Matrix4.Identity;
                rotcam *= Matrix4.CreateRotationX((float)((90) * Math.PI / 180.0f));        // flip 90 along the x axis to give the top down view
                ModelMatrix = Matrix4.Mult(offset, rotcam);
            }

            ProjectionModelMatrix = Matrix4.Mult(ModelMatrix, ProjectionMatrix);        // order order order ! so important.
        }

        // used for calculating positions on the screen from pixel positions.  Remembering Apollo
        public Matrix4 GetResMat
        {
            get
            {
                return ProjectionModelMatrix;
            }
        }

        // Projection matrix - projects the 3d model space to the 2D screen
        // Has a Y flip so that +Y is going up.

        public void CalculateProjectionMatrix(float fov, int w, int h, out float znear)
        {
            if (InPerspectiveMode)
            {                                                                   // Fov, perspective, znear, zfar
                znear = 1.0F;
                ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(fov, (float)w / h, PerspectiveNearZDistance, PerspectiveFarZDistance);
            }
            else
            {
                znear = -OrthographicDistance;
                float orthoheight = (OrthographicDistance / 5.0f) * h / w;
                ProjectionMatrix = Matrix4.CreateOrthographic(OrthographicDistance * 2.0f / 5.0f, orthoheight * 2.0F, -OrthographicDistance, OrthographicDistance);
            }

            // we want the axis orientation with +z away from us, +x to right, +y upwards.
            // this means we need to rotate the normal opengl model (+z towards us) 180 degrees around x - therefore flip y
            // we do it here since its the end of the chain - easier to keep the rest in the other method
            // notice flipping y affects the order of vertex for winding.. the vertex models need to have a opposite winding order
            // to make the ccw cull test work.

            Matrix4 flipy = Matrix4.CreateScale(new Vector3(1, -1, 1));
            ProjectionMatrix = flipy * ProjectionMatrix;

            ProjectionModelMatrix = Matrix4.Mult(ModelMatrix, ProjectionMatrix);
        }
    }
}
