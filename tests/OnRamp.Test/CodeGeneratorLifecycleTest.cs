using NUnit.Framework;
using OnRamp.Config;
using OnRamp.Scripts;
using OnRamp.Test.Config;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OnRamp.Test
{
    /// <summary>
    /// Verifies configuration loading, pre-deserialization hooks, and isolated generation modes.
    /// </summary>
    [TestFixture]
    public class CodeGeneratorLifecycleTest
    {
        /// <summary>
        /// Verifies JSON and YAML stream loading prepares configuration without creating output files.
        /// </summary>
        [TestCase(StreamContentType.Json, "{\"name\":\"Person\",\"properties\":[{\"name\":\"Age\"}]}")]
        [TestCase(StreamContentType.Yaml, "name: Person\nproperties:\n- name: Age\n")]
        public async Task LoadConfigPreparesWithoutGenerating(StreamContentType type, string text)
        {
            using var directory = new TestDirectory();
            var args = CodeGeneratorArgs.Create<CodeGeneratorLifecycleTest>("ValidEntity.yaml")
                .AddParameter("Directory", directory.DirectoryPath).AddParameter("Custom", "value");
            var generator = await CodeGenerator.CreateAsync(args);
            using var reader = new StringReader(text);
            var root = (EntityConfig)await generator.LoadConfigAsync(reader, type);
            Assert.That(root.Name, Is.EqualTo("Person"));
            Assert.That(root.CodeGenArgs, Is.SameAs(args));
            Assert.That(root.GetRuntimeParameter<string>("Custom"), Is.EqualTo("value"));
            Assert.That(root.Properties.Count, Is.EqualTo(1));
            Assert.That(root.Properties[0].Root, Is.SameAs(root));
            Assert.That(root.Properties[0].Parent, Is.SameAs(root));
            Assert.That(root.Properties[0].Type, Is.EqualTo("string"));
            Assert.That(Directory.GetFiles(directory.DirectoryPath), Is.Empty);
            Assert.That(reader.Peek(), Is.EqualTo(-1));
        }

        /// <summary>
        /// Verifies a tab delimiter from a YAML interface configuration reaches generated output.
        /// </summary>
        [Test]
        public async Task YamlInterfaceTabDelimiterReachesGeneratedOutput()
        {
            using var directory = new TestDirectory();
            var args = CodeGeneratorArgs.Create<CodeGeneratorLifecycleTest>("YamlFileInterface.yaml")
                .AddParameter("Directory", directory.DirectoryPath);
            var generator = await CodeGenerator.CreateAsync(args);
            using var reader = new StringReader("file:\n  delimiter: \"\\t\"\n");

            var statistics = await generator.GenerateAsync(reader, StreamContentType.Yaml);

            Assert.That(statistics.CreatedCount, Is.EqualTo(1));
            Assert.That(File.ReadAllText(Path.Combine(directory.DirectoryPath, "delimiter.txt")), Is.EqualTo("\t"));
        }

        /// <summary>
        /// Verifies configuration stream errors include the supplied source name and do not write output.
        /// </summary>
        [TestCase(StreamContentType.Unknown, "name: Person", "Stream content type is not supported.")]
        [TestCase(StreamContentType.Json, "null", "Stream is empty.")]
        [TestCase(StreamContentType.Yaml, "", "Stream is empty.")]
        [TestCase(StreamContentType.Json, "{bad", "invalid")]
        public async Task LoadConfigErrorsRetainSourceContext(StreamContentType type, string text, string message)
        {
            using var directory = new TestDirectory();
            var generator = await CodeGenerator.CreateAsync(CodeGeneratorArgs.Create<CodeGeneratorLifecycleTest>("ValidEntity.yaml").AddParameter("Directory", directory.DirectoryPath));
            using var reader = new StringReader(text);
            var ex = Assert.ThrowsAsync<CodeGenException>(asyncCode: () => generator.LoadConfigAsync(reader, type, "supplied-config"));
            Assert.That(ex.Message, Does.StartWith("Config 'supplied-config' is invalid:").And.Contain(message));
            Assert.That(Directory.GetFiles(directory.DirectoryPath), Is.Empty);
        }

        /// <summary>
        /// Verifies missing script and configuration arguments fail with domain-specific errors.
        /// </summary>
        [Test]
        public async Task MissingFileArgumentsAreReported()
        {
            Assert.That(Assert.ThrowsAsync<CodeGenException>(asyncCode: () => CodeGenerator.CreateAsync(new CodeGeneratorArgs())).Message,
                Is.EqualTo("Script file name must be specified."));
            var generator = await CodeGenerator.CreateAsync(CodeGeneratorArgs.Create<CodeGeneratorLifecycleTest>("ValidEntity.yaml"));
            Assert.That(Assert.ThrowsAsync<CodeGenException>(asyncCode: () => generator.LoadConfigAsync()).Message,
                Is.EqualTo("Config file must be specified."));
        }

        /// <summary>
        /// Verifies stream generation writes once and recognizes identical output on the next invocation.
        /// </summary>
        [Test]
        public async Task StreamGenerationIsRepeatableAndDetectsUnchangedFiles()
        {
            using var directory = new TestDirectory();
            var generator = await CodeGenerator.CreateAsync(CodeGeneratorArgs.Create<CodeGeneratorLifecycleTest>("ValidEntity.yaml").AddParameter("Directory", directory.DirectoryPath));
            using var first = new StringReader("{\"name\":\"Person\",\"properties\":[{\"name\":\"Age\",\"type\":\"int\"}]}");
            var created = await generator.GenerateAsync(first, StreamContentType.Json);
            Assert.That(created.CreatedCount, Is.EqualTo(2));
            Assert.That(created.UpdatedCount, Is.Zero);
            Assert.That(created.NotChangedCount, Is.Zero);
            Assert.That(created.LinesOfCodeCount, Is.EqualTo(2));
            Assert.That(created.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(0));
            Assert.That(File.ReadAllText(Path.Combine(directory.DirectoryPath, "Age.txt")), Is.EqualTo("Name: Person.Age, Type: int"));

            using var second = new StringReader("name: Person\nproperties:\n- name: Age\n  type: int\n");
            var unchanged = await generator.GenerateAsync(second, StreamContentType.Yaml);
            Assert.That(unchanged.CreatedCount, Is.Zero);
            Assert.That(unchanged.UpdatedCount, Is.Zero);
            Assert.That(unchanged.NotChangedCount, Is.EqualTo(2));
            Assert.That(Directory.GetFiles(directory.DirectoryPath).Select(Path.GetFileName), Is.EquivalentTo(new[] { "Person.txt", "Age.txt" }));
        }

        /// <summary>
        /// Verifies simulation neither creates a nonexistent output directory nor writes generated files.
        /// </summary>
        [Test]
        public async Task SimulationDoesNotCreateOutputDirectory()
        {
            using var directory = new TestDirectory();
            var output = Path.Combine(directory.DirectoryPath, "not-created");
            var args = CodeGeneratorArgs.Create<CodeGeneratorLifecycleTest>("ValidEntity.yaml").AddParameter("Directory", output);
            args.IsSimulation = true;
            var generator = await CodeGenerator.CreateAsync(args);
            using var reader = new StringReader("{\"name\":\"Person\"}");
            var stats = await generator.GenerateAsync(reader, StreamContentType.Json);
            Assert.That(stats.CreatedCount, Is.EqualTo(1));
            Assert.That(stats.UpdatedCount, Is.Zero);
            Assert.That(stats.NotChangedCount, Is.Zero);
            Assert.That(Directory.Exists(output), Is.False);
        }

        /// <summary>
        /// Verifies an expected-no-changes failure leaves an existing artifact untouched.
        /// </summary>
        [Test]
        public async Task ExpectNoChangesDoesNotOverwriteExistingArtifact()
        {
            using var directory = new TestDirectory();
            var path = Path.Combine(directory.DirectoryPath, "Person.txt");
            File.WriteAllText(path, "user content");
            var args = CodeGeneratorArgs.Create<CodeGeneratorLifecycleTest>("ValidEntity.yaml").AddParameter("Directory", directory.DirectoryPath);
            args.ExpectNoChanges = true;
            var generator = await CodeGenerator.CreateAsync(args);
            using var reader = new StringReader("{\"name\":\"Person\"}");
            var ex = Assert.ThrowsAsync<CodeGenChangesFoundException>(asyncCode: () => generator.GenerateAsync(reader, StreamContentType.Json));
            Assert.That(ex.Message, Does.Contain("Person.txt").And.Contain("updated"));
            Assert.That(File.ReadAllText(path), Is.EqualTo("user content"));
            Assert.That(Directory.GetFiles(directory.DirectoryPath).Length, Is.EqualTo(1));
        }

        /// <summary>
        /// Verifies non-root configurations cannot be passed directly to generation.
        /// </summary>
        [Test]
        public async Task GenerateRejectsNonRootConfiguration()
        {
            var generator = await CodeGenerator.CreateAsync(CodeGeneratorArgs.Create<CodeGeneratorLifecycleTest>("ValidEntity.yaml"));
            var ex = Assert.Throws<ArgumentException>((System.Action)(() => generator.GenerateAsync(new PropertyConfig())));
            Assert.That(ex.ParamName, Is.EqualTo("config"));
            Assert.That(ex.Message, Does.Contain("IRootConfig"));
        }

        /// <summary>
        /// Verifies pre-deserialization hooks can change configuration before preparation and preserve context.
        /// </summary>
        [Test]
        public async Task ConfigurationLoadHookRunsBeforePreparation()
        {
            using var directory = new TestDirectory();
            var args = CodeGeneratorArgs.Create<CodeGeneratorLifecycleTest>("ValidEntity.yaml").AddParameter("Directory", directory.DirectoryPath);
            var generator = await CodeGenerator.CreateAsync<ObservingGenerator>(args);
            using var reader = new StringReader("{\"name\":\"original\",\"properties\":[{}]}");
            var root = (EntityConfig)await generator.LoadConfigAsync(reader, StreamContentType.Json, "hook-config");
            Assert.That(generator.LoadedNames, Is.EqualTo(new[] { "hook-config" }));
            Assert.That(root.Name, Is.EqualTo("edited"));
            Assert.That(root.Properties[0].Name, Is.EqualTo("EditedProperty"));
            Assert.That(root.Properties[0].Type, Is.EqualTo("string"));
            Assert.That(Directory.GetFiles(directory.DirectoryPath), Is.Empty);
        }

        /// <summary>
        /// Captures and mutates JSON before the normal configuration deserialization and preparation.
        /// </summary>
        public class ObservingGenerator : CodeGenerator
        {
            /// <summary>
            /// Initializes the generator using the standard creation contract.
            /// </summary>
            public ObservingGenerator(ICodeGeneratorArgs args, CodeGenScript scripts) : base(args, scripts) { }

            /// <summary>
            /// Gets source names supplied to the configuration load hook.
            /// </summary>
            public List<string> LoadedNames { get; } = new List<string>();

            /// <summary>
            /// Changes required values before normal validation and defaulting occur.
            /// </summary>
            protected override void OnConfigurationLoad(CodeGenScript script, string fileName, JsonNode json)
            {
                Assert.That(script, Is.SameAs(Scripts));
                LoadedNames.Add(fileName);
                json["name"] = "edited";
                json["properties"][0]["name"] = "EditedProperty";
            }
        }
    }
}
