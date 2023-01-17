var builder = WebApplication.CreateBuilder(args);

builder.Services.AddActors(options => options.Actors.RegisterActor<MyActor.Service.Actors.MyActor>());

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