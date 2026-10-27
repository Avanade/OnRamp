using OnRamp.Test.Config;
using OnRamp.Utility;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OnRamp.Test
{
    /// <summary>
    /// Verifies Markdown reference pages, callbacks, and shared metadata validation.
    /// </summary>
    [TestFixture]
    public class MarkdownDocumentationGeneratorTest
    {
        /// <summary>
        /// Verifies existing Markdown output against the golden files.
        /// </summary>
        [Test]
        public void Generate()
        {
            using var directory = new TestDirectory();
            MarkdownDocumentationGenerator.Generate<EntityConfig>(directory: directory.DirectoryPath, addBreaksBetweenSections: true);

            var fn = Path.Combine(directory.DirectoryPath, "Entity.md");
            Assert.That(File.Exists(fn), Is.True);
            Assert.That(File.ReadAllText(fn), Is.EqualTo(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Expected", "Entity.md"))));

            fn = Path.Combine(directory.DirectoryPath, "Property.md");
            Assert.That(File.Exists(fn), Is.True);
            Assert.That(File.ReadAllText(fn), Is.EqualTo(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Expected", "Property.md"))));
        }

        /// <summary>
        /// Verifies nested and cyclic child links, custom filenames, defaults, and callback counts.
        /// </summary>
        [Test]
        public void GenerateNestedReferencesWithCallbacks()
        {
            InTemporaryDirectory(directory =>
            {
                var files = new List<string>();
                var properties = new List<string>();
                var filenames = new List<Type>();
                var types = new[] { typeof(CodeGenReferenceConfig), typeof(SourceReferenceConfig), typeof(FileReferenceConfig), typeof(ColumnReferenceConfig), typeof(ValidationReferenceConfig) };
                var expectedProperties = types.SelectMany(t => t.GetProperties().Where(p => p.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonPropertyNameAttribute), true).Length > 0).Select(p => $"{t.Name}.{p.Name}")).ToArray();
                Dictionary<string, string> first = null;
                for (var invocation = 0; invocation < 2; invocation++)
                {
                    files.Clear();
                    properties.Clear();
                    filenames.Clear();
                    MarkdownDocumentationGenerator.Generate<CodeGenReferenceConfig>(
                        directory: directory,
                        createFileName: (t, a) => { filenames.Add(t); return $"custom-{a.Name}.md"; },
                        propertyData: p =>
                        {
                            properties.Add($"{p.Type.Name}.{p.Property.Name}");
                            if (p.Type == typeof(CodeGenReferenceConfig) && p.Name == "source")
                            {
                                p.Name = "primarySource";
                                p.Psa.Title = "Customized source.";
                                p.Psa.Description = "Customized details.";
                            }
                            if (p.Type == typeof(ValidationReferenceConfig) && p.Name == "min")
                                p.Psa.JsonTypes = new[] { OnRamp.Config.CodeGenJsonType.Number };
                        },
                        fileCreation: files.Add);

                    Assert.That(files, Is.EquivalentTo(new[] { "custom-CodeGen.md", "custom-Source.md", "custom-File.md", "custom-Column.md", "custom-Validation.md" }));
                    Assert.That(properties, Is.EquivalentTo(expectedProperties));
                    Assert.That(filenames, Is.EquivalentTo(types));
                    var output = Directory.GetFiles(directory).ToDictionary(Path.GetFileName, File.ReadAllText);
                    Assert.That(output.Count, Is.EqualTo(5));
                    var root = output["custom-CodeGen.md"];
                    Assert.That(root, Does.Contain("**`primarySource`** | Customized source. The corresponding [`Source`](custom-Source.md) object. [Mandatory]"));
                    Assert.That(root, Does.Contain("Customized details."));
                    Assert.That(root, Does.Contain("`secondarySource` | Optional source. The corresponding [`Source`](custom-Source.md) object."));
                    Assert.That(output["custom-File.md"], Does.Contain("[`Column`](custom-Column.md) collection."));
                    Assert.That(output["custom-Column.md"], Does.Contain("[`Validation`](custom-Validation.md) object."));
                    Assert.That(output["custom-Validation.md"], Does.Contain("[`Validation`](custom-Validation.md) object."));
                    Assert.That(output["custom-Validation.md"], Does.Contain("[`CodeGen`](custom-CodeGen.md) object."));
                    Assert.That(output["custom-Validation.md"], Does.Contain("`min` | Inclusive minimum value. JSON types: `number`. Default: `1.5`."));
                    Assert.That(output["custom-Validation.md"], Does.Contain("Item JSON types: `string`, `number`, `boolean`."));
                    Assert.That(output["custom-Source.md"], Does.Contain("Default: `\"AzureServiceBus\"`."));
                    Assert.That(output["custom-Source.md"], Does.Contain("Default: `true`."));
                    Assert.That(output["custom-Source.md"], Does.Contain("Default: `false`."));
                    Assert.That(output["custom-Source.md"], Does.Contain("Default: `42`."));
                    Assert.That(output["custom-Source.md"], Does.Contain("Default: `12.5`."));
                    Assert.That(output["custom-Source.md"], Does.Contain("Default: `null`."));
                    var label = output["custom-Source.md"].Split('\n').Single(l => l.StartsWith("`label` | "));
                    Assert.That(label.Count(c => c == '|'), Is.EqualTo(1));
                    Assert.That(label.Count(c => c == '`'), Is.EqualTo(4));
                    Assert.That(label, Does.Contain("\\r\\n"));
                    if (first != null)
                        Assert.That(output, Is.EquivalentTo(first));
                    first = output;
                }
            });
        }

        /// <summary>
        /// Verifies scalar metadata documentation and rejection of incompatible declarations.
        /// </summary>
        [TestCaseSource(typeof(ReferenceGenerationCases), nameof(ReferenceGenerationCases.Metadata))]
        public void GenerateScalarMetadata(Type type, string expected)
        {
            InTemporaryDirectory(directory =>
            {
                if (expected == null)
                {
                    var ex = Assert.Throws<InvalidOperationException>((System.Action)(() => ReferenceGenerationCases.Markdown(type, directory)));
                    Assert.That(ex.Message, Does.Contain("Property 'Value'"));
                    Assert.That(Directory.GetFiles(directory), Is.Empty);
                    return;
                }

                ReferenceGenerationCases.Markdown(type, directory);
                Assert.That(Directory.GetFiles(directory).Select(Path.GetFileName), Is.EqualTo(new[] { "Scalar.md" }));
                var markdown = File.ReadAllText(Path.Combine(directory, "Scalar.md"));
                Assert.That(markdown, Does.Not.Contain("]("));
                if (type.Name.StartsWith("Inferred"))
                    Assert.That(markdown, Does.Not.Contain("JSON types:"));
                else
                    Assert.That(markdown, Does.Contain($"JSON types: {string.Join(", ", expected.Split(',').Select(t => $"`{t}`"))}."));
            });
        }

        /// <summary>
        /// Verifies invalid class metadata and defaults fail before any pages are generated.
        /// </summary>
        [TestCase(typeof(CollisionRoot), "CodeGenClass.Name")]
        [TestCase(typeof(MissingClassRoot), "CodeGenClassAttribute")]
        [TestCase(typeof(NonFiniteDefault), "Property 'Value' has a default")]
        [TestCase(typeof(UnserializableDefault), "Property 'Value' has a default")]
        public void GenerateRejectsInvalidReferencesAndDefaults(Type type, string message)
        {
            InTemporaryDirectory(directory =>
            {
                var ex = Assert.Throws<InvalidOperationException>((System.Action)(() => ReferenceGenerationCases.Markdown(type, directory)));
                Assert.That(ex.Message, Does.Contain(message));
                Assert.That(Directory.GetFiles(directory), Is.Empty);
                if (type == typeof(NonFiniteDefault) || type == typeof(UnserializableDefault))
                    Assert.That(ex.InnerException, Is.Not.Null);
            });
        }

        /// <summary>
        /// Verifies numeric default documentation is independent of the current culture.
        /// </summary>
        [Test]
        public void GenerateDefaultsIndependentlyOfCulture()
        {
            var original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
                InTemporaryDirectory(directory =>
                {
                    ReferenceGenerationCases.Markdown(typeof(CodeGenReferenceConfig), directory);
                    Assert.That(File.ReadAllText(Path.Combine(directory, "Source.md")), Does.Contain("Default: `12.5`."));
                });
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        /// <summary>
        /// Runs a test in a unique temporary directory and removes its generated files.
        /// </summary>
        private static void InTemporaryDirectory(Action<string> action)
        {
            using var directory = new TestDirectory();
            action(directory.DirectoryPath);
        }
    }
}