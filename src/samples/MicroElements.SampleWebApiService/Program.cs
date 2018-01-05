using System;
using System.IO;
using System.Reflection;
using MicroElements.Bootstrap;
using MicroElements.Bootstrap.Extensions.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroElements.SampleWebApiService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

        public static IWebHostBuilder CreateDefaultBuilder(string[] args)
        {
            return new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseMicroElements(new StartupConfiguration())
                .ConfigureAppConfiguration((hostingContext, config) => InitializeAppConfiguration(args, hostingContext, config))
                .ConfigureLogging((hostingContext, logging) => InitializeLogging(logging, hostingContext))
                .UseIISIntegration()
                .UseDefaultServiceProvider((context, options) => options.ValidateScopes = context.HostingEnvironment.IsDevelopment());
        }

        private static void InitializeAppConfiguration(string[] args, WebHostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            var hostingEnvironment = hostingContext.HostingEnvironment;
            config.AddJsonFile("appsettings.json", true, true).AddJsonFile(
                string.Format("appsettings.{0}.json", hostingEnvironment.EnvironmentName), true, true);
            if (hostingEnvironment.IsDevelopment())
            {
                var assembly = Assembly.Load(new AssemblyName(hostingEnvironment.ApplicationName));
                if (assembly != null)
                    config.AddUserSecrets(assembly, true);
            }
            config.AddEnvironmentVariables();
            if (args == null)
                return;
            config.AddCommandLine(args);
        }

        private static void InitializeLogging(ILoggingBuilder logging, WebHostBuilderContext hostingContext)
        {
            logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
            logging.AddConsole();
            logging.AddDebug();
        }
    }

    public static class Ext
    {

        public static IWebHostBuilder UseMicroElements(this IWebHostBuilder hostBuilder, StartupConfiguration startupConfiguration)
        {
            if (startupConfiguration == null)
                throw new ArgumentNullException(nameof(startupConfiguration));

            hostBuilder.ConfigureAppConfiguration(delegate (WebHostBuilderContext context, IConfigurationBuilder builder)
            {
                // todo: интеграция с AspNetCore EnvironmentName
                context.HostingEnvironment.EnvironmentName = startupConfiguration.Profile;
            });
            hostBuilder.UseApplicationInsights();//вместо этого можно использовать serviceCollection.Add with IConfiguration

            hostBuilder.ConfigureServices(collection => ConfigureServices(hostBuilder, collection, startupConfiguration));

            return hostBuilder;
        }

        private static void ConfigureServices(IWebHostBuilder hostBuilder, IServiceCollection serviceCollection, StartupConfiguration startupConfiguration)
        {
            startupConfiguration.ServiceCollection = serviceCollection;
            var buildContext = new ApplicationBuilder().Build(startupConfiguration);
            hostBuilder.UseConfiguration(buildContext.ConfigurationRoot);
        }
    }
}
