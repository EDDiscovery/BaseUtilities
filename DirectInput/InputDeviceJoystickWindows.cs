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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DirectInputDevices
{
    public class InputDeviceJoystickWindows : IInputDevice
    {
        public InputDeviceIdentity ID => jsi;
        public int AxisMinRange { get; set; } = 0;          // axis are reported over this range.
        public int AxisMaxRange { get; set; } = 1000;
        public int AxisHisterisis { get; set; } = 50;       // will always report if > 
        public int AxisMustReportDifference { get; set; } = 10; // will delay the report by AxisMustReportDelay. Below this all values are ignored
        public int AxisMustReportDelay { get; set; } = 500;

        public System.Threading.AutoResetEvent Eventhandle() { return eventhandle; }

        public int ButtonCount => butstate.Length;
        public int POVCount => povvalue.Length;
        public string[] AxisPresent { get; private set; }

        public InputDeviceJoystickWindows(DirectInput di, DeviceInstance d)
        {
            jsi = new InputDeviceIdentity() { 
                        Instanceguid = d.InstanceGuid, 
                        Productguid = d.ProductGuid, 
                        Name = d.InstanceName.RemoveTrailingCZeros().Trim(),        // PC has a trailing space in name!
                        GameControl = true};       

            stick = new SharpDX.DirectInput.Joystick(di, d.InstanceGuid);
            stick.SetNotification(eventhandle);
            stick.Acquire();

            axispresent = new bool[AxisCount];
            axisvalue = Enumerable.Repeat(AxisNullValue, AxisCount).ToArray();

            Capabilities c = stick.Capabilities;
            butstate = new bool[c.ButtonCount];

            povvalue = Enumerable.Repeat(POVNotPressed, c.PovCount).ToArray();
            slidercount = 0;

            DeviceProperties p = stick.Properties;

            jsi.VendorId = p.VendorId;
            jsi.ProductId = p.ProductId;

            //   string s = p.PortDisplayName;

            System.Diagnostics.Debug.WriteLine("DirectInput Joy {0} {1} but {2} pov {3}", jsi.Name, jsi.Productguid, butstate.Length, povvalue.Length);

            foreach (DeviceObjectInstance deviceObject in stick.GetObjects())
            {
                if ((deviceObject.ObjectId.Flags & DeviceObjectTypeFlags.Axis) != 0)
                {
                    System.Guid guid = deviceObject.ObjectType;
                    //System.Diagnostics.Debug.WriteLine("  {0} {1} {2} {3} {4}", jsi.Name, deviceObject.UsagePage, deviceObject.Usage, deviceObject.Offset, guid.ToString());

                    if (guid == ObjectGuid.XAxis)
                    {
                        axispresent[(int)Axis.X] = true;
                    }
                    else if (guid == ObjectGuid.YAxis)
                    {
                        axispresent[(int)Axis.Y] = true;
                    }
                    else if (guid == ObjectGuid.ZAxis)
                    {
                        axispresent[(int)Axis.Z] = true;
                    }
                    else if (guid == ObjectGuid.RxAxis)
                    {
                        axispresent[(int)Axis.RX] = true;
                    }
                    else if (guid == ObjectGuid.RyAxis)
                    {
                        axispresent[(int)Axis.RY] = true;
                    }
                    else if (guid == ObjectGuid.RzAxis)
                    {
                        axispresent[(int)Axis.RZ] = true;
                    }
                    else if (guid == ObjectGuid.Slider)
                    {
                        int axisentry = (int)Axis.U + slidercount;
                        if (axisentry < AxisCount)
                        {
                            axispresent[axisentry] = true;
                            slidercount++;      // must be sliders, only ones left with axis
                            //System.Diagnostics.Debug.WriteLine("Slider " + slidercount);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Unknown Axis " + guid.ToString());
                    }

                    ObjectProperties o = stick.GetObjectPropertiesById(deviceObject.ObjectId);
                    o.Range = new InputRange(AxisMinRange, AxisMaxRange);
                }
            }

            List<string> axis = new List<string>();
            for(int i = 0; i < axispresent.Length; i++)
            {
                if (axispresent[i])
                    axis.Add(new string[] { "X", "Y", "Z", "RX", "RY", "RZ", "U", "V" }[i]);
            }

            AxisPresent = axis.ToArray(); 
        }



        public List<InputDeviceEvent> GetEvents()
        {
            List<InputDeviceEvent> events = new List<InputDeviceEvent>();

            JoystickState js;
            try
            {
                js = stick.GetCurrentState();
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Joystick removed!");
                return null;
            }

            bool[] buttons = js.Buttons;

            for (int i = 0; i < butstate.Length; i++)
            {
                bool s = buttons[i];
                if (s != butstate[i])
                {
                    butstate[i] = s;
                    //System.Diagnostics.Debug.WriteLine("But " + (i + 1) + "=" + s);
                    events.Add(new InputDeviceEvent(this, ButtonBase + i, butstate[i], false));
                }
            }

            int[] pov = js.PointOfViewControllers;

            for (int i = 0; i < povvalue.Length; i++)
            {
                if (pov[i] != povvalue[i])
                {
                    // if both previous and current is not released, and changed.. generate a fake release event
                    // this gives the caller indication that the current state has ended..

                    if (povvalue[i] != -1 && pov[i] != -1 && pov[i] != povvalue[i])          
                    {
                        events.Add(new InputDeviceEvent(this, POVBase + i + 1, false, false, -1)); 
                    }

                    povvalue[i] = pov[i];
                    events.Add(new InputDeviceEvent(this, POVBase + i + 1, povvalue[i] != -1, false, povvalue[i]));
                }
            }

            int[] sliders = js.Sliders;

            for (int i = 0; i < axispresent.Length; i++)
            {
                if (axispresent[i])
                {
                    int value;
                    if (i == (int)Axis.X)
                        value = js.X;
                    else if (i == (int)Axis.Y)
                        value = js.Y;
                    else if (i == (int)Axis.Z)
                        value = js.Z;
                    else if (i == (int)Axis.RX)
                        value = js.RotationX;
                    else if (i == (int)Axis.RY)
                        value = js.RotationY;
                    else if (i == (int)Axis.RZ)
                        value = js.RotationZ;
                    else
                        value = sliders[i - (int)Axis.U];

                    if (axisvalue[i] == AxisNullValue)
                    {
                        axisvalue[i] = value;
                    }
                    else
                    {
                        int diff = Math.Abs(value - axisvalue[i]);
                        uint curtime = (uint)Environment.TickCount;
                        if (diff >= AxisHisterisis || (curtime-lastexisreporttime >= AxisMustReportDelay && diff >= AxisMustReportDifference)) // don't report min changes until a timeout
                        {
                            axisvalue[i] = value;
                            var ev = new InputDeviceEvent(this, AxisBase + i, true, true, value);
                            events.Add(ev);      // axis is always pressed
                            lastexisreporttime = curtime;
                            //System.Diagnostics.Debug.WriteLine($"Axis event generated {Name} {EventName(ev)} {i} {diff} {axisvalue[i]}");
                        }
                    }
                }
            }

            //foreach (var ev in events) System.Diagnostics.Debug.WriteLine($"Input Event {ev.ToString()}");
            
            return (events.Count > 0) ? events : null;
        }

        public void Dispose()
        {
            if (stick != null)
            {
                stick.Unacquire();
                stick.Dispose();
                stick = null;
            }
        }


        static Dictionary<int, string> povdir = new Dictionary<int, string>() { { 0, "Up" }, { 4500, "UpRight" }, { 9000, "Right" },{ 13500, "DownRight" },{ 18000, "Down" },
                            { 22500, "DownLeft" }, {27000, "Left" }, {31500, "UpLeft" } };

        // interface : list actionable button and keys of the device
        public List<string> EventButtonNames()
        {
            var l = new List<string>();
            for (int i = 0; i < butstate.Length; i++)
                l.Add("Joy_" + (i + 1));

            var povnames = povdir.Values.ToList();
            for( int i = 0; i < povvalue.Length; i++)
            {
                for( int j = 0; j < povdir.Count; j++)
                    l.Add("Joy_POV" + (i+1).ToStringInvariant() + povnames[j]);
            }

            return l;
        }

        // interface: turn an event number from directinput into a logical name
        public string EventName(InputDeviceEvent e)
        {
            if (e.EventNumber < POVBase)
                return "Joy_" + e.EventNumber;
            else if (e.EventNumber < AxisBase)
            {
                string j = "Joy_POV" + (e.EventNumber - POVBase);
                return j + ((povdir.ContainsKey(e.Value)) ? (povdir[e.Value]) : "Centred");
            }
            else
                return "Joy_" + ((Axis)(e.EventNumber - AxisBase)).ToString() + "Axis";
        }

        // interface: Is this event pressed?
        public bool? IsPressed(string eventname)
        {
            if (eventname.StartsWith("Joy_POV"))
            {
                if (eventname.Length >= 7 + 1 + 2)
                {
                    int num = eventname[7] - '0';
                    string sdir = eventname.Substring(8);

                    if (num >= 1 && num <= povvalue.Length)
                    {
                        int dir = povvalue[num - 1];
                        string actualdir = povdir.ContainsKey(dir) ? povdir[dir] : "Centred";
                        return actualdir.Equals(sdir);
                    }
                }
            }
            else if (eventname.Contains("Axis"))        // axis are always pressed in effect..
            {
                return null;
            }
            else if (eventname.StartsWith("Joy_"))
            {
                int but = 0;
                if (eventname.Substring(4).InvariantParse(out but) && but >= 1 && but <= butstate.Length )
                    return butstate[but - 1];
            }

            return null;
        }

        // interface name
        public string Name => jsi.Name;

        public override string ToString()
        {
            return jsi.Name + ":" + jsi.Instanceguid + ":" + jsi.Productguid + ":" + jsi.ProductId.ToString("x") + "," + jsi.VendorId.ToString("x") + ":" + butstate.Length + "," + povvalue.Length + "," + slidercount;
        }

        // call to create all joysticks into ilist
        public static void CreateJoysticks(InputDeviceList ilist)
        {
            DirectInput dinput = new DirectInput();

            foreach (DeviceInstance di in dinput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly))
            {
                //   if (di.InstanceName.Contains("Logitech"))
                {
                    InputDeviceJoystickWindows j = new InputDeviceJoystickWindows(dinput, di);
                    ilist.Add(j);
                }
            }
        }

        private InputDeviceIdentity jsi;
        private Joystick stick;

        private bool[] butstate;
        private int[] povvalue;
        private bool[] axispresent;
        private int[] axisvalue;
        private int slidercount;
        private System.Threading.AutoResetEvent eventhandle = new System.Threading.AutoResetEvent(false);       // used by joy to signal data
        private enum Axis { X = 0, Y, Z, RX, RY, RZ, U, V };         // frontier names for simplicity
        private const int AxisCount = 8;
        private const int AxisNullValue = -1;
        private const int POVNotPressed = -1;

        // give EventNumbers to various events
        private const int ButtonBase = 1;                            // event ID bases. buttons are 1 to 999
        private const int POVBase = 1000;                            // pov is 1000,1001 etc
        private const int AxisBase = 2000;                           // axis in Axis order, x = 1000, y =1001

        private uint lastexisreporttime = 0;

    }
}
