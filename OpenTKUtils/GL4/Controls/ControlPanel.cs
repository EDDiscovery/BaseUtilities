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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public class GLPanel : GLBaseControl
    {
        public GLPanel(string name, Rectangle location, Color back) : base(name, location, back)
        {
        }

        public GLPanel() : this("P?", DefaultWindowRectangle,DefaultControlBackColor)
        {
        }

        public GLPanel(string name, DockingType type, float dockpercent, Color back) : base(name, DefaultWindowRectangle, back)
        {
            Dock = type;
            DockPercent = dockpercent;
        }

        public GLPanel(string name, Size sizep, DockingType type, float dockpercentage, Color back) : base(name, DefaultWindowRectangle, back)
        {
            Dock = type;
            DockPercent = dockpercentage;
            SetLocationSizeNI(size: sizep);
        }
    }
}


