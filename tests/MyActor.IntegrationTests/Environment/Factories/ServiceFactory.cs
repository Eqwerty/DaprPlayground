﻿using MyActor.Service;

namespace MyActor.IntegrationTests.Environment.Factories;

public class ServiceFactory : DaprServiceFactory<IMyActorServiceMarker>
{
    public ServiceFactory() : base(Settings.Service)
    { }
}