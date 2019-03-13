using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousCore
{
    public class EliteNameClassifier
    {
        public const string NonStandard = "NonStandard";

        public enum NameType { NonStandard, Identifier, Masscode, N1, N2 };
        public NameType EntryType = NameType.NonStandard;   // 0 non standard, 1 CQ-L, 2 mass code, 3 N1, 4 N2

        public string SectorName = null;    // for string inputs, set always, the sector name or "NonStandard".            For numbers, null

        public string StarName = null;      // for string inputs, set for surveys and non standard names                   For numbers, null
        public long NameId = 0;             // set for number inputs

        public uint L1, L2, L3, MassCode, N1, N2;

        // ID Standard
        //      ID Bit 42 = 1
        //      ID Bit 37 = L1 (1 = A, 26=Z)
        //      ID Bit 32 = L2 (1 = A, 26=Z)
        //      ID Bit 27 = L3 (1 = A, 26=Z)
        //      ID Bit 24 = mass code (0=A,7=H)
        //      ID Bit 16 = N1
        //      ID Bit 0 = N2
        // Non standard:
        //      ID Bit 42 = 0
        //      ID Bit 0..41 = ID from name table

        public const int StandardPosMarker = 42;

        public bool IsStandard { get { return EntryType>=NameType.N1; } }

        public ulong ID
        {
            get      // without the sector code
            {
                if (IsStandard)
                {
                    System.Diagnostics.Debug.Assert(L1 < 31 && L2 < 32 && L3 < 32 && N1 < 256 && N2 < 65536 && MassCode < 8);
                    return ((ulong)N2 << 0) | ((ulong)N1 << 16) | ((ulong)(MassCode) << 24) | ((ulong)(L3) << 27) | ((ulong)(L2) << 32) | ((ulong)(L1) << 37) | (1UL << StandardPosMarker);
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
                return (SectorName!=null ? (SectorName + " "):"") + (char)(L1+'A'-1) + (char)(L2+'A'-1) + "-" + (char)(L3+'A'-1) + " " + (char)(MassCode+'a') + (N1>0?(N1.ToStringInvariant()+"-"):"") + N2.ToStringInvariant();
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
                N2 = (uint)(id >> 0) & 0xffff;
                N1 = (uint)(id >> 16) & 0xff;
                MassCode = (char)(((id >> 24) & 7));
                L3 = (char)(((id >> 27) & 31));
                L2 = (char)(((id >> 32) & 31));
                L1 = (char)(((id >> 37) & 31));
                EntryType = NameType.N2;
                System.Diagnostics.Debug.Assert(L1 < 31 && L2 < 32 && L3 < 32 && N1 < 256 && N2 < 65536 && MassCode < 8);
            }
            else
            {
                NameId = (long)((id>>0) & 0xffffff);
                EntryType = NameType.NonStandard;
            }

            SectorName = StarName = null;
        }

        public void Classify(string starname)
        {
            EntryType = NameType.NonStandard;

            string[] nameparts = starname.Split(' ');

            for (int i = 0; i < nameparts.Length - 1; i++)
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
                                            N1 = (uint)first;
                                            N2 = (uint)second;
                                            EntryType = NameType.N2;
                                        }
                                        else
                                        {               // thats d29-
                                            N1 = (uint)first;
                                            N2 = 0;
                                            EntryType = NameType.N1;
                                        }
                                    }
                                    else
                                    {       // got to presume its the whole monty, d23
                                        System.Diagnostics.Debug.Assert(first < 65536);
                                        N1 = 0;
                                        N2 = (uint)first;
                                        EntryType = NameType.N2;
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
                    StarName = starname.Substring(nameparts[0].Length + 1);
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
