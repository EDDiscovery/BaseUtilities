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
using QuickJSON;
using NFluent;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static BaseUtils.EnhancedSendKeysParser;
using System.Windows.Forms;

namespace EDDiscoveryTests
{
    [TestFixture(TestOf = typeof(string))]
    public class KeyParser
    {
        class Bindings : IAdditionalKeyParser
        {
            public Tuple<string, string> Parse(ref string s)
            {
                if (s.StartsWith("{one}"))
                {
                    s = s.Substring(5);
                    return new Tuple<string, string>("!LControlKey !LShiftKey A ^LShiftKey ^LControlKey", null);
                }
                if (s.StartsWith("{UI_Up}"))
                {
                    s = s.Substring(7);
                    return new Tuple<string, string>("Up", null);
                }

                return null;
            }
        }

        void CheckKey(Queue<EnhancedSendKeysParser.SKEvent> events, Keys vkey, int wm, int delay)
        {
            var f1 = events.Dequeue(); 
            Check.That(f1.vkey == vkey && f1.wm == wm && f1.delay == delay).IsTrue();
        }

        [Test]
        public void Key1()
        {

            {
                Queue<EnhancedSendKeysParser.SKEvent> events = new Queue<EnhancedSendKeysParser.SKEvent>();
                string res = BaseUtils.EnhancedSendKeysParser.ParseKeys(events, "Ctrl+A", 100, 120, 140);
                Check.That(res).IsEmpty();
                Check.That(events.Count).Equals(4);
                var f1 = events.Dequeue(); Check.That(f1.vkey == System.Windows.Forms.Keys.ControlKey && f1.wm == 256).IsTrue();
                var f2 = events.Dequeue(); Check.That(f2.vkey == System.Windows.Forms.Keys.A && f2.wm == 256).IsTrue();
                var f3 = events.Dequeue(); Check.That(f3.vkey == System.Windows.Forms.Keys.A && f3.wm == 257).IsTrue();
                var f4 = events.Dequeue(); Check.That(f4.vkey == System.Windows.Forms.Keys.ControlKey && f4.wm == 257).IsTrue();
            }
            {
                Queue<EnhancedSendKeysParser.SKEvent> events = new Queue<EnhancedSendKeysParser.SKEvent>();
                string res = BaseUtils.EnhancedSendKeysParser.ParseKeys(events, "[500,600]Ctrl", 100, 120, 140);
                Check.That(res).IsEmpty();
                Check.That(events.Count).Equals(2);
                var f1 = events.Dequeue(); Check.That(f1.vkey == System.Windows.Forms.Keys.ControlKey && f1.wm == 256 && f1.delay == 500).IsTrue();
                var f2 = events.Dequeue(); Check.That(f2.vkey == System.Windows.Forms.Keys.ControlKey && f2.wm == 257 && f2.delay == 600).IsTrue();
            }
            {
                Queue<EnhancedSendKeysParser.SKEvent> events = new Queue<EnhancedSendKeysParser.SKEvent>();
                string res = BaseUtils.EnhancedSendKeysParser.ParseKeys(events, "[500,600]#2Ctrl", 100, 120, 140);
                Check.That(res).IsEmpty();
                Check.That(events.Count).Equals(4);
                var f1 = events.Dequeue(); Check.That(f1.vkey == System.Windows.Forms.Keys.ControlKey && f1.wm == 256 && f1.delay == 500).IsTrue();
                var f2 = events.Dequeue(); Check.That(f2.vkey == System.Windows.Forms.Keys.ControlKey && f2.wm == 257 && f2.delay == 600).IsTrue();
                var f3 = events.Dequeue(); Check.That(f3.vkey == System.Windows.Forms.Keys.ControlKey && f3.wm == 256 && f3.delay == 500).IsTrue();
                var f4 = events.Dequeue(); Check.That(f4.vkey == System.Windows.Forms.Keys.ControlKey && f4.wm == 257 && f4.delay == 600).IsTrue();
            }
            {
                Queue<EnhancedSendKeysParser.SKEvent> events = new Queue<EnhancedSendKeysParser.SKEvent>();
                string res = BaseUtils.EnhancedSendKeysParser.ParseKeys(events, "[501,502]A", 100, 120, 140);
                Check.That(res).IsEmpty();
                Check.That(events.Count).Equals(2);
                var f1 = events.Dequeue(); Check.That(f1.vkey == System.Windows.Forms.Keys.A && f1.wm == 256 && f1.delay == 501).IsTrue();
                var f2 = events.Dequeue(); Check.That(f2.vkey == System.Windows.Forms.Keys.A && f2.wm == 257 && f2.delay == 502).IsTrue();
            }
            {
                var additionalkeyparser = new Bindings();
                Queue<EnhancedSendKeysParser.SKEvent> events = new Queue<EnhancedSendKeysParser.SKEvent>();
                string res = BaseUtils.EnhancedSendKeysParser.ParseKeys(events, "!LControlKey !LShiftKey A ^LShiftKey ^LControlKey", 100, 120, 140, additionalkeyparser);
                Check.That(res).IsEmpty();
                Check.That(events.Count).Equals(6);
                CheckKey(events, System.Windows.Forms.Keys.LControlKey, 256, 100);
                CheckKey(events, System.Windows.Forms.Keys.LShiftKey, 256, 100);
                CheckKey(events, System.Windows.Forms.Keys.A, 256, 100);
                CheckKey(events, System.Windows.Forms.Keys.A, 257, 140);
                CheckKey(events, System.Windows.Forms.Keys.LShiftKey, 257, 140);
                CheckKey(events, System.Windows.Forms.Keys.LControlKey, 257, 140);
            }
            {
                var additionalkeyparser = new Bindings();
                Queue<EnhancedSendKeysParser.SKEvent> events = new Queue<EnhancedSendKeysParser.SKEvent>();
                string res = BaseUtils.EnhancedSendKeysParser.ParseKeys(events, "{one}", 100, 120, 140, additionalkeyparser);
                Check.That(res).IsEmpty();
                Check.That(events.Count).Equals(6);
                CheckKey(events, System.Windows.Forms.Keys.LControlKey, 256, 100);
                CheckKey(events, System.Windows.Forms.Keys.LShiftKey, 256, 100);
                CheckKey(events, System.Windows.Forms.Keys.A, 256, 100);
                CheckKey(events, System.Windows.Forms.Keys.A, 257, 140);
                CheckKey(events, System.Windows.Forms.Keys.LShiftKey, 257, 140);
                CheckKey(events, System.Windows.Forms.Keys.LControlKey, 257, 140);
            }

            {
                var additionalkeyparser = new Bindings();
                Queue<EnhancedSendKeysParser.SKEvent> events = new Queue<EnhancedSendKeysParser.SKEvent>();
                string res = BaseUtils.EnhancedSendKeysParser.ParseKeys(events, "[50,60]#2{one}", 100, 120, 140, additionalkeyparser);
                Check.That(res).IsEmpty();
                Check.That(events.Count).Equals(12);
                CheckKey(events, System.Windows.Forms.Keys.LControlKey, 256, 50);
                CheckKey(events, System.Windows.Forms.Keys.LShiftKey, 256, 50);
                CheckKey(events, System.Windows.Forms.Keys.A, 256, 50);
                CheckKey(events, System.Windows.Forms.Keys.A, 257, 60);
                CheckKey(events, System.Windows.Forms.Keys.LShiftKey, 257, 50);
                CheckKey(events, System.Windows.Forms.Keys.LControlKey, 257, 50);
                CheckKey(events, System.Windows.Forms.Keys.LControlKey, 256, 50);
                CheckKey(events, System.Windows.Forms.Keys.LShiftKey, 256, 50);
                CheckKey(events, System.Windows.Forms.Keys.A, 256, 50);
                CheckKey(events, System.Windows.Forms.Keys.A, 257, 60);
                CheckKey(events, System.Windows.Forms.Keys.LShiftKey, 257, 50);
                CheckKey(events, System.Windows.Forms.Keys.LControlKey, 257, 50);
            }
            {
                var additionalkeyparser = new Bindings();
                Queue<EnhancedSendKeysParser.SKEvent> events = new Queue<EnhancedSendKeysParser.SKEvent>();
                string res = BaseUtils.EnhancedSendKeysParser.ParseKeys(events, "#2{UI_Up}", 100, 120, 140, additionalkeyparser);
                Check.That(res).IsEmpty();
                Check.That(events.Count).Equals(4);
                CheckKey(events, System.Windows.Forms.Keys.Up, 256, 100);
                CheckKey(events, System.Windows.Forms.Keys.Up, 257, 140);
                CheckKey(events, System.Windows.Forms.Keys.Up, 256, 100);
                CheckKey(events, System.Windows.Forms.Keys.Up, 257, 140);
            }

            {
                var additionalkeyparser = new Bindings();
                Queue<EnhancedSendKeysParser.SKEvent> events = new Queue<EnhancedSendKeysParser.SKEvent>();
                string res = BaseUtils.EnhancedSendKeysParser.ParseKeys(events, "!{UI_Up}", 100, 120, 140, additionalkeyparser);
                Check.That(res).IsEmpty();
                Check.That(events.Count).Equals(1);
                CheckKey(events, System.Windows.Forms.Keys.Up, 256, 100);
            }

        }
    }
}
