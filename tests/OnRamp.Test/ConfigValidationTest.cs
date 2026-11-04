using NUnit.Framework;
using OnRamp.Config;
using OnRamp.Test.Config;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnRamp.Test
{
    [TestFixture]
    public class ConfigValidationTest
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t\r\n")]
        [TestCase("\u00a0\u2003")]
        public void RequireTextRejectsBlankValuesWithContext(string value)
        {
            var config = new EntityConfig();
            var exception = Assert.Throws<CodeGenException>((Action)(() => config.RequireText("Version", value)));
            Assert.That(exception.Message, Is.EqualTo($"[{config.BuildFullyQualifiedName("Version")}] Value must contain non-whitespace text."));
        }

        [TestCase("value")]
        [TestCase(" value \t")]
        public void RequireTextPreservesOriginalValue(string value) =>
            Assert.That(new EntityConfig().RequireText("Version", value), Is.SameAs(value));

        [Test]
        public void RequireTextRejectsNullPropertyNameEvenWhenValueWouldPass()
        {
            Assert.That(Assert.Throws<ArgumentNullException>((Action)(() => new EntityConfig().RequireText(null, "valid"))).ParamName, Is.EqualTo("propertyName"));
        }

        [Test]
        public void RejectUnknownPropertiesAllowsNullAndEmpty()
        {
            var config = new EntityConfig();
            Assert.DoesNotThrow((Action)(() => config.RejectUnknownProperties()));
            config.ExtraProperties = new Dictionary<string, JsonElement>();
            Assert.DoesNotThrow((Action)(() => config.RejectUnknownProperties()));
        }

        [TestCase("z", "a", "Z")]
        [TestCase("a", "Z", "z")]
        public void RejectUnknownPropertiesUsesOrdinalOrder(string first, string second, string third)
        {
            var config = new EntityConfig
            {
                ExtraProperties = new Dictionary<string, JsonElement>
                {
                    [first] = default,
                    [second] = default,
                    [third] = default
                }
            };
            var exception = Assert.Throws<CodeGenException>((Action)(() => config.RejectUnknownProperties()));
            Assert.That(exception.Message, Is.EqualTo($"[{config.BuildFullyQualifiedName("Z")}] Unknown property."));
            Assert.That(config.ExtraProperties.Count, Is.EqualTo(3));
        }

        [TestCase("")]
        [TestCase(" \t")]
        public async Task PreparationStillAllowsBlankMandatoryTextAndExtensionProperties(string name)
        {
            var config = new EntityConfig
            {
                Name = name,
                ExtraProperties = new Dictionary<string, JsonElement> { ["extension"] = default }
            };
            await config.PrepareAsync(config, config);
            Assert.That(config.Name, Is.EqualTo(name));
            Assert.That(config.ExtraProperties.ContainsKey("extension"), Is.True);
        }

        [TestCase(0L)]
        [TestCase(-1L)]
        [TestCase(long.MinValue)]
        [TestCase(long.MaxValue)]
        public void LongDefaultPreservesSuppliedValuesWithoutCallingFactory(long value)
        {
            Assert.That(ConfigBase.DefaultWhereNull((long?)value, () => throw new InvalidOperationException()), Is.EqualTo(value));
            Assert.That(ConfigBase.DefaultWhereNull((long?)value, null), Is.EqualTo(value));
        }

        [Test]
        public void LongDefaultCallsFactoryExactlyOnceAndAllowsNullResult()
        {
            var calls = 0;
            Assert.That(ConfigBase.DefaultWhereNull((long?)null, () => { calls++; return long.MaxValue; }), Is.EqualTo(long.MaxValue));
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(ConfigBase.DefaultWhereNull((long?)null, () => null), Is.Null);
            Assert.That(Assert.Throws<ArgumentNullException>((Action)(() => ConfigBase.DefaultWhereNull((long?)null, null))).ParamName, Is.EqualTo("defaultValue"));
            Assert.Throws<InvalidOperationException>((Action)(() => ConfigBase.DefaultWhereNull((long?)null, () => throw new InvalidOperationException())));
        }
    }
}
