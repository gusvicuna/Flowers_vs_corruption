using UnityEngine;

namespace FlowersVsCorruption.World
{
    /// <summary>
    /// Angle-to-transform math shared by everything that sits on the ring
    /// (tiles, player, camera, highlight). Angles follow WorldGrid's
    /// convention: degrees, 90 = top, decreasing = clockwise.
    /// </summary>
    public static class RingGeometry
    {
        /// <summary>Center-relative position on a ring of the given radius.</summary>
        public static Vector3 PositionAt(float angleDegrees, float radius)
        {
            float radians = angleDegrees * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * radius;
        }

        /// <summary>Rotation whose local up points radially outward at this angle.</summary>
        public static Quaternion RotationAt(float angleDegrees)
        {
            return Quaternion.Euler(0f, 0f, angleDegrees - 90f);
        }
    }
}
