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
    public partial class MapDialog : Form
    {
        public string DeviceName { get { return textBoxDevice.Text; } }
        public string ButtonName { get { return textBoxName.Text; } }
        public bool Press { get { return radioButtonPressed.Checked; } }

        private InputDeviceList inputdevices;
        private Timer clickwaittimer = new Timer();
        private InputDeviceEvent waitingevent;
        public MapDialog()
        {
            InitializeComponent();
            inputdevices = new InputDeviceList((s) => { BeginInvoke(s); });
            inputdevices.OnNewEvent += Inputdevices_OnNewEvent;

            InputDeviceJoystickWindows.CreateJoysticks(inputdevices, false);
            InputDeviceKeyboard.CreateKeyboard(inputdevices);              // Created.. not started..
            InputDeviceMouse.CreateMouse(inputdevices);

            inputdevices.Start();

            clickwaittimer.Interval = 200;
            clickwaittimer.Tick += Clickwaittimer_Tick;

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            inputdevices.Stop();
        }

        //we wait until accepting it due to mouse down on ok
        private void Inputdevices_OnNewEvent(List<InputDeviceEvent> list)   
        {
            waitingevent = list[0];
            System.Diagnostics.Debug.WriteLine($"Event {waitingevent.ToString()}");
            clickwaittimer.Start();
        }

        private void Clickwaittimer_Tick(object sender, EventArgs e)
        {
            clickwaittimer.Stop();
            textBoxDevice.Text = waitingevent.Device.Name();
            textBoxName.Text = waitingevent.EventName();
        }

    }
}
