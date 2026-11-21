using CsharpSemanticMerger.Core.Enums;
using CsharpSemanticMerger.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CsharpSemanticMerger.Core.Mapping
{
    /// <summary>
    /// Reconstructs HEAD and Incoming file versions from parsed <see cref="FileSegment"/> items
    /// and builds <see cref="LineMapping"/> instances to track line number shifts.
    /// </summary>
    public sealed class LineMappingService
    {
        /// <summary>
        /// Reconstructs the HEAD version and its line mapping from the parsed file segments.
        /// </summary>
        /// <param name="segments">The ordered segments produced by <see cref="GitParsing.GitParsingService"/>.</param>
        /// <param name="newLine">The line ending sequence used when joining reconstructed lines.</param>
        /// <returns>A <see cref="ReconstructedSide"/> containing the HEAD content and line mapping.</returns>
        public ReconstructedSide BuildHead(IEnumerable<FileSegment> segments, string newLine = "\n")
        {
            return BuildReconstructedSide(segments, ConflictSide.Head, newLine);
        }

        /// <summary>
        /// Reconstructs the Incoming version and its line mapping from the parsed file segments.
        /// </summary>
        /// <param name="segments">The ordered segments produced by <see cref="GitParsing.GitParsingService"/>.</param>
        /// <param name="newLine">The line ending sequence used when joining reconstructed lines.</param>
        /// <returns>A <see cref="ReconstructedSide"/> containing the Incoming content and line mapping.</returns>
        public ReconstructedSide BuildIncoming(IEnumerable<FileSegment> segments, string newLine = "\n")
        {
            return BuildReconstructedSide(segments, ConflictSide.Incoming, newLine);
        }

        private static ReconstructedSide BuildReconstructedSide(IEnumerable<FileSegment> segments,ConflictSide conflictSide, string newLine)
        {
            if(segments is null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            if(newLine is null)
            {
                throw new ArgumentNullException(nameof(newLine));
            }

            var outputLines = new List<string>();
            var virtualToOriginal = new List<int>();

            foreach(FileSegment segment in segments)
            {
                switch (segment)
                {
                    case UnchangedSegment unchangedSegment:
                        {
                            // Unchanged lines preserve thir exact physical line indices
                            for(int i = 0; i < unchangedSegment.Lines.Count; i++)
                            {
                                outputLines.Add(unchangedSegment.Lines[i]);
                                virtualToOriginal.Add(unchangedSegment.StartLine + i);
                            }
                            break;
                        }
                    case ConflictSegment conflictSegment:
                        {
                            if(conflictSide == ConflictSide.Head)
                            {
                                // First HEAD line sits immediately after the opening '<<<<<<<' marker line
                                int headSTartPhysicalLine = conflictSegment.StartLine + 1;
                                for(int i = 0; i < conflictSegment.HeadLines.Count; i++)
                                {
                                    outputLines.Add(conflictSegment.HeadLines[i]);
                                    virtualToOriginal.Add(headSTartPhysicalLine + i);
                                }
                            }
                            else
                            {
                                // Account for '<<<<<<<' (1 line), HeadLines, optional diff3 '|||||||' + BaseLines, and '=======' (1 line)
                                int incomingStartPhysicalLine = conflictSegment.StartLine +1 + conflictSegment.HeadLines.Count;

                                if(conflictSegment.BaseLines is not null && conflictSegment.BaseLines.Count > 0)
                                {
                                    incomingStartPhysicalLine += 1 + conflictSegment.BaseLines.Count;
                                }

                                // Advance past the '=======' separator line
                                incomingStartPhysicalLine += 1;

                                for(int i = 0; i < conflictSegment.IncomingLines.Count; i++)
                                {
                                    outputLines.Add(conflictSegment.IncomingLines[i]);
                                    virtualToOriginal.Add(incomingStartPhysicalLine + i);
                                }
                            }
                            break;
                        }
                    default: 
                        throw new NotSupportedException($"Unsupported segment type {segment.GetType().Name}.");

                }
            }

            string content = string.Join(newLine, outputLines);
            return new ReconstructedSide(content, new LineMapping(virtualToOriginal));
        }
    }
}
