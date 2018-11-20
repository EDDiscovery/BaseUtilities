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
using System;
using System.Diagnostics;

namespace OpenTKUtils.Common
{
    public class MatrixCalc
    {
        public bool InPerspectiveMode { get; set; } = true;
        public Matrix4 ModelMatrix { get; private set; }
        public Matrix4 ProjectionMatrix { get; private set; }
        public Matrix4 ProjectionModelMatrix { get; private set; }

        public Matrix4 InvEyeRotate { get; private set; }

        public float ZoomDistance { get; set; } = 1000F;       // distance that zoom=1 will be from the Position, in the direction of the camera.
        public float PerspectiveFarZDistance { get; set; } = 1000000.0f;        // perspective, set Z's for clipping
        public float PerspectiveNearZDistance { get; set; } = 1f;
        public float OrthographicDistance { get; set; } = 5000.0f;              // Orthographic, give scale

        public float CalcEyeDistance(float zoom) { return ZoomDistance / zoom; }    // distance of eye from target position

        public Vector3 TargetPosition { get; private set; }                     // after ModelMatrix
        public Vector3 EyePosition { get; private set; }                        // after ModelMatrix
        public float EyeDistance { get; private set; }                          // after ModelMatrix

        // Calculate the model matrix, which is the model translated to world space then to view space..
        // Model matrix does not have any Y inversion(axis flip around x).
        // opengl normal model has +z towards viewer, y up, x to right.  We want +z away, y up, x to right
        // we use the normal in lookup to position the axis to +z away, y down, x to right (180 rot).  then use the projection model to flip y.

        public void CalculateModelMatrix(Vector3 position, Vector3 cameraDir, float zoom)       // We compute the model matrix, not opengl, because we need it before we do a Paint for other computations
        {
            TargetPosition = position;      // record for shader use

            if (InPerspectiveMode)
            {
                Matrix3 transform = Matrix3.Identity;                   // identity nominal matrix, dir is in degrees

                // we rotate the identity matrix by the camera direction
                // .x and .y values are set by our axis orientations..
                // .x translates around the x axis, x = 0 = to +Y, x = 90 on the x/z plane, x = 180 = to -Y
                // .y translates around the y axis. y= 0 = to +Z (forward), y = 90 = to +x (look from left), y = -90 = to -x (look from right), y = 180 = look back
                // .z rotates the camera.

                transform *= Matrix3.CreateRotationX((float)(cameraDir.X * Math.PI / 180.0f));
                transform *= Matrix3.CreateRotationY((float)(cameraDir.Y * Math.PI / 180.0f));
                transform *= Matrix3.CreateRotationZ((float)(cameraDir.Z * Math.PI / 180.0f));

                //System.Diagnostics.Debug.WriteLine("XY transform " + transform);

                EyeDistance = CalcEyeDistance(zoom);

                Vector3 eyerel = Vector3.Transform(new Vector3(0,-EyeDistance,0), transform);       // the 0,-E,0 sets the axis of the system..

                Vector3 normal = Vector3.Transform(new Vector3(0.0f, 0.0f, 1.0f), transform);       // 0,0,1 also sets the axis - this whats make .x/.y address the x/y rotations

                EyePosition = position + eyerel;              // eye is here, the target pos, plus the eye relative position

                System.Diagnostics.Debug.WriteLine("Eye " + EyePosition + " target " + position + " dir " + cameraDir + " camera dist " + CalcEyeDistance(zoom) + " zoom " + zoom + " normal "+ normal);

                ModelMatrix = Matrix4.LookAt(EyePosition, position, normal);   // from eye, look at target, with up giving the rotation of the look
                    
                System.Diagnostics.Debug.WriteLine("... model matrix " + ModelMatrix);

                InvEyeRotate = Matrix4.Identity;                   // identity nominal matrix, dir is in degrees
                float xrot = -(180f - cameraDir.X);                // 180-cameradir, but inveye is applied before model/projection flips axis, so needs to be backwards
                float yrot = cameraDir.Y;                          // y maps directly

                System.Diagnostics.Debug.WriteLine("Inv rot x:" + xrot + " y:" + yrot);
                InvEyeRotate *= Matrix4.CreateRotationX(xrot.Radians());
                InvEyeRotate *= Matrix4.CreateRotationY(yrot.Radians());
            }
            else
            {            

                // TBD.. 
                Matrix4 scale = Matrix4.CreateScale(zoom);
                Matrix4 offset = Matrix4.CreateTranslation(-position.X, -position.Y, -position.Z);
                Matrix4 rotcam = Matrix4.Identity;
                rotcam *= Matrix4.CreateRotationY((float)(-cameraDir.Y * Math.PI / 180.0f));
                rotcam *= Matrix4.CreateRotationX((float)((cameraDir.X - 90) * Math.PI / 180.0f));
                rotcam *= Matrix4.CreateRotationZ((float)(cameraDir.Z * Math.PI / 180.0f));

                Matrix4 preinverted = Matrix4.Mult(offset, scale);
                EyePosition = new Vector3(preinverted.Row0.X, preinverted.Row1.Y, preinverted.Row2.Z);          // TBD.. 
                preinverted = Matrix4.Mult(preinverted, rotcam);
                ModelMatrix = preinverted;
            }

            ProjectionModelMatrix = Matrix4.Mult(ModelMatrix, ProjectionMatrix);        // order order order ! so important.
        }

        // used for calculating positions on the screen from pixel positions
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
                ProjectionMatrix = Matrix4.CreateOrthographic(OrthographicDistance*2.0f/5.0f, orthoheight * 2.0F, -OrthographicDistance, OrthographicDistance);
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
