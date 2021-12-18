﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Sqlbi.Bravo.Infrastructure;
using System;
using System.IO;
using System.Linq;

namespace Sqlbi.Bravo
{
    internal partial class Program
    {
        private static IHost CreateHost()
        {
            var hostBuilder = new HostBuilder();

            hostBuilder.UseContentRoot(Directory.GetCurrentDirectory());

            hostBuilder.ConfigureHostConfiguration((builder) =>
            {
                builder.SetBasePath(Directory.GetCurrentDirectory());
                //builder.AddJsonFile("hostsettings.json", optional: true);
                //builder.AddEnvironmentVariables(prefix: "CUSTOMPREFIX_");
                //builder.AddCommandLine(args);
            });

            hostBuilder.ConfigureAppConfiguration((HostBuilderContext hostingContext, IConfigurationBuilder config) =>
            {
                //var hostingEnvironment = hostingContext.HostingEnvironment;
                //var reloadConfigOnChange = hostingContext.Configuration.GetValue("hostBuilder:reloadConfigOnChange", defaultValue: true);

                //config.AddJsonFile($"appsettings.json", optional: true, reloadConfigOnChange);
                //config.AddJsonFile($"appsettings.{ hostingEnvironment.EnvironmentName }.json", optional: true, reloadConfigOnChange);

                //if (hostingEnvironment.IsDevelopment() && !string.IsNullOrEmpty(hostingEnvironment.ApplicationName))
                //{
                //    var assembly = Assembly.Load(new AssemblyName(hostingEnvironment.ApplicationName));
                //    if (assembly != null)
                //        config.AddUserSecrets(assembly, optional: true);
                //}

                //config.AddEnvironmentVariables();

                //if (args != null)
                //    config.AddCommandLine(args);
            });

            hostBuilder.ConfigureLogging((HostBuilderContext hostingContext, ILoggingBuilder logging) =>
            {
                logging.AddFilter<EventLogLoggerProvider>((LogLevel level) => level >= LogLevel.Warning);
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
                logging.AddEventSourceLogger();
                logging.AddEventLog();

                logging.Configure((LoggerFactoryOptions options) =>
                {
                    options.ActivityTrackingOptions = ActivityTrackingOptions.SpanId | ActivityTrackingOptions.TraceId | ActivityTrackingOptions.ParentId;
                });
            });

            hostBuilder.UseDefaultServiceProvider((HostBuilderContext context, ServiceProviderOptions options) =>
            {
                options.ValidateOnBuild = (options.ValidateScopes = context.HostingEnvironment.IsDevelopment());
            });

            hostBuilder.ConfigureWebHostDefaults((webBuilder) =>
            {
                //webBuilder.ConfigureLogging(builder =>
                //{
                //    builder.
                //});

                webBuilder.ConfigureKestrel((options) =>
                {
                    // Allow sync IO - required by ImportVpax
                    options.AllowSynchronousIO = true;
                    
                    // TODO: randomise the listening port 
                    //var listenAddress = NetworkHelper.GetLoopbackAddress();
                    //options.Listen(listenAddress, port: 0, (listenOptions) =>
                    options.ListenLocalhost(port: 5000, (listenOptions) =>
                    {
#if DEBUG
                        listenOptions.UseConnectionLogging();
#endif
                        //listenOptions.UseHttps(); // TODO: do we need https ?
                    }); 
                });

                webBuilder.UseStartup<Startup>();
            });

            var host = hostBuilder.Build();
            return host;
        }

        /// <summary>
        /// Get the listening address and port used by the server
        /// </summary>
        /// <remarks>The port binding happens only when IWebHost.Run() is called and it is not accessible on Startup.Configure() because port has not been yet assigned on this stage.</remarks>
        /// <exception cref="BravoException" />
        private static Uri GetUri(IHost host)
        {
            var server = host.Services.GetRequiredService<IServer>();
            var feature = server.Features.Get<IServerAddressesFeature>() ?? throw new BravoException($"ServerFeature not found { nameof(IServerAddressesFeature) }");
            var address = feature.Addresses.Single(); // here a single address is expected so let the exception be raised in case of multiple addresses

            var uri = new Uri(address, UriKind.Absolute);
            return uri;
        }
    }
}
