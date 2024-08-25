using System.Threading.Channels;
using MarketMaker.Models;

namespace MarketMaker.Services;

public interface IRequestQueue
{
    Task SendRequest(Request message, CancellationToken cancellationToken);
    Task<Request> GetRequestAsync(CancellationToken cancellationToken);
}

public class RequestQueue : IRequestQueue
{
    private readonly Channel<Request> _request = Channel.CreateUnbounded<Request>();

    public async Task SendRequest(Request message, CancellationToken cancellationToken)
    {
        await _request.Writer.WriteAsync(message, cancellationToken);
    }

    public async Task<Request> GetRequestAsync(CancellationToken cancellationToken)
    {
        return await _request.Reader.ReadAsync(cancellationToken);
    }
}