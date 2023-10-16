using DirectInputDevices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestDirectInput
{
    public partial class TestDI : Form
    {
        InputDeviceList inputdevices;

        public TestDI()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            inputdevices?.Stop();
        }

        private void Print(string s)
        {
            richTextBox.AppendText(s + Environment.NewLine);
            //richTextBox.Select(richTextBox.Text.Length - 1, 1);
            richTextBox.ScrollToCaret();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MapDialog mp = new MapDialog();
            if ( mp.ShowDialog(this) == DialogResult.OK )
            {
                System.Diagnostics.Debug.WriteLine($"Device {mp.DeviceName} {mp.ButtonName} {mp.Press}");
            }
            return;

            inputdevices = new InputDeviceList((s) => { BeginInvoke(s); } );
            inputdevices.OnNewEvent += Inputdevices_OnNewEvent;

            DirectInputDevices.InputDeviceJoystickWindows.CreateJoysticks(inputdevices, true);
            DirectInputDevices.InputDeviceKeyboard.CreateKeyboard(inputdevices);              // Created.. not started..
            DirectInputDevices.InputDeviceMouse.CreateMouse(inputdevices);

            foreach( var id in inputdevices)
            {
                Print("Device " + id.ToString());
                Print("Buttons: " + string.Join(",", id.EventButtonNames()));
            }


            inputdevices.Start();
        }

        private void Inputdevices_OnNewEvent(List<InputDeviceEvent> list)
        {
            foreach( var i in list)
            {
                Print($"Event {i.ToString()} : {i.EventNumber} {i.Pressed} {i.Value}");
            }
        }
    }
}
