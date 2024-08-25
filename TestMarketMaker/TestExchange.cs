using MarketMaker.Exchange;
using MarketMaker.ExchangeRules;
using MarketMaker.Models;
using MarketMaker.Services;
using Moq;

namespace TestMarketMaker;


[TestFixture]
public class TestExchange
{
    private Mock<IMarket> _market;
    private Exchange _exchange;
    private Mock<IResponder> _responderMock;
    private Mock<IOrderValidator> _validator;
    private MarketState _marketState;

    [SetUp]
    public void Setup()
    {
        _responderMock = new Mock<IResponder>();
        _market = new Mock<IMarket>();
        _validator = new Mock<IOrderValidator>();
        _marketState = new MarketState();
        _exchange = new Exchange(_responderMock.Object, _market.Object, _validator.Object, _marketState);
    }

    [Test]
    [TestCase("TestSymbol", "TestUser", 10, 10)]
    [TestCase("TestSymbol", "TestUser", 20, -10)]
    public void NewTradeSendsConfirmationResponse(string symbol, string user, int price, int quantity)
    {
        _market
            .Setup(m => m.HandleOrder(symbol, user, price, quantity))
            .Returns(new NewOrderMarketEvent()
            {
               User = user,
               Id = Guid.NewGuid(),
               Price = price,
               Quantity = quantity,
               TimeStamp = DateTime.Now,
               TradesFilled = [],
               TradesId = [],
               TradesQuantity = [],
               TradesPrice = [],
            });
        
        var orderRequest = new OrderRequest()
        {
            User = user,
            Price = price,
            Quantity = quantity,
            Symbol = symbol
        };

        var validationMessage = "";
        _validator.Setup(v => v.ValidateOrder(It.IsAny<MarketState>(), orderRequest, out validationMessage))
            .Returns(true);
        
       _exchange.HandleOrder(orderRequest); 
       
        _responderMock.Verify(r => r.HandleEvent(It.Is<Response<MarketEvent>>(response => 
            string.Equals(response.User, user) && response.Payload is NewOrderMarketEvent))); 
    }
    
    
    [Test]
    [TestCase("TestSymbol", "TestUser", 10, 10)]
    [TestCase("TestSymbol", "TestUser", 20, -10)]
    public void NewTradeRejectSendsRejectionResponse(string symbol, string user, int price, int quantity)
    {
        var orderRequest = new OrderRequest()
        {
            User = user,
            Price = price,
            Quantity = quantity,
            Symbol = symbol
        };
       
        var validationMessage = "validationMessage";
        _validator.Setup(v => v.ValidateOrder(It.IsAny<MarketState>(), orderRequest, out validationMessage))
            .Returns(false);
        
       _exchange.HandleOrder(orderRequest); 
        _responderMock.Verify(r => r.HandleReject(It.Is<Response<RejectResponse>>(response => 
            string.Equals(response.User, user) && string.Equals(response.Payload.Message, validationMessage)))); 
    }
    
    
    [Test]
    [TestCase("TestUser")]
    public void NewCancelSendsConfirmationResponse(string user)
    {
        var id = Guid.NewGuid();
        
        _market
            .Setup(m => m.HandleCancel(user, id))
            .Returns(new CancelMarketEvent()
            {
               User = user,
               Id = id,
            });
        
        var cancelRequest = new CancelRequest()
        {
            User = user,
            OrderId = id
        };

        var validationMessage = "";
        _validator.Setup(v => v.ValidateCancel(It.IsAny<MarketState>(), cancelRequest, out validationMessage))
            .Returns(true);
        
       _exchange.HandleCancel(cancelRequest); 
       
        _responderMock.Verify(r => r.HandleEvent(It.Is<Response<MarketEvent>>(response => 
            string.Equals(response.User, user) && response.Payload is CancelMarketEvent))); 
    }
    
    
   
    [Test]
    [TestCase("TestUser")]
    public void NewCancelRejectSendsRejectResponse(string user)
    {
        var id = Guid.NewGuid();
        _market
            .Setup(m => m.HandleCancel(user, id))
            .Returns(new CancelMarketEvent()
            {
               User = user,
               Id = id,
            });
        
        var cancelRequest = new CancelRequest()
        {
            User = user,
            OrderId = id
        };

        var validationMessage = "";
        _validator.Setup(v => v.ValidateCancel(It.IsAny<MarketState>(), cancelRequest, out validationMessage))
            .Returns(false);
        
       _exchange.HandleCancel(cancelRequest); 
       
        _responderMock.Verify(r => r.HandleReject(It.Is<Response<RejectResponse>>(response => 
            string.Equals(response.User, user) && string.Equals(response.Payload.Message, validationMessage)))); 
    }

}