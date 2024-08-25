using MarketMaker.ExchangeRules;
using MarketMaker.Models;
using MarketMaker.Services;

namespace MarketMaker.Exchange;

public class Exchange
{
    private readonly IResponder _responder;

    private const string CensoredName = "*****";
    
    private readonly IMarket _market;

    public readonly StateListener StateListener = new StateListener();
    public readonly TransactionsListener TransactionsListener = new TransactionsListener();

    private readonly ExchangeConfig Config = new ExchangeConfig()
    {
        CensorNames = false, // temp TODO
    };

    private readonly IOrderValidator _validator;

    public Exchange(IResponder responder, IMarket market, IOrderValidator validator)
    {
        _responder = responder;
        _market = market;
        _validator = validator;
    }

    public void HandleConfigUpdate(ConfigChange configChange)
    {
        
    }

    public void HandleOrder(OrderRequest or)
    {
        if (!_validator.ValidateOrder(StateListener, or, out var validationMessage))
        {
            _responder.HandleReject(new RejectResponse()
            {
                ClientReference = or.ClientReference,
                User = or.User,
                Message = validationMessage
            });

            return;
        }

        var marketEvent = _market.HandleOrder(or.Symbol, or.User, or.Price, or.Quantity);

        if (marketEvent is null)
        {
            _responder.HandleReject(new RejectResponse()
            {
                ClientReference = or.ClientReference,
                User = or.User,
                Message = "internal error - order rejected"
            });
         
            return;   
        }

        EmitEvent(marketEvent, or.ClientReference);
    }
    
    public void HandleCancel(CancelRequest cr)
    {
        if (!_validator.ValidateCancel(StateListener, cr, out var validationMessage))
        {
            _responder.HandleReject(new RejectResponse()
            {
                ClientReference = cr.ClientReference,
                User = cr.User,
                Message = validationMessage
            });

            return;
        }
        
        var marketEvent = _market.HandleCancel(cr.User, cr.OrderId);
        
        EmitEvent(marketEvent, cr.ClientReference);
    }
    
    private void EmitEvent(MarketEvent marketEvent, string clientReference)
    {
        _validator.HandleEvent(marketEvent);
        StateListener.RecordEvent(marketEvent);
        
        // TODO: client-ref
        marketEvent.User = Config.CensorNames ? CensoredName : marketEvent.User;
        _responder.HandleEvent(new MarketEventResponse()
        {
            ClientReference = clientReference,
            User = marketEvent.User,
            MarketEvent = marketEvent
        });
    }
}
