
using System.Runtime.Serialization;

namespace MarketMaker.Models;

public class RejectResponse
{
    public string Message { get; set; }
}

public abstract class MarketEvent
{
    public string User{ get; set; }
    public Guid Id{ get; set; }
    public DateTime TimeStamp{ get; set; }
}

public class NewOrderMarketEvent : MarketEvent
{
    public int Price{ get; set; }
    public int Quantity{ get; set; }
    public string Symbol{ get; set; }

    public Guid[] TradesId{ get; set; }
    public int[] TradesQuantity{ get; set; }
    public int[] TradesPrice{ get; set; }
    public bool[] TradesFilled{ get; set; }
}

public class CancelMarketEvent : MarketEvent { }