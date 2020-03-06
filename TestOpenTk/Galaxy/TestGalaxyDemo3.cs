using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTKUtils;
using OpenTKUtils.Common;
using OpenTKUtils.GL4;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using OpenTKUtils.GL4.Controls;

namespace TestOpenTk
{
    public partial class TestGalaxyDemo3 : Form
    {
        public TestGalaxyDemo3()
        {
            InitializeComponent();

            glwfc = new OpenTKUtils.WinForm.GLWinFormControl(glControlContainer);

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
        }

        private OpenTKUtils.WinForm.GLWinFormControl glwfc;

        private Timer systemtimer = new Timer();

        private Map map;


        /// ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            map.Dispose();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Closed += ShaderTest_Closed;
            map = new Map(glwfc);
            map.Start();
            systemtimer.Start();
        }

        private void SystemTick(object sender, EventArgs e)
        {
            OpenTKUtils.Timers.Timer.ProcessTimers();
            map.Systick();
        }
    }
}


