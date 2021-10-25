using NUnit.Framework;
using OnRamp.Utility;

namespace OnRamp.Test
{
    [TestFixture]
    public class HandlebarsHelpersTest
    {
        [Test]
        public void IfEq()
        {
            var g = new HandlebarsCodeGenerator("{{#ifeq Name 'Bob'}}Hi Fella.{{else}}Hi {{Name}}.{{/ifeq}}");
            Assert.AreEqual("Hi Fella.", g.Generate(new { Name = "Bob" }));
            Assert.AreEqual("Hi Mary.", g.Generate(new { Name = "Mary" }));
        }

        [Test]
        public void IfNe()
        {
            var g = new HandlebarsCodeGenerator("{{#ifne Name 'Bob' 'Gary'}}Hi {{Name}}.{{else}}Hi Fella.{{/ifne}}");
            Assert.AreEqual("Hi Fella.", g.Generate(new { Name = "Bob" }));
            Assert.AreEqual("Hi Fella.", g.Generate(new { Name = "Gary" }));
            Assert.AreEqual("Hi Mary.", g.Generate(new { Name = "Mary" }));
        }

        [Test]
        public void IfLe()
        {
            var g = new HandlebarsCodeGenerator("{{#ifle Amount -1}}Negative{{else}}Positive{{/ifle}}");
            Assert.AreEqual("Positive", g.Generate(new { Amount = 1 }));
            Assert.AreEqual("Positive", g.Generate(new { Amount = 0 }));
            Assert.AreEqual("Negative", g.Generate(new { Amount = -1 }));
            Assert.AreEqual("Negative", g.Generate(new { Amount = -2 }));
        }

        [Test]
        public void IfGe()
        {
            var g = new HandlebarsCodeGenerator("{{#ifge Amount 0}}Positive{{else}}Negative{{/ifge}}");
            Assert.AreEqual("Positive", g.Generate(new { Amount = 1 }));
            Assert.AreEqual("Positive", g.Generate(new { Amount = 0 }));
            Assert.AreEqual("Negative", g.Generate(new { Amount = -1 }));
            Assert.AreEqual("Negative", g.Generate(new { Amount = -2 }));
        }

        [Test]
        public void IfVal()
        {
            var g = new HandlebarsCodeGenerator("{{#ifval First Last}}true{{else}}false{{/ifval}}");
            Assert.AreEqual("false", g.Generate(new { First = (string)null, Last = (string)null }));
            Assert.AreEqual("false", g.Generate(new { First = "Jane", Last = (string)null }));
            Assert.AreEqual("false", g.Generate(new { First = (string)null, Last = "Doe" }));
            Assert.AreEqual("true", g.Generate(new { First = "Jane", Last = "Doe" }));
        }

        [Test]
        public void IfNull()
        {
            var g = new HandlebarsCodeGenerator("{{#ifnull First Last}}true{{else}}false{{/ifnull}}");
            Assert.AreEqual("true", g.Generate(new { First = (string)null, Last = (string)null }));
            Assert.AreEqual("false", g.Generate(new { First = "Jane", Last = (string)null }));
            Assert.AreEqual("false", g.Generate(new { First = (string)null, Last = "Doe" }));
            Assert.AreEqual("false", g.Generate(new { First = "Jane", Last = "Doe" }));
        }

        [Test]
        public void IfOr()
        {
            var g = new HandlebarsCodeGenerator("{{#ifor Val1 Val2}}true{{else}}false{{/ifor}}");
            Assert.AreEqual("false", g.Generate(new { Val1 = false, Val2 = false }));
            Assert.AreEqual("true", g.Generate(new { Val1 = true, Val2 = false }));
            Assert.AreEqual("true", g.Generate(new { Val1 = false, Val2 = true }));
            Assert.AreEqual("true", g.Generate(new { Val1 = true, Val2 = true }));

            Assert.AreEqual("false", g.Generate(new { Val1 = false, Val2 = (string)null }));
            Assert.AreEqual("true", g.Generate(new { Val1 = true, Val2 = (string)null }));
            Assert.AreEqual("true", g.Generate(new { Val1 = false, Val2 = "X" }));
            Assert.AreEqual("true", g.Generate(new { Val1 = true, Val2 = "X" }));
        }

        [Test]
        public void Lower()
        {
            var g = new HandlebarsCodeGenerator("{{lower Name}}");
            Assert.AreEqual("etag", g.Generate(new { Name = "ETag" }));
        }

