using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using MicroElements.Bootstrap;
using MicroElements.Configuration;
using MicroElements.Tests.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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

        [Test]
        public void GetConfigurationVariants()
        {
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Bootstrap",
                Profile = null
            };
            var serviceProvider = new ApplicationBuilder().BuildAndStart(startupOptions);

            var optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<SampleOptions>>();
            var options = serviceProvider.GetRequiredService<IOptions<SampleOptions>>();
            var sampleOptionsAsInterface = serviceProvider.GetRequiredService<ISampleOptions>();
            var sampleOptions = serviceProvider.GetRequiredService<SampleOptions>();
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
        public void placeholders_should_be_replaced_with_empty()
        {
            var startupOptions = new StartupConfiguration
            {
                BeginConfiguration = builder => builder.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("Configuration:Property", "Value"),
                    new KeyValuePair<string, string>("Configuration:PropertyWithPlaceholder", "${configurationValue:Placeholders:Value}"),
                }),
                CommandLineArgs = new CommandLineArgs(new[] { "--Placeholders:OtherValue", "ValueFromPlaceholder" }),
                //Not evaluated value should be null or empty string
                //ReplaceNotEvaluatedValuesWith = ""
            };
            var serviceProvider = new ApplicationBuilder().Build(startupOptions).ServiceProvider;

            var configuration = serviceProvider.GetService<IConfiguration>();
            configuration.Should().NotBeNull();
            configuration["Configuration:Property"].Should().Be("Value");
            configuration["Configuration:PropertyWithPlaceholder"].Should().Be("");
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

        [Test]
        public void Complex1()
        {
            /*
            {
                "ComplexObjectConfiguration": {
                    "Name": "Complex",
                    "Inner": {
                        "Name": "InnerName",
                        "Value": "InnerValue"
                    }
                }
            }
            */
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Complex/complex1",
                ConfigurationTypes = new[] { typeof(ComplexObject) }
            };
            var serviceProvider = new ApplicationBuilder().BuildAndStart(startupOptions);

            var complexObject = serviceProvider.GetRequiredService<ComplexObject>();
            complexObject.Should().NotBeNull();
            complexObject.Name.Should().Be("Complex");
            complexObject.Inner.Should().NotBeNull();

            complexObject.Inner.Name.Should().Be("InnerName");
            complexObject.Inner.Value.Should().Be("InnerValue");
        }

        [Test]
        public void ComplexWithRef()
        {
            /*
            {
                "ComplexObject": {
                    "Name": "Complex",
                    "Inner": "${ref:InnerObject:First}"
                },
                "$objects": {
                    "InnerObject:First": {
                        "Name": "First",
                        "Value": "InnerValue"
                    },
                    "InnerObject:Second": {
                        "Name": "Second",
                        "Value": "InnerValue2"
                    }
                }
            }
            */
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Complex/complex2",
                ConfigurationTypes = new[] { typeof(ComplexObject), typeof(InnerObject) },
                ServiceCollection = new ServiceCollection()
            };

            var readAllText = File.ReadAllText("TestsConfiguration/Complex/complex2/config.json");
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                ReferenceResolverProvider = () => new ReferenceResolver(readAllText)
            };
            var deserializeObject = JsonConvert.DeserializeObject<ComplexObjectContainer>(readAllText, jsonSerializerSettings);

            startupOptions.ServiceCollection.AddSingleton<IConfigureOptions<ComplexObject>>(
                provider => new ConfigureRefProperty<ComplexObject>(provider, "Inner", typeof(InnerObject), "Second"));

            var serviceProvider = new ApplicationBuilder().Build(startupOptions);

            var innerObject = serviceProvider.GetService<IEnumerable<InnerObject>>();
            var complexObject = serviceProvider.GetRequiredService<ComplexObject>();
            complexObject.Should().NotBeNull();
            complexObject.Name.Should().Be("Complex");
            complexObject.Inner.Should().NotBeNull();

            complexObject.Inner.Name.Should().Be("InnerName");
            complexObject.Inner.Value.Should().Be("InnerValue");
        }

        class ReferenceResolver : IReferenceResolver
        {
            JObject _jObject;
            public ReferenceResolver(string text)
            {
                _jObject = (JObject)JsonConvert.DeserializeObject(text);
            }

            /// <inheritdoc />
            public object ResolveReference(object context, string reference)
            {
                //JsonSerializerInternalReader
                var token = _jObject.SelectToken("objects").SelectToken("InnerObject:First");
                var deserializeObject = JsonConvert.DeserializeObject(token.ToString(), typeof(InnerObject));
                return deserializeObject;
                return new InnerObject() { Name = "aa", Value = "aa" };
            }

            /// <inheritdoc />
            public string GetReference(object context, object value)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public bool IsReferenced(object context, object value)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void AddReference(object context, string reference, object value)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void RegisterMultipleObjects()
        {
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Array",
                Profile = null
            };
            var serviceProvider = new ApplicationBuilder().BuildAndStart(startupOptions);

            var options = serviceProvider.GetRequiredService<IEnumerable<SampleOptions>>().ToArray();
            options.Length.Should().Be(2);
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
}

//todo: remove nunit or xunit
