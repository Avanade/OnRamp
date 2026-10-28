using OnRamp.Test.Config;
using OnRamp.Utility;
using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OnRamp.Test
{
    /// <summary>
    /// Verifies JSON schema generation, metadata validation, and reference compatibility.
    /// </summary>
    [TestFixture]
    public class JsonSchemaGeneratorTest
    {
        /// <summary>
        /// Verifies existing schema output against the golden file.
        /// </summary>
        [Test]
        public void Generate()
        {
            using var directory = new TestDirectory();
            var fn = Path.Combine(directory.DirectoryPath, "schema.json");

            JsonSchemaGenerator.Generate<EntityConfig>(fn, "Entity Configuration");

            Assert.That(File.Exists(fn), Is.True);
            Assert.That(File.ReadAllText(fn), Is.EqualTo(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Expected", "Schema.json"))));
        }

        /// <summary>
        /// Verifies nested and cyclic references, scalar unions, and typed defaults.
        /// </summary>
        [Test]
        public void GenerateNestedReferencesAndDefaults()
        {
            var json = ReferenceGenerationCases.Schema(typeof(CodeGenReferenceConfig));
            Assert.That(ReferenceGenerationCases.Schema(typeof(CodeGenReferenceConfig)), Is.EqualTo(json));
            using var document = JsonDocument.Parse(json);
            var definitions = document.RootElement.GetProperty("definitions");
            Assert.That(definitions.EnumerateObject().Select(x => x.Name), Is.EqualTo(new[] { "CodeGen", "Source", "File", "Column", "Validation" }));

            var root = definitions.GetProperty("CodeGen");
            var source = root.GetProperty("properties").GetProperty("source");
            Assert.That(source.GetProperty("title").GetString(), Is.EqualTo("Interface source."));
            Assert.That(source.GetProperty("description").GetString(), Is.EqualTo("Source details."));
            Assert.That(source.TryGetProperty("$ref", out _), Is.False);
            Assert.That(source.TryGetProperty("type", out _), Is.False);
            Assert.That(source.GetProperty("allOf")[0].GetProperty("$ref").GetString(), Is.EqualTo("#/definitions/Source"));
            Assert.That(root.GetProperty("required").EnumerateArray().Select(x => x.GetString()), Is.EqualTo(new[] { "source" }));
            Assert.That(definitions.GetProperty("Column").GetProperty("required")[0].GetString(), Is.EqualTo("validation"));
            Assert.That(root.GetProperty("properties").GetProperty("tags").GetProperty("items").GetProperty("type").GetString(), Is.EqualTo("string"));
            Assert.That(Types(root.GetProperty("properties").GetProperty("tags")), Is.EqualTo("array,null"));
            Assert.That(root.GetProperty("properties").GetProperty("tags").GetProperty("default").GetArrayLength(), Is.Zero);
            var secondary = root.GetProperty("properties").GetProperty("secondarySource");
            Assert.That(secondary.GetProperty("default").ValueKind, Is.EqualTo(JsonValueKind.Null));
            Assert.That(secondary.GetProperty("allOf")[0].GetProperty("$ref").GetString(), Is.EqualTo("#/definitions/Source"));
            Assert.That(secondary.TryGetProperty("$ref", out _), Is.False);
            Assert.That(definitions.GetProperty("File").GetProperty("properties").GetProperty("columns").GetProperty("items").GetProperty("$ref").GetString(), Is.EqualTo("#/definitions/Column"));

            var validation = definitions.GetProperty("Validation").GetProperty("properties");
            foreach (var name in new[] { "min", "max" })
                Assert.That(Types(validation.GetProperty(name)), Is.EqualTo("string,number"));
            Assert.That(validation.GetProperty("min").GetProperty("default").GetDouble(), Is.EqualTo(1.5));
            var options = validation.GetProperty("options");
            Assert.That(Types(options), Is.EqualTo("array,null"));
            Assert.That(Types(options.GetProperty("items")), Is.EqualTo("string,number,boolean"));
            Assert.That(options.GetProperty("default").GetArrayLength(), Is.Zero);

            var defaults = definitions.GetProperty("Source").GetProperty("properties");
            Assert.That(defaults.GetProperty("type").GetProperty("default").GetString(), Is.EqualTo("AzureServiceBus"));
            Assert.That(defaults.GetProperty("enabled").GetProperty("default").ValueKind, Is.EqualTo(JsonValueKind.True));
            Assert.That(defaults.GetProperty("disabled").GetProperty("default").ValueKind, Is.EqualTo(JsonValueKind.False));
            Assert.That(defaults.GetProperty("count").GetProperty("default").GetInt32(), Is.EqualTo(42));
            Assert.That(defaults.GetProperty("amount").GetProperty("default").GetDecimal(), Is.EqualTo(12.5m));
            Assert.That(defaults.GetProperty("empty").GetProperty("default").ValueKind, Is.EqualTo(JsonValueKind.Null));
            Assert.That(defaults.GetProperty("label").GetProperty("default").GetString(), Is.EqualTo("A|B`C\r\nD"));
            Assert.That(defaults.GetProperty("tags").GetProperty("default").ValueKind, Is.EqualTo(JsonValueKind.Null));
            Assert.That(defaults.GetProperty("tags").EnumerateObject().Count(x => x.Name == "default"), Is.EqualTo(1));
            Assert.That(source.TryGetProperty("default", out _), Is.False);
            AssertReferencesResolve(document.RootElement, document.RootElement);
        }

        /// <summary>
        /// Verifies mandatory collections require presence without changing nullability or array constraints.
        /// </summary>
        [Test]
        public void GenerateMandatoryCollectionsRequirePresenceOnly()
        {
            var json = ReferenceGenerationCases.Schema(typeof(CollectionRequirementsConfig));
            Assert.That(ReferenceGenerationCases.Schema(typeof(CollectionRequirementsConfig)), Is.EqualTo(json));
            using var document = JsonDocument.Parse(json);
            var definition = document.RootElement.GetProperty("definitions").GetProperty("Collections");
            Assert.That(definition.GetProperty("required").EnumerateArray().Select(x => x.GetString()),
                Is.EqualTo(new[] { "requiredObjects", "requiredScalars", "requiredStrings" }));

            var properties = definition.GetProperty("properties");
            foreach (var property in properties.EnumerateObject())
            {
                Assert.That(Types(property.Value), Is.EqualTo("array,null"), property.Name);
                Assert.That(property.Value.GetProperty("default").ValueKind, Is.EqualTo(JsonValueKind.Array), property.Name);
                Assert.That(property.Value.GetProperty("default").GetArrayLength(), Is.Zero, property.Name);
                Assert.That(property.Value.TryGetProperty("minItems", out _), Is.False, property.Name);
            }

            foreach (var name in new[] { "requiredObjects", "optionalObjects" })
                Assert.That(properties.GetProperty(name).GetProperty("items").GetProperty("$ref").GetString(), Is.EqualTo("#/definitions/Source"));
            foreach (var name in new[] { "requiredScalars", "optionalScalars" })
                Assert.That(Types(properties.GetProperty(name).GetProperty("items")), Is.EqualTo("string,number,boolean"));
            foreach (var name in new[] { "requiredStrings", "optionalStrings" })
                Assert.That(Types(properties.GetProperty(name).GetProperty("items")), Is.EqualTo("string"));

            Assert.That(document.RootElement.GetProperty("definitions").GetProperty("Source").TryGetProperty("required", out _), Is.False);
            AssertReferencesResolve(document.RootElement, document.RootElement);
        }

        /// <summary>
        /// Verifies that escaped configuration names produce resolvable references.
        /// </summary>
        [Test]
        public void GenerateEscapedReferences()
        {
            using var document = JsonDocument.Parse(ReferenceGenerationCases.Schema(typeof(EscapedNameRoot)));
            var value = document.RootElement.GetProperty("definitions").GetProperty("Root/~ #").GetProperty("properties").GetProperty("value");
            Assert.That(value.GetProperty("title").GetString(), Is.EqualTo("Recursive root."));
            Assert.That(value.GetProperty("description").GetString(), Is.EqualTo("Reference annotations."));
            Assert.That(value.TryGetProperty("$ref", out _), Is.False);
            AssertReferencesResolve(document.RootElement, document.RootElement);
        }

        /// <summary>
        /// Verifies explicit scalar compatibility and existing inferred mappings.
        /// </summary>
        [TestCaseSource(typeof(ReferenceGenerationCases), nameof(ReferenceGenerationCases.Metadata))]
        public void GenerateScalarMetadata(Type type, string expected)
        {
            if (expected == null)
            {
                var ex = Assert.Throws<InvalidOperationException>((System.Action)(() => ReferenceGenerationCases.Schema(type)));
                Assert.That(ex.Message, Does.Contain("Property 'Value'"));
                return;
            }

            using var document = JsonDocument.Parse(ReferenceGenerationCases.Schema(type));
            var definitions = document.RootElement.GetProperty("definitions");
            Assert.That(definitions.EnumerateObject().Count(), Is.EqualTo(1));
            var value = definitions.GetProperty("Scalar").GetProperty("properties").GetProperty("value");
            if (value.TryGetProperty("items", out var items))
            {
                Assert.That(Types(value), Is.EqualTo("array,null"));
                Assert.That(value.GetProperty("default").GetArrayLength(), Is.Zero);
                value = items;
            }
            Assert.That(Types(value), Is.EqualTo(expected));
        }

        /// <summary>
        /// Verifies contextual errors for invalid class metadata and unserializable defaults.
        /// </summary>
        [TestCase(typeof(CollisionRoot), "CodeGenClass.Name")]
        [TestCase(typeof(MissingClassRoot), "CodeGenClassAttribute")]
        [TestCase(typeof(NonFiniteDefault), "Property 'Value' has a default")]
        [TestCase(typeof(UnserializableDefault), "Property 'Value' has a default")]
        public void GenerateRejectsInvalidReferencesAndDefaults(Type type, string message)
        {
            var ex = Assert.Throws<InvalidOperationException>((System.Action)(() => ReferenceGenerationCases.Schema(type)));
            Assert.That(ex.Message, Does.Contain(message));
            if (type == typeof(NonFiniteDefault) || type == typeof(UnserializableDefault))
                Assert.That(ex.InnerException, Is.Not.Null);
        }

        /// <summary>
        /// Verifies that numeric defaults are serialized independently of the current culture.
        /// </summary>
        [Test]
        public void GenerateDefaultsIndependentlyOfCulture()
        {
            var original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
                using var document = JsonDocument.Parse(ReferenceGenerationCases.Schema(typeof(CodeGenReferenceConfig)));
                Assert.That(document.RootElement.GetProperty("definitions").GetProperty("Source").GetProperty("properties").GetProperty("amount").GetProperty("default").GetDecimal(), Is.EqualTo(12.5m));
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        /// <summary>
        /// Reads a schema type or union as a comma-separated string.
        /// </summary>
        private static string Types(JsonElement element)
        {
            var types = element.GetProperty("type");
            return types.ValueKind == JsonValueKind.Array ? string.Join(",", types.EnumerateArray().Select(x => x.GetString())) : types.GetString();
        }

        /// <summary>
        /// Recursively asserts that every schema reference resolves within the document.
        /// </summary>
        private static void AssertReferencesResolve(JsonElement element, JsonElement root)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (property.Name == "$ref")
                    {
                        var reference = Uri.UnescapeDataString(property.Value.GetString());
                        Assert.That(reference, Does.StartWith("#/"));
                        var target = root;
                        foreach (var segment in reference.Substring(2).Split('/'))
                            Assert.That(target.TryGetProperty(segment.Replace("~1", "/").Replace("~0", "~"), out target), Is.True, reference);
                    }
                    else
                        AssertReferencesResolve(property.Value, root);
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
                foreach (var item in element.EnumerateArray())
                    AssertReferencesResolve(item, root);
        }
    }
}