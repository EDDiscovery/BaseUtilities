﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousCore
{
    public class EliteNameClassifier
    {
        public const string NonStandard = "NonStandard";

        public enum NameType { NonStandard, Identifier, Masscode, NValue, N1ValueOnly };
        public NameType EntryType = NameType.NonStandard;   // 0 non standard, 1 CQ-L, 2 mass code, 3 N1, 4 N2

        public string SectorName = null;    // for string inputs, set always, the sector name or "NonStandard".            For numbers, null

        public string StarName = null;      // for string inputs, set for surveys and non standard names                   For numbers, null
        public long NameId = 0;             // set for number inputs

        public uint L1, L2, L3, MassCode, NValue;

        // ID Standard
        //      ID Bit 42 = 1
        //      ID Bit 37 = L1 (1 = A, 26=Z)
        //      ID Bit 32 = L2 (1 = A, 26=Z)
        //      ID Bit 27 = L3 (1 = A, 26=Z)   
        //      ID Bit 24 = mass code (0=A,7=H)
        //      ID Bit 0 = N2 + N1<<16     
        // Non standard:
        //      ID Bit 42 = 0
        //      ID Bit 0..41 = ID from name table

        private const int StandardPosMarker = 44;
        private const int L1Marker = 38;
        private const int L2Marker = 33;
        private const int L3Marker = 28;
        private const int MassMarker = 24;
        private const int NMarker = 0;

        public bool IsStandard { get { return EntryType>=NameType.NValue; } }

        public ulong ID
        {
            get      
            {
                if (EntryType != NameType.NonStandard)
                {
                    System.Diagnostics.Debug.Assert(L1 < 31 && L2 < 32 && L3 < 32 && NValue < 0xffffff && MassCode < 8);
                    return ((ulong)NValue << NMarker) | ((ulong)(MassCode) << MassMarker) | ((ulong)(L3) << L3Marker) | ((ulong)(L2) << L2Marker) | ((ulong)(L1) << L1Marker) | (1UL << StandardPosMarker);
                }
                else
                    return (ulong)(NameId);
            }
        }
        public ulong IDHigh
        {
            get      
            {
                if (EntryType != NameType.NonStandard)
                {
                    ulong lcodes = ((ulong)(L3) << L3Marker) | ((ulong)(L2) << L2Marker) | ((ulong)(L1) << L1Marker) | (1UL << StandardPosMarker);

                    if (EntryType == NameType.Identifier)
                        return ((1UL << L3Marker) - 1) | lcodes;

                    lcodes |= ((ulong)(MassCode) << MassMarker);

                    if (EntryType == NameType.Masscode)
                        return ((1UL << MassMarker) - 1) | lcodes;

                    if (EntryType == NameType.N1ValueOnly)
                        return lcodes | ((ulong)NValue << NMarker) | 0xffff; // N1 explicit (d23-) then we can assume a wildcard in the bottom N2
                    else 
                        return lcodes | ((ulong)NValue << NMarker); // no wild card here
                }
                else
                    return (ulong)(NameId);
            }
        }

        public static bool IsIDStandard( ulong id)
        {                
            return (id & (1UL<<StandardPosMarker)) != 0;
        }

        public override string ToString()
        {
            if (IsStandard)
                return (SectorName!=null ? (SectorName + " "):"") + (char)(L1+'A'-1) + (char)(L2+'A'-1) + "-" + (char)(L3+'A'-1) + " " + (char)(MassCode+'a') + (NValue>0xffff?((NValue/0x10000).ToStringInvariant()+"-"):"") + (NValue&0xffff).ToStringInvariant();
            else
                return (SectorName != null && SectorName != NonStandard ? (SectorName + " ") : "") + StarName;
        }

        public EliteNameClassifier()
        {
        }

        public EliteNameClassifier(string n)
        {
            Classify(n);
        }

        public EliteNameClassifier(ulong id)
        {
            Classify(id);
        }

        public void Classify( ulong id)
        {
            if (IsIDStandard(id))
            {
                NValue = (uint)(id >> NMarker) & 0xffffff;
                MassCode = (char)(((id >> MassMarker) & 7));
                L3 = (char)(((id >> L3Marker) & 31));
                L2 = (char)(((id >> L2Marker) & 31));
                L1 = (char)(((id >> L1Marker) & 31));
                EntryType = NameType.NValue;
                System.Diagnostics.Debug.Assert(L1 < 31 && L2 < 32 && L3 < 32 && NValue < 0xffffff && MassCode < 8);
            }
            else
            {
                NameId = (long)(id & 0xffffff);
                EntryType = NameType.NonStandard;
            }

            SectorName = StarName = null;
        }

        public void Classify(string starname)
        {
            EntryType = NameType.NonStandard;

            string[] nameparts = starname.Split(' ');

            L1 = L2 = L3 = MassCode = NValue = 0;      // unused parts are zero

            for (int i = 0; i < nameparts.Length; i++)
            {
                if (i > 0 && nameparts[i].Length == 4 && nameparts[i][2] == '-' && char.IsLetter(nameparts[i][0]) && char.IsLetter(nameparts[i][1]) && char.IsLetter(nameparts[i][3]))
                {
                    L1 = (uint)(char.ToUpper(nameparts[i][0]) - 'A' + 1);
                    L2 = (uint)(char.ToUpper(nameparts[i][1]) - 'A' + 1);
                    L3 = (uint)(char.ToUpper(nameparts[i][3]) - 'A' + 1);

                    EntryType = NameType.Identifier;

                    if (nameparts.Length > i + 1)
                    {
                        string p = nameparts[i + 1];

                        if (p.Length > 0)
                        {
                            char mc = char.ToLower(p[0]);
                            if (mc >= 'a' && mc <= 'h')
                            {
                                MassCode = (uint)(mc - 'a');
                                EntryType = NameType.Masscode;

                                int slash = p.IndexOf("-");

                                int first = (slash >= 0 ? p.Substring(1, slash - 1) : p.Substring(1)).InvariantParseInt(-1);

                                if (first >= 0)
                                {
                                    if (slash >= 0)
                                    {
                                        System.Diagnostics.Debug.Assert(first < 256);
                                        int second = p.Substring(slash + 1).InvariantParseInt(-1);
                                        System.Diagnostics.Debug.Assert(second < 65536);
                                        if (second >= 0)
                                        {
                                            NValue = (uint)first * 0x10000 + (uint)second;
                                            EntryType = NameType.NValue;
                                        }
                                        else
                                        {               // thats d29-
                                            NValue = (uint)first * 0x10000;
                                            EntryType = NameType.N1ValueOnly;
                                        }
                                    }
                                    else
                                    {       // got to presume its the whole monty, d23
                                        System.Diagnostics.Debug.Assert(first < 65536);
                                        NValue = (uint)first;
                                        EntryType = NameType.NValue;
                                    }
                                }
                            }
                        }
                    }

                    SectorName = nameparts[0];
                    for (int j = 1; j < i; j++)
                        SectorName = SectorName + " " + nameparts[j];

                    break;
                }
            }

            if (EntryType == 0)
            {
                string[] surveys = new string[] { "HIP", "2MASS", "HD", "LTT", "TYC", "NGC", "HR", "LFT", "LHS", "LP", "Wolf" };

                if (surveys.Contains(nameparts[0]))
                {
                    SectorName = nameparts[0];
                    StarName = starname.Mid(nameparts[0].Length + 1);
                }
                else
                {
                    SectorName = NonStandard;
                    StarName = starname;
                }
            }
        }
    }
}