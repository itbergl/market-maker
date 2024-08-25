using MarketMaker.Exchange;
using Moq;
using OneOf;

namespace TestMarketMaker;

[TestFixture]
public class TestMarket
{
   private Market _market;
   private MarketState _marketState;

   [SetUp]
   public void SetUp()
   {
      _market = new Market();
      _marketState = new MarketState();
   }

   private Guid? NewOrder(string symbol, string user, int price, int quantity)
   {
       var resp = _market.HandleOrder(symbol, user, price, quantity);
       if (resp is null) return null;
       _marketState.RecordEvent(resp);
       return resp.Id;
   }
   
   private bool NewCancel(string user, Guid id)
   {
       var resp = _market.HandleCancel(user, id);
       _marketState.RecordEvent(resp);
       return true;
   }
   
   // should initialize as empty
   [Test]
   public void ShouldInitializeEmpty()
   {
       Assert.That(_marketState.Orders, Is.Empty);
   }

   [Test]
   [TestCase(12, 10, 1, 10)]
   [TestCase(12, 10, -1, 12)]
   [TestCase(10, 12, 1, 10)]
   [TestCase(12, 10, -1, 12)]
   [TestCase(12, null, 1, 12)]
   [TestCase(12, null, -1, 12)]
   [TestCase(7, 10, -1, 10)]
   public void TestGetBestPrice(int requestedPrice, int? otherBestPrice, int side, int expected)
   {
       var bestPrice = OrderBook.GetBestPrice(requestedPrice, otherBestPrice, side);
       
       Assert.That(bestPrice, Is.EqualTo(expected));
   }
   
   // should add order 
   [Test]
   public void ShouldAddOrder()
   {
       var id = NewOrder("A", "userA", 10, 10);
       
       Assert.That(id, Is.Not.Null);
       
       Assert.That(_marketState.TryGetOrder(id!.Value, out _));
   }
   
   // should be able to delete order
   [Test]
   public void ShouldDeleteOrder()
   {
       var id = NewOrder("A", "userA", 10, 10)!.Value;
       NewCancel("userA", id);
       
       Assert.That(_marketState.TryGetOrder(id, out _), Is.False);
   }
   
   // existing order should match and trade
   [Test]
   [TestCase(-1)]
   [TestCase(1)]
   public void SimpleMatch(int side)
   {
       var existingId = NewOrder("A", "userA", 10, 5 * side);

       var newId = NewOrder("A", "userB", 10, -5 * side);
       
       Assert.That(_marketState.TryGetOrder(existingId!.Value, out _), Is.False);
       Assert.That(_marketState.TryGetOrder(newId!.Value, out _), Is.False);
   }
   
   // existing order at lower price should match and trade
   [Test]
   [TestCase(-1)]
   [TestCase(1)]
   public void BestPriceShouldMatch(int side)
   {
       var price = 10;
       var existingId = NewOrder("A", "userA", price, 5 * side);

       var newId = NewOrder("A", "userB", price - side, -5 * side);
       
       Assert.That(_marketState.TryGetOrder(existingId!.Value, out _), Is.False);
       Assert.That(_marketState.TryGetOrder(newId!.Value, out _), Is.False);
   }
   
   
   // many orders exist, should match with all and have q quantity remaining 
   [Test]
   [TestCase(30)]
   [TestCase(-30)]
   [TestCase(10)]
   [TestCase(-10)]
   [TestCase(20)]
   [TestCase(-20)]
   public void ManyOrdersExist(int quantity)
   {
       // 10, 5, 2, 3
       var side = Math.Sign(quantity);
       
       var existingIdA = NewOrder("A", "userA", 10, -10 * side)!.Value;
       var existingIdB = NewOrder("A", "userB", 10, -5 * side)!.Value;
       var existingIdC = NewOrder("A", "userC", 10, -2 * side)!.Value;
       var existingIdD = NewOrder("A", "userD", 10, -3 * side)!.Value;
   
       var newId = NewOrder("A", "userE", 10, quantity)!.Value;

       var quantityAtLevel = _marketState.Orders.Where(o => o.Price == 10).Sum(o => o.Quantity);
       
       Assert.That(quantityAtLevel, Is.EqualTo(quantity - side * 20));

       if (side == 1 && quantity <= 20 || side == -1 && quantity >= -20)
       {
          Assert.That(_marketState.TryGetOrder(newId, out _), Is.False); 
       }
   }
   
