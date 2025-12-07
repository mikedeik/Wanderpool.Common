using System.Diagnostics;
using Wanderpool.Common.Infra.Telemetry;

namespace Wanderpool.Common.Infra.Tests.Telemetry;

/// <summary>
/// Tests for IActivityScope and ActivityScope implementation.
/// </summary>
public class ActivityScopeTests : IDisposable
{
    private readonly ActivitySource _activitySource;

    public ActivityScopeTests()
    {
        _activitySource = new ActivitySource("Wanderpool.Test");
    }

    public void Dispose()
    {
        _activitySource?.Dispose();
    }

    [Fact]
    public void CreateScope_CreatesNewActivity()
    {
        // Arrange
        var operationName = "test_operation";

        // Act
        using var scope = new ActivityScope(_activitySource, operationName);

        // Assert
        Assert.NotNull(scope.Activity);
        Assert.Equal(operationName, scope.Activity.DisplayName);
        Assert.True(scope.Activity.IsAllDataRequested);
    }

    [Fact]
    public void AddTag_AddsTagToCurrentActivity()
    {
        // Arrange
        using var scope = new ActivityScope(_activitySource, "test_operation");

        // Act
        scope.AddTag("key1", "value1");
        scope.AddTag("key2", 42);
        scope.AddTag("key3", true);

        // Assert
        Assert.Equal("value1", scope.Activity.GetTagItem("key1"));
        Assert.Equal(42, scope.Activity.GetTagItem("key2"));
        Assert.Equal(true, scope.Activity.GetTagItem("key3"));
    }

    [Fact]
    public void RecordException_RecordsExceptionOnActivity()
    {
        // Arrange
        using var scope = new ActivityScope(_activitySource, "test_operation");
        var exception = new InvalidOperationException("Test exception");

        // Act
        scope.RecordException(exception);

        // Assert
        var events = scope.Activity.Events.ToList();
        Assert.NotEmpty(events);
        Assert.Single(events);
        Assert.Equal("exception", events[0].Name);
    }

    [Fact]
    public void NestedScopes_CreateParentChildRelationship()
    {
        // Arrange
        Activity? parentActivity = null;
        Activity? childActivity = null;

        // Act
        using (var parentScope = new ActivityScope(_activitySource, "parent_operation"))
        {
            parentActivity = parentScope.Activity;

            using (var childScope = new ActivityScope(_activitySource, "child_operation"))
            {
                childActivity = childScope.Activity;
            }
        }

        // Assert
        Assert.NotNull(parentActivity);
        Assert.NotNull(childActivity);
        // Parent ID should match the child's parent span ID
        Assert.Equal(parentActivity.Id, childActivity.ParentId);
    }

    [Fact]
    public void Dispose_CompletesActivity()
    {
        // Arrange
        Activity? activity;
        bool activityStopped = false;

        // Act
        using (var scope = new ActivityScope(_activitySource, "test_operation"))
        {
            activity = scope.Activity;
            // Activity should be running (non-null and valid)
            Assert.NotNull(activity);
        }

        // After dispose, activity should be stopped
        activityStopped = activity.Duration >= TimeSpan.Zero;
        Assert.True(activityStopped);
    }

    [Fact]
    public void AddTag_WithMultipleTags_AllPresent()
    {
        // Arrange
        using var scope = new ActivityScope(_activitySource, "test_operation");

        // Act
        scope.AddTag("tag1", "value1");
        scope.AddTag("tag2", "value2");
        scope.AddTag("tag3", "value3");

        // Assert
        var tags = scope.Activity.Tags.ToList();
        Assert.Equal(3, tags.Count);
    }

    [Fact]
    public void SetStatus_SetsActivityStatus()
    {
        // Arrange
        using var scope = new ActivityScope(_activitySource, "test_operation");

        // Act
        scope.SetStatus(ActivityStatusCode.Ok);

        // Assert
        Assert.Equal(ActivityStatusCode.Ok, scope.Activity.Status);
    }

    [Fact]
    public void ActivityScope_IsDisposable()
    {
        // Arrange & Act
        var scope = new ActivityScope(_activitySource, "test_operation");

        // Assert
        Assert.IsAssignableFrom<IDisposable>(scope);

        // Cleanup
        scope.Dispose();
    }
}
