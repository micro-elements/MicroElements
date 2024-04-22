using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using FluentAssertions;
using MicroElements.Bootstrap;
using MicroElements.Configuration;
using MicroElements.Configuration.Evaluation;
using MicroElements.Tests.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace MicroElements.Tests
{
    public class BootstrapTests
    {
        [Test(Description = "Читаем конфигурацию без профиля")]
        public void ReadTypedConfigurationWithNoProfile()
        {
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Bootstrap",
                Profile = null
            };
            var serviceProvider = (IServiceProvider)new ApplicationBuilder().Build(startupOptions);

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
            var serviceProvider = (IServiceProvider)new ApplicationBuilder().Build(startupOptions);

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
            var serviceProvider = (IServiceProvider)new ApplicationBuilder().Build(startupOptions);
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
            var serviceProvider = (IServiceProvider)new ApplicationBuilder().Build(startupOptions);

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
            var serviceProvider = (IServiceProvider)new ApplicationBuilder().Build(startupOptions);

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
            var serviceProvider = (IServiceProvider)new ApplicationBuilder().Build(startupOptions);
            var sampleOptions = serviceProvider.GetService<SampleOptions>();

            sampleOptions.SharedValue.Should().BeEquivalentTo("SharedValue");
            sampleOptions.Value.Should().BeEquivalentTo("OverridenValueSubProfile1");
            sampleOptions.OptionalValue.Should().BeEquivalentTo("OptionalValueSubProfile1");

            var startupOptions2 = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Bootstrap",
                Profile = "profile1/sub_profile2"
            };
            var serviceProvider2 = (IServiceProvider)new ApplicationBuilder().Build(startupOptions2);
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
            var serviceProvider = (IServiceProvider)new ApplicationBuilder().Build(startupOptions);
        }

        [Test(Description = "Чтение конфигурации с переопределением через профиль и подпрофиль")]
        public void ReadConfigurationWithInclude()
        {
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Bootstrap",
                Profile = "profile2"
            };
            var serviceProvider = (IServiceProvider)new ApplicationBuilder().Build(startupOptions);
            var sampleOptions = serviceProvider.GetService<SampleOptions>();

            sampleOptions.SharedValue.Should().BeEquivalentTo("SharedValueFromCommon");
            sampleOptions.Value.Should().BeEquivalentTo("OverridenByProfile2");
            sampleOptions.OptionalValue.Should().BeEquivalentTo("OptionalValueFromCommonWithProfile2");
        }

        [Test(Description = "Чтение конфигурации с рекурсивным переопределением через профиль и подпрофиль")]
        public void ReadConfigurationWithRecursiveInclude()
        {
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Bootstrap/recursive_include_profile/entry_point",
                Profile = "production"
            };
            var serviceProvider = (IServiceProvider)new ApplicationBuilder().Build(startupOptions);
            var sampleOptions = serviceProvider.GetService<SampleOptions>();

            sampleOptions
                .Should()
                .BeEquivalentTo(
                    new SampleOptions
                    {
                        Value = "SampleOptions.Value",
                        SharedValue = "SampleOptions.SharedValue"
                    });
        }

        [Test(Description = "Динамическое обновление конфигурации")]
        public void ReloadConfigurationOnFileContentChanged()
        {
            const string rootFile = "TestsConfiguration/Bootstrap/reload_on_change_profile/entry_point/configuration.json";
            const string includedFile = "TestsConfiguration/Bootstrap/reload_on_change_profile/shared_resources/included_configuration.json";
            const string placeholderFile = "TestsConfiguration/Bootstrap/reload_on_change_profile/shared_resources/placeholders.json";

            // Restore original content.
            File.WriteAllText(rootFile, File.ReadAllText(rootFile).Replace("SampleOptions.ValueChanged", "SampleOptions.Value"));
            File.WriteAllText(includedFile, File.ReadAllText(includedFile).Replace("SampleOptions.SharedValueChanged", "SampleOptions.SharedValue"));
            File.WriteAllText(placeholderFile, File.ReadAllText(placeholderFile).Replace("Placeholder.OptionalValueChanged", "Placeholder.OptionalValue"));

            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Bootstrap/reload_on_change_profile/entry_point",
                Profile = "production"
            };
            var serviceProvider = (IServiceProvider)new ApplicationBuilder().Build(startupOptions);
            IOptionsSnapshot<SampleOptions> sampleOptions = serviceProvider.GetService<IOptionsSnapshot<SampleOptions>>();

            sampleOptions
                .Value
                .Should()
                .BeEquivalentTo(
                    new SampleOptions
                    {
                        Value = "SampleOptions.Value",
                        SharedValue = "SampleOptions.SharedValue",
                        OptionalValue = "Placeholder.OptionalValue"
                    });


            File.WriteAllText(rootFile, File.ReadAllText(rootFile).Replace("SampleOptions.Value", "SampleOptions.ValueChanged"));
            Thread.Sleep(1000);
            serviceProvider.CreateScope().ServiceProvider.GetService<IOptionsSnapshot<SampleOptions>>().Value.Value.Should().Be("SampleOptions.ValueChanged");

            File.WriteAllText(includedFile, File.ReadAllText(includedFile).Replace("SampleOptions.SharedValue", "SampleOptions.SharedValueChanged"));
            Thread.Sleep(1000);
            serviceProvider.CreateScope().ServiceProvider.GetService<IOptionsSnapshot<SampleOptions>>().Value.SharedValue.Should().Be("SampleOptions.SharedValueChanged");

            File.WriteAllText(placeholderFile, File.ReadAllText(placeholderFile).Replace("Placeholder.OptionalValue", "Placeholder.OptionalValueChanged"));
            Thread.Sleep(1000);
            serviceProvider.CreateScope().ServiceProvider.GetService<IOptionsSnapshot<SampleOptions>>().Value.OptionalValue.Should().Be("Placeholder.OptionalValueChanged");
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
            action.Should().NotThrow();
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
        [TestCase("${configurationValue:Placeholders.Value}", "", TestName = "simple_placeholder_with_dot")]
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

        class DictionaryEvaluator : IValueEvaluator
        {
            private readonly IDictionary<string, string> _propertyValues;

            public DictionaryEvaluator()
            {
                Info = new EvaluatorInfo("dict");
                _propertyValues = new Dictionary<string, string>();
            }

            public DictionaryEvaluator(string name, IDictionary<string, string> propertyValues, bool canEvaluateExpressions = false)
            {
                Info = new EvaluatorInfo(name) { IsUnevaluatedExpressionsAllowed = canEvaluateExpressions };
                _propertyValues = propertyValues;
            }

            /// <inheritdoc />
            public EvaluatorInfo Info { get; }

            /// <inheritdoc />
            public EvaluationResult Evaluate(EvaluationContext context)
            {
                _propertyValues.TryGetValue(context.Expression, out string value);
                if (value is null)
                {
                    string wrapped = UnwrapEvaluator.Wrap(context.Expression);
                    return EvaluationResult.Create(context, wrapped);
                }
                return EvaluationResult.Create(context, value);
            }
        }

        [Test]
        public void placeholders_with_recursion()
        {
            IValueEvaluator[] evaluators =
            {
                new DictionaryEvaluator("eval1", new Dictionary<string, string> {{ "Prop1", "Value1"}, { "Prop2", "Value2"}, { "Success", "Success!" } }),
                new DictionaryEvaluator("eval2", new Dictionary<string, string> {{ "Value1_Value2", "Success"}}),
            };

            SimpleExpressionParser
                .ParseAndRender(default, "${eval1:Prop1}_${eval1:Prop2}", evaluators)
                .Should().Be("Value1_Value2");

            SimpleExpressionParser
                .ParseAndRender(default, "${eval2:${eval1:Prop1}_${eval1:Prop2}}", evaluators)
                .Should().Be("Success");

            evaluators = evaluators.Reverse().ToArray();
            SimpleExpressionParser
                .ParseAndRender(default, "${eval2:${eval1:Prop1}_${eval1:Prop2}}", evaluators)
                .Should().Be("Success");

            SimpleExpressionParser
                .ParseAndRender(default, "${eval1:${eval2:${eval1:Prop1}_${eval1:Prop2}}}", evaluators)
                .Should().Be("Success!");
        }

        [Test]
        public void placeholders_with_recursion2()
        {
            IValueEvaluator[] evaluators =
            {
                new DictionaryEvaluator("eval1", new Dictionary<string, string> { { "Value2_Value3", "Success" } }),
                new DictionaryEvaluator("eval2", new Dictionary<string, string> { { "Prop2", "Value2"} }),
                new DictionaryEvaluator("eval3", new Dictionary<string, string> { { "Prop3", "Value3"} }),
            };

            SimpleExpressionParser
                .ParseAndRender(default, "${eval1:${eval2:Prop2}_${eval3:Prop3}}", evaluators)
                .Should().Be("Success");
        }

        [Test]
        public void placeholders_with_recursion3()
        {
            IValueEvaluator[] evaluators =
            {
                new DictionaryEvaluator("eval1", new Dictionary<string, string> { { "Value2_Value3", "Success" } }, canEvaluateExpressions: true),
                new DictionaryEvaluator("eval2", new Dictionary<string, string> { { "Prop2", "Value2"} }),
                new DictionaryEvaluator("eval3", new Dictionary<string, string> { { "Prop3", "Value3"} }),
            };

            SimpleExpressionParser
                .ParseAndRender(default, "${eval1:${eval2:Prop2}_${eval3:Prop3}}", evaluators)
                .Should().Be("${eval2:Prop2}_${eval3:Prop3}");
        }

        [Test]
        public void expression_with_error()
        {
            IValueEvaluator[] evaluators =
            {
                new DictionaryEvaluator("eval1", new Dictionary<string, string> {{ "Prop1", "Value1"}, { "Prop2", "Value2"}, { "Success", "Success!" } }),
                new DictionaryEvaluator("eval2", new Dictionary<string, string> {{ "Value1_Value2", "Success"}}),
            };

            SimpleExpressionParser
                .ParseAndRender(default, "${eval1:Prop1", evaluators)
                .Should().Be("${eval1:Prop1");
        }

        [Test]
        public void Diagnostic_too_many_assemblies()
        {
            var testLoggerProvider = new TestLoggerProvider();
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Bootstrap",
                ConfigureLogging = (s) =>
                {
                    var loggerFactory = new LoggerFactory();
                    loggerFactory.AddProvider(testLoggerProvider);
                    return loggerFactory;
                }
            };
            new ApplicationBuilder().Build(startupOptions);

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
            var serviceProvider = (IServiceProvider)new ApplicationBuilder().Build(startupOptions);

            var complexObject = serviceProvider.GetRequiredService<ComplexObject>();
            complexObject.Should().NotBeNull();
            complexObject.Name.Should().Be("Complex");
            complexObject.Inner.Should().NotBeNull();

            complexObject.Inner.Name.Should().Be("InnerName");
            complexObject.Inner.Value.Should().Be("InnerValue");
        }

        [Test]
        public void Complex2()
        {
            var startupOptions = new StartupConfiguration
            {
                ConfigurationPath = "TestsConfiguration/Complex/complex2",
                ConfigurationTypes = new[] { typeof(ComplexObject) }
            };
            var buildContext = new ApplicationBuilder().Build(startupOptions);
            buildContext.ConfigurationRoot["ComplexObject:UserName"].Should().Be("Second");
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
