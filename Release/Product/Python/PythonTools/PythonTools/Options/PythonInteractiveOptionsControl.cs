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
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.PythonTools.Interpreter;
using Microsoft.PythonTools.Repl;

namespace Microsoft.PythonTools.Options {
    public partial class PythonInteractiveOptionsControl : UserControl {
        private readonly List<IPythonInterpreterFactory> _factories = new List<IPythonInterpreterFactory>();
        internal const string PythonExecutionModeKey = PythonCoreConstants.BaseRegistryKey + "\\ReplExecutionModes";
        private readonly ExecutionMode[] _executionModes;

        public PythonInteractiveOptionsControl() {
            InitializeComponent();

            _executionModes = ExecutionMode.GetRegisteredModes();

            foreach (var mode in _executionModes) {
                // TODO: Support localizing these names...
                _executionMode.Items.Add(mode.FriendlyName);
            }            

            foreach (var interpreter in OptionsPage._options.Keys) {
                var display = interpreter.GetInterpreterDisplay();

                _factories.Add(interpreter);
                _showSettingsFor.Items.Add(display);

                if (interpreter.Id == PythonToolsPackage.Instance.InterpreterOptionsPage.DefaultInterpreter &&
                    interpreter.Configuration.Version == PythonToolsPackage.Instance.InterpreterOptionsPage.DefaultInterpreterVersion) {
                    _showSettingsFor.SelectedIndex = _showSettingsFor.Items.Count - 1;
                }
            }
            
            AddToolTips();

            if (_showSettingsFor.Items.Count > 0) {
                if (_showSettingsFor.SelectedIndex == -1) {
                    _showSettingsFor.SelectedIndex = 0;
                }

                RefreshOptions();
            } else {
                _showSettingsFor.Enabled = false;
                _showSettingsFor.Items.Add("No Python Interpreters Installed");
                _showSettingsFor.SelectedIndex = 0;
                _startupScript.Enabled = false;
                _executionMode.Enabled = false;
                _completionModeGroup.Enabled = false;
                _promptOptionsGroup.Enabled = false;
            }
        }

        public void NewInterpreter(IPythonInterpreterFactory factory) {
            bool firstInterpreter;
            if (firstInterpreter = !_showSettingsFor.Enabled) {
                // previously there were no interpreters installed
                _showSettingsFor.Items.Clear();
                _showSettingsFor.Enabled = true;
                _startupScript.Enabled = true;
                _executionMode.Enabled = true;
                _completionModeGroup.Enabled = true;
                _promptOptionsGroup.Enabled = true;
            }

            _factories.Add(factory);
            _showSettingsFor.Items.Add(factory.GetInterpreterDisplay());
            OptionsPage._options[factory] = OptionsPage.GetOptions(factory);
            if (firstInterpreter) {
                _showSettingsFor.SelectedIndex = 0;
            }

            RefreshOptions();
        }

        private void AddToolTips() {
            const string inlinePromptsToolTip = "When checked the prompts are in the editor buffer.  When unchecked the prompts are on the side in a separate margin.";
            const string useInterpreterPromptsToolTip = "When checked the prompt strings are defined by sys.ps1 and sys.ps2.  When unchecked the prompts are the ones configured here.";
            const string backendToolTop = @"Specifies the mode to be used for the interactive window.

The standard mode talks to a local Python process.
The IPython mode talks to an IPython kernel which can control multiple remote machines.

You can also specify a custom type as modulename.typename.  The module will 
be imported and the custom backend will be used.
";
            _tooltips.SetToolTip(_inlinePrompts, inlinePromptsToolTip);
            _tooltips.SetToolTip(_useInterpreterPrompts, useInterpreterPromptsToolTip);
            _tooltips.SetToolTip(_executionMode, backendToolTop);
            _tooltips.SetToolTip(_executionModeLabel, backendToolTop);
        }

