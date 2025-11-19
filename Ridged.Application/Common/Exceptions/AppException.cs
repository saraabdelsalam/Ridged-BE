namespace Ridged.Application.Common.Exceptions
{
    public class AppException : Exception
    {
        public int StatusCode { get; }
        public Dictionary<string, string[]>? ValidationErrors { get; }

        public AppException(string message, int statusCode = 500) 
            : base(message)
        {
            StatusCode = statusCode;
        }

        public AppException(string message, int statusCode, Dictionary<string, string[]> validationErrors) 
            : base(message)
        {
            StatusCode = statusCode;
            ValidationErrors = validationErrors;
        }
    }
}
