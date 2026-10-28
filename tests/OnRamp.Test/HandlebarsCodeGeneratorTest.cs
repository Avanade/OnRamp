using NUnit.Framework;
using OnRamp.Utility;
using System;
using System.IO;
using System.Text;

namespace OnRamp.Test
{
    /// <summary>
    /// Verifies template compilation, reuse, missing bindings, and helper error handling.
    /// </summary>
    [TestFixture]
    public class HandlebarsCodeGeneratorTest
    {
        /// <summary>
        /// Verifies string and reader constructors reject null template input.
        /// </summary>
        [Test]
        public void ConstructorsRejectNullInput()
        {
            Assert.That(Assert.Throws<ArgumentNullException>((System.Action)(() => new HandlebarsCodeGenerator((string)null))).ParamName, Is.EqualTo("templateContent"));
            Assert.That(Assert.Throws<ArgumentNullException>((System.Action)(() => new HandlebarsCodeGenerator((StreamReader)null))).ParamName, Is.EqualTo("sr"));
        }

        /// <summary>
        /// Verifies templates loaded from streams are reusable and leave the caller's reader open.
        /// </summary>
        [Test]
        public void ReaderTemplateIsReusableAndPreservesCodeCharacters()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{{Name}}: {{Type}}"));
            using var reader = new StreamReader(stream);
            var generator = new HandlebarsCodeGenerator(reader);
            Assert.That(generator.Generate(new { Name = "First", Type = "List<T> & \"quoted\"" }), Is.EqualTo("First: List<T> & \"quoted\""));
            Assert.That(generator.Generate(new { Name = "Second", Type = "int" }), Is.EqualTo("Second: int"));
            Assert.That(reader.Peek(), Is.EqualTo(-1));
            Assert.That(stream.CanRead, Is.True);
        }

        /// <summary>
        /// Verifies explicit null bindings render empty text but missing bindings raise contextual errors.
        /// </summary>
        [Test]
        public void MissingBindingsAreNotSilentlyRenderedAsEmptyStrings()
        {
            var generator = new HandlebarsCodeGenerator("{{Name}}");
            Assert.That(generator.Generate(new { Name = (string)null }), Is.Empty);
            var ex = Assert.Throws<CodeGenException>((System.Action)(() => generator.Generate(new { Other = "value" })));
            Assert.That(ex.Message, Is.EqualTo("Handlebars template references 'Name' which is undefined."));
        }

        /// <summary>
        /// Verifies helper argument binding failures identify the helper and missing member.
        /// </summary>
        [TestCase("{{lower Missing}}", "lower")]
        [TestCase("{{upper Missing}}", "upper")]
        [TestCase("{{add Missing 1}}", "add")]
        [TestCase("{{#ifeq Missing 'value'}}yes{{/ifeq}}", "#ifeq")]
        public void HelpersRejectUndefinedArguments(string template, string helper)
        {
            var generator = new HandlebarsCodeGenerator(template);
            var ex = Assert.Throws<CodeGenException>((System.Action)(() => generator.Generate(new { Name = "value" })));
            Assert.That(ex.Message, Does.Contain($"function '{helper}'").And.Contain("'Missing'").And.Contain("undefined"));
        }

        /// <summary>
        /// Verifies mutation helpers report argument, property, and assignment errors.
        /// </summary>
        [TestCase("{{set-value 'Count'}}", "only supports two arguments")]
        [TestCase("{{set-value 42 1}}", "first argument must be a string")]
        [TestCase("{{set-value 'Missing' 1}}", "property 'Missing' does not exist")]
        [TestCase("{{set-value 'Count' 'invalid'}}", "set value error")]
        [TestCase("{{add-value 42 1}}", "first argument must be a string")]
        [TestCase("{{add-value 'Missing' 1}}", "property 'Missing' does not exist")]
        [TestCase("{{add-value 'Count' 1}}", "get value error")]
        [TestCase("{{add-value 'ReadOnly' 1}}", "set value error")]
        public void MutationHelpersExposeContextualErrors(string template, string message)
        {
            var generator = new HandlebarsCodeGenerator(template);
            var ex = Assert.Throws<CodeGenException>((System.Action)(() => generator.Generate(new MutationValue())));
            Assert.That(ex.Message, Does.Contain(message));
        }

