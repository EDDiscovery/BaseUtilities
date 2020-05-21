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
            pform.BackColor = Color.FromArgb(180, 60,60,60);
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
            butgal.Checked = map.EnableGalaxy;
            butgal.CheckChanged += (e1) => { map.EnableGalaxy = butgal.Checked; };
            pform.Add(butgal);

            GLCheckBox butsd = new GLCheckBox("StarDots", new Rectangle(50, vpos, iconsize, iconsize), Properties.Resources.StarDots, null);
            butsd.ToolTipText = "Show star field";
            butsd.Checked = map.EnableStarDots;
            butsd.CheckChanged += (e1) => { map.EnableStarDots = butsd.Checked; };
            pform.Add(butsd);

            GLCheckBox buttp = new GLCheckBox("TravelPath", new Rectangle(100, vpos, iconsize, iconsize), Properties.Resources.StarDots, null);
            buttp.ToolTipText = "Show travel path";
            buttp.Checked = map.EnableTravelPath;
            buttp.CheckChanged += (e1) => { map.EnableTravelPath = buttp.Checked; };
            pform.Add(buttp);

            vpos += butgal.Height + ypad;

            GLGroupBox galgb = new GLGroupBox("GalGB", "Galaxy Objects", new Rectangle(leftmargin, vpos, pform.ClientWidth - leftmargin * 2, 50));
            galgb.ClientHeight = (iconsize + 4) * 2;
            galgb.BackColor = pform.BackColor;
            galgb.ForeColor = Color.Orange;
            pform.Add(galgb);
            GLFlowLayoutPanel galfp = new GLFlowLayoutPanel("GALFP", DockingType.Fill, 0);
            galfp.FlowPadding = new Padding(2, 2, 2, 2);
            galfp.BackColor = pform.BackColor;
            galgb.Add(galfp);

            for ( int i = map.galmap.RenderableMapTypes.Length-1;i>=0;i--)
            {
                var gt = map.galmap.RenderableMapTypes[i];
                GLCheckBox butg = new GLCheckBox("GMSEL", new Rectangle(0, 0, iconsize, iconsize), gt.Image, null);
                butg.ToolTipText = "Enable/Disable " + gt.Description;
                butg.Checked = gt.Enabled;
                butg.CheckChanged += (e1) => { gt.Enabled = butg.Checked; map.UpdateGalObjectsStates(); };
                galfp.Add(butg);
            }

            GLCheckBox butgonoff = new GLCheckBox("GMONOFF", new Rectangle(0, 0, iconsize, iconsize), Properties.Resources.dotted, null);
            butgonoff.ToolTipText = "Enable/Disable Display";
            butgonoff.Checked = map.GalObjectEnable;
            butgonoff.CheckChanged += (e1) => { map.GalObjectEnable = !map.GalObjectEnable; };
            galfp.Add(butgonoff);

            vpos += galgb.Height + ypad;

            GLGroupBox edsmregionsgb = new GLGroupBox("EDSMR", "EDSM Regions", new Rectangle(leftmargin, vpos, pform.ClientWidth - leftmargin * 2, 50));
            edsmregionsgb.ClientHeight = iconsize + 8;
            edsmregionsgb.BackColor = pform.BackColor;
            edsmregionsgb.ForeColor = Color.Orange;
            pform.Add(edsmregionsgb);
            vpos += edsmregionsgb.Height + ypad;

            GLGroupBox eliteregionsgb = new GLGroupBox("ELITER", "Elite Regions", new Rectangle(leftmargin, vpos, pform.ClientWidth - leftmargin * 2, 50));
            eliteregionsgb.ClientHeight = iconsize + 8;
            eliteregionsgb.BackColor = pform.BackColor;
            eliteregionsgb.ForeColor = Color.Orange;
            pform.Add(eliteregionsgb);
            vpos += eliteregionsgb.Height + ypad;

            GLCheckBox butedre = new GLCheckBox("EDSMRE", new Rectangle(leftmargin, 0, iconsize, iconsize), Properties.Resources.ShowGalaxy, null);
            GLCheckBox butelre = new GLCheckBox("ELITERE", new Rectangle(leftmargin, 0, iconsize, iconsize), Properties.Resources.ShowGalaxy, null);

            butedre.ToolTipText = "Enable EDSM Regions";
            butedre.Checked = map.EDSMRegionsEnable;
            butedre.UserCanOnlyCheck = true;
            edsmregionsgb.Add(butedre);

            butelre.ToolTipText = "Enable Elite Regions";
            butelre.Checked = map.EliteRegionsEnable;
            butelre.UserCanOnlyCheck = true;
            eliteregionsgb.Add(butelre);

            GLCheckBox buted2 = new GLCheckBox("EDSMR2", new Rectangle(50, 0, iconsize, iconsize), Properties.Resources.ShowGalaxy, null);
            buted2.Checked = map.EDSMRegionsOutlineEnable;
            buted2.Enabled = map.EDSMRegionsEnable;
            buted2.ToolTipText = "Enable Region Outlines";
            buted2.CheckChanged += (e1) => { map.EDSMRegionsOutlineEnable = !map.EDSMRegionsOutlineEnable; };
            edsmregionsgb.Add(buted2);
            GLCheckBox buted3 = new GLCheckBox("EDSMR3", new Rectangle(100, 0, iconsize, iconsize), Properties.Resources.ShowGalaxy, null);
            buted3.Checked = map.EDSMRegionsShadingEnable;
            buted3.Enabled = map.EDSMRegionsEnable;
            buted3.ToolTipText = "Enable Region Shading";
            buted3.CheckChanged += (e1) => { map.EDSMRegionsShadingEnable = !map.EDSMRegionsShadingEnable; };
            edsmregionsgb.Add(buted3);
            GLCheckBox buted4 = new GLCheckBox("EDSMR4", new Rectangle(150, 0, iconsize, iconsize), Properties.Resources.ShowGalaxy, null);
            buted4.Checked = map.EDSMRegionsTextEnable;
            buted4.Enabled = map.EDSMRegionsEnable;
            buted4.ToolTipText = "Enable Region Naming";
            buted4.CheckChanged += (e1) => { map.EDSMRegionsTextEnable = !map.EDSMRegionsTextEnable; };
            edsmregionsgb.Add(buted4);


            GLCheckBox butel2 = new GLCheckBox("EDSMR2", new Rectangle(50, 0, iconsize, iconsize), Properties.Resources.ShowGalaxy, null);
            butel2.Checked = map.EliteRegionsOutlineEnable;
            butel2.Enabled = map.EliteRegionsEnable;
            butel2.ToolTipText = "Enable Region Outlines";
            butel2.CheckChanged += (e1) => { map.EliteRegionsOutlineEnable = !map.EliteRegionsOutlineEnable; };
            eliteregionsgb.Add(butel2);
            GLCheckBox butel3 = new GLCheckBox("EDSMR3", new Rectangle(100, 0, iconsize, iconsize), Properties.Resources.ShowGalaxy, null);
            butel3.Checked = map.EliteRegionsShadingEnable;
            butel3.Enabled = map.EliteRegionsEnable;
            butel3.ToolTipText = "Enable Region Shading";
            butel3.CheckChanged += (e1) => { map.EliteRegionsShadingEnable = !map.EliteRegionsShadingEnable; };
            eliteregionsgb.Add(butel3);
            GLCheckBox butel4 = new GLCheckBox("EDSMR4", new Rectangle(150, 0, iconsize, iconsize), Properties.Resources.ShowGalaxy, null);
            butel4.Checked = map.EliteRegionsTextEnable;
            butel4.Enabled = map.EliteRegionsEnable;
            butel4.ToolTipText = "Enable Region Naming";
            butel4.CheckChanged += (e1) => { map.EliteRegionsTextEnable = !map.EliteRegionsTextEnable; };
            eliteregionsgb.Add(butel4);


            butedre.CheckChanged += (e) => 
            {
                if (e.Name == "EDSMRE")
                {
                    butelre.CheckedNoChangeEvent = !butedre.Checked;
                }
                else
                {
                    butedre.CheckedNoChangeEvent = !butelre.Checked;
                }

                map.EDSMRegionsEnable = butedre.Checked;
                map.EliteRegionsEnable = butelre.Checked;

                buted2.Enabled = buted3.Enabled = buted4.Enabled = butedre.Checked;
                butel2.Enabled = butel3.Enabled = butel4.Enabled = butelre.Checked;
            };

            butelre.CheckChanged += butedre.CheckChanged;

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
