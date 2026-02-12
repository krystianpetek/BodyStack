using BodyStack.Server.Domain.Exceptions;
using Xunit;

namespace BodyStack.Server.Tests.Domain.Exceptions;

public class DomainExceptionTests
{
    [Fact]
    public void DomainException_Should_Set_ErrorCode()
    {
        // Arrange & Act - using concrete implementation for testing
        var exception = new TestDomainException("TEST_CODE", "Test message");

        // Assert
        Assert.Equal("TEST_CODE", exception.ErrorCode);
        Assert.Equal("Test message", exception.Message);
    }

    [Fact]
    public void DomainException_With_InnerException_Should_Set_ErrorCode_And_Inner()
    {
        // Arrange
        var inner = new InvalidOperationException("Inner error");

        // Act
        var exception = new TestDomainException("TEST_CODE", "Test message", inner);

        // Assert
        Assert.Equal("TEST_CODE", exception.ErrorCode);
        Assert.Equal("Test message", exception.Message);
        Assert.Same(inner, exception.InnerException);
    }

    [Fact]
    public void FitatuSessionNotFoundException_Should_Have_Correct_ErrorCode()
    {
        // Arrange & Act
        var exception = new FitatuSessionNotFoundException("user123");

        // Assert
        Assert.Equal("FITATU_SESSION_NOT_FOUND", exception.ErrorCode);
        Assert.Equal("user123", exception.FitatuUserId);
        Assert.Contains("user123", exception.Message);
    }

    [Fact]
    public void FitatuSessionNotFoundException_With_Null_UserId_Should_Work()
    {
        // Arrange & Act
        var exception = new FitatuSessionNotFoundException(null);

        // Assert
        Assert.Equal("FITATU_SESSION_NOT_FOUND", exception.ErrorCode);
        Assert.Null(exception.FitatuUserId);
        Assert.Contains("unknown", exception.Message);
    }

    [Fact]
    public void FitatuSessionNotFoundException_Should_Inherit_From_DomainException()
    {
        // Arrange & Act
        var exception = new FitatuSessionNotFoundException("user123");

        // Assert
        Assert.IsAssignableFrom<DomainException>(exception);
    }

    [Fact]
    public void MonthExportIncompleteException_Should_Have_Correct_ErrorCode_And_Properties()
    {
        // Arrange
        var missingDays = new[] { "2024-01-05", "2024-01-10" };

        // Act
        var exception = new MonthExportIncompleteException(2024, 1, missingDays);

        // Assert
        Assert.Equal("MONTH_EXPORT_INCOMPLETE", exception.ErrorCode);
        Assert.Equal(2024, exception.Year);
        Assert.Equal(1, exception.Month);
        Assert.Equal(missingDays, exception.MissingDays);
        Assert.Contains("2024-01-05", exception.Message);
        Assert.Contains("2024-01-10", exception.Message);
    }

    [Fact]
    public void MonthExportIncompleteException_Should_Have_ReadOnly_MissingDays()
    {
        // Arrange
        var missingDays = new List<string> { "2024-01-05" };
        var exception = new MonthExportIncompleteException(2024, 1, missingDays);

        // Act & Assert
        Assert.IsAssignableFrom<IReadOnlyList<string>>(exception.MissingDays);
    }

    [Fact]
    public void MonthExportIncompleteException_Should_Inherit_From_DomainException()
    {
        // Arrange & Act
        var exception = new MonthExportIncompleteException(2024, 1, Array.Empty<string>());

        // Assert
        Assert.IsAssignableFrom<DomainException>(exception);
    }

    [Fact]
    public void UnauthorizedIntegrationException_Should_Have_Correct_ErrorCode()
    {
        // Arrange & Act
        var exception = new UnauthorizedIntegrationException("Fitatu", 401);

        // Assert
        Assert.Equal("UNAUTHORIZED_INTEGRATION", exception.ErrorCode);
        Assert.Equal("Fitatu", exception.IntegrationName);
        Assert.Equal(401, exception.StatusCode);
    }

    [Fact]
    public void IntegrationApiException_Should_Have_Correct_ErrorCode_And_Properties()
    {
        // Arrange & Act
        var exception = new IntegrationApiException("Fitatu", "API call failed", 500, "error body");

        // Assert
        Assert.Equal("INTEGRATION_API_ERROR", exception.ErrorCode);
        Assert.Equal("Fitatu", exception.IntegrationName);
        Assert.Equal(500, exception.StatusCode);
        Assert.Equal("error body", exception.ResponseBody);
    }

    [Fact]
    public void IntegrationApiException_With_InnerException_Should_Work()
    {
        // Arrange
        var inner = new HttpRequestException("Connection failed");

        // Act
        var exception = new IntegrationApiException("Fitatu", "API call failed", inner, 503);

        // Assert
        Assert.Equal("INTEGRATION_API_ERROR", exception.ErrorCode);
        Assert.Same(inner, exception.InnerException);
        Assert.Equal(503, exception.StatusCode);
    }

    // Helper class for testing abstract DomainException
    private class TestDomainException : DomainException
    {
        public TestDomainException(string errorCode, string message)
            : base(errorCode, message) { }

        public TestDomainException(string errorCode, string message, Exception inner)
            : base(errorCode, message, inner) { }
    }
}
