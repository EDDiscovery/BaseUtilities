using OpenTKUtils.GL4.Controls;
using System.Collections.Generic;
using System.Drawing;

namespace TestOpenTk
{
    public class MapMenu
    {
        private Map map;
        private GLLabel status;
        private const int iconsize = 32;

        public MapMenu(Map g)
        {
            map = g;

            // names of MS* are on screen items hidden during main menu presentation

            GLImage menuimage = new GLImage("MSMainMenu", new Rectangle(10, 10, iconsize, iconsize), Properties.Resources.hamburgermenu);
            menuimage.ToolTipText = "Open configuration menu";
            map.displaycontrol.Add(menuimage);
            menuimage.MouseClick = (o, e1) => { ShowMenu(); };

            GLImage tpback = new GLImage("MSTPBack", new Rectangle(50, 10, iconsize, iconsize), Properties.Resources.GoBackward);
            tpback.ToolTipText = "Go back one system";
            map.displaycontrol.Add(tpback);
            tpback.MouseClick = (o, e1) => { g.TravelPathMoveBack(); };

            GLImage tphome = new GLImage("MSTPHome", new Rectangle(90, 10, iconsize, iconsize), Properties.Resources.GoToHomeSystem);
            tphome.ToolTipText = "Go to current home system";
            map.displaycontrol.Add(tphome);
            tphome.MouseClick = (o, e1) => { g.GoToCurrentSystem(); };

            GLImage tpforward = new GLImage("MSTPForward", new Rectangle(130, 10, iconsize, iconsize), Properties.Resources.GoForward);
            tpforward.ToolTipText = "Go forward one system";
            map.displaycontrol.Add(tpforward);
            tpforward.MouseClick = (o, e1) => { g.TravelPathMoveForward(); };

            GLToolTip maintooltip = new GLToolTip("MTT",Color.FromArgb(180,50,50,50));
            maintooltip.ForeColor = Color.Orange;
            map.displaycontrol.Add(maintooltip);

            status = new GLLabel("Status", new Rectangle(10, 500, 400, 24), "x");
            status.Dock = DockingType.BottomLeft;
            status.ForeColor = Color.Orange;
            status.BackColor = Color.FromArgb(50, 50, 50, 50);
            map.displaycontrol.Add(status);

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

            int leftmargin = 4;
            int vpos = 10;
            int ypad = 10;

            GLForm pform = new GLForm("FormMenu", "Configure Map", new Rectangle(10, 10, 600, 400));
            pform.BackColor = Color.FromArgb(50, Color.Red);
            pform.ForeColor = Color.Orange;
            pform.FormClosed = (frm) => { map.displaycontrol.ApplyToControlOfName("MS*", (c) => { c.Visible = true; }); };

            GLPanel p3d2d = new GLPanel("3d2d", new Rectangle(leftmargin, vpos, 80, iconsize), Color.Transparent);

            GLCheckBox but3d = new GLCheckBox("3d", new Rectangle(0, 0, iconsize, iconsize), Properties.Resources._3d, null);
            but3d.Checked = map.gl3dcontroller.MatrixCalc.InPerspectiveMode;
            but3d.ToolTipText = "3D View";
            but3d.GroupRadioButton = true;
            but3d.MouseClick += (e1, e2) => { map.gl3dcontroller.ChangePerspectiveMode(true); };
            p3d2d.Add(but3d);

            GLCheckBox but2d = new GLCheckBox("2d", new Rectangle(40, 0, iconsize, iconsize), Properties.Resources._2d, null);
            but2d.Checked = !map.gl3dcontroller.MatrixCalc.InPerspectiveMode;
            but2d.ToolTipText = "2D View";
            but2d.GroupRadioButton = true;
            but2d.MouseClick += (e1,e2) => { map.gl3dcontroller.ChangePerspectiveMode(false); };
            p3d2d.Add(but2d);

            pform.Add(p3d2d);

            GLCheckBox butelite = new GLCheckBox("Elite", new Rectangle(100, vpos, iconsize, iconsize), Properties.Resources.EliteMovement, null);
            butelite.ToolTipText = "Select elite movement (on Y plain)";
            butelite.Checked = map.gl3dcontroller.EliteMovement;
            butelite.CheckChanged += (e1) => { map.gl3dcontroller.EliteMovement = butelite.Checked; };
            pform.Add(butelite);

            vpos += p3d2d.Height+ypad;

            GLCheckBox butgal = new GLCheckBox("Galaxy", new Rectangle(leftmargin, vpos, iconsize, iconsize), Properties.Resources.ShowGalaxy, null);
            butgal.ToolTipText = "Show galaxy image";
            butgal.Checked = map.GalaxyEnabled();
            butgal.CheckChanged += (e1) => { map.EnableToggleGalaxy(butgal.Checked); };
            pform.Add(butgal);

            GLCheckBox butsd = new GLCheckBox("StarDots", new Rectangle(50, vpos, iconsize, iconsize), Properties.Resources.StarDots, null);
            butsd.ToolTipText = "Show star field";
            butsd.Checked = map.StarDotsEnabled();
            butsd.CheckChanged += (e1) => { map.EnableToggleStarDots(butsd.Checked); };
            pform.Add(butsd);

            vpos += butgal.Height+ypad;

            GLGroupBox galgb = new GLGroupBox("GalGB", "Galaxy Objects", new Rectangle(leftmargin, vpos, pform.ClientWidth-leftmargin*2, 50));
            galgb.Height = (iconsize + 4) * 2 + galgb.GroupBoxHeight + GLGroupBox.GBMargins*2 + GLGroupBox.GBPadding*2 + 8;
            galgb.BackColor = Color.FromArgb(60, Color.Red);
            galgb.ForeColor = Color.Orange;
            pform.Add(galgb);
            GLFlowLayoutPanel galfp = new GLFlowLayoutPanel("GALFP", DockingType.Fill, 0);
            galfp.FlowPadding = new Padding(2, 2, 2, 2);
            galfp.BackColor = Color.FromArgb(60, Color.Red);
            galgb.Add(galfp);
            
            for( int i = map.galmap.RenderableMapTypes.Length-1;i>=0;i--)
            {
                var gt = map.galmap.RenderableMapTypes[i];
                GLCheckBox butg = new GLCheckBox("GMSEL", new Rectangle(0,0, iconsize, iconsize), gt.Image, null);
                butg.ToolTipText = "Enable/Disable " + gt.Description;
                butg.Checked = gt.Enabled;
                butg.CheckChanged += (e1) => { gt.Enabled = butg.Checked; map.UpdateGalObjects();  };
                galfp.Add(butg);
            }

            map.displaycontrol.Add(pform);
        }

        public void UpdateCoords(OpenTKUtils.GLMatrixCalc c)
        {
            status.Text = c.TargetPosition.X.ToStringInvariant("N1") + " ," + c.TargetPosition.Y.ToStringInvariant("N1") + " ,"
                         + c.TargetPosition.Z.ToStringInvariant("N1") + " Dist " + c.EyeDistance.ToStringInvariant("N1") + " Eye " +
                         c.EyePosition.X.ToStringInvariant("N1") + " ," + c.EyePosition.Y.ToStringInvariant("N1") + " ," + c.EyePosition.Z.ToStringInvariant("N1");
        }
    }
}
