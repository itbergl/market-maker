using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using MarketMaker.Exchange;
using MarketMaker.Models;

namespace BenchmarkMarketMaker
{
    public class Benchmarks
    {
        private OrderBook _orderBook;
        private Random _random;
        private List<Action> _actions;

        private const int nOrders = 1000;

        [GlobalSetup]
        public void SetUp()
        {
            _orderBook = new OrderBook("Symbol");
            _random = new Random();

            _actions = GenerateRandomOrders();
        }
        
        [Benchmark]
        public void RandomActivity()
        {
            foreach (var a in _actions)
            {
                a?.Invoke();        
            }
        }

        private List<Action> GenerateRandomOrders()
        {
            return Enumerable
                .Range(0, nOrders)
                .Select(_ => new Action(() =>
                {
                    var buy = _random.NextDouble() > 0.5;
                    var price = (int)Math.Round(_random.NextDouble() * 50);
                    var quantity = 1 + (int)Math.Round(_random.NextDouble() * 99);
                    _orderBook.NewOrder(buy ? "buyer" : "seller", price, quantity);
                }))
                .ToList();
        }
    }
}
