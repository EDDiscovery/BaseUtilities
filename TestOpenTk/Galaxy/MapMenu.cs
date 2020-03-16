using OpenTKUtils.GL4.Controls;
using System.Collections.Generic;
using System.Drawing;

namespace TestOpenTk
{
    public class MapMenu
    {
        Map map;

        public MapMenu(Map g )
        {
            map = g;

            // names of MS* are on screen items hidden during main menu presentation

            GLImage menuimage = new GLImage("MSMainMenu", new Rectangle(10, 10, 32, 32), Properties.Resources.hamburgermenu);
            map.displaycontrol.Add(menuimage);
            menuimage.MouseClick = (o, e1) => { ShowMenu(); };

            GLImage tpback = new GLImage("MSTPBack", new Rectangle(50, 10, 32, 32), Properties.Resources.hamburgermenu);
            map.displaycontrol.Add(tpback);
            tpback.MouseClick = (o, e1) => { g.TravelPathMoveBack(); };

            GLImage tphome = new GLImage("MSTPHome", new Rectangle(90, 10, 32, 32), Properties.Resources.hamburgermenu);
            map.displaycontrol.Add(tphome);
            tphome.MouseClick = (o, e1) => { g.GoToCurrentSystem(); };

            GLImage tpforward = new GLImage("MSTPForward", new Rectangle(130, 10, 32, 32), Properties.Resources.hamburgermenu);
            map.displaycontrol.Add(tpforward);
            tpforward.MouseClick = (o, e1) => { g.TravelPathMoveForward(); };

            GLBaseControl.Themer = Theme;

            map.displaycontrol.GlobalFocusChanged += (i, from, to) =>       // intercept global focus changes to close menu if required
            {
                if (to== map.displaycontrol && map.displaycontrol["FormMenu"] != null )
                {
                    ((GLForm)map.displaycontrol["FormMenu"]).Close();
                }
            };
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
            map.displaycontrol.ApplyToControlOfName("MS*", (c) => { c.Visible = false; });
            //map.displaycontrol["MSMainMenu"].Visible = false;

            GLForm pform = new GLForm("FormMenu", "Configure Map", new Rectangle(10, 10, 200, 400));
            pform.BackColor = Color.FromArgb(50, Color.Red);
            pform.ForeColor = Color.Orange;
            pform.FormClosed = (frm) => { map.displaycontrol.ApplyToControlOfName("MS*", (c) => { c.Visible = true; }); };

            GLPanel p3d2d = new GLPanel("3d2d", new Rectangle(10, 10, 80, 50), Color.Transparent);

            GLCheckBox but3d = new GLCheckBox("3d", new Rectangle(0, 0, 32, 32), Properties.Resources._3d, null);
            but3d.Checked = map.gl3dcontroller.MatrixCalc.InPerspectiveMode;
            but3d.GroupRadioButton = true;
            but3d.MouseClick += (e1, e2) => { map.gl3dcontroller.ChangePerspectiveMode(true); };
            p3d2d.Add(but3d);

            GLCheckBox but2d = new GLCheckBox("2d", new Rectangle(40, 0, 32, 32), Properties.Resources._2d, null);
            but2d.Checked = !map.gl3dcontroller.MatrixCalc.InPerspectiveMode;
            but2d.GroupRadioButton = true;
            but2d.MouseClick += (e1,e2) => { map.gl3dcontroller.ChangePerspectiveMode(false); };
            p3d2d.Add(but2d);

            pform.Add(p3d2d);

            GLCheckBox butelite = new GLCheckBox("Elite", new Rectangle(100, 10, 32, 32), Properties.Resources._2d, null);
            butelite.Checked = !map.gl3dcontroller.EliteMovement;
            butelite.CheckChanged += (e1) => { map.gl3dcontroller.EliteMovement = butelite.Checked; };
            pform.Add(butelite);

            GLCheckBox butgal = new GLCheckBox("Galaxy", new Rectangle(10, 50, 32, 32), Properties.Resources._2d, null);
            butgal.Checked = map.GalaxyEnabled();
            butgal.CheckChanged += (e1) => { map.EnableToggleGalaxy(butgal.Checked); };
            pform.Add(butgal);

            GLCheckBox butsd = new GLCheckBox("Galaxy", new Rectangle(50, 50, 32, 32), Properties.Resources._2d, null);
            butsd.Checked = map.StarDotsEnabled();
            butsd.CheckChanged += (e1) => { map.EnableToggleStarDots(butsd.Checked); };
            pform.Add(butsd);

            map.displaycontrol.Add(pform);
            //displaycontrol.Focusable = false;
        }
    }
}
