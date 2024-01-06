using NUnit.Framework;
using NUnit.Framework.Legacy;
using OnRamp.Utility;
using System;

namespace OnRamp.Test
{
    [TestFixture]
    public class HandlebarsHelpersTest
    {
        [Test]
        public void IfEq()
        {
            var g = new HandlebarsCodeGenerator("{{#ifeq Name 'Bob'}}Hi Fella.{{else}}Hi {{Name}}.{{/ifeq}}");
            ClassicAssert.AreEqual("Hi Fella.", g.Generate(new { Name = "Bob" }));
            ClassicAssert.AreEqual("Hi Mary.", g.Generate(new { Name = "Mary" }));
        }

        [Test]
        public void IfNe()
        {
            var g = new HandlebarsCodeGenerator("{{#ifne Name 'Bob' 'Gary'}}Hi {{Name}}.{{else}}Hi Fella.{{/ifne}}");
            ClassicAssert.AreEqual("Hi Fella.", g.Generate(new { Name = "Bob" }));
            ClassicAssert.AreEqual("Hi Fella.", g.Generate(new { Name = "Gary" }));
            ClassicAssert.AreEqual("Hi Mary.", g.Generate(new { Name = "Mary" }));
        }

        [Test]
        public void IfLe()
        {
            var g = new HandlebarsCodeGenerator("{{#ifle Amount -1}}Negative{{else}}Positive{{/ifle}}");
            ClassicAssert.AreEqual("Positive", g.Generate(new { Amount = 1 }));
            ClassicAssert.AreEqual("Positive", g.Generate(new { Amount = 0 }));
            ClassicAssert.AreEqual("Negative", g.Generate(new { Amount = -1 }));
            ClassicAssert.AreEqual("Negative", g.Generate(new { Amount = -2 }));
        }

        [Test]
        public void IfGe()
        {
            var g = new HandlebarsCodeGenerator("{{#ifge Amount 0}}Positive{{else}}Negative{{/ifge}}");
            ClassicAssert.AreEqual("Positive", g.Generate(new { Amount = 1 }));
            ClassicAssert.AreEqual("Positive", g.Generate(new { Amount = 0 }));
            ClassicAssert.AreEqual("Negative", g.Generate(new { Amount = -1 }));
            ClassicAssert.AreEqual("Negative", g.Generate(new { Amount = -2 }));
        }

        [Test]
        public void IfVal()
        {
            var g = new HandlebarsCodeGenerator("{{#ifval First Last}}true{{else}}false{{/ifval}}");
            ClassicAssert.AreEqual("false", g.Generate(new { First = (string)null, Last = (string)null }));
            ClassicAssert.AreEqual("false", g.Generate(new { First = "Jane", Last = (string)null }));
            ClassicAssert.AreEqual("false", g.Generate(new { First = (string)null, Last = "Doe" }));
            ClassicAssert.AreEqual("true", g.Generate(new { First = "Jane", Last = "Doe" }));
        }

        [Test]
        public void IfNull()
        {
            var g = new HandlebarsCodeGenerator("{{#ifnull First Last}}true{{else}}false{{/ifnull}}");
            ClassicAssert.AreEqual("true", g.Generate(new { First = (string)null, Last = (string)null }));
            ClassicAssert.AreEqual("false", g.Generate(new { First = "Jane", Last = (string)null }));
            ClassicAssert.AreEqual("false", g.Generate(new { First = (string)null, Last = "Doe" }));
            ClassicAssert.AreEqual("false", g.Generate(new { First = "Jane", Last = "Doe" }));
        }

