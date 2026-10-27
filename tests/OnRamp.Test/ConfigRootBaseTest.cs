using NUnit.Framework;
using NUnit.Framework.Legacy;
using OnRamp.Test.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnRamp.Test
{
    /// <summary>
    /// Verifies root runtime parameters and template convenience properties.
    /// </summary>
    [TestFixture]
    public class ConfigRootBaseTest
    {
        /// <summary>
        /// Verifies parameter conversion, missing-value defaults, and reset behavior.
        /// </summary>
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

        /// <summary>
        /// Verifies merging replaces matching keys and preserves unrelated parameters.
        /// </summary>
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

        /// <summary>
        /// Verifies local time falls within the inclusive observation interval.
        /// </summary>
        [Test]
        public void DateTimeNow()
        {
            var now = DateTime.Now;

            var ec = new EntityConfig();
            var dtn = ec.DateTimeNow;
            Assert.That(dtn, Is.InRange(now, DateTime.Now));
            ClassicAssert.AreEqual(DateTimeKind.Local, dtn.Kind);
        }

        /// <summary>
        /// Verifies UTC time falls within the inclusive observation interval.
        /// </summary>
        [Test]
        public void DateTimeUtcNow()
        {
            var now = DateTime.UtcNow;

            var ec = new EntityConfig();
            var dtn = ec.DateTimeUtcNow;
            Assert.That(dtn, Is.InRange(now, DateTime.UtcNow));
            ClassicAssert.AreEqual(DateTimeKind.Utc, dtn.Kind);
        }

        /// <summary>
        /// Verifies generated identifiers are nonempty and distinct.
        /// </summary>
        [Test]
        public void NewGuid()
        {
            var ec = new EntityConfig();
            var first = ec.NewGuid;
            var second = ec.NewGuid;
            Assert.That(first, Is.Not.EqualTo(Guid.Empty));
            Assert.That(second, Is.Not.EqualTo(Guid.Empty).And.Not.EqualTo(first));
        }

        /// <summary>
        /// Verifies generation selects the root instance exactly once.
        /// </summary>
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