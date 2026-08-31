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

using System;
using System.Threading;
using System.Collections.Generic;

namespace DirectInputDevices
{
    // Identity of a input device
    [System.Diagnostics.DebuggerDisplay("IDI {Name} {VendorID:X4} {ProductId:X4}")]
    public class InputDeviceIdentity
    {
        public string Name { get; set; }
        public Guid Instanceguid { get; set; }
        public Guid Productguid { get; set; }
        public int VendorId { get; set; }
        public int ProductId { get; set; }
        public string VendorProductId => $"{VendorId:X4}{ProductId:X4}";        // as used by frontier
        public bool GameControl { get; set; }       // per sharpdx DeviceClass
    }

    // An Input device
    public interface IInputDevice
    {
        InputDeviceIdentity ID { get; }
        AutoResetEvent Eventhandle();               // set when device changes state

        List<InputDeviceEvent> GetEvents();         // get events after change state
        void Dispose();

        string EventName(InputDeviceEvent e);       // Frontier event name from input event

        bool? IsPressed(string eventname);          // if an input supports pressed, true/false, else null

        string Name { get; }                        // device name

        string ToString();                          // description

        List<string> EventButtonNames();            // names of actionable button/keys of this device
    }

    // A event raised by an Input device
    [System.Diagnostics.DebuggerDisplay("ID {Device.Name} {EventName()} {EventNumber} {Pressed} {Axis} {Value}")]
    public class InputDeviceEvent
    {
        public IInputDevice Device { get; set; }
        public string EventName() { return Device.EventName(this); }        // logical event name
        public int EventNumber { get; set; }        // indentity of event specific to the device : VKeys key code, joystick buttons/axis/pov number, etc
        public bool Pressed { get; set; }           // button pressed.. or POV is not centred, or null if it does not press.
        public bool Axis { get; set; }              // If its an axis
        public int Value { get; set; }              // axis = range, from declared AxisMin to Max.  Pov = direction

        public InputDeviceEvent(IInputDevice d, int en , bool p, bool axis, int v = 0)
        {
            Device = d; EventNumber = en; Pressed = p; Axis = axis; Value = v; 
        }

        public string ToString(int trunc = 1000)
        {
            return string.Format("Device {0} Event {1} Pressed {2} Value {3}", Device.ID.Name.Truncate(0, trunc), EventName(), Pressed, Value);
        }

    }
}
