namespace MoviesApi.Exceptions
{
	/// <summary>
	/// This is a custom exception class that provides detailed information about exceptional situations.
	/// </summary>
	public class CustomException : Exception
	{
		/// <summary>
		/// Gets the error code associated with the exception.
		/// </summary>
		public string ErrorCode { get; }

		/// <summary>
		/// Initializes a new instance of the CustomException class.
		/// </summary>
		public CustomException() : base()
		{
		}

		/// <summary>
		/// Initializes a new instance of the CustomException class with a specified message.
		/// </summary>
		/// <param name="message">The error message.</param>
		public CustomException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the CustomException class with a specified message and error code.
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <param name="errorCode">The error code associated with the exception.</param>
		public CustomException(string message, string errorCode) : base(message)
		{
			ErrorCode = errorCode;
		}

		/// <summary>
		/// Initializes a new instance of the CustomException class with a specified message and inner exception.
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <param name="innerException">The inner exception that caused this exception.</param>
		public CustomException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
