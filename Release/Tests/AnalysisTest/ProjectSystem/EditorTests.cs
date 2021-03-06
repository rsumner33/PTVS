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
using System.Text;
using AnalysisTest.UI;
using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using TestUtilities;

namespace AnalysisTest.ProjectSystem {
    [TestClass]
    [DeploymentItem(@"Python.VS.TestData\", "Python.VS.TestData")]
    public class EditorTests {
        [TestCleanup]
        public void MyTestCleanup() {
            VsIdeTestHostContext.Dte.Solution.Close(false);
        }

        #region Test Cases

        [TestMethod, Priority(2), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void OutliningTest() {
            var project = DebugProject.OpenProject(@"Python.VS.TestData\Outlining.sln");

            var item = project.ProjectItems.Item("Program.py");
            var window = item.Open();
            window.Activate();


            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            var doc = app.GetDocument(item.Document.FullName);

            var snapshot = doc.TextView.TextBuffer.CurrentSnapshot;
            var tags = doc.GetTaggerAggregator<IOutliningRegionTag>(doc.TextView.TextBuffer).GetTags(new SnapshotSpan(snapshot, 0, snapshot.Length));

            VerifyTags(doc.TextView.TextBuffer, tags,
                new ExpectedTag(8, 18, "\r\n    pass"),
                new ExpectedTag(40, 50, "\r\n    pass"),
                new ExpectedTag(72, 82, "\r\n    pass"),
                new ExpectedTag(104, 131, "\r\n    pass\r\nelse:\r\n    pass"),
                new ExpectedTag(153, 185, "\r\n    pass\r\nelif True:\r\n    pass")
            );
        }

        [TestMethod, Priority(2), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ClassificationTest() {
            var project = DebugProject.OpenProject(@"Python.VS.TestData\Classification.sln");

            var item = project.ProjectItems.Item("Program.py");
            var window = item.Open();
            window.Activate();


            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            var doc = app.GetDocument(item.Document.FullName);

            var snapshot = doc.TextView.TextBuffer.CurrentSnapshot;
            var classifier = doc.Classifier;
            var spans = classifier.GetClassificationSpans(new SnapshotSpan(snapshot, 0, snapshot.Length));

            VerifyClassification(doc.TextView.TextBuffer, spans,
                new Classifcation("comment", 0, 8, "#comment"),
                new Classifcation("whitespace", 8, 10, "\r\n"),
                new Classifcation("literal", 10, 11, "1"),
                new Classifcation("whitespace", 11, 13, "\r\n"),
                new Classifcation("string", 13, 18, "\"abc\""),
                new Classifcation("whitespace", 18, 20, "\r\n"),
                new Classifcation("keyword", 20, 23, "def"),
                new Classifcation("identifier", 24, 25, "f"),
                new Classifcation("Python open grouping", 25, 26, "("),
                new Classifcation("Python close grouping", 26, 27, ")"),
                new Classifcation("operator", 27, 28, ":"),
                new Classifcation("keyword", 29, 33, "pass"),
                new Classifcation("whitespace", 33, 35, "\r\n"),
                new Classifcation("string", 35, 46, "'abc\\\r\ndef'")
            );
        }

        #endregion

        #region Helpers

        private void VerifyTags(ITextBuffer buffer, IEnumerable<IMappingTagSpan<IOutliningRegionTag>> tags, params ExpectedTag[] expected) {
            var ltags = new List<IMappingTagSpan<IOutliningRegionTag>>(tags);

            Assert.AreEqual(expected.Length, ltags.Count);

            for (int i = 0; i < ltags.Count; i++) {
                int start = ltags[i].Span.Start.GetInsertionPoint(x => x == buffer).Value.Position;
                int end = ltags[i].Span.End.GetInsertionPoint(x => x == buffer).Value.Position;
                Assert.AreEqual(expected[i].Start, start);
                Assert.AreEqual(expected[i].End, end);
                Assert.AreEqual(expected[i].Text, buffer.CurrentSnapshot.GetText(Span.FromBounds(start, end)));
            }
        }

        private class ExpectedTag {
            public readonly int Start, End;
            public readonly string Text;

            public ExpectedTag(int start, int end, string text) {
                Start = start;
                End = end;
                Text = text;
            }
        }

        private void VerifyClassification(ITextBuffer buffer, IList<ClassificationSpan> spans, params Classifcation[] expected) {
            bool passed = false;
            try {
                Assert.AreEqual(expected.Length, spans.Count);

                for (int i = 0; i < spans.Count; i++) {
                    var curSpan = spans[i];


                    int start = curSpan.Span.Start.Position;
                    int end = curSpan.Span.End.Position;

                    Assert.AreEqual(expected[i].Start, start);
                    Assert.AreEqual(expected[i].End, end);
                    Assert.AreEqual(expected[i].Text, buffer.CurrentSnapshot.GetText(Span.FromBounds(start, end)));
                }
                passed = true;
            } finally {
                if (!passed) {
                    // output results for easy test creation...
                    for (int i = 0; i < spans.Count; i++) {
                        var curSpan = spans[i];

                        Console.WriteLine("new Classifcation(\"{0}\", {1}, {2}, \"{3}\"),",
                            curSpan.ClassificationType.Classification,
                            curSpan.Span.Start.Position,
                            curSpan.Span.End.Position,
                            FormatString(curSpan.Span.GetText())
                        );
                    }
                }
            }
        }

        private string FormatString(string p) {
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < p.Length; i++) {
                switch (p[i]) {
                    case '\\': res.Append("\\\\"); break;
                    case '\n': res.Append("\\n"); break;
                    case '\r': res.Append("\\r"); break;
                    case '\t': res.Append("\\t"); break;
                    case '"': res.Append("\\\""); break;
                    default: res.Append(p[i]); break;
                }
            }
            return res.ToString();
        }

        private class Classifcation {
            public readonly int Start, End;
            public readonly string Text;

            public Classifcation(string classificationType, int start, int end, string text) {
                Start = start;
                End = end;
                Text = text;
            }
        }

        #endregion

    }
}
