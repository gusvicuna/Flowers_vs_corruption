using System;
using System.Collections.Generic;
using FlowersVsCorruption.Player;
using NUnit.Framework;

namespace FlowersVsCorruption.Tests
{
    /// <summary>
    /// Spec for Vital (Feature 6): a 0..Max stat shared by hunger and health.
    /// Changed only fires on real movement; Emptied fires once per emptying.
    /// </summary>
    public class VitalTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Constructor_SetsMaxAndCurrent()
        {
            var vital = new Vital(100f, 60f);

            Assert.That(vital.Max, Is.EqualTo(100f));
            Assert.That(vital.Current, Is.EqualTo(60f));
            Assert.That(vital.Fraction01, Is.EqualTo(0.6f).Within(Tolerance));
            Assert.That(vital.IsEmpty, Is.False);
        }

        [Test]
        public void Constructor_ClampsInitialIntoRange()
        {
            Assert.That(new Vital(100f, 250f).Current, Is.EqualTo(100f));
            Assert.That(new Vital(100f, -20f).Current, Is.EqualTo(0f));
        }

        [Test]
        public void Constructor_NonPositiveMax_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Vital(0f, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Vital(-5f, 0f));
        }

        [Test]
        public void Subtract_ReducesAndRaisesChanged()
        {
            var vital = new Vital(100f, 100f);
            var received = new List<float>();
            vital.Changed += received.Add;

            vital.Subtract(30f);

            Assert.That(vital.Current, Is.EqualTo(70f).Within(Tolerance));
            Assert.That(received, Is.EqualTo(new[] { 70f }));
        }

        [Test]
        public void Subtract_ClampsAtZero()
        {
            var vital = new Vital(100f, 10f);

            vital.Subtract(999f);

            Assert.That(vital.Current, Is.EqualTo(0f));
            Assert.That(vital.IsEmpty, Is.True);
            Assert.That(vital.Fraction01, Is.EqualTo(0f));
        }

        [Test]
        public void Add_ClampsAtMax()
        {
            var vital = new Vital(100f, 90f);

            vital.Add(50f);

            Assert.That(vital.Current, Is.EqualTo(100f));
        }

        [Test]
        public void Changed_SilentWhenValueDoesNotMove()
        {
            var full = new Vital(100f, 100f);
            var empty = new Vital(100f, 0f);
            int fullEvents = 0, emptyEvents = 0;
            full.Changed += _ => fullEvents++;
            empty.Changed += _ => emptyEvents++;

            full.Add(10f);        // already at max
            empty.Subtract(10f);  // already at zero

            Assert.That(fullEvents, Is.EqualTo(0));
            Assert.That(emptyEvents, Is.EqualTo(0));
        }

        [Test]
        public void ZeroOrNegativeAmounts_AreNoOps()
        {
            var vital = new Vital(100f, 50f);
            int events = 0;
            vital.Changed += _ => events++;

            vital.Add(0f);
            vital.Add(-25f);       // never adds by subtracting
            vital.Subtract(0f);
            vital.Subtract(-25f);  // never heals by damaging

            Assert.That(vital.Current, Is.EqualTo(50f));
            Assert.That(events, Is.EqualTo(0));
        }

        [Test]
        public void Emptied_FiresOnceWhenReachingZero()
        {
            var vital = new Vital(100f, 20f);
            int emptied = 0;
            vital.Emptied += () => emptied++;

            vital.Subtract(20f);
            vital.Subtract(5f);   // already empty: must not fire again

            Assert.That(emptied, Is.EqualTo(1));
        }

        [Test]
        public void Emptied_RearmsAfterRefill()
        {
            var vital = new Vital(100f, 10f);
            int emptied = 0;
            vital.Emptied += () => emptied++;

            vital.Subtract(10f);   // empty (1)
            vital.Add(30f);        // refilled
            vital.Subtract(30f);   // empty again (2)

            Assert.That(emptied, Is.EqualTo(2));
        }
    }
}
