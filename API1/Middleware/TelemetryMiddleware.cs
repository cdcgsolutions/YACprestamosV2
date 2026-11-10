using Microsoft.ApplicationInsights.DataContracts;
using System.Text;

namespace API1.Middleware
{
    public class TelemetryMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            var requestBody = await ReadRequestBody(request);

            var requestTelemetry = context.Features.Get<RequestTelemetry>();

            // Add request body to custom properties if telemetry exists
            if (requestTelemetry != null && !string.IsNullOrEmpty(requestBody))
            {
                requestTelemetry.Properties.TryAdd("RequestBody", requestBody);
            }

            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await next(context);

                // Read the response body
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                // Add response body to custom properties
                if (requestTelemetry != null && !string.IsNullOrEmpty(responseBodyText))
                {
                    requestTelemetry.Properties.TryAdd("ResponseBody", responseBodyText);
                }

                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch
            {
                // Rethrow exception to be caught by global exception filter or telemetry
                throw;
            }
        }

        private static async Task<string> ReadRequestBody(HttpRequest request)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }
    }
}
