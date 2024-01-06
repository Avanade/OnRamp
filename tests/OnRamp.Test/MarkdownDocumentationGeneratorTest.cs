using OnRamp.Test.Config;
using OnRamp.Utility;
using NUnit.Framework;
using System.IO;

namespace OnRamp.Test
{
    [TestFixture]
    public class MarkdownDocumentationGeneratorTest
    {
        [Test]
        public void Generate()
        {
            if (Directory.Exists("MSG"))
                Directory.Delete("MSG", true);

            Directory.CreateDirectory("MSG");
            MarkdownDocumentationGenerator.Generate<EntityConfig>(directory: "MSG", addBreaksBetweenSections: true);

            var fn = Path.Combine("MSG", "Entity.md");
            Assert.That(File.Exists(fn), Is.True);
            Assert.That(File.ReadAllText(fn), Is.EqualTo(File.ReadAllText(Path.Combine("Expected", "Entity.md"))));

            fn = Path.Combine("MSG", "Property.md");
            Assert.That(File.Exists(fn), Is.True);
            Assert.That(File.ReadAllText(fn), Is.EqualTo(File.ReadAllText(Path.Combine("Expected", "Property.md"))));
        }
    }
}