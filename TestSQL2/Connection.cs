
using SQLLiteExtensions;

namespace TestSQL2
{
    public class SQLiteConnectionSystem : SQLExtConnectionRegister<SQLiteConnectionSystem>
    {
        public SQLiteConnectionSystem() : this("", false)
        {
        }

        public SQLiteConnectionSystem(string db, bool ro) : base(db, utctimeindicator: true, mode: ro ? AccessMode.Reader : AccessMode.ReaderWriter)
        {
        }

    }
}
