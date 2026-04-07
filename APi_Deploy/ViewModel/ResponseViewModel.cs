using Domain.Enums;

namespace APi_Presentation.ViewModel
{
    public class ResponseViewModel<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public AppErrorCode? ErrorCode { get; set; }
        public string? TraceId { get; set; }
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
        public DateTime Timestamp { get; set; }

        private ResponseViewModel()
        {
            Timestamp = DateTime.UtcNow;
        }

        public static ResponseViewModel<T> Success(T data, string message = "Operation completed successfully")
        {
            return new ResponseViewModel<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ResponseViewModel<T> Error(
            string message,
            AppErrorCode errorCode,
            string? traceId = null)
        {
            return new ResponseViewModel<T>
            {
                IsSuccess = false,
                Message = message,
                ErrorCode = errorCode,
                TraceId = traceId,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ResponseViewModel<T> ValidationError(
            string message,
            Dictionary<string, string[]> validationErrors,
            string? traceId = null)
        {
            return new ResponseViewModel<T>
            {
                IsSuccess = false,
                Message = message,
                ErrorCode = AppErrorCode.ValidationError,
                ValidationErrors = validationErrors,
                TraceId = traceId,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
