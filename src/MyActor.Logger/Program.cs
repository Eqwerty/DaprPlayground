using MyActor.Logger.Actors;
using MyActor.Logger.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddActors(options => options.Actors.RegisterActor<LoggerActor>());
builder.Services.AddSingleton<ISystemClock, SystemClock>();

var app = builder.Build();

if (app.Environment.IsEnvironment("Tests"))
{
    var httpPort = builder.Configuration.GetValue<string>("environmentVariables:daprHttpPort");
    var grpcPort = builder.Configuration.GetValue<string>("environmentVariables:daprGrpcPort");
    Environment.SetEnvironmentVariable("DAPR_HTTP_PORT", httpPort);
    Environment.SetEnvironmentVariable("DAPR_GRPC_PORT", grpcPort);
}

app.MapActorsHandlers();

app.Run();