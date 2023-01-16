var builder = WebApplication.CreateBuilder(args);

builder.Services.AddActors(options => options.Actors.RegisterActor<MyActor.Service.Actors.MyActor>());

var app = builder.Build();

Environment.SetEnvironmentVariable("DAPR_HTTP_PORT", "1501");
Environment.SetEnvironmentVariable("DAPR_GRPC_PORT", "54201");

app.MapActorsHandlers();

app.Run();