using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Wanderpool.Common.Infra.Telemetry;

namespace Wanderpool.Common.Infra.Tests.Telemetry;

public class TracingEnricherTests
{
    [Fact]
    public void EnrichWithCorrelationId_WithValidCorrelationId_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();
        var httpContext = new DefaultHttpContext();
        var correlationId = Guid.NewGuid().ToString();
        httpContext.Items["CorrelationId"] = correlationId;

        // Act & Assert
        activity.EnrichWithCorrelationId(httpContext); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithCorrelationId_WithNullHttpContext_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();

        // Act & Assert
        activity.EnrichWithCorrelationId(null); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithCorrelationId_WithNullActivity_DoesNotThrow()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Items["CorrelationId"] = "test-id";

        // Act & Assert
        ((Activity?)null).EnrichWithCorrelationId(httpContext); // Should not throw
    }

    [Fact]
    public void EnrichWithRequestBodySize_WithContentLength_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();
        var httpRequest = new DefaultHttpContext().Request;
        httpRequest.ContentLength = 1024;

        // Act & Assert
        activity.EnrichWithRequestBodySize(httpRequest); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithRequestBodySize_WithoutContentLength_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();
        var httpRequest = new DefaultHttpContext().Request;

        // Act & Assert
        activity.EnrichWithRequestBodySize(httpRequest); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithResponseBodySize_WithContentLength_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();
        var httpResponse = new DefaultHttpContext().Response;
        httpResponse.ContentLength = 2048;

        // Act & Assert
        activity.EnrichWithResponseBodySize(httpResponse); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithResponseBodySize_WithoutContentLength_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();
        var httpResponse = new DefaultHttpContext().Response;

        // Act & Assert
        activity.EnrichWithResponseBodySize(httpResponse); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithClientIp_WithValidClientIp_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.100");
        var httpRequest = httpContext.Request;

        // Act & Assert
        activity.EnrichWithClientIp(httpRequest); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithUserAgent_WithUserAgentHeader_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["User-Agent"] = "Mozilla/5.0";
        var httpRequest = httpContext.Request;

        // Act & Assert
        activity.EnrichWithUserAgent(httpRequest); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithUserAgent_WithoutUserAgentHeader_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();
        var httpRequest = new DefaultHttpContext().Request;

        // Act & Assert
        activity.EnrichWithUserAgent(httpRequest); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithRequestPath_WithPath_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/users";
        var httpRequest = httpContext.Request;

        // Act & Assert
        activity.EnrichWithRequestPath(httpRequest); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithRequestPath_WithQueryString_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?id=123&name=test");
        var httpRequest = httpContext.Request;

        // Act & Assert
        activity.EnrichWithRequestPath(httpRequest); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithEnvironmentInfo_WithEnvironmentName_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();

        // Act & Assert
        activity.EnrichWithEnvironmentInfo("Production"); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithEnvironmentInfo_WithoutEnvironmentName_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();

        // Act & Assert
        activity.EnrichWithEnvironmentInfo(); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithExceptionDetails_WithException_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();
        var exception = new ArgumentException("Test error message");

        // Act & Assert
        activity.EnrichWithExceptionDetails(exception); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithExceptionDetails_WithInnerException_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();
        var innerException = new InvalidOperationException("Inner error");
        var exception = new ApplicationException("Outer error", innerException);

        // Act & Assert
        activity.EnrichWithExceptionDetails(exception); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithExceptionDetails_WithNullException_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();

        // Act & Assert
        activity.EnrichWithExceptionDetails(null); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithContentType_WithContentType_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.ContentType = "application/json";
        var httpRequest = httpContext.Request;

        // Act & Assert
        activity.EnrichWithContentType(httpRequest); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithContentType_WithoutContentType_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();
        var httpRequest = new DefaultHttpContext().Request;

        // Act & Assert
        activity.EnrichWithContentType(httpRequest); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithResponseContentType_WithContentType_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();
        var httpContext = new DefaultHttpContext();
        httpContext.Response.ContentType = "application/json; charset=utf-8";
        var httpResponse = httpContext.Response;

        // Act & Assert
        activity.EnrichWithResponseContentType(httpResponse); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void EnrichWithResponseContentType_WithoutContentType_DoesNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();
        var httpResponse = new DefaultHttpContext().Response;

        // Act & Assert
        activity.EnrichWithResponseContentType(httpResponse); // Should not throw

        activity.Stop();
    }

    [Fact]
    public void MultipleEnrichmentCalls_AllInvoked_DoNotThrow()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/test";
        httpContext.Request.ContentType = "application/json";
        httpContext.Request.ContentLength = 256;
        httpContext.Request.Headers["User-Agent"] = "TestAgent/1.0";
        httpContext.Response.ContentLength = 512;

        // Act & Assert - Call multiple enrichments
        activity.EnrichWithRequestPath(httpContext.Request);
        activity.EnrichWithContentType(httpContext.Request);
        activity.EnrichWithRequestBodySize(httpContext.Request);
        activity.EnrichWithUserAgent(httpContext.Request);
        activity.EnrichWithResponseBodySize(httpContext.Response);
        activity.EnrichWithEnvironmentInfo("Test");

        activity.Stop();
    }
}
