
using SQLLiteExtensions;

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
