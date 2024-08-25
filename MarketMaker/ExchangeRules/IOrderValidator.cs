using MarketMaker.Exchange;
using MarketMaker.Models;

namespace MarketMaker.ExchangeRules;

public interface IOrderValidator // singleton   
{
    public bool ValidateOrder(IStateReader marketState, OrderRequest orderRequest, out string validationMessage);
    public bool ValidateCancel(IStateReader marketState, CancelRequest cancelRequest, out string validationMessage);

    public void HandleEvent(MarketEvent marketEvent);
}

public abstract class OrderValidator : IOrderValidator
{
    public virtual bool ValidateOrder(IStateReader marketState, OrderRequest orderRequest, out string validationMessage)
    {
        validationMessage = null;
        return true;
    }

    public virtual bool ValidateCancel(IStateReader marketState, CancelRequest cancelRequest, out string validationMessage)
    {
        validationMessage = null;
        return true;
    }
    
    public virtual void HandleEvent(MarketEvent _) {}
}
