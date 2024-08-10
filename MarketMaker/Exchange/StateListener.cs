using System.Text;
using MarketMaker.Models;

namespace MarketMaker.Exchange;

public class StateListener
{
    public Dictionary<Guid, Order> Orders { get; } = new();
    public void RecordEvent(MarketEvent marketEvent) 
    {
        switch (marketEvent)
        {
            case NewOrderMarketEvent no:
                var tradedQuantity = no.TradesQuantity.Sum();
                if (no.Quantity != tradedQuantity)
                {
                    Orders[no.Id] = new Order()
                    {
                        Id = no.Id,
                        Price = no.Price,
                        Symbol = no.Symbol,
                        Quantity = no.Quantity - tradedQuantity,
                        TimeStamp = no.TimeStamp,
                        User = no.User,
                    };
                }

                for (int i = 0; i < no.TradesFilled.Length; i++)
                {
                    var guid = no.TradesId[i];
                    var order = Orders[guid];
                    order.Quantity += no.TradesQuantity[i];
                    if (order.Quantity == 0)
                    {
                        Orders.Remove(guid);
                    }
                }

                break;
            case CancelMarketEvent co:
                Orders.Remove(co.Id);
                break;
        }
    }
    
    public override string ToString()
    {
        if (Orders.Count == 0)
        {
            return "\t|-|\t";
        }
        var maxPrice = Orders.Values.MaxBy(o => o.Price)!.Price;
        var minPrice = Orders.Values.MinBy(o => o.Price)!.Price;

        var bidsByPrice = Orders.Values.Where(o => o.Quantity > 0)
            .GroupBy(o => o.Price, (o, c) => new { Price = o, Orders = c })
            .ToDictionary(e => e.Price, e => e.Orders);

        var asksByPrice = Orders.Values.Where(o => o.Quantity < 0)
            .GroupBy(o => o.Price, (o, c) => new { Price = o, Orders = c })
            .ToDictionary(e => e.Price, e => e.Orders);
        
        
        var strBuilder = new StringBuilder();

        foreach (var priceLevel in Enumerable.Range(minPrice, maxPrice - minPrice + 1).Reverse())
        {
            var buys = bidsByPrice.GetValueOrDefault(priceLevel, null)?.Sum(e => e.Quantity) ?? 0;
            var sells = asksByPrice.GetValueOrDefault(priceLevel, null)?.Sum(e => e.Quantity) ?? 0;
            strBuilder.Append($"{buys}\t|{priceLevel}|\t{Math.Abs(sells)}\n");
        }
        

        return strBuilder.ToString();
    }
}