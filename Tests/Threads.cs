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
using QuickJSON;
using BaseUtils.Threads;
using NFluent;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace EDDiscoveryTests
{
    [TestFixture(TestOf = typeof(JToken))]
    public class ThreadTests
    {
        [Test]
        public void ThreadTest1()
        {
            TaskQueue tq = new TaskQueue();

            for (int i = 0; i < 2; i++)
            {
                System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Queue 1");
                tq.Enqueue(() => {
                    System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Task 1");
                    Thread.Sleep(500);
                    System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Task 1 END");
                });
                System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Queue 2");
                tq.Enqueue(() => {
                    System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Task 2");
                    Thread.Sleep(250);
                    System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000) + "Task 2 END");
                });

                Thread.Sleep(200);
            }

            Thread.Sleep(10000);
        }

    }
}