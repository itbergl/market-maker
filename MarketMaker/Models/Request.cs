using OneOf;

namespace MarketMaker.Models;

public abstract class Request
{
    public string Market;
    public string User;
    public string ClientReference;
}

public class OrderRequest : Request
{
    public int Price;
    public int Quantity;
    public string Symbol;
}

public class CancelRequest : Request
{
    public Guid OrderId;
}

public class ConfigChange
{
}
