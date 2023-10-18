/*
 * Copyright © 2023 EDDiscovery development team
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
using System.Windows.Forms;

namespace DirectInputDevices
{
    public partial class InputMapDialog : Form
    {
        public string DeviceName { get { return textBoxDevice.Text; } }
        public string ButtonName { get { return textBoxName.Text; } }
        public bool Press { get { return radioButtonPressed.Checked; } }

        private InputDeviceList inputdevices;

        Timer timer = new Timer();

        public InputMapDialog()
        {
            InitializeComponent();
            inputdevices = new InputDeviceList((s) => { BeginInvoke(s); });
            inputdevices.OnNewEvent += Inputdevices_OnNewEvent;

            InputDeviceJoystickWindows.CreateJoysticks(inputdevices, false);
            InputDeviceKeyboard.CreateKeyboard(inputdevices);              // Created.. not started..
            InputDeviceMouse.CreateMouse(inputdevices);

            inputdevices.Start();

            timer.Interval = 500;
            timer.Tick += Timer_Tick;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            waitingevent = null;
            timer.Stop();
            inputdevices.Stop();
            base.OnClosing(e);
        }

        DirectInputDevices.InputDeviceEvent waitingevent;
        bool allowmouse = false;

        //we wait until accepting it due to mouse down on ok
        private void Inputdevices_OnNewEvent(List<InputDeviceEvent> list)
        {
            if (waitingevent == null)       // nothing in queue
            {
                waitingevent = list[0];
                //System.Diagnostics.Debug.WriteLine($"waiting event {waitingevent.Device.Name()}");
                timer.Start();
            }
            else
            {
               // System.Diagnostics.Debug.WriteLine($"reject waiting event {waitingevent.Device.Name()}");
            }

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (waitingevent != null)
            {
                if (waitingevent.Device.Name() != "Mouse" || allowmouse)
                {
                    //System.Diagnostics.Debug.WriteLine($"Event {waitingevent.ToString()}");
                    textBoxDevice.Text = waitingevent.Device.Name();
                    textBoxName.Text = waitingevent.EventName();
                }
                allowmouse = false;
                waitingevent = null;
            }
        }

        private void buttonMouseClick_MouseDown(object sender, MouseEventArgs e)
        {
            allowmouse = true;
        }
    }
}
