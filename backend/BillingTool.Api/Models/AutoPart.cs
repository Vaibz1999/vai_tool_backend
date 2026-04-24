namespace BillingTool.Api.Models;

public record AutoPart(
    int Id,
    string Name,
    string HsnSac,
    decimal DefaultRate,
    decimal DefaultTaxPercent);
