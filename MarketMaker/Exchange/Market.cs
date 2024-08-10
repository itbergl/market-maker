using MarketMaker.Models;

namespace MarketMaker.Exchange;

public interface IMarket
{
    MarketEvent? HandleOrder(string symbol, string user, int price, int quantity);
    MarketEvent HandleCancel(string user, Guid id);
}

public class Market : IMarket
{
    private Dictionary<string, OrderBook> _orderBooks = new();
    public MarketEvent? HandleOrder(string symbol, string user, int price, int quantity)
    {
        _orderBooks.TryAdd(symbol, new OrderBook(symbol));
        return _orderBooks[symbol].NewOrder(user, price, quantity);
    }

    public MarketEvent HandleCancel(string user, Guid id)
    {
        foreach (var orderBook in _orderBooks.Values)
        {
            if (orderBook.TryDeleteOrder(id, user, out var cancelEvent)) // TODO
            {
                return cancelEvent;
            }
        }

        throw new ArgumentException($"Order {id} does not exist");
    }
}