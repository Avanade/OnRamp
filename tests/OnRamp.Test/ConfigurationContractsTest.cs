using NUnit.Framework;
using OnRamp.Config;
using OnRamp.Test.Config;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnRamp.Test
{
    /// <summary>
    /// Verifies configuration default factories, type relationships, preparation, and property access.
    /// </summary>
    [TestFixture]
    public class ConfigurationContractsTest
    {
        /// <summary>
        /// Verifies every default factory is evaluated lazily and exactly once when required.
        /// </summary>
        [Test]
        public void DefaultFactoriesAreLazyAcrossAllSupportedTypes()
        {
            var calls = 0;
            Assert.That(ConfigBase.DefaultWhereNull("present", () => { calls++; return "fallback"; }), Is.EqualTo("present"));
            Assert.That(ConfigBase.DefaultWhereNull((int?)0, () => { calls++; return 7; }), Is.Zero);
            Assert.That(ConfigBase.DefaultWhereNull((decimal?)0m, () => { calls++; return 1.5m; }), Is.Zero);
            Assert.That(ConfigBase.DefaultWhereNull((bool?)false, () => { calls++; return true; }), Is.False);
            Assert.That(calls, Is.Zero);
            Assert.That(ConfigBase.DefaultWhereNull((string)null, () => { calls++; return "fallback"; }), Is.EqualTo("fallback"));
            Assert.That(ConfigBase.DefaultWhereNull((int?)null, () => { calls++; return 7; }), Is.EqualTo(7));
            Assert.That(ConfigBase.DefaultWhereNull((decimal?)null, () => { calls++; return 1.5m; }), Is.EqualTo(1.5m));
            Assert.That(ConfigBase.DefaultWhereNull((bool?)null, () => { calls++; return true; }), Is.True);
            Assert.That(calls, Is.EqualTo(4));
        }

        /// <summary>
        /// Verifies missing factories are rejected only when a fallback is needed.
        /// </summary>
        [Test]
        public void NullDefaultFactoriesFailOnlyWhenNeeded()
        {
            Assert.That(ConfigBase.DefaultWhereNull("present", null), Is.EqualTo("present"));
            Assert.That(ConfigBase.DefaultWhereNull((int?)1, null), Is.EqualTo(1));
            Assert.That(ConfigBase.DefaultWhereNull((decimal?)1m, null), Is.EqualTo(1m));
            Assert.That(ConfigBase.DefaultWhereNull((bool?)false, null), Is.False);
            Assert.That(Assert.Throws<ArgumentNullException>((System.Action)(() => ConfigBase.DefaultWhereNull((string)null, null))).ParamName, Is.EqualTo("defaultValue"));
            Assert.That(Assert.Throws<ArgumentNullException>((System.Action)(() => ConfigBase.DefaultWhereNull((int?)null, null))).ParamName, Is.EqualTo("defaultValue"));
            Assert.That(Assert.Throws<ArgumentNullException>((System.Action)(() => ConfigBase.DefaultWhereNull((decimal?)null, null))).ParamName, Is.EqualTo("defaultValue"));
            Assert.That(Assert.Throws<ArgumentNullException>((System.Action)(() => ConfigBase.DefaultWhereNull((bool?)null, null))).ParamName, Is.EqualTo("defaultValue"));
        }

        /// <summary>
        /// Verifies integer and decimal comparison helpers distinguish null, equal, and unequal values.
        /// </summary>
        [TestCase(null, 2, true)]
        [TestCase(2, 2, true)]
        [TestCase(1, 2, false)]
        public void NumericComparisonsHandleNullAndEquality(int? value, int other, bool nullOrEqual)
        {
            Assert.That(ConfigBase.CompareNullOrValue(value, other), Is.EqualTo(nullOrEqual));
            Assert.That(ConfigBase.CompareNullOrValue((decimal?)value, other), Is.EqualTo(nullOrEqual));
        }

        /// <summary>
        /// Verifies open and closed generic base types are recognized without accepting unrelated types.
        /// </summary>
        [TestCase(typeof(ConfigBase), typeof(EntityConfig), true)]
        [TestCase(typeof(ConfigRootBase<>), typeof(EntityConfig), true)]
        [TestCase(typeof(ConfigBase<,>), typeof(PropertyConfig), true)]
        [TestCase(typeof(EntityConfig), typeof(EntityConfig), true)]
        [TestCase(typeof(ConfigRootBase<>), typeof(PropertyConfig), false)]
        [TestCase(typeof(ConfigBase), typeof(string), false)]
        public void SubclassDetectionHandlesGenericHierarchy(Type baseType, Type type, bool expected) =>
            Assert.That(ConfigBase.IsSubclassOfBaseType(baseType, type), Is.EqualTo(expected));

        /// <summary>
        /// Verifies missing type arguments are rejected with contextual parameter names.
        /// </summary>
        [Test]
        public void SubclassDetectionRejectsNullTypes()
        {
            Assert.That(Assert.Throws<ArgumentNullException>((System.Action)(() => ConfigBase.IsSubclassOfBaseType(null, typeof(EntityConfig)))).ParamName, Is.EqualTo("baseType"));
            Assert.That(Assert.Throws<ArgumentNullException>((System.Action)(() => ConfigBase.IsSubclassOfBaseType(typeof(ConfigBase), null))).ParamName, Is.EqualTo("type"));
        }

        /// <summary>
        /// Verifies preparation assigns root and parent links and applies child defaults without duplicating items.
        /// </summary>
        [Test]
        public async Task PreparationConnectsHierarchyAndIsRepeatable()
        {
            var property = new PropertyConfig { Name = "Age" };
            var root = new EntityConfig { Name = "Person", Properties = new List<PropertyConfig> { property } };
            await root.PrepareAsync(root, root);
            await root.PrepareAsync(root, root);
            Assert.That(root.Root, Is.SameAs(root));
            Assert.That(root.Parent, Is.SameAs(root));
            Assert.That(root.Properties.Count, Is.EqualTo(1));
            Assert.That(root.Properties[0], Is.SameAs(property));
            Assert.That(property.Root, Is.SameAs(root));
            Assert.That(property.Parent, Is.SameAs(root));
            Assert.That(property.Type, Is.EqualTo("string"));
            Assert.That(property.BuildFullyQualifiedName("Type"), Is.EqualTo("Property(Name='Age').Type"));
        }

        /// <summary>
        /// Verifies absent child collections become empty during explicit preparation.
        /// </summary>
        [Test]
        public async Task PreparationInitializesNullCollections()
        {
            var root = new EntityConfig { Name = "Person" };
            await root.PrepareAsync(root, root);
            Assert.That(root.Properties, Is.Not.Null.And.Empty);
        }

        /// <summary>
        /// Verifies invalid configuration fails validation before child defaulting occurs.
        /// </summary>
        [TestCase(null, "Value is mandatory.")]
        [TestCase("Age", "Value 'unknown' is invalid")]
        public void PreparationRejectsInvalidChildrenBeforeDefaulting(string name, string message)
        {
            var root = new EntityConfig { Name = "Person" };
            var property = new PropertyConfig { Name = name, Type = "unknown" };
            root.Properties = new List<PropertyConfig> { property };
            var ex = Assert.ThrowsAsync<CodeGenException>(asyncCode: () => root.PrepareAsync(root, root));
            Assert.That(ex.Message, Does.Contain(message));
            Assert.That(property.Type, Is.EqualTo("unknown"));
        }

        /// <summary>
        /// Verifies duplicate child keys identify the offending property.
        /// </summary>
        [Test]
        public void PreparationRejectsDuplicateChildKeys()
        {
            var root = new EntityConfig
            {
                Name = "Person",
                Properties = new List<PropertyConfig> { new PropertyConfig { Name = "Age" }, new PropertyConfig { Name = "Age" } }
            };
            var ex = Assert.ThrowsAsync<CodeGenException>(asyncCode: () => root.PrepareAsync(root, root));
            Assert.That(ex.Message, Does.Contain("Property(Name='Age').Name").And.Contain("not unique"));
        }

        /// <summary>
        /// Verifies root and parent overrides require non-null references.
        /// </summary>
        [Test]
        public void HierarchyOverrideRejectsNullReferences()
        {
            var root = new EntityConfig();
            var property = new PropertyConfig();
            property.OverrideRootAndParent(root, root);
            Assert.That(property.Root, Is.SameAs(root));
            Assert.That(property.Parent, Is.SameAs(root));
            Assert.That(Assert.Throws<ArgumentNullException>((System.Action)(() => property.OverrideRootAndParent(null, root))).ParamName, Is.EqualTo("root"));
            Assert.That(Assert.Throws<ArgumentNullException>((System.Action)(() => property.OverrideRootAndParent(root, null))).ParamName, Is.EqualTo("parent"));
        }

        /// <summary>
        /// Verifies missing, explicit-null, and invalidly typed extension data are distinct.
        /// </summary>
        [Test]
        public void ExtraPropertiesPreserveNullAndRejectInvalidTypes()
        {
            var root = new EntityConfig();
            Assert.That(root.GetExtraProperty("missing", 7), Is.EqualTo(7));
            Assert.That(root.TryGetExtraProperty<int>("missing", out var missing), Is.False);
            Assert.That(missing, Is.Zero);
            using var json = JsonDocument.Parse("{\"empty\":null,\"count\":42,\"wrong\":\"text\"}");
            root.ExtraProperties = new Dictionary<string, JsonElement>();
            foreach (var property in json.RootElement.EnumerateObject())
                root.ExtraProperties.Add(property.Name, property.Value.Clone());
            Assert.That(root.TryGetExtraProperty<string>("empty", out var empty), Is.True);
            Assert.That(empty, Is.Null);
            Assert.That(root.GetExtraProperty("empty", "fallback"), Is.Null);
            Assert.That(root.GetExtraProperty<int>("count"), Is.EqualTo(42));
            Assert.Throws<JsonException>((System.Action)(() => root.GetExtraProperty<int>("wrong")));
        }

        /// <summary>
        /// Verifies custom and runtime parameter conversions fail visibly for invalid input.
        /// </summary>
        [Test]
        public void RuntimeAndCustomPropertiesExposeConversionErrors()
        {
            var root = new EntityConfig();
            root.CustomProperties.Add("count", "42");
            root.RuntimeParameters.Add("count", "42");
            Assert.That(root.GetCustomProperty<int>("count"), Is.EqualTo(42));
            Assert.That(root.TryGetCustomProperty<int>("count", out var count), Is.True);
            Assert.That(count, Is.EqualTo(42));
            Assert.That(root.GetRuntimeParameter<int>("count"), Is.EqualTo(42));
            root.CustomProperties["count"] = "invalid";
            root.RuntimeParameters["count"] = "invalid";
            Assert.Throws<FormatException>((System.Action)(() => root.GetCustomProperty<int>("count")));
            Assert.Throws<FormatException>((System.Action)(() => root.TryGetCustomProperty<int>("count", out _)));
            Assert.Throws<FormatException>((System.Action)(() => root.GetRuntimeParameter<int>("count")));
            Assert.Throws<FormatException>((System.Action)(() => root.TryGetRuntimeParameter<int>("count", out _)));
        }
    }
}
