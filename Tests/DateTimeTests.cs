/*
* Copyright © 2018 EDDiscovery development team
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
using BaseUtils;
using NFluent;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace EDDiscoveryTests
{
    [TestFixture(TestOf = typeof(Eval))]
    public class DateTimeTests
    {
        [Test]
        public void DateTime()
        {
            {
                DateTime t = new DateTime(1900, 1, 1);

                while (t.Year < 2500)
                {
                    DateTime smend = t.StartOfMonth();
                    DateTime smweek = t.StartOfWeek();
                    DateTime smyear = t.StartOfYear();
                    DateTime tmend = t.EndOfMonth();
                    DateTime tmweek = t.EndOfWeek();
                    DateTime tmyear = t.EndOfYear();
                    t = t.AddDays(1);
                }
            }


        }
    }
}
