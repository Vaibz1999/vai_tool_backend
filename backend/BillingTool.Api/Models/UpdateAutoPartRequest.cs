namespace BillingTool.Api.Models;

public class UpdateAutoPartRequest
{
    public string Name { get; set; } = string.Empty;
    public string HsnSac { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal TaxPercent { get; set; }
}
