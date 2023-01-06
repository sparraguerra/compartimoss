namespace Demo.Products.Api.Models;

public record ProductResponse(int Id, string Name, string Surname, string Email, DateTime? DateOfBirth);