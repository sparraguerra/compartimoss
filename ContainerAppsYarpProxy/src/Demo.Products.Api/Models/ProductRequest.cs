namespace Demo.Products.Api.Models;

public record ProductRequest(string Name, string Surname, string Email, DateTime? DateOfBirth);