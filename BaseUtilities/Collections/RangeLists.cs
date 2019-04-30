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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseUtils
{
    // optimised list of ranges.  merges/demerges as required

    // Can't use generics, as it does not spuidly support a where T is numeric (that i can find)

    public class IntRangeList 
    { 
        public class Range
        {
            public int Start;
            public int End;
            public Range(int s, int e) { Start = s; End = e; }
        }

        public List<Range> Ranges = new List<Range>();

        public IntRangeList()
        {
        }

        public void Add(int start, int end)
        {
            if (Ranges.Count == 0)
                Ranges.Add(new Range(start, end));
            else
            {
                Range sr = Ranges.Find(x => start >= x.Start && start <= x.End+1);      // include next cell along
                Range er = Ranges.Find(x => end >= x.Start-1 && end <= x.End);      // and at start

                if (sr != null && er != null) // three way merge
                {
                    if ( sr != er )         // no action if all inside this one.  else we have two blocks, which the new one overlaps
                    {
                        sr.End = er.End;        // span from st.Start to er.End
                        Ranges.Remove(er);      // no need for er
                    }
                }
                else if (sr != null)      // back merge
                {
                    sr.End = end;           // move end to new end
                }
                else if (er != null)      // front merge
                {
                    er.Start = start;       // move start to new start
                }
                else
                {
                    List<Range> todelete = new List<Range>();       // check to see if we are subsuming ranges
                    foreach (var r in Ranges)
                    {
                        if (r.Start >= start && r.End <= end) // if within this
                            todelete.Add(r);
                    }

                    foreach (var r in todelete)
                        Ranges.Remove(r);

                    Ranges.Add(new Range(start, end));  // no overlap
                }
            }
        }

        public void Remove(int start, int end)
        {
            Range sr = Ranges.Find(x => start >= x.Start && start <= x.End);        // find start one
            Range er = Ranges.Find(x => end >= x.Start && end <= x.End);            // find end one. since we always keep unique list this should be same if entirely encased

            if ( sr != null && er != null )     // both start and end inside a range - may not be the same one
            {
                if ( sr == er )                 // if within same block
                {
                    int foundend = er.End;              // need to cache because sr will change it

                    if (start == sr.Start)              // if start is the same as sr, we end up with one end block..
                        Ranges.Remove(sr);              // so we don't need this one
                    else
                        sr.End = start - 1;             // this one becomes the start, up to start-1

                    if (end != foundend)              // if end is not er.End, we need a new tail block
                        Ranges.Add(new Range(end + 1, foundend));      // add a new range for the tail portion
                }
                else
                {                                       // across two blocks..
                    if (start == sr.Start)              // if start is the same as sr, we end up with one end block..
                        Ranges.Remove(sr);              // so we don't need this one
                    else
                        sr.End = start - 1;             // this one becomes the start, up to start-1

                    if (end == er.End)                  // if end is the same as er.End, whole block disappears
                        Ranges.Remove(er);
                    else
                        er.Start = end + 1;             // else its start is moved to this
                }
            }
            else if ( sr != null )              // start is, end not..
            {
                sr.End = start-1;               // move end to our start less 1

                if (sr.Start > sr.End)          // if moved past end.. block disappears. This occurs if start = sr.start
                    Ranges.Remove(sr);
            }
            else if ( er != null )              // end is, start not
            {
                er.Start = end + 1;             // move start to our end+1

                if (er.Start > er.End)          // if moved past end.. block disappears. This occurs if end = er.End
                    Ranges.Remove(er);
            }

            List<Range> todelete = new List<Range>();       // also check to see if any ranges are entirely within this area
            foreach (var r in Ranges)
            {
                if (r.Start >= start && r.End <= end) // if within this
                    todelete.Add(r);
            }

            foreach (var r in todelete)
                Ranges.Remove(r);
        }

        public void Sort()
        {
            Ranges.Sort(delegate (Range s, Range e) { return s.Start.CompareTo(e.Start); });
        }

        public void Dump()
        {
            System.Diagnostics.Debug.WriteLine("Total " + Ranges.Count);
            foreach (Range r in Ranges)
                System.Diagnostics.Debug.WriteLine("  Range " + r.Start + "-" + r.End);
        }

    }
}
