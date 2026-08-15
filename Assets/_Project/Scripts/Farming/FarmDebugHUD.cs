using UnityEngine;
using System.Collections.Generic;

namespace FlowersVsCorruption.Farming
{
    // TEMP: debug readout for selected seed and harvest counts.
    public class FarmDebugHUD : MonoBehaviour
    {
        [SerializeField] private FarmingController _farmingController;

        private readonly Dictionary<string, int> _harvestCounts = new();
        public IReadOnlyDictionary<string, int> HarvestCounts => _harvestCounts;

        private void Start()
        {
            if (_farmingController == null)
                Debug.LogWarning("FarmDebugHUD has no FarmingController reference.", this);
            _farmingController.FarmingSystem.CropHarvested += (_, def) =>
            {
                _harvestCounts.TryGetValue(def.DisplayName, out int n);
                _harvestCounts[def.DisplayName] = n + 1;
            };
        }

        private void OnGUI()
        {
            if (_farmingController == null) return;

            // Right-anchored so it never overlaps the time HUD at the top-left.
            const float width = 200f;
            GUILayout.BeginArea(new Rect(Screen.width - width - 10f, 10f, width, 220f));

            CropDefinition seed = _farmingController.SelectedSeed;
            GUILayout.Label($"Seed: {(seed != null ? seed.DisplayName : "—")}");

            foreach (KeyValuePair<string, int> kv in _harvestCounts)
                GUILayout.Label($"  {kv.Key}: {kv.Value}");

            GUILayout.EndArea();
        }
    }
}
