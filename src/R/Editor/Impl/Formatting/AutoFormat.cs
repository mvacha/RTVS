﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.Tree.Definitions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    internal static class AutoFormat {
        public static bool IsAutoformatTriggerCharacter(char ch) {
            return ch == '\n' || ch == '\r' || ch == ';' || ch == '}';
        }
        public static bool IgnoreOnce { get; set; }

        public static void HandleAutoformat(ITextView textView, char typedChar) {
            if (!REditorSettings.AutoFormat || IgnoreOnce) {
                IgnoreOnce = false;
                return;
            }

            SnapshotPoint? rPoint = GetCaretPointInBuffer(textView);
            if (!rPoint.HasValue) {
                return;
            }

            var document = REditorDocument.FromTextBuffer(textView.TextBuffer);
            var et = document.EditorTree;
            var ast = (!et.IsReady && et.PreviousAstRoot != null) ? et.PreviousAstRoot : et.AstRoot;

            // We don't want to auto-format inside strings
            if (ast.IsPositionInsideString(rPoint.Value.Position)) {
                return;
            }

            ITextBuffer subjectBuffer = rPoint.Value.Snapshot.TextBuffer;
            if (typedChar.IsLineBreak()) {
                // Special case for hitting caret after } and before 'else'. We do want to format
                // the construct as '} else {' but if user types Enter after } and we auto-format
                // it will look as if the editor just eats the Enter. Instead, we will not be
                // autoformatting in this specific case. User can always format either the document
                // or select the block and reformat it.
                et.EnsureTreeReady();
                if (!IsBetweenCurlyAndElse(subjectBuffer, rPoint.Value.Position)) {
                    bool formatScope = ShouldFormatScope(textView, subjectBuffer, et.AstRoot , - 1);
                    if (formatScope) {
                        FormatOperations.FormatCurrentNode<IStatement>(textView, subjectBuffer);
                    } else {
                        FormatOperations.FormatLine(textView, subjectBuffer, et.AstRoot, -1);
                    }
                }
            } else if (typedChar == ';') {
                // Verify we are at the end of the string and not in a middle
                // of another string or inside a statement.
                ITextSnapshotLine line = subjectBuffer.CurrentSnapshot.GetLineFromPosition(rPoint.Value.Position);
                int positionInLine = rPoint.Value.Position - line.Start;
                string lineText = line.GetText();
                if (positionInLine >= lineText.TrimEnd().Length) {
                    et.EnsureTreeReady();
                    FormatOperations.FormatLine(textView, subjectBuffer, et.AstRoot, 0);
                }
            } else if (typedChar == '}') {
                FormatOperations.FormatNode<IStatement>(textView, subjectBuffer, Math.Max(rPoint.Value - 1, 0));
            }
        }

        private static bool IsBetweenCurlyAndElse(ITextBuffer textBuffer, int position) {
            // Note that this is post-typing to the construct is now '}<line_break>else'
            int lineNum = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(position);
            if (lineNum < 1) {
                return false;
            }

            ITextSnapshotLine prevLine = textBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNum - 1);

            string leftSide = prevLine.GetText().TrimEnd();
            if (!leftSide.EndsWith("}", StringComparison.Ordinal)) {
                return false;
            }

            ITextSnapshotLine currentLine = textBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNum);
            string rightSide = currentLine.GetText().TrimStart();
            if (!rightSide.StartsWith("else", StringComparison.Ordinal)) {
                return false;
            }

            return true;
        }

        private static SnapshotPoint? GetCaretPointInBuffer(ITextView textView) {
            return textView.BufferGraph.MapDownToFirstMatch(
                textView.Caret.Position.BufferPosition,
                PointTrackingMode.Positive,
                snapshot => snapshot.TextBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType),
                PositionAffinity.Successor
            );
        }

        private static SnapshotPoint? MapCaretToBuffer(ITextView textView, ITextBuffer textBuffer) {
            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            return textView.MapDownToBuffer(textView.Caret.Position.BufferPosition, textBuffer);
        }

        private static bool ShouldFormatScope(ITextView textView, ITextBuffer textBuffer, AstRoot ast, int lineOffset) {
            SnapshotPoint? caret = MapCaretToBuffer(textView, textBuffer);
            if (caret.HasValue) {
                try {
                    int lineNumber = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(caret.Value.Position);
                    ITextSnapshotLine line = textBuffer.CurrentSnapshot.GetLineFromLineNumber(Math.Max(lineNumber - 1, 0));
                    string lineText = line.GetText();
                    if (lineText.TrimEnd().EndsWith("}", StringComparison.Ordinal)) {
                        IKeywordScopeStatement scopeStatement = ast.GetNodeOfTypeFromPosition<IKeywordScopeStatement>(caret.Value);
                        return scopeStatement != null;
                    }
                } catch (Exception) { }
            }

            return false;
        }
    }
}
