using NUnit.Framework;
using OnRamp.Test.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnRamp.Test
{
    [TestFixture]
    public class ConfigRootBaseTest
    {
        [Test]
        public void RuntimeParameters()
        {
            var ec = new EntityConfig();
            ec.RuntimeParameters.Add("XXX", "123");

            Assert.AreEqual("123", ec.GetRuntimeParameter("XXX", "456"));
            Assert.AreEqual(123, ec.GetRuntimeParameter("XXX", 456));
            Assert.AreEqual("456", ec.GetRuntimeParameter("YYY", "456"));
            Assert.AreEqual(456, ec.GetRuntimeParameter("YYY", 456));

            Assert.IsTrue(ec.TryGetRuntimeParameter("XXX", out string sv));
            Assert.AreEqual("123", sv);

            Assert.IsFalse(ec.TryGetRuntimeParameter("YYY", out sv));
            Assert.IsNull(sv);

            Assert.IsTrue(ec.TryGetRuntimeParameter("XXX", out int iv));
            Assert.AreEqual(123, iv);

            Assert.IsFalse(ec.TryGetRuntimeParameter("YYY", out iv));
            Assert.AreEqual(0, iv);

            Assert.IsFalse(ec.TryGetRuntimeParameter("YYY", out int? nv));
            Assert.IsNull(nv);

            ec.ResetRuntimeParameters();
            Assert.AreEqual(0, ec.RuntimeParameters.Count);
            Assert.AreEqual("456", ec.GetRuntimeParameter("XXX", "456"));
        }

        [Test]
        public void MergeRuntimeParameters()
        {
            var rp = new Dictionary<string, object>
            {
                { "XXX", "456" },
                { "YYY", "789" }
            };

            var ec = new EntityConfig();
            ec.RuntimeParameters.Add("XXX", "123");
            ec.RuntimeParameters.Add("ZZZ", "ABC");

            ec.MergeRuntimeParameters(rp);
            Assert.AreEqual(3, ec.RuntimeParameters.Count);
            Assert.AreEqual("456", ec.GetRuntimeParameter<string>("XXX"));
            Assert.AreEqual("789", ec.GetRuntimeParameter<string>("YYY"));
            Assert.AreEqual("ABC", ec.GetRuntimeParameter<string>("ZZZ"));
        }

        [Test]
        public void DateTimeNow()
        {
            var now = DateTime.Now;

            var ec = new EntityConfig();
            var dtn = ec.DateTimeNow;
            Assert.IsTrue(dtn > now);
            Assert.AreEqual(DateTimeKind.Local, dtn.Kind);
        }

        [Test]
        public void DateTimeUtcNow()
        {
            var now = DateTime.UtcNow;

            var ec = new EntityConfig();
            var dtn = ec.DateTimeUtcNow;
            Assert.IsTrue(dtn > now);
            Assert.AreEqual(DateTimeKind.Utc, dtn.Kind);
        }

        [Test]
        public void NewGuid()
        {
            var ec = new EntityConfig();
            Assert.AreNotEqual(ec.NewGuid, ec.NewGuid);
        }

        [Test]
        public void SelectGenResult()
        {
            var ec = new EntityConfig();
            var r = ec.SelectGenResult;
            Assert.IsNotNull(r);
            Assert.AreEqual(1, r.Count());
            Assert.AreSame(ec, r.First());
        }
    }
}