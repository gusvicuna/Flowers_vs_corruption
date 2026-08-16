using System;
using FlowersVsCorruption.Player;
using NUnit.Framework;

namespace FlowersVsCorruption.Tests
{
    /// <summary>
    /// Spec for PlayerVitals (Feature 6). Hunger drains all cycle long; health
    /// drains while standing on corruption and while starving, and the two
    /// stack. Eating restores both and can also damage (corrupted crops).
    /// </summary>
    public class PlayerVitalsTests
    {
        private const float Tolerance = 0.0001f;

        private const float MaxHunger = 100f;
        private const float MaxHealth = 100f;
        private const float HungerDrain = 2f;      // per second
        private const float CorruptionDamage = 10f;
        private const float StarvingDamage = 4f;

        private static VitalsSettings MakeSettings(
            float startingHunger = MaxHunger, float startingHealth = MaxHealth)
        {
            return new VitalsSettings
            {
                MaxHunger = MaxHunger,
                MaxHealth = MaxHealth,
                HungerDrainPerSecond = HungerDrain,
                CorruptionDamagePerSecond = CorruptionDamage,
                StarvingDamagePerSecond = StarvingDamage,
                StartingHunger = startingHunger,
                StartingHealth = startingHealth
            };
        }

        private static PlayerVitals MakeVitals(
            float startingHunger = MaxHunger, float startingHealth = MaxHealth)
        {
            return new PlayerVitals(MakeSettings(startingHunger, startingHealth));
        }

