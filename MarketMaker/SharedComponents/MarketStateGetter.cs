using System.Collections.Concurrent;
using MarketMaker.Exchange;

namespace MarketMaker.SharedComponents;

public class MarketStateGetter
{
    private ConcurrentDictionary<string, IStateReader> _states = new();
    
    public void RegisterState(string marketCode, IStateReader stateReader)
    {
        _states.TryAdd(marketCode, stateReader);
    }

    public IStateReader Get(string marketCode)
    {
        return _states[marketCode];
    }

    public void Remove(string marketCode)
    {
        _states.TryRemove(marketCode, out _);
    }
}