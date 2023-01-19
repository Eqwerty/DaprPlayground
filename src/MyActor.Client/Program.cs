using Dapr.Actors.Client;
using Dapr.Actors.Runtime;
using Microsoft.Extensions.Options;
using MyActor.Client.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddControllers();

builder.Services.AddSingleton<IActorProxyFactory>(
    s =>
    {
        var options = s.GetRequiredService<IOptions<ActorRuntimeOptions>>().Value;
        var daprBaseConfig = s.GetRequiredService<IOptions<DaprConfig>>().Value;
        var factory = new ActorProxyFactory(
            new()
            {
                JsonSerializerOptions = options.JsonSerializerOptions, DaprApiToken = options.DaprApiToken, HttpEndpoint = options.HttpEndpoint
            });

        return new NamespaceActorProxyFactory(factory, daprBaseConfig);
    });

var app = builder.Build();

if (app.Environment.IsEnvironment("Tests"))
{
    var httpPort = builder.Configuration.GetValue<string>("environmentVariables:daprHttpPort");
    var grpcPort = builder.Configuration.GetValue<string>("environmentVariables:daprGrpcPort");
    Environment.SetEnvironmentVariable("DAPR_HTTP_PORT", httpPort);
    Environment.SetEnvironmentVariable("DAPR_GRPC_PORT", grpcPort);
}

app.MapControllers();

app.Run();