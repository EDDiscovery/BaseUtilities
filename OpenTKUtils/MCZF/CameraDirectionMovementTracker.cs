/*
 * Copyright 2016-2019 EDDiscovery development team
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

namespace OpenTKUtils.Common
{
    public class CameraDirectionMovementTracker       // keeps track of previous and works out how to present bitmaps
    {
        public Vector3 LastPosition;
        public Vector3 LastCameraDir;
        public float LastZoom;
        public Vector3 LastCameraGrossDir;               // for gross direction camera adjustments

        public Vector3 Rotation = new Vector3(0, 0, 0);

        public bool CameraDirChanged { get; private set; }
        public float MinimumCameraDirChange { get; set; } = 1.0f;

        public bool CameraDirGrossChanged { get; private set; }
        public float MinimumGrossCameraDirChange { get; set; } = 10.0f;

        public bool CameraMoved { get; private set; }
        public float MinimumCameraMoved { get; set; } = 0.05f;

        public bool CameraZoomed { get; private set; }
        public float MinimumZoomMoved { get; set; } = 0.05f;

        public bool AnythingChanged { get { return CameraDirChanged || CameraMoved || CameraZoomed; } }         //DIR is more sensitive than gross, so no need to use

        public void Update(Vector3 cameraDir, Vector3 position, float zoom)
        {
            CameraDirChanged = Vector3.Subtract(LastCameraDir, cameraDir).LengthSquared >= MinimumCameraDirChange;

            if (CameraDirChanged)
            {
                LastCameraDir = cameraDir;
            }

            CameraDirGrossChanged = Vector3.Subtract(LastCameraGrossDir, cameraDir).LengthSquared >= MinimumGrossCameraDirChange;

            if ( CameraDirGrossChanged )
            {
                LastCameraGrossDir = cameraDir;
            }

            CameraMoved = Vector3.Subtract(LastPosition, position).LengthSquared >= MinimumCameraMoved;

            if ( CameraMoved )
                LastPosition = position;

            float zoomfact = zoom / LastZoom;

            CameraZoomed = (zoomfact >= (1.0+ MinimumZoomMoved) || zoomfact <= (1.0- MinimumZoomMoved));     // prevent too small zoom causing a repaint

            if ( CameraZoomed )
                LastZoom = zoom;

            Rotation = LastCameraDir;
            Rotation.X = -Rotation.X;       // invert to face
            Rotation.Z = 0;                 // no Z, not used much, and cause the other two axis to spin .. would need more work to understand
        }

        public void ForceMoveChange()
        {
            LastPosition = new Vector3(float.MinValue, 0, 0);
        }

        public void SetGrossChanged()       // tell it that we dealt with it and move gross back to last camera
        {
            LastCameraGrossDir = LastCameraDir;
        }
    }


}
