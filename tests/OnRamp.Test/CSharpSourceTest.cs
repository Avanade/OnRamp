using NUnit.Framework;
using OnRamp.Utility;
using System;
using System.Globalization;

namespace OnRamp.Test
{
    [TestFixture]
    public class CSharpSourceTest
    {
        [TestCase("", "\"\"")]
        [TestCase("A\"B\n", "\"A\\\"B\\n\"")]
        [TestCase("\\path/", "\"\\\\path/\"")]
        [TestCase("\0\a\b\f\n\r\t\v", "\"\\0\\a\\b\\f\\n\\r\\t\\v\"")]
        [TestCase("\0F\u0001A", "\"\\0F\\u0001A\"")]
        [TestCase("\u007f\u0085\u2028\u2029", "\"\\u007F\\u0085\\u2028\\u2029\"")]
        [TestCase("\ud83d\ude00", "\"\\uD83D\\uDE00\"")]
        public void EmitsCompleteRegularLiteral(string value, string expected) =>
            Assert.That(CSharpSource.StringLiteral(value), Is.EqualTo(expected));

        [Test]
        public void PreservesLoneSurrogates()
        {
            Assert.That(CSharpSource.StringLiteral(new string(new[] { '\ud800', 'A', '\udfff' })),
                Is.EqualTo("\"\\uD800A\\uDFFF\""));
        }

        [Test]
        public void EveryNonAsciiCodeUnitUsesAnInvariantFixedWidthEscape()
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
                for (var code = 127; code <= char.MaxValue; code++)
                    Assert.That(CSharpSource.StringLiteral(new string((char)code, 1)),
                        Is.EqualTo("\"\\u" + code.ToString("X4", CultureInfo.InvariantCulture) + "\""));
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        [Test]
        public void RejectsNull() =>
            Assert.That(Assert.Throws<ArgumentNullException>((Action)(() => CSharpSource.StringLiteral(null))).ParamName, Is.EqualTo("value"));
    }
}
