/*
 * Copyright © 2017 EDDiscovery development team
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
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlTypes;

namespace BaseUtils
{
    public class CSVWrite
    {
        protected string delimiter = ",";
        protected System.Globalization.CultureInfo formatculture = new System.Globalization.CultureInfo("en-US");

        public void SetCSVDelimiter( bool comma )
        {
            if (comma)
            {
                delimiter = ",";
                formatculture = new System.Globalization.CultureInfo("en-US");
            }
            else
            {
                delimiter = ";";
                formatculture = new System.Globalization.CultureInfo("sv");
            }
        }


        public string Double(double value, bool delimit = true, int decplaces = 2)
        {
            return Format(value, delimit, "N" + decplaces.ToStringInvariant());
        }

        public string Double(double? value, bool delimit = true, int decplaces = 2)
        {
            return Format(value, delimit, "N" + decplaces.ToStringInvariant());
        }

        public string Format(object value, bool delimit = true, string defformat = null)
        {
            string output = "";

            if (value != null && !(value is INullable && ((INullable)value).IsNull))  // if not null
            {
                if (value is DateTime)
                {
                    if (((DateTime)value).TimeOfDay.TotalSeconds == 0)
                        return ((DateTime)value).ToString("yyyy-MM-dd");

                    output = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    if (defformat == null)
                        output = Convert.ToString(value, formatculture);
                    else if (value is double)
                        output = ((double)value).ToString(defformat, formatculture);
                    else if (value is double?)
                        output = ((double?)value).Value.ToString(defformat, formatculture);
                    else if (value is int)
                        output = ((int)value).ToString(defformat, formatculture);
                    else if (value is uint)
                        output = ((uint)value).ToString(defformat, formatculture);
                    else if (value is long)
                        output = ((long)value).ToString(defformat, formatculture);
                    else if (value is ulong)
                        output = ((ulong)value).ToString(defformat, formatculture);
                    else
                        System.Diagnostics.Debug.Assert(false, "defformat does not support this type");

                    if (output.Contains(",") || output.Contains("\"") || output.Contains("\r") || output.Contains("\n"))
                    {
                        output = output.Replace("\r\n", "\n");
                        output = output.Replace("\"", "\"\"");
                        output = "\"" + output + "\"";
                    }
                }
            }

            if (delimit)
                return output + delimiter;
            else
                return output;
        }
    }
}
