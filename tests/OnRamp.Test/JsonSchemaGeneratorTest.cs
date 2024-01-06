using OnRamp.Test.Config;
using OnRamp.Utility;
using NUnit.Framework;
using System.IO;

namespace OnRamp.Test
{
    [TestFixture]
    public class JsonSchemaGeneratorTest
    {
        [Test]
        public void Generate()
        {
            if (Directory.Exists("JSG"))
                Directory.Delete("JSG", true);

            Directory.CreateDirectory("JSG");
            var fn = Path.Combine("JSG", "schema.json");

            JsonSchemaGenerator.Generate<EntityConfig>(fn, "Entity Configuration");

            Assert.That(File.Exists(fn), Is.True);
            Assert.That(File.ReadAllText(fn), Is.EqualTo(File.ReadAllText(Path.Combine("Expected", "Schema.json"))));
        }
    }
}