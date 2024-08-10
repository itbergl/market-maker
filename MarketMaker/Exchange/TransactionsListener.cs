using MarketMaker.Models;

namespace MarketMaker.Exchange;

public class TransactionsListener
{
    public List<Trade> Transactions { get; } = new();
    
    public void RecordEvent(MarketEvent marketEvent) 
    {
        if (marketEvent is not NewOrderMarketEvent no)
        {
            return;
        }

        if (no.TradesFilled is null)
        {
            return;
        }

        for (int i = 0; i < no.TradesFilled.Length; i++)
        {
            Transactions.Add(new Trade()
            {
                ArgressorOrderId = no.Id,
                TimeStamp = no.TimeStamp,
                PassiveOrderId = no.TradesId[i],
                Price = no.TradesPrice[i],
                Quantity = no.TradesQuantity[i],
                Symbol = no.Symbol,
            });
        }
    }

}
