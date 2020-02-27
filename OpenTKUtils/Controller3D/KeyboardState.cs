/*
 * Copyright © 2015 - 2020 EDDiscovery development team
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

namespace OpenTKUtils.Common
{
    // hook Keydown/Keyup to it, then it keeps track of what keys are currently pressed

    public class KeyboardMonitor
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

        // from the has been pressed list

        public bool HasBeenPressed(Keys key, ShiftState state)       // is pressed and in this state, remove from list saying it has been pressed.
        {
            bool ret = false;
            if (hasbeenpressed.ContainsKey(key) && hasbeenpressed[key] == state)
            {
                ret = true;
                hasbeenpressed.Remove(key);
            }

            return ret;
        }

        public bool HasBeenPressed(Keys key)                        // is pressed, remove from list saying it has been pressed.  True if no shift state
        {
            bool ret = false;
            if (hasbeenpressed.ContainsKey(key) && hasbeenpressed[key] == ShiftState.None)
            {
                ret = true;
                hasbeenpressed.Remove(key);
            }

            return ret;
        }

        public void ClearHasBeenPressed()                                    // all has been checked, clear them
        {
            hasbeenpressed.Clear();
        }

        // Currently pressed

        public bool IsCurrentlyPressed(ShiftState state, Keys key)             // is currently pressed and in this shift state
        {
            return keyspressed.ContainsKey(key) && keyspressed[key] == state;
        }

        public ShiftState? IsCurrentlyPressed(Keys key)                        // is currently pressed and in any shift state
        {
            if (keyspressed.ContainsKey(key))
                return keyspressed[key];
            else
                return null;
        }

        public bool IsCurrentlyPressed(ShiftState state, params Keys[] keys)  // is currently pressed and in this shift state
        {
            System.Diagnostics.Debug.Assert(keys.Length > 0);
            foreach (var k in keys)
            {
                if (IsCurrentlyPressed(state, k))
                    return true;
            }

            return false;
        }

        public ShiftState? IsCurrentlyPressed(params Keys[] keys)  // is currently pressed and in this shift state
        {
            System.Diagnostics.Debug.Assert(keys.Length > 0);
            foreach (var k in keys)
            {
                ShiftState? s = IsCurrentlyPressed(k);
                if (s != null)
                    return s;
            }

            return null;
        }

        public bool IsAnyCurrentlyPressed()
        {
            return keyspressed.Count > 0;
        }

        public bool IsAnyCurrentlyOrHasBeenPressed()
        {
            return keyspressed.Count > 0 || hasbeenpressed.Count > 0;
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

        public void KeyDown(bool c, bool s, bool a, Keys keycode)      // hook to handler
        {
            Ctrl = c;
            Alt = a;
            Shift = s;
            keyspressed[keycode] = SetShift(c, s, a);
            hasbeenpressed[keycode] = SetShift(c, s, a);
            //System.Diagnostics.Debug.WriteLine("Keycode down " + keycode);
        }

        public void KeyUp(bool c, bool s, bool a, Keys keycode)      // hook to handler
        {
            Ctrl = c;
            Alt = a;
            Shift = s;
            keyspressed.Remove(keycode);
            //System.Diagnostics.Debug.WriteLine("Keycode up " + keycode);
        }
    }
}
