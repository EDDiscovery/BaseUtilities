﻿/*
 * Copyright © 2023 - 2023 EDDiscovery development team
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

public static partial class ObjectExtensionsChars
{
    // compare a buffer area

    public static bool Equals(this char[] buf, string s, int len, int offset = 0)
    {
        if (s.Length == len && offset + len <= buf.Length)     // if len is the same length, and have enough data in buf to covert the area offset+len
        {
            for (int i = 0; i < len; i++)
            {
                if (buf[i + offset] != s[i])
                    return false;
            }

            return true;
        }

        return false;
    }
}
