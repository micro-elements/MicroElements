using System;
using System.Collections.Generic;
using FluentAssertions;
using MicroElements.Bootstrap;
using MicroElements.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace MicroElements.Tests
{
    public class BootstrapTests
    {
        //[OneTimeSetUp]
        //public void TestFixtureSetUp()
        //{
        //    var startupConfiguration = new StartupConfiguration
        //    {
        //        ConfigurationPath = "TestsConfiguration/Bootstrap",
        //        Profile = null
        //    };

        //    // Прогрев
        //    // new ApplicationBuilder().BuildAndStart(startupConfiguration);
        //}

        [Test(Description = "Читаем конфигурацию без профиля")]
        public void ReadTypedConfigurationWithNoProfile()
        {
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Bootstrap",
                Profile = null
            };
            var serviceProvider = new ApplicationBuilder().BuildAndStart(startupOptions);

            var sampleOptions = serviceProvider.GetService<SampleOptions>();
            sampleOptions.ShouldBeWithDefaultValues();
        }

        [Test(Description = "Чтение конфигурации с переопределением через профиль")]
        public void ReadTypedConfigurationWithProfile()
        {
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Bootstrap",
                Profile = "profile1"
            };
            var serviceProvider = new ApplicationBuilder().BuildAndStart(startupOptions);
            var sampleOptions = serviceProvider.GetService<SampleOptions>();

            sampleOptions.Value.Should().BeEquivalentTo("OverridenValueProfile1");
            sampleOptions.SharedValue.Should().BeEquivalentTo("SharedValue");
        }

        [Test(Description = "Читаем конфигурацию с заданием профиля через командную строку")]
        public void ReadTypedConfigurationWithProfileFromCommandLine()
        {
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Bootstrap",
                Profile = null,
                CommandLineArgs = new CommandLineArgs(new[] { "--profile", "profile1" })
            };
            var serviceProvider = new ApplicationBuilder().BuildAndStart(startupOptions);

            var sampleOptions = serviceProvider.GetService<SampleOptions>();

            sampleOptions.Value.Should().BeEquivalentTo("OverridenValueProfile1");
            sampleOptions.SharedValue.Should().BeEquivalentTo("SharedValue");
        }

        [Test(Description = "Чтение конфигурации в виде IOptions, IOptionsSnapshot")]
        public void ReadTypedConfigurationAsWrappers()
        {
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Bootstrap",
                Profile = null
            };
            var serviceProvider = new ApplicationBuilder().BuildAndStart(startupOptions);

            var sampleOptions = serviceProvider.GetService<IOptions<SampleOptions>>();
            sampleOptions.Value.ShouldBeWithDefaultValues();

            var sampleOptionsSnapshot = serviceProvider.GetService<IOptionsSnapshot<SampleOptions>>();
            sampleOptionsSnapshot.Value.ShouldBeWithDefaultValues();

            var sampleOptionsMonitor = serviceProvider.GetService<IOptionsMonitor<SampleOptions>>();
            sampleOptionsMonitor.CurrentValue.ShouldBeWithDefaultValues();
        }

        [Test(Description = "Чтение конфигурации с переопределением через профиль и подпрофиль")]
        public void ReadTypedConfigurationWithProfileAndSubProfile()
        {
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Bootstrap",
                Profile = "profile1/sub_profile1"
            };
            var serviceProvider = new ApplicationBuilder().BuildAndStart(startupOptions);
            var sampleOptions = serviceProvider.GetService<SampleOptions>();

            sampleOptions.SharedValue.Should().BeEquivalentTo("SharedValue");
            sampleOptions.Value.Should().BeEquivalentTo("OverridenValueSubProfile1");
            sampleOptions.OptionalValue.Should().BeEquivalentTo("OptionalValueSubProfile1");

            var startupOptions2 = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Bootstrap",
                Profile = "profile1/sub_profile2"
            };
            var serviceProvider2 = new ApplicationBuilder().BuildAndStart(startupOptions2);
            var sampleOptions2 = serviceProvider2.GetService<SampleOptions>();

            sampleOptions2.SharedValue.Should().BeEquivalentTo("SharedValue");
            sampleOptions2.Value.Should().BeEquivalentTo("OverridenValueSubProfile2");
            // Значение должно быть из профиля profile1, а не из sub_profile2 где его нет
            sampleOptions2.OptionalValue.Should().BeEquivalentTo("OptionalValueProfile1");
        }

        [Test(Description = "Чтение конфигурации в виде IOptions, IOptionsSnapshot")]
        public void TypedConfigurationCodeFirstPrototype()
        {
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Bootstrap",
                Profile = null
            };
            var serviceProvider = new ApplicationBuilder().BuildAndStart(startupOptions);
        }

        [Test(Description = "Чтение конфигурации с переопределением через профиль и подпрофиль")]
        public void ReadConfigurationWithInclude()
        {
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Bootstrap",
                Profile = "profile2"
            };
            var serviceProvider = new ApplicationBuilder().BuildAndStart(startupOptions);
            var sampleOptions = serviceProvider.GetService<SampleOptions>();

            sampleOptions.SharedValue.Should().BeEquivalentTo("SharedValueFromCommon");
            sampleOptions.Value.Should().BeEquivalentTo("OverridenByProfile2");
            sampleOptions.OptionalValue.Should().BeEquivalentTo(null);
        }

        [Test]
        public void ConfigurationPathCanBeNull()
        {
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = null,
                Profile = null,
            };

            Action action = () => new ApplicationBuilder().Build(startupOptions);
            action.ShouldNotThrow();
        }
        [Test]
        public void RegisterModules1()
        {
            var startupConfiguration = new StartupConfiguration
            {
                ConfigureModules = options => options.AutoDiscoverModules = false
            };
            var buildContext = new ApplicationBuilder().Build(startupConfiguration);
            var testModule1Service = buildContext.GetService<TestModule1Service>();
            testModule1Service.Should().BeNull();
        }

        [Test]
        public void RegisterModules2()
        {
            var serviceCollection = new ServiceCollection();

            var startupOptions = new StartupConfiguration
            {
                ServiceCollection = serviceCollection,
                Modules = new ModulesOptions
                {
                    ModuleTypes = new[] { typeof(TestModule1) }
                }
            };
            var buildContext = new ApplicationBuilder().Build(startupOptions);

            var testModule1Service = buildContext.GetService<TestModule1Service>();
            testModule1Service.Should().NotBeNull("testModule1Service had to be registered in TestModule1");
        }

        [Test]
        public void RegisterModulesWithInjectedValues()
        {
            var serviceCollection = new ServiceCollection();

            // Регистрируем конфигурацию.
            serviceCollection.AddSingleton(new TestModule2Configuration { ConfigurationValue = "Test" });

            var startupOptions = new StartupConfiguration
            {
                ServiceCollection = serviceCollection,
                Modules = new ModulesOptions
                {
                    ModuleTypes = new[] { typeof(TestModule1), typeof(TestModule2) }
                }
            };
            var buildContext = new ApplicationBuilder().Build(startupOptions);

            var testModule1Service = buildContext.GetService<TestModule1Service>();
            testModule1Service.Should().NotBeNull("testModule1Service had to be registered in TestModule1");

            var testModule2Service = buildContext.GetService<TestModule2Service>();
            testModule2Service.Should().NotBeNull("testModule2Service had to be registered in TestModule2");
            testModule2Service.ConfigurationValue.Should().Be("Test");
        }

        [Test]
        public void ServiceCollection_IConfiguration_should_be_registered()
        {
            var serviceCollection = new ServiceCollection();
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Bootstrap",
                Profile = "profile1",
                ServiceCollection = serviceCollection
            };
            new ApplicationBuilder().Build(startupOptions);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var sampleOptions = serviceProvider.GetService<SampleOptions>();
            sampleOptions.Should().NotBeNull();

            var configuration = serviceProvider.GetService<IConfiguration>();
            configuration.Should().NotBeNull();

            var configurationRoot = serviceProvider.GetService<IConfigurationRoot>();
            configurationRoot.Should().NotBeNull();
        }

        [Test]
        public void placeholders_should_be_replaced_from_file()
        {
            var serviceCollection = new ServiceCollection();
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Bootstrap",
                Profile = "placeholders",
                ServiceCollection = serviceCollection
            };
            new ApplicationBuilder().Build(startupOptions);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var sampleOptions = serviceProvider.GetService<SampleOptions>();
            sampleOptions.Should().NotBeNull();
            sampleOptions.Value.Should().Be("ValueFromPlaceholder");
        }

        [Test]
        public void placeholders_should_be_replaced_from_command_line()
        {
            var startupOptions = new StartupConfiguration
            {
                BeginConfiguration = builder => builder.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("Configuration:Property", "Value"),
                    new KeyValuePair<string, string>("Configuration:PropertyWithPlaceholder", "${configurationValue:Placeholders:Value}"),
                }),
                CommandLineArgs = new CommandLineArgs(new[] { "--Placeholders:Value", "ValueFromPlaceholder" }),
            };
            var serviceProvider = new ApplicationBuilder().Build(startupOptions).ServiceProvider;

            var configuration = serviceProvider.GetService<IConfiguration>();
            configuration.Should().NotBeNull();
            configuration["Configuration:Property"].Should().Be("Value");
            configuration["Configuration:PropertyWithPlaceholder"].Should().Be("ValueFromPlaceholder");
        }

        [Test]
        [TestCase("${configurationValue:Placeholders:Value}", "ValueFromPlaceholder", TestName = "simple_placeholder")]
        [TestCase("${configurationValue:Placeholders.Value}", "ValueFromPlaceholder", TestName = "simple_placeholder_with_dot")]
        [TestCase("${configurationValue:Placeholders:Value} end", "ValueFromPlaceholder end", TestName = "placeholder_in_start_of_text")]
        [TestCase("Some text ${configurationValue:Placeholders:Value}", "Some text ValueFromPlaceholder", TestName = "placeholder_in_end_of_text")]
        [TestCase("Some text ${configurationValue:Placeholders:Value} end", "Some text ValueFromPlaceholder end", TestName = "placeholder_in_middle_of_text")]
        [TestCase("${configurationValue:Placeholders:Value} ${configurationValue:Placeholders:Value2}", "ValueFromPlaceholder ValueFromPlaceholder2", TestName = "several_placeholders")]
        public void placeholders_should_be_replaced(string valueWithPlaceholder, string resultValue)
        {
            var startupOptions = new StartupConfiguration
            {
                BeginConfiguration = builder => builder.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("Configuration:Property", "Value"),
                    new KeyValuePair<string, string>("Configuration:PropertyWithPlaceholder", valueWithPlaceholder),
                }),
                CommandLineArgs = new CommandLineArgs(new[] { "--Placeholders:Value", "ValueFromPlaceholder", "--Placeholders:Value2", "ValueFromPlaceholder2" }),
            };
            var serviceProvider = new ApplicationBuilder().Build(startupOptions).ServiceProvider;

            var configuration = serviceProvider.GetService<IConfiguration>();
            configuration.Should().NotBeNull();
            configuration["Configuration:PropertyWithPlaceholder"].Should().Be(resultValue);
        }

        [Test]
        public void Diagnostic_too_many_assemblies()
        {
            var testLoggerProvider = new TestLoggerProvider();
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Bootstrap",
                ConfigureLogging = () =>
                {
                    var loggerFactory = new LoggerFactory();
                    loggerFactory.AddProvider(testLoggerProvider);
                    return loggerFactory;
                }
            };
            new ApplicationBuilder().BuildAndStart(startupOptions);

            testLoggerProvider.Log.Should().Contain("Diagnostic: too many assemblies found. Specify AssemblyScanPatterns.");
        }
    }

    public class TestModule1 : IModule
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<TestModule1Service>();
        }
    }

    public class TestModule1Service
    {
    }

    public class TestModule2 : IModule
    {
        private readonly TestModule2Configuration _configuration;

        public TestModule2(TestModule2Configuration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new TestModule2Service(_configuration.ConfigurationValue));
        }
    }
    public class TestModule2Configuration
    {
        public string ConfigurationValue { get; set; }
    }

    public class TestModule2Service
    {
        public string ConfigurationValue { get; }

        public TestModule2Service(string configurationValue)
        {
            ConfigurationValue = configurationValue;
        }
    }

    public static class AssertExtensions
    {
        public static void ShouldBeWithDefaultValues(this SampleOptions sampleOptions)
        {
            sampleOptions.Value.Should().BeEquivalentTo("DefaultValue");
            sampleOptions.SharedValue.Should().BeEquivalentTo("SharedValue");
        }
    }

    public class SampleOptions
    {
        public string Value { get; set; }

        public string SharedValue { get; set; }

        public int? OptionalIntValue { get; set; }

        public string OptionalValue { get; set; }
    }
}