        [Test]
        public void Upper()
        {
            var g = new HandlebarsCodeGenerator("{{upper Name}}");
            Assert.AreEqual("ETAG", g.Generate(new { Name = "ETag" }));
        }

        [Test]
        public void Camel()
        {
            var g = new HandlebarsCodeGenerator("{{camel Name}}");
            Assert.AreEqual("etag", g.Generate(new { Name = "ETag" }));

            g = new HandlebarsCodeGenerator("{{camelx Name}}");
            Assert.AreEqual("eTag", g.Generate(new { Name = "ETag" }));
        }

        [Test]
        public void Pascal()
        {
            var g = new HandlebarsCodeGenerator("{{pascal Name}}");
            Assert.AreEqual("ETag", g.Generate(new { Name = "etag" }));

            g = new HandlebarsCodeGenerator("{{pascalx Name}}");
            Assert.AreEqual("Etag", g.Generate(new { Name = "etag" }));
        }

        [Test]
        public void Private()
        {
            var g = new HandlebarsCodeGenerator("{{private Name}}");
            Assert.AreEqual("_etag", g.Generate(new { Name = "ETag" }));

            g = new HandlebarsCodeGenerator("{{privatex Name}}");
            Assert.AreEqual("_eTag", g.Generate(new { Name = "ETag" }));
        }

        [Test]
        public void Sentence()
        {
            var g = new HandlebarsCodeGenerator("{{sentence Name}}");
            Assert.AreEqual("ETag Name", g.Generate(new { Name = "ETagName" }));

            g = new HandlebarsCodeGenerator("{{sentencex Name}}");
            Assert.AreEqual("E Tag Name", g.Generate(new { Name = "ETagName" }));
        }

        [Test]
        public void Snake()
        {
            var g = new HandlebarsCodeGenerator("{{snake Name}}");
            Assert.AreEqual("etag_name", g.Generate(new { Name = "ETagName" }));

            g = new HandlebarsCodeGenerator("{{snakex Name}}");
            Assert.AreEqual("e_tag_name", g.Generate(new { Name = "ETagName" }));
        }

        [Test]
        public void Kebab()
        {
            var g = new HandlebarsCodeGenerator("{{kebab Name}}");
            Assert.AreEqual("etag-name", g.Generate(new { Name = "ETagName" }));

            g = new HandlebarsCodeGenerator("{{kebabx Name}}");
            Assert.AreEqual("e-tag-name", g.Generate(new { Name = "ETagName" }));
        }

        [Test]
        public void SeeComments()
        {
            var g = new HandlebarsCodeGenerator("{{see-comments Name}}");
            Assert.AreEqual("<see cref=\"string\"/>", g.Generate(new { Name = "string" }));
        }

        [Test]
        public void Indent()
        {
            var g = new HandlebarsCodeGenerator("{{indent 4}}{{Name}}");
            Assert.AreEqual("    Bob", g.Generate(new { Name = "Bob" }));
        }

        [Test]
        public void Add()
        {
            var g = new HandlebarsCodeGenerator("{{add 4 '10' Count}}");
            Assert.AreEqual("12", g.Generate(new { Count = -2 }));
        }

        [Test]
        public void Debug()
        {
            var g = new HandlebarsCodeGenerator("{{debug 'Name: {0}.' Name}}");
            Assert.AreEqual("", g.Generate(new { Name = "Nancy" }));
        }

        [Test]
        public void LogInfo()
        {
            var g = new HandlebarsCodeGenerator("{{log-info 'Name: {0}.' Name}}");
            Assert.AreEqual("", g.Generate(new { Name = "Bob" }));
        }

        [Test]
        public void LogWarning()
        {
            var g = new HandlebarsCodeGenerator("{{log-warning 'Name: {0}.' Name}}");
            Assert.AreEqual("", g.Generate(new { Name = "Jane" }));
        }

        [Test]
        public void LogError()
        {
            var g = new HandlebarsCodeGenerator("{{log-error 'Name: {0}.' Name}}");
            Assert.AreEqual("", g.Generate(new { Name = "Bruce" }));
        }

        [Test]
        public void LogDebug()
        {
            var g = new HandlebarsCodeGenerator("{{log-debug 'Name: {0}.' Name}}");
            Assert.AreEqual("", g.Generate(new { Name = "Grace" }));
        }
    }
}