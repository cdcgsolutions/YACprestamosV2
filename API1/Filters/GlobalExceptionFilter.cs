using API1.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API1.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Error no controlado: {Message}", context.Exception.Message);

            var errors = _env.IsDevelopment()
                ? new List<string> { context.Exception.Message, context.Exception.StackTrace ?? string.Empty }
                : new List<string> { context.Exception.Message };

            var result = Result.Fail500<object>("Ha ocurrido un error inesperado en el servidor.", errors);

            context.Result = new ObjectResult(result)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

            context.ExceptionHandled = true;
        }
    }
}
