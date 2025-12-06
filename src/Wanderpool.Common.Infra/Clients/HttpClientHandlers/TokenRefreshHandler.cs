namespace Wanderpool.Common.Infra.Clients.HttpClientHandlers;

public class TokenRefreshHandler : DelegatingHandler
{
    private readonly ITokenProvider _tokenProvider;
    private readonly ILogger<TokenRefreshHandler> _logger;


    public TokenRefreshHandler(ITokenProvider tokenProvider, ILogger<TokenRefreshHandler> logger)
    {
        _tokenProvider = tokenProvider;
        _logger = logger;
    }


    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetAccessTokenAsync().ConfigureAwait(false);
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);


        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);


        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogInformation("401 from upstream, trying refresh token");
            var refreshed = await _tokenProvider.RefreshTokenAsync().ConfigureAwait(false);
            if (refreshed)
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _tokenProvider.GetAccessTokenAsync().ConfigureAwait(false));
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
        }


        return response;
    }
}