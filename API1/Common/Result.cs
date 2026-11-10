using System.Text.Json.Serialization;

namespace API1.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; } = 200;
        public string? Message { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Value { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Errors { get; set; }

        public static Result<T> Success(T value, string? message = null) =>
            new() { IsSuccess = true, StatusCode = 200, Value = value, Message = message };

        public static Result<T> Failure(string message, int statusCode = 400, List<string>? errors = null) =>
            new() { IsSuccess = false, StatusCode = statusCode, Message = message, Errors = errors };
    }

    public static class Result
    {
        public static Result<T> Ok<T>(T value, string? message = null) => Result<T>.Success(value, message);
        public static Result<object> Ok(string? message = "Operación realizada con éxito.") => Result<object>.Success(new { }, message);

        public static Result<T> Fail<T>(string message, int statusCode = 400, List<string>? errors = null) => Result<T>.Failure(message, statusCode, errors);
        public static Result<object> Fail(string message, int statusCode = 400, List<string>? errors = null) => Result<object>.Failure(message, statusCode, errors);

        public static Result<T> Fail400<T>(string message, List<string>? errors = null) => Result<T>.Failure(message, 400, errors);
        public static Result<T> Fail401<T>(string message = "No autorizado.") => Result<T>.Failure(message, 401);
        public static Result<T> Fail403<T>(string message = "Acceso prohibido.") => Result<T>.Failure(message, 403);
        public static Result<T> Fail404<T>(string message = "Recurso no encontrado.") => Result<T>.Failure(message, 404);
        public static Result<T> Fail500<T>(string message = "Error interno del servidor.", List<string>? errors = null) => Result<T>.Failure(message, 500, errors);
    }
}
