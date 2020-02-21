
/*
 * Copyright © 2017-2019 EDDiscovery development team
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
 * 
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public class GLFormConfigurable : GLForm
    {
        // returns GLformConfiguratble, Entry (or null) actioning, logical string action, callertag
        // logical string action:
        //  Return for number box, textBox.  
        //  Cancel for ending dialog,
        //  Validity:value for number boxes
        //  otherwise logical control name
        //  Escape for escape (Entry = null)

        public event Action<GLFormConfigurable, Entry, string, Object> Trigger;

        private List<Entry> entries;
        private Object callertag;

        // You give an array of Entries describing the controls
        // either added programatically by Add(entry) or via a string descriptor Add(string) (disabled for now)
        // Directly Supported Types (string name/base type)
        //      "button" ButtonExt, "textbox" TextBoxBorder, "checkbox" CheckBoxCustom, 
        //      "label" Label, "datetime" CustomDateTimePicker, 
        //      "numberboxdouble" NumberBoxDouble, "numberboxlong" NumberBoxLong, 
        //      "combobox" ComboBoxCustom
        // Or any type if you set controltype=null and set control field directly.
        // Set controlname, text,pos,size, tooltip
        // for specific type, set the other fields.
        // See action document for string descriptor format

        public class Entry
        {
            public string controlname;                  // logical name of control
            public Type controltype;                    // if non null, activate this type.  Else if null, control should be filled up with your specific type

            public string text;                         // for certain types, the text
            public System.Drawing.Point pos;
            public System.Drawing.Size size;
            public string tooltip;                      // can be null.

            // normal ones
            public Entry(string nam, Type c, string t, System.Drawing.Point p, System.Drawing.Size s, string tt = null, Object tag = null)
            {
                controltype = c; text = t; pos = p; size = s; tooltip = tt; controlname = nam; customdateformat = "long"; this.tag = tag;
            }

            public Entry( GLBaseControl ctrl, string tt = null, Object tag = null)
            {
                control = ctrl; tooltip = tt; this.tag = tag;
            }

            // ComboBox
            public Entry(string nam, string t, System.Drawing.Point p, System.Drawing.Size s, string tt, List<string> comboitems, Object tag = null)
            {
                controltype = typeof(GLComboBox); text = t; pos = p; size = s; tooltip = tt; controlname = nam; this.tag = null;
                comboboxitems = string.Join(",", comboitems);
            }

            public bool checkboxchecked;        // fill in for checkbox
            public bool clearonfirstchar;       // fill in for textbox
            public string comboboxitems;        // fill in for combobox. comma separ list.
            public string customdateformat;     // fill in for datetimepicker
            public double numberboxdoubleminimum = double.MinValue;   // for double box
            public double numberboxdoublemaximum = double.MaxValue;
            public long numberboxlongminimum = long.MinValue;   // for long box
            public long numberboxlongmaximum = long.MaxValue;
            public string numberboxformat;      // for both number boxes
            public int taborder = -1;           // tab order
            public Object tag;                  // for use by caller

            public GLBaseControl control; // if controltype is set, don't set.  If contrDaveoltype=null, pass your control type.
        }

        #region Public interface

        public GLFormConfigurable() : base("ConfigForm","ConfigForm",DefaultWindowRectangle)
        {
            entries = new List<Entry>();
        }

        public void Add(Entry e)               // add an entry..
        {
            entries.Add(e);
        }

        public Entry Last { get { return entries.Last(); } }

        public void Init(Point pos, string caption, string lname = null, Object callertag = null)
        {
            InitInt(pos, caption, lname, callertag);
        }

        public T GetControl<T>(string controlname) where T : GLBaseControl      // return value of dialog control
        {
            Entry t = entries.Find(x => x.controlname.Equals(controlname, StringComparison.InvariantCultureIgnoreCase));
            if (t != null)
                return (T)t.control;
            else
                return null;
        }

        public string Get(string controlname)      // return value of dialog control
        {
            Entry t = entries.Find(x => x.controlname.Equals(controlname, StringComparison.InvariantCultureIgnoreCase));
            if (t != null)
            {
                GLBaseControl c = t.control;
                if (c is GLMultiLineTextBox)
                    return (c as GLMultiLineTextBox).Text;
                else if (c is GLCheckBox)
                    return (c as GLCheckBox).Checked ? "1" : "0";
                else if (c is GLDateTimePicker)
                    return (c as GLDateTimePicker).Value.ToString("yyyy/dd/MM HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                else if (c is GLNumberBoxDouble)
                {
                    var cn = c as GLNumberBoxDouble;
                    return cn.IsValid ? cn.Value.ToStringInvariant() : "INVALID";
                }
                else if (c is GLNumberBoxLong)
                {
                    var cn = c as GLNumberBoxLong;
                    return cn.IsValid ? cn.Value.ToStringInvariant() : "INVALID";
                }
                else if (c is GLComboBox)
                {
                    GLComboBox cb = c as GLComboBox;
                    return (cb.SelectedIndex != -1) ? cb.Text : "";
                }
            }

            return null;
        }

        public double? GetDouble(string controlname)     // Null if not valid
        {
            Entry t = entries.Find(x => x.controlname.Equals(controlname, StringComparison.InvariantCultureIgnoreCase));
            if (t != null)
            {
                var cn = t.control as GLNumberBoxDouble;
                if (cn.IsValid)
                    return cn.Value;
            }
            return null;
        }

        public long? GetLong(string controlname)     // Null if not valid
        {
            Entry t = entries.Find(x => x.controlname.Equals(controlname, StringComparison.InvariantCultureIgnoreCase));
            if (t != null)
            {
                var cn = t.control as GLNumberBoxLong;
                if (cn.IsValid)
                    return cn.Value;
            }
            return null;
        }

        public DateTime? GetDateTime(string controlname)
        {
            Entry t = entries.Find(x => x.controlname.Equals(controlname, StringComparison.InvariantCultureIgnoreCase));
            if (t != null)
            {
                GLDateTimePicker c = t.control as GLDateTimePicker;
                if (c != null)
                    return c.Value;
            }

            return null;
        }

        public bool Set(string controlname, string value)      // set value of dialog control
        {
            Entry t = entries.Find(x => x.controlname.Equals(controlname, StringComparison.InvariantCultureIgnoreCase));
            if (t != null)
            {
                GLBaseControl c = t.control;
                if (c is GLTextBox)
                {
                    (c as GLTextBox).Text = value;
                    return true;
                }
                else if (c is GLMultiLineTextBox)
                {
                    (c as GLMultiLineTextBox).Text = value;
                    return true;
                }
                else if (c is GLCheckBox)
                {
                    (c as GLCheckBox).Checked = !value.Equals("0");
                    return true;
                }
                else if (c is GLComboBox)
                {
                    GLComboBox cb = c as GLComboBox;
                    if (cb.Items.Contains(value))
                    {
                        cb.Enabled = false;
                        cb.SelectedItem = value;
                        cb.Enabled = true;
                        return true;
                    }
                }
                else if (c is GLNumberBoxDouble)
                {
                    var cn = c as GLNumberBoxDouble;
                    double? v = value.InvariantParseDoubleNull();
                    if (v.HasValue)
                    {
                        cn.Value = v.Value;
                        return true;
                    }
                }
                else if (c is GLNumberBoxLong)
                {
                    var cn = c as GLNumberBoxLong;
                    long? v = value.InvariantParseLongNull();
                    if (v.HasValue)
                    {
                        cn.Value = v.Value;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool SetEnabled(string controlname, bool state)      // set enable state of dialog control
        {
            Entry t = entries.Find(x => x.controlname.Equals(controlname, StringComparison.InvariantCultureIgnoreCase));
            if (t != null)
            {
                var cn = t.control as GLBaseControl;
                cn.Enabled = state;
                return true;
            }
            else
                return false;
        }


        #endregion

        #region Implementation

        private void InitInt(System.Drawing.Point pos, string caption, string lname, Object callertag)
        {
            this.Name = lname;    // passed back to caller via trigger
            this.callertag = callertag;      // passed back to caller via trigger
            this.Text = caption;

            for (int i = 0; i < entries.Count; i++)
            {
                Entry ent = entries[i];

                bool oursmade = ent.control == null;

                if (oursmade)
                {
                    ent.control = (GLBaseControl)Activator.CreateInstance(ent.controltype);
                    ent.control.Size = ent.size;
                    ent.control.Location = ent.pos;
                    ent.control.Name = ent.controlname;
                }

                GLBaseControl c = ent.control;
                c.Tag = ent;     // point control tag at ent structure
                c.TabOrder = ent.taborder;

                if ( c is GLLabel)
                {
                    if (oursmade)
                        ((GLLabel)c).Text = ent.text;
                }
                else if ( c is GLMultiLineTextBox ) // also TextBox as its inherited
                {
                    GLMultiLineTextBox tb = c as GLMultiLineTextBox;

                    if (oursmade)
                    {
                        tb.Text = ent.text;
                        tb.ClearOnFirstChar = ent.clearonfirstchar;
                    }

                    tb.ReturnPressed += (box) =>        // only works for text box
                    {
                        Entry en = (Entry)(box.Tag);
                        Trigger?.Invoke(this, en, "Return", this.callertag);       // pass back the logical name of dialog, the name of the control, the caller tag
                    };

                }
                else if ( c is GLButton )
                { 
                    GLButton b = c as GLButton;
                    if (oursmade)
                        b.Text = ent.text;

                    b.Click += (sender, ev) =>
                    {
                        Entry en = (Entry)(((GLBaseControl)sender).Tag);
                        Trigger?.Invoke(this, en, en.controlname, this.callertag);       // pass back the logical name of dialog, the name of the control, the caller tag
                    };

                    b.Return += (sender) =>
                    {
                        Entry en = (Entry)(((GLBaseControl)sender).Tag);
                        Trigger?.Invoke(this, en, en.controlname, this.callertag);       // pass back the logical name of dialog, the name of the control, the caller tag
                    };
                }
                else if (c is GLCheckBox)
                {
                    GLCheckBox cb = c as GLCheckBox;
                    if (oursmade)
                        cb.Checked = ent.checkboxchecked;
                    cb.CheckChanged = (sender) =>
                    {
                        Entry en = (Entry)(((GLBaseControl)sender).Tag);
                        Trigger?.Invoke(this, en, en.controlname, this.callertag);       // pass back the logical name of dialog, the name of the control, the caller tag
                    };
                }
                else if (c is GLDateTimePicker)
                {
                    GLDateTimePicker dt = c as GLDateTimePicker;
                    if (oursmade)
                    {
                        DateTime t;
                        if (DateTime.TryParse(ent.text, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out t))     // assume local, so no conversion
                            dt.Value = t;
                    }

                    switch (ent.customdateformat.ToLowerInvariant())
                    {
                        case "short":
                            dt.Format = GLDateTimePicker.DateTimePickerFormat.Short;
                            break;
                        case "long":
                            dt.Format = GLDateTimePicker.DateTimePickerFormat.Long;
                            break;
                        case "time":
                            dt.Format = GLDateTimePicker.DateTimePickerFormat.Time;
                            break;
                        default:
                            dt.CustomFormat = ent.customdateformat;
                            break;
                    }
                }
                else if (c is GLComboBox)
                {
                    GLComboBox cb = c as GLComboBox;

                    if (oursmade)
                    {
                        cb.Items.AddRange(ent.comboboxitems.Split(','));
                        if (cb.Items.Contains(ent.text))
                            cb.SelectedItem = ent.text;
                    }

                    cb.SelectedIndexChanged += (sender) =>
                    {
                        GLBaseControl ctr = (GLBaseControl)sender;
                        if (ctr.Enabled)
                        {
                            Entry en = (Entry)(ctr.Tag);
                            Trigger?.Invoke(this, en, en.controlname, this.callertag);       // pass back the logical name of dialog, the name of the control, the caller tag
                        }
                    };

                }
                else if (c is GLNumberBoxDouble)
                {
                    GLNumberBoxDouble cb = c as GLNumberBoxDouble;

                    if (oursmade)
                    {
                        cb.Minimum = ent.numberboxdoubleminimum;
                        cb.Maximum = ent.numberboxdoublemaximum;
                        double? v = ent.text.InvariantParseDoubleNull();
                        cb.Value = v.HasValue ? v.Value : cb.Minimum;
                        if (ent.numberboxformat != null)
                            cb.Format = ent.numberboxformat;
                    }

                    cb.ReturnPressed += (box) =>
                    {
                        Entry en = (Entry)(box.Tag);
                        Trigger?.Invoke(this, en, "Return", this.callertag);       // pass back the logical name of dialog, the name of the control, the caller tag
                    };
                    cb.ValidityChanged += (box, s) =>
                    {
                        Entry en = (Entry)(box.Tag);
                        Trigger?.Invoke(this, en, "Validity:" + s.ToString(), this.callertag);       // pass back the logical name of dialog, the name of the control, the caller tag
                    };
                }
                else if (c is GLNumberBoxLong)
                {
                    GLNumberBoxLong cb = c as GLNumberBoxLong;
                    if (oursmade)
                    {
                        cb.Minimum = ent.numberboxlongminimum;
                        cb.Maximum = ent.numberboxlongmaximum;
                        long? v = ent.text.InvariantParseLongNull();
                        cb.Value = v.HasValue ? v.Value : cb.Minimum;
                        if (ent.numberboxformat != null)
                            cb.Format = ent.numberboxformat;
                    }

                    cb.ReturnPressed += (box) =>
                    {
                        Entry en = (Entry)(box.Tag);
                        Trigger?.Invoke(this, en, "Return", this.callertag);       // pass back the logical name of dialog, the name of the control, the caller tag
                    };
                    cb.ValidityChanged += (box, s) =>
                    {
                        Entry en = (Entry)(box.Tag);
                        Trigger?.Invoke(this, en, "Validity:" + s.ToString(), this.callertag);       // pass back the logical name of dialog, the name of the control, the caller tag
                    };
                }

                Add(c);
            }

            Location = pos;
            Rectangle area = ChildArea();
            Size = new Size(area.Right + Margin.TotalWidth + 20, area.Bottom + Margin.TotalHeight + 10 + Padding.TotalHeight + BorderWidth);
        }

        public override void OnKeyPress(GLKeyEventArgs e)       // forms gets first dibs at keys of children
        {
            base.OnKeyPress(e);
            if ( !e.Handled && e.KeyChar == 27 )
            {
                Trigger?.Invoke(this, null, "Escape", callertag);
                e.Handled = true;
            }
        }

        #endregion

    }
}

