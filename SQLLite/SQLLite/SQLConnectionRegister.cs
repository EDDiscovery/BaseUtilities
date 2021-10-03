/*
 * Copyright © 2019-2021 EDDiscovery development team
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

using System;
using System.Data;
using System.Data.Common;

namespace SQLLiteExtensions
{
    // Connection with a register

    public class SQLExtConnectionRegister<TConn> : SQLExtConnection where TConn : SQLExtConnection, new()
    {
        public SQLExtRegister RegisterClass;

        public SQLExtConnectionRegister(string dbfile, bool utctimeindicator, AccessMode mode = AccessMode.ReaderWriter) : base(dbfile,utctimeindicator, mode)
        {
            RegisterClass = new SQLExtRegister(this);
        }
    }
}
