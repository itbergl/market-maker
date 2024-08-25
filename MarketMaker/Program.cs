using MarketMaker.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHostedService<RequestProcessorService>();
builder.Services.AddScoped<RequestProcessorService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();