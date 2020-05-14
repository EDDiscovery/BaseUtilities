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
using EliteDangerousCore.EDSM;

namespace TestOpenTk
{
    public partial class TestGalaxy : Form
    {
        public TestGalaxy()
        {
            InitializeComponent();

            glwfc = new OpenTKUtils.WinForm.GLWinFormControl(glControlContainer);

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
        }

        private OpenTKUtils.WinForm.GLWinFormControl glwfc;

        private Timer systemtimer = new Timer();

        private GalacticMapping galacticMapping;

        private Map map;


        /// ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Closed += ShaderTest_Closed;

            galacticMapping = new GalacticMapping();
            galacticMapping.ParseJson(System.Text.Encoding.UTF8.GetString(Properties.Resources.galacticmapping));                            // at this point, gal map data has been uploaded - get it into memory

            map = new Map();
            map.Start(glwfc, galacticMapping);
            systemtimer.Start();
        }

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            map.Dispose();
        }

        private void SystemTick(object sender, EventArgs e)
        {
            OpenTKUtils.Timers.Timer.ProcessTimers();
            map.Systick();
        }
    }
}


