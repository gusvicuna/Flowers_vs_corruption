using FlowersVsCorruption.World;
using NUnit.Framework;
using UnityEngine;

namespace FlowersVsCorruption.Tests
{
    public class RingGeometryTests
    {
        private const float Tolerance = 0.0001f;

        private static void AssertVector(Vector3 actual, Vector3 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(Tolerance));
        }

        [Test]
        public void PositionAt_Top_IsUp()
        {
            AssertVector(RingGeometry.PositionAt(90f, 3f), new Vector3(0f, 3f, 0f));
        }

        [Test]
        public void PositionAt_QuarterTurns()
        {
            AssertVector(RingGeometry.PositionAt(0f, 2f), new Vector3(2f, 0f, 0f));
            AssertVector(RingGeometry.PositionAt(180f, 2f), new Vector3(-2f, 0f, 0f));
            AssertVector(RingGeometry.PositionAt(270f, 2f), new Vector3(0f, -2f, 0f));
        }

        [Test]
        public void RotationAt_MakesUpRadial()
        {
            foreach (float angle in new[] { 90f, 0f, 180f, 270f, 37.5f })
            {
                Vector3 up = RingGeometry.RotationAt(angle) * Vector3.up;
                AssertVector(up, RingGeometry.PositionAt(angle, 1f));
            }
        }

        [Test]
        public void RotationAt_Top_IsIdentity()
        {
            Quaternion rotation = RingGeometry.RotationAt(90f);
            Assert.That(Quaternion.Angle(rotation, Quaternion.identity), Is.LessThan(0.001f));
        }
    }
}
