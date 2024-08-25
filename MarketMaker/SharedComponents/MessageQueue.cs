using System.Threading.Channels;
using MarketMaker.Models;

namespace MarketMaker.Services;

public interface IRequestQueue
{
    Task SendRequest(Request message);
    bool GetRequest(out Request message);
}

public class RequestQueue : IRequestQueue
{
    private readonly Channel<Request> _request = Channel.CreateUnbounded<Request>();

    public async Task SendRequest(Request message)
    {
        await _request.Writer.WriteAsync(message);
    }

    public bool GetRequest(out Request message)
    {
        return _request.Reader.TryRead(out message);
    }
}