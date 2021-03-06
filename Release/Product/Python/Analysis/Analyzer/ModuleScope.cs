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

namespace Microsoft.PythonTools.Analysis.Interpreter {
    sealed class ModuleScope : InterpreterScope {

        public ModuleScope(ModuleInfo moduleInfo)
            : base(moduleInfo) {
        }

        public ModuleInfo Module { get { return Namespace as ModuleInfo; } }

        public override string Name {
            get { return Module.Name; }
        }

        public override IEnumerable<AnalysisVariable> GetVariablesForDef(string name, VariableDef def) {
            foreach (var type in def.Types) {
                if (type.Location != null) {
                    yield return new AnalysisVariable(VariableType.Definition, type.Location);
                }

                foreach (var reference in type.References) {
                    yield return new AnalysisVariable(VariableType.Reference, reference);
                }
            }
        }
    }
}
