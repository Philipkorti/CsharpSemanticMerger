using System;
using System.Collections.Generic;
using System.Text;

namespace CsharpSemanticMerger.Core.Models
{
    /// <summary>
    /// Represents a Git merge conflict block containing HEAD, optional diff3 base, and incoming code segments.
    /// </summary>
    public sealed class ConflictSegment : FileSegment
    {
        /// <summary>
        /// Gets the branch or commit label extracted from the opening conflict marker (e.g., <c>&lt;&lt;&lt;&lt;&lt;&lt;&lt; HEAD</c>).
        /// </summary>
        public string HeadLabel { get; init; }

        /// <summary>
        /// Gets or sets the header lines.
        /// </summary>
        public IReadOnlyList<string> HeadLines { get; init; }

        /// <summary>
        /// Gets the branch or commit label extracted from the optional diff3 base marker (e.g., <c>||||||| base</c>), if present.
        /// </summary>
        public string BaseLabel { get; init; }

        /// <summary>
        /// Gets the optional original source code lines from the common ancestor in a 3-way (diff3) conflict.
        /// </summary>
        public IReadOnlyList<string> BaseLines { get; init; }

        /// <summary>
        /// Gets the branch or commit label extracted from the closing conflict marker (e.g., <c>&gt;&gt;&gt;&gt;&gt;&gt;&gt; feature/login</c>)
        /// </summary>
        public string IncomingLabel { get; init; }

        /// <summary>
        /// Gets the lines from the incoming change.
        /// </summary>
        public IReadOnlyList<string> IncomingLines { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictSegment"/> class with labels and lines for the head,
        /// base, and incoming versions.
        /// </summary>
        /// <param name="headLabel">The label for the head version.</param>
        /// <param name="headLines">The lines from the head version.</param>
        /// <param name="baseLabel">The label for the base version.</param>
        /// <param name="baseLines">The lines from the base version.</param>
        /// <param name="incomingLabel">The label for the incoming version.</param>
        /// <param name="incomingLines">The lines from the incoming version.</param>
        public ConflictSegment(string headLabel, IReadOnlyList<string> headLines, string baseLabel, IReadOnlyList<string> baseLines, string incomingLabel, IReadOnlyList<string> incomingLines)
        {
            HeadLabel = headLabel;
            HeadLines = headLines;
            BaseLabel = baseLabel;
            BaseLines = baseLines;
            IncomingLabel = incomingLabel;
            IncomingLines = incomingLines;
        }
    }
}
