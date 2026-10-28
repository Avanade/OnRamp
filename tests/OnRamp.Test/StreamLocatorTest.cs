using NUnit.Framework;
using OnRamp.Utility;
using System;
using System.IO;

namespace OnRamp.Test
{
    /// <summary>
    /// Verifies file and embedded-resource lookup, extension probing, and content classification.
    /// </summary>
    [TestFixture]
    public class StreamLocatorTest
    {
        /// <summary>
        /// Verifies supported extensions are case-insensitive and unknown types remain unknown.
        /// </summary>
        [TestCase("config.yaml", StreamContentType.Yaml)]
        [TestCase("config.YML", StreamContentType.Yaml)]
        [TestCase("config.json", StreamContentType.Json)]
        [TestCase("config.JSN", StreamContentType.Json)]
        [TestCase("config.xml", StreamContentType.Unknown)]
        [TestCase("config", StreamContentType.Unknown)]
        public void ContentTypeMatchesExtension(string fileName, StreamContentType expected) =>
            Assert.That(StreamLocator.GetContentType(fileName), Is.EqualTo(expected));

        /// <summary>
        /// Verifies direct files take precedence over content folders and extension candidates.
        /// </summary>
        [Test]
        public void DirectFileWinsOverFolderAndExtensionCandidates()
        {
            using var directory = new TestDirectory();
            var path = Path.Combine(directory.DirectoryPath, "config");
            File.WriteAllText(path, "direct");
            File.WriteAllText($"{path}.json", "extension");
            Directory.CreateDirectory(Path.Combine(directory.DirectoryPath, "Resources"));
            File.WriteAllText(Path.Combine(directory.DirectoryPath, "Resources", "config"), "folder");
            var result = StreamLocator.GetResourcesStreamReader(path, extensions: new[] { "json" });
            using var reader = result.StreamReader;
            Assert.That(result.FileName, Is.EqualTo(path));
            Assert.That(reader.ReadToEnd(), Is.EqualTo("direct"));
            Assert.That(StreamLocator.HasResourceStream(path).Exists, Is.True);
        }

        /// <summary>
        /// Verifies file extension candidates are evaluated in the declared order.
        /// </summary>
        [Test]
        public void ExtensionProbeOrderIsPreserved()
        {
            using var directory = new TestDirectory();
            var path = Path.Combine(directory.DirectoryPath, "config");
            File.WriteAllText($"{path}.json", "json");
            File.WriteAllText($"{path}.yaml", "yaml");
            var extensions = new[] { "missing", "yaml", "json" };
            var result = StreamLocator.GetStreamReader(path, extensions: extensions);
            using var reader = result.StreamReader;
            Assert.That(result.FileName, Is.EqualTo($"{path}.yaml"));
            Assert.That(reader.ReadToEnd(), Is.EqualTo("yaml"));
            Assert.That(StreamLocator.HasStream(path, extensions: extensions), Is.EqualTo((true, $"{path}.yaml")));
        }

        /// <summary>
        /// Verifies resources can be read from a content folder on disk.
        /// </summary>
        [Test]
        public void ResourceContentFolderIsSearched()
        {
            using var directory = new TestDirectory();
            Directory.CreateDirectory(Path.Combine(directory.DirectoryPath, "Resources"));
            File.WriteAllText(Path.Combine(directory.DirectoryPath, "Resources", "sample.txt"), "resource content");
            var requested = Path.Combine(directory.DirectoryPath, "sample.txt");
            var result = StreamLocator.GetResourcesStreamReader(requested);
            using var reader = result.StreamReader;
            Assert.That(reader, Is.Not.Null);
            Assert.That(reader.ReadToEnd(), Is.EqualTo("resource content"));
            Assert.That(result.FileName, Is.EqualTo(requested));
            Assert.That(StreamLocator.HasResourceStream(requested), Is.EqualTo((true, requested)));
        }