   // large order exists, should partially consume existing order
   [Test]
   [TestCase(-1)]
   [TestCase(1)]
   public void SingleLargeOrderExists_ShouldBePartiallyConsumed(int side)
   {
       // 10, 5, 2, 3
       var existingId = NewOrder("A", "userA", 10, -100 * side)!.Value;
       var newId = NewOrder("A", "userE", 10, 50 * side)!.Value;

       var quantityAtLevel = _marketState.Orders.Where(o => o.Price == 10).Sum(o => o.Quantity);
       
       Assert.That(quantityAtLevel, Is.EqualTo(- side * 50));

       Assert.That(_marketState.TryGetOrder(newId, out _), Is.False); 
       Assert.That(_marketState.TryGetOrder(existingId, out _), Is.True); 
   }

   internal record SimpleOrderRequest(string User, int Price, int Quantity);
   internal record SimpleOrderDeleteRequest(string User);

   private void AssertAtPrice(int price, int quantity, int nOrders)
   {
       var ordersAtPrice = _marketState.Orders.Where(o => o.Price == price).ToList();
       
       var quantityAtLevel = ordersAtPrice.Sum(o => o.Quantity);
       Assert.That(quantityAtLevel, Is.EqualTo(quantity));
       Assert.That(ordersAtPrice, Has.Count.EqualTo(nOrders));
   }
   
   // random market tests
   [Test]
   public void EventTestA()
   {
       List<SimpleOrderRequest> events =
       [
           new("A", 12, 5),
           new("B", 12, 2),
           new("C", 12, 3),
           new("D", 16, -4),
           new("E", 16, -1),
           new("F", 16, -1),
           new("F", 15, -7),
           new("A", 15, 10),
           new("F", 13, -3),
           new("C", 10, 9),
           new("E", 9, -7),
           new("E", 12, -11),
           new("enderB", 16, 14),
           new("enderA", 10, -9),
       ];
       
       events.ForEach(e =>
       {
           var format = _marketState.ToString();
           Console.WriteLine(format);
           NewOrder("symbol", e.User, e.Price, e.Quantity);
       });
       
       Console.WriteLine(_marketState.ToString());
       Assert.That(_marketState.Orders, Is.Empty);
   }
   
   
   [Test]
   public void EventTestB()
   {
       List<SimpleOrderRequest> events =
       [
           new("A", 200, -80),
           new("A", 200, -20),
           new("B", 160, 32),
           new("A", 180, -40),
           new("B", 180, 5),
           new("B", 210, 200),
           new("A", 190, -5),
           new("A", 210, -60),
           new("A", 160, -16),
           new("A", 130, -10),
           new("A", 100, -6),
       ];
       
       events.ForEach(e =>
       {
           var format = _marketState.ToString();
           Console.WriteLine(format);
           NewOrder("symbol", e.User, e.Price, e.Quantity);
       });
       
       Console.WriteLine(_marketState.ToString());
        
       Assert.That(_marketState.Orders, Is.Empty);
   }
   
   
   [Test]
   public void EventTestC()
   {
       List<object> events =
       [
           new SimpleOrderRequest("DeleteMeC", 59, -10),
           new SimpleOrderRequest("DeleteMeB", 56, 15),
           new SimpleOrderRequest("DeleteMeA", 57, -3),
           new SimpleOrderDeleteRequest("DeleteMeA"),
           new SimpleOrderRequest("A", 58, 3),
           new SimpleOrderRequest("B", 54, 6),
           new SimpleOrderRequest("A", 53, -15),
           new SimpleOrderDeleteRequest("DeleteMeB"),
           new SimpleOrderRequest("A", 54, -1),
           new SimpleOrderDeleteRequest("DeleteMeC"),
           new SimpleOrderRequest("A", 50, -6),
           new SimpleOrderRequest("B", 50, 1),
       ];

       var lastOrders = new Dictionary<string, Guid>();
       
       events.ForEach(e =>
       {
           var format = _marketState.ToString();
           Console.WriteLine(format);
           if (e is SimpleOrderRequest so)
           {
               var guid = NewOrder("symbol", so.User, so.Price, so.Quantity);
               lastOrders[so.User] = guid.Value;
           }
           else if (e is SimpleOrderDeleteRequest sd)
           {
               var id = lastOrders[sd.User];
               NewCancel(sd.User, id);
           }
       });
       
       Console.WriteLine(_marketState.ToString());
        
       Assert.That(_marketState.Orders, Is.Empty);
   }
}