        private void RefreshOptions() {
            if (_showSettingsFor.Enabled) {
                _smartReplHistory.Checked = CurrentOptions.ReplSmartHistory;

                switch (CurrentOptions.ReplIntellisenseMode) {
                    case ReplIntellisenseMode.AlwaysEvaluate: _evalAlways.Checked = true; break;
                    case ReplIntellisenseMode.DontEvaluateCalls: _evalNoCalls.Checked = true; break;
                    case ReplIntellisenseMode.NeverEvaluate: _evalNever.Checked = true; break;
                }

                _inlinePrompts.Checked = CurrentOptions.InlinePrompts;
                _useInterpreterPrompts.Checked = CurrentOptions.UseInterpreterPrompts;
                _priPrompt.Text = CurrentOptions.PrimaryPrompt;
                _secPrompt.Text = CurrentOptions.SecondaryPrompt;
                _startupScript.Text = CurrentOptions.StartupScript;

                int selectedExecutionMode = -1;
                for (int i = 0; i < _executionModes.Length; i++) {
                    var mode = _executionModes[i];
                    if (_showSettingsFor.SelectedIndex != -1) {
                        if (CurrentOptions.ExecutionMode == mode.Id ||
                            (String.IsNullOrWhiteSpace(CurrentOptions.ExecutionMode) && mode.Id == ExecutionMode.StandardModeId)) {

                            selectedExecutionMode = i;
                        }
                    }
                }

                if (selectedExecutionMode != -1) {
                    _executionMode.SelectedIndex = selectedExecutionMode;
                } else if (CurrentOptions.ExecutionMode != null) {
                    _executionMode.Text = CurrentOptions.ExecutionMode;
                }
            }
        }

        private PythonInteractiveOptionsPage OptionsPage {
            get {
                return PythonToolsPackage.Instance.InteractiveOptionsPage;
            }
        }

        internal PythonInteractiveOptions CurrentOptions {
            get {
                return OptionsPage._options[_factories[_showSettingsFor.SelectedIndex]];
            }
        }

        private void _smartReplHistory_CheckedChanged(object sender, EventArgs e) {
            CurrentOptions.ReplSmartHistory = _smartReplHistory.Checked;
        }

        private void _evalNever_CheckedChanged(object sender, EventArgs e) {
            if (_evalNever.Checked) {
                CurrentOptions.ReplIntellisenseMode = ReplIntellisenseMode.NeverEvaluate;
            }
        }

        private void _evalNoCalls_CheckedChanged(object sender, EventArgs e) {
            if (_evalNoCalls.Checked) {
                CurrentOptions.ReplIntellisenseMode = ReplIntellisenseMode.DontEvaluateCalls;
            }
        }

        private void _evalAlways_CheckedChanged(object sender, EventArgs e) {
            if (_evalAlways.Checked) {
                CurrentOptions.ReplIntellisenseMode = ReplIntellisenseMode.AlwaysEvaluate;
            }
        }

        private void _useInterpreterPrompts_CheckedChanged(object sender, EventArgs e) {
            _priPromptLabel.Enabled = _secPromptLabel.Enabled = _secPrompt.Enabled = _priPrompt.Enabled = !_useInterpreterPrompts.Checked;
            CurrentOptions.UseInterpreterPrompts = _useInterpreterPrompts.Checked;
        }

        private void _inlinePrompts_CheckedChanged(object sender, EventArgs e) {
            CurrentOptions.InlinePrompts = _inlinePrompts.Checked;
        }

        private void _priPrompt_TextChanged(object sender, EventArgs e) {
            CurrentOptions.PrimaryPrompt = _priPrompt.Text;
        }

        private void _secPrompt_TextChanged(object sender, EventArgs e) {
            CurrentOptions.SecondaryPrompt = _secPrompt.Text;
        }

        private void _showSettingsFor_SelectedIndexChanged(object sender, EventArgs e) {
            RefreshOptions();
        }

        private void _startupScriptButton_Click(object sender, EventArgs e) {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            if (dialog.ShowDialog() == DialogResult.OK) {
                _startupScript.Text = dialog.FileName;
            }
        }

        private void _startupScript_TextChanged(object sender, EventArgs e) {
            CurrentOptions.StartupScript = _startupScript.Text;
        }

        private void _executionMode_SelectedIndexChanged(object sender, EventArgs e) {
            if (_executionMode.SelectedIndex != -1) {
                var mode = _executionModes[_executionMode.SelectedIndex];
                CurrentOptions.ExecutionMode = mode.Id;
            }
        }

        private void _executionMode_TextChanged(object sender, EventArgs e) {
            CurrentOptions.ExecutionMode = _executionMode.Text;
        }
    }
}
