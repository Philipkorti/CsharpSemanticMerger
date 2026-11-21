using CsharpSemanticMerger.Core.Enums;
using CsharpSemanticMerger.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CsharpSemanticMerger.Core.Reconstruction
{
    public class SourceReconstructionService
    {
        /// <summary>
        /// Reconstructs both the HEAD and Incoming source strings from the provided segments.
        /// </summary>
        /// <param name="segments">The ordered segments produced by <c>GitParsingService.ParseGitConflict</c>.</param>
        /// <param name="newLine">The line separator used to join lines into the final source strings.</param>
        /// <param name="preserveTrailingNewline">
        /// When <see langword="true"/>, appends a trailing <paramref name="newLine"/> if at least one line was emitted.
        /// </param>
        /// <returns>A <see cref="ReconstructedSources"/> instance containing <c>HeadVersion</c> and <c>IncomingVersion</c>.</returns>
        public ReconstructedSources Reconstruct(IEnumerable<FileSegment> segments, string newLine = "\n", bool preserveTrailingNewline = false)
        {
            if(segments is null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            if(newLine is null)
            {
                throw new ArgumentNullException(nameof(newLine));
            }

            string headVersion = BuildSideSourceText(segments, ConflictSide.Head, newLine, preserveTrailingNewline);
            string incomingVersion = BuildSideSourceText(segments, ConflictSide.Incoming, newLine, preserveTrailingNewline);

            return new ReconstructedSources(headVersion, incomingVersion);
        }

        /// <summary>
        /// Builds the reconstructed source string for a single <see cref="ConflictSide"/>.
        /// </summary>
        /// <param name="segments">The ordered file segments.</param>
        /// <param name="side">The conflict side (<c>Head</c> or <c>Incoming</c>) to include.</param>
        /// <param name="newLine">The line separator used between lines.</param>
        /// <param name="preserveTrailingNewline">Whether to append a trailing newline when lines are present.</param>
        /// <returns>The reconstructed C# source string without conflict markers.</returns>
        public string BuildSideSourceText(IEnumerable<FileSegment> segments, ConflictSide side, string newLine = "\n", bool preserveTrailingNewline = false)
        {
            if (segments is null)
            {
                throw new ArgumentNullException(nameof(segments));
            }
            if (newLine is null)
            {
                throw new ArgumentNullException(nameof(newLine));
            }

            var builder = new StringBuilder();
            bool hasAnyLine = false;

            foreach(FileSegment segment in segments)
            {
                switch (segment)
                {
                    case UnchangedSegment unchanged:
                        AppendLines(builder, unchanged.Lines, newLine, ref hasAnyLine);
                        break;
                    case ConflictSegment conflict:
                        IReadOnlyList<string> sideLines = side == ConflictSide.Head 
                            ? conflict.HeadLines
                            : conflict.IncomingLines;

                        AppendLines(builder, sideLines, newLine, ref hasAnyLine);
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported segment type '{segment.GetType().Name}'.");
                }
            }

            if (preserveTrailingNewline && hasAnyLine)
            {
                builder.Append(newLine);
            }

            return builder.ToString();
        }

        private static void AppendLines(StringBuilder builder, IReadOnlyList<string> lines, string newLine,  ref bool hasAnyLine)
        {
            for(int i = 0; i < lines.Count; i++)
            {
                if (hasAnyLine)
                {
                    builder.Append(newLine);
                }

                // Append the exact line string so indentation, spacing, and comments/trivia remain untouched
                builder.Append(lines[i]);
                hasAnyLine = true;
            }
        }
    }
}
