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

    public static void AddParamsWithValue(this DbCommand cmd, string[] paras, Object[] values)
    {
        for (int i = 0; i < paras.Length; i++)
            cmd.AddParameterWithValue("@" + paras[i], values[i]);
    }

    public static void SetParameterValue(this DbCommand cmd, string name, object val)
    {
        cmd.Parameters[name].Value = val;
    }

    // Either a list of names/types, or types = null, in which case paras contains name:dbtype (case insensitive)

    public static void CreateParams(this DbCommand cmd, string[] paras, DbType[] types)
    {
        if (paras != null)
        {
            System.Diagnostics.Debug.Assert(types == null || paras.Length == types.Length);
            for (int i = 0; i < paras.Length; i++)
            {
                if (types == null)
                {
                    string[] parts = paras[i].Split(':');
                    System.Diagnostics.Debug.Assert(parts.Length == 2);

                    DbType dbt = (DbType)Enum.Parse(typeof(DbType), parts[1], true);
                    cmd.AddParameter("@" + parts[0], dbt);
                }
                else
                    cmd.AddParameter("@" + paras[i], types[i]);
            }
        }
    }

    static public T ExecuteScalar<T>(this DbCommand cmd, T def)
    {
        Object res = cmd.ExecuteScalar();
        return res == null ? def : (T)res;
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
        return "SQL Query:" + Environment.NewLine + cmdexplain.CommandText + Environment.NewLine + "Plan:" + Environment.NewLine + string.Join(Environment.NewLine, ret);
    }

    public static long MaxIdOf(this SQLExtConnection r, string table, string idfield)
    {
        using (DbCommand queryNameCmd = r.CreateCommand("SELECT Max(" + idfield + ") as " + idfield + " FROM " + table))
            return (long)queryNameCmd.ExecuteScalar();
    }

    public static long CountOf(this SQLExtConnection r, string table, string idfield, string where = null)
    {
        using (DbCommand queryNameCmd = r.CreateCommand("SELECT Count(" + idfield + ") as " + idfield + " FROM " + table + (where != null ?  (" WHERE " + where) : "")))
            return (long)queryNameCmd.ExecuteScalar();
    }

    // for these, types are optional if paras given as name:type strings

    public static DbCommand CreateInsert(this SQLExtConnection r, string table, string[] paras, DbType[] types = null, DbTransaction tx = null, bool insertorreplace = false, bool insertorignore = false)
    {
        string plist = "";
        string atlist = "";
        foreach (string s in paras)
        {
            string n = s.Split(':')[0];
            plist = plist.AppendPrePad(n, ",");
            atlist = atlist.AppendPrePad("@" + n, ",");
        }

        string cmdtext = "INSERT " + (insertorreplace ? "OR REPLACE " : "") + (insertorignore ? "OR IGNORE " : "") + "INTO " + table + " (" + plist + ") VALUES (" + atlist + ")";

        DbCommand cmd = r.CreateCommand(cmdtext, tx);
        cmd.CreateParams(paras, types);
        return cmd;
    }

    public static DbCommand CreateReplace(this SQLExtConnection r, string table, string[] paras, DbType[] types = null, DbTransaction tx = null)
    {
        return CreateInsert(r, table, paras, types, tx, insertorreplace: true);
    }

    public static DbCommand CreateInsertOrIgnore(this SQLExtConnection r, string table, string[] paras, DbType[] types = null, DbTransaction tx = null)
    {
        return CreateInsert(r, table, paras, types, tx, insertorignore: true);
    }

    public static DbCommand CreateUpdate(this SQLExtConnection r, string table, string where, string[] paras, DbType[] types,  DbTransaction tx = null)
    {
        string plist = "";
        foreach (string s in paras)
            plist = plist.AppendPrePad(s.Split(':')[0], ",");

        string cmdtext = "UPDATE " + table + " SET " + plist + " " + where;

        DbCommand cmd = r.CreateCommand(cmdtext, tx);
        cmd.CreateParams(paras, types);
        return cmd;
    }

    // no paras, or delayed paras

    public static DbCommand CreateSelect(this SQLExtConnection r, string table, string outparas, string where = null, string orderby = "",
                                            string[] inparas = null, DbType[] intypes = null,
                                            string[] joinlist = null, object limit = null, DbTransaction tx = null)
    {
        string lmt = "";
        if (limit != null)
            lmt = " LIMIT " + (limit is string ? (string)limit : ((int)limit).ToStringInvariant());

        string cmdtext = "SELECT " + outparas + " FROM " + table + " " + (joinlist != null ? string.Join(" ", joinlist) : "") +
                                        (where.HasChars() ? (" WHERE " + where) : "") + (orderby.HasChars() ? (" ORDER BY " + orderby) : "") + lmt;
        DbCommand cmd = r.CreateCommand(cmdtext, tx);
        cmd.CreateParams(inparas, intypes);
        return cmd;
    }

    // immediate paras, called p1,p2,p3 etc. 

    public static DbCommand CreateSelect(this SQLExtConnection r, string table, string outparas, string where, Object[] paras , 
                                            string orderby = "", string[] joinlist = null, object limit = null, DbTransaction tx = null)
    {
        string lmt = "";
        if (limit != null)
            lmt = " LIMIT " + (limit is string ? (string)limit : ((int)limit).ToStringInvariant());

        string cmdtext = "SELECT " + outparas + " FROM " + table + " " + (joinlist != null ? string.Join(" ", joinlist) : "") +
                                        (where.HasChars() ? (" WHERE " + where) : "") + (orderby.HasChars() ? (" ORDER BY " + orderby) : "") + lmt;
                                        
        DbCommand cmd = r.CreateCommand(cmdtext, tx);
        int pname = 1;
        foreach( var o in paras)
        {
            var par = cmd.CreateParameter();
            par.ParameterName = "@p" + (pname++).ToStringInvariant();
            par.Value = o;
            cmd.Parameters.Add(par);
        }
        return cmd;
    }

    public static DbCommand CreateDelete(this SQLExtConnection r, string table, string where , string[] paras = null, DbType[] types = null, DbTransaction tx = null)
    {
        string cmdtext = "DELETE FROM " + table + " WHERE " + where;
        DbCommand cmd = r.CreateCommand(cmdtext, tx);
        cmd.CreateParams(paras, types);
        return cmd;
    }





    // Dodgy old ones - check this on re-integration

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
