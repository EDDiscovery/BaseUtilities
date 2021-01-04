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
using System.Collections.Generic;
using OpenTKUtils;
using OpenTK;

namespace EDDiscoveryTests
{
    [TestFixture(TestOf = typeof(Eval))]
    public class OpenTKUtilsTests
    {
        [Test]
        public void OpenTK()
        {
            { 
                List<Vector2> points = new List<Vector2>();
                points.Add(new Vector2(100, 100));
                points.Add(new Vector2(100, 200));
                points.Add(new Vector2(200, 200));
                points.Add(new Vector2(200, 100));

                Vector2 centroid = PolygonTriangulator.Centroid(points, out float area);
                Check.That(centroid.X == 150 && centroid.Y == 150 && area == 10000).IsTrue();
            }

            {
                List<Vector2> points = new List<Vector2>();

                points.Add(new Vector2(0, 100));
                points.Add(new Vector2(100, 200));
                points.Add(new Vector2(100, 100));
                points.Add(new Vector2(200, 200));
                points.Add(new Vector2(300, 100));
                points.Add(new Vector2(150, 0));
                points.Add(new Vector2(0, 50));

                List<List<Vector2>> res = PolygonTriangulator.Triangulate(points, false);

                Check.That(res.Count == 3).IsTrue();

                Check.That(res[0].Count == 3).IsTrue();
                Check.That(res[0][0] == new Vector2(0, 100)).IsTrue();
                Check.That(res[0][1] == new Vector2(100, 200)).IsTrue();
                Check.That(res[0][2] == new Vector2(100, 100)).IsTrue();

                Check.That(res[1].Count == 4).IsTrue();
                Check.That(res[1][0] == new Vector2(100, 100)).IsTrue();
                Check.That(res[1][1] == new Vector2(200, 200)).IsTrue();
                Check.That(res[1][2] == new Vector2(300, 100)).IsTrue();
                Check.That(res[1][3] == new Vector2(150, 0)).IsTrue();

                Check.That(res[2].Count == 4).IsTrue();
                Check.That(res[2][0] == new Vector2(150, 0)).IsTrue();
                Check.That(res[2][1] == new Vector2(0, 50)).IsTrue();
                Check.That(res[2][2] == new Vector2(0, 100)).IsTrue();
                Check.That(res[1][3] == new Vector2(150, 0)).IsTrue();
            }




        }
    }
}
