using MarketMaker.Exchange;
using MarketMaker.Models;
using Moq;
using Newtonsoft.Json.Serialization;
using OneOf.Types;

namespace TestMarketMaker;

[TestFixture]
public class TestMarketState
{
    private StateListener _state;

    [SetUp]
    public void SetUp()
    {
        _state = new StateListener();
    }

    [Test]
    public void NewOrderShouldAddToState()
    {

        Guid id = new Guid();
        var order = AddOrder(id, _state);

        Assert.Contains(id, _state.Orders.Keys);

        Assert.True(_state.Orders.Values.Any(newOrder => newOrder.TimeStamp == order.TimeStamp &&
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
        var order = AddOrder(id, _state);

        var marketEvent = new CancelMarketEvent
        {
            Id = id,
            TimeStamp = DateTime.Now,
            User = "user",
        };
        
        _state.RecordEvent(marketEvent);

        Assert.That(_state.Orders.ContainsKey(id), Is.False);
    }

    [Test]
    public void NewOrderShouldUpdateAnyTradedOrders()
    {
        Guid[] ids = Enumerable.Range(0, 3).Select(_ => Guid.NewGuid()).ToArray();
        
        foreach (var guid in ids)
        {
            _state.Orders[guid] = new Order()
            {
                Id = guid,
                Price = 10,
                Quantity = 10,
                Symbol = "symbol",
                TimeStamp = DateTime.Now,
                User = guid.ToString()[..3]
            };
        }

        Guid id = Guid.NewGuid();
        var market = new NewOrderMarketEvent
        {
            Filled = false,
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
        
        _state.RecordEvent(market);
        
        Assert.That(_state.Orders.ContainsKey(ids[0]), Is.False);
        Assert.That(_state.Orders.ContainsKey(ids[1]), Is.False);
        
        
        Assert.That(_state.Orders.ContainsKey(ids[2]), Is.True);
        Assert.That(_state.Orders[ids[2]].Quantity, Is.EqualTo(5));
    }

    private NewOrderMarketEvent AddOrder(Guid id, StateListener listener, int price = 10)
    {
        var order = new NewOrderMarketEvent
        {
            Filled = false,
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