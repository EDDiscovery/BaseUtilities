
using SQLLiteExtensions;

namespace TestSQL2
{
    public class SQLiteConnectionSystem : SQLExtConnectionRegister
    {
        public SQLiteConnectionSystem(string db, bool utctimeindicator, AccessMode mode = AccessMode.ReaderWriter) : base(db, utctimeindicator, mode)
        {
        }

    }
}
