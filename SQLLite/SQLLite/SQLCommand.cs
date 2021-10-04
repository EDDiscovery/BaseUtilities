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
    // This class wraps a DbCommand 

    public class SQLExtCommand<TConn> : DbCommand where TConn : SQLExtConnection
    {
        // Overridden methods and properties passed to inner command

        public override string CommandText { get { return InnerCommand.CommandText; } set { InnerCommand.CommandText = value; } }
        public override int CommandTimeout { get { return InnerCommand.CommandTimeout; } set { InnerCommand.CommandTimeout = value; } }
        public override CommandType CommandType { get { return InnerCommand.CommandType; } set { InnerCommand.CommandType = value; } }
        protected override DbConnection DbConnection { get { return InnerCommand.Connection; } set { throw new InvalidOperationException("Cannot change connection of command"); } }
        protected override DbParameterCollection DbParameterCollection { get { return InnerCommand.Parameters; } }
        public override bool DesignTimeVisible { get { return InnerCommand.DesignTimeVisible; } set { InnerCommand.DesignTimeVisible = value; } }
        public override UpdateRowSource UpdatedRowSource { get { return InnerCommand.UpdatedRowSource; } set { InnerCommand.UpdatedRowSource = value; } }
        protected override DbParameter CreateDbParameter() { return InnerCommand.CreateParameter(); }
        public override void Cancel() { InnerCommand.Cancel(); }
        public override void Prepare() { InnerCommand.Prepare(); }

        protected override DbTransaction DbTransaction { get { return InnerCommand.Transaction; } set { InnerCommand.Transaction = value; } }

        // interface

        public SQLExtCommand(DbCommand cmd, SQLExtConnection conn, DbTransaction txn = null)
        {
            InnerCommand = cmd;
            if (txn != null)
            {
                InnerCommand.Transaction = txn;
            }
        }

        // implementation

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return new SQLExtDataReader(this.InnerCommand, behavior);
        }

        public override object ExecuteScalar()
        {
            return InnerCommand.ExecuteScalar();
        }

        public override int ExecuteNonQuery()
        {
            return InnerCommand.ExecuteNonQuery();
        }

        // disposing: true if Dispose() was called, false
        // if being finalized by the garbage collector
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (InnerCommand != null)
                {
                    InnerCommand.Dispose();
                    InnerCommand = null;
                }

#if DEBUG
                hasbeendisposed = true;
#endif
            }

            base.Dispose(disposing);
        }

#if DEBUG
        private bool hasbeendisposed;
        //since finalisers impose a penalty, we shall check only in debug mode
        ~SQLExtCommand()
        {
            if (!hasbeendisposed)       // finalisation may come very late.. not immediately as its done on garbage collection.  Warn by message and assert.
            {
                System.Windows.Forms.MessageBox.Show("Missing dispose for command " + this.CommandText);
                System.Diagnostics.Debug.Assert(hasbeendisposed, "Missing dispose for command " + this.CommandText);       // must have been disposed
            }
        }
#endif

        public DbCommand InnerCommand { get; set; }
    }
}

