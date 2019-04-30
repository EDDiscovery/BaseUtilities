using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousCore
{
    class EliteConfigInstance
    {
        static public EliteConfigInstance InstanceOptions
        { get
            {
                if (instance == null)
                    instance = new EliteConfigInstance();

                return instance;
            }
        }

        static EliteConfigInstance instance;

        public string SystemDatabasePath { get { return @"c:\code\edsm\edsm.sql"; } }

    }
}
