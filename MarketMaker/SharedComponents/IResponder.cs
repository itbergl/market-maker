using MarketMaker.Models;

namespace MarketMaker.Services;

public interface IResponder
{
    public void HandleEvent(MarketEventResponse marketEventResponse);
    public void HandleReject(RejectResponse rejectResponse);
}