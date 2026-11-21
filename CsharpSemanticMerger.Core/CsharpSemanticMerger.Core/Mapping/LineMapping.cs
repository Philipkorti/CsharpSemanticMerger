using System;
using System.Collections.Generic;
using System.Text;

namespace CsharpSemanticMerger.Core.Mapping
{
    /// <summary>
    /// Represents a mapping between source code lines.
    /// </summary>
    public class LineMapping
    {
        private readonly IReadOnlyList<int> virtualToOriginalLines;

        /// <summary>
        /// Initializes a new instance of the <see cref="LineMapping"/> class.
        /// </summary>
        /// <param name="virtualToOriginalLines">
        /// A list where index represents the 0-basevirtual line number and value
        /// ´represents the 0-base physical line number in the original file.
        /// </param>
        public LineMapping(IReadOnlyList<int> virtualToOriginalLines)
        {
            this.virtualToOriginalLines = virtualToOriginalLines ?? throw new ArgumentNullException(nameof(virtualToOriginalLines));
        }

        /// <summary>
        /// Gets the total number of mapped virtual lines.
        /// </summary>
        public int LineCount => virtualToOriginalLines.Count;

        /// <summary>
        /// Maps a 0-based virtual line index to ots corresponding 0-based physical
        /// in the original conflicted file.
        /// </summary>
        /// <param name="virtualLine">The 0-based line index in the reconstructed content.</param>
        /// <returns>The 0-based physical line index in the original file.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Throw when <paramref name="virtualLine"/> is negative or outside the mapped range.
        /// </exception>
        public int ToOriginalLine(int virtualLine)
        {
            if(virtualLine < 0 || virtualLine >= virtualToOriginalLines.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(virtualLine),
                    $"Virtual line index {virtualLine} is out of range [0, {virtualToOriginalLines.Count - 1}].");
            }

            return virtualToOriginalLines[virtualLine];
        }
    }
}
