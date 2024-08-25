namespace MarketMaker.Services;

public interface IMarketNameProvider
{
    string Generate();
}

public class MarketNameProvider : IMarketNameProvider
{
    const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly Random Random = new Random();
    private const int Length = 6;
    
    public string Generate()
    {
        return string.Join("", Random.GetItems(AllowedChars.AsSpan(), Length));
    }
    
}