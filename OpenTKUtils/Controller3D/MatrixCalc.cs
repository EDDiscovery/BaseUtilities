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
        public bool InPerspectiveMode { get { return perspectivemode; } set { perspectivemode = value; } }
        public Matrix4 ModelMatrix { get { return modelmatrix; } }
        public Matrix4 ProjectionMatrix { get { return projectionmatrix; } }

        public float ZoomDistance { get; set; } = 1000F;       // distance that zoom=1 will be from the Position, in the direction of the camera.
        public float PerspectiveFarZDistance { get; set; } = 1000000.0f;        // perspective, set Z's for clipping
        public float PerspectiveNearZDistance { get; set; } = 1f;
        public float OrthographicDistance { get; set; } = 5000.0f;              // Orthographic, give scale

        public float EyeDistance(float zoom) { return ZoomDistance / zoom; }    // distance of eye from target position

        public Vector3 EyePosition { get; private set; }                        // after ModelMatrix

        private bool perspectivemode = true;
        private Matrix4 modelmatrix;
        private Matrix4 projectionmatrix;

        // Calculate the model matrix, which is the view onto the model
        // model matrix rotates and scales the model to the eye position

        public void CalculateModelMatrix(Vector3 position, Vector3 cameraDir, float zoom)       // We compute the model matrix, not opengl, because we need it before we do a Paint for other computations
        {
            Matrix4 flipy = Matrix4.CreateScale(new Vector3(1, -1, 1));
            Matrix4 preinverted;

            if (InPerspectiveMode)
            {
                Vector3 eye, normal;
                CalculateEyePosition(position, cameraDir, zoom, out eye, out normal);
                EyePosition = eye;
                preinverted = Matrix4.LookAt(eye, position, normal);   // from eye, look at target, with up giving the rotation of the look
                modelmatrix = Matrix4.Mult(flipy, preinverted);    //ORDER VERY important this one took longer to work out the order! replaces GL.Scale(1.0, -1.0, 1.0);
            }
            else
            {                                                               // replace open gl computation with our own.
                Matrix4 scale = Matrix4.CreateScale(zoom);
                Matrix4 offset = Matrix4.CreateTranslation(-position.X, -position.Y, -position.Z);
                Matrix4 rotcam = Matrix4.Identity;
                rotcam *= Matrix4.CreateRotationY((float)(-cameraDir.Y * Math.PI / 180.0f));
                rotcam *= Matrix4.CreateRotationX((float)((cameraDir.X - 90) * Math.PI / 180.0f));
                rotcam *= Matrix4.CreateRotationZ((float)(cameraDir.Z * Math.PI / 180.0f));

                preinverted = Matrix4.Mult(offset, scale);
                EyePosition = new Vector3(preinverted.Row0.X, preinverted.Row1.Y, preinverted.Row2.Z);          // TBD.. 
                preinverted = Matrix4.Mult(preinverted, rotcam);
                modelmatrix = preinverted;
            }
        }

        // used for calculating positions on the screen from pixel positions
        public Matrix4 GetResMat
        {
            get
            {
                Matrix4 resmat = Matrix4.Mult(modelmatrix, projectionmatrix);
                return resmat;
            }
        }

        // Projection matrix - projects the 3d model space to the 2D screen

        public void CalculateProjectionMatrix(float fov, int w, int h, out float znear)
        {
            if (InPerspectiveMode)
            {                                                                   // Fov, perspective, znear, zfar
                znear = 1.0F;
                projectionmatrix = Matrix4.CreatePerspectiveFieldOfView(fov, (float)w / h, PerspectiveNearZDistance, PerspectiveFarZDistance);
            }
            else
            {
                znear = -OrthographicDistance;
                float orthoheight = (OrthographicDistance / 5.0f) * h / w;
                projectionmatrix = Matrix4.CreateOrthographic(OrthographicDistance*2.0f/5.0f, orthoheight * 2.0F, -OrthographicDistance, OrthographicDistance);
            }
        }

        // calculate pos of eye, given position, CameraDir, zoom.

        private void CalculateEyePosition(Vector3 position, Vector3 cameraDir, float zoom, out Vector3 eye, out Vector3 normal)
        {
            Matrix3 transform = Matrix3.Identity;                   // identity nominal matrix, dir is in degrees
            transform *= Matrix3.CreateRotationZ((float)(cameraDir.Z * Math.PI / 180.0f));
            transform *= Matrix3.CreateRotationX((float)(cameraDir.X * Math.PI / 180.0f));
            transform *= Matrix3.CreateRotationY((float)(cameraDir.Y * Math.PI / 180.0f));
            // transform ends as the camera direction vector

            // calculate where eye is, relative to target. its 1000/zoom, rotated by camera rotation.  This is based on looking at 0,0,0.  
            // So this is the camera pos to look at 0,0,0
            Vector3 eyerel = Vector3.Transform(new Vector3(0.0f, -EyeDistance(zoom), 0.0f), transform);

            // rotate the up vector (0,0,1) by the eye camera dir to get a vector upwards from the current camera dir
            normal = Vector3.Transform(new Vector3(0.0f, 0.0f, 1.0f), transform);

            eye = position + eyerel;              // eye is here, the target pos, plus the eye relative position
            System.Diagnostics.Debug.WriteLine("Camera at " + eye + " looking at " + position + " dir " + cameraDir + " camera dist " + EyeDistance(zoom) + " zoom " + zoom);
        }

    }
}
