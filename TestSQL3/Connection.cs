
using SQLLiteExtensions;
using System;

namespace TestSQL2
{
    public class SQLiteConnectionSystem : SQLExtConnectionRegister
    {
        public SQLiteConnectionSystem(string db, bool utctimeindicator, AccessMode mode = AccessMode.ReaderWriter, JournalModes journalmode = JournalModes.WAL ) : 
                    base(db, utctimeindicator, mode, journalmode)
        {
        }

    }

    //public class SQLiteDBWALProcessor : SQLWALProcessor<SQLiteConnectionSystem>, IDisposable
    //{
    //    public string DBFile;
    //    public SQLiteDBWALProcessor(string file)
    //    {
    //        DBFile = file;
    //    }

    //    // singleton
    //    static private string locker = "k";
    //    static private SQLiteConnectionSystem csys;
    //    protected override SQLiteConnectionSystem GetConnection()
    //    {
    //        lock (locker)
    //        {
    //            if (csys == null)
    //                csys = new SQLiteConnectionSystem(DBFile, false, journalmode: SQLExtConnection.JournalModes.WAL);

    //            return csys;
    //        }
    //    }


    //    public void Dispose()
    //    {
    //        if (csys != null)
    //        {
    //            csys.Dispose();
    //            csys = null;
    //        }
    //    }
    //}
    public class SQLiteDBAPT : SQLAdvProcessingThread<SQLiteConnectionSystem>, IDisposable
    {
        public string DBFile;
        public SQLiteDBAPT(string file)
        {
            DBFile = file;
        }

        public void Dispose()
        {
            Stop();
        }
        protected override SQLiteConnectionSystem CreateConnection()
        {
            return new SQLiteConnectionSystem(DBFile, false, journalmode:SQLExtConnection.JournalModes.WAL);
        }
    }

}
