using System.Text;
using MarketMaker.Endpoints;
using MarketMaker.Exchange;
using MarketMaker.ExchangeRules;
using MarketMaker.Hubs;
using MarketMaker.Services;
using MarketMaker.SharedComponents;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("web-ui", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins("http://localhost:3000");
    });
});

builder.Services.AddHostedService<RequestProcessorService>();
builder.Services.AddScoped<RequestProcessorService>();

builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddSingleton<IResponder, LoggerResponder>();

builder.Services.AddSingleton<IRequestQueue, RequestQueue>();
builder.Services.AddSingleton<ExchangeService>();
builder.Services.AddTransient<Exchange>();
builder.Services.AddTransient<IMarket, Market>();
builder.Services.AddSignalR();

builder.Services.AddTransient<IMarketNameProvider, MarketNameProvider>();

builder.Services.AddTransient<MarketState>();
builder.Services.AddTransient<IOrderValidator, AggregatorValidator>(); 
builder.Services.AddSingleton<MarketStateGetter>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminRequired", policy => policy.RequireClaim("isAdmin", "true"));
    options.AddPolicy("PlayerRequired", policy => policy.RequireClaim("name"));
    options.AddPolicy("ViewerRequired", policy => policy.RequireClaim("marketCode"));
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "market-maker",
        ValidAudience = "web-client",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("dflksjdf;alskj;223423j23j0001232321212312321321312321")),
        
    };

    options.Events = new JwtBearerEvents()
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/market"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("web-ui");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets(new WebSocketOptions()
{
});
app.UseAuthentication();
app.UseAuthorization();
app.AddTokenRequestEndpoints();
app.AddMarketEndpoints();

app.MapHub<MarketHub>("/market");
app.Run();


// /create-market -> adminToken, roomId
// /request-viewer-token (body: roomId) -> enterToken
// /request-participant-token (body: roomId, name) -> joinToken

// /request-leave-game?roomId={roomId}

// /update-market [auth token]
    // marketUpdate
    // 