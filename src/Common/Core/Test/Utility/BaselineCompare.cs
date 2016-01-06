﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using FluentAssertions;

namespace Microsoft.Common.Core.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class BaselineCompare {
        public static string CompareStrings(string expected, string actual) {
            var result = new StringBuilder();

            int length = Math.Min(expected.Length, actual.Length);
            for (int i = 0; i < length; i++) {
                if (expected[i] != actual[i]) {
                    result.Append(FormattableString.Invariant($"Position: {i}: expected: '{expected[i]}', actual '{actual[i]}'\r\n"));
                    if (i > 6 && i < length - 6) {
                        result.Append(FormattableString.Invariant($"Context: {expected.Substring(i - 6, 12)} -> {actual.Substring(i - 6, 12)}"));
                    }
                    break;
                }

            }

            if (expected.Length != actual.Length)
                result.Append(FormattableString.Invariant($"\r\nLength different. Expected: '{expected.Length}' , actual '{actual.Length}'"));

            return result.ToString();
        }

        public static void CompareStringLines(string expected, string actual) {
            string baseLine, newLine;
            int line = CompareLines(expected, actual, out baseLine, out newLine);
            line.Should().Be(0, "There should be no difference at line {0}\r\n\tExpected:\t{1}\r\n\tActual:\t{2}\r\n", line, baseLine.Trim(), newLine.Trim());
        }

        public static int CompareLines(string expected, string actual, out string baseLine, out string newLine, bool ignoreCase = false) {
            var newReader = new StringReader(actual);
            var baseReader = new StringReader(expected);

            int lineNum = 1;
            for (lineNum = 1; ; lineNum++) {
                baseLine = baseReader.ReadLine();
                newLine = newReader.ReadLine();

                if (baseLine == null || newLine == null)
                    break;

                if (string.Compare(baseLine, newLine, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) != 0)
                    return lineNum;
            }

            if (baseLine == null && newLine == null) {
                baseLine = string.Empty;
                newLine = string.Empty;

                return 0;
            }

            return lineNum;
        }

        public static void CompareFiles(string baselineFile, string actual, bool regenerateBaseline, bool ignoreCase = false) {
            if (regenerateBaseline) {
                if (File.Exists(baselineFile))
                    File.SetAttributes(baselineFile, FileAttributes.Normal);

                using (var sw = new StreamWriter(baselineFile)) {
                    sw.Write(actual);
                }
            } else {
                using (var sr = new StreamReader(baselineFile)) {
                    string expected = sr.ReadToEnd();

                    string baseLine, newLine;
                    int line = CompareLines(expected, actual, out baseLine, out newLine, ignoreCase);
                    line.Should().Be(0, "There should be no difference at line {0}\r\n\tExpected:\t{1}\r\n\tActual:\t{2}\r\nBaselineFile:\t{3}\r\n",
                        line, baseLine.Trim(), newLine.Trim(), baselineFile.Substring(baselineFile.LastIndexOf('\\') + 1));
                }
            }
        }
    }
}
