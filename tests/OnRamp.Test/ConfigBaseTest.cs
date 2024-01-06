using NUnit.Framework;
using OnRamp.Config;
using OnRamp.Test.Config;
using System.Linq;
using System.Text.Json;

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

            Assert.That(ConfigBase.IsTrue(val1), Is.True);
            Assert.That(ConfigBase.IsTrue(val2), Is.True);

            val1 = false;
            val2 = false;
            Assert.That(ConfigBase.IsTrue(val1), Is.False);
            Assert.That(ConfigBase.IsTrue(val2), Is.False);

            val2 = null;
            Assert.That(ConfigBase.IsTrue(val2), Is.False);
        }

        [Test]
        public void IsFalse()
        {
            bool val1 = true;
            bool? val2 = true;

            Assert.That(ConfigBase.IsFalse(val1), Is.False);
            Assert.That(ConfigBase.IsFalse(val2), Is.False);

            val1 = false;
            val2 = false;
            Assert.That(ConfigBase.IsFalse(val1), Is.True);
            Assert.That(ConfigBase.IsFalse(val2), Is.True);

            val2 = null;
            Assert.That(ConfigBase.IsFalse(val2), Is.True);
        }

        [Test]
        public void DefaultWhereNull_String()
        {
            Assert.That(ConfigBase.DefaultWhereNull("ABC", () => "DEF"), Is.EqualTo("ABC"));
            Assert.That(ConfigBase.DefaultWhereNull("", () => "DEF"), Is.EqualTo(""));
            Assert.That(ConfigBase.DefaultWhereNull(null, () => "DEF"), Is.EqualTo("DEF"));
        }

        [Test]
        public void DefaultWhereNull_Bool()
        {
            Assert.That(ConfigBase.DefaultWhereNull(true, () => false), Is.True);
            Assert.That(ConfigBase.DefaultWhereNull(false, () => true), Is.False);
            Assert.That(ConfigBase.DefaultWhereNull(null, () => true), Is.True);
        }

        [Test]
        public void CompareValue_String()
        {
            Assert.That(ConfigBase.CompareValue("ABC", "ABC"), Is.True);
            Assert.That(ConfigBase.CompareValue("ABC", "DEF"), Is.False);
            Assert.That(ConfigBase.CompareValue(null, "DEF"), Is.False);
        }

        [Test]
        public void CompareValue_Bool()
        {
            Assert.That(ConfigBase.CompareValue(true, true), Is.True);
            Assert.That(ConfigBase.CompareValue(true, false), Is.False);
            Assert.That(ConfigBase.CompareValue(null, true), Is.False);
        }

        [Test]
        public void CompareNullOrValue_String()
        {
            Assert.That(ConfigBase.CompareNullOrValue("ABC", "ABC"), Is.True);
            Assert.That(ConfigBase.CompareNullOrValue("ABC", "DEF"), Is.False);
            Assert.That(ConfigBase.CompareNullOrValue(null, "DEF"), Is.True);
        }

        [Test]
        public void CompareNullOrValue_Bool()
        {
            Assert.That(ConfigBase.CompareNullOrValue(true, true), Is.True);
            Assert.That(ConfigBase.CompareNullOrValue(true, false), Is.False);
            Assert.That(ConfigBase.CompareNullOrValue(null, true), Is.True);
        }

        [Test]
        public void ExtraProperties()
        {
            var ec = (EntityConfig)Utility.JsonSerializer.Deserialize("{ \"name\": \"Bob\", \"XXX\": \"AAA\", \"YYY\": { \"BBB\": \"CCC\" } }", typeof(EntityConfig));

            Assert.That(ec.ExtraProperties.Count, Is.EqualTo(2));
            Assert.That(ec.GetExtraProperty<string>("XXX"), Is.EqualTo("AAA"));
            Assert.That(ec.GetExtraProperty<string>("JJJ", "CCC"), Is.EqualTo("CCC"));

            Assert.That(ec.TryGetExtraProperty<string>("XXX", out var val), Is.True);
            Assert.That(val, Is.EqualTo("AAA"));

            Assert.That(ec.TryGetExtraProperty<string>("JJJ", out val), Is.False);
            Assert.That(val, Is.Null);

            Assert.That(ec.TryGetExtraProperty<YYY>("YYY", out var bval), Is.True);
            Assert.That(bval, Is.Not.Null);
            Assert.That(bval.BBB, Is.EqualTo("CCC"));
        }

        private class YYY
        {
            public string BBB { get; set; }
        }

        [Test]
        public void CustomProperties()
        {
            var ec = new EntityConfig();
            ec.CustomProperties.Add("XXX", "AAA");

            string sv = ec.GetCustomProperty<string>("XXX");
            Assert.That(sv, Is.EqualTo("AAA"));

            sv = ec.GetCustomProperty("YYY", "BBB");
            Assert.That(sv, Is.EqualTo("BBB"));

            Assert.That(ec.GetCustomProperty<string>("YYY"), Is.Null);

            Assert.That(ec.TryGetCustomProperty("XXX", out sv), Is.True);
            Assert.That(sv, Is.EqualTo("AAA"));

            Assert.That(ec.TryGetCustomProperty("YYY", out sv), Is.False);
            Assert.That(sv, Is.Null);
        }
    }
}