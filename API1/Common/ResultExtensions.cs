using Microsoft.AspNetCore.Mvc;

namespace API1.Common
{
    public static class ResultExtensions
    {
        public static IActionResult ToResult<T>(this Result<T> result)
        {
            return result.StatusCode switch
            {
                200 => new OkObjectResult(result),
                201 => new ObjectResult(result) { StatusCode = 201 },
                204 => new NoContentResult(),
                400 => new BadRequestObjectResult(result),
                401 => new UnauthorizedObjectResult(result),
                403 => new ObjectResult(result) { StatusCode = 403 },
                404 => new NotFoundObjectResult(result),
                _ => new ObjectResult(result) { StatusCode = result.StatusCode }
            };
        }

        public static IActionResult ToResult(this Task<Result<object>> taskResult)
        {
            return taskResult.Result.ToResult();
        }
    }
}
