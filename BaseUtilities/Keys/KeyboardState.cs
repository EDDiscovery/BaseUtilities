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
        public enum ShiftState
        {
            None = 0,
            Shift = 1,
            Ctrl = 2,
            Alt = 4,
        };

        public ShiftState SetShift(bool ctrl, bool shift, bool alt)
        {
            return (ShiftState)((ctrl ? ShiftState.Ctrl : 0) | (shift ? ShiftState.Shift : 0) | (alt ? ShiftState.Alt : 0));
        }

        public bool IsPressedRemove(Keys key, ShiftState state)       // is pressed, remove from list saying it has been pressed.  Only true if shift state matches
        {
            bool ret = false;
            if (hasbeenpressed.ContainsKey(key))
            {
                ret = state == hasbeenpressed[key];
                hasbeenpressed.Remove(key);
            }

            return ret;
        }

        public bool IsPressedRemove(Keys key)                       // is pressed, remove from list saying it has been pressed.  True if no shift state
        {
            bool ret = false;
            if (hasbeenpressed.ContainsKey(key) )
            {
                ret = hasbeenpressed[key] == ShiftState.None;
                hasbeenpressed.Remove(key);
            }

            return ret;
        }

        public void ClearPressed()                                    // all has been checked, clear them
        {
            hasbeenpressed.Clear();
        }

        public bool IsPressed(ShiftState state, Keys key)             // is currently pressed and in this shift state
        {
            return keyspressed.ContainsKey(key) && keyspressed[key] == state;
        }

        public ShiftState? IsPressed(Keys key)                        // is currently pressed and in any shift state
        {
            if (keyspressed.ContainsKey(key))
                return keyspressed[key];
            else
                return null;
        }

        public bool IsAnyPressed(ShiftState state, params Keys[] keys)  // is currently pressed and in this shift state
        {
            foreach (var k in keys)
            {
                if (IsPressed(state, k))
                    return true;
            }

            return false;
        }

        public ShiftState? IsAnyPressed(params Keys[] keys)  // is currently pressed and in this shift state
        {
            foreach (var k in keys)
            {
                ShiftState? s = IsPressed(k);
                if (s != null)
                    return s;
            }

            return null;
        }

        private Dictionary<Keys, ShiftState> keyspressed = new Dictionary<Keys, ShiftState>();
        private Dictionary<Keys, ShiftState> hasbeenpressed = new Dictionary<Keys, ShiftState>();

        public bool Ctrl { get; private set; } = false;
        public bool Alt { get; private set; } = false;
        public bool Shift { get; private set; } = false;

        public void Reset()
        {
            Ctrl = Alt = Shift = false;
            keyspressed.Clear();
            hasbeenpressed.Clear();
        }

        public void KeyDown(object sender, KeyEventArgs e)      // hook to handler
        {
            Ctrl = e.Control;
            Alt = e.Alt;
            Shift = e.Shift;
            keyspressed[e.KeyCode] = SetShift(e.Control, e.Shift, e.Alt);
            hasbeenpressed[e.KeyCode] = SetShift(e.Control, e.Shift, e.Alt);
        }

        public void KeyUp(object sender, KeyEventArgs e)        // hook to handler
        {
            Ctrl = e.Control;
            Alt = e.Alt;
            Shift = e.Shift;
            keyspressed.Remove(e.KeyCode);
        }
    }
}
