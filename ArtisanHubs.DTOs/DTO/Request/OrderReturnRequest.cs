public class OrderReturnRequest
{
    public int OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string BankAccountName { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
}