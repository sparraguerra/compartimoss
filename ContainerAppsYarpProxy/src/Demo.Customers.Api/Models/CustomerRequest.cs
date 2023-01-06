namespace Demo.Customers.Api.Models;

public record CustomerRequest(string Name, string Surname, string Email, DateTime? DateOfBirth);