        [Test]
        public void IfOr()
        {
            var g = new HandlebarsCodeGenerator("{{#ifor Val1 Val2}}true{{else}}false{{/ifor}}");
            ClassicAssert.AreEqual("false", g.Generate(new { Val1 = false, Val2 = false }));
            ClassicAssert.AreEqual("true", g.Generate(new { Val1 = true, Val2 = false }));
            ClassicAssert.AreEqual("true", g.Generate(new { Val1 = false, Val2 = true }));
            ClassicAssert.AreEqual("true", g.Generate(new { Val1 = true, Val2 = true }));

            ClassicAssert.AreEqual("false", g.Generate(new { Val1 = false, Val2 = (string)null }));
            ClassicAssert.AreEqual("true", g.Generate(new { Val1 = true, Val2 = (string)null }));
            ClassicAssert.AreEqual("true", g.Generate(new { Val1 = false, Val2 = "X" }));
            ClassicAssert.AreEqual("true", g.Generate(new { Val1 = true, Val2 = "X" }));
        }

        [Test]
        public void Lower()
        {
            var g = new HandlebarsCodeGenerator("{{lower Name}}");
            ClassicAssert.AreEqual("etag", g.Generate(new { Name = "ETag" }));
        }

        [Test]
        public void Upper()
        {
            var g = new HandlebarsCodeGenerator("{{upper Name}}");
            ClassicAssert.AreEqual("ETAG", g.Generate(new { Name = "ETag" }));
        }

        [Test]
        public void Camel()
        {
            var g = new HandlebarsCodeGenerator("{{camel Name}}");
            ClassicAssert.AreEqual("etag", g.Generate(new { Name = "ETag" }));

            g = new HandlebarsCodeGenerator("{{camelx Name}}");
            ClassicAssert.AreEqual("eTag", g.Generate(new { Name = "ETag" }));
        }

        [Test]
        public void Pascal()
        {
            var g = new HandlebarsCodeGenerator("{{pascal Name}}");
            ClassicAssert.AreEqual("ETag", g.Generate(new { Name = "etag" }));

            g = new HandlebarsCodeGenerator("{{pascalx Name}}");
            ClassicAssert.AreEqual("Etag", g.Generate(new { Name = "etag" }));
        }

        [Test]
        public void Private()
        {
            var g = new HandlebarsCodeGenerator("{{private Name}}");
            ClassicAssert.AreEqual("_etag", g.Generate(new { Name = "ETag" }));

            g = new HandlebarsCodeGenerator("{{privatex Name}}");
            ClassicAssert.AreEqual("_eTag", g.Generate(new { Name = "ETag" }));
        }

        [Test]
        public void Sentence()
        {
            var g = new HandlebarsCodeGenerator("{{sentence Name}}");
            ClassicAssert.AreEqual("ETag Name", g.Generate(new { Name = "ETagName" }));

            g = new HandlebarsCodeGenerator("{{sentencex Name}}");
            ClassicAssert.AreEqual("E Tag Name", g.Generate(new { Name = "ETagName" }));
        }

        [Test]
        public void Snake()
        {
            var g = new HandlebarsCodeGenerator("{{snake Name}}");
            ClassicAssert.AreEqual("etag_name", g.Generate(new { Name = "ETagName" }));

            g = new HandlebarsCodeGenerator("{{snakex Name}}");
            ClassicAssert.AreEqual("e_tag_name", g.Generate(new { Name = "ETagName" }));
        }

        [Test]
        public void Kebab()
        {
            var g = new HandlebarsCodeGenerator("{{kebab Name}}");
            ClassicAssert.AreEqual("etag-name", g.Generate(new { Name = "ETagName" }));

            g = new HandlebarsCodeGenerator("{{kebabx Name}}");
            ClassicAssert.AreEqual("e-tag-name", g.Generate(new { Name = "ETagName" }));
        }

        [Test]
        public void PastTense()
        {
            var g = new HandlebarsCodeGenerator("{{past-tense Name}}");
            ClassicAssert.AreEqual("Ordered", g.Generate(new { Name = "Order" }));
        }

        [Test]
        public void Pluralize()
        {
            var g = new HandlebarsCodeGenerator("{{pluralize Name}}");
            ClassicAssert.AreEqual("Orders", g.Generate(new { Name = "Order" }));
        }

