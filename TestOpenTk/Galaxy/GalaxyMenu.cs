using OpenTKUtils.Common;
using OpenTKUtils.GL4.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOpenTk
{
    public class GalaxyMenu
    {
        GLControlDisplay displaycontrol;
        Controller3D gl3dcontroller;

        public GalaxyMenu(GLControlDisplay displaycontrolp, Controller3D gl3dcontrollerp )
        {
            displaycontrol = displaycontrolp;
            gl3dcontroller = gl3dcontrollerp;
            GLImage menuimage = new GLImage("MainMenu", new Rectangle(10, 10, 32, 32), Properties.Resources.hamburgermenu);
            displaycontrol.Add(menuimage);
            menuimage.MouseClick = (o, e1) => { ShowMenu(); };

            GLBaseControl.Themer = (s) => { Theme(s); };
            
        }

        static void Theme(GLBaseControl s)      // run on each control during add, theme it
        {
            var cb = s as GLCheckBox;
            if ( cb != null )
            {
                float[][] colorMatrixElements = {
                           new float[] {0.5f,  0,  0,  0, 0},        // red scaling factor of 0.5
                           new float[] {0,  0.5f,  0,  0, 0},        // green scaling factor of 1
                           new float[] {0,  0,  0.5f,  0, 0},        // blue scaling factor of 1
                           new float[] {0,  0,  0,  1, 0},        // alpha scaling factor of 1
                           new float[] {0.0f, 0.0f, 0.0f, 0, 1}};    // three translations of 

                var colormap1 = new System.Drawing.Imaging.ColorMap();
                cb.SetDrawnBitmapUnchecked(new System.Drawing.Imaging.ColorMap[] { colormap1 }, colorMatrixElements);
            }
        }

        public void ShowMenu()
        {
            displaycontrol["MainMenu"].Visible = false;
            GLForm pform = new GLForm("form", "Configure Map", new Rectangle(10, 10, 200, 400));
            pform.BackColor = Color.FromArgb(50, Color.Red);
            pform.ForeColor = Color.Orange;
            pform.FormClosed = (frm) => { displaycontrol["MainMenu"].Visible = true; displaycontrol.SetFocus(); };

            GLPanel p3d2d = new GLPanel("3d2d", new Rectangle(10, 10, 80, 50), Color.Transparent);
            GLCheckBox but3d = new GLCheckBox("3d", new Rectangle(4, 2, 32, 32), Properties.Resources._3d, null);
            but3d.AutoCheck = true;
            but3d.Checked = true;
            but3d.GroupRadioButton = true;
            but3d.MouseClick += (e1, e2) => { gl3dcontroller.ChangePerspectiveMode(true); };
            p3d2d.Add(but3d);

            GLCheckBox but2d = new GLCheckBox("2d", new Rectangle(40, 2, 32, 32), Properties.Resources._2d, null);
            but2d.AutoCheck = true;
            but2d.GroupRadioButton = true;
            but2d.MouseClick += (e1, e2) => { gl3dcontroller.ChangePerspectiveMode(false); };
            p3d2d.Add(but2d);

            pform.Add(p3d2d);
            displaycontrol.Add(pform);
        }
    }
}
