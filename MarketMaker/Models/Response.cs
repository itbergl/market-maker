namespace MarketMaker.Models;

public class Response<T>
{
    public string ClientReference { get; set; }
    public string User{ get; set; }
    public string Market { get; set; }
    public T Payload{ get; set; }
}
