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
using Microsoft.PythonTools.Analysis.Values;
using Microsoft.PythonTools.Parsing.Ast;

namespace Microsoft.PythonTools.Analysis.Interpreter {
    sealed class ClassScope : InterpreterScope {
        public ClassScope(ClassInfo classInfo, Node ast)
            : base(classInfo, ast) {
        }

        public ClassInfo Class {
            get {
                return Namespace as ClassInfo;
            }
        }

        public override string Name {
            get { return Class.ClassDefinition.Name; }
        }

        public override bool VisibleToChildren {
            get {
                return false;
            }
        }

        public override IEnumerable<AnalysisVariable> GetVariablesForDef(string name, VariableDef def) {
            return ModuleAnalysis.ReferencablesToVariables(this.Class.GetDefinitions(name));
        }
    }
}
