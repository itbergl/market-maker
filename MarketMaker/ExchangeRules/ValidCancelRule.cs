using MarketMaker.Exchange;
using MarketMaker.Models;

namespace MarketMaker.ExchangeRules;

public class ValidCancelRule : OrderValidator
{
    // TODO: add DI to whole project
    public override bool ValidateCancel(IStateReader marketState, CancelRequest cancelRequest, out string validationMessage)
    {
        if (!marketState.TryGetOrder(cancelRequest.OrderId, out var order))
        {
            validationMessage = $"Order with ID '{cancelRequest.OrderId}' does not exist";
            return false;
        }

        if (!order.User.Equals(cancelRequest.User))
        {
            validationMessage = $"Order with ID '{cancelRequest.OrderId} does not belong to User {cancelRequest.User}";
            return false;
        }

        validationMessage = null;
        return true;
    }
}