        /// <summary>
        /// Verifies read and existence APIs agree for qualified content-folder paths and extension probes.
        /// </summary>
        [TestCase("Resources", false, false)]
        [TestCase("Resources", false, true)]
        [TestCase("Resources", true, false)]
        [TestCase("Resources", true, true)]
        [TestCase("Scripts", false, false)]
        [TestCase("Scripts", false, true)]
        [TestCase("Scripts", true, false)]
        [TestCase("Scripts", true, true)]
        [TestCase("Templates", false, false)]
        [TestCase("Templates", false, true)]
        [TestCase("Templates", true, false)]
        [TestCase("Templates", true, true)]
        public void QualifiedContentFolderLookupsAgree(string contentType, bool relative, bool probeExtensions)
        {
            using var directory = new TestDirectory();
            var parent = Path.Combine(directory.DirectoryPath, "nested");
            Directory.CreateDirectory(Path.Combine(parent, contentType));
            var extension = contentType == "Templates" ? "hbs" : "yaml";
            var name = probeExtensions ? "sample" : $"sample.{extension}";
            var absolute = Path.Combine(parent, name);
            var requested = relative ? Path.GetRelativePath(Environment.CurrentDirectory, absolute) : absolute;
            var extensions = probeExtensions ? new[] { "missing", extension } : null;
            var expectedName = probeExtensions ? $"{requested}.{extension}" : requested;
            var path = Path.Combine(parent, contentType, $"sample.{extension}");
            File.WriteAllText(path, "folder content");

            var result = StreamLocator.GetStreamReader(requested, contentType, extensions: extensions);
            using (var reader = result.StreamReader)
            {
                Assert.That(reader, Is.Not.Null);
                Assert.That(reader.ReadToEnd(), Is.EqualTo("folder content"));
                Assert.That(result.FileName, Is.EqualTo(expectedName));
                Assert.That(StreamLocator.HasStream(requested, contentType, extensions: extensions), Is.EqualTo((true, expectedName)));
            }

            File.Delete(path);
            var missing = StreamLocator.GetStreamReader(requested, contentType, extensions: extensions);
            using var missingReader = missing.StreamReader;
            Assert.That(missingReader, Is.Null);
            Assert.That(missing.FileName, Is.EqualTo(requested));
            Assert.That(StreamLocator.HasStream(requested, contentType, extensions: extensions), Is.EqualTo((false, requested)));
        }

        /// <summary>
        /// Verifies script and template resources are found with inferred extensions.
        /// </summary>
        [TestCase(true)]
        [TestCase(false)]
        public void EmbeddedResourcesUseExtensionProbing(bool script)
        {
            var assemblies = new[] { typeof(StreamLocatorTest).Assembly };
            var result = script
                ? StreamLocator.GetScriptStreamReader("ValidEntity", assemblies, StreamLocator.YamlJsonExtensions)
                : StreamLocator.GetTemplateStreamReader("EntityExample", assemblies, StreamLocator.HandlebarsExtensions);
            using var reader = result.StreamReader;
            Assert.That(reader, Is.Not.Null);
            Assert.That(reader.ReadToEnd(), Does.Contain(script ? "configType:" : "{{Name}}"));
            Assert.That(result.FileName, Is.EqualTo(script ? "ValidEntity.yaml" : "EntityExample.hbs"));
            var exists = script
                ? StreamLocator.HasScriptStream("ValidEntity", assemblies, StreamLocator.YamlJsonExtensions)
                : StreamLocator.HasTemplateStream("EntityExample", assemblies, StreamLocator.HandlebarsExtensions);
            Assert.That(exists, Is.EqualTo((true, result.FileName)));
        }

        /// <summary>
        /// Verifies missing lookups preserve the requested name and return absence without creating files.
        /// </summary>
        [Test]
        public void MissingResourcePreservesRequestedName()
        {
            using var directory = new TestDirectory();
            var name = Path.Combine(directory.DirectoryPath, "missing");
            var result = StreamLocator.GetStreamReader(name, extensions: StreamLocator.YamlJsonExtensions);
            Assert.That(result.StreamReader, Is.Null);
            Assert.That(result.FileName, Is.EqualTo(name));
            Assert.That(StreamLocator.HasStream(name, extensions: StreamLocator.YamlJsonExtensions), Is.EqualTo((false, name)));
            Assert.That(Directory.GetFiles(directory.DirectoryPath), Is.Empty);
        }

        /// <summary>
        /// Verifies null and empty resource names are rejected consistently by read and existence APIs.
        /// </summary>
        [TestCase(null)]
        [TestCase("")]
        public void MissingFileNameIsRejected(string name)
        {
            Assert.That(Assert.Throws<ArgumentNullException>((System.Action)(() => StreamLocator.GetStreamReader(name))).ParamName, Is.EqualTo("fileName"));
            Assert.That(Assert.Throws<ArgumentNullException>((System.Action)(() => StreamLocator.HasStream(name))).ParamName, Is.EqualTo("fileName"));
        }
    }
}
