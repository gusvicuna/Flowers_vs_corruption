using UnityEngine;

namespace FlowersVsCorruption.Farming
{
    public class Crop
    {
        private readonly CropDefinition _definition;
        private int _stage;
        private int _health;

        private bool _wateredToday;

        public Crop(CropDefinition definition)
        {
            if (definition == null) throw new System.ArgumentNullException(nameof(definition));
            _definition = definition;
            _stage = 0;
            _health = definition.GrownHealth;
        }

        public CropDefinition Definition => _definition;

        public int Stage => _stage;

        public int Health => _health;

        public bool IsGrown => _stage >= _definition.GrowthStages - 1;

        public bool IsHarvestable => IsGrown && _definition.IsHarvestable;

        public bool WateredToday => _wateredToday;

        internal void AdvanceGrowth()
        {
            if (_stage < _definition.GrowthStages - 1)
                _stage++;
        }

        internal void Damage(int amount)
        {
            if (amount <= 0) return;
            _health -= amount;
            if (_health < 0) _health = 0;
        }

        internal void Water()
        {
            _wateredToday = true;
        }

        internal void ResetWatered()
        {
            _wateredToday = false;
        }
    }
}
