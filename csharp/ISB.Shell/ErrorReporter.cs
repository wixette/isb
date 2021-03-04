using System;
using System.Diagnostics;
using System.IO;
using ISB.Scanning;
using ISB.Utilities;

namespace ISB.Shell
{
    internal sealed class ErrorReport
    {
        public static void Report(string code, DiagnosticBag diagnostics, TextWriter err)
        {
            if (diagnostics == null || diagnostics.Contents.Count <= 0)
                return;
            string[] lines = code.Split(new string[] { "\r\n", "\r", "\n" },
                System.StringSplitOptions.None);

            TextRange lastRange = TextRange.None;
            foreach (var diagnostic in diagnostics.Contents)
            {
                bool showCode = diagnostic.Range != TextRange.None && diagnostic.Range != lastRange;
                ReportDiagnostic(lines, diagnostic, showCode, err);
                lastRange = diagnostic.Range;
            }
        }

        private static void ReportDiagnostic(string[] lines, Diagnostic diagnostic, bool showCode, TextWriter err)
        {
            if (showCode)
            {
                Debug.Assert(diagnostic.Range != TextRange.None);
                err.WriteLine($"Error found at Line {diagnostic.Range.Start.Line}, Col {diagnostic.Range.Start.Column}:");
                int startLine = diagnostic.Range.Start.Line;
                int endLine = diagnostic.Range.End.Line;
                for (int i = startLine; i <= endLine; i++)
                {
                    err.WriteLine(lines[i]);
                    int startColumn = (i == startLine) ?
                        Math.Max(0, diagnostic.Range.Start.Column) : 0;
                    int endColumn = (i == endLine) ?
                        Math.Min(lines[i].Length - 1, diagnostic.Range.End.Column) : lines[i].Length - 1;
                    string indicator = new String('~', endColumn - startColumn + 1);
                    string padding = startColumn > 0 ? new String(' ', startColumn) : "";
                    err.WriteLine(padding + indicator);
                }
            }
            err.WriteLine(diagnostic.ToDisplayString());
            err.WriteLine();
        }
    }
}