﻿/*
 * Copyright © 2016 - 2021 EDDiscovery development team
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
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace BaseUtils
{
    public class TraceLog           // intercepts trace/debug and sends it to a file
    {
        public static event Action<Exception> LogFileWriterException;
        public static string LogFileName { get; private set; }
        public static string LogFileBaseName { get; private set; }
        public static Thread LogFileWriterThread;
        public static bool DisableLogDeduplication { get; set; }
        private static BlockingCollection<string> LogLineQueue = new BlockingCollection<string>();
        private static AutoResetEvent LogLineQueueEvent = new AutoResetEvent(false);

        private class TraceLogWriter : TextWriter
        {
            private ThreadLocal<StringBuilder> logline = new ThreadLocal<StringBuilder>(() => new StringBuilder());

            public override Encoding Encoding { get { return Encoding.UTF8; } }
            public override IFormatProvider FormatProvider { get { return CultureInfo.InvariantCulture; } }

            public override void Write(string value)
            {
                if (value != null)
                {
                    logline.Value.Append(value);
                    string logval = logline.ToString();

                    if (logval.Contains("\n"))
                    {
                        logline.Value.Clear();
                        var lastnewline = logval.LastIndexOf('\n');
                        TraceLog.WriteLine(logval.Substring(0, lastnewline));
                        if (lastnewline < logval.Length - 1)
                        {
                            logval = logval.Substring(lastnewline + 1);
                            logline.Value.Append(logval);
                        }
                    }
                }
            }

            public override void Write(char value) { Write(new string(new[] { value })); }
            public override void WriteLine(string value) { Write((value ?? "") + "\n"); }
            public override void WriteLine() { Write("\n"); }
        }

        public static void RedirectTrace(string logroot, bool debugtoo, string filename = null)
        {
            if (Directory.Exists(logroot))
            {
                string logname = Path.Combine(logroot, filename ?? $"Trace_{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}");
                LogFileBaseName = logname;
                LogFileWriterThread = new Thread(LogWriterThreadProc);
                LogFileWriterThread.IsBackground = true;
                LogFileWriterThread.Name = "Log Writer";
                LogFileWriterThread.Start();
                System.Diagnostics.Trace.AutoFlush = true;
                // Log trace events to the above file
                var tlw = new TraceLogWriter();
                System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(tlw));
            }
        }

        public static void WriteLine(string msg)
        {
            LogLineQueue.Add(msg);
        }

        public static void WaitForOutput(int ms = 100)
        {
            LogLineQueueEvent.WaitOne(ms);
        }

        private static void LogWriterThreadProc()
        {
            int partnum = 0;
            Dictionary<string, int> msgrepeats = new Dictionary<string, int>();
            while (true)
            {
                try
                {
                    LogFileName = $"{LogFileBaseName}.{partnum}.log";
                    using (TextWriter writer = new StreamWriter(LogFileName))
                    {
                        int linenum = 0;
                        while (true)
                        {
                            string msg = null;
                            if (msgrepeats.Count < 100 && !msgrepeats.Any(m => m.Value >= 10000) && LogLineQueue.TryTake(out msg, msgrepeats.Count > 1 ? 1000 : Timeout.Infinite))
                            {
                                if (msg == null)
                                {
                                    LogLineQueueEvent.Set();
                                    return;
                                }
                                else if (!DisableLogDeduplication && msgrepeats.ContainsKey(msg))
                                {
                                    msgrepeats[msg]++;
                                }
                                else
                                {
                                    var lines = msg.Split('\n');
                                    var timestamp = DateTime.UtcNow.ToStringZulu();

                                    foreach (var line in lines)
                                    {
                                        writer.WriteLine($"[{timestamp}] {line}");
                                        linenum++;
                                    }

                                    writer.Flush();
                                    msgrepeats[msg] = 0;
                                    if (linenum >= 100000)
                                    {
                                        partnum++;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                foreach (KeyValuePair<string, int> rptkvp in msgrepeats)
                                {
                                    if (rptkvp.Value >= 1)
                                    {
                                        writer.WriteLine($"[{DateTime.UtcNow.ToStringZulu()}] {rptkvp.Key}");
                                        if (rptkvp.Value > 1)
                                        {
                                            writer.WriteLine($"[{DateTime.UtcNow.ToStringZulu()}] Last message repeated {(rptkvp.Value)} times");
                                        }
                                    }
                                }

                                msgrepeats = new Dictionary<string, int>();
                            }
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception ex)
                {
                    LogFileWriterException?.Invoke(ex);
                    Thread.Sleep(30000);
                }
            }
        }
    }

}
