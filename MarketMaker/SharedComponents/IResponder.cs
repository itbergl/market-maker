using System.Collections;
using System.Net.Sockets;
using System.Runtime.Caching;
using MarketMaker.Contracts;
using MarketMaker.Hubs;
using MarketMaker.Models;
using Microsoft.AspNetCore.SignalR;

namespace MarketMaker.Services;

public interface IResponder
{
    public void HandleEvent(Response<MarketEvent> eventResponse);
    public void HandleReject(Response<RejectResponse> rejectResponse);
}

public class LoggerResponder : IResponder
{
    private readonly ILogger<LoggerResponder> _logger;
    private readonly IHubContext<MarketHub, IClient> _hub;

    private Dictionary<string, UnderflowCollection<Response<MarketEvent>>> _marketCache = new();

    public LoggerResponder(ILogger<LoggerResponder> logger, IHubContext<MarketHub, IClient> hub)
    {
        _logger = logger;
        _hub = hub;
    }
    public async void HandleEvent(Response<MarketEvent> eventResponse)
    {
        
        _logger.LogInformation($"event: {eventResponse}");

        switch (eventResponse.Payload)
        {
           case NewOrderMarketEvent ne:
               await _hub.Clients.Group($"{eventResponse.Market}").NewOrder(ne);
               break;
           
           case CancelMarketEvent ce:
               await _hub.Clients.Group($"{eventResponse.Market}").NewCancel(ce);
               break;
        }

    }

    public async void HandleReject(Response<RejectResponse> rejectResponse)
    {
        _logger.LogInformation($"reject: {rejectResponse}");

        await _hub.Clients.Group($"{rejectResponse.Market}/{rejectResponse.User}").HandleReject(rejectResponse.Payload);
    }
}

class UnderflowCollection<T> : IEnumerable<T>
{
    private T[] array;
    private int N;
    private int i;

    public UnderflowCollection(int size)
    {
        N = size;
        i = 0;
        array = new T[N];
    }

    public void Add(T element)
    {
        array[i] = element;
        i = (i + 1) % N;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new UnderflowCollectionEnumerator<T>(array, i);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

class UnderflowCollectionEnumerator<T> : IEnumerator<T>
{
    private readonly T[] _array;
    private int _pos;
    private readonly int _start;

    public UnderflowCollectionEnumerator(T[] array, int pos)
    {
        _array = array;
        _start = (pos + 1)%_array.Length;
        _pos = _start;
    }
    
    public bool MoveNext()
    {
        _pos = (_pos - 1)%_array.Length;
        return (_pos == _start);
    }

    public void Reset()
    {
        _pos = _start%_array.Length;
    }

    public T Current
    {
        get
        {
            try
            {
                return _array[_pos];
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException();
            }
        }
    }

    object IEnumerator.Current => Current;
    
    public void Dispose()
    {
    }
}