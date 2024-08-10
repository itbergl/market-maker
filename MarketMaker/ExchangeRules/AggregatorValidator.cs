﻿using MarketMaker.Exchange;
using MarketMaker.Models;

namespace MarketMaker.ExchangeRules;

public class AggregatorValidator : IOrderValidator
{
    public List<IOrderValidator> Validators { get; set; }

    public AggregatorValidator()
    {
        Validators = new List<IOrderValidator>()
        {
            new ValidCancelRule(),
            new NoSelfTradeRule()
        };
    }
    
    public bool ValidateOrder(StateListener stateListener, OrderRequest orderRequest, out string validationMessage)
    {
        List<string> validationErrors = new();
        foreach (var validator in Validators)
        {
            // smells like a aggregator validator is needed, which needs a factory
            if (!validator.ValidateOrder(stateListener, orderRequest, out var validationError)) {
                validationErrors.Add(validationError); 
            }
        }

        if (validationErrors.Count != 0)
        {
            validationMessage = string.Join(", ", validationErrors);
            return false;
        }

        validationMessage = null;
        return true;
    }

    public bool ValidateCancel(StateListener stateListener, CancelRequest cancelRequest, out string validationMessage)
    {
        List<string> validationErrors = new();
        foreach (var validator in Validators)
        {
            // smells like a aggregator validator is needed, which needs a factory
            if (!validator.ValidateCancel(stateListener, cancelRequest, out var validationError)) {
                validationErrors.Add(validationError); 
            }
        }
        
        if (validationErrors.Count != 0)
        {
            validationMessage = string.Join(", ", validationErrors);
            return false;
        }

        validationMessage = null;
        return true;
    }

    public void HandleEvent(MarketEvent marketEvent)
    {
        foreach (var orderValidator in Validators)
        {
            orderValidator.HandleEvent(marketEvent);
        }
    }
}