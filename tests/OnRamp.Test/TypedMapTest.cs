#nullable enable

using NUnit.Framework;
using OnRamp.Config;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnRamp.Test
{
    [TestFixture]
    public class TypedMapTest
    {
        [Test]
        public void GenerateSchemaForRecursiveTypedMap()
        {
            using var stream = new MemoryStream();
            JsonSchemaGenerator.Generate<TypedMapRoot>(stream);
            using var document = JsonDocument.Parse(stream.ToArray());

            var definitions = document.RootElement.GetProperty("definitions");
            Assert.That(definitions.EnumerateObject().Select(x => x.Name), Is.EqualTo(new[] { "TypedMapRoot", "TypedMapValue" }));

            var root = definitions.GetProperty("TypedMapRoot");
            var types = root.GetProperty("properties").GetProperty("types");
            Assert.That(types.GetProperty("type").EnumerateArray().Select(x => x.GetString()), Is.EqualTo(new[] { "object", "null" }));
            Assert.That(types.GetProperty("title").GetString(), Is.EqualTo("Record definitions."));
            Assert.That(types.GetProperty("additionalProperties").GetProperty("$ref").GetString(), Is.EqualTo("#/definitions/TypedMapValue"));
            Assert.That(types.TryGetProperty("items", out _), Is.False);
            Assert.That(types.TryGetProperty("default", out _), Is.False);
            Assert.That(root.GetProperty("required")[0].GetString(), Is.EqualTo("types"));
            var optionalTypes = root.GetProperty("properties").GetProperty("optionalTypes");
            Assert.That(optionalTypes.GetProperty("default").ValueKind, Is.EqualTo(JsonValueKind.Null));
            Assert.That(optionalTypes.EnumerateObject().Count(x => x.Name == "default"), Is.EqualTo(1));

            var valueProperties = definitions.GetProperty("TypedMapValue").GetProperty("properties");
            Assert.That(valueProperties.GetProperty("children").GetProperty("additionalProperties").GetProperty("$ref").GetString(), Is.EqualTo("#/definitions/TypedMapValue"));
            Assert.That(valueProperties.GetProperty("items").GetProperty("items").GetProperty("$ref").GetString(), Is.EqualTo("#/definitions/TypedMapValue"));
            Assert.That(valueProperties.GetProperty("children").TryGetProperty("items", out _), Is.False);
        }

        [Test]
        public void GenerateMarkdownForMapOnceWithLinksAndCallbacks()
        {
            using var directory = new TestDirectory();
            var created = new List<string>();
            var properties = new List<string>();

            MarkdownDocumentationGenerator.Generate<TypedMapRoot>(
                directory: directory.DirectoryPath,
                createFileName: (_, metadata) => $"{metadata.Name}-custom.md",
                propertyData: data => properties.Add($"{data.Type!.Name}.{data.Name}"),
                fileCreation: created.Add);

            Assert.That(created, Is.EqualTo(new[] { "TypedMapRoot-custom.md", "TypedMapValue-custom.md" }));
            Assert.That(properties.Count(x => x == "TypedMapRoot.types"), Is.EqualTo(1));
            Assert.That(File.ReadAllText(Path.Combine(directory.DirectoryPath, "TypedMapRoot-custom.md")),
                Does.Contain("**`types`** | Record definitions. A string-keyed map of [`TypedMapValue`](TypedMapValue-custom.md) objects. [Mandatory]"));
            Assert.That(File.ReadAllText(Path.Combine(directory.DirectoryPath, "TypedMapValue-custom.md")), Does.Contain("A string-keyed map of"));
        }

        [Test]
        public async Task PrepareMapRetainsDictionaryAndAddsKeyContext()
        {
            var comparer = StringComparer.OrdinalIgnoreCase;
            var map = new Dictionary<string, TypedMapValue>(comparer)
            {
                ["Header name!"] = new() { Code = "HDR", Match = "HDR" }
            };
            var root = new TypedMapRoot { Types = map };

            await root.PrepareAsync(root, root);

            Assert.That(root.Types, Is.SameAs(map));
            Assert.That(root.Types.Comparer, Is.SameAs(comparer));
            Assert.That(root.Types.ContainsKey("header NAME!"), Is.True);
            Assert.That(root.Types["Header name!"].MapKey, Is.EqualTo("Header name!"));
            Assert.That(root.Types["Header name!"].WasPrepared, Is.True);
            Assert.That(root.Types["Header name!"].PreparedRoot, Is.SameAs(root));
            Assert.That(root.Types["Header name!"].PreparedParent, Is.SameAs(root));
        }

        [Test]
        public async Task DeserializeAndPrepareYamlMapEntries()
        {
            const string yaml =
                "types:\n" +
                "  'Header name!':\n" +
                "    match: HDR\n" +
                "    code: HDR\n" +
                "  Detail:\n" +
                "    match: DTL\n" +
                "    code: DTL\n";
            using var reader = new StringReader(yaml);

            var root = reader.DeserializeYaml<TypedMapRoot>()!;
            var originalMap = root.Types;
            await root.PrepareAsync(root, root);

            Assert.That(root.Types, Is.SameAs(originalMap));
            Assert.That(root.Types!.Keys, Is.EqualTo(new[] { "Header name!", "Detail" }));
            Assert.That(root.Types["Header name!"].MapKey, Is.EqualTo("Header name!"));
            Assert.That(root.Types["Detail"].MapKey, Is.EqualTo("Detail"));
            Assert.That(root.OptionalTypes, Is.Empty);
        }

        [Test]
        public void PrepareMapReportsEntryKeyForNestedValidationAndNullValues()
        {
            var root = new TypedMapRoot
            {
                Types = new Dictionary<string, TypedMapValue>
                {
                    ["Header"] = new() { Code = "HDR" }
                }
            };
            var fieldError = Assert.ThrowsAsync<CodeGenException>((Func<Task>)(async () => await root.PrepareAsync(root, root)));
            Assert.That(fieldError!.Message, Does.Contain("Types['Header'].Match"));
            Assert.That(fieldError.Message, Does.Contain("Value is mandatory."));

            root = new TypedMapRoot
            {
                Types = new Dictionary<string, TypedMapValue> { ["Bad entry"] = null! }
            };
            var nullError = Assert.ThrowsAsync<CodeGenException>((Func<Task>)(async () => await root.PrepareAsync(root, root)));
            Assert.That(nullError!.Message, Does.Contain("Types['Bad entry']"));
            Assert.That(nullError.Message, Does.Contain("Map value is null."));
        }

        [Test]
        public void PrepareMapReportsMandatoryMapAndEnforcesValueUniqueness()
        {
            var missing = new TypedMapRoot();
            var mandatory = Assert.ThrowsAsync<CodeGenException>((Func<Task>)(async () => await missing.PrepareAsync(missing, missing)));
            Assert.That(mandatory!.Message, Does.Contain("Map is mandatory."));

            var duplicate = new TypedMapRoot
            {
                Types = new Dictionary<string, TypedMapValue>
                {
                    ["one"] = new() { Code = "same", Match = "one" },
                    ["two"] = new() { Code = "same", Match = "two" }
                }
            };
            var unique = Assert.ThrowsAsync<CodeGenException>((Func<Task>)(async () => await duplicate.PrepareAsync(duplicate, duplicate)));
            Assert.That(unique!.Message, Does.Contain("Value 'same' is not unique."));
            Assert.That(unique.Message, Does.Contain("Types['two']"));
        }

        [Test]
        public void YamlDuplicateKeysFailInBothReaderPaths()
        {
            const string yaml = "types:\n  Header:\n    match: HDR\n  Header:\n    match: DTL\n";

            using var typedReader = new StringReader(yaml);
            Assert.Throws<YamlDotNet.Core.YamlException>((Action)(() => typedReader.DeserializeYaml<TypedMapRoot>()));

            using var nodeReader = new StringReader(yaml);
            Assert.Throws<YamlDotNet.Core.YamlException>((Action)(() => nodeReader.YamlToJsonNode()));
        }

        [TestCase(typeof(InvalidMapKeyRoot), "map keys must be strings")]
        [TestCase(typeof(InvalidMapValueRoot), "map values must derive from ConfigBase")]
        [TestCase(typeof(ConflictingMapShapeRoot), "conflicting property shape attributes")]
        public void InvalidMapMetadataFailsWithPropertyContext(Type type, string expected)
        {
            using var stream = new MemoryStream();
            var invocation = Assert.Throws<TargetInvocationException>((Action)(() => GenerateSchema(type, stream)));
            Assert.That(invocation!.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(invocation.InnerException!.Message, Does.Contain("Property 'Values'"));
            Assert.That(invocation.InnerException.Message, Does.Contain(expected));
            Assert.That(stream.Length, Is.Zero);
        }

        private static void GenerateSchema(Type type, Stream stream)
        {
            typeof(JsonSchemaGenerator).GetMethod(nameof(JsonSchemaGenerator.Generate), new[] { typeof(Stream), typeof(string) })!
                .MakeGenericMethod(type)
                .Invoke(null, new object?[] { stream, null });
        }

        [CodeGenClass("TypedMapRoot", Title = "Typed map root.")]
        [CodeGenCategory("Primary")]
        public class TypedMapRoot : ConfigRootBase<TypedMapRoot>
        {
            [JsonPropertyName("types")]
            [CodeGenPropertyMap("Primary", Title = "Record definitions.", IsMandatory = true)]
            public Dictionary<string, TypedMapValue>? Types { get; set; }

            [JsonPropertyName("optionalTypes")]
            [DefaultValue(null)]
            [CodeGenPropertyMap("Primary", Title = "Optional record definitions.")]
            public Dictionary<string, TypedMapValue>? OptionalTypes { get; set; }

            protected override async Task PrepareAsync()
            {
                Types = await PrepareMapAsync(Types, nameof(Types)).ConfigureAwait(false);
                OptionalTypes = await PrepareMapAsync(OptionalTypes, nameof(OptionalTypes)).ConfigureAwait(false);
            }
        }

        [CodeGenClass("TypedMapValue", Title = "Typed map value.")]
        [CodeGenCategory("Primary")]
        public class TypedMapValue : ConfigBase<TypedMapRoot, TypedMapRoot>
        {
            [JsonPropertyName("match")]
            [CodeGenProperty("Primary", IsMandatory = true)]
            public string? Match { get; set; }

            [JsonPropertyName("code")]
            [CodeGenProperty("Primary", IsUnique = true)]
            public string? Code { get; set; }

            [JsonPropertyName("children")]
            [CodeGenPropertyMap("Primary", Title = "Child definitions.")]
            public Dictionary<string, TypedMapValue>? Children { get; set; }

            [JsonPropertyName("items")]
            [CodeGenPropertyCollection("Primary", Title = "Items.")]
            public List<TypedMapValue>? Items { get; set; }

            public string? MapKey => MapEntryKey;
            public bool WasPrepared { get; private set; }
            public TypedMapRoot? PreparedRoot { get; private set; }
            public TypedMapRoot? PreparedParent { get; private set; }

            protected override Task PrepareAsync()
            {
                WasPrepared = true;
                PreparedRoot = Root;
                PreparedParent = Parent;
                return Task.CompletedTask;
            }
        }

        [CodeGenClass("InvalidMapKeyRoot")]
        [CodeGenCategory("Primary")]
        public class InvalidMapKeyRoot : ConfigRootBase<InvalidMapKeyRoot>
        {
            [JsonPropertyName("values")]
            [CodeGenPropertyMap("Primary")]
            public Dictionary<int, TypedMapValue>? Values { get; set; }

            protected override Task PrepareAsync() => Task.CompletedTask;
        }

        [CodeGenClass("InvalidMapValueRoot")]
        [CodeGenCategory("Primary")]
        public class InvalidMapValueRoot : ConfigRootBase<InvalidMapValueRoot>
        {
            [JsonPropertyName("values")]
            [CodeGenPropertyMap("Primary")]
            public Dictionary<string, string>? Values { get; set; }

            protected override Task PrepareAsync() => Task.CompletedTask;
        }

        [CodeGenClass("ConflictingMapShapeRoot")]
        [CodeGenCategory("Primary")]
        public class ConflictingMapShapeRoot : ConfigRootBase<ConflictingMapShapeRoot>
        {
            [JsonPropertyName("values")]
            [CodeGenProperty("Primary")]
            [CodeGenPropertyMap("Primary")]
            public Dictionary<string, TypedMapValue>? Values { get; set; }

            protected override Task PrepareAsync() => Task.CompletedTask;
        }
    }
}
