/*
 * Copyright 2016-2024 EDDiscovery development team
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
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace BaseUtils
{
    public class TraceLog           // intercepts trace/debug and sends it to a file
    {
        public static event Action<Exception> LogFileWriterException;
        public static bool DisableLogDeduplication { get; set; }
        public static bool IsThreadOurs(Thread x) => x == logFileWriterThread;

        public static void RedirectTrace(string logroot, string filename = null)
        {
            if (Directory.Exists(logroot))
            {
                string logname = Path.Combine(logroot, filename ?? $"Trace_{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}");
                logFileBaseName = logname;
                logFileWriterThread = new Thread(LogWriterThreadProc);
                logFileWriterThread.IsBackground = true;
                logFileWriterThread.Name = "Log Writer";
                logFileWriterThread.Start();
                System.Diagnostics.Trace.AutoFlush = true;
                tracelistener = new System.Diagnostics.TextWriterTraceListener(new TraceLogWriter());
                System.Diagnostics.Trace.Listeners.Add(tracelistener);
            }
        }

        // submit a message. \rs are ignored. \ns show line boundaries
        public static void WriteLine(string msg)
        {
            LogLineQueue.Add(msg);
        }

        public static void TerminateLogger(int ms = 10000)
        {
            if (logFileWriterThread != null)
            {
                TraceLog.WriteLine(null);
                logFileWriterThread.Join();
            }
        }

        private static void LogWriterThreadProc()
        {
            int partnum = 0;
            string lastline = null;
            string nextoutput = null;
            int repeats = 0;

            while (true)
            {
                try
                {
                    string logfilename = $"{logFileBaseName}.{partnum}.log";
                    using (TextWriter writer = new StreamWriter(logfilename))
                    {
                        int msgnum = 0;
                        while (true)
                        {
                            bool gotone = LogLineQueue.TryTake(out string msg, 1000);       // messages closer than 1s can be merged

                            if (gotone)
                            {
                                if (msg == null)        // stop condition, terminate thread
                                {
                                    var timestamp = DateTime.UtcNow.ToStringZulu();
                                    writer.WriteLine($"{++msgnum,4}: {timestamp}: Closing log");
                                    writer.Flush();
                                    writer.Close();
                                    System.Diagnostics.Trace.Listeners.Remove(tracelistener);
                                    return;
                                }

                                msg = msg.Replace("\r", "");        // remove any /rs as they will double space the log output in editors

                                if (msg.StartsWith("\n"))       // remove any pre line feeds to clean up the output
                                    msg = msg.Substring(1);
                            }

                            if (gotone && msg == lastline && !DisableLogDeduplication) // merge with above
                            {
                                repeats++;
                            }
                            else
                            {
                                if (nextoutput != null)
                                {
                                    msgnum++;

                                    var lines = nextoutput.Split('\n');           // no empty starts (\n starting) or trailing \ns
                                    int colon = nextoutput.IndexOf(": ");       // find text after time
                                    for (int i = 0; i < lines.Length; i++)
                                    {
                                        var line = lines[i];
                                        if (i == 0 && repeats > 0)      // if first line, and repeats, introduce repeat
                                        {
                                            writer.WriteLine($"{msgnum,4}: {line.Substring(0, colon)}: Repeated {repeats+1}" + line.Substring(colon));
                                        }
                                        else
                                        {
                                            if (i > 0)       // if after first line, space out to same size as time
                                                writer.WriteLine(new string(' ', colon + 4 + 2) + "  " + line);
                                            else
                                                writer.WriteLine($"{msgnum,4}: " + line);
                                        }
                                    }

                                    writer.Flush();

                                    if (msgnum >= 100000)
                                    {
                                        partnum++;
                                        break;
                                    }
                                }

                                repeats = 0;

                                if (gotone)        // if we got one, set up nextoutput
                                {
                                    var timestamp = DateTime.UtcNow.ToStringZulu();
                                    nextoutput = $"{timestamp}: {msg}";
                                    lastline = msg;
                                }
                                else
                                {
                                    nextoutput = null;  // cancel nextoutput
                                    lastline = null;
                                }
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

        private static string logFileBaseName;
        private static Thread logFileWriterThread;
        private static System.Diagnostics.TextWriterTraceListener tracelistener;
        private static BlockingCollection<string> LogLineQueue = new BlockingCollection<string>();

        private class TraceLogWriter : TextWriter
        {
            private ThreadLocal<StringBuilder> logline = new ThreadLocal<StringBuilder>(() => new StringBuilder());

            public override Encoding Encoding { get { return Encoding.UTF8; } }
            public override IFormatProvider FormatProvider { get { return CultureInfo.InvariantCulture; } }

            public override void Write(string value)    // receive string from trace log
            {
                if (value != null)
                {
                    logline.Value.Append(value);            // we append to the SB, per thread.

                    string logtext = logline.ToString();
                    // find last new line in the string buffer (last \n so we can pass long bits of text to it with \n in the middle)
                    var lastnewline = logtext.LastIndexOf('\n');     

                    if (lastnewline>=0)     // if we have a line, we submit it to the trace log
                    {
                        string submit = logtext.Substring(0, lastnewline);
                        TraceLog.WriteLine(submit);

                        logline.Value.Clear();      // clear the buffer

                        if (lastnewline < logtext.Length - 1)    // if we have anything left in logval, add it back to the buffer
                        {
                            logtext = logtext.Substring(lastnewline + 1);
                            logline.Value.Append(logtext);
                        }
                    }
                }
            }

            public override void Write(char value) { Write(new string(new[] { value })); }
            public override void WriteLine(string value) { Write((value ?? "") + "\n"); }
            public override void WriteLine() { Write("\n"); }
        }


    }

}
