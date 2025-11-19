namespace Ridged.Domain.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string Message { get; }
        public int StatusCode { get; }

        protected Result(bool isSuccess, string message, int statusCode = 200)
        {
            IsSuccess = isSuccess;
            Message = message;
            StatusCode = statusCode;
        }

        public static Result Success(string message = "Operation completed successfully") 
            => new(true, message, 200);

        public static Result Failure(string message, int statusCode = 400) 
            => new(false, message, statusCode);

        public static Result<T> Success<T>(T data, string message = "Operation completed successfully") 
            => new(true, data, message, 200);

        public static Result<T> Failure<T>(string message, int statusCode = 400) 
            => new(false, default, message, statusCode);
    }

    public class Result<T> : Result
    {
        public T? Data { get; }

        internal Result(bool isSuccess, T? data, string message, int statusCode = 200)
            : base(isSuccess, message, statusCode)
        {
            Data = data;
        }
    }
}
