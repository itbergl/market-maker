namespace MarketMaker.Models;

public class Trade
{
    public DateTime TimeStamp;
    public Guid ArgressorOrderId;
    public Guid PassiveOrderId;
    public int Quantity;
    public int Price;
    public string Symbol;
}

public record Order
{
    public DateTime TimeStamp;
    public string Symbol;
    public Guid Id;
    public string User;
    public int Quantity;
    public int Price;
}
