/*
 * Copyright 2017-2026 EDDiscovery development team
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

using SharpDX.DirectInput;
using System.Collections.Generic;

namespace DirectInputDevices
{
    public class InputDeviceMouse : IInputDevice
    {
        public int ButtonCount => 0;
        public int POVCount => 0;
        public string[] AxisPresent => new string[0];
        public InputDeviceIdentity ID => msi;
        public System.Threading.AutoResetEvent Eventhandle() { return eventhandle; }

        public InputDeviceMouse(DirectInput di,DeviceInstance d)
        {
            // those silly foreign people call mouse something other than it in english, so we need to fix it to english

            msi = new InputDeviceIdentity() { Instanceguid = d.InstanceGuid, Productguid = d.ProductGuid, Name = "Mouse"};

            mouse = new SharpDX.DirectInput.Mouse(di);
            mouse.SetNotification(eventhandle);
            mouse.Acquire();
            Capabilities c = mouse.Capabilities;
            butstate = new bool[c.ButtonCount];
        }

        public void Dispose()
        {
            if (mouse != null)
            {
                mouse.Unacquire();
                mouse.Dispose();
                mouse = null;
            }
        }

        MouseState ms;

        public List<InputDeviceEvent> GetEvents()
        {
            try
            {
                ms = mouse.GetCurrentState();
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Mouse removed!");
                return null;
            }

            // At the moment, not returning axis, as not needed..

            bool[] buttons = ms.Buttons;

            List<InputDeviceEvent> events = new List<InputDeviceEvent>();

            for (int i = 0; i < butstate.Length; i++)
            {
                bool s = buttons[i];
                if (s != butstate[i])
                {
                    butstate[i] = s;
                    //System.Diagnostics.Debug.WriteLine("But " + (i+1) + "=" + s);
                    events.Add(new InputDeviceEvent(this, i+1, butstate[i],false));     // 1..N
                }
            }

            return (events.Count > 0) ? events : null;
        }

        public List<string> EventButtonNames()
        {
            var l = new List<string>();
            for (int i = 0; i < butstate.Length; i++)
                l.Add("Mouse_" + (i + 1));

            return l;
        }

        public string EventName(InputDeviceEvent e) // need to return frontier naming convention!
        {
            return "Mouse_" + e.EventNumber.ToStringInvariant();
        }

        public bool? IsPressed(string keyname)
        {
            int mno;
            if (keyname.StartsWith("Mouse_") && int.TryParse(keyname.Substring(6), out mno) && mno >= 1 && mno <= butstate.Length)
            {
                //System.Diagnostics.Debug.WriteLine("Check press " + mno + "=" + butstate[mno-1]);
                return butstate[mno-1];
            }

            return null;
        }

        public string Name => msi.Name;

        public override string ToString()
        {
            return msi.Name + ":" + msi.Instanceguid + ":" + msi.Productguid;
        }

        public static void CreateMouse(InputDeviceList ilist)
        {
            DirectInput dinput = new DirectInput();

            foreach (DeviceInstance di in dinput.GetDevices(DeviceClass.Pointer, DeviceEnumerationFlags.AttachedOnly))
            {
                InputDeviceMouse k = new InputDeviceMouse(dinput,di);
                ilist.Add(k);
            }

        }

        private InputDeviceIdentity msi;
        private SharpDX.DirectInput.Mouse mouse;
        private bool[] butstate;
        private System.Threading.AutoResetEvent eventhandle = new System.Threading.AutoResetEvent(false);       // used by joy to signal data
    }
}
