using API1.Common;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API1.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAuthorizationFilter
    {
        private const string ApiKeyHeaderName = "X-Api-Key";

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();

            if (configuration == null)
            {
                context.Result = Result.Fail500<object>("Error de configuración del servidor.").ToResult();
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialApiKey))
            {
                context.Result = Result.Fail401<object>("API Key no proporcionada. Envíe el encabezado X-Api-Key.").ToResult();
                return;
            }

            var apiKeyFromConfig = configuration["App:ApiKey"];

            if (string.IsNullOrEmpty(apiKeyFromConfig) || !apiKeyFromConfig.Equals(potentialApiKey.ToString()))
            {
                context.Result = Result.Fail401<object>("API Key inválida o no autorizada.").ToResult();
                return;
            }
        }
    }
}
