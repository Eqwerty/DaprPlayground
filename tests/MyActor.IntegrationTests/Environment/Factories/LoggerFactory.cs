using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyActor.IntegrationTests.Dapr;
using MyActor.Logger;
using MyActor.Logger.Services;
using NSubstitute;

namespace MyActor.IntegrationTests.Environment.Factories;

public class LoggerFactory : DaprServiceFactory<IMyActorLoggerMarker>
{
    public LoggerFactory() : base(DaprSettings.Logger)
    { }
}