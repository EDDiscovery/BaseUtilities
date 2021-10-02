using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLLiteExtensions;
using System;

namespace TestSQL2
{
    public class SQLiteThread : SQLAdvProcessingThread<SQLiteConnectionSystem>
    {
        public SQLiteThread()
        {
        }

        protected override SQLiteConnectionSystem CreateConnection()
        {
            return new SQLiteConnectionSystem(@"c:\code\edsm\edsm.sql", false);
        }
    }
}
