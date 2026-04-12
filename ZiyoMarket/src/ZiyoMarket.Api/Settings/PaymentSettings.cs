namespace ZiyoMarket.Api.Settings;

public class PaymentSettings
{
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolder { get; set; } = string.Empty;
    public string BankName   { get; set; } = string.Empty;
    public string Note       { get; set; } = string.Empty;
}
