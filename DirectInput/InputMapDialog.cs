/*
 * Copyright 2023-2026 EDDiscovery development team
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DirectInputDevices
{
    public partial class InputMapDialog : Form
    {
        public IInputDevice Device { get; set; }
        public string KeyName { get { return labelKeyboard.Text; } }        // Joy_XAxis, POV, Joy_14, or VKey Name
        public bool DirectionPositive { get; set; }                         // Joy direction
        public bool Press { get { return radioButtonPressed.Checked; } }

        public bool AllowAxis { get; set; } = false;                        // moving an axis is a trigger
        public bool AllowMouse { get; set; } = true;                        // moving an axis is a trigger
        public bool AllowKeyboard { get; set; } = true;                        // moving an axis is a trigger
        public bool ShowPressOrRelease { get; set; } = true;
        public bool ShowOKCancel { get; set; } = true;
        public bool EscapeQuits { get; set; } = false;

        public InputMapDialog()
        {
            InitializeComponent();
        }

        public void Init(InputDeviceList idl)    
        {
            inputdevices = idl;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            string t = labelTitle.Text = AllowKeyboard ? "Press a Key" : "";
            t = t.AppendPrePad(AllowMouse ? "Press a Mouse Button" : "", ",");
            t = t.AppendPrePad(AllowAxis ? "Move a joystick axis" : "", ",");
            labelTitle.Text = t;

            panelPressRelease.Visible = ShowPressOrRelease;
            buttonCancel.Visible = buttonOK.Visible = ShowOKCancel;

            buttonback = buttonMouseClick.BackColor;
            lastclicktimemousebutton = (uint)Environment.TickCount;
            closetimer.Tick += (s, e2) => { DialogResult = DialogResult.OK; Close(); };

            inputdevices.OnNewEventInThread += Inputdevices_OnNewEvent;

            extButtonDrawnClose.Visible = FormBorderStyle == FormBorderStyle.None;
            labelClickForMouse.Visible = buttonMouseClick.Visible = AllowMouse;
        }

        protected override void OnShown(EventArgs e)
        {
            int okbottopressbot = buttonOK.Bottom - panelPressRelease.Bottom;       // need to do before fiddling
            int oktoptopresstop = buttonOK.Top - panelPressRelease.Top;
            if (!ShowOKCancel)
                Height -= okbottopressbot;
            if (!ShowPressOrRelease)
                Height -= oktoptopresstop;
            if (!AllowMouse)
                Height -= buttonMouseClick.Height;
            base.OnShown(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            inputdevices.OnNewEventInThread -= Inputdevices_OnNewEvent;
        }

        //we wait until accepting it due to mouse down on ok
        private void Inputdevices_OnNewEvent(List<InputDeviceEvent> list)
        {
            if (!Application.MessageLoop)      // pump to message loop
            {
                BeginInvoke((MethodInvoker)delegate { Inputdevices_OnNewEvent(list); });
                return;
            }

            foreach (var ev in list)
            {
                System.Diagnostics.Debug.WriteLine($"Event {ev.ToString()}");

                bool accept = false;

                if (ev.Axis)
                {
                    if (AllowAxis)
                    {
                        if (Math.Abs(ev.Value - 500) > 250)       // make sure they push it more than half way
                        {
                            DirectionPositive = ev.Value > 500;
                            accept = true;
                        }
                    }
                }
                else if (ev.Pressed)        // must be a pressed event, ignore up events
                {
                    if (ev.Device.Name == "Mouse")
                    {
                        if ( AllowMouse && ev.Pressed && mouseallowed && ((uint)Environment.TickCount - lastclicktimemousebutton) > 250)
                        {
                            mouseallowed = false;
                            lastclicktimemousebutton = (uint)Environment.TickCount;
                            buttonMouseClick.BackColor = buttonback;
                            //System.Diagnostics.Debug.WriteLine($"Mouse click accepted");
                            accept = true;
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine($"Mouse click rejected");
                        }
                    }
                    else 
                    {
                        if (EscapeQuits && ev.Device.Name == "Keyboard" && ev.EventNumber == (int)Keys.Escape)
                        {
                            DialogResult = DialogResult.Cancel;
                            Close();
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Keyboard Hit");
                            accept = AllowKeyboard;
                        }
                    }
                }

                if (accept)
                {
                    Device = ev.Device;
                    labelDevice.Text = ev.Device.Name;
                    labelKeyboard.Text = ev.EventName();

                    if (!ShowOKCancel)
                    {
                        closetimer.Start();
                    }
                }
            }


        }

        private void buttonMouseClick_MouseDown(object sender, MouseEventArgs e)
        {
            if (mouseallowed == false && ((uint)Environment.TickCount - lastclicktimemousebutton) > 250)
            {
                lastclicktimemousebutton = (uint)Environment.TickCount;     // records last click time on button to debounce
                mouseallowed = true;
                buttonMouseClick.BackColor = Color.Green;
                //System.Diagnostics.Debug.WriteLine($"Enable mouse click accepted");
            }
        }

        private Color buttonback;
        private uint lastclicktimemousebutton;
        private bool mouseallowed = false;
        private Timer closetimer = new Timer() { Interval = 500 };
        private InputDeviceList inputdevices;

        private void extButtonDrawnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
