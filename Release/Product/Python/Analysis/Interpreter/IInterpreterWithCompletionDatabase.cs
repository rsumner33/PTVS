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

namespace Microsoft.PythonTools.Interpreter {
    /// <summary>
    /// Implemented by Python interpreters which support generating a completion database ahead of time.
    /// 
    /// This interface is implemented on a class which also implements IPythonInterpreterFactory.
    /// </summary>
    public interface IInterpreterWithCompletionDatabase {
        /// <summary>
        /// Generates the completeion database.  After analysis is complete databaseGenerationCompleted should be called.
        /// 
        /// Returns true if analysis is proceeding on a background thread, false if the analysis completed synchronessly.
        /// </summary>
        bool GenerateCompletionDatabase(GenerateDatabaseOptions options, Action databaseGenerationCompleted);

        /// <summary>
        /// Generates the completion database if it has not already been generated.  Called only if the user has
        /// not disabled the option to automatically generate a completion database.
        /// 
        /// The database should be generated in the background.
        /// </summary>
        void AutoGenerateCompletionDatabase();
    }
}
