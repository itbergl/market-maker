
using MarketMaker.Models;

namespace MarketMaker.Contracts;

public interface IClient
{
   Task SendServerMessage(string message);

   Task NewOrder(NewOrderMarketEvent marketEvent);
   Task NewCancel(CancelMarketEvent marketEvent);
   Task HandleReject(RejectResponse response);
}