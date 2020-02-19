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
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public class GLDateTimePicker: GLForeDisplayBase
    {
        public Action<GLBaseControl> CheckChanged { get; set; } = null;     // not fired by programatically changing CheckState
        public Action<GLBaseControl> ValueChanged { get; set; } = null;     // not fired by programatically changing CheckState

        public DateTime Value { get { return datetimevalue; } set { datetimevalue = value; Invalidate(); } }

        public enum DateTimePickerFormat
        {
            Long = 1,
            Short = 2,
            Time = 4,
            Custom = 8
        }

        public string CustomFormat { get { return customformat; } set { customformat = value; Recalc(); InvalidateLayout(); } }
        public DateTimePickerFormat Format { get { return format; } set { SetFormat(value); InvalidateLayout(); } }
        public bool ShowUpDown { get { return showupdown; } set { showupdown = value; InvalidateLayout(); } }
        public bool ShowCheckBox { get { return showcheckbox; } set { showcheckbox = value; InvalidateLayout(); } }
        public bool ShowCalendar { get { return showcalendar; } set { showcalendar = value; InvalidateLayout(); } }

        public bool Checked { get { return checkbox.Checked; } set { checkbox.Checked = value; } }

        public Color SelectedColor { get { return selectedColor; } set { selectedColor = value; Invalidate(); } }

        public GLCheckBox checkbox = new GLCheckBox();              // access for setting colours
        public GLUpDownControl updown = new GLUpDownControl();
        public GLImage cal = new GLImage();


        public GLDateTimePicker(string name, Rectangle location, DateTime t, Color back) : base(name, location, back)
        {
            checkbox.CheckChanged += checkboxchanged;
            checkbox.BackColor = Color.Transparent;
            checkbox.AutoCheck = true;
            Add(checkbox);
            updown.ValueChanged += updownchanged;
            updown.BackColor = Color.Transparent;
            updown.Enabled = false;
            updown.FocusChanged += OnFocusChangedUpdown;
            Add(updown);
            cal.Image = Properties.Resources.Calendar;
            cal.BackColor = Color.Transparent;
            cal.ImageStretch = true;
            cal.MouseDown += calclicked;
            Add(cal);
            Focusable = true;
        }

        public GLDateTimePicker() : this("DTP?", DefaultWindowRectangle, DateTime.Now, DefaultControlBackColor)
        {
        }

        #region Layout paint

        public override void PerformRecursiveLayout()
        {
            base.PerformRecursiveLayout();

            int borderoffset = 0;
            int height = ClientHeight - 2;

            // NI versions stops repeated invalidates/layouts
            checkbox.VisibleNI = ShowCheckBox;
            checkbox.SetLocationSizeNI(location: new Point(2, borderoffset), size: new Size(height, height));

            updown.VisibleNI = ShowUpDown;
            updown.SetLocationSizeNI(location: new Point(ClientRectangle.Width - height - 2, borderoffset), size: new Size(height, height));

            cal.VisibleNI = false; // tbd
            int ch = ClientHeight * 3 / 5;
            cal.SetLocationSizeNI(location: new Point(updown.Left - 4 - cal.Width, (ClientHeight / 2 - ch/2)), size: new Size(ch * 20 / 12, ch));

            xstart = (showcheckbox ? (checkbox.Right + 2) : 2);

            Recalc();   // cause anything might have changed, like fonts
        }

        // called after the background of the panel has been drawn - so it will be clear to write.

        protected override void Paint(Rectangle area, Graphics gr)
        {
            using (Brush textb = new SolidBrush(this.ForeColor))
            {
                for (int i = 0; i < partlist.Count; i++)
                {
                    Parts p = partlist[i];

                    string t = (p.ptype == PartsTypes.Text) ? p.maxstring : datetimevalue.ToString(p.format);

                    if (i == selectedpart)
                    {
                        using (Brush br = new SolidBrush(this.SelectedColor))
                            gr.FillRectangle(br, new Rectangle(area.Left + p.xpos + xstart, area.Y, p.endx - p.xpos, area.Height));
                    }

                    gr.DrawString(t, this.Font, textb, new Point(area.Left + p.xpos + xstart, area.Y + 2));
                }
            }
        }

        #endregion

        #region UI

        public override void OnMouseDown(GLMouseEventArgs e)
        {
            base.OnMouseDown(e);

            if ( !e.Handled )
            {
                for (int i = 0; i < partlist.Count; i++)
                {
                    if (partlist[i].ptype >= PartsTypes.Day && e.X >= partlist[i].xpos + xstart && e.X <= partlist[i].endx + xstart)
                    {
                        //System.Diagnostics.Debug.WriteLine(".. on " + i);
                        if (selectedpart == i)      // click again, increment
                        {
                            UpDown((e.Button == GLMouseEventArgs.MouseButtons.Right) ? -1 : 1);
                            break;
                        }
                        else
                        {
                            selectedpart = i;
                            updown.Enabled = true;
                            Invalidate();
                            break;
                        }
                    }
                }
            }
        }

        public override void OnMouseLeave(GLMouseEventArgs e)
        {
            base.OnMouseLeave(e);
        }

        public override void OnKeyDown(GLKeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!e.Handled)
            {
                System.Diagnostics.Debug.WriteLine("Key down" + e.KeyCode);

                if (e.KeyCode == System.Windows.Forms.Keys.Up)
                    UpDown(1);
                else if (e.KeyCode == System.Windows.Forms.Keys.Down)
                    UpDown(-1);
                else if (e.KeyCode == System.Windows.Forms.Keys.Left && selectedpart > 0)
                {
                    int findprev = selectedpart - 1; // back 1
                    while (findprev >= 0 && partlist[findprev].ptype < PartsTypes.Day)       // back until valid
                        findprev--;

                    if (findprev >= 0)
                    {
                        selectedpart = findprev;
                        Invalidate();
                    }
                }
                else if (e.KeyCode == System.Windows.Forms.Keys.Right && selectedpart < partlist.Count - 1)
                {
                    int findnext = selectedpart + 1; // fwd 1
                    while (findnext < partlist.Count && partlist[findnext].ptype < PartsTypes.Day)       // fwd until valid
                        findnext++;

                    if (findnext < partlist.Count)
                    {
                        selectedpart = findnext;
                        Invalidate();
                    }
                }
                else if (e.KeyCode >= System.Windows.Forms.Keys.D0 && e.KeyCode <= System.Windows.Forms.Keys.D9)
                {
                    keybuffer += (char)((e.KeyCode - System.Windows.Forms.Keys.D0) + '0');
                    if (!TryNum(keybuffer))
                    {
                        keybuffer = "";
                        keybuffer += (char)((e.KeyCode - System.Windows.Forms.Keys.D0) + '0');
                        TryNum(keybuffer);
                    }
                }
            }
        }


        #endregion

        #region Parts

        void SetFormat(DateTimePickerFormat f)
        {
            format = f;
            if (format == DateTimePickerFormat.Long)
                customformat = CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern.Trim();
            else if (format == DateTimePickerFormat.Short)
                customformat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Trim();
            else if (format == DateTimePickerFormat.Time)
                customformat = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern.Trim();

            Recalc();
        }

        void Recalc()
        {
            if (Font == null)
                return; 
            //System.Diagnostics.Debug.WriteLine(Name + " Format " + customformat);
            partlist.Clear();

            int xpos = 0;

            using (Bitmap b = new Bitmap(1, 1))
            {
                using (Graphics e = Graphics.FromImage(b))
                {
                    string fmt = customformat;

                    while (fmt.Length > 0)
                    {
                        Parts p = null;

                        if (fmt[0] == '\'')
                        {
                            int index = fmt.IndexOf('\'', 1);
                            if (index == -1)
                                index = fmt.Length;

                            p = new Parts() { maxstring = fmt.Substring(1, index - 1), ptype = PartsTypes.Text };
                            fmt = (index < fmt.Length) ? fmt.Substring(index + 1) : "";
                        }
                        else if (fmt.StartsWith("dddd"))
                            p = Make(ref fmt, 4, PartsTypes.DayName, Maxlengthof(CultureInfo.CurrentCulture.DateTimeFormat.DayNames));
                        else if (fmt.StartsWith("ddd"))
                            p = Make(ref fmt, 3, PartsTypes.DayName, Maxlengthof(CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames));
                        else if (fmt.StartsWith("dd"))
                            p = Make(ref fmt, 2, PartsTypes.Day, "99");
                        else if (fmt.StartsWith("d"))
                            p = Make(ref fmt, 1, PartsTypes.Day, "99");
                        else if (fmt.StartsWith("MMMM"))
                            p = Make(ref fmt, 4, PartsTypes.Month, Maxlengthof(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames));
                        else if (fmt.StartsWith("MMM"))
                            p = Make(ref fmt, 3, PartsTypes.Month, Maxlengthof(CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames));
                        else if (fmt.StartsWith("MM"))
                            p = Make(ref fmt, 2, PartsTypes.Month, "99");
                        else if (fmt.StartsWith("M"))
                            p = Make(ref fmt, 1, PartsTypes.Month, "99");
                        else if (fmt.StartsWith("HH", StringComparison.InvariantCultureIgnoreCase))
                            p = Make(ref fmt, 2, PartsTypes.Hours, "99");
                        else if (fmt.StartsWith("H", StringComparison.InvariantCultureIgnoreCase))
                            p = Make(ref fmt, 1, PartsTypes.Hours, "99");
                        else if (fmt.StartsWith("mm"))
                            p = Make(ref fmt, 2, PartsTypes.Mins, "99");
                        else if (fmt.StartsWith("m"))
                            p = Make(ref fmt, 1, PartsTypes.Mins, "99");
                        else if (fmt.StartsWith("ss"))
                            p = Make(ref fmt, 2, PartsTypes.Seconds, "99");
                        else if (fmt.StartsWith("s"))
                            p = Make(ref fmt, 1, PartsTypes.Seconds, "99");
                        else if (fmt.StartsWith("tt"))
                            p = Make(ref fmt, 2, PartsTypes.AmPm, "AM");
                        else if (fmt.StartsWith("t"))
                            p = Make(ref fmt, 1, PartsTypes.AmPm, "AM");
                        else if (fmt.StartsWith("yyyyy"))
                            p = Make(ref fmt, 5, PartsTypes.Year, "99999");
                        else if (fmt.StartsWith("yyyy"))
                            p = Make(ref fmt, 4, PartsTypes.Year, "9999");
                        else if (fmt.StartsWith("yyy"))
                            p = Make(ref fmt, 3, PartsTypes.Year, "9999");
                        else if (fmt.StartsWith("yy"))
                            p = Make(ref fmt, 2, PartsTypes.Year, "99");
                        else if (fmt.StartsWith("y"))
                            p = Make(ref fmt, 1, PartsTypes.Year, "99");
                        else if (fmt[0] != ' ')
                        {
                            p = new Parts() { maxstring = fmt.Substring(0, 1), ptype = PartsTypes.Text };
                            fmt = fmt.Substring(1).Trim();
                        }
                        else
                            fmt = fmt.Substring(1).Trim();

                        if (p != null)
                        {
                            p.xpos = xpos;
                            SizeF sz = e.MeasureString(p.maxstring, this.Font);
                            int width = (int)(sz.Width + 1);
                            p.endx = xpos + width;
                            xpos = p.endx + 1;
                            partlist.Add(p);
                        }
                    }
                }
            }
        }

        Parts Make(ref string c, int len, PartsTypes t, string maxs)
        {
            Parts p = new Parts() { format = c.Substring(0, len) + " ", ptype = t, maxstring = maxs }; // space at end seems to make multi ones work
            c = c.Substring(len);
            return p;
        }

        string Maxlengthof(string[] a)
        {
            string m = "";
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].Length > m.Length)
                    m = a[i];
            }
            return m;
        }

        #endregion

        #region Implementation

        public void UpDown(int dir)
        {
            if (selectedpart != -1)
            {
                Parts p = partlist[selectedpart];
                if (p.ptype == PartsTypes.Day)
                    datetimevalue = datetimevalue.AddDays(dir);
                else if (p.ptype == PartsTypes.Month)
                    datetimevalue = datetimevalue.AddMonths(dir);
                else if (p.ptype == PartsTypes.Year)
                    datetimevalue = datetimevalue.AddYears(dir);
                else if (p.ptype == PartsTypes.Hours)
                    datetimevalue = datetimevalue.AddHours(dir);
                else if (p.ptype == PartsTypes.Mins)
                    datetimevalue = datetimevalue.AddMinutes(dir);
                else if (p.ptype == PartsTypes.Seconds)
                    datetimevalue = datetimevalue.AddSeconds(dir);
                else if (p.ptype == PartsTypes.AmPm)
                    datetimevalue = datetimevalue.AddHours((datetimevalue.Hour >= 12) ? -12 : 12);
                else
                    return;

                OnValueChanged();
                Invalidate();
            }
        }

        private bool TryNum(string s)
        {
            int newvalue;
            int.TryParse(s, out newvalue);
            DateTime nv = DateTime.Now;

            Parts p = partlist[selectedpart];

            try
            {
                if (p.ptype == PartsTypes.Day)
                    nv = new DateTime(datetimevalue.Year, datetimevalue.Month, newvalue, datetimevalue.Hour, datetimevalue.Minute, datetimevalue.Second, datetimevalue.Kind);
                else if (p.ptype == PartsTypes.Month)
                    nv = new DateTime(datetimevalue.Year, newvalue, datetimevalue.Day, datetimevalue.Hour, datetimevalue.Minute, datetimevalue.Second, datetimevalue.Kind);
                else if (p.ptype == PartsTypes.Year)
                    nv = new DateTime(newvalue, datetimevalue.Month, datetimevalue.Day, datetimevalue.Hour, datetimevalue.Minute, datetimevalue.Second, datetimevalue.Kind);
                else if (p.ptype == PartsTypes.Hours)
                    nv = new DateTime(datetimevalue.Year, datetimevalue.Month, datetimevalue.Day, newvalue, datetimevalue.Minute, datetimevalue.Second, datetimevalue.Kind);
                else if (p.ptype == PartsTypes.Mins)
                    nv = new DateTime(datetimevalue.Year, datetimevalue.Month, datetimevalue.Day, datetimevalue.Hour, newvalue, datetimevalue.Second, datetimevalue.Kind);
                else if (p.ptype == PartsTypes.Seconds)
                    nv = new DateTime(datetimevalue.Year, datetimevalue.Month, datetimevalue.Day, datetimevalue.Hour, datetimevalue.Minute, newvalue, datetimevalue.Kind);

                datetimevalue = nv;

                OnValueChanged();
                Invalidate();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void checkboxchanged(GLBaseControl b)
        {
            OnCheckBoxChanged();
        }

        private void updownchanged(GLBaseControl b, GLMouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Up down");
            if (e.Delta > 0)
                UpDown(1);
            else
                UpDown(-1);
        }

        private System.Windows.Forms.MonthCalendar calendar = new System.Windows.Forms.MonthCalendar();

        private void calclicked(Object b, GLMouseEventArgs e)
        { 
            // tbd
        }

        protected virtual void OnCheckBoxChanged()
        {
            CheckChanged?.Invoke(this);
        }

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this);
        }

        public override void OnFocusChanged(bool focused, GLBaseControl fromto)
        {
            base.OnFocusChanged(focused, fromto);
            System.Diagnostics.Debug.WriteLine("DTP Focus chg {0} {1}", focused, fromto?.Name);

            if (!focused && (fromto != updown && fromto != this))
            {
                selectedpart = -1;
                updown.Enabled = false;
                Invalidate();
            }
        }
        public void OnFocusChangedUpdown(Object s, bool focused, GLBaseControl fromto)
        {
            OnFocusChanged(focused, fromto);
        }

        private DateTime datetimevalue = DateTime.Now;
        private DateTimePickerFormat format = DateTimePickerFormat.Long;
        private string customformat = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;
        private bool showupdown = false;
        private bool showcheckbox = false;
        private bool showcalendar = false;

        enum PartsTypes { Text, DayName, Day, Month, Year, Hours, Mins, Seconds, AmPm }
        class Parts
        {
            public PartsTypes ptype;
            public string maxstring;
            public string format;
            public int xpos;
            public int endx;
        };

        List<Parts> partlist = new List<Parts>();
        int selectedpart = -1;

        private int xstart = 0;                             // where the text starts


        string keybuffer;
        private Color selectedColor = Color.Green;

        #endregion

    }
}
