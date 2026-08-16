using System;

namespace FlowersVsCorruption.Player
{
    /// <summary>
    /// A 0..Max stat shared by hunger and health. Changed only fires on real
    /// movement; Emptied fires once per emptying.
    /// </summary>
    public class Vital
    {
        public float Max { get; }
        public float Current { get; private set; }
        public float Fraction01 => Current / Max;
        public bool IsEmpty => Current <= 0f;

        public event Action<float> Changed;
        public event Action Emptied;

        public Vital(float max, float initial)
        {
            if (max <= 0f)
                throw new ArgumentOutOfRangeException(nameof(max), "Max must be positive.");

            Max = max;
            Current = Math.Clamp(initial, 0f, Max);
        }

        public bool Subtract(float amount)
        {
            if (amount < 0f)
                return false;

            float oldCurrent = Current;
            Current = Math.Max(Current - amount, 0f);

            if (Current != oldCurrent)
            {
                Changed?.Invoke(Current);
                if (Current <= 0f && oldCurrent > 0f)
                    Emptied?.Invoke();
                return true;
            }
            return false;
        }

        public bool Add(float amount)
        {
            if (amount < 0f)
                return false;

            float oldCurrent = Current;
            Current = Math.Min(Current + amount, Max);

            if (Current != oldCurrent)
            {
                Changed?.Invoke(Current);
                return true;
            }
            return false;
        }
    }
}
