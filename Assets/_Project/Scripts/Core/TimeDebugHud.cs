using UnityEngine;

namespace FlowersVsCorruption.Core
{
    // TEMP (Feature 3 debug): OnGUI readout of the clock; removed when the
    // real UI arrives.
    public class TimeDebugHud : MonoBehaviour
    {
        [SerializeField] private TimeSystem _timeSystem;

        private void OnGUI()
        {
            if (_timeSystem == null) return;

            GUILayout.Label($"Phase: {_timeSystem.CurrentPhase}");
            GUILayout.Label($"Day: {_timeSystem.DayNumber}");
            GUILayout.Label($"Progress: {_timeSystem.PhaseProgress01:P1}");
            GUILayout.Label($"Time Remaining: {_timeSystem.PhaseTimeRemaining:F2}s");
        }
    }
}
