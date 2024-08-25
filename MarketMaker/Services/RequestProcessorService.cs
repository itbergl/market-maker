using System.Net;
using System.Threading.Channels;
using MarketMaker.Models;

namespace MarketMaker.Services;

public class RequestProcessorService : BackgroundService 
{
    private readonly IRequestQueue _queue;
    private readonly ExchangeService _exchangeService;

    public RequestProcessorService(IRequestQueue queue, ExchangeService exchangeService)
    {
        _queue = queue;
        _exchangeService = exchangeService;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // todo: use blocking collection
            // if (!_queue.GetRequest(out Request request))
            // {
            //     continue;
            // }
            var request = await _queue.GetRequestAsync(stoppingToken);

            _exchangeService.HandleRequest(request);
        }
    }
}