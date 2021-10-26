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
using System.Linq;

namespace EDDiscoveryTests
{
    [TestFixture(TestOf = typeof(CSVRead))]
    public class CSVTests
    {
        [Test]
        public void CSVBasic()
        {

            CheckCSV(false);
            CheckCSV(true);

        }

        public void CheckCSV(bool comma)
        {
            string workingfolder = Path.GetTempPath();

            string chks1 = "Helllo,There";
            string chks2 = "Helllo;There";
            CSVWriteGrid wr = new CSVWriteGrid();
            wr.SetCSVDelimiter(comma);
            wr.GetPreHeader += (o) => { if (o == 0) return new object[] { "Pre header" }; else return null; };
            wr.GetLineHeader += (o) => { if (o == 0) return new object[] { "CA", "CB", "CC" }; else return null; };
            wr.GetLine += (i) => { if (i < 10) return new object[] { i.ToStringInvariant(), (i + 1).ToStringInvariant(), (i + 2).ToStringInvariant(), chks1, chks2 }; else return null; };
            wr.WriteCSV(Path.Combine(workingfolder, "comma.csv"));

            CSVFile rd = new CSVFile();
            if (rd.Read(Path.Combine(workingfolder, "comma.csv"), FileShare.ReadWrite, comma))
            {
                CSVFile.Row r0 = rd[0];
                Check.That(r0[0].Equals("Pre header")).IsTrue();
                CSVFile.Row r1 = rd[1];
                Check.That(r1[0].Equals("CA")).IsTrue();
                Check.That(r1[1].Equals("CB")).IsTrue();
                Check.That(r1[2].Equals("CC")).IsTrue();
                for (int i = 0; i < 10; i++)
                {
                    CSVFile.Row rw = rd[2 + i];
                    Check.That(rw[0].Equals((i).ToStringInvariant())).IsTrue();
                    Check.That(rw[1].Equals((i + 1).ToStringInvariant())).IsTrue();
                    Check.That(rw[2].Equals((i + 2).ToStringInvariant())).IsTrue();
                    Check.That(rw[3].Equals(chks1)).IsTrue();
                    Check.That(rw[4].Equals(chks2)).IsTrue();
                }

            }
        }

    }
}