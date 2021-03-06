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
using Microsoft.PythonTools.Analysis.Interpreter;
using Microsoft.PythonTools.Interpreter;
using Microsoft.PythonTools.Parsing.Ast;

namespace Microsoft.PythonTools.Analysis.Values {
    internal class BuiltinClassInfo : BuiltinNamespace<IPythonType>, IReferenceableContainer {
        private BuiltinInstanceInfo _inst;
        private string _doc;
        private readonly MemberReferences _referencedMembers = new MemberReferences();
        private ReferenceDict _references;

        private static string[] EmptyStrings = new string[0];

        public BuiltinClassInfo(IPythonType classObj, PythonAnalyzer projectState)
            : base(classObj, projectState) {
            // TODO: Get parameters from ctor
            // TODO: All types should be shared via projectState
            _doc = null;
        }

        public override IPythonType PythonType {
            get { return _type; }
        }

        public override ISet<Namespace> Call(Node node, AnalysisUnit unit, ISet<Namespace>[] args, string[] keywordArgNames) {
            // TODO: More Type propagation
            IAdvancedPythonType advType = _type as IAdvancedPythonType;
            if (advType != null) {
                var types = advType.GetTypesPropagatedOnCall();
                if (types != null) {
                    ISet<Namespace>[] propagating = new ISet<Namespace>[types.Count];
                    for (int i = 0; i < propagating.Length; i++) {
                        propagating[i] = unit.ProjectState.GetInstance(types[i]).SelfSet;
                    }
                    foreach (var arg in args) {
                        arg.Call(node, unit, propagating, EmptyStrings);
                    }
                }
            }

            return Instance.SelfSet;
        }

        public BuiltinInstanceInfo Instance {
            get {
                return _inst ?? (_inst = MakeInstance());
            }
        }

        private BuiltinInstanceInfo MakeInstance() {
            if (_type.TypeId == BuiltinTypeId.Int || _type.TypeId == BuiltinTypeId.Long || _type.TypeId == BuiltinTypeId.Float || _type.TypeId  == BuiltinTypeId.Complex) {
                return new NumericInstanceInfo(this);
            }

            return new BuiltinInstanceInfo(this);
        }

        /// <summary>
        /// Returns the overloads avaialble for calling the constructor of the type.
        /// </summary>
        public override ICollection<OverloadResult> Overloads {
            get {
                // TODO: sometimes might have a specialized __init__.
                // This just covers typical .NET types
                var ctors = _type.GetConstructors();

                if (ctors != null) {
                    var result = new OverloadResult[ctors.Overloads.Count];
                    for (int i = 0; i < result.Length; i++) {
                        result[i] = new BuiltinFunctionOverloadResult(ProjectState, ctors.Overloads[i], 1, _type.Name);
                    }
                    return result;
                }
                return new OverloadResult[0];
            }
        }        

        public override ISet<Namespace> GetMember(Node node, AnalysisUnit unit, string name) {
            var res = base.GetMember(node, unit, name);
            if (res.Count > 0) {
                _referencedMembers.AddReference(node, unit, name);
                return res.GetStaticDescriptor(unit);
            }
            return res;
        }

        public override void SetMember(Node node, AnalysisUnit unit, string name, ISet<Namespace> value) {
            var res = base.GetMember(node, unit, name);
            if (res.Count > 0) {
                _referencedMembers.AddReference(node, unit, name);
            }
        }

