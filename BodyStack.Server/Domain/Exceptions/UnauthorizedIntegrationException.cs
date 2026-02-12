namespace BodyStack.Server.Domain.Exceptions;

public class UnauthorizedIntegrationException : DomainException
{
    public string IntegrationName { get; }
    public int? StatusCode { get; }

    public UnauthorizedIntegrationException(string integrationName, int? statusCode = null)
        : base("UNAUTHORIZED_INTEGRATION", 
               $"Unauthorized access to {integrationName} integration. Status code: {statusCode}")
    {
        IntegrationName = integrationName;
        StatusCode = statusCode;
    }
}
