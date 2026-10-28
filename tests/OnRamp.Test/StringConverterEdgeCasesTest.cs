using NUnit.Framework;
using OnRamp.Utility;
using System;
using System.Globalization;

namespace OnRamp.Test
{
    /// <summary>
    /// Verifies empty input, malformed comments, culture independence, and string customization boundaries.
    /// </summary>
    [TestFixture]
    [NonParallelizable]
    public class StringConverterEdgeCasesTest
    {
        /// <summary>
        /// Verifies case and comment converters preserve null and empty input.
        /// </summary>
        [TestCase(null)]
        [TestCase("")]
        public void EmptyInputIsPreserved(string value)
        {
            foreach (var ignoreSpecialNames in new[] { false, true })
            {
                Assert.That(StringConverter.ToCamelCase(value, ignoreSpecialNames), Is.EqualTo(value));
                Assert.That(StringConverter.ToPascalCase(value, ignoreSpecialNames), Is.EqualTo(value));
                Assert.That(StringConverter.ToPrivateCase(value, ignoreSpecialNames), Is.EqualTo(value));
                Assert.That(StringConverter.ToSentenceCase(value, ignoreSpecialNames), Is.EqualTo(value));
                Assert.That(StringConverter.ToSnakeCase(value, ignoreSpecialNames), Is.EqualTo(value));
                Assert.That(StringConverter.ToKebabCase(value, ignoreSpecialNames), Is.EqualTo(value));
            }
            Assert.That(StringConverter.ToPastTense(value), Is.EqualTo(value));
            Assert.That(StringConverter.ToComments(value), Is.EqualTo(value));
            Assert.That(StringConverter.ToSeeComments(value), Is.EqualTo(value));
        }

        /// <summary>
        /// Verifies single-letter identifiers do not require multiple characters.
        /// </summary>
        [TestCase("A", "a", "A")]
        [TestCase("a", "a", "A")]
        public void SingleCharacterIdentifiersAreSupported(string value, string lower, string upper)
        {
            Assert.That(StringConverter.ToCamelCase(value), Is.EqualTo(lower));
            Assert.That(StringConverter.ToPascalCase(value), Is.EqualTo(upper));
            Assert.That(StringConverter.ToPrivateCase(value), Is.EqualTo($"_{lower}"));
            Assert.That(StringConverter.ToSentenceCase(value), Is.EqualTo(upper));
            Assert.That(StringConverter.ToSnakeCase(value), Is.EqualTo(lower));
            Assert.That(StringConverter.ToKebabCase(value), Is.EqualTo(lower));
        }

        /// <summary>
        /// Verifies unmatched comment delimiters raise errors rather than emitting malformed XML.
        /// </summary>
        [TestCase("See {{Type.")]
        [TestCase("See Type}}.")]
        [TestCase("}} before {{")]
        [TestCase("{{Valid}} then {{Missing")]
        [TestCase("{{List<Dictionary<string,int>>}} then {{Missing")]
        [TestCase("{{List<Dictionary<string,int>>}} then stray }}")]
        public void MalformedCommentDelimitersAreRejected(string text)
        {
            var ex = Assert.Throws<CodeGenException>((System.Action)(() => StringConverter.ToComments(text)));
            Assert.That(ex.Message, Does.Contain("parameter mismatch"));
        }

        /// <summary>
        /// Verifies multiple and repeated references are converted without changing ordinary text.
        /// </summary>
        [Test]
        public void CommentsConvertRepeatedReferences()
        {
            Assert.That(StringConverter.ToComments("Plain text."), Is.EqualTo("Plain text."));
            Assert.That(StringConverter.ToComments("{{Type}} then {{List<int>}} and {{Type}}."),
                Is.EqualTo("<see cref=\"Type\"/> then <see cref=\"List{int}\"/> and <see cref=\"Type\"/>."));
            Assert.That(StringConverter.ToSeeComments("List<Dictionary<string,int>>"),
                Is.EqualTo("<see cref=\"List{Dictionary{string,int}}\"/>"));
        }

        /// <summary>
        /// Verifies generated nested generic braces are not reparsed as comment delimiters.
        /// </summary>
        [TestCase("See {{List<Dictionary<string,int>>}}.", "See <see cref=\"List{Dictionary{string,int}}\"/>.")]
        [TestCase("{{List<List<int>>}}{{Type}}", "<see cref=\"List{List{int}}\"/><see cref=\"Type\"/>")]
        [TestCase("{{List<List<int>>}} and {{List<List<int>>}}", "<see cref=\"List{List{int}}\"/> and <see cref=\"List{List{int}}\"/>")]
        public void CommentsShouldSupportNestedGenericReferences(string text, string expected)
        {
            Assert.That(StringConverter.ToComments(text), Is.EqualTo(expected));
        }

        /// <summary>
        /// Verifies identifier casing remains invariant under Turkish casing rules.
        /// </summary>
        [Test]
        public void IdentifierCasingDoesNotDependOnCurrentCulture()
        {
            var original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
                Assert.That(StringConverter.ToCamelCase("InterfaceId"), Is.EqualTo("interfaceId"));
                Assert.That(StringConverter.ToPascalCase("interfaceId"), Is.EqualTo("InterfaceId"));
                Assert.That(StringConverter.ToSnakeCase("InterfaceId"), Is.EqualTo("interface_id"));
                Assert.That(StringConverter.ToKebabCase("InterfaceId"), Is.EqualTo("interface-id"));
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        /// <summary>
        /// Verifies temporary special-case customization is honored and global state is restored.
        /// </summary>
        [Test]
        public void CustomSpecialCaseHandlerCanBeReplacedAndDisabled()
        {
            var original = StringConverter.SpecialCaseHandler;
            try
            {
                StringConverter.SpecialCaseHandler = text => text.Replace("Api", "API", StringComparison.Ordinal);
                Assert.That(StringConverter.ToSentenceCase("ApiName"), Is.EqualTo("API Name"));
                Assert.That(StringConverter.ToSentenceCase("ApiName", true), Is.EqualTo("Api Name"));
                StringConverter.SpecialCaseHandler = null;
                Assert.That(StringConverter.ToSentenceCase("ETagName"), Is.EqualTo("E Tag Name"));
            }
            finally
            {
                StringConverter.SpecialCaseHandler = original;
            }
        }

        /// <summary>
        /// Verifies past-tense handling for regular endings, existing past tense, and dictionary entries.
        /// </summary>
        [TestCase("like", "liked")]
        [TestCase("play", "played")]
        [TestCase("walk", "walked")]
        [TestCase("walked", "walked")]
        [TestCase("send", "sent")]
        [TestCase("Send", "Sent")]
        public void PastTenseHandlesRuleBoundaries(string value, string expected) =>
            Assert.That(StringConverter.ToPastTense(value), Is.EqualTo(expected));
    }
}
