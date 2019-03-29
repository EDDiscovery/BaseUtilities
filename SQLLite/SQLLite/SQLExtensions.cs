/*
 * Copyright © 2019 EDDiscovery development team
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

using SQLLiteExtensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

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

    static public T ConvertTo<T>(this DbDataReader r, int index)
    {
        Object v = r[index];
        if (v is System.DBNull)
            return default(T);
        else
            return (T)v;
    }

    static public List<string> Tables(this SQLExtConnection r)
    {
        List<string> tables = new List<string>();

        using (DbCommand cmd = r.CreateCommand("select name From sqlite_master Where type='table'"))
        {
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    tables.Add((string)reader[0]);
            }
        }

        return tables;
    }

    public static void Vacuum(this SQLExtConnection r)
    {
        using (DbCommand cmd = r.CreateCommand("VACUUM"))
        {
            cmd.ExecuteNonQuery();
        }
    }

    // either give a fully formed cmd to it, or give cmdexplain=null and it will create one for you using cmdtextoptional (but with no variable variables allowed)
    public static List<string> ExplainQueryPlan(this SQLExtConnection r, DbCommand cmdexplain, string cmdtextoptional = null )
    {
        if (cmdexplain == null)
        {
            System.Diagnostics.Debug.Assert(cmdtextoptional != null);
            cmdexplain = r.CreateCommand(cmdtextoptional);
        }

        List<string> ret = new List<string>();

        using (DbCommand cmd = r.CreateCommand("Explain Query Plan " + cmdexplain.CommandText))
        {
            foreach( var p in cmdexplain.Parameters )
                cmd.Parameters.Add(p);  

            using (DbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string detail = (string)reader[3];
                    int order = (int)(long)reader[1];
                    int from = (int)(long)reader[2];
                    int selectid = (int)(long)reader[0];
                    ret.Add("Select ID " + selectid + " Order " + order + " From " + from + ": " + detail);
                }
            }
        }

        return ret;
    }

    public static string ExplainQueryPlanString(this SQLExtConnection r, DbCommand cmdexplain)
    {
        var ret = ExplainQueryPlan(r, cmdexplain);
        return "SQL Query:" + Environment.NewLine + ">" + cmdexplain.CommandText + Environment.NewLine + "Plan:" + Environment.NewLine + string.Join(Environment.NewLine, ret);
    }

}
