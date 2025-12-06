using System.Text.Json;
using Wanderpool.Common.Infra.Clients.Exceptions;

namespace Wanderpool.Common.Infra.Clients;

public abstract class BaseHttpClient
{
    protected readonly HttpClient _httpClient;
    protected readonly ILogger _logger;
    protected readonly JsonSerializerOptions _jsonOptions;


    protected BaseHttpClient(HttpClient httpClientClient, ILogger logger, JsonSerializerOptions jsonOptions)
    {
        _httpClient = httpClientClient;
        _logger = logger;
        _jsonOptions = jsonOptions;
    }


    protected async Task<T?> GetAsync<T>(string uri, CancellationToken ct = default)
    {
        var res = await _httpClient.GetAsync(uri, ct).ConfigureAwait(false);
        await EnsureSuccessOrThrow(res).ConfigureAwait(false);
        return await res.Content.ReadFromJsonAsync<T>(_jsonOptions, ct).ConfigureAwait(false);
    }


    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest payload, CancellationToken ct = default)
    {
        var res = await _httpClient.PostAsJsonAsync(uri, payload, _jsonOptions, ct).ConfigureAwait(false);
        await EnsureSuccessOrThrow(res).ConfigureAwait(false);
        return await res.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, ct).ConfigureAwait(false);
    }


    protected async Task EnsureSuccessOrThrow(HttpResponseMessage res)
    {
        if (res.IsSuccessStatusCode) return;


        var content = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
        _logger.LogWarning("Request to {Url} failed with status {Status}. Content: {Content}", res.RequestMessage?.RequestUri, (int)res.StatusCode, content);


        throw new RemoteServiceException((int)res.StatusCode, content);
    }
}