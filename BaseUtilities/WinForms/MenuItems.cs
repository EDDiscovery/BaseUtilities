/*
 * Copyright © 2024 - 2024 EDDiscovery development team
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

using System.Collections.Generic;
using System.Windows.Forms;

public static partial class ControlHelpersStaticFunc
{
    // clears and disposes of a MenuItem DropDownItems tool collection
    // does not dispose itself.
    public static void ClearRecursive(this ToolStripItemCollection c)
    {
        List<ToolStripMenuItem> ls = new List<ToolStripMenuItem>();
        foreach (ToolStripMenuItem ts in c)
            ls.Add(ts);

        c.Clear();

        foreach (ToolStripMenuItem tn in ls)
        {
            tn.ClearRecursive();
            tn.Dispose();
            System.Diagnostics.Debug.WriteLine($"Dispose of {tn.Name} {tn.Text}");
        }
    }

    // clears and disposes of a tool strip Item DropDownItems tool collection
    // does not dispose itself.
    public static void ClearRecursive( this ToolStripMenuItem t)
    {
        List<ToolStripMenuItem> ls = new List<ToolStripMenuItem>();
        foreach (ToolStripMenuItem ts in t.DropDownItems)
        { 
            ls.Add(ts);
            ts.ClearRecursive();
        }

        t.DropDownItems.Clear();

        foreach (ToolStripMenuItem tn in ls)
        {
            tn.Dispose();
            System.Diagnostics.Debug.WriteLine($"  Dispose of {tn.Name} {tn.Text}");
        }
    }
}
