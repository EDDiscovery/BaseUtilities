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

        public string SectorName = null;    // for string inputs, set always, the sector name or "NonStandard".            For numbers, null
        public string StarName = null;      // for string inputs, set for surveys and non standard names                   For numbers, null

        public long SectorId = 0;       // set for number inputs
        public long NameId = 0;         // set for number inputs
        public uint L1, L2, L3, MassCode, N1, N2;

        // ID Standard
        //      ID bit 44 = sector ID from db. >=1
        //      ID Bit 42+43 = 0
        //      ID Bit 37 = L1 (1 = A, 26=Z)
        //      ID Bit 32 = L2 (1 = A, 26=Z)
        //      ID Bit 27 = L3 (1 = A, 26=Z)
        //      ID Bit 24 = mass code (0=A,7=H)
        //      ID Bit 16 = N1
        //      ID Bit 0 = N2
        // Non standard:
        //      ID Bit 44..63 = sector ID
        //      Bits 24-43 = zero
        //      Bits 0-23 = ID from name table

        public const int SectorPos = 44;

        public bool IsValidID { get { return SectorId > 0; } }
        public void SetInvalid() { SectorId = 0; }

        public bool IsStandard { get { return L1 != 0; } }

        public ulong ID
        {
            get      // without the sector code
            {
                if (IsStandard)
                    return (ulong)N2 | ((ulong)N1 << 16) | ((ulong)(MassCode) << 24) | ((ulong)(L3) << 27) | ((ulong)(L2) << 32) | ((ulong)(L1) << 37) | ((ulong)(SectorId) << SectorPos);
                else
                    return (ulong)NameId | ((ulong)(SectorId) << SectorPos);
            }
        }

        public static bool IsIDStandard( ulong id)
        {                   //27
            return (id & 0xfff8000000UL) != 0;
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
            if (IsIDStandard(id))
            {
                N1 = (uint)(id >> 16) & 0xff;
                N2 = (uint)id & 0xffff;
                MassCode = (char)(((id >> 24) & 7));
                L3 = (char)(((id >> 27) & 31));
                L2 = (char)(((id >> 32) & 31));
                L1 = (char)(((id >> 37) & 31));
            }
            else
                NameId = (long)(id & 0xffffff);

            SectorName = StarName = null;
            SectorId = (long)(id >> SectorPos);
        }

        public void Classify(string starname)
        {
            L1 = 0;

            string[] nameparts = starname.Split(' ');

            for (int i = 0; i < nameparts.Length - 1; i++)
            {
                if (i > 0 && nameparts[i].Length == 4 && nameparts[i][2] == '-' && char.IsLetter(nameparts[i][0]) && char.IsLetter(nameparts[i][1]) && char.IsLetter(nameparts[i][3]))
                {
                    string p = nameparts[i + 1];
                    int slash = nameparts[i + 1].IndexOf("-");

                    int n1,n2;

                    if (slash >= 0)
                    {
                        n1 = p.Substring(1, slash - 1).InvariantParseInt(-1);
                        n2 = p.Substring(slash + 1).InvariantParseInt(-1);
                    }
                    else
                    {
                        n1 = 0;
                        n2 = p.Substring(1).InvariantParseInt(-1);
                    }

                    if (N1 >= 0 && N2 >= 0)     // accept
                    {
                        MassCode = (uint)(char.ToLower(p[0])-'a');
                        L1 = (uint)(char.ToUpper(nameparts[i][0]) - 'A' + 1);
                        L2 = (uint)(char.ToUpper(nameparts[i][1]) - 'A' + 1);
                        L3 = (uint)(char.ToUpper(nameparts[i][3]) - 'A' + 1);
                        N1 = (uint)n1;
                        N2 = (uint)n2;
                        SectorName = nameparts[0];
                        for (int j = 1; j < i; j++)
                            SectorName = SectorName + " " + nameparts[j];
                    }

                    break;
                }
            }

            if (L1 == 0)
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
