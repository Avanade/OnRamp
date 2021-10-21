using NUnit.Framework;
using OnRamp.Utility;

namespace OnRamp.Test
{
    [TestFixture]
    public class StringConversionTest
    {
        [Test]
        public void ToCamelCase()
        {
            Assert.AreEqual("fieldName", StringConversion.ToCamelCase("FieldName"));
            Assert.AreEqual("etagName", StringConversion.ToCamelCase("ETagName"));
            Assert.AreEqual("odataName", StringConversion.ToCamelCase("ODataName"));

            Assert.AreEqual("fieldName", StringConversion.ToCamelCase("FieldName", true));
            Assert.AreEqual("eTagName", StringConversion.ToCamelCase("ETagName", true));
            Assert.AreEqual("oDataName", StringConversion.ToCamelCase("ODataName", true));
        }

        [Test]
        public void ToPascalCase()
        {
            Assert.AreEqual("FieldName", StringConversion.ToPascalCase("fieldName"));
            Assert.AreEqual("ETagName", StringConversion.ToPascalCase("etagName"));
            Assert.AreEqual("ODataName", StringConversion.ToPascalCase("odataName"));

            Assert.AreEqual("FieldName", StringConversion.ToPascalCase("fieldName", true));
            Assert.AreEqual("EtagName", StringConversion.ToPascalCase("etagName", true));
            Assert.AreEqual("OdataName", StringConversion.ToPascalCase("odataName", true));
        }

        [Test]
        public void ToPrivateCase()
        {
            Assert.AreEqual("_fieldName", StringConversion.ToPrivateCase("FieldName"));
            Assert.AreEqual("_etagName", StringConversion.ToPrivateCase("ETagName"));
            Assert.AreEqual("_odataName", StringConversion.ToPrivateCase("ODataName"));

            Assert.AreEqual("_fieldName", StringConversion.ToPrivateCase("FieldName", true));
            Assert.AreEqual("_eTagName", StringConversion.ToPrivateCase("ETagName", true));
            Assert.AreEqual("_oDataName", StringConversion.ToPrivateCase("ODataName", true));
        }

        [Test]
        public void ToSentenceCase()
        {
            Assert.AreEqual("Field Name", StringConversion.ToSentenceCase("FieldName"));
            Assert.AreEqual("ETag Name", StringConversion.ToSentenceCase("ETagName"));
            Assert.AreEqual("OData Name", StringConversion.ToSentenceCase("ODataName"));
            Assert.AreEqual("XML Name", StringConversion.ToSentenceCase("XMLName"));

            Assert.AreEqual("Field Name", StringConversion.ToSentenceCase("FieldName", true));
            Assert.AreEqual("E Tag Name", StringConversion.ToSentenceCase("ETagName", true));
            Assert.AreEqual("O Data Name", StringConversion.ToSentenceCase("ODataName", true));
            Assert.AreEqual("XML Name OR Other", StringConversion.ToSentenceCase("XMLNameOROther", true));
        }

        [Test]
        public void ToKebabCase()
        {
            Assert.AreEqual("field-name", StringConversion.ToKebabCase("FieldName"));
            Assert.AreEqual("etag-name", StringConversion.ToKebabCase("ETagName"));
            Assert.AreEqual("odata-name", StringConversion.ToKebabCase("ODataName"));
            Assert.AreEqual("xml-name", StringConversion.ToKebabCase("XMLName"));

            Assert.AreEqual("field-name", StringConversion.ToKebabCase("FieldName", true));
            Assert.AreEqual("e-tag-name", StringConversion.ToKebabCase("ETagName", true));
            Assert.AreEqual("o-data-name", StringConversion.ToKebabCase("ODataName", true));
            Assert.AreEqual("xml-name", StringConversion.ToKebabCase("XMLName", true));
        }

        [Test]
        public void ToSnakeCase()
        {
            Assert.AreEqual("field_name", StringConversion.ToSnakeCase("FieldName"));
            Assert.AreEqual("etag_name", StringConversion.ToSnakeCase("ETagName"));
            Assert.AreEqual("odata_name", StringConversion.ToSnakeCase("ODataName"));
            Assert.AreEqual("xml_name", StringConversion.ToSnakeCase("XMLName"));

            Assert.AreEqual("field_name", StringConversion.ToSnakeCase("FieldName", true));
            Assert.AreEqual("e_tag_name", StringConversion.ToSnakeCase("ETagName", true));
            Assert.AreEqual("o_data_name", StringConversion.ToSnakeCase("ODataName", true));
            Assert.AreEqual("xml_name", StringConversion.ToSnakeCase("XMLName", true));
        }
    }
}