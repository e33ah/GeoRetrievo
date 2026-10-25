namespace GeoRetrievo.Proxy;

public sealed class CountryApiProxy(HttpClient apiClient, ICloudRunIdentityTokenProvider tokenProvider,
    ILogger<CountryApiProxy> logger) {
    public async Task ForwardAsync(HttpContext context, string path) {
        string token;
        try {
            token = await tokenProvider.GetTokenAsync(context.RequestAborted);
        } catch (HttpRequestException exception) {
            logger.LogError(exception, "Could not obtain a Cloud Run identity token for the API request");
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return;
        } catch (OperationCanceledException) when (!context.RequestAborted.IsCancellationRequested) {
            logger.LogError("Timed out obtaining a Cloud Run identity token for the API request");
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try {
            using var response = await apiClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                context.RequestAborted);

            context.Response.StatusCode = (int)response.StatusCode;
            if (response.Content.Headers.ContentType is { } contentType) {
                context.Response.ContentType = contentType.ToString();
            }

            context.Response.Headers.CacheControl = "no-store";
            if (response.Headers.RetryAfter is { } retryAfter) {
                context.Response.Headers.RetryAfter = retryAfter.ToString();
            }

            await response.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
        } catch (HttpRequestException exception) {
            logger.LogError(exception, "Could not reach the private Cloud Run API");
            if (!context.Response.HasStarted) {
                context.Response.StatusCode = StatusCodes.Status502BadGateway;
            }
        } catch (OperationCanceledException) when (!context.RequestAborted.IsCancellationRequested) {
            logger.LogError("Timed out reaching the private Cloud Run API");
            if (!context.Response.HasStarted) {
                context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
            }
        }
    }
}
