namespace ArtisanHubs.API.DTOs.Common
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Success", int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> FailResponse(string message = "Failed", int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                StatusCode = statusCode,
                Message = message,
                Data = default
            };
        }
    }
}