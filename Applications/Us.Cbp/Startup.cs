using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Camunda.Worker;
using Camunda.Worker.Client;
using Camunda.Worker.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Us.Cbp.Services;

namespace Us.Cbp;

internal class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddExternalTaskClient(client =>
        {
            client.BaseAddress = new Uri(Configuration["TaskClientBaseAddress"] ?? throw new ArgumentException("No task client base address configured."));
        });

        string serviceType = Configuration["ServiceType"] ?? throw new ArgumentException("No service type configured.");
        Type service = LoadServiceClass(serviceType); // todo: rename to worker/handler
        ICamundaWorkerBuilder builder = services.AddCamundaWorker($"us-cbp-worker-{serviceType}");

        // Dynamic invocation of generic AddHandler<T> method requires some reflection
        MethodInfo? baseMethod = typeof(CamundaWorkerBuilderExtensions)
            .GetMethods()
            .First(method => method.Name == "AddHandler" && method.GetParameters().Length == 2 && method.GetParameters().Last().ParameterType == typeof(HandlerMetadata));
        MethodInfo genericMethod = baseMethod.MakeGenericMethod(service.UnderlyingSystemType)!;

        string? topics = Configuration["Topics"];
        string? variables = Configuration["Variables"];
        HandlerMetadata metaData = topics is not null ? new HandlerMetadata(topics.Split(",").ToList<string>()) : new HandlerMetadata(Array.Empty<string>());
        metaData.Variables = !string.IsNullOrWhiteSpace(variables) ? variables.Split(",") : Array.Empty<string>();
        builder = (genericMethod.Invoke(builder, new object[] { builder, metaData! }) as ICamundaWorkerBuilder) ?? throw new Exception("Unable to invoke AddHandler method.");

        builder
            .ConfigurePipeline(pipeline =>
            {
                pipeline.Use(next => async context =>
                {
                    var logger = context.ServiceProvider.GetRequiredService<ILogger<Startup>>();
                    logger.LogInformation("Started processing of task {Id}", context.Task.Id);
                    await next(context);
                    logger.LogInformation("Finished processing of task {Id}", context.Task.Id);
                });
            });

        Console.WriteLine("Finished setting up things.");
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        MailService.Configuration = Configuration;
    }

    private Type LoadServiceClass(string service)
    {
        Console.WriteLine($"Initializing service '{service}'...");

        Assembly assembly = Assembly.GetExecutingAssembly();
        List<Type> types = assembly
            .GetTypes()
            .Where(type => type.GetInterfaces().Contains(typeof(IExternalTaskHandler)))
            .ToList();

        Type? serviceType = types.SingleOrDefault(type => type.Name.ToLower().EndsWith(service.ToLower()));

        if (serviceType is not null && !string.IsNullOrWhiteSpace(serviceType.FullName))
        {
            return serviceType;
        }

        throw new InvalidOperationException($"Operation '{service}' not recognized.");
    }
}