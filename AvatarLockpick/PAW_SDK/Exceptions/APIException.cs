namespace PAW_SDK.Exceptions
{
    public class APIException : Exception
    {
        public int StatusCode { get; }

        public APIException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public APIException(int statusCode, string message, Exception inner) : base(message, inner)
        {
            StatusCode = statusCode;
        }
    }
}