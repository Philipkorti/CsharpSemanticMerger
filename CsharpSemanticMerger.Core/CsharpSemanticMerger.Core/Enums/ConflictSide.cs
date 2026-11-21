using System;
using System.Collections.Generic;
using System.Text;

namespace CsharpSemanticMerger.Core.Enums
{
    /// <summary>
    /// Identifies which side of a Git merge conflict is bgeing reconstructed or queried.
    /// </summary>
    public enum ConflictSide
    {
        /// <summary>
        /// Represents the current branch side (<c>HEAD</c>).
        /// </summary>
        Head,
        /// <summary>
        /// Represents the incoming or merged branch side.
        /// </summary>
        Incoming
    }
}
