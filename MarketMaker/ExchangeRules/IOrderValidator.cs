using MarketMaker.Exchange;
using MarketMaker.Models;

namespace MarketMaker.ExchangeRules;

public interface IOrderValidator // singleton   
{
    public bool ValidateOrder(StateListener stateListener, OrderRequest orderRequest, out string validationMessage);
    public bool ValidateCancel(StateListener stateListener, CancelRequest cancelRequest, out string validationMessage);

    public void HandleEvent(MarketEvent marketEvent);
}

public abstract class OrderValidator : IOrderValidator
{
    public virtual bool ValidateOrder(StateListener stateListener, OrderRequest orderRequest, out string validationMessage)
    {
        validationMessage = null;
        return true;
    }

    public virtual bool ValidateCancel(StateListener stateListener, CancelRequest cancelRequest, out string validationMessage)
    {
        validationMessage = null;
        return true;
    }
    
    public virtual void HandleEvent(MarketEvent _) {}
}
