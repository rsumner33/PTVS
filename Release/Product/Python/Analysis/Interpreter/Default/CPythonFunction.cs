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
using Microsoft.PythonTools.Interpreter;

namespace Microsoft.PythonTools.Interpreter.Default {
    class CPythonFunction : IPythonFunction {
        private readonly string _name;
        private readonly string _doc;
        private readonly CPythonType _declaringType;
        private readonly CPythonModule _declaringModule;
        private readonly CPythonFunctionOverload[] _overloads;
        private readonly bool _isBuiltin, _isStatic;
        private static readonly CPythonFunctionOverload[] EmptyOverloads = new CPythonFunctionOverload[0];

        public CPythonFunction(TypeDatabase typeDb, string name, Dictionary<string, object> functionTable, IMemberContainer declaringType, bool isMethod = false) {
            _name = name;

            object doc;
            if (functionTable.TryGetValue("doc", out doc)) {
                _doc = doc as string;
            }

            object value;
            if (functionTable.TryGetValue("builtin", out value)) {
                _isBuiltin = Convert.ToBoolean(value);
            } else {
                _isBuiltin = true;
            }

            if (functionTable.TryGetValue("static", out value)) {
                _isStatic = Convert.ToBoolean(value);
            } else {
                _isStatic = true;
            }

            _declaringModule = (declaringType as CPythonModule) ?? (CPythonModule)((CPythonType)declaringType).DeclaringModule;
            object overloads;
            functionTable.TryGetValue("overloads", out overloads);
            _overloads = LoadOverloads(typeDb, overloads, isMethod);
            _declaringType = declaringType as CPythonType;
        }

        private CPythonFunctionOverload[] LoadOverloads(TypeDatabase typeDb, object overloads, bool isMethod) {
            var overloadsArr = overloads as IList<object>;
            if (overloadsArr != null) {
                CPythonFunctionOverload[] res = new CPythonFunctionOverload[overloadsArr.Count];

                for (int i = 0; i < overloadsArr.Count; i++) {
                    res[i] = LoadOverload(typeDb, overloadsArr[i], isMethod);
                }
                return res;
            }
            return EmptyOverloads;
        }

        private CPythonFunctionOverload LoadOverload(TypeDatabase typeDb, object overloadObj, bool isMethod) {
            return new CPythonFunctionOverload(typeDb, overloadObj as Dictionary<string, object>, isMethod);
        }

        #region IBuiltinFunction Members

        public string Name {
            get { return _name; }
        }

        public string Documentation {
            get { return _doc; }
        }

        public IList<IPythonFunctionOverload> Overloads {
            get { return _overloads; }
        }

        public IPythonType DeclaringType {
            get { return _declaringType; }
        }

        public IPythonModule DeclaringModule {
            get { return _declaringModule; }
        }

        public bool IsBuiltin {
            get {
                return _isBuiltin;
            }
        }

        public bool IsStatic {
            get {
                return _isStatic;
            }
        }

        #endregion

        #region IMember Members

        public PythonMemberType MemberType {
            get { return PythonMemberType.Function; }
        }

        #endregion
    }
}
