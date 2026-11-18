using System;
using System.Collections.Generic;
using System.Text;

namespace CsharpSemanticMerger.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a Git merge conflict block is syntactically invalid,
    /// such as when conflict markers are uncloses, nested, or appear out of order.
    /// </summary>
    public class MalformedConflictException : Exception
    {
        /// <summary>
        /// Gets the 1-based line number where the malformed conflict marker was encountered,
        /// or <see cref="null"/> if the error occurred at the end of the file (e.g., an unclosed conflict).
        /// </summary>
        public int? LineNumber { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MalformedConflictException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MalformedConflictException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new unstance of the <see cref="MalformedConflictException"/> class with a specified error message
        /// and the line number where the error occurred.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="lineNumber">The 1-based line number associated with the malformed marker.</param>
        public MalformedConflictException(string message, int? lineNumber) : base($"Line {lineNumber}: {message}")
        {
            LineNumber = lineNumber;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MalformedConflictException"/> class with a specified error
        /// message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that caused the current exception.</param>
        public MalformedConflictException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
