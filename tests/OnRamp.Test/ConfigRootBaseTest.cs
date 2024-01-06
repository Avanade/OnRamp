using NUnit.Framework;
using NUnit.Framework.Legacy;
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

            ClassicAssert.AreEqual("123", ec.GetRuntimeParameter("XXX", "456"));
            ClassicAssert.AreEqual(123, ec.GetRuntimeParameter("XXX", 456));
            ClassicAssert.AreEqual("456", ec.GetRuntimeParameter("YYY", "456"));
            ClassicAssert.AreEqual(456, ec.GetRuntimeParameter("YYY", 456));

            ClassicAssert.IsTrue(ec.TryGetRuntimeParameter("XXX", out string sv));
            ClassicAssert.AreEqual("123", sv);

            ClassicAssert.IsFalse(ec.TryGetRuntimeParameter("YYY", out sv));
            ClassicAssert.IsNull(sv);

            ClassicAssert.IsTrue(ec.TryGetRuntimeParameter("XXX", out int iv));
            ClassicAssert.AreEqual(123, iv);

            ClassicAssert.IsFalse(ec.TryGetRuntimeParameter("YYY", out iv));
            ClassicAssert.AreEqual(0, iv);

            ClassicAssert.IsFalse(ec.TryGetRuntimeParameter("YYY", out int? nv));
            ClassicAssert.IsNull(nv);

            ec.ResetRuntimeParameters();
            ClassicAssert.AreEqual(0, ec.RuntimeParameters.Count);
            ClassicAssert.AreEqual("456", ec.GetRuntimeParameter("XXX", "456"));
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
            ClassicAssert.AreEqual(3, ec.RuntimeParameters.Count);
            ClassicAssert.AreEqual("456", ec.GetRuntimeParameter<string>("XXX"));
            ClassicAssert.AreEqual("789", ec.GetRuntimeParameter<string>("YYY"));
            ClassicAssert.AreEqual("ABC", ec.GetRuntimeParameter<string>("ZZZ"));
        }

        [Test]
        public void DateTimeNow()
        {
            var now = DateTime.Now;

            var ec = new EntityConfig();
            var dtn = ec.DateTimeNow;
            ClassicAssert.IsTrue(dtn > now);
            ClassicAssert.AreEqual(DateTimeKind.Local, dtn.Kind);
        }

        [Test]
        public void DateTimeUtcNow()
        {
            var now = DateTime.UtcNow;

            var ec = new EntityConfig();
            var dtn = ec.DateTimeUtcNow;
            ClassicAssert.IsTrue(dtn > now);
            ClassicAssert.AreEqual(DateTimeKind.Utc, dtn.Kind);
        }

        [Test]
        public void NewGuid()
        {
            var ec = new EntityConfig();
            ClassicAssert.AreNotEqual(ec.NewGuid, ec.NewGuid);
        }

        [Test]
        public void SelectGenResult()
        {
            var ec = new EntityConfig();
            var r = ec.SelectGenResult;
            ClassicAssert.IsNotNull(r);
            ClassicAssert.AreEqual(1, r.Count());
            ClassicAssert.AreSame(ec, r.First());
        }
    }
}