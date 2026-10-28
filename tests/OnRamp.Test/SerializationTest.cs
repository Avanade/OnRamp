using NUnit.Framework;
using OnRamp.Config;
using OnRamp.Test.Config;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serializer = OnRamp.Utility.JsonSerializer;

namespace OnRamp.Test
{
    /// <summary>
    /// Verifies serialization opt-in behavior and YAML-to-JSON value fidelity.
    /// </summary>
    [TestFixture]
    public class SerializationTest
    {
        /// <summary>
        /// Verifies the stream, reader, and string overloads use the same configuration contract.
        /// </summary>
        [TestCase("string")]
        [TestCase("reader")]
        [TestCase("stream")]
        public void JsonOverloadsPreserveExtensionDataAndIgnorePropertyCase(string input)
        {
            const string json = "{\"NAME\":\"Person\",\"extra\":{\"enabled\":true,\"count\":42}}";
            using var reader = new StringReader(json);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var result = (EntityConfig)(input switch
            {
                "reader" => Serializer.Deserialize(reader, typeof(EntityConfig)),
                "stream" => Serializer.Deserialize(stream, typeof(EntityConfig)),
                _ => Serializer.Deserialize(json, typeof(EntityConfig))
            });
            Assert.That(result.Name, Is.EqualTo("Person"));
            Assert.That(result.ExtraProperties.Count, Is.EqualTo(1));
            Assert.That(result.ExtraProperties["extra"].GetProperty("enabled").GetBoolean(), Is.True);
            Assert.That(result.ExtraProperties["extra"].GetProperty("count").GetInt32(), Is.EqualTo(42));
            Assert.That(stream.CanRead, Is.True);
            Assert.That(reader.Peek(), Is.EqualTo(input == "reader" ? -1 : '{'));
        }

        /// <summary>
        /// Verifies attributed classes serialize only explicitly named properties while retaining extension data.
        /// </summary>
        [Test]
        public void AttributedClassesOptInAndPreserveUnknownProperties()
        {
            var value = (OptInValue)Serializer.Deserialize("{\"name\":\"Person\",\"unexpected\":42}", typeof(OptInValue));
            Assert.That(value.Name, Is.EqualTo("Person"));
            Assert.That(value.Extra["unexpected"].GetInt32(), Is.EqualTo(42));

            using var json = JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(value, Serializer.Options));
            Assert.That(json.RootElement.GetProperty("name").GetString(), Is.EqualTo("Person"));
            Assert.That(json.RootElement.GetProperty("unexpected").GetInt32(), Is.EqualTo(42));
            Assert.That(json.RootElement.TryGetProperty(nameof(OptInValue.Computed), out _), Is.False);
            Assert.That(json.RootElement.TryGetProperty(nameof(OptInValue.Ignored), out _), Is.False);
        }

