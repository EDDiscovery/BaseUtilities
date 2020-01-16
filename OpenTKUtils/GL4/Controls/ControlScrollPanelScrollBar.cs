using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public class GLVerticalScrollPanelScrollBar : GLBaseControl
    {
        public new Color BackColor { get { return scrollpanel.BackColor; } set { scrollpanel.BackColor = value;  } }

        public Color ArrowColor { get { return scrollbar.ArrowColor; } set { scrollbar.ArrowColor = value;  } }       // of text
        public Color SliderColor { get { return scrollbar.SliderColor; } set { scrollbar.SliderColor = value;  } }

        public Color ArrowButtonColor { get { return scrollbar.ArrowButtonColor; } set { scrollbar.ArrowButtonColor = value; } }
        public Color ArrowBorderColor { get { return scrollbar.ArrowBorderColor; } set { scrollbar.ArrowBorderColor = value;  } }
        public float ArrowUpDrawAngle { get { return scrollbar.ArrowUpDrawAngle; } set { scrollbar.ArrowUpDrawAngle = value;  } }
        public float ArrowDownDrawAngle { get { return scrollbar.ArrowDownDrawAngle; } set { scrollbar.ArrowDownDrawAngle = value;  } }
        public float ArrowColorScaling { get { return scrollbar.ArrowColorScaling; } set { scrollbar.ArrowColorScaling = value;  } }

        public Color MouseOverButtonColor { get { return scrollbar.MouseOverButtonColor; } set { scrollbar.MouseOverButtonColor = value;  } }
        public Color MousePressedButtonColor { get { return scrollbar.MousePressedButtonColor; } set { scrollbar.MousePressedButtonColor = value;  } }
        public Color ThumbButtonColor { get { return scrollbar.ThumbButtonColor; } set { scrollbar.ThumbButtonColor = value;  } }
        public Color ThumbBorderColor { get { return scrollbar.ThumbBorderColor; } set { scrollbar.ThumbBorderColor = value;  } }
        public float ThumbColorScaling { get { return scrollbar.ThumbColorScaling; } set { scrollbar.ThumbColorScaling = value;  } }
        public float ThumbDrawAngle { get { return scrollbar.ThumbDrawAngle; } set { scrollbar.ThumbDrawAngle = value;  } }

        public new string Name { get { return base.Name; } set { base.Name = value; scrollbar.Name = base.Name + "-SB"; scrollpanel.Name = base.Name + "-SP"; } }
        public override List<GLBaseControl> Controls { get { return scrollpanel.Controls; } }      // read only

        public int ScrollBarWidth { get { return Font?.ScalePixels(20) ?? 20; } }

        public GLVerticalScrollPanelScrollBar()
        {
            scrollpanel = new GLVerticalScrollPanel();
            scrollpanel.Dock = DockingType.Fill;
            base.Add(scrollpanel);  // base because we don't want to use the overrides

            scrollbar = new GLScrollBar();
            scrollbar.Dock = DockingType.Right;
            scrollbar.Width = 20;
            base.Add(scrollbar);     // last added always goes to top of z-order

            scrollbar.Scroll += Scrolled;
        }

        public GLVerticalScrollPanelScrollBar(string name, Rectangle location, Color back) : this()
        {
            Name = name;
            Bounds = location;
            BackColor = back;
        }

        public override void Add(GLBaseControl other)           // we need to override, since we want controls added to the scroll panel not us
        {
            scrollpanel.Add(other);
        }

        public override void Remove(GLBaseControl other)
        {
            scrollpanel.Remove(other);
        }

        private GLScrollBar scrollbar;
        private GLVerticalScrollPanel scrollpanel;

        private void Scrolled(GLBaseControl c, ScrollEventArgs e)
        {
            scrollpanel.ScrollPos = scrollbar.Value;
        }

        public override void PerformLayout()
        {
            if (scrollbar != null)
                scrollbar.Width = ScrollBarWidth;

            base.PerformLayout();   // the docking sorts out the positioning of the controls

            if ( scrollbar != null && scrollpanel != null)
            {
                scrollbar.Maximum = scrollpanel.ScrollRange + scrollbar.LargeChange;
                System.Diagnostics.Debug.WriteLine("Scroll panel range " + scrollbar.Maximum);
            }
        }
    }
}

