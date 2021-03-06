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

using System.Collections.Generic;
using System.IO;
using Microsoft.PythonTools.Analysis;
using Microsoft.PythonTools.Intellisense;

namespace Microsoft.PythonTools.Analysis {
    sealed class XamlProjectEntry : IXamlProjectEntry {
        private XamlAnalysis _analysis;
        private readonly string _filename;
        private int _version;
        private TextReader _content;
        private IAnalysisCookie _cookie;
        private Dictionary<object, object> _properties;
        private HashSet<IProjectEntry> _dependencies = new HashSet<IProjectEntry>();

        public XamlProjectEntry(string filename) {
            _filename = filename;
        }

        public void ParseContent(TextReader content, IAnalysisCookie fileCookie) {            
            _content = content;
            _cookie = fileCookie;
        }

        public void AddDependency(IProjectEntry projectEntry) {
            _dependencies.Add(projectEntry);
        }

        #region IProjectEntry Members

        public bool IsAnalyzed {
            get { return _analysis != null; }
        }

        public void Analyze() {
            lock (this) {
                if (_analysis == null) {
                    _analysis = new XamlAnalysis(_filename);
                    _cookie = new FileCookie(_filename);
                }
                _analysis = new XamlAnalysis(_content);

                _version++;

                // update any .py files which depend upon us.
                foreach (var dep in _dependencies) {
                    dep.Analyze();
                }
            }
        }

        public string FilePath { get { return _filename; } }

        public int Version {
            get {
                return _version;
            }
        }

        public string GetLine(int lineNo) {
            return _cookie.GetLine(lineNo);
        }

        public Dictionary<object, object> Properties {
            get {
                if (_properties == null) {
                    _properties = new Dictionary<object, object>();
                }
                return _properties;
            }
        }

        #endregion

        #region IXamlProjectEntry Members

        public XamlAnalysis Analysis {
            get { return _analysis; }
        }

        #endregion
    }

    public interface IXamlProjectEntry : IExternalProjectEntry {
        XamlAnalysis Analysis {
            get;
        }
    }
}
