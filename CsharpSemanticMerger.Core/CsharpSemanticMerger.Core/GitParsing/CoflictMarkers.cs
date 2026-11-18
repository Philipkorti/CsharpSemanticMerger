using System;
using System.Collections.Generic;
using System.Text;

namespace CsharpSemanticMerger.Core.GitParsing
{
    /// <summary>
    /// Defines standard Git and diff3 merge conflict marker constants and provides
    /// zero-allocation helper methods for identifying markers and extracting metadata labels.
    /// </summary>
    public static class ConflictMarkers
    {
        #region Constants

        /// <summary>
        /// The standard 7-character Length of a Git merge conflict marker.
        /// </summary>
        public const int MarkerLength = 7;

        /// <summary>
        /// Marks the beginning of a merge conflict and the start of the current (HEAD) section.
        /// </summary>
        public const string HeadStart = "<<<<<<<";

        /// <summary>
        /// Marks the start of the common ancestor (base) section in a 3-way (diff3) merge conflict.
        /// </summary>
        public const string BaseStart = "|||||||";

        /// <summary>
        /// The marker separating the conflicting changes in a merge conflict.
        /// </summary>
        public const string Separator = "=======";

        /// <summary>
        /// Marks the end of a merge conflict and closes the incoming section.
        /// </summary>
        public const string IncomingEnd = ">>>>>>>";

        #endregion

        #region Helper Methods

        /// <summary>
        /// Determines whether the specified line starts with an exact 7-character conflict marker
        /// followed by either the end of the line or whitespace.
        /// </summary>
        /// <param name="line">The line span to inspect (without trailing newline characters)</param>
        /// <param name="marker">The 7-character marker prefix to match against. </param>
        /// <returns><see langword="true"/> if the line is a valid conflict marker; otherwise, <see langword="false"/>.</returns>
        public static bool IsMarker(ReadOnlySpan<char> line, string marker)
        {
            if(!line.StartsWith(marker, StringComparison.Ordinal))
                return false;

            return line.Length == MarkerLength || char.IsWhiteSpace(line[MarkerLength]);
        }

        /// <summary>
        /// Extracts and trims the branch, tag, or commit label following a 7-character conflict marker.
        /// </summary>
        /// <param name="line">The marker line span from which to extract the label.</param>
        /// <returns>The trimmed metadata label, or <see cref="string.Empty"/> if no label is present.</returns>
        public static string ExtractLabel(ReadOnlySpan<char> line)
        {
            if(line.Length <= MarkerLength)
                return string.Empty;

            return line.Slice(MarkerLength).Trim().ToString();
        }

        #endregion
    }
}