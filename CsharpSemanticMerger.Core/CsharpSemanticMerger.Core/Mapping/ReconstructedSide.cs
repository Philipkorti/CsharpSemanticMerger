using System;
using System.Collections.Generic;
using System.Text;

namespace CsharpSemanticMerger.Core.Mapping
{
    /// <summary>
    /// Represents a reconstructed side (such as HEAD or Incoming) of a conflicted file,
    /// including its generated content and line mapping back to the original file.
    /// </summary>
    public sealed record ReconstructedSide
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReconstructedSide"/> record.
        /// </summary>
        /// <param name="content">The reconstructed file content with conflict markers stripped.</param>
        /// <param name="lineMapping">The mapping from virtual line indices to original physical line indices.</param>
        public ReconstructedSide(string content, LineMapping lineMapping)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            LineMapping = lineMapping ?? throw new ArgumentNullException(nameof(lineMapping));
        }

        /// <summary>
        /// Gets the reconstructed file content with conflict markers stripped.
        /// </summary>
        public string Content { get; init; }

        /// <summary>
        /// Gets the line mapping translating virtual line indices back to original physical line indices.
        /// </summary>
        public LineMapping LineMapping { get; init; }
    }
}
