// ReSharper disable once CheckNamespace

using System;

namespace Telegram.Bot.Exceptions
{
    /// <summary>
    /// The exception that is thrown when the user does not exist
    /// </summary>
    [Obsolete("Custom exceptions will be removed in the next major update")]
    public class UserNotFoundException : BadRequestException
    {
        /// <summary>
        /// Initializes a new object of the <see cref="UserNotFoundException"/> class
        /// </summary>
        /// <param name="message">The error message of this exception.</param>
        public UserNotFoundException(string message)
            : base(message)
        {
        }
    }
}
