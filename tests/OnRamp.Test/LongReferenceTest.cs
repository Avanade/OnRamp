using NUnit.Framework;
using OnRamp.Config;
using OnRamp.Test.Config;
using OnRamp.Utility;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OnRamp.Test
{
    [TestFixture]
    public class LongReferenceTest
    {
        [CodeGenClass("Budgets", Title = "Budgets.")]
        public class BudgetConfig : ReferenceRoot<BudgetConfig>
        {
            [JsonPropertyName("bytes")]
            [CodeGenProperty("Primary", Title = "Byte budget.")]
            public long Bytes => throw new NotSupportedException();

            [JsonPropertyName("records")]
            [CodeGenProperty("Primary", Title = "Record budget.")]
            public long? Records => throw new NotSupportedException();

            [JsonPropertyName("explicitInteger")]
            [CodeGenProperty("Primary", JsonTypes = new[] { CodeGenJsonType.Integer })]
            public long ExplicitInteger => throw new NotSupportedException();

            [JsonPropertyName("explicitNumber")]
            [CodeGenProperty("Primary", JsonTypes = new[] { CodeGenJsonType.Number })]
            public long? ExplicitNumber => throw new NotSupportedException();
        }

        [Test]
        public void SchemaInfersLongIntegersAndPreservesCompatibleDeclarations()
        {
            using var stream = new MemoryStream();
            JsonSchemaGenerator.Generate<BudgetConfig>(stream);
            using var document = JsonDocument.Parse(stream.ToArray());
            var properties = document.RootElement.GetProperty("definitions").GetProperty("Budgets").GetProperty("properties");
            foreach (var name in new[] { "bytes", "records", "explicitInteger" })
                Assert.That(properties.GetProperty(name).GetProperty("type").GetString(), Is.EqualTo("integer"));
            Assert.That(properties.GetProperty("explicitNumber").GetProperty("type").GetString(), Is.EqualTo("number"));
        }

        [Test]
        public void MarkdownSupportsInferredLongsWithoutAddingInferredTypeAnnotations()
        {
            using var directory = new TestDirectory();
            MarkdownDocumentationGenerator.Generate<BudgetConfig>(directory: directory.DirectoryPath);
            var markdown = File.ReadAllText(Path.Combine(directory.DirectoryPath, "Budgets.md"));
            Assert.That(markdown, Does.Contain("`bytes` | Byte budget."));
            Assert.That(markdown, Does.Contain("`records` | Record budget."));
            Assert.That(markdown, Does.Not.Contain("Byte budget. JSON types").And.Not.Contain("Record budget. JSON types"));
            Assert.That(markdown, Does.Contain("JSON types: `integer`.").And.Contain("JSON types: `number`."));
        }
    }
}
