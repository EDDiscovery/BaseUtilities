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

            // compute barwidth, and if percentange set, set a minimum
            int barwidth = (int)((percent * cellBounds.Width /100.0 - 4));
            if ( percent>0)
                barwidth = Math.Max(BarMinPixelSize, barwidth);     

            // set bar area
            Rectangle bararea = new Rectangle(cellBounds.X + 2, cellBounds.Y + 2, barwidth, cellBounds.Height - 4);

            // measure text
            string text = string.Format(PercentageTextFormat, percent);
            var size = TextRenderer.MeasureText(text, cellStyle.Font, new Size(30000, 1000));

            // work out if text will fit in bar, or needs to be to the right. Text preference helps set the condition
            bool texttoright = TextToRightPreferentially ? (barwidth + size.Width + 4 < cellBounds.Width) : size.Width + 4 > barwidth;

            // if we have a bar..
            if (bararea.Width > 0)
            {
                using (Brush foreColorBarBrush = new LinearGradientBrush(bararea, BarForeColor, BarForeColor.Multiply(BarColorScaling), 90))
                {
                    g.FillRectangle(foreColorBarBrush, bararea);
                }
            }

            // paint text
            using (Brush textBrush = new SolidBrush(this.DataGridView.CurrentRow.Index == rowIndex && texttoright ? cellStyle.SelectionForeColor : cellStyle.ForeColor))
            { 
                if (texttoright)
                {
                    g.DrawString(text, cellStyle.Font, textBrush, bararea.X + barwidth, bararea.Y);
                }
                else
                {
                    using (StringFormat fmt = new StringFormat() { Alignment = StringAlignment.Center })
                    {
                        g.DrawString(text, cellStyle.Font, textBrush, bararea, fmt);
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

            using (Brush backColorBrush = new SolidBrush(cellStyle.BackColor))
            {
                using (Brush foreColorBrush = new SolidBrush(cellStyle.ForeColor))
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
