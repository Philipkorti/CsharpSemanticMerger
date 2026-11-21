using CsharpSemanticMerger.Core.Exceptions;
using CsharpSemanticMerger.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CsharpSemanticMerger.Core.GitParsing
{
    /// <summary>
    /// Provides high-performance services for parsing Git merge conflicts from raw file content.
    /// </summary>
    public class GitParsingService
    {
        /// <summary>
        /// Parses the given file content line by line, separating it into a unchanged text segments
        /// and structured conflict segments (supporting both standard 2-way and diff 3-way conflicts).
        /// </summary>
        /// <param name="fileContent">The raw content of the file containing potential Git merge conflicts markers.</param>
        /// <returns>A list of <see cref="FileSegment"/> instances representing alternating blocks of code.</returns>
        /// <exception cref="MalformedConflictException">Thrown when conflict markers are out of order, nested, or unclosed.</exception>
        public static List<FileSegment> ParseGitConflict(string fileContent)
        {
            var fileSegments = new List<FileSegment>();
            var currentUnchangedLines = new List<string>();

            // Track 0-based physical line indices in the raw conflicted file
            int currentLineIndex = 0;
            int unchangedStartLine = 0;
            bool hasUnchangedStartLine = false;

            // Use a StringReader for line-by-line, memory-efficient reading (supports both CRLF and LF)
            using (var reader = new StringReader(fileContent))
            {
                string lineStr;
                while ((lineStr = reader.ReadLine()) != null)
                {
                    // Create a zero-allocation ReadOnlySpan for highly efficient string checks
                    ReadOnlySpan<char> lineSpan = lineStr.AsSpan();

                    // Check for the start of a conflict block (HEAD marker)
                    if (ConflictMarkers.IsMarker(lineSpan, ConflictMarkers.HeadStart))
                    {
                        // If we have collected normal, unchanged code up to this point, save it as a segment
                        if (currentUnchangedLines.Count > 0)
                        {
                            fileSegments.Add(new UnchangedSegment(currentUnchangedLines, unchangedStartLine, currentLineIndex - 1));
                            currentUnchangedLines.Clear();
                            hasUnchangedStartLine = false;
                        }

                        // Extract metadata (branch/commit label) of the head marker
                        int conflictStartLine = currentLineIndex;
                        string headLabel = ConflictMarkers.ExtractLabel(lineSpan);
                        var headLines = new List<string>();
                        var baseLines = new List<string>();
                        var incomingLines = new List<string>();

                        string baseLabel = null;
                        bool hasBase = false;
                        bool separatorFound = false;
                        bool endFound = false;

                        // Advance past the '<<<<<<<' marker line
                        currentLineIndex++;

                        // Step 1: Read head lines (until either diff3 base '|||||||' or separator '=======' is reached)
                        while ((lineStr = reader.ReadLine()) != null)
                        {
                            ReadOnlySpan<char> currentSpan = lineStr.AsSpan();

                            if(ConflictMarkers.IsMarker(currentSpan, ConflictMarkers.BaseStart))
                            {
                                hasBase = true;
                                baseLabel = ConflictMarkers.ExtractLabel(currentSpan);
                                currentLineIndex++;
                                break;
                            }
                            else if(ConflictMarkers.IsMarker(currentSpan, ConflictMarkers.Separator))
                            {
                                separatorFound = true;
                                currentLineIndex++;
                                break;
                            }
                            else if (ConflictMarkers.IsMarker(currentSpan, ConflictMarkers.IncomingEnd))
                            {
                                throw new MalformedConflictException("Unexpected end marker '>>>>>>>' before separator '======='.");
                            }
                            else
                            {
                                headLines.Add(lineStr);
                                currentLineIndex++;
                            }
                        }

                        // Step 2: If a diff3 base marker was found, read base lines until the separator ('=======')
                        if (hasBase)
                        {
                            while ((lineStr = reader.ReadLine()) != null)
                            {
                                ReadOnlySpan<char> currentSpan = lineStr.AsSpan();
                                if (ConflictMarkers.IsMarker(currentSpan, ConflictMarkers.Separator))
                                {
                                    separatorFound = true;
                                    currentLineIndex++;
                                    break;
                                }
                                else if (ConflictMarkers.IsMarker(currentSpan, ConflictMarkers.HeadStart) || ConflictMarkers.IsMarker(currentSpan, ConflictMarkers.IncomingEnd))
                                {
                                    throw new MalformedConflictException("Malformed conflict block: missing '=======' separator.");
                                }
                                else
                                {
                                    baseLines.Add(lineStr);
                                    currentLineIndex++;
                                }
                            }
                        }

                        // Validation: Without a separator, the conflict block is invalid
                        if (!separatorFound)
                        {
                            throw new MalformedConflictException("Malformed conflict block: missing '=======' separator.");
                        }

                        // Step 3: Read incoming lines (the other version) until the end marker ('>>>>>>>')
                        while ((lineStr = reader.ReadLine()) != null)
                        {
                            ReadOnlySpan<char> currentSpan = lineStr.AsSpan();

                            if(ConflictMarkers.IsMarker(currentSpan, ConflictMarkers.IncomingEnd))
                            {
                                string incomingLabel = ConflictMarkers.ExtractLabel(currentSpan);
                                endFound = true;

                                // The conflict block ends on the '>>>>>>>' marker line
                                int conflictEndLine = currentLineIndex;
                                currentLineIndex++;

                                // Add the complete conflict segment block to the list
                                fileSegments.Add(new ConflictSegment(headLabel, headLines, baseLabel, baseLines, incomingLabel, incomingLines, conflictStartLine, conflictEndLine));
                                break;
                            }
                            else if(ConflictMarkers.IsMarker(currentSpan, ConflictMarkers.HeadStart) || ConflictMarkers.IsMarker(currentSpan, ConflictMarkers.Separator))
                            {
                                throw new MalformedConflictException("Malformed conflict block: nested or out-of-order markers.");
                            }
                            else
                            {
                                incomingLines.Add(lineStr);
                                currentLineIndex++;
                            }
                        }
                        // Validation: Check if the block was properly closed
                        if (!endFound)
                        {
                            throw new MalformedConflictException("Unclosed conflict block: missing '>>>>>>>' end marker.");
                        }
                    }
                    else
                    {
                        // Record the first line index of the current unchanged block
                        if (!hasUnchangedStartLine)
                        {
                            unchangedStartLine = currentLineIndex;
                            hasUnchangedStartLine = true;
                        }
                        // Line does not belong to a conflict -> buffer as unchanged code
                        currentUnchangedLines.Add(lineStr);
                        currentLineIndex++;
                    }
                }
            }

            // Add any remaining unchanged text at the end of the file as the final segment
            if (currentUnchangedLines.Count > 0)
            {
                fileSegments.Add(new UnchangedSegment(currentUnchangedLines, unchangedStartLine, currentLineIndex - 1));
            }

            return fileSegments;
        }
    }
}
