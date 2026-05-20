namespace BillingTool.Api.Models;

public class CreateCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Gstin { get; set; } = string.Empty;
}
