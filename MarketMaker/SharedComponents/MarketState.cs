using System.Text;
using MarketMaker.Models;

namespace MarketMaker.Exchange;


public interface IStateWriter
{
    public void RecordEvent(MarketEvent marketEvent);

    public void SetConfig(ExchangeConfig config);
}

public interface IStateReader
{
   public List<Order> Orders { get; }
   public List<Trade> Transactions { get; }
   public bool TryGetOrder(Guid id, out Order order);

   public ExchangeConfig Config { get; set; }
}

public class MarketState : IStateWriter, IStateReader
{
    private static readonly ReaderWriterLockSlim ReaderWriterLock = new();

    private readonly List<Trade> _transactions = new();
    public List<Trade> Transactions
    {
        get
        {
            ReaderWriterLock.EnterWriteLock();
            var trades = _transactions.ToList();
            ReaderWriterLock.ExitWriteLock();
            return trades;
        }
    }

    private readonly Dictionary<Guid, Order> _orders = new();
    public List<Order> Orders
    {
        get
        {
            ReaderWriterLock.EnterWriteLock();
            var orders = _orders.Values.ToList();
            ReaderWriterLock.ExitWriteLock();
            return orders;
        }
    }
    
    private ExchangeConfig Conifg { get; set; }
    public void RecordEvent(MarketEvent marketEvent) 
    {
        ReaderWriterLock.TryEnterWriteLock(10);
        switch (marketEvent)
        {
            case NewOrderMarketEvent no:
                var tradedQuantity = no.TradesQuantity.Sum();
                if (no.Quantity != tradedQuantity)
                {
                    _orders[no.Id] = new Order()
                    {
                        Id = no.Id,
                        Price = no.Price,
                        Symbol = no.Symbol,
                        Quantity = no.Quantity - tradedQuantity,
                        TimeStamp = no.TimeStamp,
                        User = no.User,
                    };
                }

                for (int i = 0; i < no.TradesFilled.Length; i++)
                {
                    var guid = no.TradesId[i];
                    var order = _orders[guid];
                    order.Quantity += no.TradesQuantity[i];
                    if (order.Quantity == 0)
                    {
                        _orders.Remove(guid);
                    }
                }
                
                // transaction
                
                for (int i = 0; i < no.TradesFilled.Length; i++)
                {
                    _transactions.Add(new Trade()
                    {
                        ArgressorOrderId = no.Id,
                        TimeStamp = no.TimeStamp,
                        PassiveOrderId = no.TradesId[i],
                        Price = no.TradesPrice[i],
                        Quantity = no.TradesQuantity[i],
                        Symbol = no.Symbol,
                    });
                }

                break;
            case CancelMarketEvent co:
                _orders.Remove(co.Id);
                break;
        }
        ReaderWriterLock.ExitWriteLock();
    }

    public void SetConfig(ExchangeConfig config)
    {
        Conifg = config;
    }

    public bool TryGetOrder(Guid id, out Order order)
    {
        ReaderWriterLock.TryEnterReadLock(10);
        var success = _orders.TryGetValue(id, out order);
        ReaderWriterLock.ExitReadLock();
        return success;
    }

    public ExchangeConfig Config { get; set; }


    public override string ToString()
    {
        ReaderWriterLock.EnterReadLock();
        if (_orders.Count == 0)
        {
            ReaderWriterLock.ExitReadLock();
            return "\t|-|\t";
        }
        var maxPrice = _orders.Values.MaxBy(o => o.Price)!.Price;
        var minPrice = _orders.Values.MinBy(o => o.Price)!.Price;

        var bidsByPrice = _orders.Values.Where(o => o.Quantity > 0)
            .GroupBy(o => o.Price, (o, c) => new { Price = o, Orders = c })
            .ToDictionary(e => e.Price, e => e.Orders);

        var asksByPrice = _orders.Values.Where(o => o.Quantity < 0)
            .GroupBy(o => o.Price, (o, c) => new { Price = o, Orders = c })
            .ToDictionary(e => e.Price, e => e.Orders);
        
        
        var strBuilder = new StringBuilder();

        foreach (var priceLevel in Enumerable.Range(minPrice, maxPrice - minPrice + 1).Reverse())
        {
            var buys = bidsByPrice.GetValueOrDefault(priceLevel, null)?.Sum(e => e.Quantity) ?? 0;
            var sells = asksByPrice.GetValueOrDefault(priceLevel, null)?.Sum(e => e.Quantity) ?? 0;
            strBuilder.Append($"{buys}\t|{priceLevel}|\t{Math.Abs(sells)}\n");
        }
        
        var str =  strBuilder.ToString();
        ReaderWriterLock.ExitReadLock();

        return str;
    }
}