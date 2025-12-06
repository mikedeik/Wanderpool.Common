namespace Wanderpool.Common.Infra.Clients.HttpClientHandlers;

public interface ITokenProvider
{
    Task<string?> GetAccessTokenAsync();
    Task<bool> RefreshTokenAsync();
}