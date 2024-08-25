using MarketMaker.ExchangeRules;
using MarketMaker.Models;
using MarketMaker.Services;

namespace MarketMaker.Exchange;

public class Exchange
{
    private readonly IResponder _responder;

    private const string CensoredName = "*****";
    
    private readonly IMarket _market;

    private readonly ExchangeConfig Config = new ExchangeConfig()
    {
        CensorNames = false, // temp TODO
    };

    private readonly IOrderValidator _validator;
    public MarketState MarketState { get; }

    public Exchange(IResponder responder, IMarket market, IOrderValidator validator, MarketState marketState)
    {
        _responder = responder;
        _market = market;
        _validator = validator;
        MarketState = marketState;
    }

    public void HandleConfigUpdate(ConfigChange configChange)
    {
        
    }

    public void HandleOrder(OrderRequest or)
    {
        if (!_validator.ValidateOrder(MarketState, or, out var validationMessage))
        {
            _responder.HandleReject(new Response<RejectResponse>()
            {
                ClientReference = or.ClientReference,
                User = or.User,
                Payload = new RejectResponse {Message = validationMessage}
            });

            return;
        }

        var marketEvent = _market.HandleOrder(or.Symbol, or.User, or.Price, or.Quantity);

        if (marketEvent is null)
        {
            _responder.HandleReject(new Response<RejectResponse>()
            {
                ClientReference = or.ClientReference,
                User = or.User,
                Payload = new RejectResponse {Message = "internal error - order rejected"},
                Market = or.Market
            });
         
            return;   
        }

        EmitEvent(marketEvent, or);
    }
    
    public void HandleCancel(CancelRequest cr)
    {
        if (!_validator.ValidateCancel(MarketState, cr, out var validationMessage))
        {
            _responder.HandleReject(new Response<RejectResponse>()
            {
                ClientReference = cr.ClientReference,
                User = cr.User,
                Market = cr.Market,
                Payload = new RejectResponse {Message = validationMessage}
            });

            return;
        }
        
        var marketEvent = _market.HandleCancel(cr.User, cr.OrderId);
        
        EmitEvent(marketEvent, cr);
    }
    
    private void EmitEvent(MarketEvent marketEvent, Request request)
    {
        _validator.HandleEvent(marketEvent);
        MarketState.RecordEvent(marketEvent);
        
        // TODO: client-ref
        marketEvent.User = Config.CensorNames ? CensoredName : marketEvent.User;
        _responder.HandleEvent(new Response<MarketEvent>()
        {
            ClientReference = request.ClientReference,
            User = marketEvent.User,
            Payload = marketEvent,
            Market = request.Market
        });
    }
}