        [Test]
        public void Constructor_NullSettings_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new PlayerVitals(null));
        }

        [Test]
        public void Constructor_StartsAtConfiguredValues()
        {
            PlayerVitals vitals = MakeVitals();

            Assert.That(vitals.Hunger.Current, Is.EqualTo(MaxHunger));
            Assert.That(vitals.Health.Current, Is.EqualTo(MaxHealth));
        }

        [Test]
        public void Tick_ZeroOrNegativeDelta_DoesNothing()
        {
            PlayerVitals vitals = MakeVitals();

            vitals.Tick(0f, standingOnCorruption: true);
            vitals.Tick(-1f, standingOnCorruption: true);

            Assert.That(vitals.Hunger.Current, Is.EqualTo(MaxHunger));
            Assert.That(vitals.Health.Current, Is.EqualTo(MaxHealth));
        }

        [Test]
        public void Tick_HungerDrainsOnCleanGround()
        {
            PlayerVitals vitals = MakeVitals();

            vitals.Tick(2f, standingOnCorruption: false);

            Assert.That(vitals.Hunger.Current, Is.EqualTo(MaxHunger - HungerDrain * 2f).Within(Tolerance));
            Assert.That(vitals.Health.Current, Is.EqualTo(MaxHealth), "clean ground never hurts");
        }

        [Test]
        public void Tick_StandingOnCorruption_DrainsHealth()
        {
            PlayerVitals vitals = MakeVitals();

            vitals.Tick(3f, standingOnCorruption: true);

            Assert.That(vitals.Health.Current, Is.EqualTo(MaxHealth - CorruptionDamage * 3f).Within(Tolerance));
        }

        [Test]
        public void Tick_HungerEmpty_DrainsHealthSlowly()
        {
            PlayerVitals vitals = MakeVitals(startingHunger: 0f);

            vitals.Tick(5f, standingOnCorruption: false);

            Assert.That(vitals.Hunger.Current, Is.EqualTo(0f));
            Assert.That(vitals.Health.Current, Is.EqualTo(MaxHealth - StarvingDamage * 5f).Within(Tolerance));
        }

        [Test]
        public void Tick_StarvingOnCorruption_DamageStacks()
        {
            PlayerVitals vitals = MakeVitals(startingHunger: 0f);

            vitals.Tick(2f, standingOnCorruption: true);

            float expected = MaxHealth - (CorruptionDamage + StarvingDamage) * 2f;
            Assert.That(vitals.Health.Current, Is.EqualTo(expected).Within(Tolerance));
        }

        [Test]
        public void Tick_HealthReachesZero_RaisesPlayerDiedOnce()
        {
            PlayerVitals vitals = MakeVitals(startingHealth: 15f);
            int deaths = 0;
            vitals.PlayerDied += () => deaths++;

            vitals.Tick(2f, standingOnCorruption: true);   // 20 damage: lethal
            vitals.Tick(2f, standingOnCorruption: true);   // already dead
            vitals.Tick(2f, standingOnCorruption: true);

            Assert.That(vitals.Health.Current, Is.EqualTo(0f));
            Assert.That(deaths, Is.EqualTo(1));
        }

        [Test]
        public void Tick_AfterDeath_IsFrozen()
        {
            PlayerVitals vitals = MakeVitals(startingHunger: 50f, startingHealth: 5f);
            vitals.Tick(1f, standingOnCorruption: true);   // dies here
            float hungerAtDeath = vitals.Hunger.Current;

            vitals.Tick(10f, standingOnCorruption: false);

            Assert.That(vitals.Hunger.Current, Is.EqualTo(hungerAtDeath),
                "the world stops for a dead player — hunger must not keep draining");
        }

        [Test]
        public void Consume_RestoresHungerAndHealth()
        {
            PlayerVitals vitals = MakeVitals(startingHunger: 40f, startingHealth: 50f);

            bool result = vitals.Consume(hungerRestored: 25f, healthRestored: 5f, healthDamage: 0f);

            Assert.That(result, Is.True);
            Assert.That(vitals.Hunger.Current, Is.EqualTo(65f).Within(Tolerance));
            Assert.That(vitals.Health.Current, Is.EqualTo(55f).Within(Tolerance));
        }

        [Test]
        public void Consume_ClampsAtMax()
        {
            PlayerVitals vitals = MakeVitals(startingHunger: 95f, startingHealth: 98f);

            vitals.Consume(50f, 50f, 0f);

            Assert.That(vitals.Hunger.Current, Is.EqualTo(MaxHunger));
            Assert.That(vitals.Health.Current, Is.EqualTo(MaxHealth));
        }

        [Test]
        public void Consume_CorruptedCrop_FeedsButDamages()
        {
            PlayerVitals vitals = MakeVitals(startingHunger: 30f, startingHealth: 80f);

            vitals.Consume(hungerRestored: 45f, healthRestored: 0f, healthDamage: 20f);

            Assert.That(vitals.Hunger.Current, Is.EqualTo(75f).Within(Tolerance));
            Assert.That(vitals.Health.Current, Is.EqualTo(60f).Within(Tolerance));
        }

        [Test]
        public void Consume_LethalDamage_RaisesPlayerDied()
        {
            PlayerVitals vitals = MakeVitals(startingHunger: 10f, startingHealth: 15f);
            int deaths = 0;
            vitals.PlayerDied += () => deaths++;

            vitals.Consume(hungerRestored: 45f, healthRestored: 0f, healthDamage: 20f);

            Assert.That(vitals.Health.Current, Is.EqualTo(0f));
            Assert.That(deaths, Is.EqualTo(1), "eating something rotten can kill you");
        }

        [Test]
        public void Consume_AfterDeath_DoesNotResurrect()
        {
            PlayerVitals vitals = MakeVitals(startingHunger: 10f, startingHealth: 5f);
            vitals.Tick(1f, standingOnCorruption: true);   // dies here
            Assert.That(vitals.Health.IsEmpty, Is.True);

            bool result = vitals.Consume(50f, 50f, 0f);

            Assert.That(result, Is.False);
            Assert.That(vitals.Health.Current, Is.EqualTo(0f), "dead is dead — food cannot revive");
        }

        [Test]
        public void Consume_AllZeroAmounts_ReturnsFalse()
        {
            PlayerVitals vitals = MakeVitals(startingHunger: 50f, startingHealth: 50f);

            bool result = vitals.Consume(0f, 0f, 0f);

            Assert.That(result, Is.False);
            Assert.That(vitals.Hunger.Current, Is.EqualTo(50f));
            Assert.That(vitals.Health.Current, Is.EqualTo(50f));
        }
    }
}
