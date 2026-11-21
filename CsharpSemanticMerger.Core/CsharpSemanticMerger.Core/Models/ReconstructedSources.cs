using System;
using System.Collections.Generic;
using System.Text;

namespace CsharpSemanticMerger.Core.Models
{
    /// <summary>
    /// Represents the two reconstructed in-memory C# source versions derived from a parsed file,
    /// ready to be fed into Roslyn's <c>CSharpSyntaxTree</c> parser.
    /// </summary>
    public sealed record ReconstructedSources
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReconstructedSources"/> record.
        /// </summary>
        /// <param name="headVersion">The full reconstructed source text representing the HEAD side.</param>
        /// <param name="incomingVersion">The full reconstructed source text representing the Incoming side.</param>
        public ReconstructedSources(string headVersion, string incomingVersion)
        {
            HeadVersion = headVersion;
            IncomingVersion = incomingVersion;
        }

        /// <summary>
        /// Gets the complete in-memory C# source string for the HEAD branch side.
        /// </summary>
        public string HeadVersion { get; init; }

        /// <summary>
        /// Gets the complete in-memory C# source string for the Incoming branch side.
        /// </summary>
        public string IncomingVersion { get; init; }
    }
}
