var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddControllers();

var app = builder.Build();

Environment.SetEnvironmentVariable("DAPR_HTTP_PORT", "1500");
Environment.SetEnvironmentVariable("DAPR_GRPC_PORT", "54200");

app.MapControllers();

app.Run();