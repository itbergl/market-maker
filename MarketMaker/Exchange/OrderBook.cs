using System.Text;
using MarketMaker.Models;

namespace MarketMaker.Exchange;

public class OrderBook
{
    private readonly string _symbol;
    private readonly Dictionary<Guid, InternalOrder> _orders = new();
    public readonly Dictionary<int, PriorityQueue<Guid, DateTime>> Ask = new();
    public readonly Dictionary<int, PriorityQueue<Guid, DateTime>> Bid = new();
    private int? _bestAsk;
    private int? _bestBid;

    public OrderBook(string symbol)
    {
        _symbol = symbol;
    }

    public static int GetBestPrice(int requestedPrice, int? bestOtherSide, int side)
    {
        return bestOtherSide is not null
            ? Math.Abs(Math.Min(side * requestedPrice, side * bestOtherSide.Value))
            : requestedPrice;
    }
public MarketEvent NewOrder(string user, int requestedPrice, int quantity)
    {
        var sideIsBid = quantity > 0;

        var side = sideIsBid ? Bid : Ask;
        var otherSide = !sideIsBid ? Bid : Ask;
        var sign = sideIsBid ? 1 : -1;
        int? otherBestPrice = sideIsBid ? _bestAsk : _bestBid;

        // keep removing from queue until first order exists
        var price = GetBestPrice(requestedPrice, otherBestPrice, sign);

        // while we've made all trades possible
        var order = new InternalOrder()
        {
            Id = Guid.NewGuid(),
            User = user,
            Price = requestedPrice,
            Quantity = quantity,
            TimeStamp = DateTime.Now
        };

        List<(Guid, int, long, int, bool)> trades = [];
        
        while ( order.Quantity != 0)
        {
            // if we can make trades at this price
            if (otherSide.TryGetValue(price, out var otherQueue))
            {
                if ((side!.GetValueOrDefault(price, null)?.Count ?? 0) > 0 || otherQueue.Count > 0)
                {
                    while (otherQueue.Count > 0 && order.Quantity != 0)
                    {
                        Guid otherId = otherQueue.Peek();
                        
                        // dormant deleted orders
                        if (!_orders.ContainsKey(otherId))
                        {
                            otherQueue.Dequeue();
                            continue;
                        }

                        var otherOrder = _orders[otherId];
                        int quantityTraded;

                        if (Math.Sign(order.Quantity + otherOrder.Quantity) != Math.Sign(order.Quantity))
                        {
                            quantityTraded = order.Quantity;

                            otherOrder.Quantity += order.Quantity;
                            order.Quantity = 0;
                        }
                        else
                        {
                            quantityTraded = -otherOrder.Quantity;

                            order.Quantity += otherOrder.Quantity;
                            otherOrder.Quantity = 0;
                        }

                        if (otherOrder.Quantity == 0)
                        {
                            otherQueue.Dequeue();
                            _orders.Remove(otherId);
                        }
                        
                        // add consumed order to response
                        trades.Add((otherOrder.Id, quantityTraded, otherOrder.Price, otherOrder.Quantity, otherOrder.Quantity == 0)); 
                    }
                }
            }
            // move up to next best price in otherSide
            price += sign;
            
            if (price == 0 || price * sign > sign * requestedPrice) {
                break;
            }
        }
        
        
        if (order.Quantity != 0)
        {
            _orders.Add(order.Id, order);
            if (!side.ContainsKey(order.Price)) side.TryAdd(order.Price, new PriorityQueue<Guid, DateTime>());
            side[order.Price].Enqueue(order.Id, order.TimeStamp);
        }
        
        // NOTE: the opposite side's best price will be outdated but 
        //       can never be less competitive so we don't need to update it

        if (sign == 1)
        {
            _bestBid ??= order.Price;
            _bestBid = order.Quantity == 0 ? _bestBid : Math.Max(_bestBid.Value, order.Price);
        }
        else
        {
            _bestAsk ??= order.Price;
            _bestAsk = order.Quantity == 0 ? _bestAsk : Math.Min(_bestAsk.Value, order.Price);
        }
        
        // todo: set this per side
        if (_orders.Count == 0)
        {
            _bestAsk = null;
            _bestBid = null;
        }

        return new NewOrderMarketEvent
        {
            Id = order.Id,
            Price = (int)order.Price,
            Quantity = quantity,
            User = order.User,
            Symbol = _symbol,
            TimeStamp = order.TimeStamp,
            TradesFilled = trades.Select(trade => trade.Item5).ToArray(),
            TradesId = trades.Select(trade => trade.Item1).ToArray(),
            TradesPrice = trades.Select(t => (int)t.Item3).ToArray(),
            TradesQuantity = trades.Select(t => t.Item2).ToArray(),
        };
    }

    public bool TryDeleteOrder(Guid id, string user, out CancelMarketEvent cancelEvent)
    {
        if (!_orders.ContainsKey(id))
        {
            cancelEvent = null;
            return false;
        }
        
        _orders.Remove(id);
        
        cancelEvent = new CancelMarketEvent()
        {
            Id = id,
            User = user,
            TimeStamp = DateTime.Now // TODO: datetime provider for unit testing
        };
        
        return true;
    }

    public void Close(long price)
    {
        // foreach (var order in _orders.Values) UserProfits[order.User] += (price - order.Price) * order.Quantity;

        _orders.Clear();
        Bid.Clear();
        Ask.Clear();
    }

    public void RemoveEmptyOrders()
    {
        Bid.Clear();
        Ask.Clear();

        foreach (var order in _orders.Values)
        {
            var side = order.Quantity > 0 ? Bid : Ask;
            side.TryAdd(order.Price, new PriorityQueue<Guid, DateTime>());
            side[order.Price].Enqueue(order.Id, order.TimeStamp);
        }
    }
    
}

public class InternalOrder
{
    public Guid Id;
    public string User;
    public int Price;
    public int Quantity;
    public DateTime TimeStamp;
}