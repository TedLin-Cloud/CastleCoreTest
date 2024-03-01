using Autofac.Core;
using Castle.Core;
using Castle.Facilities.AspNetCore;
using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Services.Logging.SerilogIntegration;
using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using CastleCoreTest;
using CastleCoreTest.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

#region Serilog Setting
var serlogConfig = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft.Extensions.Hosting", LogEventLevel.Verbose)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
    .WriteTo.Logger(ms => ms.WriteTo.File(
                                @"logs\log-.txt",
                                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {RequestId}{NewLine}{Message}{NewLine}",
                                rollingInterval: RollingInterval.Day,
                                rollOnFileSizeLimit: true,
                                fileSizeLimitBytes: 1000000,
                                retainedFileCountLimit: 30
                            ))
    .CreateLogger();
#endregion Serilog

try
{
    // builder.Services.AddLogging(loggingBuilder =>
    //        loggingBuilder.AddSerilog(dispose: true));
    var container = new WindsorContainer();
    // container.Register(Component.For<IService>().ImplementedBy<MyService>().LifestyleTransient());

    // builder.Services.AddSingleton<IServiceProvider>(container.Kernel.Resolve<IServiceProvider>);
    // builder.Host.UseSerilog();
    
    container.AddFacility<LoggingFacility>(f => f.LogUsing(new SerilogFactory(serlogConfig)));
    builder.Services.AddWindsor(container, opts =>
    {
        opts.RegisterControllers(typeof(WeatherForecastController).Assembly, LifestyleType.Transient);
    });
    builder.Services.AddWindsor(container, opts => opts.UseEntryAssembly(typeof(WeatherForecastController).GetType().Assembly));

    // https://www.cnblogs.com/ljknlb/p/16953515.html
    // https://www.aspnets.cn/articleinfo/80.html
    builder.Services.AddInterceptedSingleton<IEncryptService, EncryptService, LogInterceptor>();
    builder.Services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}


//public class LoggerInstaller : IWindsorInstaller
//{
//    public void Install(IWindsorContainer container, IConfigurationStore store)
//    {
//        container.Register(Component.For<Serilog.ILogger>().Instance(Log.Logger).LifestyleSingleton());
//    }
//}