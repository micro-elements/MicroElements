using System;
using System.IO;
using System.Reflection;
using MicroComponents.Bootstrap;
using MicroComponents.Bootstrap.Extensions.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroComponents.SampleWebApiService
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
                .UseMicroComponents(new StartupConfiguration())
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

        public static IWebHostBuilder UseMicroComponents(this IWebHostBuilder hostBuilder, StartupConfiguration startupConfiguration)
        {
            if (startupConfiguration == null)
                throw new ArgumentNullException(nameof(startupConfiguration));

      
            
            var buildContext = new ApplicationBuilder().Build(startupConfiguration);

            Action<IServiceCollection> configureServices;

            //hostBuilder.ConfigureAppConfiguration((context, builder) => { buildContext.ConfigurationRoot });
            hostBuilder.UseConfiguration(buildContext.ConfigurationRoot);

            hostBuilder.ConfigureServices(ConfigureServices1);
            hostBuilder.ConfigureServices(ConfigureServices2);
            //hostBuilder.ConfigureServices(collection => buildContext.ServiceCollection)

            return hostBuilder;
        }

        private static void ConfigureServices1(IServiceCollection serviceCollection)
        {
            throw new NotImplementedException();
        }

        private static void ConfigureServices2(WebHostBuilderContext webHostBuilderContext, IServiceCollection serviceCollection)
        {
            throw new NotImplementedException();
        }
    }
}