        /// <summary>
        /// Verifies nullable context and supplemental data remain supported by compiled templates.
        /// </summary>
        [Test]
        public void TemplatesSupportNullContextAndData()
        {
            var generator = new HandlebarsCodeGenerator("literal");
            Assert.That(generator.Generate(null), Is.EqualTo("literal"));
            Assert.That(generator.Generate(null, null), Is.EqualTo("literal"));
            Assert.That(new HandlebarsCodeGenerator("{{@Name}}").Generate(null, new { Name = "data" }), Is.EqualTo("data"));
        }

        /// <summary>
        /// Verifies mutation helpers report missing context rather than dereferencing null.
        /// </summary>
        [TestCase("set-value")]
        [TestCase("add-value")]
        public void MutationHelpersRejectNullContext(string helper)
        {
            var generator = new HandlebarsCodeGenerator("{{" + helper + " 'Count' 1}}");
            var ex = Assert.Throws<CodeGenException>((System.Action)(() => generator.Generate(null)));
            Assert.That(ex.Message, Is.EqualTo($"Handlebars template invokes function '{helper}' with a null context."));
        }

        /// <summary>
        /// Verifies nullable comparison operands retain equality and ordering semantics.
        /// </summary>
        [TestCase("{{#ifeq Value null}}yes{{else}}no{{/ifeq}}", "yes")]
        [TestCase("{{#ifne Value null}}yes{{else}}no{{/ifne}}", "no")]
        [TestCase("{{#ifle Value 1}}yes{{else}}no{{/ifle}}", "yes")]
        [TestCase("{{#ifge 1 Value}}yes{{else}}no{{/ifge}}", "yes")]
        public void ComparisonHelpersSupportNullOperands(string template, string expected)
        {
            Assert.That(new HandlebarsCodeGenerator(template).Generate(new { Value = (object)null }), Is.EqualTo(expected));
        }

        /// <summary>
        /// Verifies null format arguments produce explicit errors, including logging helpers.
        /// </summary>
        [TestCase("{{format Value}}")]
        [TestCase("{{format Value 1}}")]
        [TestCase("{{log-info Value}}")]
        [TestCase("{{log-warning Value 1}}")]
        [TestCase("{{log-error Value}}")]
        public void FormattingHelpersRejectNullFormat(string template)
        {
            var generator = new HandlebarsCodeGenerator(template);
            var ex = Assert.Throws<CodeGenException>((System.Action)(() => generator.Generate(new { Value = (string)null })));
            Assert.That(ex.Message, Is.EqualTo("Handlebars template requires a non-null format argument."));
        }

        /// <summary>
        /// Verifies formatting permits omitted arguments, scalar values, and null substitution values.
        /// </summary>
        [TestCase("{{format}}", "")]
        [TestCase("{{format 42}}", "42")]
        [TestCase("{{format 'A{0}B' Value}}", "AB")]
        public void FormattingHelpersPreserveSupportedArguments(string template, string expected)
        {
            Assert.That(new HandlebarsCodeGenerator(template).Generate(new { Value = (string)null }), Is.EqualTo(expected));
        }

        /// <summary>
        /// Provides mutable and read-only members for helper error tests.
        /// </summary>
        public class MutationValue
        {
            /// <summary>
            /// Gets or sets an integer that cannot be read as a boxed decimal.
            /// </summary>
            public int Count { get; set; }

            /// <summary>
            /// Gets a decimal value that cannot be assigned by mutation helpers.
            /// </summary>
            public decimal ReadOnly => 1m;
        }
    }
}
