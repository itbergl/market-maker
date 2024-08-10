using MarketMaker.Models;

namespace MarketMaker.Services;

public class ExchangeService
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<ExchangeService> _logger;
    private readonly IResponder _responder;
    private readonly IServiceProvider _serviceProvider;
    private Dictionary<string, Exchange.Exchange> _exchanges = new();

    public ExchangeService(IDateTimeProvider dateTimeProvider, ILogger<ExchangeService> logger, IResponder responder, IServiceProvider serviceProvider)
    {
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
        _responder = responder;
        _serviceProvider = serviceProvider;
    }
    
    public bool TryAddExchange(out string id)
    {
        id = Guid.NewGuid().ToString().Substring(0, 5);
        _exchanges[id] = _serviceProvider.GetService<Exchange.Exchange>() ?? throw new NullReferenceException();
        return true;
    }

    public bool TryDeleteExchange(string id)
    {
        return _exchanges.Remove(id);
    }

    public void HandleRequest(Request request)
    {
        if (!_exchanges.TryGetValue(request.Market, out var exchange)) return;

        switch (request)
        {
            case OrderRequest or:
                exchange.HandleOrder(or);
                break;
            case CancelRequest cr:
                exchange.HandleCancel(cr);
                break;
            default:
                return;
        }
    }
}