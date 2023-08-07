namespace DaprWorkflows.Models;

public record Notification(string Message);  
public record PaymentRequest(string CustomerId, string Description, decimal Amount, string CurrencyName);
public record PaymentResult(bool Processed); 
public enum ApprovalResult
{
    Unspecified = 0,
    Approved = 1,
    Rejected = 2,
}