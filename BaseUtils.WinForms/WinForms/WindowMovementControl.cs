/*
 * Copyright © 2019 EDDiscovery development team
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

using System;
using System.Drawing;
using System.Windows.Forms;

namespace BaseUtils
{
    // this tries to mimic the win key left/right functionality

    public class WindowMovementControl
    {
        Control ctrl;
        public WindowMovementControl(Control c)
        {
            ctrl = c;
            centreloc.X = int.MinValue;
            foreach (var s in Screen.AllScreens)
            {
               // System.Diagnostics.Debug.WriteLine(s.DeviceName + " " + s.WorkingArea + " B: " + s.Bounds);
            }

        }

        public bool OnLeftEdge(Screen scr)
        {
            return scr.WorkingArea.Left == ctrl.Left;
        }

        public bool OnRightEdge(Screen scr)
        {
            return scr.WorkingArea.Right == ctrl.Right;
        }

        public Tuple<Point, Size> Left(Screen scr)
        {
            Point p = new Point(scr.WorkingArea.Left, scr.WorkingArea.Top);
            Size s = new Size(scr.WorkingArea.Width/2, scr.WorkingArea.Height);
            return new Tuple<Point, Size>(p, s);
        }

        public Tuple<Point, Size> Right(Screen scr)
        {
            Point p = new Point(scr.WorkingArea.Left + scr.WorkingArea.Width / 2, scr.WorkingArea.Top);
            Size s = new Size(scr.WorkingArea.Width/2, scr.WorkingArea.Height);
            return new Tuple<Point, Size>(p, s);
        }

        private Point centreloc;
        private Size centresize;

        public void GotoCentre(Screen scr)
        {
            if (centreloc.X == int.MinValue)
            {
                centreloc = new Point(scr.WorkingArea.Width / 8, scr.WorkingArea.Height / 8);
                centresize = new Size(scr.WorkingArea.Width * 6 / 8, scr.WorkingArea.Height * 6 / 8);
            }

            ctrl.Location = new Point( scr.Bounds.Left + Math.Min(centreloc.X,scr.WorkingArea.Width-64) , 
                                       scr.Bounds.Top + Math.Min(centreloc.Y,scr.WorkingArea.Height-64));
            ctrl.Size = centresize;
        }

        public void ResetCentre()
        {
            centreloc.X = int.MinValue;
        }

        public void RecordCentre(Screen scr)
        {
            if (centreloc.X == int.MinValue)
            {
                centreloc = new Point(ctrl.Left - scr.Bounds.Left, ctrl.Top - scr.Bounds.Top);
                centresize = ctrl.Size;
                System.Diagnostics.Debug.WriteLine("Rec " + centreloc + " " + centresize);
            }
        }

        public void Align( bool goleft)
        {
            Screen cur = Screen.FromControl(ctrl);
            var l = Left(cur);
            var r = Right(cur);

            bool setleft = false;
            bool setright = false;

            if (OnLeftEdge(cur))
            {
                if (!goleft)
                    GotoCentre(cur);
                else
                {
                    Screen next = FindNextScreenUp(cur);

                    if (next == null)
                        next = FindNextScreenLeftBottom(cur);

                    if (next != null)
                    {
                        setright = true;
                        r = Right(next);
                    }
                }
            }
            else if (OnRightEdge(cur))
            {
                if (goleft)
                    GotoCentre(cur);
                else
                {
                    Screen next = FindNextScreenDown(cur);

                    if (next == null)
                        next = FindNextScreenRightTop(cur);

                    if (next != null)
                    {
                        setleft = true;
                        l = Left(next);
                    }
                }
            }
            else
            {
                RecordCentre(cur);
                setleft = goleft;
                setright = !goleft;
            }

            if (setleft)
            {
                ctrl.Location = l.Item1;
                ctrl.Size = l.Item2;
            }
            else if ( setright )
            {
                ctrl.Location = r.Item1;
                ctrl.Size = r.Item2;
            }
        }

        public void AlignBot()
        {
            Screen cur = Screen.FromControl(ctrl);

            if (OnLeftEdge(cur) || OnRightEdge(cur))
            {
                ctrl.Location = new Point(ctrl.Left, cur.WorkingArea.Top+ cur.WorkingArea.Height / 2);
                ctrl.Size = new Size(cur.WorkingArea.Width / 2, cur.WorkingArea.Height / 2);
            }
        }

        public void AlignTop()
        {
            Screen cur = Screen.FromControl(ctrl);
            var l = Left(cur);
            var r = Right(cur);

            if (OnLeftEdge(cur) || OnRightEdge(cur))
            {
                ctrl.Location = new Point(ctrl.Left, cur.WorkingArea.Top);
                ctrl.Size = new Size(cur.WorkingArea.Width / 2, cur.WorkingArea.Height / 2);
            }
        }

        public Screen FindNextScreenRightDown(Screen cur)
        {
            Point righttop = new Point(cur.Bounds.Right, cur.Bounds.Top);
            int mostnegfound = int.MaxValue;

            Screen found = null;

            foreach (var s in Screen.AllScreens)
            {
                if (cur != s)
                {
                    if (s.Bounds.Left == righttop.X + 1)
                    {
                        if (s.Bounds.Top < mostnegfound)
                        {
                            found = s;
                            mostnegfound = s.Bounds.Top;
                        }
                    }
                }
            }

            return found;
        }

        public Screen FindNextScreenDown(Screen cur)
        {
            foreach (var s in Screen.AllScreens)
            {
                if (s.Bounds.Top == cur.Bounds.Bottom)
                    return s;
            }

            return null;
        }

        public Screen FindNextScreenUp(Screen cur)
        {
            foreach (var s in Screen.AllScreens)
            {
                if (s.Bounds.Bottom == cur.Bounds.Top)
                    return s;
            }

            return null;
        }

        public Screen FindNextScreenRightTop(Screen cur)
        {
            int mostnegfound = int.MaxValue;
            Screen next = null;
            foreach (var s in Screen.AllScreens)
            {
                if (s.Bounds.Left == cur.Bounds.Right)
                {
                    if (s.Bounds.Top < mostnegfound)
                    {
                        mostnegfound = s.Bounds.Top;
                        next = s;
                    }
                }
            }

            return next;
        }

        public Screen FindNextScreenLeftBottom(Screen cur)
        {
            int mostposfound = int.MinValue;
            Screen next = null;
            foreach (var s in Screen.AllScreens)
            {
                if (s.Bounds.Right == cur.Bounds.Left)
                {
                    if (s.Bounds.Top > mostposfound)
                    {
                        mostposfound = s.Bounds.Top;
                        next = s;
                    }
                }
            }

            return next;
        }
    }
}

