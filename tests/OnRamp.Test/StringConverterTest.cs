using NUnit.Framework;
using NUnit.Framework.Legacy;
using OnRamp.Utility;

namespace OnRamp.Test
{
    [TestFixture]
    public class StringConverterTest
    {
        [Test]
        public void ToCamelCase()
        {
            ClassicAssert.AreEqual("fieldName", StringConverter.ToCamelCase("FieldName"));
            ClassicAssert.AreEqual("etagName", StringConverter.ToCamelCase("ETagName"));
            ClassicAssert.AreEqual("odataName", StringConverter.ToCamelCase("ODataName"));

            ClassicAssert.AreEqual("fieldName", StringConverter.ToCamelCase("FieldName", true));
            ClassicAssert.AreEqual("eTagName", StringConverter.ToCamelCase("ETagName", true));
            ClassicAssert.AreEqual("oDataName", StringConverter.ToCamelCase("ODataName", true));
        }

        [Test]
        public void ToPascalCase()
        {
            ClassicAssert.AreEqual("FieldName", StringConverter.ToPascalCase("fieldName"));
            ClassicAssert.AreEqual("ETagName", StringConverter.ToPascalCase("etagName"));
            ClassicAssert.AreEqual("ODataName", StringConverter.ToPascalCase("odataName"));

            ClassicAssert.AreEqual("FieldName", StringConverter.ToPascalCase("fieldName", true));
            ClassicAssert.AreEqual("EtagName", StringConverter.ToPascalCase("etagName", true));
            ClassicAssert.AreEqual("OdataName", StringConverter.ToPascalCase("odataName", true));
        }

        [Test]
        public void ToPrivateCase()
        {
            ClassicAssert.AreEqual("_fieldName", StringConverter.ToPrivateCase("FieldName"));
            ClassicAssert.AreEqual("_etagName", StringConverter.ToPrivateCase("ETagName"));
            ClassicAssert.AreEqual("_odataName", StringConverter.ToPrivateCase("ODataName"));

            ClassicAssert.AreEqual("_fieldName", StringConverter.ToPrivateCase("FieldName", true));
            ClassicAssert.AreEqual("_eTagName", StringConverter.ToPrivateCase("ETagName", true));
            ClassicAssert.AreEqual("_oDataName", StringConverter.ToPrivateCase("ODataName", true));
        }

        [Test]
        public void ToSentenceCase()
        {
            ClassicAssert.AreEqual("Field Name", StringConverter.ToSentenceCase("FieldName"));
            ClassicAssert.AreEqual("ETag Name", StringConverter.ToSentenceCase("ETagName"));
            ClassicAssert.AreEqual("OData Name", StringConverter.ToSentenceCase("ODataName"));
            ClassicAssert.AreEqual("XML Name", StringConverter.ToSentenceCase("XMLName"));

            ClassicAssert.AreEqual("Field Name", StringConverter.ToSentenceCase("FieldName", true));
            ClassicAssert.AreEqual("E Tag Name", StringConverter.ToSentenceCase("ETagName", true));
            ClassicAssert.AreEqual("O Data Name", StringConverter.ToSentenceCase("ODataName", true));
            ClassicAssert.AreEqual("XML Name OR Other", StringConverter.ToSentenceCase("XMLNameOROther", true));
        }

        [Test]
        public void ToKebabCase()
        {
            ClassicAssert.AreEqual("field-name", StringConverter.ToKebabCase("FieldName"));
            ClassicAssert.AreEqual("etag-name", StringConverter.ToKebabCase("ETagName"));
            ClassicAssert.AreEqual("odata-name", StringConverter.ToKebabCase("ODataName"));
            ClassicAssert.AreEqual("xml-name", StringConverter.ToKebabCase("XMLName"));

            ClassicAssert.AreEqual("field-name", StringConverter.ToKebabCase("FieldName", true));
            ClassicAssert.AreEqual("e-tag-name", StringConverter.ToKebabCase("ETagName", true));
            ClassicAssert.AreEqual("o-data-name", StringConverter.ToKebabCase("ODataName", true));
            ClassicAssert.AreEqual("xml-name", StringConverter.ToKebabCase("XMLName", true));
        }

        [Test]
        public void ToSnakeCase()
        {
            ClassicAssert.AreEqual("field_name", StringConverter.ToSnakeCase("FieldName"));
            ClassicAssert.AreEqual("etag_name", StringConverter.ToSnakeCase("ETagName"));
            ClassicAssert.AreEqual("odata_name", StringConverter.ToSnakeCase("ODataName"));
            ClassicAssert.AreEqual("xml_name", StringConverter.ToSnakeCase("XMLName"));

            ClassicAssert.AreEqual("field_name", StringConverter.ToSnakeCase("FieldName", true));
            ClassicAssert.AreEqual("e_tag_name", StringConverter.ToSnakeCase("ETagName", true));
            ClassicAssert.AreEqual("o_data_name", StringConverter.ToSnakeCase("ODataName", true));
            ClassicAssert.AreEqual("xml_name", StringConverter.ToSnakeCase("XMLName", true));
        }

        [Test]
        public void ToPastTense()
        {
            ClassicAssert.AreEqual("A", StringConverter.ToPastTense("A"));
            ClassicAssert.AreEqual("Be", StringConverter.ToPastTense("Be"));
            ClassicAssert.AreEqual("Pried", StringConverter.ToPastTense("Pry"));
            ClassicAssert.AreEqual("Applied", StringConverter.ToPastTense("Apply"));
            ClassicAssert.AreEqual("Dropped", StringConverter.ToPastTense("Drop"));
            ClassicAssert.AreEqual("Rotted", StringConverter.ToPastTense("Rot"));
            ClassicAssert.AreEqual("Concurred", StringConverter.ToPastTense("Concur"));
            ClassicAssert.AreEqual("Ordered", StringConverter.ToPastTense("Order"));
            ClassicAssert.AreEqual("Sent", StringConverter.ToPastTense("Send"));
            ClassicAssert.AreEqual("sent", StringConverter.ToPastTense("send"));
            ClassicAssert.AreEqual("Frolicked", StringConverter.ToPastTense("Frolic"));
            ClassicAssert.AreEqual("Picnicked", StringConverter.ToPastTense("Picnic"));
        }

        [Test]
        public void ToPlural()
        {
            ClassicAssert.AreEqual("Castles", StringConverter.ToPlural("Castle"));
            ClassicAssert.AreEqual("Successes", StringConverter.ToPlural("Success"));
        }

        [Test]
        public void ToSingle()
        {
            ClassicAssert.AreEqual("Castle", StringConverter.ToSingle("Castles"));
            ClassicAssert.AreEqual("Success", StringConverter.ToSingle("Successes"));
        }

        [Test]
        public void ToComments()
        {
            ClassicAssert.AreEqual("See <see cref=\"Xyz\"/>.", StringConverter.ToComments("See {{Xyz}}."));
            ClassicAssert.AreEqual("See <see cref=\"List{Xyz}\"/>.", StringConverter.ToComments("See {{List<Xyz>}}."));
        }

        [Test]
        public void ToSeeComments()
        {
            ClassicAssert.AreEqual("<see cref=\"Xyz\"/>", StringConverter.ToSeeComments("Xyz"));
            ClassicAssert.AreEqual("<see cref=\"List{Xyz}\"/>", StringConverter.ToSeeComments("List<Xyz>"));
        }
    }
}