var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();