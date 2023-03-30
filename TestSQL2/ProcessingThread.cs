
using SQLLiteExtensions;

namespace TestSQL2
{
    public class SQLiteThread : SQLAdvProcessingThread<SQLiteConnectionSystem>
    {
        public string DBFile;

        public SQLiteThread(string file)
        {
            DBFile = file;
        }

        protected override SQLiteConnectionSystem CreateConnection()
        {
            return new SQLiteConnectionSystem(DBFile, false);
        }
    }
}
