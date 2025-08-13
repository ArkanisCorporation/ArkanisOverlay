namespace Arkanis.Overlay.Common.Extensions;

using System.Globalization;
using Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Seq;
using Serilog.Templates;
using Serilog.Templates.Themes;

public static class DependencyInjection
{
    private static readonly CommonOptions DefaultOptions = new();

    public static IHostApplicationBuilder UseCommonServices(
        this IHostApplicationBuilder builder,
        Action<IHostEnvironment, CommonOptions>? configureOptions = null
    )
    {
        var options = new CommonOptions();

        configureOptions?.Invoke(builder.Environment, options);

        builder.Services.AddCommonServices(builder.Configuration, options);
        return builder;
    }

    public static IHostBuilder UseCommonServices(
        this IHostBuilder builder,
        Action<IHostEnvironment, CommonOptions>? configureOptions = null
    )
        => builder.ConfigureServices((ctx, services) =>
            {
                var options = new CommonOptions();

                configureOptions?.Invoke(ctx.HostingEnvironment, options);

                services.AddCommonServices(ctx.Configuration, options);
            }
        );


    private static IServiceCollection AddCommonServices(this IServiceCollection services, IConfiguration configuration, CommonOptions? options)
    {
        options ??= DefaultOptions;

        services.AddCommonLogging(configuration, options);

        return services;
    }

    private static IServiceCollection AddCommonLogging(this IServiceCollection services, IConfiguration configuration, CommonOptions options)
    {
        var logger = new LoggerConfiguration()
            .WriteTo.Async(
                a =>
                {
                    a.Console(
                        new ExpressionTemplate(
                            $"[{{@t:HH:mm:ss}} {{@l:u3}}] [{{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}}]{{@m}}{Environment.NewLine}{{@x}}",
                            CultureInfo.InvariantCulture,
                            applyThemeWhenOutputIsRedirected: true,
                            theme: TemplateTheme.Literate
                        )
                    );

                    if (options.UseFileLogging)
                    {
                        a.File(
                            formatter: new SeqCompactJsonFormatter(),
                            path: Path.Join(ApplicationConstants.ApplicationLogsDirectory.FullName, "app.log"),
                            buffered: true,
                            rollingInterval: RollingInterval.Day,
                            retainedFileCountLimit: 7
                        );
                    }

                    if (options.UseSeqLogging)
                    {
                        a.Seq(
                            serverUrl: "http://localhost:5341",
                            formatProvider: CultureInfo.InvariantCulture
                        );
                    }
                },
                // see: https://github.com/serilog/serilog-sinks-async?tab=readme-ov-file#blocking
                blockWhenFull: true
            )
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        services.AddSerilog(logger, true);

        AppDomain.CurrentDomain.UnhandledException += EnsureLoggerIsClosedAndFlushed;
        AppDomain.CurrentDomain.ProcessExit += EnsureLoggerIsClosedAndFlushed;

        return services;

        // When using async logging it is important to ensure that the logger is closed and flushed
        // before the process exits.
        // Otherwise, we may lose log messages.
        // see: https://github.com/serilog/serilog-sinks-async?tab=readme-ov-file#getting-started
        void EnsureLoggerIsClosedAndFlushed(object? o, EventArgs eventArgs)
        {
            Log.Information("[CommonLogging] Process exit, shutting down logger");
            if (Log.Logger != logger)
            {
                // Can be indicative of a misconfiguration or early flush
                Log.Debug("[CommonLogging] Global logger instance is not ours - CloseAndFlush might already have been called elsewhere!");
            }

            Log.CloseAndFlush();
        }
    }

    public static IServiceCollection AddConfiguration<T>(this IServiceCollection services, IConfiguration configuration) where T : class, ISelfBindableOptions
        => services.Configure<T>(instance => instance.Bind(configuration));

    public static IServiceCollection Alias<TAlias, TService>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TService : class, TAlias
        => services.UnsafeAlias<TAlias>(typeof(TService), lifetime);

    public static IServiceCollection UnsafeAlias<TAlias>(
        this IServiceCollection services,
        Type targetServiceType,
        ServiceLifetime lifetime = ServiceLifetime.Singleton
    )
    {
        var serviceDescriptor = ServiceDescriptor.Describe(
            typeof(TAlias),
            provider =>
            {
                var requiredService = provider.GetService(targetServiceType);
                if (requiredService is not TAlias service)
                {
                    throw new InvalidOperationException(
                        $"Unable to resolve aliased service {typeof(TAlias)} from {targetServiceType},"
                        + $" because the actual service instance {requiredService?.GetType().ToString() ?? "<null>"} is not assignable to {typeof(TAlias)}."
                    );
                }

                return service;
            },
            lifetime
        );
        services.Add(serviceDescriptor);

        return services;
    }

    public static IServiceCollection AliasVia<TAlias, TRegistered, TService>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Singleton
    )
        where TService : class, TAlias, TRegistered
        where TRegistered : notnull
    {
        var serviceDescriptor = ServiceDescriptor.Describe(
            typeof(TAlias),
            provider =>
            {
                var requiredService = provider.GetService<TRegistered>();
                if (requiredService is not TService service)
                {
                    throw new InvalidOperationException(
                        $"Unable to resolve aliased service {typeof(TAlias)} from {typeof(TService)} registered as {typeof(TRegistered)},"
                        + $" because the actual service instance {requiredService?.GetType().ToString() ?? "<null>"} is not assignable to {typeof(TService)}."
                    );
                }

                return service;
            },
            lifetime
        );
        services.Add(serviceDescriptor);

        return services;
    }
}

public record CommonOptions
{
    public bool UseFileLogging { get; set; }
    public bool UseSeqLogging { get; set; }
}
