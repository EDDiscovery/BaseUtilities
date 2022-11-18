/*
 * Copyright © 2022 - 2022 EDDiscovery development team
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
using System.Windows.Forms;

public static partial class ControlHelpersStaticFunc
{
    public class ControlDragger
    {
        public List<Control> DragControls { get; set; } = null;
        public List<Point> OrgPos { get; set; } = null;
        public Point RefPosition { get; set; }
        public Rectangle ClipArea { get; set; }
        public bool DragStarted { get { return DragControls != null; } }
        public bool InMoveDrag { get; private set; } 

        public void Start(IEnumerable<Control> controls, bool movetofront, Point refpos, Rectangle cliparea)
        {
            DragControls = new List<Control>(controls);
            if ( movetofront)
            {
                int ci = 0;
                foreach (var c in DragControls)
                    c.Parent.Controls.SetChildIndex(c,ci++);
            }

            OrgPos = DragControls.Select(x => x.Location).ToList();
            RefPosition = refpos;
            ClipArea = cliparea;
            //System.Diagnostics.Debug.WriteLine($"Controls {DragControls.Count} grabbed at {RefPosition} clipped to {ClipArea} Cursor at {Cursor.Position}");
            //for (int i = 0; i < DragControls.Count; i++)   System.Diagnostics.Debug.WriteLine($".. Controls {DragControls[i].Name} pos {OrgPos[i]}");

        }

        // Move position of controls.  We callback to allow for special handling of the location change.
        public void MouseMoved(Point pos, Action<Control,Point> changeloc)
        {
            if (DragStarted)
            {
                if (InMoveDrag)
                {
                    var clippedpos = pos.ClipTo(ClipArea);
                    Point offset = new Point(clippedpos.X - RefPosition.X, clippedpos.Y - RefPosition.Y);
               //     System.Diagnostics.Debug.WriteLine($"Controls moved to {pos} clipped {clippedpos} mouse offset {offset} OrgPos {OrgPos[0].X} {OrgPos[0].Y} Cursor {Cursor.Position}");

                    for (int i = 0; i < DragControls.Count; i++)
                    {
                  //      System.Diagnostics.Debug.WriteLine($"Changed {DragControls[i].Name} to {new Point(OrgPos[i].X + offset.X, OrgPos[i].Y + offset.Y)}");
                        changeloc( DragControls[i], new Point(OrgPos[i].X + offset.X, OrgPos[i].Y + offset.Y));
                    }
                }
                else if (Math.Abs(RefPosition.X - pos.X) >= 4 || Math.Abs(RefPosition.Y - pos.Y) >= 4)
                {
                    InMoveDrag = true;
                    // System.Diagnostics.Debug.WriteLine($"Controls moved mouse outside initial area");
                }
            }
        }

        public void End()
        {
            DragControls = null;
            OrgPos = null;
        }

    }
}
