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
using System.Windows.Forms;
using Microsoft.PythonTools.Interpreter;
using Microsoft.VisualStudio.ComponentModelHost;

namespace Microsoft.PythonTools.Project {
    public partial class PythonGeneralyPropertyPageControl : UserControl {
        private readonly PythonGeneralPropertyPage _propPage;
        private readonly IPythonInterpreterFactory[] _interpreters;

        public PythonGeneralyPropertyPageControl() {
            InitializeComponent();

            var model = (IComponentModel)PythonToolsPackage.GetGlobalService(typeof(SComponentModel));
            _interpreters = model.GetAllPythonInterpreterFactories();
            
            foreach (var interpreter in _interpreters) {
                _defaultInterpreter.Items.Add(interpreter.GetInterpreterDisplay());
            }
            if (_defaultInterpreter.Items.Count == 0) {

                _defaultInterpreter.Enabled = false;
                _defaultInterpreter.Items.Add("No Python Interpreters Installed");
                _defaultInterpreter.SelectedIndexChanged -= this.Changed;
                _defaultInterpreter.SelectedIndex = 0;
                _defaultInterpreter.SelectedIndexChanged += this.Changed;
            }
        }

        internal PythonGeneralyPropertyPageControl(PythonGeneralPropertyPage newPythonGeneralPropertyPage)
            : this() {
            _propPage = newPythonGeneralPropertyPage;
        }

        public string StartupFile {
            get { return _startupFile.Text; }
            set { _startupFile.Text = value; }
        }

        public string WorkingDirectory {
            get { return _workingDirectory.Text; }
            set { _workingDirectory.Text = value; }
        }

        public bool IsWindowsApplication {
            get { return _windowsApplication.Checked; }
            set { _windowsApplication.Checked = value; }
        }

        public IPythonInterpreterFactory DefaultInterpreter {
            get {
                if (_defaultInterpreter.SelectedIndex != -1) {
                    return _interpreters[_defaultInterpreter.SelectedIndex];
                }
                return null;
            }
        }

        public void SetDefaultInterpreter(Guid id, Version version) {
            for (int i = 0; i < _interpreters.Length; i++) {
                var interpreter = _interpreters[i];
                if (interpreter.Id == id && interpreter.Configuration.Version == version) {
                    _defaultInterpreter.SelectedIndex = i;
                    break;
                }
            }
        }

        private void Changed(object sender, EventArgs e) {
            _propPage.IsDirty = true;
        }
    }
}
