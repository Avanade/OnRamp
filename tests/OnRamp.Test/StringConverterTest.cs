using NUnit.Framework;
using OnRamp.Utility;

namespace OnRamp.Test
{
    [TestFixture]
    public class StringConverterTest
    {
        [Test]
        public void ToCamelCase()
        {
            Assert.AreEqual("fieldName", StringConverter.ToCamelCase("FieldName"));
            Assert.AreEqual("etagName", StringConverter.ToCamelCase("ETagName"));
            Assert.AreEqual("odataName", StringConverter.ToCamelCase("ODataName"));

            Assert.AreEqual("fieldName", StringConverter.ToCamelCase("FieldName", true));
            Assert.AreEqual("eTagName", StringConverter.ToCamelCase("ETagName", true));
            Assert.AreEqual("oDataName", StringConverter.ToCamelCase("ODataName", true));
        }

        [Test]
        public void ToPascalCase()
        {
            Assert.AreEqual("FieldName", StringConverter.ToPascalCase("fieldName"));
            Assert.AreEqual("ETagName", StringConverter.ToPascalCase("etagName"));
            Assert.AreEqual("ODataName", StringConverter.ToPascalCase("odataName"));

            Assert.AreEqual("FieldName", StringConverter.ToPascalCase("fieldName", true));
            Assert.AreEqual("EtagName", StringConverter.ToPascalCase("etagName", true));
            Assert.AreEqual("OdataName", StringConverter.ToPascalCase("odataName", true));
        }

        [Test]
        public void ToPrivateCase()
        {
            Assert.AreEqual("_fieldName", StringConverter.ToPrivateCase("FieldName"));
            Assert.AreEqual("_etagName", StringConverter.ToPrivateCase("ETagName"));
            Assert.AreEqual("_odataName", StringConverter.ToPrivateCase("ODataName"));

            Assert.AreEqual("_fieldName", StringConverter.ToPrivateCase("FieldName", true));
            Assert.AreEqual("_eTagName", StringConverter.ToPrivateCase("ETagName", true));
            Assert.AreEqual("_oDataName", StringConverter.ToPrivateCase("ODataName", true));
        }

        [Test]
        public void ToSentenceCase()
        {
            Assert.AreEqual("Field Name", StringConverter.ToSentenceCase("FieldName"));
            Assert.AreEqual("ETag Name", StringConverter.ToSentenceCase("ETagName"));
            Assert.AreEqual("OData Name", StringConverter.ToSentenceCase("ODataName"));
            Assert.AreEqual("XML Name", StringConverter.ToSentenceCase("XMLName"));

            Assert.AreEqual("Field Name", StringConverter.ToSentenceCase("FieldName", true));
            Assert.AreEqual("E Tag Name", StringConverter.ToSentenceCase("ETagName", true));
            Assert.AreEqual("O Data Name", StringConverter.ToSentenceCase("ODataName", true));
            Assert.AreEqual("XML Name OR Other", StringConverter.ToSentenceCase("XMLNameOROther", true));
        }

        [Test]
        public void ToKebabCase()
        {
            Assert.AreEqual("field-name", StringConverter.ToKebabCase("FieldName"));
            Assert.AreEqual("etag-name", StringConverter.ToKebabCase("ETagName"));
            Assert.AreEqual("odata-name", StringConverter.ToKebabCase("ODataName"));
            Assert.AreEqual("xml-name", StringConverter.ToKebabCase("XMLName"));

            Assert.AreEqual("field-name", StringConverter.ToKebabCase("FieldName", true));
            Assert.AreEqual("e-tag-name", StringConverter.ToKebabCase("ETagName", true));
            Assert.AreEqual("o-data-name", StringConverter.ToKebabCase("ODataName", true));
            Assert.AreEqual("xml-name", StringConverter.ToKebabCase("XMLName", true));
        }

        [Test]
        public void ToSnakeCase()
        {
            Assert.AreEqual("field_name", StringConverter.ToSnakeCase("FieldName"));
            Assert.AreEqual("etag_name", StringConverter.ToSnakeCase("ETagName"));
            Assert.AreEqual("odata_name", StringConverter.ToSnakeCase("ODataName"));
            Assert.AreEqual("xml_name", StringConverter.ToSnakeCase("XMLName"));

            Assert.AreEqual("field_name", StringConverter.ToSnakeCase("FieldName", true));
            Assert.AreEqual("e_tag_name", StringConverter.ToSnakeCase("ETagName", true));
            Assert.AreEqual("o_data_name", StringConverter.ToSnakeCase("ODataName", true));
            Assert.AreEqual("xml_name", StringConverter.ToSnakeCase("XMLName", true));
        }

        [Test]
        public void ToPastTense()
        {
            Assert.AreEqual("A", StringConverter.ToPastTense("A"));
            Assert.AreEqual("Be", StringConverter.ToPastTense("Be"));
            Assert.AreEqual("Pried", StringConverter.ToPastTense("Pry"));
            Assert.AreEqual("Applied", StringConverter.ToPastTense("Apply"));
            Assert.AreEqual("Dropped", StringConverter.ToPastTense("Drop"));
            Assert.AreEqual("Rotted", StringConverter.ToPastTense("Rot"));
            Assert.AreEqual("Concurred", StringConverter.ToPastTense("Concur"));
            Assert.AreEqual("Ordered", StringConverter.ToPastTense("Order"));
            Assert.AreEqual("Sent", StringConverter.ToPastTense("Send"));
            Assert.AreEqual("sent", StringConverter.ToPastTense("send"));
            Assert.AreEqual("Frolicked", StringConverter.ToPastTense("Frolic"));
            Assert.AreEqual("Picnicked", StringConverter.ToPastTense("Picnic"));
        }

        [Test]
        public void ToPlural()
        {
            Assert.AreEqual("Castles", StringConverter.ToPlural("Castle"));
            Assert.AreEqual("Successes", StringConverter.ToPlural("Success"));
        }

        [Test]
        public void ToSingle()
        {
            Assert.AreEqual("Castle", StringConverter.ToSingle("Castles"));
            Assert.AreEqual("Success", StringConverter.ToSingle("Successes"));
        }

        [Test]
        public void ToComments()
        {
            Assert.AreEqual("See <see cref=\"Xyz\"/>.", StringConverter.ToComments("See {{Xyz}}."));
            Assert.AreEqual("See <see cref=\"List{Xyz}\"/>.", StringConverter.ToComments("See {{List<Xyz>}}."));
        }

        [Test]
        public void ToSeeComments()
        {
            Assert.AreEqual("<see cref=\"Xyz\"/>", StringConverter.ToSeeComments("Xyz"));
            Assert.AreEqual("<see cref=\"List{Xyz}\"/>", StringConverter.ToSeeComments("List<Xyz>"));
        }
    }
}