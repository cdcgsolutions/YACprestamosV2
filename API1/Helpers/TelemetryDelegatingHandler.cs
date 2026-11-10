using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Text;

namespace API1.Helpers
{
    public class TelemetryDelegatingHandler(TelemetryClient telemetryClient) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestBody = "";
            if (request.Content != null)
            {
                requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            // Track request trace
            var requestTrace = new TraceTelemetry($"Outgoing Request: {request.Method} {request.RequestUri}", SeverityLevel.Information);
            requestTrace.Properties.Add("Method", request.Method.ToString());
            requestTrace.Properties.Add("Uri", request.RequestUri?.ToString());
            requestTrace.Properties.Add("RequestBody", requestBody);

            // Link to parent activity if exists (System.Diagnostics.Activity is handled by AI automatically for correlation)

            telemetryClient.TrackTrace(requestTrace);

            var response = await base.SendAsync(request, cancellationToken);

            var responseBody = "";
            if (response.Content != null)
            {
                responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            }

            // Track response trace
            var responseTrace = new TraceTelemetry($"Outgoing Response: {request.Method} {request.RequestUri} - {response.StatusCode}", SeverityLevel.Information);
            responseTrace.Properties.Add("StatusCode", response.StatusCode.ToString());
            responseTrace.Properties.Add("ResponseBody", responseBody);

            telemetryClient.TrackTrace(responseTrace);

            return response;
        }
    }
}
