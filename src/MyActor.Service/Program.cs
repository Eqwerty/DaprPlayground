var builder = WebApplication.CreateBuilder(args);

builder.Services.AddActors(options => options.Actors.RegisterActor<MyActor.Service.Actors.MyActor>());

var app = builder.Build();

app.MapActorsHandlers();

app.Run();