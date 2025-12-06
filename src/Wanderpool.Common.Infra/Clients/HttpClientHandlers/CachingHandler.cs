using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;

namespace Wanderpool.Common.Infra.Clients.HttpClientHandlers;

public class CachingHandler : DelegatingHandler
{
    private readonly IDistributedCache _cache;
    private readonly System.TimeSpan _ttl;


    public CachingHandler(IDistributedCache cache, System.TimeSpan ttl)
    {
        _cache = cache;
        _ttl = ttl;
    }


    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method == HttpMethod.Get && request.RequestUri != null)
        {
            var key = GenerateCacheKey(request.RequestUri.ToString());
            var cached = await _cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
            if (cached != null)
            {
                var content = Encoding.UTF8.GetString(cached);
                var msg = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(content, Encoding.UTF8, "application/json"),
                    RequestMessage = request
                };
                return msg;
            }


            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                await _cache.SetAsync(key, Encoding.UTF8.GetBytes(body), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _ttl }, cancellationToken).ConfigureAwait(false);
            }
            return response;
        }


        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }


    private static string GenerateCacheKey(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return System.Convert.ToBase64String(bytes);
    }
}