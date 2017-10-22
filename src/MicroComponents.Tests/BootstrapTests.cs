using FluentAssertions;
using MicroComponents.Bootstrap;
using MicroComponents.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace MicroComponents.Tests
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