        /// <summary>
        /// Verifies unannotated CLR classes retain ordinary public-property serialization.
        /// </summary>
        [Test]
        public void PlainClassesRetainNormalSerialization()
        {
            var value = (PlainValue)Serializer.Deserialize("{\"name\":\"Person\",\"count\":7}", typeof(PlainValue));
            Assert.That(value.Name, Is.EqualTo("Person"));
            Assert.That(value.Count, Is.EqualTo(7));
            using var json = JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(value, Serializer.Options));
            Assert.That(json.RootElement.GetProperty(nameof(PlainValue.Name)).GetString(), Is.EqualTo("Person"));
            Assert.That(json.RootElement.GetProperty(nameof(PlainValue.Count)).GetInt32(), Is.EqualTo(7));
        }

        /// <summary>
        /// Verifies YAML conversion preserves scalar kinds, leading zeros, quoted values, and large integers.
        /// </summary>
        [TestCase("true", JsonValueKind.True, "true")]
        [TestCase("false", JsonValueKind.False, "false")]
        [TestCase("42", JsonValueKind.Number, "42")]
        [TestCase("-42", JsonValueKind.Number, "-42")]
        [TestCase("2147483648", JsonValueKind.Number, "2147483648")]
        [TestCase("9223372036854775807", JsonValueKind.Number, "9223372036854775807")]
        [TestCase("-9223372036854775808", JsonValueKind.Number, "-9223372036854775808")]
        [TestCase("12.5", JsonValueKind.Number, "12.5")]
        [TestCase("0123", JsonValueKind.String, "0123")]
        [TestCase("'42'", JsonValueKind.String, "42")]
        [TestCase("\"true\"", JsonValueKind.String, "true")]
        [TestCase("null", JsonValueKind.Null, null)]
        public void YamlScalarKindsArePreserved(string yaml, JsonValueKind kind, string expected)
        {
            using var reader = new StringReader($"value: {yaml}");
            var node = reader.YamlToJsonNode();
            using var json = JsonDocument.Parse(node.ToJsonString());
            var value = json.RootElement.GetProperty("value");
            Assert.That(value.ValueKind, Is.EqualTo(kind));
            if (kind == JsonValueKind.String)
                Assert.That(value.GetString(), Is.EqualTo(expected));
            else if (kind != JsonValueKind.Null)
                Assert.That(value.GetRawText(), Is.EqualTo(expected));
        }

        /// <summary>
        /// Verifies typed YAML deserialization preserves child collections through both overloads.
        /// </summary>
        [TestCase(true)]
        [TestCase(false)]
        public void DeserializeYamlPreservesTypedConfiguration(bool generic)
        {
            using var reader = new StringReader("name: Person\nproperties:\n- name: Age\n  type: int\n  isNullable: true\n");
            var value = generic ? reader.DeserializeYaml<EntityConfig>() : (EntityConfig)reader.DeserializeYaml(typeof(EntityConfig));
            Assert.That(value.Name, Is.EqualTo("Person"));
            Assert.That(value.Properties.Count, Is.EqualTo(1));
            Assert.That(value.Properties[0].Name, Is.EqualTo("Age"));
            Assert.That(value.Properties[0].Type, Is.EqualTo("int"));
            Assert.That(value.Properties[0].IsNullable, Is.True);
        }

        /// <summary>
        /// Verifies JSON reader overloads deserialize without closing the caller's reader.
        /// </summary>
        [TestCase(true)]
        [TestCase(false)]
        public void DeserializeJsonLeavesReaderOpen(bool generic)
        {
            using var reader = new StringReader("{\"name\":\"Person\"}");
            var value = generic ? reader.DeserializeJson<EntityConfig>() : (EntityConfig)reader.DeserializeJson(typeof(EntityConfig));
            Assert.That(value.Name, Is.EqualTo("Person"));
            Assert.That(reader.Peek(), Is.EqualTo(-1));
        }

        /// <summary>
        /// Verifies malformed JSON is surfaced through the reader and node conversion APIs.
        /// </summary>
        [Test]
        public void InvalidJsonFailsInsteadOfReturningAnEmptyConfiguration()
        {
            using var reader = new StringReader("{bad");
            using var nodeReader = new StringReader("{bad");
            Assert.Catch<JsonException>((System.Action)(() => reader.DeserializeJson<EntityConfig>()));
            Assert.Catch<JsonException>((System.Action)(() => nodeReader.JsonToJsonNode()));
        }

        /// <summary>
        /// Verifies explicit JSON and YAML null inputs remain null.
        /// </summary>
        [Test]
        public void NullDocumentsRemainNull()
        {
            using var json = new StringReader("null");
            using var yaml = new StringReader("null");
            using var empty = new StringReader("");
            Assert.That(json.DeserializeJson<EntityConfig>(), Is.Null);
            Assert.That(yaml.YamlToJsonNode(), Is.Null);
            Assert.That(empty.DeserializeYaml<EntityConfig>(), Is.Null);
        }

        /// <summary>
        /// Verifies a null JSON reader is rejected with a named argument error.
        /// </summary>
        [Test]
        public void NullJsonReaderIsRejected()
        {
            var ex = Assert.Throws<ArgumentNullException>((System.Action)(() => TextReaderExtensions.DeserializeJson<EntityConfig>(null)));
            Assert.That(ex.ParamName, Is.EqualTo("json"));
        }

        /// <summary>
        /// Models an opt-in object with extension data and properties that must not be accessed.
        /// </summary>
        [CodeGenClass("OptIn")]
        public class OptInValue
        {
            /// <summary>
            /// Gets or sets the explicitly serialized name.
            /// </summary>
            [JsonPropertyName("name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets an unannotated property that serialization must not access.
            /// </summary>
            public string Computed => throw new InvalidOperationException("Computed property must be excluded.");

            /// <summary>
            /// Gets an explicitly ignored property that serialization must not access.
            /// </summary>
            [JsonIgnore]
            public string Ignored => throw new InvalidOperationException("Ignored property must be excluded.");

            /// <summary>
            /// Gets or sets unknown JSON properties.
            /// </summary>
            [JsonExtensionData]
            public Dictionary<string, JsonElement> Extra { get; set; }
        }

        /// <summary>
        /// Models ordinary CLR serialization without configuration class metadata.
        /// </summary>
        public class PlainValue
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the numeric value.
            /// </summary>
            public int Count { get; set; }
        }
    }
}
