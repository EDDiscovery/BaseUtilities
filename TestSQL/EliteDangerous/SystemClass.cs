﻿/*
 * Copyright © 2015 - 2019 EDDiscovery development team
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
using System.Diagnostics;

namespace EliteDangerousCore
{
    // For when you need a minimal version and don't want to mess up the database. 
    // Useful for creation of test doubles
    public class SystemClassBase : ISystemBase
    {
        public const float XYZScalar = 128.0F;     // scaling between DB stored values and floats

        static public float IntToFloat(int pos) { return (float)pos / XYZScalar; }
        static public double IntToDouble(int pos) { return (double)pos / XYZScalar; }
        static public int DoubleToInt(double pos) { return (int)(pos * XYZScalar); }
        static public double IntToDoubleSq(int pos) { double p = (float)pos / XYZScalar; return p * p; }

        public string Name { get; set; }

        public int Xi { get; set; }
        public int Yi { get; set; }
        public int Zi { get; set; }

        public double X { get { return Xi >= int.MinValue ? (double)Xi / XYZScalar : double.NaN; } set { Xi = double.IsNaN(value) ? int.MinValue : (int)(value * XYZScalar); } }
        public double Y { get { return Xi >= int.MinValue ? (double)Yi / XYZScalar : double.NaN; } set { Yi = (int)(value * XYZScalar); } }
        public double Z { get { return Xi >= int.MinValue ? (double)Zi / XYZScalar : double.NaN; } set { Zi = (int)(value * XYZScalar); } }

        public bool HasCoordinate { get { return Xi != int.MinValue; } }

        public int GridID { get; set; }
        public long? SystemAddress { get; set; }

        public Tuple<string, long?> NameSystemAddress { get { return new Tuple<string, long?>(Name, SystemAddress); } }

        public SystemClassBase()
        {
            Xi = int.MinValue;
        }

        public SystemClassBase(string name, double vx, double vy, double vz)
        {
            Name = name;
            X = vx; Y = vy; Z = vz;
            GridID = EliteDangerousCore.DB.GridId.Id128(Xi, Zi);
        }

        public SystemClassBase(string name, int xi, int yi, int zi, int gridid = -1)
        {
            Name = name;
            Xi = xi; Yi = yi; Zi = zi;
            GridID = gridid == -1 ? EliteDangerousCore.DB.GridId.Id128(Xi,Zi) : gridid;
        }

        public SystemClassBase(ISystemBase sys)
        {
            this.Name = sys.Name;
            this.Xi = sys.Xi;
            this.Yi = sys.Yi;
            this.Zi = sys.Zi;
            this.GridID = sys.GridID;
            this.SystemAddress = sys.SystemAddress;
        }

        public bool Equals(ISystemBase other)
        {
            return other != null &&
                   other.Name.Equals(this.Name, StringComparison.InvariantCultureIgnoreCase) &&
                   (!this.HasCoordinate || !other.HasCoordinate ||
                    (Math.Abs(this.X - other.X) < 0.125 &&
                     Math.Abs(this.Y - other.Y) < 0.125 &&
                     Math.Abs(this.Z - other.Z) < 0.125));
        }

        public double Distance(ISystemBase s2)
        {
            if (s2 != null && HasCoordinate && s2.HasCoordinate)
                return Math.Sqrt((X - s2.X) * (X - s2.X) + (Y - s2.Y) * (Y - s2.Y) + (Z - s2.Z) * (Z - s2.Z));
            else
                return -1;
        }

        public bool Distance(ISystemBase s2, double min, double max)
        {
            if (s2 != null && HasCoordinate && s2.HasCoordinate)
            {
                double distsq = (X - s2.X) * (X - s2.X) + (Y - s2.Y) * (Y - s2.Y) + (Z - s2.Z) * (Z - s2.Z);
                return distsq >= min * min && distsq <= max * max;
            }
            else
                return false;
        }

        public double Distance(double ox, double oy, double oz)
        {
            if (HasCoordinate)
                return Math.Sqrt((X - ox) * (X - ox) + (Y - oy) * (Y - oy) + (Z - oz) * (Z - oz));
            else
                return -1;
        }

        public double DistanceSq(double x, double y, double z)
        {
            if (HasCoordinate)
                return (X - x) * (X - x) + (Y - y) * (Y - y) + (Z - z) * (Z - z);
            else
                return -1;
        }

        public bool Cuboid(ISystemBase s2, double min, double max)
        {
            if (s2 != null && HasCoordinate && s2.HasCoordinate)
            {
                double xd = Math.Abs(X - s2.X);
                double yd = Math.Abs(Y - s2.Y);
                double zd = Math.Abs(Z - s2.Z);
                return xd >= min && xd <= max && yd >= min && yd <= max && zd >= min && zd <= max;
            }
            else
                return false;
        }
    }

    [DebuggerDisplay("System {Name} ({X,nq},{Y,nq},{Z,nq}) {Source}")]
    public class SystemClass : SystemClassBase, ISystem
    {
        public SystemClass() : base()
        {
        }

        public SystemClass(ISystem sys) : base(sys)
        {
            this.EDSMID = sys.EDSMID;
            this.Source = sys.Source;

            this.Population = sys.Population;
            this.Faction = sys.Faction;
            this.Government = sys.Government;
            this.Allegiance = sys.Allegiance;
            this.State = sys.State;
            this.Security = sys.Security;
            this.PrimaryEconomy = sys.PrimaryEconomy;
            this.Power = sys.Power;
            this.PowerState = sys.PowerState;
            this.NeedsPermit = sys.NeedsPermit;
        }

        public SystemClass(string name) : base()
        {
            Name = name;
            Source = SystemSource.Synthesised;
        }

        public SystemClass(long? sysaddr, string name) : base()
        {
            Name = name;
            SystemAddress = sysaddr;
            Source = SystemSource.Synthesised;
        }

        public SystemClass(string name, long edsmid)
        {
            Name = name;
            EDSMID = edsmid;
            Source = EDSMID <= 0 ? SystemSource.Synthesised : SystemSource.FromEDSM;
        }

        public SystemClass(string name, double vx, double vy, double vz, long edsmid) : base(name, vx, vy, vz)
        {
            EDSMID = edsmid;
            Source = SystemSource.FromEDSM;
        }

        public SystemClass(string name, double vx, double vy, double vz) : base(name, vx, vy, vz)
        {
            Source = SystemSource.Synthesised;
        }

        public SystemClass(string name, int xi, int yi, int zi, long edsmid, int gridid = -1) : base(name,xi,yi,zi,gridid)
        {
            EDSMID = edsmid;
            Source = EDSMID <= 0 ? SystemSource.Synthesised : SystemSource.FromEDSM;
        }

        public long EDSMID { get; set; }
        public SystemSource Source { get; set; }

        public long Population { get; set; }        // just because its 0 does not mean it may have other info
        public string Faction { get; set; }
        public EDGovernment Government { get; set; }
        public EDAllegiance Allegiance { get; set; }
        public EDState State { get; set; }
        public EDSecurity Security { get; set; }
        public EDEconomy PrimaryEconomy { get; set; }
        public string Power { get; set; }
        public string PowerState { get; set; }
        public int NeedsPermit { get; set; }

        public bool HasSystemStateInfo
        {
            get
            {
                return Government != EDGovernment.Unknown || NeedsPermit != 0 || Allegiance != EDAllegiance.Unknown ||
                       State != EDState.Unknown || Security != EDSecurity.Unknown || PrimaryEconomy != EDEconomy.Unknown || (Faction != null && Faction.Length > 0);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} @ {1:N1},{2:N1},{3:N1}", Name, X, Y, Z);
        }

        public string ToStringVerbose()
        {
            string x = string.Format("{0} @ {1:N1},{2:N1},{3:N1} EDSMID:{4}", Name, X, Y, Z, EDSMID);
            if (HasSystemStateInfo)
            {
                x += " " + Population + " " + Faction + " " + Government + " " + Allegiance + " " + State + " " + Security + " " + PrimaryEconomy
                                    + " " + Power + " " + PowerState + " " + NeedsPermit;
            }
            return x;
        }
    }
}
