/*
 * Copyright © 2021-2023 EDDiscovery development team
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
 */

using System;
using System.Data.Common;
using System.Data.SQLite;

namespace SQLLiteExtensions
{
    public static class SQLDbProvider
    {
        public static DbProviderFactory DbProvider()      // get the provider, and cache it
        {
            lock (Locker)
            {
                if (dbprovider == null)
                {
                    dbprovider = GetSqliteProvider();
                }
                return dbprovider;
            }
        }

        private static Object Locker = new object();
        private static DbProviderFactory dbprovider;

        private static DbProviderFactory GetSqliteProvider()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                try
                {
                    string sqliteversion = SQLiteConnection.SQLiteVersion;

                    if (!String.IsNullOrEmpty(sqliteversion))
                    {
                        System.Diagnostics.Trace.WriteLine($"SQLite Version {sqliteversion}");
                        return new System.Data.SQLite.SQLiteFactory();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"No windows DB provider -check interop DLL etc {ex}");
                }
            }
            else
            {
                try
                {
                    // Disable CS0618 warning for LoadWithPartialName
#pragma warning disable 618
                    var asm = System.Reflection.Assembly.LoadWithPartialName("Mono.Data.Sqlite");
#pragma warning restore 618
                    var factorytype = asm.GetType("Mono.Data.Sqlite.SqliteFactory");
                    var factory = (DbProviderFactory)factorytype.GetConstructor(new Type[0]).Invoke(new object[0]);

                    using (var conn = factory.CreateConnection())
                    {
                        conn.ConnectionString = "Data Source=:memory:;Pooling=true;";
                        conn.Open();
                        return factory;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"No MONO DB provider -check interop DLL etc {ex}");
                }
            }

            return null;
        }
    }
}
