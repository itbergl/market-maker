
namespace MarketMaker.Models;

public record RejectResponse
{
    public string User;
    public string ClientReference;
    public string Message;
}

public record MarketEventResponse
{
    public string ClientReference;
    public string User;
    public MarketEvent MarketEvent;
}

public abstract record MarketEvent
{
    public string User;
    public Guid Id;
    public DateTime TimeStamp;
}

public record NewOrderMarketEvent : MarketEvent
{
    public int Price;
    public int Quantity;
    public string Symbol;

    public bool Filled; // get rid == Quanity == 0?
    public Guid[] TradesId;
    public int[] TradesQuantity;
    public int[] TradesPrice;
    public bool[] TradesFilled;
}

public record CancelMarketEvent : MarketEvent { }