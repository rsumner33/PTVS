﻿// Python Tools for Visual Studio
// Copyright(c) Microsoft Corporation
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the License); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED ON AN  *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY
// IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

using System.Threading.Tasks;
using Microsoft.PythonTools.VsCode.Core.Shell;
using StreamJsonRpc;

namespace Microsoft.PythonTools.VsCode.Services {
    public sealed class TelemetryService : ITelemetryService {
        private readonly JsonRpc _rpc;
        public TelemetryService(JsonRpc rpc) {
            _rpc = rpc;
        }
        public Task SendTelemetry(object o) => _rpc.NotifyWithParameterObjectAsync("telemetry/event", o);
    }
}
