using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MarketMaker.Endpoints;

public class ConfigUpdate
{
   public string Name { get; set; }
}

public static class MarketEndpoints
{
   public static WebApplication AddMarketEndpoints(this WebApplication app)
   {
      app.MapPost("/api/v1/update-config", UpdateConfig)
         .RequireAuthorization("AdminRequired");

      return app;
   }
   
   // add exchange -> exchange info, token
   // view exchange -> token, state(seq)
   // join exchange -> token, state(seq)
   
   //  -> 

   // for admins
   public static string UpdateConfig(HttpContext context, ConfigUpdate configUpdate)
   {
      var market = context.User.Claims
         .Single(c => string.Equals(c.Type, "marketCode"))
         .Value;
      
      return $"market-updated: {configUpdate} for market {market}";
   }
   
}
