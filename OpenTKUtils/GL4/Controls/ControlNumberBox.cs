/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public abstract class GLNumberBox<T> : GLTextBox
    {
        public string Format { get { return format; } set { format = value; base.Text = ConvertToString(Value); } }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Globalization.CultureInfo FormatCulture { get { return culture; } set { culture = value; } }

        public Action<GLBaseControl> ValueChanged;
        public Action<GLBaseControl,bool> ValidityChanged;

        public T Minimum { get; set; }
        public T Maximum { get; set; }

        public bool IsValid { get { T v; return ConvertFromString(base.Text, out v); } }        // is the text a valid value?

        public void SetComparitor(GLNumberBox<T> other, int compare)         // aka -2 (<=) -1(<) 0 (=) 1 (>) 2 (>=)
        {
            othernumberbox = other;
            othercomparision = compare;
            InErrorCondition = !IsValid;
        }

        public void SetBlank()          // Blanks it, but does not declare an error
        {
            base.Text = "";
            InErrorCondition = false;
        }

        public void SetNonBlank()       // restores it to its last value
        {
            base.Text = ConvertToString(Value);
        }

        public T Value                                          // will fire a ValueChanged event
        {
            get { return number; }
            set
            {
                number = value;
                base.Text = ConvertToString(number);            // triggers change text event, which sets validity
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public T ValueNoChange                                  //will not fire a ValueChanged event
        {
            get { return number; }
            set
            {
                number = value;
                base.Text = ConvertToString(number);            // triggers change text event but its ignored
                InErrorCondition = !IsValid;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Text { get { return base.Text; } set { System.Diagnostics.Debug.Assert(false, "Can't set Number box"); } }       // can't set Text, only read..

        #region Implementation

        protected GLNumberBox(string  name, Rectangle pos ) : base(name,pos)
        {
        }

        protected abstract string ConvertToString(T v);
        protected abstract bool ConvertFromString(string t, out T number);
        protected abstract bool AllowedChar(char c);

        private T number;
        private string format = "N";
        private System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.CurrentCulture;
        protected GLNumberBox<T> othernumberbox { get; set; } = null;             // attach to another box for validation
        protected int othercomparision { get; set; } = 0;              // aka -2 (<=) -1(<) 0 (=) 1 (>) 2 (>=)

        protected override void OnTextChanged()
        {
            if (ConvertFromString(Text, out T newvalue))
            {
                number = newvalue;

                OnValueChanged();

                if (InErrorCondition)
                    OnValidityChanged(true);

                InErrorCondition = false;
            }
            else
            {                               // Invalid, indicate
                if (!InErrorCondition)
                    OnValidityChanged(false);

                InErrorCondition = true;
            }

            base.OnTextChanged();
        }

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this);
        }

        protected virtual void OnValidityChanged(bool valid)
        {
            ValidityChanged?.Invoke(this,valid);
        }

        public override void OnKeyPress(GLKeyEventArgs e) // limit keys to whats allowed for a double
        {
            if (AllowedChar(e.KeyChar))
            {
                base.OnKeyPress(e);
            }
            else
            {
                e.Handled = true;
            }
        }

        public override void OnFocusChanged(bool focused, GLBaseControl fromto)
        {
            if ( !focused )
            {
                if (!IsValid)           // if text box is not valid, go back to the original colour with no chanve event
                    ValueNoChange = number;
            }
            base.OnFocusChanged(focused, fromto);
        }

        #endregion
    }

    public class GLNumberBoxFloat : GLNumberBox<float>
    {
        public GLNumberBoxFloat(string name, Rectangle pos, float value) : base(name, pos)
        {
            Minimum = float.MinValue;
            Maximum = float.MaxValue;
            ValueNoChange = value;
        }

        protected override string ConvertToString(float v)
        {
            return v.ToString(Format, FormatCulture);
        }

        protected override bool ConvertFromString(string t, out float number)
        {
            bool ok = float.TryParse(t, System.Globalization.NumberStyles.Float, FormatCulture, out number) &&
                      number >= Minimum && number <= Maximum;
            if (ok && othernumberbox != null)
                ok = number.CompareTo(othernumberbox.Value, othercomparision);
            return ok;
        }

        protected override bool AllowedChar(char c)
        {
          return (char.IsDigit(c) || c == 8 ||
                    (c == FormatCulture.NumberFormat.CurrencyDecimalSeparator[0] && Text.IndexOf(FormatCulture.NumberFormat.CurrencyDecimalSeparator, StringComparison.Ordinal) == -1) ||
                    (c == FormatCulture.NumberFormat.NegativeSign[0] && SelectionStart == 0 && Minimum < 0));
        }
    }

    public class GLNumberBoxDouble : GLNumberBox<double>
    {
        public GLNumberBoxDouble(string name, Rectangle pos, double value) : base(name, pos)
        {
            Minimum = double.MinValue;
            Maximum = double.MaxValue;
            ValueNoChange = value;
        }

        protected override string ConvertToString(double v)
        {
            return v.ToString(Format, FormatCulture);
        }
        protected override bool ConvertFromString(string t, out double number)
        {
            bool ok = double.TryParse(t, System.Globalization.NumberStyles.Float, FormatCulture, out number) &&
                number >= Minimum && number <= Maximum;
            if (ok && othernumberbox != null)
                ok = number.CompareTo(othernumberbox.Value, othercomparision);
            return ok;
        }

        protected override bool AllowedChar(char c)
        {
            return (char.IsDigit(c) || c == 8 ||
                (c == FormatCulture.NumberFormat.CurrencyDecimalSeparator[0] && Text.IndexOf(FormatCulture.NumberFormat.CurrencyDecimalSeparator) == -1) ||
                (c == FormatCulture.NumberFormat.NegativeSign[0] && SelectionStart == 0 && Minimum < 0));
        }
    }

    public class GLNumberBoxLong : GLNumberBox<long>
    {
        public GLNumberBoxLong(string name, Rectangle pos, long value) : base(name, pos)
        {
            Minimum = long.MinValue;
            Maximum = long.MaxValue;
            ValueNoChange = 0;
            Format = "D";
        }

        protected override string ConvertToString(long v)
        {
            return v.ToString(Format, FormatCulture);
        }
        protected override bool ConvertFromString(string t, out long number)
        {
            bool ok = long.TryParse(t, System.Globalization.NumberStyles.Integer, FormatCulture, out number) &&
                            number >= Minimum && number <= Maximum;
            if (ok && othernumberbox != null)
                ok = number.CompareTo(othernumberbox.Value, othercomparision);
            return ok;
        }

        protected override bool AllowedChar(char c)
        {
            return (char.IsDigit(c) || c == 8 ||
                (c == FormatCulture.NumberFormat.NegativeSign[0] && SelectionStart == 0 && Minimum < 0));
        }
    }


}
