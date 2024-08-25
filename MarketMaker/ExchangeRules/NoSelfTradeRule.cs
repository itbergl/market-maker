using MarketMaker.Exchange;
using MarketMaker.Models;
using OneOf.Types;

namespace MarketMaker.ExchangeRules;

public class NoSelfTradeRule : OrderValidator
{
    private Dictionary<(string, int), int> _netQuantity = new();
    // TODO: add DI to whole project
    public override bool ValidateOrder(IStateReader marketState, OrderRequest orderRequest, out string validationMessage)
    {
        var currentQuantity = _netQuantity.GetValueOrDefault((orderRequest.User, orderRequest.Price), 0);
        if (currentQuantity != 0 && int.Sign(currentQuantity) != int.Sign(orderRequest.Quantity))
        {
            validationMessage = $"Cannot buy and sell at same price";
            return false;
        }
        validationMessage = null;
        return true;
    }
}