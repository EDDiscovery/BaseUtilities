/*
 * Copyright © 2019 Robbyxp1 @ github.com
 * Part of the EDDiscovery Project
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public abstract class GLImageBase : GLBaseControl
    {
        public Image Image { get { return image; } set { image = value; Invalidate(); } }
        public bool ImageStretch { get { return imagestretch; } set { imagestretch = value; Invalidate(); } }
        public System.Drawing.ContentAlignment ImageAlign { get { return imagealign; } set { imagealign = value; Invalidate(); } }

        public float DisabledScaling
        {
            get { return disabledScaling; }
            set
            {
                if (float.IsNaN(value) || float.IsInfinity(value))
                    return;
                else if (disabledScaling != value)
                {
                    disabledScaling = value;
                    Invalidate();
                }
            }
        }

        public void SetDrawnBitmapRemapTable(System.Drawing.Imaging.ColorMap[] remap, float[][] colormatrix = null)
        {
            if (remap == null)
                throw new ArgumentNullException(nameof(remap));

            drawnImageAttributesEnabled?.Dispose();
            drawnImageAttributesDisabled?.Dispose();

            ControlHelpersStaticFunc.ComputeDrawnPanel(out drawnImageAttributesEnabled, out drawnImageAttributesDisabled, disabledScaling, remap, colormatrix);
            Invalidate();
        }

        private Image image;
        private bool imagestretch { get; set; } = false;
        private System.Drawing.ContentAlignment imagealign { get; set; } = ContentAlignment.MiddleCenter;
        private float disabledScaling = 0.5F;

        protected System.Drawing.Imaging.ImageAttributes drawnImageAttributesEnabled = null;         // Image override (colour etc) for background when using Image while Enabled.
        protected System.Drawing.Imaging.ImageAttributes drawnImageAttributesDisabled = null;        // Image override (colour etc) for background when using Image while !Enabled.

        protected void DrawImage(Image image, Rectangle box, Graphics g)
        {
            Size isize = ImageStretch ? box.Size : image.Size;
            Rectangle drawarea = ImageAlign.ImagePositionFromContentAlignment(box, isize,true,true);

            //System.Diagnostics.Debug.WriteLine("Image for " + Name + " " + Enabled + " " + DrawnImageAttributesEnabled);

            if (drawnImageAttributesEnabled != null)
                g.DrawImage(image, drawarea, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, (Enabled) ? drawnImageAttributesEnabled : drawnImageAttributesDisabled);
            else
                g.DrawImage(image, drawarea, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
        }
    }

    public class GLImage : GLImageBase
    {
        public override void Paint(Bitmap bmp, Rectangle area, Graphics gr)
        {
            base.DrawImage(Image, area, gr);
        }
    }
}
