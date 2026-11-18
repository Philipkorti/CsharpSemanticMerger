using System;
using System.Collections.Generic;
using System.Text;

namespace CsharpSemanticMerger.Core.Models
{
    /// <summary>
    /// Represents a segment of a file that has not changed between the two versions being merged. This segment contains the unchanged lines of code.
    /// </summary>
    public sealed class UnchangedSegment : FileSegment
    {
        /// <summary>
        /// Gets the unchanged lines belonging to this segment, preserving their original indentation.
        /// </summary>
        public IReadOnlyList<string> Lines { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnchangedSegment"/> class.
        /// </summary>
        /// <param name="lines">The collection of unchanged lines within this segment.</param>
        public UnchangedSegment(IReadOnlyList<string> lines)
        {
            Lines = lines;
        }
    }
}
