using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OnRamp.Config;
using OnRamp.Test.Config;
using System.Collections.Generic;

namespace OnRamp.Test
{
    [TestFixture]
    public class ConfigBaseTest
    {
        [Test]
        public void IsTrue()
        {
            bool val1 = true;
            bool? val2 = true;

            Assert.IsTrue(ConfigBase.IsTrue(val1));
            Assert.IsTrue(ConfigBase.IsTrue(val2));

            val1 = false;
            val2 = false;
            Assert.IsFalse(ConfigBase.IsTrue(val1));
            Assert.IsFalse(ConfigBase.IsTrue(val2));

            val2 = null;
            Assert.IsFalse(ConfigBase.IsTrue(val2));
        }

        [Test]
        public void IsFalse()
        {
            bool val1 = true;
            bool? val2 = true;

            Assert.IsFalse(ConfigBase.IsFalse(val1));
            Assert.IsFalse(ConfigBase.IsFalse(val2));

            val1 = false;
            val2 = false;
            Assert.IsTrue(ConfigBase.IsFalse(val1));
            Assert.IsTrue(ConfigBase.IsFalse(val2));

            val2 = null;
            Assert.IsTrue(ConfigBase.IsFalse(val2));
        }

        [Test]
        public void DefaultWhereNull_String()
        {
            Assert.AreEqual("ABC", ConfigBase.DefaultWhereNull("ABC", () => "DEF"));
            Assert.AreEqual("", ConfigBase.DefaultWhereNull("", () => "DEF"));
            Assert.AreEqual("DEF", ConfigBase.DefaultWhereNull(null, () => "DEF"));
        }

        [Test]
        public void DefaultWhereNull_Bool()
        {
            Assert.IsTrue(ConfigBase.DefaultWhereNull(true, () => false));
            Assert.IsFalse(ConfigBase.DefaultWhereNull(false, () => true));
            Assert.IsTrue(ConfigBase.DefaultWhereNull(null, () => true));
        }

        [Test]
        public void CompareValue_String()
        {
            Assert.IsTrue(ConfigBase.CompareValue("ABC", "ABC"));
            Assert.IsFalse(ConfigBase.CompareValue("ABC", "DEF"));
            Assert.IsFalse(ConfigBase.CompareValue(null, "DEF"));
        }

        [Test]
        public void CompareValue_Bool()
        {
            Assert.IsTrue(ConfigBase.CompareValue(true, true));
            Assert.IsFalse(ConfigBase.CompareValue(true, false));
            Assert.IsFalse(ConfigBase.CompareValue(null, true));
        }

        [Test]
        public void CompareNullOrValue_String()
        {
            Assert.IsTrue(ConfigBase.CompareNullOrValue("ABC", "ABC"));
            Assert.IsFalse(ConfigBase.CompareNullOrValue("ABC", "DEF"));
            Assert.IsTrue(ConfigBase.CompareNullOrValue(null, "DEF"));
        }

        [Test]
        public void CompareNullOrValue_Bool()
        {
            Assert.IsTrue(ConfigBase.CompareNullOrValue(true, true));
            Assert.IsFalse(ConfigBase.CompareNullOrValue(true, false));
            Assert.IsTrue(ConfigBase.CompareNullOrValue(null, true));
        }

        [Test]
        public void ExtraProperties()
        {
            var ec = new EntityConfig { ExtraProperties = new Dictionary<string, JToken> { { "XXX", new JValue("AAA") } } };

            JValue jv = ec.GetExtraProperty<JValue>("XXX");
            Assert.AreEqual("AAA", jv.ToObject(typeof(string)));

            jv = ec.GetExtraProperty("YYY", new JValue("BBB"));
            Assert.AreEqual("BBB", jv.ToObject(typeof(string)));

            Assert.IsNull(ec.GetExtraProperty<JValue>("YYY"));

            Assert.IsTrue(ec.TryGetExtraProperty("XXX", out jv));
            Assert.AreEqual("AAA", jv.ToObject(typeof(string)));

            Assert.IsFalse(ec.TryGetExtraProperty("YYY", out jv));
            Assert.IsNull(jv);
        }

        [Test]
        public void CustomProperties()
        {
            var ec = new EntityConfig();
            ec.CustomProperties.Add("XXX", "AAA");

            string sv = ec.GetCustomProperty<string>("XXX");
            Assert.AreEqual("AAA", sv);

            sv = ec.GetCustomProperty("YYY", "BBB");
            Assert.AreEqual("BBB", sv);

            Assert.IsNull(ec.GetCustomProperty<string>("YYY"));

            Assert.IsTrue(ec.TryGetCustomProperty("XXX", out sv));
            Assert.AreEqual("AAA", sv);

            Assert.IsFalse(ec.TryGetCustomProperty("YYY", out sv));
            Assert.IsNull(sv);
        }
    }
}