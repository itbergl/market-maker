using MarketMaker.Services;

namespace MarketMaker.Endpoints;

public static class AuthEndpoint
{
   public static WebApplication AddTokenRequestEndpoints(this WebApplication app)
   {
      app.MapGet("/api/v1/request-admin-token", RequestAdminToken);
      app.MapGet("/api/v1/request-viewer-token", RequestViewerToken);
      app.MapGet("/api/v1/request-player-token", RequestPlayerToken);

      return app;
   }

   public static NewTokenResponse RequestAdminToken(HttpContext context, IMarketNameProvider nameProvider)
   {
      var market = nameProvider.Generate();
      var token = JwtBuilder
         .CreateNewBuilder()
         .SetAdmin(true)
         .SetMarket(market)
         .Build();

      return new NewTokenResponse()
      {
         MarketCode = market,
         IsAdmin = true,
         Token = token,
      };
   }

   public static NewTokenResponse RequestViewerToken(HttpContext context, string marketCode)
   {
      return new NewTokenResponse()
      {
         MarketCode = marketCode,
         Token = JwtBuilder
            .CreateNewBuilder()
            .SetMarket(marketCode)
            .Build()
      };
   }
   
   public static NewTokenResponse RequestPlayerToken(HttpContext context, string marketCode, string name)
   {
      return new NewTokenResponse()
      {
         MarketCode = marketCode,
         Name = name,
         Token = JwtBuilder
            .CreateNewBuilder()
            .SetMarket(marketCode)
            .SetUser(name)
            .Build()
      };
   }
}

public class NewTokenResponse
{
   public string Token { get; set; }
   public string MarketCode { get; set; }
   public string Name { get; set; }
   public bool IsAdmin { get; set; }
}