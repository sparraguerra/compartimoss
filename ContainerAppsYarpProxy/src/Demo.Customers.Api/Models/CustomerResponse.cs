namespace Demo.Customers.Api.Models;

public record CustomerResponse(int Id, string Name, string Surname, string Email, DateTime? DateOfBirth);