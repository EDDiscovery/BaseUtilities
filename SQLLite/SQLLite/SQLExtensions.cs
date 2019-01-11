using SQLLiteExtensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class SQLiteCommandExtensions
{
    public static void AddParameterWithValue(this DbCommand cmd, string name, object val)
    {
        var par = cmd.CreateParameter();
        par.ParameterName = name;
        par.Value = val;
        cmd.Parameters.Add(par);
    }

    public static void AddParameter(this DbCommand cmd, string name, DbType type)
    {
        var par = cmd.CreateParameter();
        par.ParameterName = name;
        par.DbType = type;
        cmd.Parameters.Add(par);
    }

    public static void SetParameterValue(this DbCommand cmd, string name, object val)
    {
        cmd.Parameters[name].Value = val;
    }

    public static DataSet SQLQueryText(this SQLExtConnection cn, DbCommand cmd)
    {
        try
        {
            DataSet ds = new DataSet();
            DbDataAdapter da = cn.CreateDataAdapter(cmd);
            da.Fill(ds);
            return ds;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("SqlQuery Exception: " + ex.Message);
            throw;
        }
    }

    static public int SQLNonQueryText(this SQLExtConnection cn, DbCommand cmd)
    {
        int rows = 0;

        try
        {
            rows = cmd.ExecuteNonQuery();
            return rows;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("SqlNonQueryText Exception: " + ex.Message);
            throw;
        }
    }

    static public object SQLScalar(this SQLExtConnection cn, DbCommand cmd)
    {
        object ret = null;

        try
        {
            ret = cmd.ExecuteScalar();
            return ret;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("SqlNonQuery Exception: " + ex.Message);
            throw;
        }
    }
}
