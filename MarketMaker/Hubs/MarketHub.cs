using System.Runtime.CompilerServices;
using MarketMaker.Contracts;
using MarketMaker.Models;
using MarketMaker.Services;
using MarketMaker.SharedComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MarketMaker.Hubs
{
    [Authorize("ViewerRequired")]
    public class MarketHub : Hub<IClient>
    {
        private readonly ILogger<MarketHub> _logger;
        private readonly MarketStateGetter _stateGetter;
        private readonly ExchangeService _exchangeService;
        private readonly IRequestQueue _requestQueue;

        public MarketHub(ILogger<MarketHub> logger, MarketStateGetter stateGetter, ExchangeService exchangeService, IRequestQueue requestQueue)
        {
            _logger = logger;
            _stateGetter = stateGetter;
            _exchangeService = exchangeService;
            _requestQueue = requestQueue;
        }
        
        public override Task OnConnectedAsync()
        {
            Clients.All.SendServerMessage($"{Context.ConnectionId} joined");
            
            // todo: create user object
            var marketCode = Context.User.Claims.Single(c => c.Type == "marketCode").Value;

            _exchangeService.TryAddExchange(marketCode);

            var state = _stateGetter.Get(marketCode);

            var user = Context.User.Claims.Single(c => c.Type == "name").Value;
            var market = Context.User.Claims.Single(c => c.Type == "marketCode").Value;

            Groups.AddToGroupAsync(Context.ConnectionId, $"{market}");
            Groups.AddToGroupAsync(Context.ConnectionId, $"{market}/{user}");
            
            Clients.Caller.SendServerMessage(state.ToString());
            
            return base.OnConnectedAsync();
        }
        
        public Task SendOrder(int price, int quantity, string symbol, string clientReference)
        {
            var user = Context.User.Claims.Single(c => c.Type == "name").Value;
            var market = Context.User.Claims.Single(c => c.Type == "marketCode").Value;

            _logger.LogInformation($"{user} to {market}({symbol}): {quantity}@(${price})");
            _requestQueue.SendRequest(new OrderRequest()
            {
                ClientReference = clientReference,
                Market = market,
                Price = price,
                Quantity = quantity,
                Symbol = symbol,
                User = user
            }, new CancellationToken());

            return Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Clients.All.SendServerMessage($"{Context.ConnectionId} left");
            return base.OnDisconnectedAsync(exception);
        }
    }
}