namespace BodyStack.Server.Domain.Exceptions;

public class IntegrationApiException : DomainException
{
    public string IntegrationName { get; }
    public int? StatusCode { get; }
    public string? ResponseBody { get; }

    public IntegrationApiException(string integrationName, string message, int? statusCode = null, string? responseBody = null)
        : base("INTEGRATION_API_ERROR", message)
    {
        IntegrationName = integrationName;
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public IntegrationApiException(string integrationName, string message, Exception innerException, int? statusCode = null)
        : base("INTEGRATION_API_ERROR", message, innerException)
    {
        IntegrationName = integrationName;
        StatusCode = statusCode;
    }
}
