using System;

namespace FlowersVsCorruption.Player
{
    public class PlayerVitals
    {
        public Vital Hunger { get; }
        public Vital Health { get; }

        private readonly float _hungerDrainPerSecond;
        private readonly float _corruptionDamagePerSecond;
        private readonly float _starvingDamagePerSecond;

        public event Action PlayerDied;

        public bool IsDead => Health.IsEmpty;

        public PlayerVitals(VitalsSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            Hunger = new Vital(settings.MaxHunger, settings.StartingHunger);
            Health = new Vital(settings.MaxHealth, settings.StartingHealth);

            _hungerDrainPerSecond = settings.HungerDrainPerSecond;
            _corruptionDamagePerSecond = settings.CorruptionDamagePerSecond;
            _starvingDamagePerSecond = settings.StarvingDamagePerSecond;

            Health.Emptied += () => PlayerDied?.Invoke();
        }

        public void Tick(float deltaTime, bool standingOnCorruption)
        {
            if (deltaTime <= 0f || IsDead)
                return;

            // Drain hunger over time
            Hunger.Subtract(_hungerDrainPerSecond * deltaTime);

            // Apply damage from corruption and/or starvation
            float damage = 0f;
            if (standingOnCorruption)
                damage += _corruptionDamagePerSecond * deltaTime;
            if (Hunger.IsEmpty)
                damage += _starvingDamagePerSecond * deltaTime;

            Health.Subtract(damage);
        }

        public bool Consume(float hungerRestored, float healthRestored, float healthDamage)
        {
            if (IsDead)
                return false;

            bool hungerAdded = Hunger.Add(hungerRestored);
            bool healthAdded = Health.Add(healthRestored);
            bool healthDamaged = Health.Subtract(healthDamage);
            return hungerAdded || healthAdded || healthDamaged;
        }
    }
}
