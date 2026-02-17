/*
 * Copyright 2016 - 2026 EDDiscovery development team
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace BaseUtils
{
    public class Processes
    {
        [DllImport("shell32.dll")]
        static extern int FindExecutable(string lpFile, string lpDirectory, [Out] System.Text.StringBuilder lpResult);

        private Dictionary<int, Process> processes;

        public Processes()
        {
            processes = new Dictionary<int, Process>();
        }

        public void CloseAll()
        {
            foreach (int i in processes.Keys)
            {
                processes[i].Close();
            }

            processes.Clear();
        }

        public int StartProcess(string proc, string cmdline, string runas = null)
        {
            Process p = new Process();
            p.StartInfo.FileName = proc;
            p.StartInfo.Arguments = cmdline;

            if (runas != null)
                p.StartInfo.Verb = runas;

            try
            {
                bool ok = p.Start();
                if (ok)
                {
                    processes[p.Id] = p;
                    return p.Id;
                }
            }
            catch( Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception " + ex);
            }
            return 0;
        }

        public int FindProcess(string proc)
        {
            Process[] curprocs = Process.GetProcesses();
            Process f = Array.Find(curprocs, x => x.ProcessName.Equals(proc, StringComparison.InvariantCultureIgnoreCase));
            if (f != null)
            {
                processes[f.Id] = f;
                return f.Id;
            }
            else
                return 0;
        }

        static public string[] ListProcesses()
        {
            Process[] curprocs = Process.GetProcesses();
            return (from x in curprocs select x.ProcessName).ToArray();
        }

        public enum ProcessResult { OK, UnknownPID, Timeout, NotExited };

        public ProcessResult WaitForProcess(int pid, int timeout)
        {
            if (processes.ContainsKey(pid))
            {
                return processes[pid].WaitForExit(timeout) ? ProcessResult.OK : ProcessResult.Timeout;
            }
            else
                return ProcessResult.UnknownPID;
        }

        public ProcessResult KillProcess(int pid)
        {
            if (processes.ContainsKey(pid))
            {
                processes[pid].Kill();
                return ProcessResult.OK;
            }
            else
                return ProcessResult.UnknownPID;
        }

        public ProcessResult CloseProcess(int pid)
        {
            if (processes.ContainsKey(pid))
            {
                processes[pid].CloseMainWindow();
                return ProcessResult.OK;
            }
            else
                return ProcessResult.UnknownPID;
        }

        public ProcessResult HasProcessExited(int pid, out int exitcode)
        {
            exitcode = 0;
            if (processes.ContainsKey(pid))
            {
                if (processes[pid].HasExited)
                {
                    exitcode = processes[pid].ExitCode;
                    return ProcessResult.OK;
                }
                else
                    return ProcessResult.NotExited;
            }
            else
                return ProcessResult.UnknownPID;
        }

        public static string GetExecutableForFile(string path)
        {
            System.Text.StringBuilder res = new System.Text.StringBuilder();
            int v = FindExecutable(path, null, res);
            if (v >= 32) // ! whoosh
                return res.ToString();
            else
                return null;
        }


        // open a text file, goto first line containing all of the text elements
        // uses assigned editor or notepad
        // returns PID, -1 if not opened
        public static int OpenEditorForTextFileAtText(string file, string[] text = null)
        {
            string[] lines;
            if (file != null && (lines = FileHelpers.TryReadAllLinesFromFile(file)) != null)
            {
                int lineno = text != null ? Array.FindIndex(lines, x => x.ContainsCount(text, StringComparison.InvariantCultureIgnoreCase) == text.Length) : -1;

                string exeforlog = Processes.GetExecutableForFile(file);

                string exe = "Notepad.exe";
                string cmd = $"\"{file}\"";

                if (exeforlog != null) // got one
                {
                    exe = exeforlog;

                    if (exe.ContainsIIC("Notepad++"))
                    {
                        cmd = lineno >= 0 ? $"-n{lineno + 1} \"{file}\"" : $"\"{file}\"";
                    }
                    else if (exe.ContainsIIC("\\code"))
                    {
                        cmd = lineno >= 0 ? $"--goto \"{file}:{lineno + 1}\"" : $"\"{file}\"";
                    }
                }

                BaseUtils.Processes process = new BaseUtils.Processes();
                return process.StartProcess(exe, cmd);
            }

            return -1;
        }

    }
}
