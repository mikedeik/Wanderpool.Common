namespace Wanderpool.Common.Infra.Clients.Exceptions;

public class RemoteServiceException : System.Exception
{
    public int StatusCode { get; }
    public string? RemoteContent { get; }
    public RemoteServiceException(int status, string? content) : base($"Remote call failed with {status}")
    {
        StatusCode = status;
        RemoteContent = content;
    }
}