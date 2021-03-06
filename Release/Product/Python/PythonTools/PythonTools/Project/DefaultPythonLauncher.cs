﻿/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.PythonTools.Debugger.DebugEngine;
using Microsoft.PythonTools.Interpreter;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.PythonTools.Project {
    /// <summary>
    /// Implements functionality of starting a project or a file with or without debugging.
    /// </summary>
    sealed class DefaultPythonLauncher : IProjectLauncher {
        private readonly IPythonProject/*!*/ _project;

        public DefaultPythonLauncher(IPythonProject/*!*/ project) {
            Utilities.ArgumentNotNull("project", project);

            _project = project;
        }

        #region IPythonLauncher Members

        public int LaunchProject(bool debug) {
            string startupFile = ResolveStartupFile();
            return LaunchFile(startupFile, debug);
        }

        public int LaunchFile(string/*!*/ file, bool debug) {
            if (debug) {
                StartWithDebugger(file);
            } else {
                StartWithoutDebugger(file);
            }
            return VSConstants.S_OK;
        }

        #endregion

        public string InstallPath {
            get {
                return Path.GetDirectoryName(InterpreterExecutable);
            }
        }

        private string InterpreterExecutable {
            get {
                return _project.GetInterpreterFactory().Configuration.InterpreterPath;
            }
        }

        private string WindowsInterpreterExecutable {
            get {
                return _project.GetInterpreterFactory().Configuration.WindowsInterpreterPath;
            }
        }

        /// <summary>
        /// Returns full path of the language specififc iterpreter executable file.
        /// </summary>
        public string GetInterpreterExecutable(out bool isWindows) {
            isWindows = Convert.ToBoolean(_project.GetProperty(CommonConstants.IsWindowsApplication));
            return isWindows ? WindowsInterpreterExecutable : InterpreterExecutable;
        }

        private string/*!*/ GetInterpreterExecutableInternal(out bool isWindows) {
            string result;
            result = (_project.GetProperty(CommonConstants.InterpreterPath) ?? "").Trim();
            if (!String.IsNullOrEmpty(result)) {
                if (!File.Exists(result)) {
                    throw new FileNotFoundException(String.Format("Interpreter specified in the project does not exist: '{0}'", result), result);
                }
                isWindows = false;
                return result;
            }


            result = GetInterpreterExecutable(out isWindows);
            if (result == null) {
                Contract.Assert(result != null);
            }
            return result;
        }

        /// <summary>
        /// Creates language specific command line for starting the project without debigging.
        /// </summary>
        public string CreateCommandLineNoDebug(string startupFile) {
            string cmdLineArgs = _project.GetProperty(CommonConstants.CommandLineArguments);

            return String.Format("{0} \"{1}\" {2}", GetOptions(), startupFile, cmdLineArgs);
        }

        /// <summary>
        /// Creates language specific command line for starting the project with debigging.
        /// </summary>
        public string CreateCommandLineDebug(string startupFile) {
            return CreateCommandLineNoDebug(startupFile);
        }

        /// <summary>
        /// Default implementation of the "Start withput Debugging" command.
        /// </summary>
        private void StartWithoutDebugger(string startupFile) {
            Process.Start(CreateProcessStartInfoNoDebug(startupFile));
        }

        /// <summary>
        /// Default implementation of the "Start Debugging" command.
        /// </summary>
        private void StartWithDebugger(string startupFile) {
            VsDebugTargetInfo dbgInfo = new VsDebugTargetInfo();
            dbgInfo.cbSize = (uint)Marshal.SizeOf(dbgInfo);
            
            SetupDebugInfo(ref dbgInfo, startupFile);

            LaunchDebugger(PythonToolsPackage.Instance, dbgInfo);
        }

        private static void LaunchDebugger(IServiceProvider provider, VsDebugTargetInfo dbgInfo) {
            if (!Directory.Exists(UnquotePath(dbgInfo.bstrCurDir))) {
                MessageBox.Show(String.Format("Working directory \"{0}\" does not exist.", dbgInfo.bstrCurDir));
            } else if (!File.Exists(UnquotePath(dbgInfo.bstrExe))) {
                MessageBox.Show(String.Format("Interpreter \"{0}\" does not exist.", dbgInfo.bstrExe));
            } else {
                VsShellUtilities.LaunchDebugger(provider, dbgInfo);
            }
        }

        private static string UnquotePath(string p) {
            if (p.StartsWith("\"") && p.EndsWith("\"")) {
                return p.Substring(1, p.Length - 2);
            }
            return p;
        }

        /// <summary>
        /// Sets up debugger information.
        /// </summary>
        private void SetupDebugInfo(ref VsDebugTargetInfo dbgInfo, string startupFile) {
            dbgInfo.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            bool isWindows;
            dbgInfo.bstrExe = GetInterpreterExecutableInternal(out isWindows);
            dbgInfo.bstrCurDir = _project.GetWorkingDirectory();
            dbgInfo.bstrArg = CreateCommandLineDebug(startupFile);
            dbgInfo.bstrRemoteMachine = null;

            dbgInfo.fSendStdoutToOutputWindow = 0;
            StringDictionary env = new StringDictionary();
            dbgInfo.bstrOptions = AD7Engine.VersionSetting + "=" + _project.GetInterpreterFactory().GetLanguageVersion().ToString();
            if (!isWindows && PythonToolsPackage.Instance.OptionsPage.WaitOnExit) {
                dbgInfo.bstrOptions += ";" + AD7Engine.WaitOnAbnormalExitSetting + "=True";
            }

            SetupEnvironment(env);
            if (env.Count > 0) {
                // add any inherited env vars
                var variables = Environment.GetEnvironmentVariables();
                foreach (var key in variables.Keys) {
                    string strKey = (string)key;
                    if (!env.ContainsKey(strKey)) {
                        env.Add(strKey, (string)variables[key]);
                    }
                }

                //Environemnt variables should be passed as a
                //null-terminated block of null-terminated strings. 
                //Each string is in the following form:name=value\0
                StringBuilder buf = new StringBuilder();
                foreach (DictionaryEntry entry in env) {
                    buf.AppendFormat("{0}={1}\0", entry.Key, entry.Value);
                }
                buf.Append("\0");
                dbgInfo.bstrEnv = buf.ToString();
            }
            // Set the Python debugger
            dbgInfo.clsidCustom = new Guid(AD7Engine.DebugEngineId);
            dbgInfo.grfLaunch = (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd;
        }

        /// <summary>
        /// Sets up environment variables before starting the project.
        /// </summary>
        private void SetupEnvironment(StringDictionary environment) {
            string searchPath = _project.GetProperty(CommonConstants.SearchPath);
            if (!String.IsNullOrWhiteSpace(searchPath)) {
                environment[this._project.GetInterpreterFactory().Configuration.PathEnvironmentVariable] = searchPath;
            }
        }

        private string GetOptions() {
            return "";
        }

        /// <summary>
        /// Creates process info used to start the project with no debugging.
        /// </summary>
        private ProcessStartInfo CreateProcessStartInfoNoDebug(string startupFile) {
            string command = CreateCommandLineNoDebug(startupFile);

            bool isWindows;
            string interpreter = GetInterpreterExecutableInternal(out isWindows);
            ProcessStartInfo startInfo;
            if (!isWindows && PythonToolsPackage.Instance.OptionsPage.WaitOnExit) {
                command = "/c \"\"" + interpreter + "\" " + command + " & if errorlevel 1 pause\"";
                startInfo = new ProcessStartInfo("cmd.exe", command);
            } else {
                startInfo = new ProcessStartInfo(interpreter, command);
            }

            startInfo.WorkingDirectory = _project.GetWorkingDirectory();

            //In order to update environment variables we have to set UseShellExecute to false
            startInfo.UseShellExecute = false;
            SetupEnvironment(startInfo.EnvironmentVariables);
            return startInfo;
        }

        private string ResolveStartupFile() {
            string startupFile = _project.GetStartupFile();
            if (string.IsNullOrEmpty(startupFile)) {
                //TODO: need to start active file then
                throw new ApplicationException("No startup file is defined for the startup project.");
            }
            return startupFile;
        }
    }
}
