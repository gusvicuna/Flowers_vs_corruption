using FlowersVsCorruption.Core;
using UnityEngine;

namespace FlowersVsCorruption.World
{
    public class CorruptionController : MonoBehaviour
    {
        [SerializeField] private WorldView _worldView;
        [SerializeField] private TimeSystem _timeSystem;
        [SerializeField] private GameConfig _gameConfig;

        private CorruptionSystem _corruptionSystem;
        public CorruptionSystem CorruptionSystem => _corruptionSystem;

        private void Start()
        {
            _corruptionSystem = new CorruptionSystem(_worldView.Grid);
            _timeSystem.NightStarted += OnNightStarted;
            _corruptionSystem.HouseCorrupted += OnHouseCorrupted;
        }

        private void OnDestroy()
        {
            if (_timeSystem != null)
                _timeSystem.NightStarted -= OnNightStarted;
        }

        private void OnNightStarted(int day)
        {
            _corruptionSystem.Spread(_gameConfig.CorruptionSpreadPerNight);
        }

        private void OnHouseCorrupted(int index)
        {
            Debug.Log($"House corrupted at index {index}!");
            // Aquí puedes agregar lógica adicional, como mostrar un mensaje al jugador o activar efectos visuales.
        }

    }
}
