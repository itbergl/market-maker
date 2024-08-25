using MarketMaker.Exchange;
using MarketMaker.Models;
using Moq;
using Newtonsoft.Json.Serialization;
using OneOf.Types;

namespace TestMarketMaker;

[TestFixture]
public class TestMarketState
{
    private MarketState _marketState;

    [SetUp]
    public void SetUp()
    {
        _marketState = new MarketState();
    }

    [Test]
    public void NewOrderShouldAddToState()
    {

        Guid id = new Guid();
        var order = AddOrder(id, _marketState);

        Assert.Contains(id, _marketState.Orders.Select(o => o.Id).ToList());

        Assert.True(_marketState.Orders.Any(newOrder => newOrder.TimeStamp == order.TimeStamp &&
                                                         newOrder.Quantity == order.Quantity &&
                                                         newOrder.User == order.User &&
                                                         newOrder.Price == order.Price &&
                                                         newOrder.Id == order.Id &&
                                                         newOrder.Symbol == order.Symbol));
    }

    [Test]
    public void OrderShouldBeDeleted()
    {
        Guid id = new Guid();
        var order = AddOrder(id, _marketState);

        var marketEvent = new CancelMarketEvent
        {
            Id = id,
            TimeStamp = DateTime.Now,
            User = "user",
        };
        
        _marketState.RecordEvent(marketEvent);

        Assert.That(_marketState.TryGetOrder(id, out _), Is.False);
    }

    [Test]
    public void NewOrderShouldUpdateAnyTradedOrders()
    {
        Guid[] ids = Enumerable.Range(0, 3).Select(_ => Guid.NewGuid()).ToArray();
        
        foreach (var guid in ids)
        {
            _marketState.RecordEvent(new NewOrderMarketEvent()
            {
                Id = guid,
                Price = 10,
                Quantity = 10,
                Symbol = "symbol",
                TimeStamp = DateTime.Now,
                TradesFilled = [],
                TradesId = [],
                TradesQuantity = [],
                User = guid.ToString()[..3]
            });
        }

        Guid id = Guid.NewGuid();
        var market = new NewOrderMarketEvent
        {
            Id = id,
            Price = 10,
            Quantity = 25,
            Symbol = "symbol",
            TimeStamp = DateTime.Now,
            TradesFilled = [true, true, false],
            TradesId = ids,
            TradesPrice = [10, 10, 10],
            TradesQuantity = [-10, -10, -5],
        };
        
        _marketState.RecordEvent(market);
        
        Assert.That(_marketState.TryGetOrder(ids[0], out _), Is.False);
        Assert.That(_marketState.TryGetOrder(ids[1], out _), Is.False);
        
        
        Assert.That(_marketState.TryGetOrder(ids[2], out _), Is.True);
        _marketState.TryGetOrder(ids[2], out var order);
        Assert.That(order.Quantity, Is.EqualTo(5));
    }

    private NewOrderMarketEvent AddOrder(Guid id, MarketState listener, int price = 10)
    {
        var order = new NewOrderMarketEvent
        {
            Id = id,
            Price = price,
            Quantity = 1,
            Symbol = "TestSymbol",
            TimeStamp = DateTime.Now,
            TradesFilled = [],
            TradesId = [],
            TradesQuantity = [],
        };
        
        listener.RecordEvent(order);

        return order;
    }


}