        [Test]
        public void Singularize()
        {
            var g = new HandlebarsCodeGenerator("{{singularize Name}}");
            ClassicAssert.AreEqual("Order", g.Generate(new { Name = "Orders" }));
        }

        [Test]
        public void SeeComments()
        {
            var g = new HandlebarsCodeGenerator("{{see-comments Name}}");
            ClassicAssert.AreEqual("<see cref=\"string\"/>", g.Generate(new { Name = "string" }));
        }

        [Test]
        public void Indent()
        {
            var g = new HandlebarsCodeGenerator("{{indent 4}}{{Name}}");
            ClassicAssert.AreEqual("    Bob", g.Generate(new { Name = "Bob" }));
        }

        [Test]
        public void Add()
        {
            var g = new HandlebarsCodeGenerator("{{add 4 '10' Count}}");
            ClassicAssert.AreEqual("12", g.Generate(new { Count = -2 }));
        }

        [Test]
        public void Add_Index()
        {
            var g = new HandlebarsCodeGenerator("{{#each .}}{{.}}{{add @index 1}}{{/each}}");
            ClassicAssert.AreEqual("a1b2", g.Generate(new System.Collections.Generic.List<string> { "a", "b" }));
        }

        public class SetData { public int Count { get; set; } internal bool Check { get; set; } public decimal Sum { get; set; }  }

        [Test]
        public void SetValue()
        {
            var sd = new SetData();
            var g = new HandlebarsCodeGenerator("{{set-value 'Count' 88}}");
            ClassicAssert.AreEqual("", g.Generate(sd));
            ClassicAssert.AreEqual(88, sd.Count);

            g = new HandlebarsCodeGenerator("xx{{set-value 'Check' true}}yy");
            ClassicAssert.AreEqual("xxyy", g.Generate(sd));
            ClassicAssert.AreEqual(true, sd.Check);
        }

        [Test]
        public void AddValue()
        {
            var sd = new SetData();
            var g = new HandlebarsCodeGenerator("{{add-value 'Sum'}}");
            ClassicAssert.AreEqual("", g.Generate(sd));
            ClassicAssert.AreEqual(1, sd.Sum);

            g = new HandlebarsCodeGenerator("{{add-value 'Sum' 3 -3.5 '8.4'}}");
            ClassicAssert.AreEqual("", g.Generate(sd));
            ClassicAssert.AreEqual(8.9m, sd.Sum);
        }

        [Test]
        public void Format()
        {
            var dt = new DateTime(2021, 10, 26, 08, 55, 16);
            var g = new HandlebarsCodeGenerator("Date is {{format '{0:yyyy-MM-dd HH:mm:ss}' Date}}.");
            ClassicAssert.AreEqual($"Date is {dt:yyyy-MM-dd HH:mm:ss}.", g.Generate(new { Date = dt }));
        }

        [Test]
        public void Debug()
        {
            var g = new HandlebarsCodeGenerator("{{debug 'Name: {0}.' Name}}");
            ClassicAssert.AreEqual("", g.Generate(new { Name = "Nancy" }));
        }

        [Test]
        public void LogInfo()
        {
            var g = new HandlebarsCodeGenerator("{{log-info 'Name: {0}.' Name}}");
            ClassicAssert.AreEqual("", g.Generate(new { Name = "Bob" }));
        }

        [Test]
        public void LogWarning()
        {
            var g = new HandlebarsCodeGenerator("{{log-warning 'Name: {0}.' Name}}");
            ClassicAssert.AreEqual("", g.Generate(new { Name = "Jane" }));
        }

        [Test]
        public void LogError()
        {
            var g = new HandlebarsCodeGenerator("{{log-error 'Name: {0}.' Name}}");
            ClassicAssert.AreEqual("", g.Generate(new { Name = "Bruce" }));
        }

        [Test]
        public void LogDebug()
        {
            var g = new HandlebarsCodeGenerator("{{log-debug 'Name: {0}.' Name}}");
            ClassicAssert.AreEqual("", g.Generate(new { Name = "Grace" }));
        }
    }
}