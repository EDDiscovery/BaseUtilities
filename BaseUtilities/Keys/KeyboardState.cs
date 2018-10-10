/*
 * Copyright © 2015 - 2016 EDDiscovery development team
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
using System.Windows.Forms;

namespace BaseUtils
{
    // hook Keydown/Keyup to it, then it keeps track of what keys are currently pressed

    public class KeyboardState
    {
        public bool IsPressedRemove(Keys key)       // is pressed, remove from list to say you've processed it
        {
            if (hasbeenpressed.Contains(key))
            {
                hasbeenpressed.Remove(key);
                return true;
            }
            else
                return false;
        }

        public bool IsPressed(Keys key)             // is currently pressed
        {
            return keyspressed.Contains(key);
        }

        public bool IsAnyPressed( params Keys[] keys) // is currently pressed
        {
            foreach (var k in keys)
            {
                if (IsPressed(k))
                    return true;
            }

            return false;
        }

        public bool Shift { get { return shiftpressed; } }              // is shift pressed?
        public bool Ctrl { get { return ctrlpressed; } }                // is ctrl pressed?

        private HashSet<Keys> keyspressed = new HashSet<Keys>();        // set of current keys pressed...
        private HashSet<Keys> hasbeenpressed = new HashSet<Keys>();        // set of current not yet processed
        private bool shiftpressed = false;
        private bool ctrlpressed = false;

        public void Reset()
        {
            shiftpressed = false;
            ctrlpressed = false;
            keyspressed.Clear();
            hasbeenpressed.Clear();
        }

        public void KeyDown(object sender, KeyEventArgs e)      // hook to handler
        {
            ctrlpressed = e.Control;
            shiftpressed = e.Shift;
            keyspressed.Add(e.KeyCode);
            hasbeenpressed.Add(e.KeyCode);                      // yes its pressed - add to queue
        }

        public void KeyUp(object sender, KeyEventArgs e)        // hook to handler
        {
            ctrlpressed = e.Control;
            shiftpressed = e.Shift;
            keyspressed.Remove(e.KeyCode);
        }
    }
}
