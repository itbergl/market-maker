using MarketMaker.Models;
using MarketMaker.SharedComponents;

namespace MarketMaker.Services;

public class ExchangeService
{
    private readonly MarketStateGetter _stateGetter;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<ExchangeService> _logger;
    private readonly IResponder _responder;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMarketNameProvider _nameProvider;
    private Dictionary<string, Exchange.Exchange> _exchanges = new();

    public ExchangeService(MarketStateGetter stateGetter, IDateTimeProvider dateTimeProvider, ILogger<ExchangeService> logger, IResponder responder, IServiceProvider serviceProvider, IMarketNameProvider nameProvider)
    {
        _stateGetter = stateGetter;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
        _responder = responder;
        _serviceProvider = serviceProvider;
        _nameProvider = nameProvider;
    }
    
    public bool TryAddExchange(string id)
    {
        var exchange = _serviceProvider.GetService<Exchange.Exchange>()!;
        _exchanges[id] = exchange;
        // map shared resource to exchange's state
        _stateGetter.RegisterState(id, exchange.MarketState); 
        
        _logger.LogInformation($"Exchange {id} created");
        return true;
    }

    public bool TryDeleteExchange(string id)
    {
        return _exchanges.Remove(id);
    }

    public void HandleRequest(Request request)
    {
        //temp
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