        public override ISet<Namespace> GetIndex(Node node, AnalysisUnit unit, ISet<Namespace> index) {
            // TODO: Needs to actually do indexing on type
            var clrType = _type as IAdvancedPythonType;
            if (clrType == null || !clrType.IsGenericTypeDefinition) {
                return EmptySet<Namespace>.Instance;
            }
            
            var result = new HashSet<Namespace>();
            foreach (var indexType in index) {
                if (indexType is BuiltinClassInfo) {
                    var clrIndexType = indexType.PythonType;
                    try {
                        var klass = ProjectState.MakeGenericType(clrType, clrIndexType);
                        result.Add(klass);
                    } catch {
                        // wrong number of type args, violated constraint, etc...
                    }
                } else if (indexType is SequenceInfo) {
                    List<IPythonType>[] types = GetSequenceTypes(indexType as SequenceInfo);

                    if (!MissingType(types)) {
                        foreach (IPythonType[] indexTypes in GetTypeCombinations(types)) {                            
                            try {
                                var klass = ProjectState.MakeGenericType(clrType, indexTypes);
                                result.Add(klass);
                            } catch {
                                // wrong number of type args, violated constraint, etc...
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static IEnumerable<IPythonType[]> GetTypeCombinations(List<IPythonType>[] types) {
            List<IPythonType> res = new List<IPythonType>();
            for (int i = 0; i < types.Length; i++) {
                res.Add(null);
            }

            return GetTypeCombinationsWorker(types, res, 0);
        }

        private static IEnumerable<IPythonType[]> GetTypeCombinationsWorker(List<IPythonType>[] types, List<IPythonType> res, int curIndex) {
            if (curIndex == types.Length) {
                yield return res.ToArray();
            } else {
                foreach (IPythonType t in types[curIndex]) {
                    res[curIndex] = t;

                    foreach (var finalRes in GetTypeCombinationsWorker(types, res, curIndex + 1)) {
                        yield return finalRes;
                    }
                }
            }
        }

        private static List<IPythonType>[] GetSequenceTypes(SequenceInfo seq) {
            List<IPythonType>[] types = new List<IPythonType>[seq.IndexTypes.Length];
            for (int i = 0; i < types.Length; i++) {
                foreach (var seqIndexType in seq.IndexTypes[i]) {
                    if (seqIndexType is BuiltinClassInfo) {
                        if (types[i] == null) {
                            types[i] = new List<IPythonType>();
                        }

                        types[i].Add(seqIndexType.PythonType);
                    }
                }
            }
            return types;
        }

        private static bool MissingType(List<IPythonType>[] types) {
            for (int i = 0; i < types.Length; i++) {
                if (types[i] == null) {
                    return true;
                }
            }
            return false;
        }

        public override string Description {
            get {
                var res = ShortDescription;
                if (!String.IsNullOrEmpty(Documentation)) {
                    res += Environment.NewLine + Documentation;
                }
                return res;
            }
        }

        public override string ShortDescription {
            get {
                return (_type.IsBuiltin ? "type " : "class ") + _type.Name;
            }
        }

        public override string Documentation {
            get {
                if (_doc == null) {
                    try {
                        var doc = _type.Documentation;
                        _doc = Utils.StripDocumentation(doc.ToString());
                    } catch {
                        _doc = String.Empty;
                    }
                }
                return _doc;
            }
        }

        public override PythonMemberType ResultType {
            get {
                return _type.MemberType;
            }
        }

        public override string ToString() {
            // return 'Class#' + hex(id(self)) + ' ' + self.clrType.__name__
            return "Class " + _type.Name;
        }

        #region IReferenceableContainer Members

        public IEnumerable<IReferenceable> GetDefinitions(string name) {
            return _referencedMembers.GetDefinitions(name);
        }

        #endregion

        internal void AddMemberReference(Node node, AnalysisUnit unit, string name) {
            _referencedMembers.AddReference(node, unit, name);
        }

        internal override void AddReference(Node node, AnalysisUnit unit) {
            if (!unit.ForEval) {
                if (_references == null) {
                    _references = new ReferenceDict();
                }
                _references.GetReferences(unit.DeclaringModule.ProjectEntry).AddReference(new SimpleSrcLocation(node.Span));
            }
        }

        public override IEnumerable<LocationInfo> References {
            get {
                if (_references != null) {
                    return _references.AllReferences;
                }
                return new LocationInfo[0];
            }
        }
    }
}
