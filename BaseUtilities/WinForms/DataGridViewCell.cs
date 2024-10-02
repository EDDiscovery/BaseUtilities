/*
 * Copyright © 2024 - 2024 EDDiscovery development team
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BaseUtils
{
    // gives you more cell choises

    public class DataGridViewExtImageCellBase : DataGridViewImageCell
    {
        // Used to make custom cell consistent with a DataGridViewImageCell
        protected static Image emptyImage;
        static DataGridViewExtImageCellBase()
        {
            emptyImage = new Bitmap(1, 1);
        }

        // Return empty image as get formatted value
        protected override object GetFormattedValue(object value,
                            int rowIndex, ref DataGridViewCellStyle cellStyle,
                            TypeConverter valueTypeConverter,
                            TypeConverter formattedValueTypeConverter,
                            DataGridViewDataErrorContexts context)
        {
            return emptyImage;
        }
    }


    // progress bar. Value can be int or float or double
    public class DataGridViewProgressCell : DataGridViewExtImageCellBase
    {
        public Color BarForeColor { get; set; } = Color.FromArgb(203, 235, 108);
        public float BarColorScaling { get; set; } = 0.5F;
        public int BarMinPixelSize { get; set; } = 4;       // when %>0T
        public bool TextToRightPreferentially { get; set; } = false;
        public string PercentageTextFormat { get; set; } = "{0:0.#}%";
        public int BarMaxHeight { get; set; } = 0;  // set to max height limit in pixels. 0 means no limit
        public int BarMinHeight { get; set; } = 4;  // set to min height limit in pixels
        public float BarHeightPercentage { get; set; } = 100;  // set to % of cell height (less margin)
        public Padding Margin { get; set; } = new Padding(2,2,2,2);

        // float is it preferred type
        public DataGridViewProgressCell()
        {
            this.ValueType = typeof(float);
        }

        // clone our values as well, as recommended by https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.datagridviewimagecell?view=windowsdesktop-8.0        
        public override object Clone()
        {
            DataGridViewProgressCell n = base.Clone() as DataGridViewProgressCell;
            n.BarForeColor = this.BarForeColor;
            n.BarColorScaling = this.BarColorScaling;
            n.BarMinPixelSize = this.BarMinPixelSize;
            n.TextToRightPreferentially = this.TextToRightPreferentially;
            n.PercentageTextFormat = this.PercentageTextFormat;
            n.BarMaxHeight = this.BarMaxHeight;
            n.BarMinHeight = this.BarMinHeight;
            n.BarHeightPercentage = this.BarHeightPercentage;
            n.Margin = this.Margin;
            return n;
        }

        protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle clipBounds, System.Drawing.Rectangle cellBounds, int rowIndex,
                        DataGridViewElementStates cellState, object value, object formattedValue, string errorText,
                        DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            // Draws the cell grid with an empty image
            base.Paint(g, clipBounds, cellBounds,
                rowIndex, cellState, value, formattedValue, errorText,
                cellStyle, advancedBorderStyle, (paintParts & ~DataGridViewPaintParts.ContentForeground));

            float percent = value is int ? ((int)value) : value is double ? ((float)(double)value) : ((float)value);
            percent = percent.Range(0, 100);

            int cellwidthavailable = cellBounds.Width - Margin.Left - Margin.Right;

            // compute barwidth, and if percentange set, set a minimum
            int barwidth = (int)(percent * (float)cellwidthavailable/100.0f);
            if ( percent>0)
                barwidth = Math.Max(BarMinPixelSize, barwidth);

            int barheight = (int)((float)(cellBounds.Height - Margin.Top - Margin.Bottom) * BarHeightPercentage / 100.0f);
            barheight = BarMaxHeight > 0 ? Math.Min(BarMaxHeight, barheight) : barheight;
            barheight = Math.Max(barheight, BarMinHeight);

            // set bar area
            Rectangle bararea = new Rectangle(cellBounds.X + Margin.Left, cellBounds.Y + cellBounds.Height/2 - barheight/2, barwidth, barheight);

            //System.Diagnostics.Debug.WriteLine($"row {RowIndex} Percent {percent} bounds {cellBounds} width {barwidth} height {barheight} {bararea} col {BarForeColor}");

            // measure text
            string text = string.Format(PercentageTextFormat, percent);
            var size = TextRenderer.MeasureText(text, cellStyle.Font, new Size(30000, 1000));

            // work out if text will fit in bar, or needs to be to the right. Text preference helps set the condition
            bool texttoright = TextToRightPreferentially ? (barwidth + size.Width + Margin.Left < cellBounds.Width) : size.Width + Margin.Left > barwidth;

            // if we have a bar..
            if (bararea.Width > 0)
            {
                using (Brush foreColorBarBrush = new LinearGradientBrush(bararea, BarForeColor, BarForeColor.Multiply(BarColorScaling), 90))
                {
                    g.FillRectangle(foreColorBarBrush, bararea);
                }
            }

            // paint text
            using (Brush textBrush = new SolidBrush(this.Selected ? cellStyle.SelectionForeColor : cellStyle.ForeColor))        // if selected, inverse video
            { 
                if (texttoright)
                {
                    using (StringFormat fmt = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
                    {
                        var textarea = new Rectangle(bararea.X + bararea.Width, cellBounds.Y, cellBounds.Width - bararea.Width, cellBounds.Height);
                        g.DrawString(text, cellStyle.Font, textBrush, textarea, fmt);
                    }
                }
                else
                {
                    using (StringFormat fmt = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    {
                        var textarea = new Rectangle(bararea.X, cellBounds.Y, bararea.Width, cellBounds.Height);
                        g.DrawString(text, cellStyle.Font, textBrush, textarea, fmt);
                    }
                }

            }
        }
    }

    // Text as an image cell

    public class DataGridViewTextImageCell : DataGridViewExtImageCellBase
    {
        public DataGridViewTextImageCell()
        {
            this.ValueType = typeof(string);
        }

        protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle clipBounds, System.Drawing.Rectangle cellBounds, int rowIndex,
                            DataGridViewElementStates cellState,
                            object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle,
                            DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            string text = (string)value;

            // Draws the cell grid
            base.Paint(g, clipBounds, cellBounds,  rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, (paintParts & ~DataGridViewPaintParts.ContentForeground));

            using (Brush backColorBrush = new SolidBrush(this.Selected ? cellStyle.SelectionForeColor : cellStyle.BackColor))       // inverse video on selection
            {
                using (Brush foreColorBrush = new SolidBrush(!this.Selected ? cellStyle.SelectionForeColor : cellStyle.BackColor))
                {
                    using (StringFormat fmt = cellStyle.Alignment.StringFormatFromDataGridViewContentAlignment())
                    {
                        g.DrawString(text, cellStyle.Font, foreColorBrush, cellBounds, fmt);
                    }
                }
            }
        }
    }


}
