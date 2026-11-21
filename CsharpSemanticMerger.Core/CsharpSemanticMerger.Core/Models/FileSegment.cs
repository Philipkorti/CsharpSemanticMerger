using System;
using System.Collections.Generic;
using System.Text;

namespace CsharpSemanticMerger.Core.Models
{
    /// <summary>
    /// Represents the abstract base class for a contiguous segment of a parsed file,
    /// such as an unchanged block of code or a git merge conflict.
    /// </summary>
    /// <remarks>
    /// Stores the physical line boundaries in the raw conflicted file so downstream
    /// components (such as line mappers, Roslyn diagnostics, or UI highlighters)
    /// can locate where each segment originated.
    /// </remarks>
    public abstract class FileSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSegment"/> class with
        /// the specified physical line range in the original file.
        /// </summary>
        /// <param name="startLine">The starting line index of the segment in the original file.</param>
        /// <param name="endLine">The ending line index of the segment in the original file.</param>
        public FileSegment(int startLine, int endLine)
        {
            StartLine = startLine;
            EndLine = endLine;
        }

        /// <summary>
        /// Gets the starting line index of thid segment in the original conflicted file.
        /// </summary>
        public int StartLine { get; init; }

        /// <summary>
        /// Gets the ending line index of this segment in the original conflicted file.
        /// </summary>
        public int EndLine { get; init; }
    }
}
