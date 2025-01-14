﻿using System;
using DistributedTracingSample.AzureFunctions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

[assembly: FunctionsStartup(typeof(Startup))]
namespace DistributedTracingSample.AzureFunctions
{
    public class Startup : FunctionsStartup
    {
        private const string ServiceName = "zipkin-test";
        
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            // Registering Serilog provider
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            builder.Services.AddLogging(lb => lb.AddSerilog(logger));

            // Example from https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/examples/Console/TestConsoleExporter.cs
            var zipkinUri = "http://localhost:9411/api/v2/spans";

            var openTelemetry = Sdk.CreateTracerProviderBuilder()
                .AddSource(Functions.ActivitySourceName)
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(ServiceName))
                .AddZipkinExporter(o =>
                {
                    o.Endpoint = new Uri(zipkinUri);
                })
                .AddConsoleExporter()
                .Build();

            builder.Services.AddSingleton(openTelemetry);
        }
    }
}

