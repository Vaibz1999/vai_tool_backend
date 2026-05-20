namespace BillingTool.Api.Models;

public record Customer(
    int Id,
    string Name,
    string Address,
    string Gstin);
