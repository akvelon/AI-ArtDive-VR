namespace DeepArtConsoleClient.Errors
{
    internal class DeepArtConvertException : ApplicationException
    {
        public DeepArtConvertException(
            AppErrorCode code,
            string message,
            Exception? innerException = null
        ) : base(message, innerException)
        {
            ErrorCode = code;
        }

        public AppErrorCode ErrorCode { get; set; }
    }
}
